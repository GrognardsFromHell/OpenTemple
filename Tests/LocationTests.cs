using System;
using FluentAssertions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Utils;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class LocationTests
{

    private static locXY Center => new locXY(480,480);

    [Test]
    public void ConversionRoundtrip()
    {
        var zeroTile = LocAndOffsets.FromInches(0, 0);
        zeroTile.location.locx.Should().Be(0);
        zeroTile.location.locy.Should().Be(0);
        zeroTile.off_x.Should().Be(-locXY.INCH_PER_HALFTILE);
        zeroTile.off_y.Should().Be(-locXY.INCH_PER_HALFTILE);

        var conversionBack = zeroTile.ToInches2D();
        conversionBack.X.Should().Be(0);
        conversionBack.Y.Should().Be(0);
    }

    private static void AssertAngles(float expectedDegrees, float actualRadians)
    {
        var expectedRadians = Angles.ToRadians(expectedDegrees);
        var delta = Angles.ShortestAngleBetween(expectedRadians, actualRadians);
        if (MathF.Abs(delta) < Angles.OneDegreeInRadians)
        {
            return; // Everything okay!
        }

        Assert.AreEqual(expectedRadians, actualRadians);
    }

    [Test]
    public void RotationTowardsUpIsZero()
    {
        var top = new LocAndOffsets(Center.Offset(CompassDirection.Top));
        var rot = new LocAndOffsets(Center).RotationTo(top);
        AssertAngles(0.0f, rot);
    }

    [Test]
    public void RotationTowardsRightIs90Degrees()
    {
        var right = new LocAndOffsets(Center.Offset(CompassDirection.Right));
        var rot = new LocAndOffsets(Center).RotationTo(right);
        AssertAngles(90, rot);
    }

    [Test]
    public void RotationTowardsBottomIs180Degrees()
    {
        var bottom = new LocAndOffsets(Center.Offset(CompassDirection.Bottom));
        var rot = new LocAndOffsets(Center).RotationTo(bottom);
        AssertAngles(180, rot);
    }

    [Test]
    public void RotationTowardsLeftIs270Degrees()
    {
        var left = new LocAndOffsets(Center.Offset(CompassDirection.Left));
        var rot = new LocAndOffsets(Center).RotationTo(left);
        AssertAngles(270, rot);
    }

}