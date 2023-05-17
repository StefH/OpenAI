using System.Collections.Generic;
using System.Linq;

namespace TextSplitter
{
    public struct AtomicStringsProvider : ITextProvider {
        public readonly IReadOnlyList<string> Text;
        public AtomicStringsProvider(IReadOnlyList<string> text) {
            Text = text;
        }

        ITextPosition ITextProvider.GetStartPosition() => new AtomicStringsPosition(Text);

        private struct AtomicStringsPosition : ITextPosition
        {
            public readonly IReadOnlyList<string> Text;
            private readonly int _length;

            public AtomicStringsPosition(IReadOnlyList<string> text)
            {
                Text = text;
                _length = Text.Sum(s => s.Length);
            }

            bool ITextPosition.IsAtEnd => _length == 0;

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                if (maxLength >= _length)
                    return (new AtomicStringsChunk(Text), EndPosition.Instance);
                return (EmptyChunk.Instance, this);
            }
        }
    }
}