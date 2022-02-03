using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.MapSector;

/// <summary>
/// Addresses a single sector, which encompasses an area of 64x64 tiles.
/// </summary>
public readonly struct SectorLoc
{

    public int X { get; }

    public int Y { get; }

    public SectorLoc(int x, int y)
    {
        X = x;
        Y = y;
    }

    public SectorLoc(locXY loc)
    {
        X = loc.locx / Sector.SectorSideSize;
        Y = loc.locy / Sector.SectorSideSize;
    }

    [TempleDllLocation(0x10081a00)]
    public locXY GetBaseTile() // get the corner tile (lowest x,y in the sector)
    {
        locXY loc;
        loc.locx = X * Sector.SectorSideSize;
        loc.locy = Y * Sector.SectorSideSize;
        return loc;
    }

    /// <summary>
    /// Converts this to the internal legacy representation used by ToEE.
    /// The lower 26-bit are the x coordinate, while the y location is shifted
    /// left by 26-bit.
    /// </summary>
    [TempleDllLocation(0x100819c0)]
    public ulong Pack()
    {
        return ((ulong) Y << 26) | (ulong) (X & 0x3FFFFFF);
    }

    public static SectorLoc Unpack(ulong fieldValue)
    {
        return new SectorLoc(
            (int) (fieldValue & 0x3FFFFFF),
            (int)(fieldValue >> 26)
        );
    }

    public bool Equals(SectorLoc other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is SectorLoc other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public static bool operator ==(SectorLoc left, SectorLoc right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SectorLoc left, SectorLoc right)
    {
        return !left.Equals(right);
    }
}