using OpenTemple.Core.Utils;
using Xunit;

namespace OpenTemple.Tests
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