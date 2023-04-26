using System.Text;
using UglyToad.PdfPig;

namespace LangChain.Example.PDFUtils;

internal class DocumentSplitter : IDocumentSplitter
{
    private const char Dot = '.';
    private const char Space = ' ';
    private const int MaxCharactersPerChunk = 1000;
    private static readonly char[] SplitChars = { Dot, '\r', '\n' };

    public IReadOnlyList<string> Split(string filePath)
    {
        var stringBuilder = new StringBuilder();

        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages().Where(p => !string.IsNullOrWhiteSpace(p.Text)))
            {
                stringBuilder.Append(page.Text.Trim());
            }
        }

        var lines = SplitToLines(stringBuilder);

        return GetChunks(lines);
    }

    private static string[] SplitToLines(StringBuilder stringBuilder)
    {
        return stringBuilder.ToString()
            .Split(SplitChars)
            .Select(line => line.Trim() + Dot)
            .ToArray();
    }

    private static IReadOnlyList<string> GetChunks(string[] lines)
    {
        var chunks = new List<string>();
        var currentChunk = string.Empty;

        foreach (string line in lines)
        {
            if (currentChunk.Length + line.Length + 1 <= MaxCharactersPerChunk)
            {
                currentChunk += line + Space;
            }
            else
            {
                chunks.Add(currentChunk);
                currentChunk = line + Space;
            }
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk);
        }

        return chunks;
    }
}