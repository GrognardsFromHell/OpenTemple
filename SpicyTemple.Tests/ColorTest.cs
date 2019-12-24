using FluentAssertions;
using OpenTemple.Core.GFX;
using Xunit;

namespace OpenTemple.Tests
{
    public class ColorTest
    {

        [Fact]
        public void PackRoundtrip()
        {
            var c = new PackedLinearColorA(0xFF_FE_FD_FC);
            c.A.Should().Be(255);
            c.R.Should().Be(254);
            c.G.Should().Be(253);
            c.B.Should().Be(252);

            c.Pack().Should().Be(0xFF_FE_FD_FC);
        }

    }
}