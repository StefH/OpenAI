namespace TextSplitter
{
    public class NoText : ITextProvider
    {
        private NoText() {}

        ITextPosition ITextProvider.GetStartPosition() => EndPosition.Instance;

        private static NoText _instance;
        public static NoText Instance {
            get {
                if (_instance == null)
                    _instance = new NoText();
                return _instance;
            }
        }
    }
}