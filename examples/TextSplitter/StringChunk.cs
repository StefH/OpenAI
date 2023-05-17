using System.IO;

namespace TextSplitter
{
    public struct StringChunk : ITextChunk
    {
        private readonly string _text;
        int ITextChunk.Length => _text.Length;

        public StringChunk(string text) {
            _text = text;
        }

        void ITextChunk.WriteTo(TextWriter writer)
        {
            writer.Write(_text);
        }
    }
}