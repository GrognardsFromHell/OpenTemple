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
        public const byte UNK = 2;
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

                    losBuffer[index] |= UNK1 | UNK;
                    return true;
                }

                return false;
            }

            // When it's not blocking, mark it as UNK + UNK2 directly
            if ((losBuffer[index] & BLOCKING) == 0)
            {
                losBuffer[index] |= UNK | UNK1;
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

            losBuffer[index] |= UNK | UNK1;
            _currentLineOfSightBlocked = true;
            return true;
        }

        [TempleDllLocation(0x100317e0)]
        public void ExtendLineOfSight2()
        {
            int v1; // ebx@1
            int v2; // ebp@1
            int v3; // esi@2
            int v4; // edi@3
            byte v6; // dl@4
            int v7; // ecx@5
            int v8; // ebp@5
            int v9; // esi@5
            int v10; // edi@8
            byte v11; // al@9
            int v12; // edi@28
            byte v13; // al@29
            byte v15; // cl@46
            int v16; // edx@47
            int v17; // ecx@48
            int v18; // esi@50
            byte v19; // al@51
            int v20; // ecx@59
            int i; // esi@61
            byte v22; // al@62
            int v23; // [sp+0h] [bp-14h]@3
            int v24; // [sp+4h] [bp-10h]@1
            int v25; // [sp+8h] [bp-Ch]@2
            int v26; // [sp+Ch] [bp-8h]@2
            int v27; // [sp+10h] [bp-4h]@1

            var buffer = Buffer;

            v1 = 0;
            v2 = Dimension;
            v24 = 0;
            v27 = Dimension;
            if (Dimension > 0)
            {
                v3 = -Dimension;
                v26 = 0;
                v25 = -Dimension;
                do
                {
                    v4 = 0;
                    v23 = 0;
                    do
                    {
                        v6 = buffer[v26 + v4];
                        if ((v6 & 8) == 0)
                            goto LABEL_46;
                        v7 = 0;
                        v8 = v2 + 1;
                        v9 = v4 - 1 + v3;
                        if ((v6 & 2) != 0)
                        {
                            if (v4 < v1)
                                v1 = v4;
                            v10 = 1;
                            if (v1 >= 1)
                            {
                                while (true)
                                {
                                    v11 = buffer[v9];
                                    if ((buffer[v9] & 0x30) == 0)
                                        goto LABEL_45;
                                    if (v7 == 0)
                                    {
                                        if ((v11 & 8) != 0)
                                            goto LABEL_22;
                                        v7 = 1;
                                    }

                                    if (v7 == 1)
                                    {
                                        if ((v11 & 8) == 0)
                                            goto LABEL_22;
                                        if ((v6 & 0x20) != 0)
                                            goto LABEL_45;
                                        v7 = 2;
                                        goto LABEL_18;
                                    }

                                    if (v7 != 2)
                                    {
                                        if (v7 == 3)
                                        {
                                            if ((v11 & 0x20) == 0)
                                                goto LABEL_45;
                                        }

                                        goto LABEL_22;
                                    }

                                    LABEL_18:
                                    if ((v11 & 0x20) != 0)
                                    {
                                        v7 = 3;
                                        if ((v11 & 0x20) == 0)
                                            goto LABEL_45;
                                    }

                                    LABEL_22:
                                    v6 = buffer[v9];
                                    if ((v11 & 0x80) == 0)
                                        buffer[v9] = (byte) (v11 | 2);
                                    v9 -= v8;
                                    if (++v10 > v1)
                                        goto LABEL_45;
                                }
                            }

                            goto LABEL_45;
                        }

                        if (v4 < v1)
                            v1 = v4;
                        v12 = 1;
                        if (v1 >= 1)
                        {
                            while (true)
                            {
                                v13 = buffer[v9];
                                if ((buffer[v9] & 0x30) == 0)
                                    goto LABEL_45;
                                if (v7 == 0)
                                {
                                    if ((v13 & 8) != 0)
                                        goto LABEL_42;
                                    v7 = 1;
                                }

                                if (v7 == 1)
                                {
                                    if ((v13 & 8) == 0)
                                        goto LABEL_42;
                                    if ((v6 & 0x20) != 0)
                                        goto LABEL_45;
                                    v7 = 2;
                                    goto LABEL_38;
                                }

                                if (v7 != 2)
                                {
                                    if (v7 == 3)
                                    {
                                        if ((v13 & 0x20) == 0)
                                            goto LABEL_45;
                                    }

                                    goto LABEL_42;
                                }

                                LABEL_38:
                                if ((v13 & 0x20) != 0)
                                {
                                    v7 = 3;
                                    if ((v13 & 0x20) == 0)
                                        goto LABEL_45;
                                }

                                LABEL_42:
                                v6 = buffer[v9];
                                if ((v13 & 0x80) == 0)
                                    buffer[v9] = (byte) (v13 & 0xFD);
                                v9 -= v8;
                                if (++v12 > v1)
                                    goto LABEL_45;
                            }
                        }

                        LABEL_45:
                        v4 = v23;
                        LABEL_46:
                        v15 = buffer[v26 + v4];
                        if ((v15 & 0x40) == 0)
                            goto LABEL_70;
                        v16 = 0;
                        if ((v15 & 2) != 0)
                        {
                            v17 = v4 - 1 + v25;
                            if (v4 >= v24)
                                v4 = v24;
                            v18 = 1;
                            if (v4 >= 1)
                            {
                                while (true)
                                {
                                    v19 = buffer[v17];
                                    if (v16 != 0)
                                    {
                                        if (v16 == 1)
                                        {
                                            if ((v19 & 0x80) == 0)
                                                goto LABEL_69;
                                            buffer[v17] = (byte) (v19 | 2);
                                            goto LABEL_57;
                                        }
                                    }
                                    else if ((v19 & 0x80) != 0)
                                    {
                                        v16 = 1;
                                        buffer[v17] = (byte) (v19 | 2);
                                        goto LABEL_57;
                                    }

                                    LABEL_57:
                                    v17 += -v27 - 1;
                                    if (++v18 > v4)
                                        goto LABEL_69;
                                }
                            }

                            goto LABEL_69;
                        }

                        v20 = v4 - 1 + v25;
                        if (v4 >= v24)
                            v4 = v24;
                        for (i = 1; i <= v4; ++i)
                        {
                            v22 = buffer[v20];
                            if (v16 != 0)
                            {
                                if (v16 == 1)
                                {
                                    if ((v22 & 0x80) == 0)
                                        break;
                                    buffer[v20] = (byte) (v22 & 0xFD);
                                    goto LABEL_68;
                                }
                            }
                            else if ((v22 & 0x80) != 0)
                            {
                                v16 = 1;
                                buffer[v20] = (byte) (v22 & 0xFD);
                                goto LABEL_68;
                            }

                            LABEL_68:
                            v20 += -v27 - 1;
                        }

                        LABEL_69:
                        v4 = v23;
                        LABEL_70:
                        v2 = v27;
                        v3 = v25;
                        v1 = v24;
                        v23 = ++v4;
                    } while (v4 < v27);

                    v1 = v24 + 1;
                    v3 = v27 + v25;
                    v24 = v1;
                    v26 += v27;
                    v25 += v27;
                } while (v1 < v27);
            }
        }

        [TempleDllLocation(0x100317e0)]
        public void ExtendLineOfSight()
        {
            var buffer = Buffer;

            for (var y = 0; y < Dimension; y++)
            {
                var iVar5 = (y - 1) * Dimension;
                var local_8 = y * Dimension;
                for (var x = 0; x < Dimension; x++)
                {
                    var bVar7 = buffer[x + local_8];
                    if ((bVar7 & BLOCKING) != 0)
                    {
                        // This might be (-1, -1) in coordinate terms
                        var pbVar8 = iVar5 + x - 1;
                        if ((bVar7 & UNK) == 0)
                        {
                            var iVar6 = y;
                            if (x < y)
                            {
                                iVar6 = x;
                            }

                            var endChain = false;
                            var iVar10 = 0;
                            for (var i = 0; i < iVar6; i++)
                            {
                                var bVar1 = buffer[pbVar8];
                                if ((bVar1 & (EXTEND | END)) == 0)
                                    break;

                                switch (iVar10)
                                {
                                    case 0:
                                        if ((bVar1 & BLOCKING) == 0)
                                        {
                                            iVar10 = 1;
                                        }

                                        break;
                                    case 1:
                                        if ((bVar1 & BLOCKING) != 0)
                                        {
                                            // This is weird because it is checking the previous tile in the chain
                                            if ((bVar7 & END) != 0)
                                            {
                                                endChain = true;
                                                break;
                                            }

                                            iVar10 = 2;
                                        }

                                        break;
                                    case 2:
                                        if ((bVar1 & END) != 0)
                                        {
                                            iVar10 = 3;
                                        }

                                        break;
                                    case 3:
                                        if ((bVar1 & END) == 0)
                                        {
                                            endChain = true;
                                            break;
                                        }

                                        break;
                                }

                                if (endChain)
                                {
                                    break;
                                }

                                if ((bVar1 & ARCHWAY) == 0)
                                {
                                    buffer[pbVar8] = (byte) (bVar1 & ~UNK);
                                }

                                // Moving diagonally to the upper left????
                                pbVar8 -= Dimension + 1;
                                bVar7 = bVar1;
                            }
                        }
                        else
                        {
                            var iVar9 = y;
                            if (x < y)
                            {
                                iVar9 = x;
                            }

                            var iVar6 = 0;
                            for (var i = 0; i < iVar9; i++)
                            {
                                var bVar1 = buffer[pbVar8];
                                if ((bVar1 & (EXTEND | END)) == 0)
                                    break;

                                var endChain = false;
                                switch (iVar6)
                                {
                                    case 0:
                                        iVar6 = 1;
                                        break;
                                    case 1:
                                        if ((bVar1 & BLOCKING) != 0)
                                        {
                                            // This is weird because it is checking the previous tile in the chain
                                            if ((bVar7 & END) != 0)
                                            {
                                                endChain = true;
                                                break;
                                            }

                                            iVar6 = 2;
                                        }

                                        break;
                                    case 2:
                                        if ((bVar1 & END) != 0)
                                        {
                                            iVar6 = 3;
                                        }

                                        break;
                                    case 3:
                                        if ((bVar1 & END) == 0)
                                        {
                                            endChain = true;
                                            break;
                                        }

                                        break;
                                }

                                if (endChain)
                                {
                                    break;
                                }

                                if ((bVar1 & ARCHWAY) == 0)
                                {
                                    buffer[pbVar8] |= UNK;
                                }

                                // This could be up one tile and left one tile
                                pbVar8 -= Dimension + 1;
                                bVar7 = bVar1;
                            }
                        }
                    }

                    if ((buffer[x + local_8] & BASE) != 0)
                    {
                        var bVar4 = false;
                        if ((buffer[x + local_8] & UNK) == 0)
                        {
                            // This starts one tile up and one tile left again...
                            var pbVar8 = x + iVar5 - 1;
                            var iVar6 = Math.Min(y, x);

                            for (var iVar9 = 0; iVar9 < iVar6; iVar9++)
                            {
                                bVar7 = buffer[pbVar8];
                                if (bVar4)
                                {
                                    if ((bVar7 & ARCHWAY) != 0)
                                    {
                                        buffer[pbVar8] &= unchecked((byte) ~UNK);
                                    }

                                    break;
                                }
                                else
                                {
                                    if ((bVar7 & ARCHWAY) != 0)
                                    {
                                        bVar4 = true;
                                        buffer[pbVar8] &= unchecked((byte) ~UNK);
                                    }
                                }

                                // Again, one row up, one column left
                                pbVar8 -= (Dimension + 1);
                            }
                        }
                        else
                        {
                            var pbVar8 = x + iVar5 - 1;
                            var iVar6 = Math.Min(y, x);

                            for (var iVar9 = 0; iVar9 < iVar6; iVar9++)
                            {
                                bVar7 = buffer[pbVar8];
                                if (bVar4)
                                {
                                    if ((bVar7 & ARCHWAY) != 0)
                                    {
                                        buffer[pbVar8] |= UNK;
                                    }

                                    break;
                                }
                                else
                                {
                                    if ((bVar7 & ARCHWAY) != 0)
                                    {
                                        bVar4 = true;
                                        buffer[pbVar8] |= 2;
                                    }
                                }

                                // Again, one row up, one column left
                                pbVar8 -= (Dimension + 1);
                            }
                        }
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