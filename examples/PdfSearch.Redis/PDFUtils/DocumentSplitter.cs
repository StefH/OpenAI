using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace PdfSearch.Redis.PDFUtils;

internal class DocumentSplitter : IDocumentSplitter
{
    private const int MaxCharactersPerTextFragment = 1000;
    private const char Dot = '.';
    private const char Space = ' ';
    private static readonly char[] SplitChars = { Dot, '\r', '\n' };
    private static readonly Regex LineSplitRegex = new ($"(.{{1,{MaxCharactersPerTextFragment}}}(?=\\s|$))", RegexOptions.Compiled);

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

    return GetTextFragments(lines)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToArray();
}

    private static string[] SplitToLines(StringBuilder stringBuilder)
    {
        return stringBuilder
            .ToString()
            .Split(SplitChars, StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(line =>
            {
                // Trim the line.
                var trimmedLine = line.Trim();

                // If the line is too long, split it into multiple lines.
                if (trimmedLine.Length >= MaxCharactersPerTextFragment - 1)
                {
                    var extraLines = LineSplitRegex
                        .Split(line) // Split the line using the Regex.
                        .Where(extraLine => !string.IsNullOrWhiteSpace(extraLine)) // Remove empty lines.
                        .Select(extraLine => extraLine.Trim()) // Trim the line.
                        .ToArray();

                    // Add a dot to the last line.
                    extraLines[^1] += Dot;

                    return extraLines;
                }

                // Add a dot to the line.
                return new[] { trimmedLine + Dot };
            })
            .ToArray();
    }

    private static IReadOnlyList<string> GetTextFragments(string[] lines)
    {
        var textFragments = new List<string>();

        var stringBuilder = new StringBuilder();
        foreach (var line in lines)
        {
            // If the line is too long, split it into multiple lines.
            if (stringBuilder.Length <= MaxCharactersPerTextFragment - 1)
            {
                stringBuilder.Append(line);
                stringBuilder.Append(Space);
            }
            else
            {
                textFragments.Add(stringBuilder.ToString());

                stringBuilder.Clear();

                stringBuilder.Append(line);
                stringBuilder.Append(Space);
            }
        }
        
        return textFragments;
    }
}