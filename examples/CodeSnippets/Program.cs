using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Moderation;
using OpenAI_API.Polly;

static async Task Main(string[] args)
{
    using var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddSingleton<IOpenAIAPI>(_ => new OpenAIAPI("Your OpenAI API-Key"));
        })
        .Build();

    var api = host.Services.GetRequiredService<IOpenAIAPI>();

    // -----------------------------------------------------------------------------------------------------------------------


    // Call the Chat API to do a moderation on a text
    ModerationResult moderation = await api.Moderation.CallModerationAsync("My question");
    if (moderation.Results.Any(r => r.Flagged))
    {
        Console.WriteLine("Sorry, that question is not allowed.");
    }


    // -----------------------------------------------------------------------------------------------------------------------


    // Call the Chat API to create an conversation
    Conversation chat = api.Chat.CreateConversation();

    // Add user input
    chat.AppendUserInput("Describe ChatGPT in 10 words");

    // Get the response
    string response = await chat.GetResponseFromChatbotAsync();


    // -----------------------------------------------------------------------------------------------------------------------


    // Call the API to embed text using the default embedding model
    float[] embeddings1 = await api.Embeddings.GetEmbeddingsAsync("Hello World");
    

    // Call the API to embed text using the default embedding model using "Retry" to handle rate limiting
    float[] embeddings2 = await api.Embeddings.WithRetry(embeddings => embeddings.GetEmbeddingsAsync("Hello World"));
}