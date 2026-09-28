namespace RagPlatform.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    IReadOnlyList<string> Roles { get; }
    string? IpAddress { get; }
}
