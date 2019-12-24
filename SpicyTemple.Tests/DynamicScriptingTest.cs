using System;
using FluentAssertions;
using OpenTemple.Core.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace OpenTemple.Tests
{
    public class DynamicScriptingTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly DynamicScripting.DynamicScripting devScripting = new DynamicScripting.DynamicScripting();

        public DynamicScriptingTest(ITestOutputHelper testOutputHelper)
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