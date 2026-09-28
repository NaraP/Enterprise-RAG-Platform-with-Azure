using Azure.AI.OpenAI;
using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Infrastructure.Embeddings;

/// <summary>Generates vector embeddings via Azure OpenAI (deployment name configured in appsettings).</summary>
public class AzureOpenAiEmbeddingService : IEmbeddingService
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;

    public AzureOpenAiEmbeddingService(AzureOpenAIClient client, string deploymentName)
    {
        _client = client;
        _deploymentName = deploymentName;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var results = await GenerateEmbeddingsAsync(new[] { text }, ct);
        return results[0];
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        var embeddingClient = _client.GetEmbeddingClient(_deploymentName);

        // Azure OpenAI embedding batch limits apply (typically 16-2048 inputs per call
        // depending on model/tier) - batch defensively for very large chunk sets.
        const int batchSize = 16;
        var allEmbeddings = new List<float[]>();

        foreach (var batch in texts.Chunk(batchSize))
        {
            var response = await embeddingClient.GenerateEmbeddingsAsync(batch, cancellationToken: ct);
            allEmbeddings.AddRange(response.Value.Select(e => e.ToFloats().ToArray()));
        }

        return allEmbeddings;
    }
}
