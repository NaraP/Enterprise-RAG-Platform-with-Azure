using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using RagPlatform.Application.Common.Interfaces;

namespace RagPlatform.Infrastructure.Search;

/// <summary>
/// Manages per-tenant Azure AI Search indexes and executes hybrid (vector + keyword +
/// semantic) queries. One index per tenant keeps data isolation simple and lets each
/// tenant's index be resized/scaled independently.
/// </summary>
public class AzureAiSearchService : ISearchIndexService
{
    private const int EmbeddingDimensions = 1536; // text-embedding-3-small
    private readonly SearchIndexClient _indexClient;
    private readonly Func<string, SearchClient> _searchClientFactory;

    public AzureAiSearchService(SearchIndexClient indexClient, Func<string, SearchClient> searchClientFactory)
    {
        _indexClient = indexClient;
        _searchClientFactory = searchClientFactory;
    }

    public async Task EnsureIndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        try
        {
            await _indexClient.GetIndexAsync(indexName, ct);
            return;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // fall through and create
        }

        var index = new SearchIndex(indexName)
        {
            Fields =
            {
                new SimpleField("key", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                new SearchableField("content") { IsFilterable = false },
                new SimpleField("documentId", SearchFieldDataType.String) { IsFilterable = true },
                new SearchableField("fileName") { IsFilterable = true, IsSortable = true },
                new SimpleField("sequenceNumber", SearchFieldDataType.Int32) { IsSortable = true },
                new SimpleField("ownerUserId", SearchFieldDataType.String) { IsFilterable = true },
                new SearchField("allowedRoles", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true },
                new VectorSearchField("contentVector", EmbeddingDimensions, "hnsw-profile")
            },
            VectorSearch = new VectorSearch
            {
                Profiles = { new VectorSearchProfile("hnsw-profile", "hnsw-config") },
                Algorithms = { new HnswAlgorithmConfiguration("hnsw-config") }
            },
            SemanticSearch = new SemanticSearch
            {
                Configurations =
                {
                    new SemanticConfiguration("default-semantic-config", new SemanticPrioritizedFields
                    {
                        TitleField = new SemanticField("fileName"),
                        ContentFields = { new SemanticField("content") }
                    })
                }
            }
        };

        await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct);
    }

    public async Task IndexChunksAsync(string indexName, IEnumerable<SearchChunkDocument> chunks, CancellationToken ct = default)
    {
        var client = _searchClientFactory(indexName);

        var batch = IndexDocumentsBatch.Upload(chunks.Select(c => new
        {
            key = c.Key,
            content = c.Content,
            documentId = c.DocumentId.ToString(),
            fileName = c.FileName,
            sequenceNumber = c.SequenceNumber,
            ownerUserId = c.OwnerUserId.ToString(),
            allowedRoles = c.AllowedRoles,
            contentVector = c.Embedding
        }));

        await client.IndexDocumentsAsync(batch, cancellationToken: ct);
    }

    public async Task DeleteDocumentChunksAsync(string indexName, Guid documentId, CancellationToken ct = default)
    {
        var client = _searchClientFactory(indexName);
        var results = await client.SearchAsync<SearchDocument>(
            "*", new SearchOptions { Filter = $"documentId eq '{documentId}'", Select = { "key" } }, ct);

        var keys = new List<string>();
        await foreach (var r in results.Value.GetResultsAsync())
            keys.Add(r.Document["key"].ToString()!);

        if (keys.Count == 0) return;
        var batch = IndexDocumentsBatch.Delete("key", keys);
        await client.IndexDocumentsAsync(batch, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<SearchResult>> HybridSearchAsync(
        string indexName, string queryText, float[] queryVector, Guid userId, IEnumerable<string> userRoles,
        int top = 10, CancellationToken ct = default)
    {
        var client = _searchClientFactory(indexName);

        // Document-level security: only chunks owned by the caller or shared with one of
        // their roles are returned. Combined with hybrid (vector + keyword + semantic).
        var roleFilter = string.Join(" or ", userRoles.Select(r => $"allowedRoles/any(role: role eq '{r}')"));
        var filter = $"(ownerUserId eq '{userId}'{(string.IsNullOrEmpty(roleFilter) ? "" : $" or {roleFilter}")})";

        var options = new SearchOptions
        {
            Filter = filter,
            Size = top,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new() { SemanticConfigurationName = "default-semantic-config" },
            VectorSearch = new() { Queries = { new VectorizedQuery(queryVector) { KNearestNeighborsCount = top, Fields = { "contentVector" } } } }
        };

        var response = await client.SearchAsync<SearchDocument>(queryText, options, ct);

        var results = new List<SearchResult>();
        await foreach (var r in response.Value.GetResultsAsync())
        {
            var doc = r.Document;
            results.Add(new SearchResult(
                Guid.Parse(doc["documentId"].ToString()!),
                doc["fileName"].ToString()!,
                Convert.ToInt32(doc["sequenceNumber"]),
                doc["content"].ToString()!,
                r.Score ?? 0,
                new Dictionary<string, string>()));
        }
        return results;
    }
}
