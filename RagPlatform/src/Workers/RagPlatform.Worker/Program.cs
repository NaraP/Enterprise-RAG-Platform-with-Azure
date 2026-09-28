using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RagPlatform.Application;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Infrastructure;
using RagPlatform.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationInsightsTelemetryWorkerService();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Override the web-request-scoped ICurrentUserService registered by AddInfrastructure with
// the message-scoped implementation the Worker needs (see DocumentProcessingService).
builder.Services.AddScoped<ICurrentUserService, WorkerCurrentUserService>();

builder.Services.AddHostedService<DocumentProcessingService>();

var host = builder.Build();
host.Run();
