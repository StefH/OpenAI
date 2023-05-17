using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSplitter
{
    public struct SeparatedChunk : ITextChunk {
        private readonly ITextChunk[] _chunks;
        private readonly string _separator;
        private readonly int _length;
        int ITextChunk.Length => _length;

        public SeparatedChunk(string separator, IEnumerable<ITextChunk> chunks) {
            _separator = separator;
            _chunks = chunks.ToArray();
            if (_chunks.Length > 0)
                _length = _chunks.Sum(c => c.Length) + (_chunks.Length - 1) * separator.Length;
            else
                _length = 0;
        }

        public SeparatedChunk(string separator, ITextChunk left, ITextChunk right) {
            _separator = separator;
            _chunks = new ITextChunk[2];
            _chunks[0] = left;
            _chunks[1] = right;
            _length = left.Length + right.Length;
        }

        void ITextChunk.WriteTo(TextWriter writer)
        {
            for (int i = 0; i < _chunks.Length; i++) {
                if (i > 0)
                    writer.Write(_separator);
                _chunks[i].WriteTo(writer);
            }
        }
    }
}