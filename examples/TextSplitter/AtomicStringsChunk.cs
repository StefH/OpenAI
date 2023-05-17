using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSplitter
{
    public struct AtomicStringsChunk : ITextChunk
    {
        public readonly IReadOnlyList<string> Text;
        private readonly int _length;
        int ITextChunk.Length => _length;

        public AtomicStringsChunk(IReadOnlyList<string> text) {
            Text = text;
            _length = text.Sum(s => s.Length);
        }

        void ITextChunk.WriteTo(TextWriter writer)
        {
            for (int i = 0; i < Text.Count; i++)
                writer.Write(Text[i]);
        }
    }
}