using RagPlatform.Domain.Entities;

namespace RagPlatform.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Document?> GetByContentHashAsync(string contentHash, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByOwnerAsync(Guid ownerUserId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Document document, CancellationToken ct = default);
    void Update(Document document);
    Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(Guid documentId, CancellationToken ct = default);
    Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken ct = default);
}
