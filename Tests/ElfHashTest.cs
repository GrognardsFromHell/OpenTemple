using OpenTemple.Core.Utils;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class ElfHashTest
{
    [Test]
    public void TestParticleSystemName()
    {
        Assert.AreEqual(2908521, ElfHash.Hash("MM-ChainFlyBy".ToLowerInvariant()));
        Assert.AreEqual(2908521, ElfHash.Hash("MM-ChainFlyBy"));
    }
}