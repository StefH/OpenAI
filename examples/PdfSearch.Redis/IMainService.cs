namespace PdfSearch.Redis;

internal interface IMainService
{
    Task CallQuestionAsync(string filePath, string question);
}