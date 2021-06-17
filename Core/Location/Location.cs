using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Location
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
    }

    public static class CompassDirectionExtensions
    {
        public static CompassDirection GetOpposite(this CompassDirection direction)
        {
            return direction.Rotate(4);
        }

        public static bool IsCardinalDirection(this CompassDirection direction)
        {
            return direction == CompassDirection.Top
                   || direction == CompassDirection.Right
                   || direction == CompassDirection.Bottom
                   || direction == CompassDirection.Left;
        }

        public static CompassDirection GetLeft(this CompassDirection direction)
        {
            return direction.Rotate(7);
        }

        public static CompassDirection Rotate(this CompassDirection direction, int ticksToRight)
        {
            return (CompassDirection) (((int) direction + ticksToRight) % 8);
        }

        public static CompassDirection GetRight(this CompassDirection direction)
        {
            return direction.Rotate(1);
        }
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

        public static locXY FromInches(Vector2 worldPosInInches)
        {
            var tileX = (int) (worldPosInInches.X / INCH_PER_TILE);
            var tileY = (int) (worldPosInInches.Y / INCH_PER_TILE);

            return new locXY(tileX, tileY);
        }

        [Pure]
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

        [TempleDllLocation(0x10029CE0)]
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

        public void Deconstruct(out int x, out int y)
        {
            x = locx;
            y = locy;
        }

        public locXY OffsetTiles(int x, int y)
        {
            return new locXY(locx + x, locy + y);
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

        [TempleDllLocation(0x10028f50)]
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

        [Pure]
        public Vector3 ToInches3D(float offsetZ = 0)
        {
            return location.ToInches3D(off_x, off_y, offsetZ);
        }

        public static LocAndOffsets FromInches(float x, float y)
        {
            var tileX = x / locXY.INCH_PER_TILE;
            var tileY = y / locXY.INCH_PER_TILE;

            LocAndOffsets result;
            result.location.locx = (int) tileX;
            result.location.locy = (int) tileY;
            result.off_x = (tileX - result.location.locx - 0.5f) * locXY.INCH_PER_TILE;
            result.off_y = (tileY - result.location.locy - 0.5f) * locXY.INCH_PER_TILE;
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
        [TempleDllLocation(0x10040010)] /* The pointer based version */
        public float DistanceTo(LocAndOffsets locB)
        {
            return Vector2.Distance(ToInches2D(), locB.ToInches2D());
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

        public LocAndOffsets OffsetSubtile(CompassDirection direction)
        {
            var result = ToInches2D();

            switch (direction)
            {
                case CompassDirection.Top:
                    result.X -= locXY.INCH_PER_SUBTILE;
                    result.Y -= locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.TopRight:
                    result.X -= locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.Right:
                    result.X -= locXY.INCH_PER_SUBTILE;
                    result.Y += locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.BottomRight:
                    result.Y += locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.Bottom:
                    result.X += locXY.INCH_PER_SUBTILE;
                    result.Y += locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.BottomLeft:
                    result.X += locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.Left:
                    result.X += locXY.INCH_PER_SUBTILE;
                    result.Y -= locXY.INCH_PER_SUBTILE;
                    break;
                case CompassDirection.TopLeft:
                    result.Y -= locXY.INCH_PER_SUBTILE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return LocAndOffsets.FromInches(result);
        }

        // This is the epsilon used in vanilla (although they used a double here, pointlessly)
        private const float AlmostEqualsEpsilon = 0.0000001192092895507812f;

        [TempleDllLocation(0x1003ffc0)]
        public bool AlmostEquals(LocAndOffsets target)
        {
            // It has to be the same tile
            if (target.location != location)
            {
                return false;
            }

            return MathF.Abs(off_x - target.off_x) <= AlmostEqualsEpsilon
                   && MathF.Abs(off_y - target.off_y) <= AlmostEqualsEpsilon;
        }

        public LocAndOffsets OffsetFeet(float angleRad, float distanceFeet)
        {
            var vec = ToInches2D();
            var vectorAngleRad = 5 * MathF.PI / 4 - angleRad;
            vec.X += distanceFeet * locXY.INCH_PER_FEET * MathF.Cos(vectorAngleRad);
            vec.Y += distanceFeet * locXY.INCH_PER_FEET * MathF.Sin(vectorAngleRad);
            return FromInches(vec);
        }
    }

    public readonly struct Subtile // every tile is subdivided into 3x3 subtiles
    {
        public readonly int X;
        public readonly int Y;

        public Subtile(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        [TempleDllLocation(0x10040750)]
        public Subtile(LocAndOffsets location)
        {
            // The tile address is converted into the center subtile
            X = 3 * location.location.locx + 1;
            Y = 3 * location.location.locy + 1;

            if (location.off_x > HalfSubtile)
            {
                X++;
            }
            else if (location.off_x < - HalfSubtile)
            {
                X--;
            }

            if (location.off_y > HalfSubtile)
            {
                Y++;
            }
            else if (location.off_y < -HalfSubtile)
            {
                Y--;
            }
        }

        private const float HalfSubtile = locXY.INCH_PER_SUBTILE / 2;

        [TempleDllLocation(0x100400c0)]
        public LocAndOffsets ToLocAndOffset()
        {
            return new LocAndOffsets(
                X / 3,
                Y / 3,
                ((X % 3) - 1) * locXY.INCH_PER_SUBTILE,
                ((Y % 3) - 1) * locXY.INCH_PER_SUBTILE
            );
        }

        [TempleDllLocation(0x10029DC0)]
        public bool OffsetByOne(CompassDirection direction, out Subtile newTile)
        {
            var newX = X;
            var newY = Y;

            switch ( direction )
            {
                case CompassDirection.Top:
                    newX--;
                    newY--;
                    break;
                case CompassDirection.TopRight:
                    newX--;
                    break;
                case CompassDirection.Right:
                    newX--;
                    newY++;
                    break;
                case CompassDirection.BottomRight:
                    newY++;
                    break;
                case CompassDirection.Bottom:
                    newX++;
                    newY++;
                    break;
                case CompassDirection.BottomLeft:
                    newX++;
                    break;
                case CompassDirection.Left:
                    newX++;
                    newY--;
                    break;
                case CompassDirection.TopLeft:
                    newY--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if ( newX < 0 || newX >= GameSystems.Location.LocationLimitX * 3
                || newY < 0 || newY >= GameSystems.Location.LocationLimitY * 3 )
            {
                newTile = default;
                return false;
            }
            else
            {
                newTile = new Subtile(newX, newY);
                return true;
            }
        }

        public bool Equals(Subtile other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Subtile other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(Subtile left, Subtile right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Subtile left, Subtile right)
        {
            return !left.Equals(right);
        }
    }

}