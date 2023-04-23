namespace TextSplitter
{
    public struct OptionalTextProvider : ITextProvider
    {
        public ITextProvider Provider { get; }

        public OptionalTextProvider(ITextProvider provider) {
            Provider = provider;
        }

        ITextPosition ITextProvider.GetStartPosition() => new OptionalPosition(Provider.GetStartPosition());

        private struct OptionalPosition : ITextPosition
        {
            public readonly ITextPosition Position;
            bool ITextPosition.IsAtEnd => false;

            public OptionalPosition(ITextPosition position) {
                Position = position;
            }

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                var (chunk, _) = Position.GetText(maxLength);
                return (chunk, EndPosition.Instance);
            }
        }
    }
}