using Microsoft.EntityFrameworkCore;
using RagPlatform.Domain.Entities;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _db;

    public DocumentRepository(ApplicationDbContext db) => _db = db;

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Documents.Include(d => d.Permissions).Include(d => d.History)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<Document?> GetByContentHashAsync(string contentHash, Guid tenantId, CancellationToken ct = default) =>
        _db.Documents.FirstOrDefaultAsync(d => d.ContentHash == contentHash && d.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<Document>> GetByOwnerAsync(Guid ownerUserId, int page, int pageSize, CancellationToken ct = default) =>
        await _db.Documents
            .Where(d => d.OwnerUserId == ownerUserId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task AddAsync(Document document, CancellationToken ct = default) =>
        await _db.Documents.AddAsync(document, ct);

    public void Update(Document document) => _db.Documents.Update(document);

    public async Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(Guid documentId, CancellationToken ct = default) =>
        await _db.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.SequenceNumber)
            .ToListAsync(ct);

    public async Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken ct = default) =>
        await _db.DocumentChunks.AddRangeAsync(chunks, ct);
}
