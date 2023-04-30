namespace PdfSearch.SQL;

internal interface IMainService
{
    Task CallQuestionAsync(string filePath, string question);
}