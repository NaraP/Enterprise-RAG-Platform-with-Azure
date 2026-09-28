using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RagPlatform.Application.Common.Interfaces;
using RagPlatform.Domain.Interfaces;
using RagPlatform.Infrastructure.Embeddings;
using RagPlatform.Infrastructure.Identity;
using RagPlatform.Infrastructure.Messaging;
using RagPlatform.Infrastructure.Persistence;
using RagPlatform.Infrastructure.Persistence.Repositories;
using RagPlatform.Infrastructure.Search;
using RagPlatform.Infrastructure.Storage;
using RagPlatform.Infrastructure.TextExtraction;

namespace RagPlatform.Infrastructure;

/// <summary>
/// Wires every Azure-backed implementation to its Application-layer interface.
/// Shared by both the MVC Web app (request-scoped, uses ICurrentUserService from HttpContext)
/// and the Worker (message-scoped; register a Worker-specific ICurrentUserService there instead).
/// All Azure clients authenticate via DefaultAzureCredential (managed identity in Azure,
/// Azure CLI / VS credential locally) - no secrets in config beyond resource URIs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var credential = new DefaultAzureCredential();

        // Persistence
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ApplicationDb")));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Azure Storage
        services.AddSingleton(new BlobServiceClient(new Uri(configuration["Azure:StorageAccountUri"]!), credential));
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        // Azure Service Bus
        services.AddSingleton(new ServiceBusClient(configuration["Azure:ServiceBusNamespace"], credential));
        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

        // Azure AI Search
        var searchEndpoint = new Uri(configuration["Azure:SearchEndpoint"]!);
        services.AddSingleton(new SearchIndexClient(searchEndpoint, credential));
        services.AddSingleton<Func<string, SearchClient>>(sp =>
            indexName => new SearchClient(searchEndpoint, indexName, credential));
        services.AddSingleton<ISearchIndexService, AzureAiSearchService>();

        // Azure OpenAI (embeddings)
        services.AddSingleton(new AzureOpenAIClient(new Uri(configuration["Azure:OpenAiEndpoint"]!), credential));
        services.AddSingleton<IEmbeddingService>(sp => new AzureOpenAiEmbeddingService(
            sp.GetRequiredService<AzureOpenAIClient>(),
            configuration["Azure:OpenAiEmbeddingDeployment"] ?? "text-embedding-3-small"));

        // Document processing pipeline building blocks
        services.AddScoped<IDocumentTextExtractor, PdfTextExtractor>();
        services.AddScoped<IDocumentTextExtractor, WordTextExtractor>();
        services.AddScoped<IDocumentTextExtractor, ExcelTextExtractor>();
        services.AddScoped<IDocumentTextExtractor, PowerPointTextExtractor>();
        services.AddScoped<IDocumentTextExtractor, PlainTextExtractor>();
        services.AddScoped<ITextExtractorFactory, TextExtractorFactory>();
        services.AddSingleton<ITextChunkingService, TextChunkingService>();

        // Current user (web request scope; Worker overrides this registration - see Worker/Program.cs)
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
