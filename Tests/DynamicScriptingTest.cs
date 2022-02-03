using System;
using FluentAssertions;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class DynamicScriptingTest
{
    private readonly DynamicScripting.DynamicScripting devScripting = new DynamicScripting.DynamicScripting();

    [Test]
    public void TestCompletion()
    {
        devScripting.Complete("GameSystems.Par").Should().Be("GameSystems.ParticleSys");
        devScripting.Complete("PartyL").Should().Be("PartyLeader");
    }
}