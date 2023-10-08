using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSplitter
{
    public struct CombinedChunk : ITextChunk
    {
        private readonly ITextChunk[] _chunks;
        private readonly int _length;
        int ITextChunk.Length => _length;

        public CombinedChunk(IEnumerable<ITextChunk> chunks) {
            _chunks = chunks.ToArray();
            _length = _chunks.Sum(c => c.Length);
        }

        public CombinedChunk(ITextChunk left, ITextChunk right) {
            _chunks = new ITextChunk[2];
            _chunks[0] = left;
            _chunks[1] = right;
            _length = left.Length + right.Length;
        }

        void ITextChunk.WriteTo(TextWriter writer)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i].WriteTo(writer);
        }
    }
}