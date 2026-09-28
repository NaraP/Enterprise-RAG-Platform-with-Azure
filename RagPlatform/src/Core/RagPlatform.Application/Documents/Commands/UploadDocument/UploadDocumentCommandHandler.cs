using MediatR;
using Microsoft.Extensions.Logging;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Application.Common.Models;
using RagPlatform.Domain.Entities;
using RagPlatform.Domain.Enums;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IDocumentRepository _documents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobStorage;
    private readonly IServiceBusPublisher _serviceBus;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogRepository _auditLog;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    private const string ContainerName = "documents";
    private const string IngestionQueue = "document-ingestion";

    public UploadDocumentCommandHandler(
        IDocumentRepository documents,
        IUnitOfWork unitOfWork,
        IBlobStorageService blobStorage,
        IServiceBusPublisher serviceBus,
        ICurrentUserService currentUser,
        IAuditLogRepository auditLog,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _documents = documents;
        _unitOfWork = unitOfWork;
        _blobStorage = blobStorage;
        _serviceBus = serviceBus;
        _currentUser = currentUser;
        _auditLog = auditLog;
        _logger = logger;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        var contentHash = await _blobStorage.ComputeSha256Async(request.FileContent, ct);

        var existing = await _documents.GetByContentHashAsync(contentHash, _currentUser.TenantId, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Duplicate upload detected for hash {Hash}; returning existing document {Id}", contentHash, existing.Id);
            return existing.Id;
        }

        var documentType = MapDocumentType(Path.GetExtension(request.FileName));
        var documentId = Guid.NewGuid();

        // structured folder hierarchy: tenant/user/yyyy/MM/documentId-filename
        var blobPath = $"{_currentUser.TenantId:N}/{_currentUser.UserId:N}/{DateTime.UtcNow:yyyy/MM}/{documentId:N}-{request.FileName}";

        request.FileContent.Position = 0;
        await _blobStorage.UploadAsync(request.FileContent, ContainerName, blobPath, request.ContentType, ct);

        var document = new Document(
            request.FileName,
            blobPath,
            ContainerName,
            documentType,
            request.SizeInBytes,
            contentHash,
            _currentUser.TenantId,
            _currentUser.UserId,
            request.Metadata);

        await _documents.AddAsync(document, ct);
        await _auditLog.AddAsync(new AuditLogEntry(_currentUser.UserId, "DocumentUploaded", document.Id, request.FileName, _currentUser.IpAddress), ct);
        await _unitOfWork.SaveChangesAsync(ct);

        document.TransitionTo(ProcessingStatus.QueuedForProcessing, "Published to ingestion queue");
        await _unitOfWork.SaveChangesAsync(ct);

        var message = new DocumentIngestionMessageContract(
            document.Id,
            document.TenantId,
            document.OwnerUserId,
            document.StoragePath,
            document.ContainerName,
            document.DocumentType,
            document.Metadata,
            AttemptNumber: 1);

        await _serviceBus.PublishAsync(IngestionQueue, message, sessionId: document.Id.ToString(), correlationId: document.Id.ToString(), ct: ct);

        return document.Id;
    }

    private static DocumentType MapDocumentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => DocumentType.Pdf,
        ".doc" or ".docx" => DocumentType.Word,
        ".xls" or ".xlsx" => DocumentType.Excel,
        ".ppt" or ".pptx" => DocumentType.PowerPoint,
        ".md" => DocumentType.Markdown,
        ".txt" => DocumentType.PlainText,
        _ => DocumentType.Other
    };
}
