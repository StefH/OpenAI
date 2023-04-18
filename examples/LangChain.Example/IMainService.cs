namespace LangChain.Example;

internal interface IMainService
{
    Task CallQuestionAsync(string filePath, string question);
}