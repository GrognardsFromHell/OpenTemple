using System;
using FluentAssertions;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO.SaveGames.Co8State;
using Xunit;

namespace OpenTemple.Tests
{
    public class PythonValueParserTest
    {
        [Fact]
        public void TestParseActiveTargetListEntry()
        {
            var serializedValue = "[[17, (2, (965518452, 33924, 17687, (181, 170, 229, 54, 235, 222, 28, 219)))]]";

            ReadOnlySpan<char> valueSpan = serializedValue;
            var value = PythonValueParser.ParseValue(valueSpan);
            value.Should().BeEquivalentTo(new object[]
            {
                new object[]
                {
                    17,
                    (2, (965518452, 33924, 17687, (181, 170, 229, 54, 235, 222, 28, 219)))
                }
            });

            var (spellId, targets) = PythonValueParser.ParseActiveTargetListEntry(((object[]) value)[0]);
            spellId.Should().Be(17);
            targets.Should().Be(ObjectId.CreatePermanent(Guid.ParseExact("398CA474-8484-4517-B5AA-E536EBDE1CDB", "D")));
        }
    }
}