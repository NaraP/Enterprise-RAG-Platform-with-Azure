using MediatR;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Application.Documents.DTOs;

namespace RagPlatform.Application.Documents.Queries.SearchDocuments;

public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, IReadOnlyList<SearchResultDto>>
{
    private readonly ISearchIndexService _searchIndex;
    private readonly IEmbeddingService _embeddingService;
    private readonly ICurrentUserService _currentUser;

    public SearchDocumentsQueryHandler(
        ISearchIndexService searchIndex,
        IEmbeddingService embeddingService,
        ICurrentUserService currentUser)
    {
        _searchIndex = searchIndex;
        _embeddingService = embeddingService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SearchResultDto>> Handle(SearchDocumentsQuery request, CancellationToken ct)
    {
        var indexName = $"tenant-{_currentUser.TenantId:N}";
        var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.QueryText, ct);

        var results = await _searchIndex.HybridSearchAsync(
            indexName,
            request.QueryText,
            queryVector,
            _currentUser.UserId,
            _currentUser.Roles,
            request.Top,
            ct);

        return results
            .Select(r => new SearchResultDto(
                r.DocumentId,
                r.FileName,
                r.SequenceNumber,
                Snippet(r.Content),
                r.Score))
            .ToList();
    }

    private static string Snippet(string content, int maxLength = 280) =>
        content.Length <= maxLength ? content : content[..maxLength] + "…";
}
