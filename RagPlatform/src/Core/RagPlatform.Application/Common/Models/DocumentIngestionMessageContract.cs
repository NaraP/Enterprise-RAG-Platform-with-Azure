using RagPlatform.Domain.Enums;

namespace RagPlatform.Application.Common.Models;

/// <summary>
/// Envelope published to the "document-ingestion" queue after a successful upload.
/// Carries everything the Worker needs without a round-trip back to the Web app.
/// </summary>
public record DocumentIngestionMessageContract(
    Guid DocumentId,
    Guid TenantId,
    Guid OwnerUserId,
    string StoragePath,
    string ContainerName,
    DocumentType DocumentType,
    Dictionary<string, string> Metadata,
    int AttemptNumber);
