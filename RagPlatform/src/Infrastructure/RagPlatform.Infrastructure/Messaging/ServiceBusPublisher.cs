using System.Text.Json;
using Azure.Messaging.ServiceBus;
using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Infrastructure.Messaging;

/// <summary>
/// Publishes to Azure Service Bus queues/topics. Sessions (sessionId) keep every message
/// for a given document processed in order by a single Worker instance; correlationId
/// flows through Application Insights for end-to-end tracing across ingestion -> status -> indexing.
/// </summary>
public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly Dictionary<string, ServiceBusSender> _senders = new();
    private readonly object _lock = new();

    public ServiceBusPublisher(ServiceBusClient client) => _client = client;

    public async Task PublishAsync<T>(string queueOrTopicName, T message, string? sessionId = null, string? correlationId = null, CancellationToken ct = default)
    {
        var sender = GetOrCreateSender(queueOrTopicName);

        var body = JsonSerializer.Serialize(message);
        var sbMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            CorrelationId = correlationId,
            SessionId = sessionId
        };

        await sender.SendMessageAsync(sbMessage, ct);
    }

    private ServiceBusSender GetOrCreateSender(string queueOrTopicName)
    {
        lock (_lock)
        {
            if (!_senders.TryGetValue(queueOrTopicName, out var sender))
            {
                sender = _client.CreateSender(queueOrTopicName);
                _senders[queueOrTopicName] = sender;
            }
            return sender;
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();
    }
}
