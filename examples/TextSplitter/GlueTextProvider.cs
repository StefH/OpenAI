namespace TextSplitter
{
    public struct GlueTextProvider : ITextProvider
    {
        public ITextProvider Left { get; }
        public ITextProvider Right { get; }
        public GlueTextProvider(ITextProvider left, ITextProvider right) {
            Left = left;
            Right = right;
        }

        ITextPosition ITextProvider.GetStartPosition() => 
            new GluedPosition(Left.GetStartPosition(), Right.GetStartPosition());

        private struct GluedPosition : ITextPosition
        {
            private ITextPosition _left;
            private ITextPosition _right;
            bool ITextPosition.IsAtEnd => _left.IsAtEnd && _right.IsAtEnd;

            public GluedPosition(ITextPosition left, ITextPosition right) {
                _left = left;
                _right = right;
            }

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                var (leftChunk, leftRest) = _left.GetText(maxLength);
                if (!leftRest.IsAtEnd) {
                    // when the first iterator can still produce more text, simply pass out the
                    // first bit.
                    return (leftChunk, new GluedPosition(leftRest, _right));
                }
                // otherwise, check that the second iterator can also provide text - else fail.
                int remainingLength = maxLength - leftChunk.Length;

                var (rightChunk, rightRest) = _right.GetText(remainingLength);
                if (rightChunk.Length == 0) {
                    if (!rightRest.IsAtEnd) {
                        // failure: the first chunk finished but the second one didn't start
                        return (EmptyChunk.Instance, this);
                    }
                    return (leftChunk, rightRest);
                }
                return (new CombinedChunk(leftChunk, rightChunk), rightRest);
            }
        }
    }
}