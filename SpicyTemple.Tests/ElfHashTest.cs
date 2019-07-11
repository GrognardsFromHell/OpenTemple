using SpicyTemple.Core.Utils;
using Xunit;

namespace SpicyTemple.Tests
{
    public class ElfHashTest
    {
        [Fact]
        public void TestParticleSystemName()
        {
            Assert.Equal(2908521, ElfHash.Hash("MM-ChainFlyBy".ToLowerInvariant()));
            Assert.Equal(2908521, ElfHash.Hash("MM-ChainFlyBy"));
        }
    }
}