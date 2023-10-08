namespace TextSplitter
{
    public class EndPosition : ITextPosition
    {
        bool ITextPosition.IsAtEnd => true;

        (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength) => (EmptyChunk.Instance, this);

        private EndPosition() {}

        private static EndPosition _instance;
        public static EndPosition Instance {
            get {
                if (_instance == null)
                    _instance = new EndPosition();
                return _instance;
            }
        }
    }
}