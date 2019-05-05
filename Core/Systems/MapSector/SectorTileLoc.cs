namespace SpicyTemple.Core.Systems.MapSector
{
    /// <summary>
    /// Addresses a single tile within a sector.
    /// </summary>
    public readonly struct SectorTileLoc
    {
        public int X { get; }
        public int Y { get; }

        public SectorTileLoc(int x, int y)
        {
            X = x;
            Y = y;
        }

        [TempleDllLocation(0x100AB7F0)]
        public static SectorTileLoc FromTile(int tileX, int tileY)
        {
            return new SectorTileLoc(
                tileX % Sector.SectorSideSize,
                tileY % Sector.SectorSideSize
            );
        }

        public bool Equals(SectorTileLoc other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is SectorTileLoc other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(SectorTileLoc left, SectorTileLoc right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectorTileLoc left, SectorTileLoc right)
        {
            return !left.Equals(right);
        }
    }
}