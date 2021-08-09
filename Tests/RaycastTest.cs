using System;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Raycast;
using NUnit.Framework;

namespace OpenTemple.Tests
{
    public class RaycastTest
    {
        [Test]
        public void TestRaycast()
        {

            var origin = new LocAndOffsets(new locXY(496, 458), -9.42809f, -8.42809f);
            var target = new LocAndOffsets(new locXY(512, 611), -7.42809f, -7.42809f);
            var radius = 17.5f;

            var ray_search = new RaycastPointSearchPacket();
            ray_search.origin = origin.ToInches2D();
            ray_search.target = target.ToInches2D();
            ray_search.radius = radius;

            ray_search.direction = ray_search.target - ray_search.origin;
            ray_search.rangeInch = ray_search.direction.Length();
            ray_search.direction /= ray_search.rangeInch;

            ray_search.absOdotU = Vector2.Dot(ray_search.direction, ray_search.origin);

            var locBeforeCover = target;
            SearchPointAlongRay search_func = RaycastPacket.IsPointCloseToSegment;

            var tileRadius = 1 + (int) (radius / locXY.INCH_PER_TILE);
            TestContext.Out.WriteLine(tileRadius.ToString());
            var tile_rect = RaycastPacket.BuildSearchRectangle(ray_search.origin, ray_search.target);

            var canFly = false;

            foreach (var s in tile_rect)
            {
                //int left = Math.Max(0, s.TileRectangle.Left - tileRadius);
                //int top = Math.Max(0, s.TileRectangle.Top - tileRadius);
                //int right = Math.Min(64, s.TileRectangle.Right + tileRadius);
                //int bottom = Math.Min(64, s.TileRectangle.Bottom + tileRadius);

                int left = s.TileRectangle.Left;
                int top = s.TileRectangle.Top;
                int right = s.TileRectangle.Right;
                int bottom = s.TileRectangle.Bottom;

                TestContext.Out.WriteLine($"{s.SectorLoc.X} {s.SectorLoc.Y} {left},{top} {right},{bottom}");
            }
            TestContext.Out.WriteLine("OLD OLD OLD");

            if (!PreciseSectorRows.Build(tile_rect, out var sector_tiles))
            {
                return;
            }

            var local_254 = Math.Min(origin.location.locx, target.location.locx);
            var local_25c = Math.Max(origin.location.locx, target.location.locx);
            var local_2a4 = Math.Min(origin.location.locy, target.location.locy);
            var local_2b4 = Math.Max(origin.location.locy, target.location.locy);
/*
            local_254 -= tileRadius;
            local_2a4 -= tileRadius;
            local_25c += tileRadius;
            local_2b4 += tileRadius;
*/
            for (var local_2dc = 0; local_2dc < sector_tiles.RowCount; local_2dc++)
            {
                ref var pPVar2 = ref sector_tiles.Rows[local_2dc];

                Span<int> local_208 = stackalloc int[pPVar2.colCount];
                Span<int> local_1d8 = stackalloc int[pPVar2.colCount];

                for (var i = 0; i < pPVar2.colCount; i++)
                {
                    local_208[i] = pPVar2.startTiles[i];
                    local_1d8[i] = 64 - pPVar2.strides[i];
                }

                for (var local_2f0 = 0; local_2f0 < pPVar2.colCount; local_2f0++)
                {
                    var sectorBaseTile = pPVar2.sectors[local_2f0].GetBaseTile();
                    var sectorTileMinX = local_254 - sectorBaseTile.locx;
                    var sectorTileMaxX = local_25c - sectorBaseTile.locx;
                    var sectorTileMinY = local_2a4 - sectorBaseTile.locy;
                    var sectorTileMaxY = local_2b4 - sectorBaseTile.locy;
                    if (sectorTileMinX < 0)
                    {
                        sectorTileMinX = 0;
                    }

                    if (sectorTileMinY < 0)
                    {
                        sectorTileMinY = 0;
                    }

                    if (sectorTileMaxX > 63)
                    {
                        sectorTileMaxX = 63;
                    }

                    if (sectorTileMaxY > 63)
                    {
                        sectorTileMaxY = 63;
                    }

                    TestContext.Out.WriteLine($"{pPVar2.sectors[local_2f0].X} {pPVar2.sectors[local_2f0].Y} {sectorTileMinX},{sectorTileMinY}->{sectorTileMaxX},{sectorTileMaxY}");
                }
            }
        }
    }
}