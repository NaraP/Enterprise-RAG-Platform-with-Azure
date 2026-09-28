namespace RagPlatform.Application.Common.Interfaces;

public interface ISearchIndexService
{
    Task EnsureIndexExistsAsync(string indexName, CancellationToken ct = default);

    Task IndexChunksAsync(string indexName, IEnumerable<SearchChunkDocument> chunks, CancellationToken ct = default);

    Task DeleteDocumentChunksAsync(string indexName, Guid documentId, CancellationToken ct = default);

    Task<IReadOnlyList<SearchResult>> HybridSearchAsync(
        string indexName,
        string queryText,
        float[] queryVector,
        Guid userId,
        IEnumerable<string> userRoles,
        int top = 10,
        CancellationToken ct = default);
}

public record SearchChunkDocument(
    string Key,
    Guid DocumentId,
    string FileName,
    int SequenceNumber,
    string Content,
    float[] Embedding,
    Guid OwnerUserId,
    IReadOnlyList<string> AllowedRoles,
    Dictionary<string, string> Metadata);

public record SearchResult(
    Guid DocumentId,
    string FileName,
    int SequenceNumber,
    string Content,
    double Score,
    Dictionary<string, string> Metadata);
