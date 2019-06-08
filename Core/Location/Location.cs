using System;
using System.Numerics;

namespace SpicyTemple.Core.Location
{
    public enum CompassDirection : byte
    {
        Top = 0,
        TopRight = 1,
        Right = 2,
        BottomRight = 3,
        Bottom = 4,
        BottomLeft = 5,
        Left = 6,
        TopLeft = 7,
        DirectionsNum = 8
// 8
    }

    public struct locXY
    {
        public static locXY Zero = new locXY(0, 0);

        public const float INCH_PER_TILE = 28.284271247461900976033774484194f; // SQRT(800)

        public const float INCH_PER_HALFTILE = (INCH_PER_TILE / 2.0f);

        // This is more related to sectoring than location
        public const float INCH_PER_SUBTILE = (INCH_PER_TILE / 3.0f);
        public const int INCH_PER_FEET = 12;

        public int locx;
        public int locy;

        public locXY(int locx, int locy)
        {
            this.locx = locx;
            this.locy = locy;
        }

        public static locXY fromField(ulong location)
        {
            Span<byte> raw = stackalloc byte[sizeof(ulong)];
            BitConverter.TryWriteBytes(raw, location);
            int locx = BitConverter.ToInt32(raw.Slice(0, sizeof(int)));
            int locy = BitConverter.ToInt32(raw.Slice(sizeof(int), sizeof(int)));
            return new locXY(locx, locy);
        }

        public ulong ToField()
        {
            // TODO: Test conversion with bit shifting
            Span<byte> raw = stackalloc byte[sizeof(ulong)];
            BitConverter.TryWriteBytes(raw.Slice(0, sizeof(uint)), locx);
            BitConverter.TryWriteBytes(raw.Slice(sizeof(uint), sizeof(uint)), locy);
            return BitConverter.ToUInt64(raw);
        }

        public static implicit operator ulong(locXY loc) => loc.ToField();

        public Vector2 ToInches2D(float offsetX = 0, float offsetY = 0)
        {
            return new Vector2(
                locx * INCH_PER_TILE + offsetX + INCH_PER_HALFTILE,
                locy * INCH_PER_TILE + offsetY + INCH_PER_HALFTILE
            );
        }

        public Vector3 ToInches3D(float offsetX = 0, float offsetY = 0, float offsetZ = 0)
        {
            return new Vector3(
                locx * INCH_PER_TILE + offsetX + INCH_PER_HALFTILE,
                offsetZ,
                locy * INCH_PER_TILE + offsetY + INCH_PER_HALFTILE
            );
        }

        public bool Equals(locXY other)
        {
            return locx == other.locx && locy == other.locy;
        }

        public override bool Equals(object obj)
        {
            return obj is locXY other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (locx * 397) ^ locy;
            }
        }

        public static bool operator ==(locXY left, locXY right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(locXY left, locXY right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => $"X:{locx},Y:{locy}";

        [TempleDllLocation(0x100299d0)]
        public CompassDirection GetCompassDirection(locXY to)
        {
            var deltaX = to.locx - locx;
            var deltaY = to.locy - locy;

            if (deltaY < 0)
            {
                if (deltaX < 0)
                    return CompassDirection.Top;
                else if (deltaX > 0)
                    return CompassDirection.Left;
                else // deltaX == 0
                    return CompassDirection.TopLeft;
            }
            else if (deltaY > 0)
            {
                if (deltaX > 0)
                    return CompassDirection.Bottom;
                else if (deltaX < 0)
                    return CompassDirection.Right;
                else // deltaX == 0
                    return CompassDirection.BottomRight;
            }
            else // deltaY == 0
            {
                if (deltaX < 0)
                    return CompassDirection.TopRight;
                else // deltaX > 0
                    return CompassDirection.BottomLeft;
            }
        }

        [TempleDllLocation(0x1002a030)]
        public int EstimateDistance(locXY other)
        {
            return Math.Max(Math.Abs(locx - other.locx), Math.Abs(locy - other.locy));
        }

        public locXY Offset(CompassDirection direction)
        {
            var x = locx;
            var y = locy;

            switch (direction)
            {
                case CompassDirection.Top:
                    x--;
                    y--;
                    break;
                case CompassDirection.TopRight:
                    x--;
                    break;
                case CompassDirection.Right:
                    x--;
                    y++;
                    break;
                case CompassDirection.BottomRight:
                    y++;
                    break;
                case CompassDirection.Bottom:
                    x++;
                    y++;
                    break;
                case CompassDirection.BottomLeft:
                    x++;
                    break;
                case CompassDirection.Left:
                    x++;
                    y--;
                    break;
                case CompassDirection.TopLeft:
                    y--;
                    break;
            }

            return new locXY(x, y);
        }
    }

    public struct LocAndOffsets
    {
        public static LocAndOffsets Zero => new LocAndOffsets(0, 0, 0, 0);

        public locXY location;
        public float off_x;
        public float off_y;

        public LocAndOffsets(int locx, int locy, float offX, float offY)
        {
            this.location = new locXY(locx, locy);
            off_x = offX;
            off_y = offY;
        }

        public LocAndOffsets(locXY location, float offX = 0, float offY = 0)
        {
            this.location = location;
            off_x = offX;
            off_y = offY;
        }

        public Vector2 ToInches2D()
        {
            return location.ToInches2D(off_x, off_y);
        }

        private static void NormalizeAxis(ref float offset, ref int tilePos)
        {
            var tiles = (int) (offset / locXY.INCH_PER_TILE);
            if (tiles != 0)
            {
                tilePos += tiles;
                offset -= tiles * locXY.INCH_PER_TILE;
            }
        }

        public void Normalize()
        {
            NormalizeAxis(ref off_x, ref location.locx);
            NormalizeAxis(ref off_y, ref location.locy);
        }

        public Vector3 ToInches3D(float offsetZ = 0)
        {
            return location.ToInches3D(off_x, off_y, offsetZ);
        }

        public static LocAndOffsets FromInches(float x, float y)
        {
            float tileX = x / locXY.INCH_PER_TILE;
            float tileY = y / locXY.INCH_PER_TILE;

            LocAndOffsets result;
            result.location.locx = (int) tileX;
            result.location.locy = (int) tileY;
            result.off_x = (tileX - MathF.Floor(tileX)) * locXY.INCH_PER_TILE - locXY.INCH_PER_HALFTILE;
            result.off_y = (tileY - MathF.Floor(tileY)) * locXY.INCH_PER_TILE - locXY.INCH_PER_HALFTILE;
            return result;
        }

        public static LocAndOffsets FromInches(Vector2 pos)
        {
            return FromInches(pos.X, pos.Y);
        }

        public static LocAndOffsets FromInches(Vector3 pos)
        {
            return FromInches(pos.X, pos.Z);
        }

        /**
         * Distance between this location and the other location in inches.
         */
        [TempleDllLocation(0x1002A0A0)]
        public float DistanceTo(LocAndOffsets locB)
        {
            return Vector2.Distance(location.ToInches2D(), locB.ToInches2D());
        }

        // ensures the floating point offset corresponds to less than half a tile
        public void Regularize()
        {
            // TODO This seems to be the same thing as Normalize()
            if (MathF.Abs(off_x) > 14.142136f)
            {
                while (off_x >= 14.142136f)
                {
                    off_x -= 28.284271f;
                    location.locx++;
                }


                while (off_x < -14.142136f)
                {
                    off_x += 28.284271f;
                    location.locx--;
                }
            }

            if (MathF.Abs(off_y) > 14.142136f)
            {
                while (off_y >= 14.142136f)
                {
                    off_y -= 28.284271f;
                    location.locy++;
                }


                while (off_y < -14.142136f)
                {
                    off_y += 28.284271f;
                    location.locy--;
                }
            }
        }

        public bool Equals(LocAndOffsets other)
        {
            return location.Equals(other.location) && off_x.Equals(other.off_x) && off_y.Equals(other.off_y);
        }

        public override bool Equals(object obj)
        {
            return obj is LocAndOffsets other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = location.GetHashCode();
                hashCode = (hashCode * 397) ^ off_x.GetHashCode();
                hashCode = (hashCode * 397) ^ off_y.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(LocAndOffsets left, LocAndOffsets right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocAndOffsets left, LocAndOffsets right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{location},X_Off:{off_x},Y_Off:{off_y}";
        }
    }
}