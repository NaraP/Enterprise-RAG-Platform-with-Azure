using RagPlatform.Domain.Common;

namespace RagPlatform.Domain.Entities;

/// <summary>
/// Local shadow of the Entra ID identity, used to associate documents/roles/audit
/// records without re-querying Graph on every request.
/// </summary>
public class ApplicationUser : BaseEntity
{
    public string ExternalObjectId { get; private set; } = default!; // Entra ID "oid" claim
    public string DisplayName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Guid TenantId { get; private set; }
    public List<string> Roles { get; private set; } = new();

    private ApplicationUser() { }

    public ApplicationUser(string externalObjectId, string displayName, string email, Guid tenantId)
    {
        ExternalObjectId = externalObjectId;
        DisplayName = displayName;
        Email = email;
        TenantId = tenantId;
    }

    public void AssignRole(string role)
    {
        if (!Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            Roles.Add(role);
    }
}
