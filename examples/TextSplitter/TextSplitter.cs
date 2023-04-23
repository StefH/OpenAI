using System;
using System.Collections.Generic;

namespace TextSplitter
{
    public static class TextSplitter {
        /// <summary>
        /// Tries to take the given number of characters from the text with the constraint that the splitting point
        /// must fulfill the predicate. Think of the predicate of meaning 'is space'. Then this function will split
        /// such that the last letter taken is non-whitespace. The returned object also indicates at what point 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitPredicate"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static TextSplit TakeUntil(ReadOnlySpan<char> text, Predicate<char> splitPredicate, int length) {
            // skip all whitespace in the beginning
            int lower = 0;
            while (lower < text.Length && splitPredicate(text[lower]))
                lower++;

            if (length > text.Length - lower)
                length = text.Length - lower;

            int upper = lower + length;
            // if there is a next letter that is non-whitespace, go back until
            // we find some whitespace to split in.
            if (upper < text.Length && !splitPredicate(text[upper])) {
                while (upper > 0 && !splitPredicate(text[upper - 1]))
                    upper--;
            }
            int next = upper;
            // navigate to the start of the current block of whitespace and break there
            while (upper > 0 && splitPredicate(text[upper - 1]))
                upper--;
            
            return new TextSplit(lower, upper - lower, next);
        }

        public struct TextSplit {
            public readonly int Start, Length, Next;
            public TextSplit(int start, int length, int next) {
                Start = start;
                Length = length;
                Next = next;
            }
        }
    }
}