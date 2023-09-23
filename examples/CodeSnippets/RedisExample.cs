using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using OpenAI_API;
using OpenAI_API.Polly;
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

static async Task<byte[]> ConvertTheQuestionToAVector(IOpenAIAPI api)
{
    string question = "What is Azure DevOps?";
    float[] questionAsVector = await api.Embeddings.GetEmbeddingsAsync(question);
    byte[] questionAsBytes = MemoryMarshal.Cast<float, byte>(questionAsVector).ToArray();

    return questionAsBytes;
}

static async Task<byte[]> ConvertTheQuestionToAVectorAsync(IOpenAIAPI api, string question)
{
    float[] questionAsVector = await api.Embeddings.GetEmbeddingsAsync(question);
    byte[] questionAsBytes = MemoryMarshal.Cast<float, byte>(questionAsVector).ToArray();

    return questionAsBytes;
}

    static Query BuildQuery(byte[] questionAsBytes)
{
    var query = new Query("*=>[KNN 5 @embedding $vector AS vector_score]")
        .AddParam("vector", questionAsBytes)
        .ReturnFields("text", "tokens", "vector_score")
        .SetSortBy("vector_score")
        .Dialect(2);

    return query;
}

static async Task<IReadOnlyList<VectorDocument>> SearchAsync(IDatabase database, string indexName, Query query)
{
    SearchResult searchResult = await database.FT().SearchAsync(indexName, query);

    return searchResult.Documents
        .Select(document => new VectorDocument
        (
            Idx: (int)document["idx"],
            Text: document["text"]!,
            TokenLength: (int)document["tokens"],
            Score: (float)document["vector_score"]
        ))
        .OrderByDescending(document => document.Score)
        .ToArray();
}

static async Task SearchForCosineSimilarityAndGetResponseFromChatGPTAsync(IDatabase database, IOpenAIAPI api)
{
    string indexName = "field-guide-to-data-science-index";
    string question = "What are fractals?";
    byte[] questionAsVector = await ConvertTheQuestionToAVectorAsync(api, question);
    var query = BuildQuery(questionAsVector);
    var vectorDocuments = await SearchAsync(database, indexName, query); // 5 VectorDocuments are returned here

    // Just concatenate all text-fragments from the found documents.
    var textBuilder = new StringBuilder();
    foreach (var vectorDocument in vectorDocuments)
    {
        textBuilder.AppendLine(vectorDocument.Text);
    }

    // Here is where the 'magic' happens
    var contentBuilder = new StringBuilder();
    contentBuilder.AppendLine($"Based on the following source text \"{textBuilder.ToString()}\" follow the next requirements:");
    contentBuilder.AppendLine($"- Answer the question: \"{question}\"");
    contentBuilder.AppendLine(@"- Make sure to give a concrete answer");
    contentBuilder.AppendLine(@"- Do not start your answer with ""Based on the source text,""");
    contentBuilder.AppendLine(@"- Only base your answer on the source text");
    contentBuilder.AppendLine(@"- When you cannot give a good answer based on the source text, return ""I cannot find any relevant information.""");

    // Start a chat
    var chat = api.Chat.WithRetry(chatAPI => chatAPI.CreateConversation());

    // Add user input
    chat.AppendUserInput(contentBuilder.ToString());

    // Get the response for that question from ChatGPT.
    var response = await chat.WithRetry(conversation => conversation.GetResponseFromChatbotAsync());

}

}

public record VectorDocument(int Idx, string Text, int TokenLength, double Score);