using RagPlatform.Domain.Entities;

namespace RagPlatform.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLogEntry>> GetForDocumentAsync(Guid documentId, CancellationToken ct = default);
}
