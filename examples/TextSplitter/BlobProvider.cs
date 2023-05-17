namespace TextSplitter
{
    public class BlobProvider : ITextProvider
    {
        private readonly string _text;

        public BlobProvider(string text)
        {
            _text = text;
        }

        ITextPosition ITextProvider.GetStartPosition()
        {
            return new BlobPosition(_text, 0);
        }

        private struct BlobPosition : ITextPosition
        {
            private readonly int _offset;
            private readonly string _text;
            bool ITextPosition.IsAtEnd => false;

            public BlobPosition(string text, int offset) {
                _text = text;
                _offset = offset;
            }

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                int textLeft = _text.Length - _offset;
                if (maxLength > textLeft) {
                    var chunk = new SubstringChunk(_text, _offset, textLeft);
                    return (chunk, EndPosition.Instance);
                } else {
                    var chunk = new SubstringChunk(_text, _offset, maxLength);
                    return (chunk, new BlobPosition(_text, _offset + maxLength));
                }
            }
        }
    }
}