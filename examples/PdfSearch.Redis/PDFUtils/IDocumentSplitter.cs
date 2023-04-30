namespace PdfSearch.Redis.PDFUtils;

public interface IDocumentSplitter
{
    IReadOnlyList<string> Split(string filePath);
}