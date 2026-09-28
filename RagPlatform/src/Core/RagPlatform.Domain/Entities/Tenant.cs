using RagPlatform.Domain.Common;

namespace RagPlatform.Domain.Entities;

/// <summary>
/// Represents an isolation boundary (organization/business unit). Drives the
/// per-tenant Azure AI Search index naming strategy and RBAC scoping.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string SearchIndexName { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public Tenant(string name, string searchIndexName)
    {
        Name = name;
        SearchIndexName = searchIndexName;
    }

    public void Deactivate() => IsActive = false;
}
