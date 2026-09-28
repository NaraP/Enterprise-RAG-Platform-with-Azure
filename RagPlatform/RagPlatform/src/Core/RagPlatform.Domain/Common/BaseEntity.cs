namespace RagPlatform.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedAtUtc { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public string? ModifiedBy { get; protected set; }

    protected void MarkModified(string? modifiedBy)
    {
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        ModifiedBy = modifiedBy;
    }
}
