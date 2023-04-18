namespace LangChain.Example.TextUtils;

/// <summary>
/// Based on https://python.langchain.com/en/latest/_modules/langchain/text_splitter.html
/// Converted to C# using GPT-4.
/// </summary>
public class RecursiveCharacterTextSplitter : TextSplitter
{
    private readonly List<string> _separators;

    public RecursiveCharacterTextSplitter(List<string>? separators = null) : base(1000)
    {
        _separators = separators ?? new List<string> { "\r\n", "\n\n", "\r", "\n", " ", "" };
    }

    public override IReadOnlyList<string> SplitText(string text)
    {
        var finalChunks = new List<string>();
        var separator = _separators.Last();

        foreach (var s in _separators)
        {
            if (string.IsNullOrEmpty(s))
            {
                separator = s;
                break;
            }

            if (text.Contains(s))
            {
                separator = s;
                break;
            }
        }

        if (!string.IsNullOrEmpty(separator))
        {
            var splits = text.Split(new[] { separator }, StringSplitOptions.None).ToList();
            var goodSplits = new List<string>();

            foreach (var s in splits)
            {
                if (LengthFunction(s) < ChunkSize)
                {
                    goodSplits.Add(s);
                }
                else
                {
                    if (goodSplits.Any())
                    {
                        var mergedText = MergeSplits(goodSplits, separator);
                        finalChunks.AddRange(mergedText);
                        goodSplits.Clear();
                    }

                    var otherInfo = SplitText(s);
                    finalChunks.AddRange(otherInfo);
                }
            }

            if (goodSplits.Any())
            {
                var mergedText = MergeSplits(goodSplits, separator);
                finalChunks.AddRange(mergedText);
            }
        }
        else
        {
            var splits = text.Select(c => c.ToString()).ToList();
            finalChunks.AddRange(MergeSplits(splits, separator));
        }

        return finalChunks;
    }

    protected IReadOnlyList<string> MergeSplits(IEnumerable<string> splits, string separator)
    {
        var separatorLength = LengthFunction(separator);

        var docs = new List<string>();
        var currentDoc = new List<string>();
        var total = 0;

        foreach (var d in splits)
        {
            var length = LengthFunction(d);
            if (total + length + (currentDoc.Count > 0 ? separatorLength : 0) > ChunkSize)
            {
                if (total > ChunkSize)
                {
                    Console.WriteLine($"Created a chunk of size {total}, which is longer than the specified {ChunkSize}");
                }

                if (currentDoc.Count > 0)
                {
                    var doc = JoinDocs(currentDoc, separator);
                    if (doc != null)
                    {
                        docs.Add(doc);
                    }

                    while (total > ChunkOverlap || (total + length + (currentDoc.Count > 0 ? separatorLength : 0) > ChunkSize && total > 0))
                    {
                        total -= LengthFunction(currentDoc[0]) + (currentDoc.Count > 1 ? separatorLength : 0);
                        currentDoc.RemoveAt(0);
                    }
                }
            }
            currentDoc.Add(d);
            total += length + (currentDoc.Count > 1 ? separatorLength : 0);
        }

        var lastDoc = JoinDocs(currentDoc, separator);
        if (lastDoc != null)
        {
            docs.Add(lastDoc);
        }

        return docs;
    }

    private static string? JoinDocs(IReadOnlyList<string> docs, string separator)
    {
        var text = string.Join(separator, docs);
        text = text.Trim();

        return string.IsNullOrEmpty(text) ? null : text;
    }
}