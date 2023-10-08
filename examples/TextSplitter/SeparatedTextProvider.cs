using System.Collections.Generic;
using System.Linq;

namespace TextSplitter
{
    public struct SeparatedTextProvider : ITextProvider {
        private readonly List<ITextProvider> _subproviders;
        private readonly string _separator;
        public SeparatedTextProvider(string separator, IEnumerable<ITextProvider> providers) {
            _subproviders = providers.ToList();
            _separator = separator;
        }

        public SeparatedTextProvider(IEnumerable<ITextProvider> providers) {
            _subproviders = providers.ToList();
            _separator = string.Empty;
        }

        ITextPosition ITextProvider.GetStartPosition() => new SeparatedPosition(this);

        private class SeparatedPosition : ITextPosition {
            private readonly SeparatedTextProvider _provider;
            private readonly ITextPosition _currentPosition;
            private readonly  int _currentSubProvider;

            public SeparatedPosition(SeparatedTextProvider provider) {
                _provider = provider;
                _currentSubProvider = 0;
                if (provider._subproviders.Count > 0)
                    _currentPosition = provider._subproviders[0].GetStartPosition();
                else
                    _currentPosition = EndPosition.Instance;
            }

            private SeparatedPosition(SeparatedTextProvider provider, int currentProvider, ITextPosition currentPosition) {
                _provider = provider;
                _currentSubProvider = currentProvider;
                _currentPosition = currentPosition;
            }

            bool ITextPosition.IsAtEnd => _currentSubProvider >= _provider._subproviders.Count;

            (ITextChunk text, ITextPosition rest) ITextPosition.GetText(int maxLength)
            {
                var (firstChunk, firstNext) = _currentPosition.GetText(maxLength);
                if (_currentSubProvider >= _provider._subproviders.Count - 1) {
                    // if this is the last chunk, we can just delegate to that
                    return (firstChunk, firstNext);
                } else if (!firstNext.IsAtEnd) {
                    // otherwise, we can still exit here because we didn't finish the chunk
                    return (firstChunk, new SeparatedPosition(_provider, _currentSubProvider, firstNext));
                }
                // advance to the next provider, but keep in mind that it may also exit early
                int remainingLength = maxLength - firstChunk.Length - _provider._separator.Length;
                var subProvider = _currentSubProvider + 1;
                var position = _provider._subproviders[subProvider].GetStartPosition();

                var (secondChunk, secondNext) = position.GetText(remainingLength);
                if (secondChunk.Length == 0 && !secondNext.IsAtEnd) {
                    // the first chunk is done, but the second didn't start.
                    return (firstChunk, new SeparatedPosition(_provider, subProvider, position));
                } else if (!secondNext.IsAtEnd) {
                    // we just need to return the first two chunks and continue with the second later
                    var pos = new SeparatedPosition(_provider, subProvider, secondNext);
                    if (_provider._separator.Length == 0)
                        return (new CombinedChunk(firstChunk, secondChunk), pos);
                    return (new SeparatedChunk(_provider._separator, firstChunk, secondChunk), pos);
                }

                // otherwise, collect the remaining chunks
                var chunks = new List<ITextChunk> { firstChunk, secondChunk };
                return CollectRemainingChunks(chunks, remainingLength - secondChunk.Length, _provider, subProvider, secondNext);
            }

            private static (ITextChunk, ITextPosition) CollectRemainingChunks(List<ITextChunk> chunks, int remainingLength, SeparatedTextProvider text, int subProvider, ITextPosition position) {
                do {
                    if (position.IsAtEnd) {
                        if (subProvider >= text._subproviders.Count - 1) {
                            ITextChunk chunk;
                            if (text._separator.Length == 0)
                                chunk = new CombinedChunk(chunks);
                            else
                                chunk = new SeparatedChunk(text._separator, chunks);
                            return (chunk, EndPosition.Instance);
                        }
                        subProvider++;
                        position = text._subproviders[subProvider].GetStartPosition();
                    }
                    var (currentChunk, nextPosition) = position.GetText(remainingLength);
                    if (currentChunk.Length == 0) {
                        // we have failed to make progress.
                        return MakeValues(chunks, text, subProvider, position);
                    }
                    // progress, add the chunk to the list
                    chunks.Add(currentChunk);
                    remainingLength -= (currentChunk.Length + text._separator.Length);
                    position = nextPosition;
                    if (!nextPosition.IsAtEnd) {
                        // we need to return to the current chunk again
                        return MakeValues(chunks, text, subProvider, nextPosition);
                    }
                } while(true);
            }

            private static (ITextChunk, ITextPosition) MakeValues(List<ITextChunk> chunks, SeparatedTextProvider text, int subProvider, ITextPosition position) {
                ITextChunk chunk;
                if (text._separator.Length == 0)
                    chunk = new CombinedChunk(chunks);
                else
                    chunk = new SeparatedChunk(text._separator, chunks);

                if (subProvider >= text._subproviders.Count - 1)
                    return (chunk, position);
                return (chunk, new SeparatedPosition(text, subProvider, position));
            }
        }
    }
}