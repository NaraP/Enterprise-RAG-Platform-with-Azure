namespace RagPlatform.Infrastructure.Messaging.Messages;

/// <summary>
/// Central registry of the Service Bus topology described in the requirements:
/// ingestion, preprocessing, indexing, status updates, error handling, retry.
/// Provision these (with a shared "document-processing" topic and per-stage
/// subscriptions, or standalone queues) via Bicep/Terraform in infra/.
/// </summary>
public static class QueueNames
{
    public const string DocumentIngestion = "document-ingestion";
    public const string DocumentPreprocessing = "document-preprocessing";
    public const string DocumentIndexing = "document-indexing";
    public const string DocumentStatusUpdates = "document-status-updates";
    public const string DocumentErrors = "document-errors";
    public const string DocumentRetry = "document-retry";
}
