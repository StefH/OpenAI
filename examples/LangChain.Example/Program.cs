using LangChain.Example.PDFUtils;
using LangChain.Example.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Polly.DependencyInjection;
using StackExchange.Redis;

namespace LangChain.Example;

internal static class Program
{
    static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IOpenAIAPI>(_ => new OpenAIAPI(new APIAuthentication(Environment.GetEnvironmentVariable("OpenAIAPI_Key"), Environment.GetEnvironmentVariable("OpenAIAPI_Org"))));
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { "localhost:6380" } }));
                services.AddSingleton<IRedisDatabaseService, RedisDatabaseService>();
                services.AddSingleton<IDocumentSplitter, DocumentSplitter>();
                services.AddSingleton<IMainService, MainService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        host.Services.UseOpenAIWithPolly();

        var service = host.Services.GetRequiredService<IMainService>();

        await RunAzureDevOpsTestsAsync(service);
        await RunDataScienceTestsAsync(service);
        await RunBlazorTestsAsync(service);
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
}

/*
Q: Kill yourself!
A: Sorry, that question is not allowed.

Q: What is Azure DevOps?
A: Azure DevOps is a platform offering from Microsoft that covers the software development lifecycle. It includes tools for tracking work, building and deploying code, testing, and managing artifacts. It is one of the integrated DevOps options alongside Visual Studio, Visual Studio Code, and GitHub, and allows for hybrid Azure DevOps and GitHub environments. It also provides features for implementing DevSecOps practices, such as GitHub Advanced Security, secret scanning, and understanding runtime behavior through Azure Monitor.

Q: How does the Machine Learning process work?
A: The Machine Learning process works as follows: Data containing patterns is collected and prepared for the ML algorithm. The ML algorithm is used to train a model to identify these patterns. The trained model is deployed so that it can be used to recognize patterns in new datasets. Applications use services or libraries to use the trained model and take actions based on the results. The crucial part of this process is that it is iterative. Thus, the ML model can be improved constantly by training it with new data and adjusting the algorithm to distinguish correct results from wrong ones.

Q: Give 5 tips when using Azure KeyVault
A: Five tips for using Azure KeyVault are:

1. Use KeyVault to securely store connection strings for web applications, instead of storing them in the configuration system.
2. KeyVault can also be used to store SSL certificates for securing traffic to and from applications over HTTPS.
3. Managed identities for Azure resources feature can be used to keep credentials out of the code completely.
4. KeyVault supports Bring Your Own Key (BYOK) feature to encrypt data using privately owned keys.
5. Use Azure Policy to create policy as code and ensure the security of your applications.

Q: What are Templated components and how to use these in Blazor?
A: I cannot find any relevant information about Templated components and how to use these in Blazor in the given source text.

Q: What is the Fractal Analytic Model?
A: The Fractal Analytic Model is an approach to engineering complete solutions in data science that requires progressively decomposing a problem into smaller sub-problems. At any given stage, the analytic itself is a collection of smaller computations that decompose into yet smaller computations, until only a single analytic technique is needed to achieve the analytic goal. The Fractal Analytic Model involves dividing the problem into four component pieces: goal, action, data, and computation, each with their own sub-problems, data, computations, and actions. The model is iterative in nature, where early versions of an analytic follow the same development process as later versions.

Q: What are fractals?
A: Fractals are mathematical sets that display self-similar patterns. As an example given in the text, a stalk of broccoli is a fractal because as you zoom in on a piece of broccoli, the same patterns reappear and progressively smaller pieces of broccoli still look like the original stalk. In the context of Data Science, analytics are also described as fractal in nature, meaning that they follow a development process where the analytic itself is a collection of smaller analytics that often decompose into yet smaller analytics.

Q: What is data science?
A: Data Science is the process of turning data into insights and taking actionable steps based on those insights. It involves the use of techniques and tools to analyze data and create models that can be used to make informed decisions. Data Scientists are experts in this field who are able to create radical new ways of thinking about data and its relevance in our lives. They play a critical role in guiding organizations towards making the most out of their data as a resource. In today's data-driven world, data is considered the new currency, and Data Science is the mechanism by which we can tap into it to solve some of humanity's toughest challenges.

Q: What are examples of good data science teams?
A: Examples of good data science teams include a multidisciplinary team of computer scientists, mathematicians, and domain experts, as well as companies with strong data science teams that focus on a diverse set of government and commercial clients across a variety of domains. Booz Allen Hamilton is noted as an industry-leading team of data scientists with a unique perspective on the conceptual models, tradecraft, processes, and culture of data science.

Q: What is the advice state of data maturity?
A: The advice state of data maturity is the highest level of maturity described in the text. Organizations that reach this level are able to achieve true insights and real competitive advantage by utilizing past observations to predict future observations and optimizing decisions for the best outcomes. The advice at this stage is to target advertise to specific groups for certain products to maximize revenue. However, the text also notes that very few organizations have reached this level of maturity and it is the new frontier of Data Science.

Q: What is the collect state of data maturity?
A: The Collect stage is the initial stage of data maturity where organizations focus on identifying and collecting internal or external datasets. The effort is focused on aggregating data, identifying the required data and collecting it. Gathering sales records and corresponding weather data is an example of data collection in this stage.

Q: What are Templated components and how to use these in Blazor?
A: Templated components in Blazor allow developers to define component parameters of type RenderFragment or RenderFragment<T>, which represent chunks of Razor markup that can be rendered by the component. These components can be used to render data in a customizable way, similar to ASP.NET Web Forms templated controls like Repeater and DataList.
*/