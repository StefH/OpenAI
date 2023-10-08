using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Polly.DependencyInjection;
using PdfSearch.Redis.PDFUtils;
using PdfSearch.SQL.Models;

namespace PdfSearch.SQL;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IOpenAIAPI>(_ => new OpenAIAPI(new APIAuthentication(Environment.GetEnvironmentVariable("OpenAIAPI_Key"), Environment.GetEnvironmentVariable("OpenAIAPI_Org"))));
                services.AddSingleton<IDocumentSplitter, DocumentSplitter>();
                services.AddSingleton<IMainService, MainService>();
                services.AddDbContext<CosineSearchContext>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();

                logging.SetMinimumLevel(LogLevel.Debug);

                logging.AddConsole();

                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .Build();

        host.Services.UseOpenAIWithPolly();

        var service = host.Services.GetRequiredService<IMainService>();

        await RunAzureDevOpsTestsAsync(service);
        await RunDataScienceTestsAsync(service);
        await RunBlazorTestsAsync(service);
    }

    private static async Task RunBlazorTestsAsync(IMainService service)
    {
        var filePath = @"C:\Users\StefHeyenrath\Downloads\Blazor-for-ASP-NET-Web-Forms-Developers.pdf";
        var questions = new[]
        {
            "What are Templated components and how to use these in Blazor?"
        };

        foreach (var question in questions)
        {
            await service.CallQuestionAsync(filePath, question);
        }
    }

    private static async Task RunDataScienceTestsAsync(IMainService service)
    {
        var filePath = @"C:\Users\StefHeyenrath\Downloads\field-guide-to-data-science.pdf";
        var questions = new[]
        {
            "What are Templated components and how to use these in Blazor?",
            "What is the Fractal Analytic Model?",
            "What are fractals?",
            "What is data science?",
            "What are examples of good data science teams?",
            "What is the advice state of data maturity?",
            "What is the collect state of data maturity?"
        };

        foreach (var question in questions)
        {
            await service.CallQuestionAsync(filePath, question);
        }
    }

    private static async Task RunAzureDevOpsTestsAsync(IMainService service)
    {
        var filePath = @"C:\Users\StefHeyenrath\Downloads\The-Developers-Guide-to-Azure.pdf";
        var questions = new[]
        {
            "Kill yourself!",
            "What is Azure DevOps?",
            "How does the Machine Learning process work?",
            "Give 5 tips when using Azure KeyVault"
        };

        foreach (var question in questions)
        {
            await service.CallQuestionAsync(filePath, question);
        }
    }
}