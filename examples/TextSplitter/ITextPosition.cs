namespace TextSplitter
{
    public interface ITextPosition {
        bool IsAtEnd { get; }
        (ITextChunk text, ITextPosition rest) GetText(int maxLength);
    }
}