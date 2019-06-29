using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    internal class MapFogdataChunk
    {
        public bool AllExplored;

        public byte[] Data = new byte[8192];
    }

    public class MapFoggingSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem
    {
        private readonly RenderingDevice mDevice;

        private const int sFogBufferDim = 102;

        [TempleDllLocation(0x10824468)]
        internal int mFogMinX;

        [TempleDllLocation(0x108EC4C8)]
        internal int mFogMinY;

        [TempleDllLocation(0x10820458)]
        internal int mSubtilesX;

        [TempleDllLocation(0x10824490)]
        internal int mSubtilesY;

        [TempleDllLocation(0x108254A0)]
        internal bool mFoggingEnabled;

        [TempleDllLocation(0x108A5498)]
        internal byte[] mFogCheckData;

        // Maximum line of sight radius
        private const int MaxLosRadius = 102;

        private const int MaxLosDiameter = MaxLosRadius * 2;

        // 8 entries, one for each controllable party member
        // The buffers themselves contain one byte per sub-tile within the creature's line of sight area
        // determined by MaxLosDiameter
        [TempleDllLocation(0x10824470)]
        private byte[][] mFogBuffers;

        [TempleDllLocation(0x11E61560)]
        private MapFogdataChunk[] mFogUnexploredData = new MapFogdataChunk[4];

        [TempleDllLocation(0x108EC590)]
        private bool mDoFullUpdate;

        [TempleDllLocation(0x102ACEFC)]
        private int mFogChecks;

        [TempleDllLocation(0x108EC6A0)]
        private int mScreenWidth => mScreenSize.Width;

        [TempleDllLocation(0x108EC6A4)]
        private int mScreenHeight => mScreenSize.Height;

        private Size mScreenSize;

        // TODO This seems to be fully unused
        [TempleDllLocation(0x102ACF00)]
        private int fogcol_field1;

        // TODO This is used in the townmap UI
        [TempleDllLocation(0x102ACF04)]
        private int fogcol_field2;

        [TempleDllLocation(0x102ACF08)]
        private int qword_102ACF08;

        [TempleDllLocation(0x102ACF10)]
        private int qword_102ACF10;

        public FogOfWarRenderer Renderer { get; }

        [TempleDllLocation(0x10032020)]
        public MapFoggingSystem(RenderingDevice renderingDevice)
        {
            mDevice = renderingDevice;

            mFogCheckData = null;

            mFoggingEnabled = true;
            mFogBuffers = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                mFogBuffers[i] = new byte[16 * sFogBufferDim * sFogBufferDim];
            }

            InitScreenBuffers();

            Renderer = new FogOfWarRenderer(this, renderingDevice);
        }

        private void InitScreenBuffers()
        {
            mScreenSize = mDevice.GetCamera().ScreenSize;

            UpdateFogLocation();
        }

        private void UpdateFogLocation()
        {
            var camera = mDevice.GetCamera();

            // Calculate the tile locations in each corner of the screen
            var topLeftLoc = camera.ScreenToTile(0, 0);
            var topRightLoc = camera.ScreenToTile(mScreenSize.Width, 0);
            var bottomLeftLoc = camera.ScreenToTile(0, mScreenSize.Height);
            var bottomRightLoc = camera.ScreenToTile(mScreenSize.Width, mScreenSize.Height);

            mFogMinX = topRightLoc.location.locx;
            mFogMinY = topLeftLoc.location.locy;

            // Whatever the point of this may be ...
            if (topLeftLoc.off_y < topLeftLoc.off_x || topLeftLoc.off_y < -topLeftLoc.off_x)
            {
                mFogMinY--;
            }

            mSubtilesX = (bottomLeftLoc.location.locx - mFogMinX + 3) * 3;
            mSubtilesY = (bottomRightLoc.location.locy - mFogMinY + 3) * 3;

            if (mFogCheckData == null || mFogCheckData.Length != mSubtilesX * mSubtilesY)
            {
                mFogCheckData = new byte[mSubtilesX * mSubtilesY];
                mDoFullUpdate = true;
            }
        }

        [TempleDllLocation(0x1002eb80)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x1002ebd0)]
        public void Reset()
        {
            mDoFullUpdate = false;
            _explorationData.Clear();
        }

        [TempleDllLocation(0x108EC6A8)]
        private int fog_next_check_for_idx;

        [TempleDllLocation(0x1080FA88)]
        private locXY[] fog_check_locations = new locXY[32];

        [TempleDllLocation(0x108203C8)]
        private int[] fog_check_subtile_x = new int[32];

        [TempleDllLocation(0x108EC4D0)]
        private int[] fog_check_subtile_y = new int[32];

        // Specifies the X-component of the tile that is the origin of a party members line of sight buffer
        [TempleDllLocation(0x1080FB88)]
        private int[] fog_buffer_origin_x = new int[32];

        // Specifies the Y-component of the tile that is the origin of a party members line of sight buffer
        [TempleDllLocation(0x108EC550)]
        private int[] fog_buffer_origin_y = new int[32];

        // This is lazily populated
        [TempleDllLocation(0x108EC598)] [TempleDllLocation(0x108EC6B0)]
        private readonly Dictionary<SectorLoc, SectorExploration> _explorationData
            = new Dictionary<SectorLoc, SectorExploration>();

        private const float HalfSubtile = locXY.INCH_PER_SUBTILE / 2;

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

        [TempleDllLocation(0x10031ef0)]
        private SectorExploration GetOrLoadExploredSectorData(SectorLoc loc)
        {
            if (_explorationData.TryGetValue(loc, out var exploration))
            {
                return exploration;
            }

            exploration = GameSystems.Map.LoadSectorExploration(loc);
            _explorationData[loc] = exploration;
            return exploration;
        }

        [TempleDllLocation(0x10030f40)]
        public void FlushSectorExploration()
        {
            foreach (var (sectorLoc, exploration) in _explorationData)
            {
                GameSystems.Map.SaveSectorExploration(sectorLoc, exploration);
            }

            _explorationData.Clear();
        }

        public byte GetFogStatus(LocAndOffsets loc) => GetFogStatus(loc.location, loc.off_x, loc.off_y);

        [TempleDllLocation(0x100336B0)]
        public void PerformFogChecks()
        {
            if (!mFoggingEnabled || GameSystems.Party.PartySize <= 0)
            {
                return;
            }

            UpdateFogLocation();

            var updateScreenFogBuffer = 0;

            int numberOfFogChecks;
            if (mDoFullUpdate)
            {
                numberOfFogChecks = 8;
            }
            else
            {
                numberOfFogChecks = Globals.Config.FogChecksPerFrame;
            }

            for (var i = 0; i < numberOfFogChecks; i++)
            {
                var partyIndex = fog_next_check_for_idx;
                if (fog_next_check_for_idx >= GameSystems.Party.PartySize)
                {
                    partyIndex = 0;
                    fog_next_check_for_idx = 0;
                }

                fog_next_check_for_idx++;

                var partyMember = GameSystems.Party.GetPartyGroupMemberN(partyIndex);

                // Determine which sub-tile of the tile the critter is standing on
                var subtileX = SubtileFromOffset(partyMember.OffsetX);
                var subtileY = SubtileFromOffset(partyMember.OffsetY);

                if (!mDoFullUpdate)
                {
                    if (fog_check_locations[partyIndex] == partyMember.GetLocation()
                        && subtileX == fog_check_subtile_x[partyIndex]
                        && subtileY == fog_check_subtile_y[partyIndex])
                    {
                        continue;
                    }
                }

                updateScreenFogBuffer |= 1; // Some party member's fog buffer has changed
                fog_check_locations[partyIndex] = partyMember.GetLocation();
                fog_check_subtile_x[partyIndex] = SubtileFromOffset(partyMember.OffsetX);
                fog_check_subtile_y[partyIndex] = SubtileFromOffset(partyMember.OffsetY);

                fog_perform_fog_checks_0(partyMember, partyIndex);
            }

            mDoFullUpdate = false;

            var translationX = GameSystems.Location.LocationTranslationX;
            var translationY = GameSystems.Location.LocationTranslationY;
            if (qword_102ACF08 != translationX || qword_102ACF10 != translationY)
            {
                updateScreenFogBuffer |= 2; // Translation has changed
                qword_102ACF08 = translationX;
                qword_102ACF10 = translationY;
            }

            if (updateScreenFogBuffer != 0)
            {
                mFogCheckData.AsSpan().Fill(0);

                for (int i = 0; i < GameSystems.Party.PartySize; i++)
                {
                    MarkLineOfSight(i);
                }

                MarkExploredSubtiles();
            }
        }

        private MemoryPool<Vector2> _vertexPool = MemoryPool<Vector2>.Shared;

        [TempleDllLocation(0x100327a0)]
        private void fog_perform_fog_checks_0(GameObjectBody partyMember, in int partyIndex)
        {
            // Lazily create the los buffer
            if (mFogBuffers[partyIndex] == null)
            {
                mFogBuffers[partyIndex] = new byte[MaxLosDiameter * MaxLosDiameter];
            }

            // Start out by clearing the LOS info
            Span<byte> losBuffer = mFogBuffers[partyIndex];
            losBuffer.Fill(0);

            var partyMemberPos = partyMember.GetLocation();
            var partyMemberX = partyMemberPos.locx;
            var partyMemberY = partyMemberPos.locy;
            var losRadiusTiles = MaxLosRadius / 3;

            fog_buffer_origin_x[partyIndex] = partyMemberX - losRadiusTiles;
            fog_buffer_origin_y[partyIndex] = partyMemberY - losRadiusTiles;

            var tilerect = new TileRect();
            tilerect.x1 = fog_buffer_origin_x[partyIndex];
            tilerect.x2 = tilerect.x1 + 2 * MaxLosRadius / 3;
            tilerect.y1 = fog_buffer_origin_y[partyIndex];
            tilerect.y2 = tilerect.y1 + 2 * MaxLosRadius / 3;

            using var sectorIterator = new SectorIterator(tilerect);

            while (sectorIterator.HasNext)
            {
                var sector = sectorIterator.Next();
                if (!sector.IsValid)
                {
                    continue;
                }

                var v15 = sector.Loc.GetBaseTile();

                var visibility = GameSystems.SectorVisibility.Lock(sector.Loc);

                var minSectorTileX = Math.Max(0, tilerect.x1 - v15.locx);
                var v22 = minSectorTileX;
                var minSectorTileY = Math.Max(0, tilerect.y1 - v15.locy);
                var v21 = minSectorTileY;
                var maxSectorTileX = Math.Min(Sector.SectorSideSize, tilerect.x2 - v15.locx + 1);
                var maxSectorTileY = Math.Min(Sector.SectorSideSize, tilerect.y2 - v15.locy + 1);

                var v139 = tilerect.x1 * locXY.INCH_PER_TILE;
                var y0 = tilerect.y1 * locXY.INCH_PER_TILE;

                var v33 = MaxLosDiameter * locXY.INCH_PER_SUBTILE;

                for (var tileY = minSectorTileY; tileY < maxSectorTileY; tileY++)
                {
                    for (var tileX = minSectorTileX; tileX < maxSectorTileX; tileX++)
                    {

                        var objects = sector.GetObjectsAt(tileX, tileY);
                        foreach (var obj in objects)
                        {
                            if ( obj.type == ObjectType.portal && obj.IsPortalOpen() )
                            {
                                var v36 = obj.GetOrCreateAnimHandle();
                              if ( v36 != null )
                              {
                                  var animParams = obj.GetAnimParams();

                                  var materials = v36.GetSubmeshes();
                                  for (var submeshIdx = 0; submeshIdx < materials.Length; submeshIdx++)
                                  {
                                      var submesh = v36.GetSubmesh(animParams, submeshIdx);

                                      using var vertexBufferHandle = _vertexPool.Rent(submesh.VertexCount);
                                      var vertexBuffer = vertexBufferHandle.Memory.Span;

                                      var meshVertices = submesh.Positions;
                                      for (var i = 0; i < submesh.VertexCount; i++)
                                      {
                                        vertexBuffer[i].X = (meshVertices[i].X - v139) * MaxLosDiameter / v33;
                                        vertexBuffer[i].Y = (meshVertices[i].Z - y0) * MaxLosDiameter / v33;
                                      }

                                      var primIdx = 0;
                                      var indices = submesh.Indices;
                                      for (var i = 0; i < submesh.PrimitiveCount; i++)
                                      {
                                          Span<Vector2> vertices = stackalloc Vector2[3]
                                          {
                                              vertexBuffer[indices[primIdx++]],
                                              vertexBuffer[indices[primIdx++]],
                                              vertexBuffer[indices[primIdx++]]
                                          };

                                          var v55 = mFogBuffers[partyIndex];
                                          TriangleRasterizer.Rasterize(MaxLosDiameter, MaxLosDiameter, v55, vertices, 8);
                                      }
                                  }
                              }
                            }

                        }

                    }
                }
/*
                v26 = v22 - v15.locx;

                HIDWORD(pXOut) = -(v22 < v15.locx);

                v29 = v21 - pYOut;
                v30 = v14->extent.y;
                v31 = v29 + v30;
                v32 = v26 + v14->extent.x;
                LODWORD(pXOut) = v26;
                v144 = v29;
                v142 = v32;
                v148 = v31;
                if ( v29 < v31 )
                {
                    sector.GetObjectsAt()
                  v118 = 4 * (v26 + (v29 * 64)) + 0x10254;
                  v133 = v31 - v29;
                  do
                  {
                    if ( v26 < v32 )
                    {
                      v121 = v118;
                      v134 = v32 - v26;
                      do
                      {
                        v35 = *(uint32_t *)((char *)&pSectorOut->flags + v121);
                        v125 = *(uint32_t *)((char *)&pSectorOut->flags + v121);
                        if ( v35 )
                        {
                          do
                          {
                            v35 = *(_DWORD *)(v35 + 8);
                          }
                          while ( v35 );
                          v32 = v142;
                          v31 = v148;
                          v29 = v144;
                        }
                        v104 = v134 == 1;
                        v121 += 4;
                        --v134;
                      }
                      while ( !v104 );
                      v26 = pXOut;
                    }
                    v118 += 256;
                    --v133;
                  }
                  while ( v133 );
                }
                v119 = v29;
                if ( v29 < v31 )
                {
                  v134 = 3 * v144;
                  v126 = 16 * (pXOut + (v144 << 6));
                  do
                  {
                    v56 = pXOut;
                    HIDWORD(sectorLoc) = pXOut;
                    if ( (signed int)pXOut < v142 )
                    {
                      v133 = 3 * pXOut;
                      v124 = v126;
                      do
                      {
                        v57 = *(uint32_t *)((char *)&pSectorOut->tiles.tiles[0].tileFlags + v124);
                        if ( v57 & 0x3FE00 )
                        {
                          v58 = v141;
                          if ( BYTE1(v57) & 2 )
                          {
                            v59 = v119;
                            v60 = v119 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v141 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF3C + 2 * v60 + v60)
                            + dword_102ACF18) = 8;
                          }
                          else
                          {
                            v59 = v119;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 1) & 4 )
                          {
                            v61 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF40 + 2 * v61 + v61)
                            + dword_102ACF1C) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 1) & 8 )
                          {
                            v62 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF44 + 2 * v62 + v62)
                            + dword_102ACF20) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 1) & 0x10 )
                          {
                            v63 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF48 + 2 * v63 + v63)
                            + dword_102ACF24) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 1) & 0x20 )
                          {
                            v64 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF4C + 2 * v64 + v64)
                            + dword_102ACF28) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 1) & 0x40 )
                          {
                            v65 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF50 + 2 * v65 + v65)
                            + dword_102ACF2C) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 1) & 0x80 )
                          {
                            v66 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF54 + 2 * v66 + v66)
                            + dword_102ACF30) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 2) & 1 )
                          {
                            v67 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF58 + 2 * v67 + v67)
                            + dword_102ACF34) = 8;
                          }
                          if ( *((_BYTE *)&pSectorOut->tiles.tiles[0].tileFlags + v124 + 2) & 2 )
                          {
                            v68 = v59 + pYOut - LODWORD(fog_buffer_origin_y[partyIndex]);
                            *((_BYTE *)fog_buffers[partyIndex]
                            + 3 * (v56 + v58 - LODWORD(fog_buffer_origin_x[partyIndex]))
                            + MaxLosDiameter * (dword_102ACF5C + 2 * v68 + v68)
                            + dword_102ACF38) = 8;
                          }
                        }
                        v122 = 0;
                        do
                        {
                          v69 = 0;
                          submeshIdx = 0;
                          int y_0 = v122 + v134;
                          do
                          {
                            v70 = COERCE_FLOAT(map_sectorvb_get_subtile(visibility, v69 + v133, y_0));
                            v139 = v70;
                            if ( LOBYTE(v70) & 0xF )
                            {
                              v71 = _longint_mul(
                                      v56
                                    - __PAIR__(HIDWORD(fog_buffer_origin_x[partyIndex]), fog_buffer_origin_x[partyIndex])
                                    + (unsigned int)v141,
                                      3i64);
                              v72 = v71;
                              LODWORD(v71) = v69;
                              v73 = HIDWORD(fog_buffer_origin_y[partyIndex]);
                              v74 = v71 + v72;
                              v150 = ((signed int)v71 + __PAIR__(HIDWORD(v71), v72)) >> 32;
                              v75 = v122
                                  + _longint_mul(
                                      v119 - __PAIR__(v73, fog_buffer_origin_y[partyIndex]) + (unsigned int)pYOut,
                                      3i64);
                              v76 = LOBYTE(v139);
                              v150 = HIDWORD(v75);
                              if ( LOBYTE(v139) & 1 )
                              {
                                v77 = (char *)fog_buffers[partyIndex] + MaxLosDiameter * v75;
                                v77[v74] |= 0x10;
                              }
                              if ( v76 & 2 )
                              {
                                v78 = (char *)fog_buffers[partyIndex] + MaxLosDiameter * v75;
                                v78[v74] |= 0x20;
                              }
                              if ( v76 & 4 )
                              {
                                v79 = (char *)fog_buffers[partyIndex] + MaxLosDiameter * v75;
                                v79[v74] |= 0x40;
                              }
                              v69 = submeshIdx;
                              if ( v76 & 8 )
                              {
                                v80 = (char *)fog_buffers[partyIndex] + MaxLosDiameter * v75 + v74;
                                *v80 |= 0x80u;
                              }
                              v56 = HIDWORD(sectorLoc);
                            }
                            submeshIdx = ++v69;
                          }
                          while ( v69 < 3 );
                          v108 = __OFSUB__(v122 + 1, 3);
                          v105 = v122++ - 2 < 0;
                        }
                        while ( v105 ^ v108 );
                        HIDWORD(sectorLoc) = ++v56;
                        v124 += 16;
                        v133 += 3;
                      }
                      while ( v56 < v142 );
                    }
                    v108 = __OFSUB__(v119 + 1, v148);
                    v105 = v119++ + 1 - v148 < 0;
                    v134 += 3;
                    v126 += 1024;
                  }
                  while ( v105 ^ v108 );
                }

                GameSystems.SectorVisibility.Unlock(sector.Loc);

                v13 = v137;
                v14 = v131;*/
            }

            // fog_perform_fog_checks_1 etc.

        }

        private void MarkExploredSubtiles()
        {
            var sectorLocMin = new SectorLoc(new locXY(mFogMinX, mFogMinY));
            var sectorLocMax = new SectorLoc(new locXY(mFogMinX + mSubtilesX / 3 - 1, mFogMinY + mSubtilesY / 3 - 1));

            for (var secY = sectorLocMin.Y; secY <= sectorLocMax.Y; secY++)
            {
                for (var secX = sectorLocMin.X; secX <= sectorLocMax.X; secX++)
                {
                    var sectorLoc = new SectorLoc(secX, secY);

                    var sectorExploration = GetOrLoadExploredSectorData(sectorLoc);
                    if (sectorExploration.State != SectorExplorationState.Unexplored)
                    {
                        var sectorOriginLoc = sectorLoc.GetBaseTile();

                        var startSubtileX = 3 * (mFogMinX - sectorOriginLoc.locx);
                        var startSubtileY = 3 * (mFogMinY - sectorOriginLoc.locy);
                        var endSubtileX = startSubtileX + mSubtilesX;
                        var endSubtileY = startSubtileY + mSubtilesY;

                        if (startSubtileX < 0)
                        {
                            startSubtileX = 0;
                        }

                        if (startSubtileY < 0)
                        {
                            startSubtileY = 0;
                        }

                        if (endSubtileX > 192)
                        {
                            endSubtileX = 192;
                        }

                        if (endSubtileY > 192)
                        {
                            endSubtileY = 192;
                        }

                        if (startSubtileY < endSubtileY && startSubtileX < endSubtileX)
                        {
                            var fogDataOut = mFogCheckData.AsSpan().Slice(
                                (startSubtileY + 3 * (sectorOriginLoc.locy - mFogMinY)) * mSubtilesX
                                + startSubtileX + 3 * (sectorOriginLoc.locx - mFogMinX)
                            );

                            var fogDataStride = mSubtilesX + startSubtileX - endSubtileX;
                            for (var y = startSubtileY; y < endSubtileY; y++)
                            {
                                var startOfRow = y * fogDataStride;
                                for (var x = startSubtileX; x < endSubtileX; x++)
                                {
                                    if (sectorExploration.IsExplored(x, y))
                                    {
                                        fogDataOut[startOfRow + x] |= 4;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10031E00)]
        private void MarkLineOfSight(int partyIdx)
        {
            // TODO: This needs to be cleaned up
            var v1 = fog_buffer_origin_x[partyIdx];
            var v2 = fog_buffer_origin_y[partyIdx];
            var v3 = 3 * (mFogMinX - v1);
            var v4 = 3 * (mFogMinY - v2);
            var v14 = v3 + mSubtilesX;
            var v16 = v4 + mSubtilesY;

            if (v3 < 0)
            {
                v3 = 0;
            }

            if (v4 < 0)
            {
                v4 = 0;
            }

            if (v14 > MaxLosDiameter)
            {
                v14 = MaxLosDiameter;
            }

            if (v16 > MaxLosDiameter)
            {
                v16 = MaxLosDiameter;
            }

            if (v4 < v16 && v3 < v14)
            {
                var v15 = v3 + mSubtilesX - v14;

                var fogDataOut = mFogCheckData.AsSpan().Slice(
                    3 * (v1 - mFogMinX) + v3
                                        + mSubtilesX * (v4 + 3 * (v2 - mFogMinY))
                );
                var idx = 0;

                ReadOnlySpan<byte> fogBuffer = mFogBuffers[partyIdx];
                var v9 = v4 * MaxLosDiameter + v3;
                var v10 = v3 + MaxLosDiameter - v14;
                var v11 = v14 - v3;
                var v12 = v16 - v4;
                do
                {
                    var v13 = v11;
                    do
                    {
                        fogDataOut[idx++] |= fogBuffer[v9++];
                        --v13;
                    } while (v13 != 0);

                    v9 += v10;
                    idx += v15;
                    --v12;
                } while (v12 != 0);
            }
        }

        [TempleDllLocation(0x1002ec20)]
        public void ResetBuffers()
        {
            InitScreenBuffers();
        }

        [TempleDllLocation(0x1002ECB0)]
        public byte GetFogStatus(locXY loc, float offsetX, float offsetY)
        {
            return 0xFF;
        }

        [TempleDllLocation(0x10030e20)]
        public void SaveExploredTileData(int id)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10030BF0)]
        public void LoadFogColor(string dataDir)
        {
            var path = $"{dataDir}/fog.col";
            if (Tig.FS.FileExists(path))
            {
                using var reader = Tig.FS.OpenBinaryReader(path);
                fogcol_field1 = reader.ReadInt32();
                fogcol_field2 = reader.ReadInt32();
            }
        }

        [TempleDllLocation(0x1002ec90)]
        public void Disable()
        {
            mFoggingEnabled = false;
        }

        [TempleDllLocation(0x1002ec80)]
        public void Enable()
        {
            mFoggingEnabled = true;
        }

        [TempleDllLocation(0x10030d10)]
        public void LoadExploredTileData(string baseDir)
        {
            if (mFoggingEnabled)
            {
                int idx = 0;
                var otherIdx = 0;
                do
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var fileId = i + otherIdx;
                        var path = $"{baseDir}/etd{fileId:D6}";

                        var unexploredData = new MapFogdataChunk();
                        if (Tig.FS.FileExists(path))
                        {
                            using var reader = Tig.FS.OpenBinaryReader(path);
                            unexploredData.AllExplored = reader.ReadByte() != 0;
                            var unexploredRaw = unexploredData.Data.AsSpan();
                            if (unexploredData.AllExplored)
                            {
                                unexploredRaw.Fill(0xFF);
                            }
                            else
                            {
                                if (reader.Read(unexploredRaw) != unexploredRaw.Length)
                                {
                                    throw new InvalidOperationException("Failed to read unexplored sector data.");
                                }
                            }
                        }
                        else
                        {
                            unexploredData.AllExplored = false;
                        }

                        mFogUnexploredData[idx] = unexploredData;
                        ++i;
                        ++idx;
                    }

                    otherIdx += 1000;
                } while (idx < 4);
            }
        }

        [TempleDllLocation(0x1002eca0)]
        public void SetMapDoFoggingUpdate()
        {
            throw new NotImplementedException();
        }
    }
}