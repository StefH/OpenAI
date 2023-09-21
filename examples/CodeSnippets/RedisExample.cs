using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using OpenAI_API;
using StackExchange.Redis;

namespace CodeSnippets;

internal class RedisExample
{

static async Task Main(string[] args)
{
    using var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = new ConfigurationOptions { EndPoints = { "localhost:6380" } };
                return ConnectionMultiplexer.Connect(options);
            });
        })
        .Build();

    // Get IConnectionMultiplexer
    var connection = host.Services.GetRequiredService<IConnectionMultiplexer>();

    // Get an interactive connection to a database inside Redis.
    var database = connection.GetDatabase();

    // The prefix used to store the hashes
    string prefix = "The-Developers-Guide-to-Azure";

    // The index from the text-fragment
    int idx = 214;

    // The text-fragment
    string text = "Azure Defender enables extended ...";

    // The number of tokens for the text-fragment (calculated using SharpToken)
    int tokens = 175;

    // The OpenAI Embeddings API is used to generate the vectors
    float[] embeddings = new float[] { 1.0f };

    // Create a byte array from embeddings
    byte[] byteArray = MemoryMarshal.Cast<float, byte>(embeddings).ToArray();

    // Insert the Hash-entry with the index, text-fragment, number of tokens and the embedding
    database.HashSet($"{prefix}:{idx}", new HashEntry[]
    {
    new(new RedisValue("idx"), idx),
    new(new RedisValue("text"), text),
    new(new RedisValue("tokens"), tokens),
    new(new RedisValue("embedding"), byteArray)
    });

}


static async Task CreateIndex(IDatabase database)
{

string indexName = "The-Developers-Guide-to-Azure-index"; // The name of the index
string prefix = "The-Developers-Guide-to-Azure"; // The prefix used to store the hashes

var schema = new Schema()
    .AddNumericField("idx")
    .AddVectorField("embedding",
        Schema.VectorField.VectorAlgo.HNSW,
        new Dictionary<string, object>
        {
        { "TYPE", "FLOAT32" },
        { "DIM", "1536" },
        { "DISTANCE_METRIC", "COSINE" }
        }
    );

var createParams = new FTCreateParams()
    .AddPrefix($"{prefix}:");

await database.FT().CreateAsync(indexName, createParams, schema);

}

static async Task ConvertTheQuestionToAVector(IOpenAIAPI api)
{
    string question = "What is Azure DevOps?";
    float[] questionAsVector = await api.Embeddings.GetEmbeddingsAsync(question);
    byte[] questionAsBytes = MemoryMarshal.Cast<float, byte>(questionAsVector).ToArray();
}


static void BuildQuery(byte[] questionAsBytes)
{
    var query = new Query("*=>[KNN 5 @embedding $vector AS vector_score]")
        .AddParam("vector", questionAsBytes)
        .ReturnFields("text", "tokens", "vector_score")
        .SetSortBy("vector_score")
        .Dialect(2);
    }
}

