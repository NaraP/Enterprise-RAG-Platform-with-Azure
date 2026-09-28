using Microsoft.AspNetCore.Authorization;

namespace RagPlatform.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string CanUploadDocuments = "CanUploadDocuments";
    public const string CanManageUsers = "CanManageUsers";

    public static void AddRagPlatformPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(CanUploadDocuments, p => p.RequireRole(RoleNames.Admin, RoleNames.DocumentContributor));
        options.AddPolicy(CanManageUsers, p => p.RequireRole(RoleNames.Admin));
    }
}
