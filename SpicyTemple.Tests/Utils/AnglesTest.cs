using System;
using FluentAssertions;
using SpicyTemple.Core.Utils;
using Xunit;

namespace SpicyTemple.Tests.Utils
{
    public class AnglesTest
    {
        private static int ShortestAngleDegrees(int from, int to)
        {
            var fromRadians = Angles.ToRadians(from);
            var toRadians = Angles.ToRadians(to);
            return (int) Math.Round(Angles.ToDegrees(Angles.ShortestAngleBetween(fromRadians, toRadians)));
        }

        [Fact]
        public void TestShortestAngleBetweenSameAngles()
        {
            ShortestAngleDegrees(0, 0).Should().Be(0);
            ShortestAngleDegrees(0, 360).Should().Be(0);
            ShortestAngleDegrees(360, 0).Should().Be(0);
        }

        [Fact]
        public void TestShortestAngleLeftDirection()
        {
            ShortestAngleDegrees(0, 270).Should().Be(-90);
            ShortestAngleDegrees(90, 0).Should().Be(-90);
        }

        [Fact]
        public void TestShortestAngleRightDirection()
        {
            ShortestAngleDegrees(0, 90).Should().Be(90);
            ShortestAngleDegrees(270, 0).Should().Be(90);
        }

        [Fact]
        public void TestLargeAnglesAreNormalized()
        {
            ShortestAngleDegrees(0, 2 * 360).Should().Be(0);
            ShortestAngleDegrees(2 * 360, 0).Should().Be(0);
            ShortestAngleDegrees(2 * 360, -2 * 360).Should().Be(0);
            ShortestAngleDegrees(2 * 360, 4 * 360 + 90).Should().Be(90);
            ShortestAngleDegrees(4 * 360 + 90, 2 * 360).Should().Be(-90);
            ShortestAngleDegrees(-2 * 360, -4 * 360).Should().Be(0);
            ShortestAngleDegrees(-2 * 360, -2 * 360 + 90).Should().Be(90);
            ShortestAngleDegrees(-2 * 360 + 90, -2 * 360).Should().Be(-90);
        }
    }
}