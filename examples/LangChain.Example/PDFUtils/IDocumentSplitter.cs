namespace LangChain.Example.PDFUtils;

public interface IDocumentSplitter
{
    IReadOnlyList<string> Split(string filePath);
}