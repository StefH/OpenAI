using System.Text;
using System.Text.RegularExpressions;
using PdfSearch.Redis.PdfUtils.Models;
using UglyToad.PdfPig;

namespace PdfSearch.Redis.PDFUtils;

internal class DocumentSplitterPdfTextFragment : IDocumentSplitterPdfTextFragment
{
    private const int MaxCharactersPerTextFragment = 1000;
    private const char Dot = '.';
    private const char Space = ' ';
    private static readonly char[] SplitChars = { Dot, '\r', '\n' };
    private static readonly Regex LineSplitRegex = new($"(.{{1,{MaxCharactersPerTextFragment}}}(?=\\s|$))", RegexOptions.Compiled);

    public IReadOnlyList<PdfTextFragment> Split(string filePath)
    {
        var pdfTextFragments = new List<PdfTextFragment>();

        using (var document = PdfDocument.Open(filePath))
        {
            var textFragmentsWithText = document
                .GetPages()
                .Select(p => new PdfTextFragment(p.Text.Trim(), p.Number))
                .Where(t => string.IsNullOrWhiteSpace(t.Text));

            pdfTextFragments.AddRange(textFragmentsWithText);
        }

        var lineFragments = SplitToLines(pdfTextFragments);

        return GetChunks(lineFragments)
            .ToArray();
    }

    private static IReadOnlyList<PdfTextFragment> SplitToLines(IReadOnlyList<PdfTextFragment> textFragments)
    {
        return textFragments
            .Select(t => t.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries).Select(line => t with { Text = line.Trim() }))
            .SelectMany(t => t)
            .SelectMany(textFragment => 
            {
                // If the line is too long, split it into multiple textFragments.
                if (textFragment.Text.Length >= MaxCharactersPerTextFragment - 1)
                {
                    var extraLines = LineSplitRegex
                        .Split(textFragment.Text) // Split the line using the Regex.
                        .Where(extraLine => !string.IsNullOrWhiteSpace(extraLine)) // Remove empty textFragments.
                        .Select(extraLine => extraLine.Trim()) // Trim the line.
                        .ToArray();

                    // Add a dot to the last line.
                    extraLines[^1] += Dot;

                    return extraLines.Select(e => textFragment with { Text = e });
                }

                // Add a dot to the line.
                return new[] { textFragment with { Text = textFragment.Text + Dot } };
            })
            .ToArray();
    }

    private static IReadOnlyList<PdfTextFragment> GetChunks(IReadOnlyList<PdfTextFragment> textFragments)
    {
        var chunks = new List<PdfTextFragment>();

        var stringBuilder = new StringBuilder();
        foreach (var textFragment in textFragments)
        {
            // If the line is too long, split it into multiple textFragments.
            if (stringBuilder.Length <= MaxCharactersPerTextFragment - 1)
            {
                stringBuilder.Append(textFragment);
                stringBuilder.Append(Space);
            }
            else
            {
                chunks.Add(textFragment with { Text = stringBuilder.ToString() });

                stringBuilder.Clear();

                stringBuilder.Append(textFragment);
                stringBuilder.Append(Space);
            }
        }

        return chunks;
    }
}