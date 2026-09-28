using Microsoft.EntityFrameworkCore;
using RagPlatform.Domain.Entities;
using RagPlatform.Domain.Interfaces;

namespace RagPlatform.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _db;

    public AuditLogRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(AuditLogEntry entry, CancellationToken ct = default) =>
        await _db.AuditLog.AddAsync(entry, ct);

    public async Task<IReadOnlyList<AuditLogEntry>> GetForDocumentAsync(Guid documentId, CancellationToken ct = default) =>
        await _db.AuditLog.Where(a => a.DocumentId == documentId).OrderBy(a => a.CreatedAtUtc).ToListAsync(ct);
}
