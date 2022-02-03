using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using FluentAssertions.Common;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Raycast;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class SectorEnumeratorTest
{
    [Test]
    public void IterateEmpty()
    {
        SectorEnumerator e = default;
        e.MoveNext().IsSameOrEqualTo(false);
    }

    [Test]
    public void IterateSingleTileAlignedAtStart()
    {
        var e = new SectorEnumerator(new Rectangle(0, 0, 1, 1), false);
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(0, 0, 1, 1),
            default));
        e.MoveNext().IsSameOrEqualTo(false);
    }

    [Test]
    public void IterateSingleTileAlignedAtEnd()
    {
        var e = new SectorEnumerator(new Rectangle(63, 63, 1, 1), false);
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(63, 63, 1, 1),
            default));
        e.MoveNext().IsSameOrEqualTo(false);
    }

    [Test]
    public void IterateSingleTileInMiddle()
    {
        var e = new SectorEnumerator(new Rectangle(10, 10, 1, 1), false);
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(10, 10, 1, 1),
            default));
        e.MoveNext().IsSameOrEqualTo(false);
    }

    [Test]
    public void IterateFourTilesSpanningFourSectors()
    {
        var e = new SectorEnumerator(new Rectangle(63, 63, 2, 2), false);
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(63, 63, 1, 1),
            default));
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(
            new PartialSector(new SectorLoc(1, 0), false, new Rectangle(0, 63, 1, 1), default));
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 1), false, new Rectangle(63, 63, 1, 1),
            default));
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(1, 1), false, new Rectangle(63, 63, 1, 1),
            default));
        e.MoveNext().IsSameOrEqualTo(false);
    }

    [Test]
    public void IterateOneFullSector()
    {
        var e = new SectorEnumerator(new Rectangle(64, 64, 64, 64), false);
        e.MoveNext().IsSameOrEqualTo(true);
        e.Current.IsSameOrEqualTo(
            new PartialSector(new SectorLoc(1, 1), true, new Rectangle(0, 0, 64, 64), default));
        e.MoveNext().IsSameOrEqualTo(false);
    }

    [Test]
    public void IterateThreeByThreeSectorsWithOneTileBorder()
    {
        var e = new SectorEnumerator(new Rectangle(1, 1, 64 * 3 - 2, 64 * 3 - 2), false);
        // First Row of 3x3
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(0, 0));
        e.Current.TileRectangle.Should().Be(new Rectangle(1, 1, 63, 63));
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(1, 0));
        e.Current.TileRectangle.Should().Be(new Rectangle(0, 1, 64, 63));
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(2, 0));
        e.Current.TileRectangle.Should().Be(new Rectangle(0, 1, 63, 63));

        // Second Row of 3x3
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(0, 1));
        e.Current.TileRectangle.Should().Be(new Rectangle(1, 0, 63, 64));
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(1, 1));
        e.Current.TileRectangle.Should().Be(new Rectangle(0, 0, 64, 64));
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(2, 1));
        e.Current.TileRectangle.Should().Be(new Rectangle(0, 0, 63, 64));

        // Third Row of 3x3
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(0, 2));
        e.Current.TileRectangle.Should().Be(new Rectangle(1, 0, 63, 63));
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(1, 2));
        e.Current.TileRectangle.Should().Be(new Rectangle(0, 0, 64, 63));
        e.MoveNext().Should().BeTrue();
        e.Current.SectorLoc.Should().Be(new SectorLoc(2, 2));
        e.Current.TileRectangle.Should().Be(new Rectangle(0, 0, 63, 63));

        e.MoveNext().Should().BeFalse();
    }
}