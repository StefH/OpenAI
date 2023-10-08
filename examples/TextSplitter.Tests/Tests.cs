using System.Linq;
using NUnit.Framework;
using TextSplitter;
using static TextSplitter.TextBuilder;

namespace TextSplitter.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void AtomicTest() {
            const string src = "a c";
            var text = Atomic(src);
            Assert.AreEqual(string.Empty, text.ToText(2));
            Assert.AreEqual(src, text.ToText(3));
            Assert.AreEqual(src, text.ToText(4));
        }

        [Test]
        public void AtomicMultipleTest() {
            var text = Atomic(Atomic("a"), Atomic("b"), Atomic("c"));
            Assert.AreEqual(string.Empty, text.ToText(2));
            Assert.AreEqual("abc", text.ToText(3));
            Assert.AreEqual("abc", text.ToText(4));
        }

        [Test]
        public void AtomicStringsTest() {
            var text = Atomic("a", "b", "c");
            Assert.AreEqual(string.Empty, text.ToText(2));
            Assert.AreEqual("abc", text.ToText(3));
            Assert.AreEqual("abc", text.ToText(4));
        }

        [Test]
        public void AtomicWrapperTest() {
            const string src = "Splits at whitespace.";
            var text = Text(src);
            Assert.AreEqual("Splits", text.ToText(6));
            Assert.AreEqual("Splits", text.ToText(8));
            Assert.AreEqual("Splits at", text.ToText(9));
            Assert.AreEqual(src, text.ToText(src.Length));

            var atomicText = Atomic(text);
            Assert.AreEqual("", atomicText.ToText(6));
            Assert.AreEqual("", atomicText.ToText(8));
            Assert.AreEqual("", atomicText.ToText(9));
            Assert.AreEqual(src, atomicText.ToText(src.Length));
        }

        [Test]
        public void EmptyTest() {
            Assert.AreEqual("", Empty().ToText(1000));
            Assert.AreEqual("", Empty().ToText(0));
        }

        [Test]
        public void WrapTest() {
            const string src = "1 2 3 4";
            var text = Wrap("<<", ">>", Text(src));
            Assert.AreEqual("", text.ToText(4));
            Assert.AreEqual("<<1>>", text.ToText(5));
            Assert.AreEqual("<<1>>", text.ToText(6));
            Assert.AreEqual("<<1 2>>", text.ToText(7));
            CollectionAssert.AreEqual(
                new[] {"<<1>>", "<<2>>", "<<3>>", "<<4>>"},
                text.GetSections(5)
            );
            CollectionAssert.AreEqual(
                new[] {"<<1 2>>", "<<3 4>>"},
                text.GetSections(7)
            );
            // we cannot split the text in blocks of size 0.
            Assert.Throws(
                typeof(TextSplitterException),
                () => text.GetChunks(0).ToList()
            );
        }

        [Test]
        public void SeparatedTest() {
            var text = Separated("\n", Atomic("abc"), Atomic("d"), Atomic("ef"));
            CollectionAssert.AreEqual(
                new[] {"abc", "d\nef"},
                text.GetSections(4)
            );
        }

        [Test]
        public void GlueTest() {
            var text = Glue(Atomic("ABC"), Atomic("D"));
            Assert.AreEqual("", text.ToText(3));
            Assert.AreEqual("ABCD", text.ToText(4));
        }

        [Test]
        public void BlobTest() {
            var text = Blob("abcdefgh");
            Assert.AreEqual("abcd", text.ToText(4));
            Assert.AreEqual("abcdefgh", text.ToText(8));
            CollectionAssert.AreEqual(
                new[]{ "abcd", "efgh" },
                text.GetSections(4)
            );
        }

        [Test]
        public void OptionalTest() {
            var text = Optional(Blob("abcd"));
            Assert.AreEqual("abcd", text.ToText(4));
            CollectionAssert.AreEqual(
                new[]{ "ab" },
                text.GetSections(2)
            );

            var optionalText = Optional(Atomic("ABC"));
            // the text is optional, so we can ignore it
            Assert.DoesNotThrow(() => optionalText.GetChunks(0).ToList());
        }

        [Test]
        public void TextTest()
        {
            var text = Text("\n This is a\n test. ");

            CollectionAssert.AreEqual(
                new[] {"This", "is a", "test."},
                text.GetSections(6)
            );
        }
    }
}