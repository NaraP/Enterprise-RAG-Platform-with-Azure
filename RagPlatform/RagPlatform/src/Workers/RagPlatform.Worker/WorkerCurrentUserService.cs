using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Worker;

/// <summary>
/// The Worker has no HTTP request/ClaimsPrincipal, so it derives "current user" context
/// from the Service Bus message being processed instead. Set per-message via BeginScope.
/// </summary>
public class WorkerCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    public string? IpAddress => null;
}
