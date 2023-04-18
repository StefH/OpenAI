using System.Runtime.InteropServices;
using System.Text;
using LangChain.Example.PDFUtils;
using LangChain.Example.Redis;
using OpenAI_API;

namespace LangChain.Example;

internal class MainService : IMainService
{
    private const string NullAnswer = "NULL";

    private readonly IDocumentSplitter _documentSplitter;
    private readonly IRedisDatabaseService _dataInserter;
    private readonly IOpenAIAPI _openAiAPI;

    public MainService(IDocumentSplitter documentSplitter, IRedisDatabaseService dataInserter, IOpenAIAPI openAiAPI)
    {
        _documentSplitter = documentSplitter;
        _dataInserter = dataInserter;
        _openAiAPI = openAiAPI;
    }

    public async Task CallQuestionAsync(string filePath, string question)
    {
        var prefix = Path.GetFileNameWithoutExtension(filePath);
        var indexName = $"{prefix}-index";

        if (!_dataInserter.DoesDataExists(prefix))
        {
            await AddData(filePath, indexName, prefix);
        }

        Console.WriteLine("Q: {0}", question);

        var questionEmbeddings = await _openAiAPI.WithRetry(api => api.Embeddings.GetEmbeddingsAsync(question));
        var questionArray = MemoryMarshal.Cast<float, byte>(questionEmbeddings).ToArray();

        var chat = _openAiAPI.WithRetry(api =>api.Chat.CreateConversation());
        // chat.Model = "gpt-4";

        var vectorDocuments = await _dataInserter.SearchAsync(indexName, questionArray);
        // var searchText = string.Join(" ", parts);

        Console.Write("A: ");
        var text = string.Join(" ", vectorDocuments.Take(3).Select(v => v.Text));

        //foreach (var vectorDocument in vectorDocuments)
        {
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"Based on the following source text \"{text}\" follow the next requirements:");
            contentBuilder.AppendLine($"- Answer the question: \"{question}\"");
            //contentBuilder.AppendLine(@"- Ignore any invalid or non-existing words in the source text");
            contentBuilder.AppendLine(@"- Make sure to give a concrete answer");
            contentBuilder.AppendLine(@"- Do not start your answer with ""Based on the source text,""");
            contentBuilder.AppendLine(@"- Only base your answer on the source text");
            //contentBuilder.AppendLine(@"- When you cannot give a good answer based on the text, return only the text NULL");

            chat.WithRetry(c => c.AppendUserInput(contentBuilder.ToString()));

            var response = await chat.WithRetry(c => c.GetResponseFromChatbotAsync());
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
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    private static async IAsyncEnumerable<string> FilterResponseAsync(IAsyncEnumerable<string> inputEnumerable)
    {
        var combinedText = new StringBuilder();
        bool isFirstElement = true;

        await foreach (var text in inputEnumerable)
        {
            if (isFirstElement)
            {
                combinedText.Append(text);
                if (combinedText.Length >= NullAnswer.Length)
                {
                    if (combinedText.ToString()[..NullAnswer.Length] == NullAnswer)
                    {
                        combinedText.Remove(0, NullAnswer.Length);
                    }

                    isFirstElement = false;

                    if (combinedText.Length > 0)
                    {
                        yield return combinedText.ToString();
                        combinedText.Clear();
                    }
                }
            }
            else
            {
                yield return text;
            }
        }
    }

    private async Task AddData(string filePath, string indexName, string prefix)
    {
        var parts = _documentSplitter.Split(filePath);

        await _dataInserter.InsertAsync(
            indexName: indexName,
            prefix: prefix,
            parts,
            func: input => _openAiAPI.WithRetry(api => api.Embeddings.GetEmbeddingsAsync(input))
        );
    }
}