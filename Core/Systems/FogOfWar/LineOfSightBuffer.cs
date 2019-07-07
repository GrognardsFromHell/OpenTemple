using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using SharpDX.WIC;
using SpicyTemple.Core.IO.Images;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    /// <summary>
    /// This buffer stores the line of sight information around a single party member.
    /// </summary>
    internal class LineOfSightBuffer
    {
        private const float HalfSubtile = locXY.INCH_PER_SUBTILE / 2;

        // Maximum line of sight
        private const int Radius = 102;

        public const int Dimension = Radius * 2;

        [TempleDllLocation(0x1080FA88)]
        public locXY CenterTile { get; private set; }

        [TempleDllLocation(0x108203C8)] [TempleDllLocation(0x108EC4D0)]
        private Point _centerSubtile;

        // Specifies the X-component of the tile that is the origin of a party members line of sight buffer
        // Specifies the Y-component of the tile that is the origin of a party members line of sight buffer
        [TempleDllLocation(0x1080FB88)]
        [TempleDllLocation(0x108EC550)]
        public locXY OriginTile { get; private set; }

        // The tiles fully or partially covered by this line of sight buffer
        public TileRect TileRect { get; private set; }

        public Span<byte> Buffer => _buffer.AsSpan();

        [TempleDllLocation(0x10824470)]
        private readonly byte[] _buffer;

        // The current ray being cast for line of sight is blocked
        [TempleDllLocation(0x108EC698)]
        private bool _currentLineOfSightBlocked;

        // a 3x3 area around the tile being checked by the los ray, used only when _currentLineOfSightBlocked is true
        [TempleDllLocation(0x10820448)]
        private readonly byte[] _tileNeighbours = new byte[9];

        public const byte UNK1 = 1;
        public const byte LINE_OF_SIGHT = 2;
        /// <summary>
        /// Means the tile was previously in LINE_OF_SIGHT, but it no longer is.
        /// This is then set by the explored sector data.
        /// </summary>
        public const byte EXPLORED = 4;
        public const byte BLOCKING = 0x8;
        public const byte EXTEND = 0x10;
        public const byte END = 0x20;
        public const byte BASE = 0x40;
        public const byte ARCHWAY = 0x80;

        public LineOfSightBuffer()
        {
            _centerSubtile = Point.Empty;
            CenterTile = locXY.Zero;
            _buffer = new byte[Dimension * Dimension];
        }

        public bool IsValid(LocAndOffsets center)
        {
            if (CenterTile != center.location)
            {
                return false;
            }

            // Determine which sub-tile of the tile the critter is standing on
            var subtileX = SubtileFromOffset(center.off_x);
            if (subtileX != _centerSubtile.X)
            {
                return false;
            }

            var subtileY = SubtileFromOffset(center.off_y);
            if (subtileY != _centerSubtile.Y)
            {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            Buffer.Fill(0);
        }

        private static int SubtileFromOffset(float offset)
        {
            if (offset < -HalfSubtile)
            {
                return 0;
            }
            else if (offset < HalfSubtile)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public void UpdateCenter(LocAndOffsets center)
        {
            CenterTile = center.location;
            _centerSubtile.X = SubtileFromOffset(center.off_x);
            _centerSubtile.Y = SubtileFromOffset(center.off_y);

            var partyMemberX = center.location.locx;
            var partyMemberY = center.location.locy;
            var radiusTiles = Radius / 3;

            OriginTile = new locXY(partyMemberX - radiusTiles, partyMemberY - radiusTiles);

            var tileRect = new TileRect
            {
                x1 = OriginTile.locx,
                y1 = OriginTile.locy,
                x2 = OriginTile.locx + Dimension / 3 - 1,
                y2 = OriginTile.locy + Dimension / 3 - 1
            };
            TileRect = tileRect;
        }

        [TempleDllLocation(0x100326d0)]
        public void ComputeLineOfSight(int sightRadius)
        {
            // Center subtile is just a subtile shift in range [0,3]
            var centerX = Radius + _centerSubtile.X;
            var centerY = Radius + _centerSubtile.Y;
            var buffer = Buffer;

            // Cast rays from the center to each point along the X-axis edge of the line of sight buffer
            // and do this for either side of the buffer (at Y- and Y+)
            for (var toX = 0; toX < Dimension; toX++)
            {
                // Upper left quadrant (towards Y-)
                CastLineOfSightRay(centerX, centerY, toX, 0, sightRadius, buffer);
                // Lower right quadrant (towards Y+)
                CastLineOfSightRay(centerX, centerY, toX, Dimension - 1, sightRadius, buffer);
            }

            // Cast rays from the center to each point along the Y-axis edge of the line of sight buffer
            // and do this for either side of the buffer (at X- and X+)
            for (var toY = 0; toY < Dimension; toY++)
            {
                // Upper right quadrant (towards X-)
                CastLineOfSightRay(centerX, centerY, 0, toY, sightRadius, buffer);
                // Lower left quadrant (towards X+)
                CastLineOfSightRay(centerX, centerY, Dimension - 1, toY, sightRadius, buffer);
            }
        }

        /// <summary>
        /// This function implements ray-casting from [fromX,fromY] to [toX,toY] in the line of sight buffer.
        /// It'll trace along the path of the ray, rasterized using Bresenham's line algorithm, and mark
        /// every tile that's still within line of sight, and stop, once it hits a tile that isn't.
        /// </summary>
        [TempleDllLocation(0x100310b0)]
        private void CastLineOfSightRay(int fromX, int fromY, int toX, int toY, int maxLength, Span<byte> losBuffer)
        {
            var maxLengthSquared = maxLength * maxLength;
            var deltaX = toX - fromX;
            var deltaY = toY - fromY;
            _currentLineOfSightBlocked = false;
            var slope = deltaY / (float) deltaX;

            if (toY == fromY)
            {
                // The ray is straight along the X-axis
                if (fromX >= toX)
                {
                    for (var x = fromX; x > fromX - maxLength; x--)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, x, fromY))
                            break;
                    }
                }
                else
                {
                    for (var x = fromX; x < fromX + maxLength; x++)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, x, fromY))
                            break;
                    }
                }
            }
            else if (toX == fromX)
            {
                // The ray is straight along the Y-axis
                if (fromY >= toY)
                {
                    for (var y = fromY; y > fromY - maxLength; y--)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX, y))
                            break;
                    }
                }
                else
                {
                    for (var y = fromY; y < fromY + maxLength; y++)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX, y))
                            break;
                    }
                }
            }
            else if (deltaX == deltaY || deltaX == -deltaY)
            {
                // Special cases for strictly diagonal raycasts. (slope == -1 or slope == 1)
                // NOTE: I think this might actually totaly not be worth it.

                if (fromX >= toX)
                {
                    // This branch handles cases that will raycast towards X-
                    if (fromY >= toY)
                    {
                        // This is the case for "straight up" (towards X- and Y- equally).
                        for (var length = 0; 2 * length * length <= maxLengthSquared; length++)
                        {
                            if (!AdvanceLineOfSightAlongRay(losBuffer, fromX - length, fromY - length))
                                break;
                        }
                    }
                    else
                    {
                        // This is the case for "straight right" (towards X- and Y+ equally).
                        for (var length = 0; 2 * length * length <= maxLengthSquared; length++)
                        {
                            if (!AdvanceLineOfSightAlongRay(losBuffer, fromX - length, fromY + length))
                                break;
                        }
                    }
                }
                else if (fromY >= toY)
                {
                    // This is the case for "straight left" (towards X+ and Y- equally).
                    for (var length = 0; 2 * length * length <= maxLengthSquared; length++)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + length, fromY - length))
                            break;
                    }
                }
                else
                {
                    // This is the case for "straight down" (towards X+ and Y+ equally).
                    for (var length = 0; 2 * length * length <= maxLengthSquared; length++)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + length, fromY + length))
                            break;
                    }
                }
            }
            else if (slope > 0.0f && slope < 1.0f)
            {
                if (fromX >= toX)
                {
                    // This handles raycasting between the X-/Y- diagonal and the X- axis.
                    // This is bresenham's line algorithm. See https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
                    // Specifically the integer version
                    var d = deltaX - 2 * deltaY;
                    var y = 0;
                    for (var x = 0; y * y + x * x < maxLengthSquared; x++)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX - x, fromY - y))
                        {
                            break;
                        }

                        if (d >= 0)
                        {
                            y++;
                            d += 2 * (deltaX + (-deltaY));
                        }
                        else
                        {
                            d += 2 * (-deltaY);
                        }
                    }
                }
                else
                {
                    var d = -deltaX + 2 * deltaY;
                    var y = 0;
                    for (var x = 0; y * y + x * x < maxLengthSquared; x++)
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + x, fromY + y))
                        {
                            break;
                        }

                        if (d >= 0)
                        {
                            y++;
                            d += 2 * ((-deltaX) + deltaY);
                        }
                        else
                        {
                            d += 2 * deltaY;
                        }
                    }
                }
            }
            else if (slope > 1.0f)
            {
                if (fromY >= toY)
                {
                    var d = deltaY - 2 * deltaX;
                    if (maxLengthSquared > 0)
                    {
                        var x = 0;
                        var y = 0;
                        do
                        {
                            if (!AdvanceLineOfSightAlongRay(losBuffer, fromX - x, fromY - y))
                                break;
                            if (d >= 0)
                            {
                                x++;
                                d += 2 * (deltaY - deltaX);
                            }
                            else
                            {
                                d -= 2 * deltaX;
                            }

                            y++;
                        } while (x * x + y * y < maxLengthSquared);
                    }
                }
                else
                {
                    var d = 2 * deltaX - deltaY;
                    if (maxLengthSquared > 0)
                    {
                        var x = 0;
                        var y = 0;
                        do
                        {
                            if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + x, fromY + y))
                                break;
                            if (d >= 0)
                            {
                                x++;
                                d += 2 * (deltaX - deltaY);
                            }
                            else
                            {
                                d += 2 * deltaX;
                            }

                            y++;
                        } while (x * x + y * y < maxLengthSquared);
                    }
                }
            }
            else if (slope > -1.0f && slope < 0.0f)
            {
                if (fromX >= toX)
                {
                    var d = deltaX + 2 * deltaY;
                    var x = 0;
                    var y = 0;
                    do
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX - x, fromY + y))
                            break;
                        if (d >= 0)
                        {
                            y++;
                            d += 2 * (deltaX + deltaY);
                        }
                        else
                        {
                            d += 2 * deltaY;
                        }

                        x++;
                    } while (x * x + y * y < maxLengthSquared);
                }
                else
                {
                    var d = -(deltaX + 2 * deltaY);
                    var x = 0;
                    var y = 0;
                    do
                    {
                        if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + x, fromY - y))
                            break;
                        if (d >= 0)
                        {
                            y++;
                            d += -2 * (deltaX + deltaY);
                        }
                        else
                        {
                            d -= 2 * deltaY;
                        }

                        x++;
                    } while (x * x + y * y < maxLengthSquared);
                }
            }
            else if (slope < -1.0f)
            {
                if (fromY >= toY)
                {
                    var d = deltaY + 2 * deltaX;
                    if (maxLengthSquared > 0)
                    {
                        var x = 0;
                        var y = 0;
                        do
                        {
                            if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + x, fromY - y))
                            {
                                break;
                            }

                            if (d >= 0)
                            {
                                x++;
                                d += 2 * (deltaX + deltaY);
                            }
                            else
                            {
                                d += 2 * deltaX;
                            }

                            y++;
                        } while (y * y + x * x < maxLengthSquared);
                    }
                }
                else
                {
                    var d = -(deltaY + 2 * deltaX);
                    if (maxLengthSquared > 0)
                    {
                        var x = 0;
                        var y = 0;
                        do
                        {
                            if (!AdvanceLineOfSightAlongRay(losBuffer, fromX + x, fromY + y))
                            {
                                break;
                            }

                            if (d >= 0)
                            {
                                --x;
                                d += -2 * (deltaX + deltaY);
                            }
                            else
                            {
                                d -= 2 * deltaX;
                            }

                            ++y;
                        } while (x * x + y * y < maxLengthSquared);
                    }
                }
            }
        }

        /// <summary>
        /// Returning false from this function will terminate the line of sight ray.
        /// </summary>
        [TempleDllLocation(0x10030fa0)]
        private bool AdvanceLineOfSightAlongRay(Span<byte> losBuffer, int x, int y)
        {
            var index = y * Dimension + x;
            if (_currentLineOfSightBlocked)
            {
                // Only consider this logic if the tile has 1 subtile of space around it,
                // that would still be within the buffer
                if (x >= 1 && x < Dimension - 1 && y >= 1 && y < Dimension - 1)
                {
                    // This would also be up one tile, one tile to the left,
                    // Starting essentially at x,y
                    var i = index - Dimension - 1;

                    int idx = 0;
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            if (_tileNeighbours[idx] != 0 && (losBuffer[i] & BLOCKING) == 0)
                            {
                                return false;
                            }

                            _tileNeighbours[idx] = (byte) (losBuffer[i] & BLOCKING);

                            i++;
                            idx++;
                        }

                        // TODO: This seems weird and might be a vanilla bug. it's not resetting to the initial column
                        i += Dimension - 2;
                    }

                    losBuffer[index] |= UNK1 | LINE_OF_SIGHT;
                    return true;
                }

                return false;
            }

            // When it's not blocking, mark it as UNK + UNK2 directly
            if ((losBuffer[index] & BLOCKING) == 0)
            {
                losBuffer[index] |= LINE_OF_SIGHT | UNK1;
                return true;
            }

            if (x < 1 || x >= Dimension - 1 || y < 1 || y >= Dimension - 1)
            {
                return false;
            }

            // This seems to copy over a 3x3 area around the adressed tile into the temp buffer
            var startIdx = index - Dimension - 1; // Start copying from the top-left of the current tile
            for (var i = 0; i < 9; i += 3)
            {
                _tileNeighbours[i] = (byte) (losBuffer[startIdx] & BLOCKING);
                _tileNeighbours[i + 1] = (byte) (losBuffer[startIdx + 1] & BLOCKING);
                _tileNeighbours[i + 2] = (byte) (losBuffer[startIdx + 2] & BLOCKING);
                // TODO The +1 here seems like a bug in vanilla.
                startIdx += Dimension + 1;
            }

            losBuffer[index] |= LINE_OF_SIGHT | UNK1;
            _currentLineOfSightBlocked = true;
            return true;
        }

        /// <summary>
        /// Extending line of sight is meant for situations where the actual line of sight is stopped by a wall
        /// or tall building, but it should extend "upwards" to cover the wall or building.
        /// It also ensures using a special "END" flag that visibility will not extend beyond a wall or such.
        /// Upwards in ToEE's world is towards x-,y-.
        /// </summary>
        [TempleDllLocation(0x100317e0)]
        public void ExtendLineOfSight()
        {
            var buffer = Buffer;

            for (var y = 0; y < Dimension; y++)
            {
                for (var x = 0; x < Dimension; x++)
                {
                    ExtendBlockingTiles(x, y, buffer);

                    ExtendArchways(x, y, buffer);
                }
            }
        }

        private static void ExtendBlockingTiles(int x, int y, Span<byte> buffer)
        {
            var currentFlags = buffer[y * Dimension + x];
            var extendLineOfSight = (currentFlags & LINE_OF_SIGHT) != 0;

            if ((currentFlags & BLOCKING) == 0)
            {
                return;
            }

            // Benchmarked this and C# is not capable of specializing this method properly based
            // on this boolean flag if it is moved into the loop. So we have two copies of the
            // algorithm here. One to clear the LOS flag, the other to set it. Perf difference is about 10%
            // over moving this flag into the inner loop.
            if (extendLineOfSight)
            {
                // The tile is within line of sight, so we *extend* the line of sight
                var diagonalLength = Math.Min(x, y);

                var state = 0;
                for (var i = 1; i <= diagonalLength; i++)
                {
                    var nextIdx = (y - i) * Dimension + x - i;
                    var nextFlags = buffer[nextIdx];
                    if (!ShouldContinueExtensionDiagonal(currentFlags, nextFlags, ref state))
                    {
                        break;
                    }

                    currentFlags = nextFlags;
                    if ((nextFlags & ARCHWAY) == 0)
                    {
                        buffer[nextIdx] |= LINE_OF_SIGHT;
                    }
                }
            }
            else
            {
                var diagonalLength = Math.Min(x, y);

                var state = 0;
                for (var i = 1; i <= diagonalLength; i++)
                {
                    var nextIdx = (y - i) * Dimension + x - i;

                    var nextFlags = buffer[nextIdx];
                    if (!ShouldContinueExtensionDiagonal(currentFlags, nextFlags, ref state))
                    {
                        break;
                    }

                    currentFlags = nextFlags;
                    if ((nextFlags & ARCHWAY) == 0)
                    {
                        buffer[nextIdx] = (byte) (nextFlags & ~LINE_OF_SIGHT);
                    }
                }
            }
        }

        private static bool ShouldContinueExtensionDiagonal(byte currentFlags, byte nextFlags, ref int state)
        {
            if ((nextFlags & (EXTEND | END)) == 0)
            {
                return false;
            }

            switch (state)
            {
                // This is the initial state
                case 0:
                    // We're still within a blocking area, so continue along
                    // without any special rules.
                    if ((nextFlags & BLOCKING) != 0)
                    {
                        return true;
                    }

                    state = 1;
                    return true;

                case 1:
                    if ((nextFlags & BLOCKING) == 0)
                    {
                        return true;
                    }

                    // We hit end first, after the first "non-blocking" area, but apparently no extend-only
                    if ((currentFlags & END) != 0)
                    {
                        return false;
                    }

                    state = 2;
                    goto case 2;

                case 2:
                    // We hit the end after an extends run, now switch to the end state
                    if ((nextFlags & END) != 0)
                    {
                        state = 3;
                    }

                    return true;

                case 3:
                    // Continue through the "tail" of tiles marked as END.
                    return (nextFlags & END) != 0;

                default:
                    return true;
            }
        }

        private static void ExtendArchways(int x, int y, Span<byte> buffer)
        {
            var currentFlags = buffer[y * Dimension + x];

            if ((currentFlags & BASE) == 0)
            {
                // Skip tiles not marked as a base.
                return;
            }

            var foundArchway = false;
            // For the reasoning behind duplicating this algorithm twice, see the function to extend blockers above
            // It's for performance reasons.
            if ((currentFlags & LINE_OF_SIGHT) != 0)
            {
                var diagonalLength = Math.Min(x, y);
                for (var i = 1; i <= diagonalLength; i++)
                {
                    var nextIdx = (y - i) * Dimension + x - i;
                    var nextFlags = buffer[nextIdx];
                    if (foundArchway)
                    {
                        if ((nextFlags & ARCHWAY) == 0)
                        {
                            break;
                        }

                        buffer[nextIdx] = (byte) (nextFlags | LINE_OF_SIGHT);
                    }
                    else if ((nextFlags & ARCHWAY) != 0)
                    {
                        foundArchway = true;
                        buffer[nextIdx] = (byte) (nextFlags | LINE_OF_SIGHT);
                    }
                }
            }
            else
            {
                var diagonalLength = Math.Min(x, y);
                for (var i = 1; i <= diagonalLength; i++)
                {
                    var nextIdx = (y - i) * Dimension + x - i;
                    var nextFlags = buffer[nextIdx];
                    if (foundArchway)
                    {
                        if ((nextFlags & ARCHWAY) == 0)
                        {
                            break;
                        }

                        buffer[nextIdx] = (byte) (nextFlags & ~LINE_OF_SIGHT);
                    }
                    else if ((nextFlags & ARCHWAY) != 0)
                    {
                        foundArchway = true;
                        buffer[nextIdx] = (byte) (nextFlags & ~LINE_OF_SIGHT);
                    }
                }
            }
        }

        public void SaveTo(string filename)
        {
            using var stream = new FileStream(filename, FileMode.Create);
            stream.Write(_buffer);
        }
    }
}