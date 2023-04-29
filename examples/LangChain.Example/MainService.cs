using System.Runtime.InteropServices;
using System.Text;
using LangChain.Example.PDFUtils;
using LangChain.Example.Redis;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using SharpToken;

namespace LangChain.Example;

internal class MainService : IMainService
{
    private const int MaxTokenLength = 4096;
    private const int MaxTokenLengthGpt4 = 8192;
    private readonly GptEncoding _encoding = GptEncoding.GetEncoding("cl100k_base");
    private const string NullAnswer = "NULL";

    private readonly ILogger<MainService> _logger;
    private readonly IDocumentSplitter _documentSplitter;
    private readonly IRedisDatabaseService _dataInserter;
    private readonly IOpenAIAPI _openAiAPI;

    public MainService(ILogger<MainService> logger, IDocumentSplitter documentSplitter, IRedisDatabaseService dataInserter, IOpenAIAPI openAiAPI)
    {
        _logger = logger;
        _documentSplitter = documentSplitter;
        _dataInserter = dataInserter;
        _openAiAPI = openAiAPI;
    }

    public async Task CallQuestionAsync(string filePath, string question)
    {
        question = question.Trim();
        if (string.IsNullOrEmpty(question))
        {
            Console.WriteLine("You did not specify a question.");
            Console.WriteLine();
        }

        Console.WriteLine("Q: {0}", question);
        Console.Write("A: ");

        var moderation = await _openAiAPI.Moderation.WithRetry(moderation => moderation.CallModerationAsync(question));
        if (moderation.Results.Any(r => r.Flagged))
        {
            Console.WriteLine("Sorry, that question is not allowed.");
            Console.WriteLine();
            return;
        }

        var questionAsVector = await _openAiAPI.Embeddings.WithRetry(api => api.GetEmbeddingsAsync(question));
        var questionAsBytes = MemoryMarshal.Cast<float, byte>(questionAsVector).ToArray();

        var canUseGpt4 = await CanUseGPT4Async() && false;

        var chat = _openAiAPI.Chat.WithRetry(chatEndpoint => chatEndpoint.CreateConversation());
        if (canUseGpt4)
        {
            _logger.LogInformation("Using GPT-4");
            chat.Model = "gpt-4";
        }

        var prefix = Path.GetFileNameWithoutExtension(filePath);
        var indexName = $"{prefix}-index";

        if (!_dataInserter.DoesDataExists(prefix))
        {
            await AddDataAsync(filePath, indexName, prefix);
        }

        var vectorDocuments = await _dataInserter.SearchAsync(indexName, questionAsBytes);
        var textBuilder = new StringBuilder();

        int tokenLength = 0;
        foreach (var vectorDocument in vectorDocuments)
        {
            if (tokenLength < (canUseGpt4 ? MaxTokenLengthGpt4 : MaxTokenLength))
            {
                textBuilder.AppendLine(vectorDocument.Text);
                tokenLength += vectorDocument.TokenLength;
            }
            else
            {
                _logger.LogDebug("TokenLength is max");
                break;
            }
        }

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine($"Based on the following source text \"{textBuilder.ToString()}\" follow the next requirements:");
        contentBuilder.AppendLine($"- Answer the question: \"{question}\"");
        contentBuilder.AppendLine(@"- Make sure to give a concrete answer");
        contentBuilder.AppendLine(@"- Do not start your answer with ""Based on the source text,""");
        contentBuilder.AppendLine(@"- Only base your answer on the source text");
        contentBuilder.AppendLine(@"- When you cannot give a good answer based on the source text, return ""I cannot find any relevant information.""");

        /*
        contentBuilder.AppendLine(@"Based on the source text answer the question and follow the next requirements:");

        contentBuilder.AppendLine(@"- Make sure to give a concrete answer");
        contentBuilder.AppendLine(@"- Do not start your answer with ""Based on the source text,""");
        contentBuilder.AppendLine(@"- Only base your answer on the source text");
        contentBuilder.AppendLine(@"- When you cannot give a good answer based on the text, return ""I cannot find any relevant information.""");

        contentBuilder.AppendLine($"source text: \"{textBuilder}\"");
        contentBuilder.AppendLine($"question: \"{question}\"");*/

        //chat.WithRetry(c => c.AppendUserInput(contentBuilder.ToString()));
        chat.AppendUserInput(contentBuilder.ToString());

        var response = await chat.WithRetry(conversation => conversation.GetResponseFromChatbotAsync());
        //if (response == NullAnswer)
        //{
        //    continue;
        //}

        Console.Write(response);

        //var chatbotResponses = chat.WithRetry(c => c.StreamResponseEnumerableFromChatbotAsync());
        //var filteredResponses = FilterResponseAsync(chatbotResponses);

        //await foreach (var response in filteredResponses)
        //{
        //    if (response == NullAnswer)
        //    {
        //        continue;
        //    }

        //    Console.Write(response);
        //}


        Console.WriteLine();
        Console.WriteLine();
    }
    
    private async Task<bool> CanUseGPT4Async()
    {
        var models = await _openAiAPI.Models.WithRetry(modelsEndpoint => modelsEndpoint.GetModelsAsync());
        return models.Any(m => m.ModelID == "gpt-4");
    }

    //private static async IAsyncEnumerable<string> FilterResponseAsync(IAsyncEnumerable<string> inputEnumerable)
    //{
    //    var combinedText = new StringBuilder();
    //    bool isFirstElement = true;

    //    await foreach (var text in inputEnumerable)
    //    {
    //        if (isFirstElement)
    //        {
    //            combinedText.Append(text);
    //            if (combinedText.Length >= NullAnswer.Length)
    //            {
    //                if (combinedText.ToString()[..NullAnswer.Length] == NullAnswer)
    //                {
    //                    combinedText.Remove(0, NullAnswer.Length);
    //                }

    //                isFirstElement = false;

    //                if (combinedText.Length > 0)
    //                {
    //                    yield return combinedText.ToString();
    //                    combinedText.Clear();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            yield return text;
    //        }
    //    }
    //}

    private async Task AddDataAsync(string filePath, string indexName, string prefix)
    {
        var parts = _documentSplitter.Split(filePath);

        await _dataInserter.InsertAsync(
            indexName: indexName,
            prefix: prefix,
            parts,
            embeddingFunc: input => _openAiAPI.Embeddings.WithRetry(embeddings => embeddings.GetEmbeddingsAsync(input)),
            tokenFunc: async input => await Task.Run(() => _encoding.Encode(input))
        );
    }
}