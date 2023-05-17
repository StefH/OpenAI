using System;
using System.IO;

namespace TextSplitter
{
    public struct SubstringChunk : ITextChunk
    {
        private readonly string _text;
        private readonly int _offset, _length;
        int ITextChunk.Length => _length;

        public SubstringChunk(string text, int offset, int length) {
            _text = text;
            _offset = offset;
            _length = length;
        }

        void ITextChunk.WriteTo(TextWriter writer)
        {
            writer.Write(_text.AsSpan(_offset, _length));
        }
    }
}