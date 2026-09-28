using MediatR;
using Microsoft.Extensions.Logging;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Application.Common.Exceptions;
using RagPlatform.Domain.Enums;
using RagPlatform.Domain.Entities;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Application.Documents.Commands.ProcessDocument;

public class ProcessDocumentCommandHandler : IRequestHandler<ProcessDocumentCommand, Unit>
{
    private const int MaxAttempts = 5;

    private readonly IDocumentRepository _documents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorage;
    private readonly ITextExtractorFactory _extractorFactory;
    private readonly ITextChunkingService _chunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ISearchIndexService _searchIndex;
    private readonly ILogger<ProcessDocumentCommandHandler> _logger;

    public ProcessDocumentCommandHandler(
        IDocumentRepository documents,
        IUnitOfWork unitOfWork,
        IBlobStorageService blobStorage,
        ITextExtractorFactory extractorFactory,
        ITextChunkingService chunkingService,
        IEmbeddingService embeddingService,
        ISearchIndexService searchIndex,
        ILogger<ProcessDocumentCommandHandler> logger)
    {
        _documents = documents;
        _unitOfWork = unitOfWork;
        _blobStorage = blobStorage;
        _extractorFactory = extractorFactory;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _searchIndex = searchIndex;
        _logger = logger;
    }

    public async Task<Unit> Handle(ProcessDocumentCommand request, CancellationToken ct)
    {
        var document = await _documents.GetByIdAsync(request.DocumentId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Document), request.DocumentId);

        try
        {
            // 03 - Extract
            document.TransitionTo(ProcessingStatus.Extracting);
            await _unitOfWork.SaveChangesAsync(ct);

            await using var stream = await _blobStorage.DownloadAsync(document.ContainerName, document.StoragePath, ct);
            var extractor = _extractorFactory.GetExtractor(document.DocumentType);
            var extracted = await extractor.ExtractAsync(stream, ct);

            // 04 - Chunk
            document.TransitionTo(ProcessingStatus.Chunking);
            await _unitOfWork.SaveChangesAsync(ct);

            var textChunks = _chunkingService.Chunk(extracted.FullText);
            var domainChunks = textChunks
                .Select(c => new DocumentChunk(document.Id, c.SequenceNumber, c.Content, c.SectionHeading))
                .ToList();
            await _documents.AddChunksAsync(domainChunks, ct);

            // Embeddings
            document.TransitionTo(ProcessingStatus.GeneratingEmbeddings);
            await _unitOfWork.SaveChangesAsync(ct);

            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(
                domainChunks.Select(c => c.Content).ToList(), ct);

            // 05 - Index
            document.TransitionTo(ProcessingStatus.Indexing);
            await _unitOfWork.SaveChangesAsync(ct);

            var indexName = $"tenant-{document.TenantId:N}";
            await _searchIndex.EnsureIndexExistsAsync(indexName, ct);

            var searchDocs = domainChunks.Zip(embeddings, (chunk, vector) => new SearchChunkDocument(
                chunk.SearchDocumentKey,
                document.Id,
                document.FileName,
                chunk.SequenceNumber,
                chunk.Content,
                vector,
                document.OwnerUserId,
                document.Permissions.Where(p => p.Role is not null).Select(p => p.Role!).ToList(),
                document.Metadata));

            await _searchIndex.IndexChunksAsync(indexName, searchDocs, ct);
            foreach (var chunk in domainChunks) chunk.MarkIndexed();

            document.CompleteIndexing(indexName, domainChunks.Count);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Document {DocumentId} indexed with {Count} chunks", document.Id, domainChunks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed for document {DocumentId} (attempt {Attempt})", document.Id, request.AttemptNumber);
            document.MarkFailed(ex.Message);

            if (request.AttemptNumber >= MaxAttempts)
                document.MarkDeadLettered($"Exceeded {MaxAttempts} attempts: {ex.Message}");
            else
                document.ScheduleRetry();

            await _unitOfWork.SaveChangesAsync(ct);
            throw; // let the Worker's Service Bus retry/dead-letter policy take over
        }

        return Unit.Value;
    }
}
