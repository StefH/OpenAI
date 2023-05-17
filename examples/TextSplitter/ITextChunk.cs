using System.IO;

namespace TextSplitter
{
    public interface ITextChunk {
        int Length { get; }
        void WriteTo(TextWriter writer);
    }
}