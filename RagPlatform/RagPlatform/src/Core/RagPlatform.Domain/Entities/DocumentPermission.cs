using RagPlatform.Domain.Common;
using RagPlatform.Domain.Enums;

namespace RagPlatform.Domain.Entities;

/// <summary>Grants a user (or role) an access level on a specific document (RBAC + document-level security).</summary>
public class DocumentPermission : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Role { get; private set; }
    public AccessLevel AccessLevel { get; private set; }

    private DocumentPermission() { }

    public static DocumentPermission ForUser(Guid documentId, Guid userId, AccessLevel level) =>
        new() { DocumentId = documentId, UserId = userId, AccessLevel = level };

    public static DocumentPermission ForRole(Guid documentId, string role, AccessLevel level) =>
        new() { DocumentId = documentId, Role = role, AccessLevel = level };
}
