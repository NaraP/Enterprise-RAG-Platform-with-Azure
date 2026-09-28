namespace RagPlatform.Domain.Enums;

/// <summary>Document-level security used to filter Azure AI Search results per user/role.</summary>
public enum AccessLevel
{
    Owner = 0,
    ReadWrite = 1,
    ReadOnly = 2
}
