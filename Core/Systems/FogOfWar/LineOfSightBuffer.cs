using System;
using System.Drawing;
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
        public void ComputeLineOfSight(int unk)
        {
            var v5 = _centerSubtile.X + Radius;
            var v7 = _centerSubtile.Y + Radius;
            var buffer = Buffer;

            for (var v8 = 0; v8 < Dimension; v8++) {
                fog_perform_fog_checks_3(v5, v7, v8, 0, unk, buffer);
                fog_perform_fog_checks_3(v5, v7, v8, Dimension - 1, unk, buffer);
            }

            for (var v9 = 0; v9 < Dimension; v9++) {
                fog_perform_fog_checks_3(v5, v7, 0, v9, unk, buffer);
                fog_perform_fog_checks_3(v5, v7, Dimension - 1, v9, unk, buffer);
            }
        }

        [TempleDllLocation(0x108EC698)]
        private bool dword_108EC698;

        [TempleDllLocation(0x100310b0)]
        private void fog_perform_fog_checks_3(int fromX, int fromY, int toX, int toY, int a5, Span<byte> losBuffer)
        {
            int v6; // ebp@1
            int v7; // esi@1
            int v8; // edi@1
            int v9; // eax@1
            int v10; // ecx@1
            float v11; // fst7@1
            int v12; // ebx@26
            int v13; // ecx@32
            int v14; // ebx@34
            int v15; // edx@40
            int v16; // ebx@44
            int v17; // ecx@50
            int v18; // ebx@52
            int v19; // ecx@58
            int v20; // eax@60
            int v21; // ebx@63
            int v22; // ebx@68
            int v23; // ecx@70
            int v24; // ebp@74
            int v25; // ebx@74
            int v26; // ecx@80
            int v27; // ebx@81
            int v28; // ebp@82
            int v29; // ebx@87
            int v30; // ecx@92
            int v31; // ebx@94
            int v32; // ebp@95
            int v33; // eax@98
            int v34; // ebx@104
            int v35; // esi@104
            int v36; // ebp@105
            int v37; // edi@105
            int v38; // ebp@112
            int v39; // ebx@112
            int v40; // edi@113
            int v41; // esi@113
            int v42; // [sp+10h] [bp-14h]@1
            int v43; // [sp+14h] [bp-10h]@1
            int v44; // [sp+18h] [bp-Ch]@60
            int v45; // [sp+18h] [bp-Ch]@81
            int v46; // [sp+1Ch] [bp-8h]@27
            int v47; // [sp+1Ch] [bp-8h]@35
            int v48; // [sp+1Ch] [bp-8h]@45
            int v49; // [sp+1Ch] [bp-8h]@53
            int v50; // [sp+20h] [bp-4h]@27
            int v51; // [sp+20h] [bp-4h]@35
            int v52; // [sp+20h] [bp-4h]@45
            int v53; // [sp+20h] [bp-4h]@53
            int v54; // [sp+20h] [bp-4h]@68
            int v55; // [sp+28h] [bp+4h]@94
            int v56; // [sp+2Ch] [bp+8h]@82
            int v57; // [sp+2Ch] [bp+8h]@95

            v6 = a5;
            v7 = fromY;
            v8 = fromX;
            v9 = toX - fromX;
            v10 = toY - fromY;
            v42 = toY - fromY;
            v43 = toX - fromX;
            dword_108EC698 = false;
            v11 = (toY - fromY) / (float) (toX - fromX);
            if (toY == fromY)
            {
                if (fromX >= toX)
                {
                    if (fromX > fromX - a5)
                    {
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, fromY))
                                break;
                            --v8;
                        } while (v8 > fromX - a5);
                    }
                }
                else if (fromX < fromX + a5)
                {
                    do
                    {
                        if (!fog_perform_fog_checks_4(losBuffer, v8, fromY))
                            break;
                        ++v8;
                    } while (v8 < fromX + a5);
                }
            }
            else if (toX == fromX)
            {
                if (fromY >= toY)
                {
                    if (fromY > fromY - a5)
                    {
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, fromX, v7))
                                break;
                            --v7;
                        } while (v7 > fromY - a5);
                    }
                }
                else if (fromY < fromY + a5)
                {
                    do
                    {
                        if (!fog_perform_fog_checks_4(losBuffer, fromX, v7))
                            break;
                        ++v7;
                    } while (v7 < fromY + a5);
                }
            }
            else if (v9 == v10 || v9 == -v10)
            {
                v20 = a5 * a5;
                v44 = a5 * a5;
                if (fromX >= toX)
                {
                    if (fromY >= toY)
                    {
                        if (v20 >= 0)
                        {
                            v29 = 0;
                            do
                            {
                                if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                    break;
                                --v8;
                                --v7;
                                --v29;
                            } while (2 * v29 * v29 <= v44);
                        }
                    }
                    else if (v20 >= 0)
                    {
                        v24 = 0;
                        v25 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            --v8;
                            --v25;
                            ++v7;
                            ++v24;
                        } while (v24 * v24 + v25 * v25 <= v44);

                        v6 = a5;
                    }
                }
                else if (fromY >= toY)
                {
                    if (v20 >= 0)
                    {
                        v22 = 0;
                        v54 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            ++v8;
                            ++v22;
                            --v7;
                            v23 = (v54 - 1) * (v54 - 1);
                            --v54;
                        } while (v23 + v22 * v22 <= v44);
                    }
                }
                else if (v20 >= 0)
                {
                    v21 = 0;
                    do
                    {
                        if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                            break;
                        ++v8;
                        ++v7;
                        ++v21;
                    } while (2 * v21 * v21 <= v44);
                }
            }
            else if ((0.0 < v11) && (v11 < 1.0))
            {
                if (fromX >= toX)
                {
                    v14 = v43 - 2 * v42;
                    if (a5 * a5 > 0)
                    {
                        v47 = 0;
                        v51 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v14 >= 0)
                            {
                                --v7;
                                --v47;
                                v14 += 2 * (v43 - v42);
                            }
                            else
                            {
                                v14 -= 2 * v42;
                            }

                            --v8;
                            v15 = (v51 - 1) * (v51 - 1);
                            --v51;
                        } while (v47 * v47 + v15 < a5 * a5);
                    }
                }
                else
                {
                    v12 = 2 * v42 - v43;
                    if (a5 * a5 > 0)
                    {
                        v46 = 0;
                        v50 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v12 >= 0)
                            {
                                ++v7;
                                ++v46;
                                v12 += 2 * (v42 - v43);
                            }
                            else
                            {
                                v12 += 2 * v42;
                            }

                            ++v8;
                            v13 = (v50 + 1) * (v50 + 1);
                            ++v50;
                        } while (v46 * v46 + v13 < a5 * a5);
                    }
                }
            }
            else if (v11 > 1.0)
            {
                v7 = fromY;
                v8 = fromX;
                if (fromY >= toY)
                {
                    v18 = v42 - 2 * v43;
                    if (a5 * a5 > 0)
                    {
                        v53 = 0;
                        v49 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v18 >= 0)
                            {
                                --v8;
                                --v49;
                                v18 += 2 * (v42 - v43);
                            }
                            else
                            {
                                v18 -= 2 * v43;
                            }

                            --v7;
                            v19 = (v53 - 1) * (v53 - 1);
                            --v53;
                        } while (v19 + v49 * v49 < a5 * a5);
                    }
                }
                else
                {
                    v16 = 2 * v43 - v42;
                    if (a5 * a5 > 0)
                    {
                        v52 = 0;
                        v48 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v16 >= 0)
                            {
                                ++v8;
                                ++v48;
                                v16 += 2 * (v43 - v42);
                            }
                            else
                            {
                                v16 += 2 * v43;
                            }

                            ++v7;
                            v17 = (v52 + 1) * (v52 + 1);
                            ++v52;
                        } while (v17 + v48 * v48 < a5 * a5);
                    }
                }
            }

            if ((-1.0 < v11) && (v11 < 0.0))
            {
                v26 = fromX;
                if (fromX >= toX)
                {
                    v55 = v43 + 2 * v42;
                    v31 = v6 * v6;
                    if ((v8 - v26) * (v8 - v26) + (v7 - fromY) * (v7 - fromY) < v6 * v6)
                    {
                        v32 = v7 - fromY;
                        v57 = v8 - v26;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v55 >= 0)
                            {
                                ++v7;
                                ++v32;
                                v33 = v55 + 2 * (v43 + v42);
                            }
                            else
                            {
                                v33 = 2 * v42 + v55;
                            }

                            v55 = v33;
                            --v8;
                            --v57;
                        } while (v32 * v32 + v57 * v57 < v31);
                    }
                }
                else
                {
                    v45 = v6 * v6;
                    v27 = -(v43 + 2 * v42);
                    if ((v8 - fromX) * (v8 - fromX) + (v7 - fromY) * (v7 - fromY) < v6 * v6)
                    {
                        v28 = v7 - fromY;
                        v56 = v8 - fromX;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v27 >= 0)
                            {
                                --v7;
                                --v28;
                                v27 += -2 * (v43 + v42);
                            }
                            else
                            {
                                v27 -= 2 * v42;
                            }

                            ++v8;
                            v30 = (v56 + 1) * (v56 + 1);
                            ++v56;
                        } while (v28 * v28 + v30 < v45);
                    }
                }
            }
            else if ((v11 < -1.0))
            {
                if (fromY >= toY)
                {
                    v38 = v6 * v6;
                    v39 = v42 + 2 * v43;
                    if (v38 > 0)
                    {
                        v40 = 0;
                        v41 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v41 + fromX, v40 + fromY))
                                break;
                            if (v39 >= 0)
                            {
                                ++v41;
                                v39 += 2 * (v43 + v42);
                            }
                            else
                            {
                                v39 += 2 * v43;
                            }

                            --v40;
                        } while (v40 * v40 + v41 * v41 < v38);
                    }
                }
                else
                {
                    v34 = v6 * v6;
                    v35 = -(v42 + 2 * v43);
                    if (v6 * v6 > 0)
                    {
                        v36 = 0;
                        v37 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v37 + fromX, fromY + v36))
                                break;
                            if (v35 >= 0)
                            {
                                --v37;
                                v35 += -2 * (v43 + v42);
                            }
                            else
                            {
                                v35 -= 2 * v43;
                            }

                            ++v36;
                        } while (v36 * v36 + v37 * v37 < v34);
                    }
                }
            }
        }

        [TempleDllLocation(0x10820448)]
        private byte[] byte_10820448 = new byte[9];

        private bool fog_perform_fog_checks_4(Span<byte> losBuffer, int x, int y)
        {
            var index = y * Dimension + x;
            if (dword_108EC698)
            {
                // Only consider this logic if the tile has 1 subtile of space around it,
                // that would still be within the buffer
                if (x >= 1 && x < Dimension - 1 && y >= 1 && y < Dimension - 1)
                {
                    // This would also be up one tile, one tile to the left,
                    // Starting essentially at x,y
                    var i = index - Dimension - 1;

                    var v16 = 0;
                    LABEL_15:
                    var v17 = 0;
                    while (byte_10820448[v16 + v17] == 0 || (losBuffer[i] & BLOCKING) != 0)
                    {
                        byte_10820448[v16 + v17++] = (byte) (losBuffer[i++] & BLOCKING);
                        if (v17 >= 3)
                        {
                            v16 += 3;
                            i += Dimension - 2;
                            if (v16 < 9)
                                goto LABEL_15;
                            losBuffer[index] |= UNK1 | UNK;
                            return true;
                        }
                    }
                }

                return false;
            }


            // When it's not blocking, mark it as UNK + UNK2 directly
            if ((losBuffer[index] & BLOCKING) == 0)
            {
                losBuffer[index] |= UNK|UNK1;
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
                byte_10820448[i] = (byte) (losBuffer[startIdx] & BLOCKING);
                byte_10820448[i + 1] = (byte) (losBuffer[startIdx + 1] & BLOCKING);
                byte_10820448[i + 2] = (byte) (losBuffer[startIdx + 2] & BLOCKING);
                startIdx += Dimension;
            }

            losBuffer[index] |= UNK|UNK1;
            dword_108EC698 = true;
            return true;
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

    }
}