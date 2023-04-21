using System.Runtime.InteropServices;
using LangChain.Example.Redis.Models;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;

namespace LangChain.Example.Redis;

internal class RedisDatabaseService : IRedisDatabaseService
{
    private readonly Lazy<IDatabase> _db;

    public RedisDatabaseService(IConnectionMultiplexer connection)
    {
        _db = new Lazy<IDatabase>(connection.GetDatabase());
    }

    public bool DoesDataExists(string prefix)
    {
        return _db.Value.HashExists($"{prefix}:0", new RedisValue("idx"));
    }

    public async Task<IReadOnlyList<VectorDocument>> SearchAsync(string indexName, byte[] vector)
    {
        var query = new Query("*=>[KNN 5 @embedding $vector AS vector_score]")
            .AddParam("vector", vector)
            .ReturnFields("idx", "embedding", "text", "vector_score")
            .SetSortBy("vector_score")
            .Dialect(2);

        var searchResult = await _db.Value.FT().SearchAsync(indexName, query);

        var sortedDocuments = searchResult.Documents
            .OrderByDescending(d => d.GetProperties().First(p => p.Key == "vector_score").Value)
            .ToArray();

        return sortedDocuments
            .Select(document => new
            {
                Idx = (int)document["idx"],
                Text = (string?)document["text"],
                Score = (float)document["vector_score"]
            })
            .Where(document => !string.IsNullOrEmpty(document.Text))
            .Select(document => new VectorDocument(document.Idx, document.Text!, document.Score))
            .ToArray();

        //var properties = document.GetProperties().ToArray();
        //var idx = (int)properties.First(p => p.Key == "idx").Value;
        // var text = (string?)document["text"];
        //var score = (float) properties.First(p => p.Key == "vector_score").Value;
        //var embedding = (byte[]) properties.First(p => p.Key == "embedding").Value!;
        //var embeddingArray = MemoryMarshal.Cast<byte, float>(embedding).ToArray();
    }

    public async Task InsertAsync(string indexName, string prefix, IReadOnlyList<string> parts, Func<string, Task<float[]>> func)
    {
        foreach (var x in parts.Select((part, idx) => new { idx, part }))
        {
            Console.WriteLine("{0}/{1}", x.idx, parts.Count);

            var embeddings = await func(x.part);
            var byteArray = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

            _db.Value.HashSet($"{prefix}:{x.idx}", new HashEntry[]
            {
                new(new RedisValue("idx"), x.idx),
                new(new RedisValue("text"), x.part),
                new(new RedisValue("embedding"), byteArray)
            });
        }

        await CreateRedisIndexAsync(indexName, prefix);
    }

    private async Task CreateRedisIndexAsync(string indexName, string prefix)
    {
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