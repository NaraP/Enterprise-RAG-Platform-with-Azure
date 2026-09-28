using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Infrastructure.Identity;

/// <summary>
/// Reads the caller's identity from the Entra ID-issued claims (populated by
/// Microsoft.Identity.Web in the MVC app, or from message metadata in the Worker).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.TryParse(User?.FindFirstValue("oid") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new InvalidOperationException("No authenticated user in the current context.");

    public Guid TenantId => Guid.TryParse(User?.FindFirstValue("tid"), out var tid) ? tid : Guid.Empty;

    public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
