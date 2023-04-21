using LangChain.Example.PDFUtils;
using LangChain.Example.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI_API;
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
            .Build();

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
            "What is Azure DevOps?",
            "How does the Machine Learning process work?"
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
            //"What are Templated components and how to use these in Blazor?",
            "What is Fractal Analytic Model?",
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
Q: What is Azure DevOps?
A: Azure DevOps is a Microsoft product that provides software development teams with a comprehensive set of tools for the entire development lifecycle. 
   It enables developers to manage code repositories, build and test applications, and deploy them to Azure or other platforms. 
   Teams can also use Azure DevOps to manage infrastructure as code, track work items, and monitor application performance. 
   The platform offers a range of features, including the ability to customize workflows and pipelines, integrate with third-party tools, and access a marketplace for extensions and add-ons. 
   Security is also a key feature of Azure DevOps, with tools for integrating security into workflows and proactively scanning repositories for potential vulnerabilities.

Q: How does the Machine Learning process work?
A: The Azure Machine Learning platform offers end-to-end capabilities for the Machine Learning process. 
   Data can be prepared, models can be trained, tested, and deployed, and their lifecycle can be tracked through the model registry. 
   The platform supports popular languages such as Python, R, and Azure CLI, as well as open-source technologies such as TensorFlow, PyTorch, and scikit-learn. 
   Additionally, it provides a low-code/no-code entry system to help those who need assistance getting started with ML concepts. 
   The platform also offers an automated ML experience where multiple ML experiments are run in parallel to identify the ideal algorithm for a scenario. 
   Overall, the Azure Machine Learning platform can save time, improve model accuracy, and enable reliable deployments when building custom models.

Q: What are Templated components and how to use these in Blazor?
A: I'm sorry, but the given text does not provide any information related to templated components and Blazor.

Q: What are fractals?
A: Fractals are mathematical sets that display self-similar patterns, meaning that as you zoom in on a fractal, the same patterns reappear. 
   They can be compared to a stalk of broccoli where progressively smaller pieces of broccoli look like the original stalk. 
   This concept is used to describe the nature of Data Science analytics, which are also fractal in nature in both time and construction. 
   The Fractal Analytic Model embodies the iterative nature of good Data Science and problem decomposition creates multiple sub-problems, each with their own goals, data, computations, and actions, which can be classified into different classes of analytics.

Q: What is data science?
A: Data Science is the art of turning data into actions and creating data products that provide actionable information without exposing decision makers to the underlying data or analytics. 
   It involves the extraction of timely, actionable information from diverse data sources to drive data products that answer questions and solve problems. 
   It is the competitive advantage for organizations interested in winning and improving decision-making.

Q: What are examples of good data science teams?
A: Good data science teams are those that have a broad view of various technologies and industries, constantly test and improve their models, and are able to tackle increasingly complex analytical goals. 
   They also prioritize collaboration and open communication within the team and with other departments in the organization. 
   Success can be achieved at every stage of maturity in data science capabilities.

Q: What is the advice state of data maturity?
A: The advice state of data maturity is the highest level of maturity in the Data Science capability model. 
   It is the stage where an organization can define its possible decisions, optimize over those decisions, and advise on the decision that gives the best outcome. 
   At this stage, organizations have the ability to generate true insights and gain a significant competitive advantage. 
   However, very few organizations are currently operating at this level of maturity.

Q: What is the collect state of data maturity?
A: The Collect stage of data maturity focuses on identifying and gathering internal or external datasets, and is the starting point for creating a Data Science capability within an organization.
*/

/*
Q: What are Templated components and how to use these in Blazor?
A: Templated components in Blazor are reusable UI components that enable developers to specify a portion of the HTML used to render a container control. 
   This is done by defining component parameters of type RenderFragment or RenderFragment<T>. 
   A RenderFragment represents a chunk of Razor markup that can then be rendered by the component, while a RenderFragment<T> is a chunk of Razor markup that takes a parameter that can be specified when the render fragment is rendered. 
   To capture child content, developers can define a component parameter of type RenderFragment and name it ChildContent. 
   Templated components can also define multiple component parameters of type RenderFragment or RenderFragment<T>, and the parameter for a RenderFragment<T> can be specified when it's invoked. 
   To use a templated component, developers can bring the component's namespace into scope using the @using directive, and then specify the component using its name in Razor syntax.
*/