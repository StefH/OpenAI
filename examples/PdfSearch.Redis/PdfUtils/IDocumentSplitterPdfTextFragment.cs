using PdfSearch.Redis.PdfUtils.Models;

namespace PdfSearch.Redis.PDFUtils;

public interface IDocumentSplitterPdfTextFragment
{
    IReadOnlyList<PdfTextFragment> Split(string filePath);
}