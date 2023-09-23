using System.Text;
using UglyToad.PdfPig;

namespace CodeSnippets;

internal class PdfExample
{

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

    string[] lines = SplitToLines(stringBuilder);

    return GetTextFragments(lines)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToArray();
}

private string[] GetTextFragments(string[] lines)
{
    throw new NotImplementedException();
}

private string[] SplitToLines(StringBuilder stringBuilder)
{
    throw new NotImplementedException();
}

}