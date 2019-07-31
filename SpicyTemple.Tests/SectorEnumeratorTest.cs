using System.Drawing;
using FluentAssertions.Common;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.Raycast;
using Xunit;

namespace SpicyTemple.Tests
{
    public class SectorEnumeratorTest
    {
        [Fact]
        public void IterateEmpty()
        {
            SectorEnumerator e = default;
            e.MoveNext().IsSameOrEqualTo(false);
        }

        [Fact]
        public void IterateSingleTileAlignedAtStart()
        {
            var e = new SectorEnumerator(new Rectangle(0, 0, 1, 1), false);
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(0, 0, 1, 1), default));
            e.MoveNext().IsSameOrEqualTo(false);
        }

        [Fact]
        public void IterateSingleTileAlignedAtEnd()
        {
            var e = new SectorEnumerator(new Rectangle(63, 63, 1, 1), false);
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(63, 63, 1, 1), default));
            e.MoveNext().IsSameOrEqualTo(false);
        }

        [Fact]
        public void IterateFourTilesSpanningFourSectors()
        {
            var e = new SectorEnumerator(new Rectangle(63, 63, 2, 2), false);
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 0), false, new Rectangle(63, 63, 1, 1), default));
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(1, 0), false, new Rectangle(0, 63, 1, 1), default));
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(0, 1), false, new Rectangle(63, 63, 1, 1), default));
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(1, 1), false, new Rectangle(63, 63, 1, 1), default));
            e.MoveNext().IsSameOrEqualTo(false);
        }

        [Fact]
        public void IterateOneFullSector()
        {
            var e = new SectorEnumerator(new Rectangle(64, 64, 64, 64), false);
            e.MoveNext().IsSameOrEqualTo(true);
            e.Current.IsSameOrEqualTo(new PartialSector(new SectorLoc(1, 1), true, new Rectangle(0, 0, 64, 64), default));
            e.MoveNext().IsSameOrEqualTo(false);
        }
    }
}