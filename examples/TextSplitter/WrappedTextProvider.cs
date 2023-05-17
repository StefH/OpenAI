using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSplitter
{
    public class WrappedTextProvider : ITextProvider
    {
        private readonly ITextProvider _provider;
        public string WrapStart { get; }
        public string WrapEnd { get; }
        public WrappedTextProvider(ITextProvider text, string wrapStart, string wrapEnd) {
            _provider = text;
            WrapStart = wrapStart;
            WrapEnd = wrapEnd;
        }

        ITextPosition ITextProvider.GetStartPosition() => new WrappedTextPosition(this);

        private struct WrappedChunk : ITextChunk
        {
            private readonly WrappedTextProvider _text;
            private readonly ITextChunk _chunk;
            

            public WrappedChunk(WrappedTextProvider wrapper, ITextChunk chunk) {
                _text = wrapper;
                _chunk = chunk;
            }

            int ITextChunk.Length => _chunk.Length + _text.WrapEnd.Length + _text.WrapStart.Length;

            void ITextChunk.WriteTo(TextWriter writer)
            {
                if (_chunk.Length == 0)
                    return;
                writer.Write(_text.WrapStart);
                _chunk.WriteTo(writer);
                writer.Write(_text.WrapEnd);
            }
        }

        private struct WrappedTextPosition : ITextPosition
        {
            private readonly WrappedTextProvider _container;
            private readonly ITextPosition _position;
            bool ITextPosition.IsAtEnd => _position.IsAtEnd;

            public WrappedTextPosition(WrappedTextProvider container) {
                _container = container;
                _position = container._provider.GetStartPosition();
            }

            private WrappedTextPosition(WrappedTextProvider container, ITextPosition position) {
                _container = container;
                _position = position;
            }

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                int actualMax = maxLength - _container.WrapStart.Length - _container.WrapEnd.Length;
                if (actualMax < 0)
                    return (EmptyChunk.Instance, this);
                var (chunk, pos) = _position.GetText(actualMax);
                if (chunk.Length == 0)
                    return (EmptyChunk.Instance, this);
                return (new WrappedChunk(_container, chunk), new WrappedTextPosition(_container, pos));
            }
        }
    }
}