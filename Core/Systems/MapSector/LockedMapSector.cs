using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.MapSector
{
    public class LockedMapSector : IDisposable
    {
#if DEBUG
        private readonly StackTrace _stackTrace;
#endif

        [TempleDllLocation(0x10082700)]
        public LockedMapSector(int secX, int secY) : this(new SectorLoc(secX, secY))
        {
        }

        [TempleDllLocation(0x10082700)]
        public LockedMapSector(SectorLoc loc)
        {
            Loc = loc;
            Sector = GameSystems.MapSector.LockSector(loc);
#if DEBUG
            _stackTrace = new StackTrace(true);
#endif
        }

        public LockedMapSector(locXY tileLocation) : this(new SectorLoc(tileLocation))
        {
        }

        ~LockedMapSector()
        {
            if (Sector != null)
            {
                Debugger.Break();
            }
        }

        public SectorLoc Loc { get; }

        public Sector Sector { get; private set; }

        public bool IsValid => Sector != null;

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

        public IEnumerable<GameObjectBody> EnumerateObjects()
        {
            if (Sector == null)
            {
                yield break;
            }

            for (var tx = 0; tx < 64; ++tx)
            {
                for (var ty = 0; ty < 64; ++ty)
                {
                    var objects = Sector.objects.tiles[tx, ty];
                    if (objects == null)
                    {
                        continue;
                    }

                    foreach (var obj in objects)
                    {
                        yield return obj;
                    }
                }
            }
        }

        public IList<GameObjectBody> GetObjectsAt(int x, int y)
        {
            Debug.Assert(x >= 0 && x < Sector.SectorSideSize);
            Debug.Assert(y >= 0 && y < Sector.SectorSideSize);

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
            if (Sector != null)
            {
                Sector.objects.Insert(obj);
                Sector.objects.staticObjsDirty = true;

                GameSystems.MapObject.StartAnimating(obj);
            }
            else
            {
                // Have to check what vanilla did here in case the sector doesnt actually exist...
                Stub.TODO();
            }
        }

        [TempleDllLocation(0x100c1930)]
        public void RemoveObject(GameObjectBody obj)
        {
            Sector?.objects.Remove(obj);
        }

        [TempleDllLocation(0x100c1990)]
        public void UpdateObjectPos(GameObjectBody obj, LocAndOffsets pos)
        {
            if (Sector == null)
            {
                return;
            }

            if (Sector.objects.Remove(obj))
            {
                obj.SetLocationFull(pos);
                Sector.objects.Insert(obj);
            }
        }

        public void Dispose()
        {
            if (Sector != null)
            {
                GameSystems.MapSector.UnlockSector(Loc, Sector);
            }

            Sector = null;
        }
    }
}