using System.Drawing;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.Systems.Raycast
{

    public readonly struct PartialSectorObjectEnumerable
    {
        private readonly PartialSectorObjectEnumerator _enumerator;

        public PartialSectorObjectEnumerable(PartialSectorObjectEnumerator enumerator)
        {
            _enumerator = enumerator;
        }

        public PartialSectorObjectEnumerator GetEnumerator() => _enumerator;
    }

    public readonly struct PartialSector
    {
        public readonly SectorLoc SectorLoc;
        public readonly bool FullSector;
        public readonly Rectangle TileRectangle;
        public readonly LockedMapSector Sector;

        public PartialSector(SectorLoc sectorLoc, bool fullSector, Rectangle tileRectangle, LockedMapSector sector)
        {
            SectorLoc = sectorLoc;
            FullSector = fullSector;
            TileRectangle = tileRectangle;
            Sector = sector;
        }

        public PartialSectorObjectEnumerable EnumerateObjects()
        {
            return new PartialSectorObjectEnumerable(
                new PartialSectorObjectEnumerator(Sector.Sector.objects.tiles, TileRectangle)
            );
        }

        public bool Equals(PartialSector other)
        {
            return SectorLoc.Equals(other.SectorLoc) && FullSector == other.FullSector &&
                   other.TileRectangle.Equals(TileRectangle);
        }

        public override bool Equals(object obj)
        {
            return obj is PartialSector other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SectorLoc.GetHashCode();
                hashCode = (hashCode * 397) ^ FullSector.GetHashCode();
                hashCode = (hashCode * 397) ^ TileRectangle.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PartialSector left, PartialSector right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PartialSector left, PartialSector right)
        {
            return !left.Equals(right);
        }
    }
}