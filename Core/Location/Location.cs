using System;
using System.Numerics;

namespace SpicyTemple.Core.Location
{

    public struct locXY {

        public const float INCH_PER_TILE = 28.284271247461900976033774484194f;
        public const float INCH_PER_HALFTILE = (INCH_PER_TILE / 2.0f);

        public int locx;
        public int locy;

        public locXY(int locx, int locy)
        {
            this.locx = locx;
            this.locy = locy;
        }

        public static locXY fromField(ulong location) {
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
};


    public struct LocAndOffsets
    {
        public locXY location;
        public float off_x;
        public float off_y;

        public LocAndOffsets(int locx, int locy, float offX, float offY)
        {
            this.location = new locXY(locx, locy);
            off_x = offX;
            off_y = offY;
        }

        public LocAndOffsets(locXY location, float offX, float offY)
        {
            this.location = location;
            off_x = offX;
            off_y = offY;
        }

        public Vector2 ToInches2D() {
            return location.ToInches2D(off_x, off_y);
        }

        private static void NormalizeAxis(ref float offset, ref int tilePos) {
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

        public Vector3 ToInches3D(float offsetZ = 0) {
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

        public static LocAndOffsets FromInches(Vector2 pos) {
            return FromInches(pos.X, pos.Y);
        }

        public static LocAndOffsets FromInches(Vector3 pos) {
            return FromInches(pos.X, pos.Z);
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
    }
}