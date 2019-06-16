using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public struct TileRect
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
    }

    [Flags]
    public enum ObjectListFilter {
        OLC_NONE = 0,
        OLC_PORTAL = 2,
        OLC_CONTAINER = 4,
        OLC_SCENERY = 8,
        OLC_PROJECTILE = 0x10,
        OLC_WEAPON = 0x20,
        OLC_AMMO = 0x40,
        OLC_ARMOR = 0x80,
        OLC_MONEY = 0x100,
        OLC_FOOD = 0x200,
        OLC_SCROLL = 0x400,
        OLC_KEY = 0x800,
        OLC_BAG = 0x1000,
        OLC_WRITTEN = 0x2000,
        OLC_GENERIC = 0x4000,
        OLC_ITEMS = 0x7FE0,
        OLC_PC = 0x8000,
        OLC_NPC = 0x10000,
        OLC_CRITTERS = 0x18000,
        OLC_MOBILE = 0x1FFF4,
        OLC_TRAP = 0x20000,
        OLC_STATIC = 0x2000A,
        OLC_ALL = 0x3FFFE,
        OLC_PATH_BLOCKER = 0x18006 // added for pathfinding purposes
    }

    public class ObjList : IDisposable, IEnumerable<GameObjectBody>
    {
        private ObjList()
        {
        }

        public void Dispose()
        {
            _objects?.Dispose();
            _objects = null;
            dword_10808CF8--;
        }

        public int Count => _objectsCount;

        public GameObjectBody this[int index] => _objects.Memory.Span[index];

        private static void CreateTypeFilter(ObjectListFilter flags, Span<bool> typeFilter)
        {
            Debug.Assert(typeFilter.Length == ObjectTypes.Count);

            if (flags.HasFlag(ObjectListFilter.OLC_PORTAL))
            {
                typeFilter[(int) ObjectType.portal] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_CONTAINER))
            {
                typeFilter[(int) ObjectType.container] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_SCENERY))
            {
                typeFilter[(int) ObjectType.scenery] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_PROJECTILE))
            {
                typeFilter[(int) ObjectType.projectile] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_WEAPON))
            {
                typeFilter[(int) ObjectType.weapon] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_AMMO))
            {
                typeFilter[(int) ObjectType.ammo] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_ARMOR))
            {
                typeFilter[(int) ObjectType.armor] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_MONEY))
            {
                typeFilter[(int) ObjectType.money] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_FOOD))
            {
                typeFilter[(int) ObjectType.food] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_SCROLL))
            {
                typeFilter[(int) ObjectType.scroll] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_KEY))
            {
                typeFilter[(int) ObjectType.key] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_BAG))
            {
                typeFilter[(int) ObjectType.bag] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_WRITTEN))
            {
                typeFilter[(int) ObjectType.written] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_GENERIC))
            {
                typeFilter[(int) ObjectType.generic] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_PC))
            {
                typeFilter[(int) ObjectType.pc] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_NPC))
            {
                typeFilter[(int) ObjectType.npc] = true;
            }
            if (flags.HasFlag(ObjectListFilter.OLC_TRAP))
            {
                typeFilter[(int) ObjectType.trap] = true;
            }
        }

        private static readonly MemoryPool<GameObjectBody> MemoryPool = MemoryPool<GameObjectBody>.Shared;

        [TempleDllLocation(0x10808CF8)]
        private static int dword_10808CF8;

        private IMemoryOwner<GameObjectBody> _objects;

        private int _objectsCount;

        private void EnsureCapacity(int count)
        {
            if (_objects != null && _objects.Memory.Length >= count)
            {
                return;
            }

            if (_objectsCount == 0)
            {
                _objects?.Dispose();
                _objects = MemoryPool.Rent(count);
                return;
            }

            var oldObjects = _objects;
            _objects = MemoryPool.Rent(count);
            if (oldObjects != null)
            {
                oldObjects.Memory.Slice(0, _objectsCount).CopyTo(_objects.Memory);
                oldObjects.Dispose();
            }
        }

        private void Add(GameObjectBody obj)
        {
            EnsureCapacity(_objectsCount + 1);
            _objects.Memory.Span[_objectsCount++] = obj;
        }

        /*
            Searches for everything on a single tile that matches the given search flags.
        */
        [TempleDllLocation(0x1001E970)]
        public static ObjList ListTile(locXY loc, ObjectListFilter flags)
        {
            Span<bool> returnTypes = stackalloc bool[ObjectTypes.Count];
            CreateTypeFilter(flags, returnTypes);

            var result = new ObjList();

            var sectorLoc = new SectorLoc(loc);

            if ((flags & ObjectListFilter.OLC_STATIC) != 0 || GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var lockedSector = new LockedMapSector(sectorLoc);

                Sector.GetSectorTileCoords(loc, out var tileX, out var tileY);

                var objects = lockedSector.GetObjectsAt(tileX, tileY);
                result.EnsureCapacity(objects.Count);
                foreach (var obj in objects)
                {
                    if (!GameSystems.MapObject.IsHiddenByFlags(obj) && returnTypes[(int) obj.type])
                    {
                        result.Add(obj);
                    }
                }
            }
            else
            {
                foreach (var obj in GameSystems.Object.SpatialIndex.EnumerateInSector(sectorLoc))
                {
                    if (!obj.type.IsStatic())
                    {
                        if (!obj.HasFlag(ObjectFlag.INVENTORY)
                            && obj.GetLocation() == loc
                            && !GameSystems.MapObject.IsHiddenByFlags(obj)
                            && returnTypes[(int) obj.type])
                            {
                                result.Add(obj);
                            }
                    }
                }
            }

            ++dword_10808CF8;
            return result;
        }

        /*
            search within worldspace rect
        */
        public static ObjList ListRect(in TileRect trect, ObjectListFilter olcCritters) {throw new NotImplementedException();}

        /*
            I believe this searches for all objects that would be visible if the screen was
            centered on the given tile.
        */
        public static ObjList  ListVicinity(locXY loc, ObjectListFilter flags) {throw new NotImplementedException();}
        public static ObjList  ListVicinity(GameObjectBody handle, ObjectListFilter flags) {throw new NotImplementedException();} // using the object's location

        /*
            Lists objects in a radius. This seems to be the radius in the X,Y 3D coordinate
            space.
        */
        [TempleDllLocation(0x10022E50)]
        public static ObjList ListRadius(LocAndOffsets loc, float radiusInches, ObjectListFilter flags)
        {
            Span<bool> returnTypes = stackalloc bool[ObjectTypes.Count];
            CreateTypeFilter(flags, returnTypes);

            var radiusSquared = radiusInches * radiusInches;
            var center = loc.ToInches2D();
            var topLeft = LocAndOffsets.FromInches(center - radiusInches * Vector2.One);
            var bottomRight = LocAndOffsets.FromInches(center + radiusInches * Vector2.One);

            var result = new ObjList();
            using var iterator = new SectorIterator(topLeft.location.locx, bottomRight.location.locx,
                topLeft.location.locy, bottomRight.location.locy);

            foreach (var obj in iterator.EnumerateObjects())
            {
                var objCenter = obj.GetLocationFull().ToInches2D();
                var distanceSquared = (objCenter - center).LengthSquared() - obj.GetRadius();
                if (distanceSquared > radiusSquared)
                {
                    continue;
                }

                if (obj.HasFlag(ObjectFlag.INVENTORY)
                    || GameSystems.MapObject.IsHiddenByFlags(obj)
                    || !returnTypes[(int) obj.type])
                {
                    continue;
                }

                result.Add(obj);
            }

            return result;
        }

        /*
        Lists objects in a radius + angles. This seems to be the radius in the X,Y 3D coordinate
        space. flags - ObjectListFilter
        */
        public static ObjList  ListRange(LocAndOffsets loc, float radius, float angleMin, float angleMax, ObjectListFilter flags) {throw new NotImplementedException();}

        /*
        Lists objects in a tile radius.
        */
        public static ObjList  ListRangeTiles(GameObjectBody handle, int rangeTiles, ObjectListFilter filter) {throw new NotImplementedException();}


        /*
            Lists objects in a cone. This seems to be the radius in the X,Y 3D coordinate
            space.
        */
        public static ObjList  ListCone(LocAndOffsets loc, float radius, float coneStartAngleRad, float coneArcRad, ObjectListFilter flags) {throw new NotImplementedException();}

        /*
            Lists all followers (and their followers).
        */
        public static ObjList  ListFollowers(GameObjectBody critter) {throw new NotImplementedException();}


        public IEnumerator<GameObjectBody> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }
    }
}