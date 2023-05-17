using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSplitter {
    public static class TextBuilder {

        public static string LineSeparator = System.Environment.NewLine;

        /// <summary>
        /// Produces an empty text chunk.
        /// </summary>
        /// <returns></returns>
        public static ITextProvider Empty() => NoText.Instance;

        /// <summary>
        /// Wraps the given text provider with a string. This is useful when you want to e.g. split
        /// code in markdown and need to ensure that each section is properly wrapped.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ITextProvider Wrap(string wrapper, ITextProvider provider) =>
            new WrappedTextProvider(provider, wrapper, wrapper);


        public static ITextProvider Wrap(string wrapStart, string wrapEnd, ITextProvider provider) =>
            new WrappedTextProvider(provider, wrapStart, wrapEnd);

        /// <summary>
        /// Glues the second provider to the first. This means that the there will be no split right
        /// between them, so the first has to end in the same section that the second starts in.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static ITextProvider Glue(ITextProvider first, ITextProvider second) =>
            new GlueTextProvider(first, second);

        /// <summary>
        /// Returns a provider that emits the given string and does not allow any splitting.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ITextProvider Atomic(string text) => new StringProvider(text);

        /// <summary>
        /// Returns a provider that prevents any splitting in the given provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ITextProvider Atomic(ITextProvider provider) => new AtomicTextProvider(provider);

        public static ITextProvider Atomic(IEnumerable<ITextProvider> providers) => new AtomicTextProvider(providers);
        public static ITextProvider Atomic(params ITextProvider[] providers) => new AtomicTextProvider(providers);

        /// <summary>
        /// Returns a provider that emits all of the given strings without any splitting inbetween.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static ITextProvider Atomic(params string[] strings) => new AtomicStringsProvider(strings);

        /// <summary>
        /// Combines the given providers by adding optional line-breaks between them. The line-breaks will be used
        /// to separate two providers, but not appear at the start or end of a section.
        /// </summary>
        /// <param name="providers"></param>
        /// <returns></returns>
        public static ITextProvider Lines(IEnumerable<ITextProvider> providers) => new SeparatedTextProvider(LineSeparator, providers);  

        /// <summary>
        /// Combindes the given providers by adding optional line-breaks between them. The line-breaks will be used
        /// to separate two providers, but not appear at the start or end of a section.
        /// </summary>
        /// <param name="providers"></param>
        /// <returns></returns>
        public static ITextProvider Lines(params ITextProvider[] providers) => new SeparatedTextProvider(LineSeparator, providers);

        /// <summary>
        /// Returns a provider that only returns the text returned from the first call to the underlying provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ITextProvider Optional(ITextProvider provider) => new OptionalTextProvider(provider);

        /// <summary>
        /// Combines the given text providers and allows splitting between them.
        /// </summary>
        /// <param name="providers"></param>
        /// <returns></returns>
        public static ITextProvider Separated(IEnumerable<ITextProvider> providers) => new SeparatedTextProvider(providers);

        public static ITextProvider Separated(params ITextProvider[] providers) => new SeparatedTextProvider(providers);

        /// <summary>
        /// Combines the given text providers and allows splitting between them. It also inserts optional separators between
        /// the sections, similar to how Lines works.
        /// </summary>
        /// <param name="providers"></param>
        /// <returns></returns>
        public static ITextProvider Separated(string separator, IEnumerable<ITextProvider> providers) => new SeparatedTextProvider(separator, providers);

        public static ITextProvider Separated(string separator, params ITextProvider[] providers) => new SeparatedTextProvider(separator, providers);

        /// <summary>
        /// Interprets the given string as common text and allows splitting at whitespace.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ITextProvider Text(string text) => new SplitStringProvider(text, char.IsWhiteSpace);
        public static ITextProvider SplitAt(Predicate<char> splittingPredicate, string text) => new SplitStringProvider(text, splittingPredicate);

        /// <summary>
        /// Allows to split the given string anywhere.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ITextProvider Blob(string text) => new BlobProvider(text);
    }
}