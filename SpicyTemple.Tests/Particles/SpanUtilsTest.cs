using System;
using System.Text;
using SpicyTemple.Core.Particles.Parser;
using Xunit;

namespace SpicyTemple.Tests
{
    public class SpanUtilsTest
    {
        private const byte SEP = (byte) ',';

        [Fact]
        public void SplitEmptyText()
        {
            using var parts = SpanUtils.SplitList(ReadOnlySpan<byte>.Empty, SEP, out var count);
            Assert.Equal(1, count);
            Assert.Equal(parts.Memory.Span[0], ..0);
        }

        [Fact]
        public void SplitWhitespace()
        {
            using var parts = SpanUtils.SplitList(Encoding.ASCII.GetBytes("   "), SEP, out var count);
            Assert.Equal(1, count);
            Assert.Equal(..0, parts.Memory.Span[0]);
        }

        [Fact]
        public void SplitEmptyTokens()
        {
            using var parts = SpanUtils.SplitList(Encoding.ASCII.GetBytes("  , "), SEP, out var count);
            Assert.Equal(2, count);
            Assert.Equal(..0, parts.Memory.Span[0]);
            Assert.Equal(3..3, parts.Memory.Span[1]);
        }

        [Fact]
        public void SplitFullExample()
        {
            using var parts = SpanUtils.SplitList(Encoding.ASCII.GetBytes("1,2,,,4"), SEP, out var count);
            Assert.Equal(5, count);
            Assert.Equal(0..1, parts.Memory.Span[0]);
            Assert.Equal(2..3, parts.Memory.Span[1]);
            Assert.Equal(4..4, parts.Memory.Span[2]);
            Assert.Equal(5..5, parts.Memory.Span[3]);
            Assert.Equal(6..7, parts.Memory.Span[4]);
        }
    }
}