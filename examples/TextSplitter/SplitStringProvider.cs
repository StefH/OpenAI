using System;

namespace TextSplitter
{
    public class SplitStringProvider : ITextProvider
    {
        private readonly string _text;
        private readonly Predicate<char> _splitPredicate;
        public SplitStringProvider(string text, Predicate<char> splitPredicate) {
            _splitPredicate = splitPredicate;
            _text = text;
        }

        ITextPosition ITextProvider.GetStartPosition()
        {
            return new SplitStringPosition(this, 0);
        }

        private class SplitStringPosition : ITextPosition
        {
            bool ITextPosition.IsAtEnd => false;
            private readonly SplitStringProvider _provider;
            private readonly int _offset;
            public SplitStringPosition(SplitStringProvider provider, int offset) {
                _provider = provider;
                _offset = offset;
            }

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                var split = TextSplitter.TakeUntil(_provider._text.AsSpan(_offset), _provider._splitPredicate, maxLength);
                ITextChunk chunk;
                if (split.Length == 0)
                    chunk = EmptyChunk.Instance;
                else
                    chunk = new SubstringChunk(_provider._text, _offset + split.Start, split.Length);

                int next = _offset + split.Next;                
                if (next >= _provider._text.Length)
                    return (chunk, EndPosition.Instance);
                return (chunk, new SplitStringPosition(_provider, next));
            }
        }
    }
}