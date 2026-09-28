namespace RagPlatform.Application.Common.Interfaces;

public interface IServiceBusPublisher
{
    /// <summary>Publishes a message to the given queue/topic with an optional session id (used to keep
    /// per-document messages ordered) and a correlation id for end-to-end tracing.</summary>
    Task PublishAsync<T>(string queueOrTopicName, T message, string? sessionId = null, string? correlationId = null, CancellationToken ct = default);
}
