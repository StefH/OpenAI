using System.Collections.Generic;

namespace TextSplitter
{

    public static class Extensions {
        public static IEnumerable<ITextChunk> GetChunks(this ITextProvider provider, int maxLength) {
            var pos = provider.GetStartPosition();
            while(!pos.IsAtEnd) {
                var (text, next) = pos.GetText(maxLength);
                if (text.Length == 0 && !next.IsAtEnd)
                    throw new TextSplitterException("Unable to fit your message to the request length!");
                if (text.Length > 0 && text.Length <= maxLength) {
                    yield return text;
                } else if (text.Length > maxLength)
                    throw new System.Exception($"Message is too long: {text.Length} - this should never happen. Check that the linebreaks you are inserting and the ones your TextWriter is producing are equivalent");
                pos = next;
            }
        }

        public static IEnumerable<string> GetSections(this ITextProvider provider, int maxLength) {
            var sb = new System.Text.StringBuilder(maxLength);
            using (var writer = new System.IO.StringWriter(sb)) {
                foreach (var chunk in GetChunks(provider, maxLength)) {
                    chunk.WriteTo(writer);
                    yield return sb.ToString();
                    sb.Clear();
                }
            }
        }

        public static string ToText(this ITextProvider provider, int length) {
            var (chunk, _) = provider.GetStartPosition().GetText(length);
            return chunk.ToText();
        }

        public static string ToText(this ITextChunk chunk) {
            var sb = new System.Text.StringBuilder(chunk.Length);
            var writer = new System.IO.StringWriter(sb);
            chunk.WriteTo(writer);
            return sb.ToString();
        }
    }
}