using RagPlatform.Domain.Common;

namespace RagPlatform.Domain.Entities;

/// <summary>Compliance/audit record for document access and processing actions.</summary>
public class AuditLogEntry : BaseEntity
{
    public Guid? DocumentId { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = default!; // e.g. "DocumentUploaded", "SearchExecuted", "DocumentViewed"
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }

    private AuditLogEntry() { }

    public AuditLogEntry(Guid userId, string action, Guid? documentId = null, string? details = null, string? ipAddress = null)
    {
        UserId = userId;
        Action = action;
        DocumentId = documentId;
        Details = details;
        IpAddress = ipAddress;
    }
}
