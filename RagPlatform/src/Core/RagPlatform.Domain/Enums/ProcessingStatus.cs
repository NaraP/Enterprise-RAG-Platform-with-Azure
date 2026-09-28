namespace RagPlatform.Domain.Enums;

public enum ProcessingStatus
{
    Uploaded = 0,
    QueuedForProcessing = 1,
    Extracting = 2,
    Chunking = 3,
    GeneratingEmbeddings = 4,
    Indexing = 5,
    Completed = 6,
    Failed = 7,
    DeadLettered = 8,
    Retrying = 9
}
