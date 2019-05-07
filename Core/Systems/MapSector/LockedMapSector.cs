using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.MapSector
{
    public struct LockedMapSector : IDisposable
    {
        [TempleDllLocation(0x10082700)]
        public LockedMapSector(int secX, int secY) : this(new SectorLoc(secX, secY))
        {
        }

        [TempleDllLocation(0x10082700)]
        public LockedMapSector(SectorLoc loc)
        {
            Loc = loc;
            Sector = GameSystems.MapSector.LockSector(loc);
        }

        public LockedMapSector(locXY tileLocation) : this(new SectorLoc(tileLocation))
        {
        }

        public SectorLoc Loc { get; }

        public Sector Sector { get; private set; }

        public Span<SectorLight> Lights
        {
            get
            {
                if (Sector == null)
                {
                    return Span<SectorLight>.Empty;
                }
                return Sector.lights.list;
            }
        }

        public IList<GameObjectBody> GetObjectsAt(int x, int y)
        {
            Trace.Assert(x >= 0 && x < Sector.SectorSideSize);
            Trace.Assert(y >= 0 && y < Sector.SectorSideSize);

            if (Sector == null)
            {
                return Array.Empty<GameObjectBody>();
            }

            var tiles = Sector.objects.tiles[x, y];
            if (tiles == null)
            {
                return Array.Empty<GameObjectBody>();
            }

            return tiles;
        }

        [TempleDllLocation(0x100c1ad0)]
        public void AddObject(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GameSystems.MapSector.UnlockSector(Loc, Sector);
            Sector = null;
        }
    }
}