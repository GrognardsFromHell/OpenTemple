using System;
using FluentAssertions;
using SpicyTemple.Core.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace SpicyTemple.Tests
{
    public class DevScriptingTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly DevScripting.DevScripting devScripting = new DevScripting.DevScripting();

        public DevScriptingTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestCompletion()
        {
            devScripting.Complete("GameSystems.Par").Should().Be("GameSystems.ParticleSys");
            devScripting.Complete("PartyL").Should().Be("PartyLeader");
        }
    }
}