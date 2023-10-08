namespace TextSplitter
{
    public struct StringProvider : ITextProvider {
        public readonly string Text;
        public StringProvider(string text) {
            Text = text;
        }

        ITextPosition ITextProvider.GetStartPosition() => new StringPosition(Text);
    }
}