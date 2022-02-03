using System;
using FluentAssertions;
using OpenTemple.Core.Utils;
using NUnit.Framework;

namespace OpenTemple.Tests.Utils;

public class AnglesTest
{
    private static int ShortestAngleDegrees(int from, int to)
    {
        var fromRadians = Angles.ToRadians(from);
        var toRadians = Angles.ToRadians(to);
        return (int) Math.Round(Angles.ToDegrees(Angles.ShortestAngleBetween(fromRadians, toRadians)));
    }

    [Test]
    public void TestShortestAngleBetweenSameAngles()
    {
        ShortestAngleDegrees(0, 0).Should().Be(0);
        ShortestAngleDegrees(0, 360).Should().Be(0);
        ShortestAngleDegrees(360, 0).Should().Be(0);
    }

    [Test]
    public void TestShortestAngleLeftDirection()
    {
        ShortestAngleDegrees(0, 270).Should().Be(-90);
        ShortestAngleDegrees(90, 0).Should().Be(-90);
    }

    [Test]
    public void TestShortestAngleRightDirection()
    {
        ShortestAngleDegrees(0, 90).Should().Be(90);
        ShortestAngleDegrees(270, 0).Should().Be(90);
    }

    [Test]
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