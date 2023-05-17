using System.Collections.Generic;
using System.Linq;

namespace TextSplitter
{
    public struct AtomicTextProvider : ITextProvider
    {
        private readonly List<ITextProvider> _providers;
        public AtomicTextProvider(ITextProvider provider) {
            _providers = new List<ITextProvider>(1) { provider };
        }
        
        public AtomicTextProvider(IEnumerable<ITextProvider> provider) {
            _providers = provider.ToList();
        }

        ITextPosition ITextProvider.GetStartPosition()
        {
            if (_providers.Count == 0)
                return EndPosition.Instance;
            return new AtomicPosition(this);
        }

        private struct AtomicPosition : ITextPosition
        {
            private readonly AtomicTextProvider _provider;
            private readonly bool _atEnd;
            bool ITextPosition.IsAtEnd => _atEnd;

            public AtomicPosition(AtomicTextProvider provider) {
                _provider = provider;
                _atEnd = _provider._providers.All(p => p.GetStartPosition().IsAtEnd);
            }

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                int lengthRemaining = maxLength;
                var chunks = new List<ITextChunk>(_provider._providers.Count);
                for (int i = 0; i < _provider._providers.Count; i++) {
                    var (chunk, rest) = _provider._providers[i].GetStartPosition().GetText(lengthRemaining);
                    if (!rest.IsAtEnd)
                        return (EmptyChunk.Instance, this);
                    lengthRemaining -= chunk.Length;
                    chunks.Add(chunk);
                }
                return (new SeparatedChunk("", chunks), EndPosition.Instance);
            }
        }
    }
}