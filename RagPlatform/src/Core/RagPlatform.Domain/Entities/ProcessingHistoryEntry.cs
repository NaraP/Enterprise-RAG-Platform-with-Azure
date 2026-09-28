using RagPlatform.Domain.Common;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Domain.Entities;

/// <summary>Append-only trail of every status transition a document goes through (audit + troubleshooting).</summary>
public class ProcessingHistoryEntry : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public ProcessingStatus Status { get; private set; }
    public string? Message { get; private set; }
    public int AttemptNumber { get; private set; }

    private ProcessingHistoryEntry() { }

    public ProcessingHistoryEntry(Guid documentId, ProcessingStatus status, string? message, int attemptNumber)
    {
        DocumentId = documentId;
        Status = status;
        Message = message;
        AttemptNumber = attemptNumber;
    }
}
