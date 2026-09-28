namespace RagPlatform.Infrastructure.Security;

/// <summary>App roles, assigned via Entra ID app registration and surfaced as "roles" claims.</summary>
public static class RoleNames
{
    public const string Admin = "RagPlatform.Admin";
    public const string DocumentContributor = "RagPlatform.Contributor";
    public const string DocumentReader = "RagPlatform.Reader";
}
