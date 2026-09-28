using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Application.Common.Models;
using RagPlatform.Application.Documents.Commands.ProcessDocument;
using RagPlatform.Infrastructure.Messaging.Messages;

namespace RagPlatform.Worker;

/// <summary>
/// Steps 03-05 entry point: a long-running BackgroundService with a session-aware Service
/// Bus processor. Sessions guarantee messages for the same DocumentId are handled by one
/// worker in order; MaxConcurrentSessions controls how many *different* documents are
/// processed in parallel, giving horizontal scalability for large document volumes.
/// </summary>
public class DocumentProcessingService : BackgroundService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessingService> _logger;
    private ServiceBusSessionProcessor? _processor;

    public DocumentProcessingService(ServiceBusClient serviceBusClient, IServiceScopeFactory scopeFactory, ILogger<DocumentProcessingService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _serviceBusClient.CreateSessionProcessor(QueueNames.DocumentIngestion, new ServiceBusSessionProcessorOptions
        {
            MaxConcurrentSessions = 8,
            MaxConcurrentCallsPerSession = 1,
            AutoCompleteMessages = false,
            PrefetchCount = 4
        });

        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Document processing worker started, listening on {Queue}", QueueNames.DocumentIngestion);

        // keep running until host shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
    }

    private async Task HandleMessageAsync(ProcessSessionMessageEventArgs args)
    {
        var message = args.Message;
        DocumentIngestionMessageContract? payload;

        try
        {
            payload = JsonSerializer.Deserialize<DocumentIngestionMessageContract>(message.Body);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Malformed ingestion message {MessageId}; dead-lettering", message.MessageId);
            await args.DeadLetterMessageAsync(message, "MalformedPayload", ex.Message);
            return;
        }

        if (payload is null)
        {
            await args.DeadLetterMessageAsync(message, "EmptyPayload");
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        // Populate the per-message "current user" context so downstream handlers (which
        // depend on ICurrentUserService) attribute work to the original uploader/tenant.
        var currentUser = (WorkerCurrentUserService)scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.UserId = payload.OwnerUserId;
        currentUser.TenantId = payload.TenantId;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var attempt = message.DeliveryCount;

        try
        {
            await mediator.Send(new ProcessDocumentCommand(payload.DocumentId, attempt), args.CancellationToken);
            await args.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing document {DocumentId} (delivery attempt {Attempt})", payload.DocumentId, attempt);

            if (attempt >= 5)
                await args.DeadLetterMessageAsync(message, "MaxAttemptsExceeded", ex.Message);
            else
                await args.AbandonMessageAsync(message); // triggers redelivery per queue's retry policy
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processor error in {Source}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
