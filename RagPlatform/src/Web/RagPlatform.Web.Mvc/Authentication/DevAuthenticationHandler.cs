using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RagPlatform.Web.Mvc.Authentication;

/// <summary>
/// LOCAL DEV ONLY. Auto-authenticates every request as a fake user, so the app can run
/// end-to-end (upload/status/search, RBAC policies) without a real Entra ID tenant.
/// Enabled by "Authentication:UseLocalDevAuth": true in appsettings — set that to false
/// and fill in real AzureAd:* values to switch to real Entra ID sign-in.
///
/// NEVER enable this in a deployed/production environment: it grants every request the
/// Admin role with no credential check whatsoever.
/// </summary>
public class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public DevAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // A fixed fake tenant/user id, distinct from the AzureAd:TenantId placeholder so
        // it's obviously a dev identity if it ever shows up in logs or audit records.
        var tenantId = "99999999-0000-0000-0000-000000000001";
        var userId = "99999999-0000-0000-0000-000000000002";

        var claims = new List<Claim>
        {
            new("oid", userId),
            new("tid", tenantId),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "Dev User"),
            new("name", "Dev User"),
            new("preferred_username", "dev.user@localhost"),
            // Granted every RBAC role so all policies/controllers are reachable locally.
            new(ClaimTypes.Role, RagPlatform.Infrastructure.Security.RoleNames.Admin),
            new(ClaimTypes.Role, RagPlatform.Infrastructure.Security.RoleNames.DocumentContributor),
            new(ClaimTypes.Role, RagPlatform.Infrastructure.Security.RoleNames.DocumentReader)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
