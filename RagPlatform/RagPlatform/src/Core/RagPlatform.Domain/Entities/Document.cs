using RagPlatform.Domain.Common;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Domain.Entities;

/// <summary>
/// Aggregate root for an uploaded document. Owns its processing status transitions and
/// the list of permissions/history entries that travel with it. Chunks are managed
/// separately (DocumentChunk) since they can number in the thousands per document.
/// </summary>
public class Document : BaseEntity
{
    public string FileName { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!; // Azure Storage blob path
    public string ContainerName { get; private set; } = default!;
    public DocumentType DocumentType { get; private set; }
    public long SizeInBytes { get; private set; }
    public string ContentHash { get; private set; } = default!; // SHA-256, used for de-dup

    public Guid TenantId { get; private set; }
    public Guid OwnerUserId { get; private set; }

    public ProcessingStatus Status { get; private set; } = ProcessingStatus.Uploaded;
    public int ProcessingAttempts { get; private set; }
    public string? LastError { get; private set; }

    public string? SearchIndexName { get; private set; }
    public int ChunkCount { get; private set; }

    public Dictionary<string, string> Metadata { get; private set; } = new();

    private readonly List<DocumentPermission> _permissions = new();
    public IReadOnlyCollection<DocumentPermission> Permissions => _permissions.AsReadOnly();

    private readonly List<ProcessingHistoryEntry> _history = new();
    public IReadOnlyCollection<ProcessingHistoryEntry> History => _history.AsReadOnly();

    private Document() { }

    public Document(
        string fileName,
        string storagePath,
        string containerName,
        DocumentType documentType,
        long sizeInBytes,
        string contentHash,
        Guid tenantId,
        Guid ownerUserId,
        Dictionary<string, string>? metadata = null)
    {
        FileName = fileName;
        StoragePath = storagePath;
        ContainerName = containerName;
        DocumentType = documentType;
        SizeInBytes = sizeInBytes;
        ContentHash = contentHash;
        TenantId = tenantId;
        OwnerUserId = ownerUserId;
        Metadata = metadata ?? new Dictionary<string, string>();

        _permissions.Add(DocumentPermission.ForUser(Id, ownerUserId, AccessLevel.Owner));
        _history.Add(new ProcessingHistoryEntry(Id, ProcessingStatus.Uploaded, "Document uploaded", 0));
    }

    public void TransitionTo(ProcessingStatus newStatus, string? message = null)
    {
        Status = newStatus;
        _history.Add(new ProcessingHistoryEntry(Id, newStatus, message, ProcessingAttempts));
        MarkModified(null);
    }

    public void MarkFailed(string error)
    {
        ProcessingAttempts++;
        LastError = error;
        Status = ProcessingStatus.Failed;
        _history.Add(new ProcessingHistoryEntry(Id, ProcessingStatus.Failed, error, ProcessingAttempts));
    }

    public void ScheduleRetry()
    {
        Status = ProcessingStatus.Retrying;
        _history.Add(new ProcessingHistoryEntry(Id, ProcessingStatus.Retrying, "Retry scheduled", ProcessingAttempts));
    }

    public void MarkDeadLettered(string reason)
    {
        Status = ProcessingStatus.DeadLettered;
        _history.Add(new ProcessingHistoryEntry(Id, ProcessingStatus.DeadLettered, reason, ProcessingAttempts));
    }

    public void CompleteIndexing(string searchIndexName, int chunkCount)
    {
        SearchIndexName = searchIndexName;
        ChunkCount = chunkCount;
        Status = ProcessingStatus.Completed;
        _history.Add(new ProcessingHistoryEntry(Id, ProcessingStatus.Completed, $"Indexed {chunkCount} chunks", ProcessingAttempts));
    }

    public void GrantAccess(DocumentPermission permission) => _permissions.Add(permission);

    public bool CanBeAccessedBy(Guid userId, IEnumerable<string> userRoles)
    {
        if (OwnerUserId == userId) return true;
        return _permissions.Any(p =>
            (p.UserId.HasValue && p.UserId.Value == userId) ||
            (p.Role is not null && userRoles.Contains(p.Role, StringComparer.OrdinalIgnoreCase)));
    }
}
