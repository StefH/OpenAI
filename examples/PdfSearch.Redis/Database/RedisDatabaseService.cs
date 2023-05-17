using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using PdfSearch.Redis.Database.Models;
using StackExchange.Redis;

namespace PdfSearch.Redis.Database;

internal class RedisDatabaseService : IRedisDatabaseService
{
    private readonly ILogger<RedisDatabaseService> _logger;
    private readonly Lazy<IDatabase> _db;

    public RedisDatabaseService(ILogger<RedisDatabaseService> logger, IConnectionMultiplexer connection)
    {
        _logger = logger;
        _db = new Lazy<IDatabase>(connection.GetDatabase());
    }

    public bool DoesDataExists(string prefix)
    {
        return _db.Value.HashExists($"{prefix}:0", new RedisValue("idx"));
    }

    public async Task InsertAsync(
        string indexName,
        string prefix,
        IReadOnlyList<string> textFragments,
        Func<string, Task<float[]>> embeddingFunc,
        Func<string, Task<IReadOnlyList<int>>> tokenFunc
    )
    {
        foreach (var items in textFragments.Select((textFragment, idx) => new { idx, textFragment }))
        {
            _logger.LogInformation("Inserting {idx}/{count}", items.idx, textFragments.Count);

            var embeddingTask = embeddingFunc(items.textFragment);
            var tokenTask = tokenFunc(items.textFragment);

            await Task.WhenAll(embeddingTask, tokenTask);

            var embeddings = await embeddingTask;
            var tokens = await tokenTask;

            var byteArray = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

            _db.Value.HashSet($"{prefix}:{items.idx}", new HashEntry[]
            {
                new(new RedisValue("idx"), items.idx),
                new(new RedisValue("text"), items.textFragment),
                new(new RedisValue("tokens"), tokens.Count),
                new(new RedisValue("embedding"), byteArray)
            });
        };

        await CreateRedisIndexAsync(indexName, prefix);
    }

    public async Task<IReadOnlyList<VectorDocument>> SearchAsync(string indexName, byte[] vectorAsBytes)
    {
        _logger.LogInformation("Doing a search for index {index}", indexName);

        var query = new Query("*=>[KNN 5 @embedding $vectorAsBytes AS vector_score]")
            .AddParam("vectorAsBytes", vectorAsBytes)
            .ReturnFields("idx", "embedding", "text", "vector_score")
            .SetSortBy("vector_score")
            .Dialect(2);

        var searchResult = await _db.Value.FT().SearchAsync(indexName, query);

        var sortedDocuments = searchResult.Documents
            .Select(document => new VectorDocument
            (
                Idx: (int)document["idx"],
                Text: document["text"]!,
                TokenLength: (int)document["tokens"],
                Score: (float)document["vector_score"]
            ))
            .OrderByDescending(document => document.Score)
            .ToArray();

        return sortedDocuments;
    }

    private async Task CreateRedisIndexAsync(string indexName, string prefix)
    {
        _logger.LogInformation("Creating index {indexName} for prefix {prefix}", indexName, prefix);

        var schema = new Schema()
            .AddNumericField("idx")
            .AddVectorField("embedding", Schema.VectorField.VectorAlgo.HNSW,
                new Dictionary<string, object>
                {
                    { "TYPE", "FLOAT32" },
                    { "DIM", "1536" },
                    { "DISTANCE_METRIC", "COSINE" }
                }
            );

        try
        {
            await _db.Value.FT().DropIndexAsync(indexName);
        }
        catch
        {
            //
        }

        await _db.Value.FT().CreateAsync(indexName, new FTCreateParams().AddPrefix($"{prefix}:"), schema);
    }
}