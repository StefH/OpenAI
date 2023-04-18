namespace LangChain.Example.TextUtils;

/// <summary>
/// Based on https://python.langchain.com/en/latest/_modules/langchain/text_splitter.html
/// </summary>
public abstract class TextSplitter
{
    protected int ChunkSize;
    protected int ChunkOverlap;
    protected Func<string, int> LengthFunction;

    protected TextSplitter(int chunkSize = 4000, int chunkOverlap = 200, Func<string, int>? lengthFunction = null)
    {
        if (chunkOverlap > chunkSize)
        {
            throw new ArgumentException($"Got a larger chunk overlap ({chunkOverlap}) than chunk size ({chunkSize}), should be smaller.");
        }

        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;
        LengthFunction = lengthFunction ?? new Func<string, int>(text => text.Length);
    }

    public abstract IReadOnlyList<string> SplitText(string text);
}