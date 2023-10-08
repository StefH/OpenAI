using System.IO;

namespace TextSplitter
{
    public class EmptyChunk : ITextChunk
    {
        int ITextChunk.Length => 0;
        void ITextChunk.WriteTo(TextWriter writer) {}

        private EmptyChunk() {}

        private static EmptyChunk _instance;
        public static EmptyChunk Instance {
            get {
                if (_instance == null)
                    _instance = new EmptyChunk();
                return _instance;
            }
        }
    }
}