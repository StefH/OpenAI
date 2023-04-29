namespace PdfCosineSearch;

internal interface IMainService
{
    Task CallQuestionAsync(string filePath, string question);
}