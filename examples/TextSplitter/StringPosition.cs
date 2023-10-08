namespace TextSplitter
{
    public struct StringPosition : ITextPosition
    {
        public readonly string Text;
        public StringPosition(string text) {
            Text = text;
        }

        bool ITextPosition.IsAtEnd => false;

        (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
        {
            if (maxLength < Text.Length)
                return (EmptyChunk.Instance, this);
            return (new StringChunk(Text), EndPosition.Instance);
        }
    }
}