using System;
using System.Text;
using OpenTemple.Core.Particles.Parser;
using NUnit.Framework;

namespace OpenTemple.Tests.Particles
{
    public class SpanUtilsTest
    {
        private const byte SEP = (byte) ',';

        [Test]
        public void SplitEmptyText()
        {
            using var parts = SpanUtils.SplitList(ReadOnlySpan<byte>.Empty, SEP, out var count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(parts.Memory.Span[0], ..0);
        }

        [Test]
        public void SplitWhitespace()
        {
            using var parts = SpanUtils.SplitList(Encoding.ASCII.GetBytes("   "), SEP, out var count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(..0, parts.Memory.Span[0]);
        }

        [Test]
        public void SplitEmptyTokens()
        {
            using var parts = SpanUtils.SplitList(Encoding.ASCII.GetBytes("  , "), SEP, out var count);
            Assert.AreEqual(2, count);
            Assert.AreEqual(..0, parts.Memory.Span[0]);
            Assert.AreEqual(3..3, parts.Memory.Span[1]);
        }

        [Test]
        public void SplitFullExample()
        {
            using var parts = SpanUtils.SplitList(Encoding.ASCII.GetBytes("1,2,,,4"), SEP, out var count);
            Assert.AreEqual(5, count);
            Assert.AreEqual(0..1, parts.Memory.Span[0]);
            Assert.AreEqual(2..3, parts.Memory.Span[1]);
            Assert.AreEqual(4..4, parts.Memory.Span[2]);
            Assert.AreEqual(5..5, parts.Memory.Span[3]);
            Assert.AreEqual(6..7, parts.Memory.Span[4]);
        }
    }
}