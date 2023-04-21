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

    public async Task InsertAsync(
        string indexName, 
        string prefix, 
        IReadOnlyList<string> parts, 
        Func<string, Task<float[]>> embeddingFunc,
        Func<string, Task<IReadOnlyList<int>>> tokenFunc
    )
    {
        var items = parts.Select((part, idx) => new { idx, part }).ToArray();

        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

        await Parallel.ForEachAsync(items, options, async (x, _) =>
        {
            Console.WriteLine("{0}/{1}", x.idx, parts.Count);

            var embeddingTask = embeddingFunc(x.part);
            var tokenTask = tokenFunc(x.part);

            await Task.WhenAll(embeddingTask, tokenTask);

            var embeddings = await embeddingTask;
            var tokens = await tokenTask;
            var byteArray = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

            _db.Value.HashSet($"{prefix}:{x.idx}", new HashEntry[]
            {
                new(new RedisValue("idx"), x.idx),
                new(new RedisValue("text"), x.part),
                new(new RedisValue("tokens"), tokens.Count),
                new(new RedisValue("embedding"), byteArray)
            });
        });

        //foreach (var x in parts.Select((part, idx) => new { idx, part }))
        //{
        //    Console.WriteLine("{0}/{1}", x.idx, parts.Count);

        //    var embeddingTask = embeddingFunc(x.part);
        //    var tokenTask = Task.Run(() => _encoding.Encode(x.part));

        //    await Task.WhenAll(embeddingTask, tokenTask);

        //    var embeddings = await embeddingTask;
        //    var tokens = await tokenTask;
        //    var byteArray = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

        //    _db.Value.HashSet($"{prefix}:{x.idx}", new HashEntry[]
        //    {
        //        new(new RedisValue("idx"), x.idx),
        //        new(new RedisValue("text"), x.part),
        //        new(new RedisValue("tokens"), tokens.Count),
        //        new(new RedisValue("embedding"), byteArray)
        //    });
        //}

        await CreateRedisIndexAsync(indexName, prefix);
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
                Tokens = (int)document["tokens"],
                Score = (float)document["vector_score"]
            })
            .Where(document => !string.IsNullOrEmpty(document.Text))
            .Select(document => new VectorDocument(document.Idx, document.Text!, document.Tokens, document.Score))
            .ToArray();

        //var properties = document.GetProperties().ToArray();
        //var idx = (int)properties.First(p => p.Key == "idx").Value;
        // var text = (string?)document["text"];
        //var score = (float) properties.First(p => p.Key == "vector_score").Value;
        //var embedding = (byte[]) properties.First(p => p.Key == "embedding").Value!;
        //var embeddingArray = MemoryMarshal.Cast<byte, float>(embedding).ToArray();
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