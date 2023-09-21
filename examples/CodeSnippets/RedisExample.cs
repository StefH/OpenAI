using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

}