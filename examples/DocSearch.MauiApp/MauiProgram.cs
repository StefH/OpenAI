using CommunityToolkit.Maui;
using LangChain.Example;
using LangChain.Example.PDFUtils;
using LangChain.Example.Redis;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using StackExchange.Redis;

namespace DocSearch.MauiApp;

public static class MauiProgram
{
    public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()

            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
            .UseMauiCommunityToolkit()

            // After initializing the .NET MAUI Community Toolkit, optionally add additional fonts
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IOpenAIAPI>(_ => new OpenAIAPI(new APIAuthentication(Environment.GetEnvironmentVariable("OpenAIAPI_Key"), Environment.GetEnvironmentVariable("OpenAIAPI_Org"))));
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { "localhost:6380" } }));
        builder.Services.AddSingleton<IRedisDatabaseService, RedisDatabaseService>();
        builder.Services.AddSingleton<IDocumentSplitter, DocumentSplitter>();
        builder.Services.AddSingleton<IMainService, MainService>();

        // services
        // builder.Services.AddSingleton<IExampleApiFactory, ExampleApiFactory>();

        // pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<App>();

        return builder.Build();
    }
}