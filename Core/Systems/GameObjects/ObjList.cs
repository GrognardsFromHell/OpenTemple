using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.GameObjects;

public struct TileRect
{
    public int x1;
    public int y1;
    public int x2;
    public int y2;

    public SectorEnumerator GetEnumerator()
    {
        return new SectorEnumerator(this);
    }
}

[Flags]
public enum ObjectListFilter
{
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

public class ObjList : IDisposable, IEnumerable<GameObject>
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

    public GameObject this[int index] => _objects.Memory.Span[index];

    public static ObjectListFilter GetFromType(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.portal:
                return ObjectListFilter.OLC_PORTAL;
            case ObjectType.container:
                return ObjectListFilter.OLC_CONTAINER;
            case ObjectType.scenery:
                return ObjectListFilter.OLC_SCROLL;
            case ObjectType.projectile:
                return ObjectListFilter.OLC_PROJECTILE;
            case ObjectType.weapon:
                return ObjectListFilter.OLC_WEAPON;
            case ObjectType.ammo:
                return ObjectListFilter.OLC_AMMO;
            case ObjectType.armor:
                return ObjectListFilter.OLC_ARMOR;
            case ObjectType.money:
                return ObjectListFilter.OLC_MONEY;
            case ObjectType.food:
                return ObjectListFilter.OLC_FOOD;
            case ObjectType.scroll:
                return ObjectListFilter.OLC_SCROLL;
            case ObjectType.key:
                return ObjectListFilter.OLC_KEY;
            case ObjectType.written:
                return ObjectListFilter.OLC_WRITTEN;
            case ObjectType.generic:
                return ObjectListFilter.OLC_GENERIC;
            case ObjectType.pc:
                return ObjectListFilter.OLC_PC;
            case ObjectType.npc:
                return ObjectListFilter.OLC_NPC;
            case ObjectType.trap:
                return ObjectListFilter.OLC_TRAP;
            case ObjectType.bag:
                return ObjectListFilter.OLC_BAG;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

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

    private static readonly MemoryPool<GameObject> MemoryPool = MemoryPool<GameObject>.Shared;

    [TempleDllLocation(0x10808CF8)]
    private static int dword_10808CF8;

    private IMemoryOwner<GameObject> _objects;

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

    [TempleDllLocation(0x100C0CA0)] // kind of...
    private void Add(GameObject obj)
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
    [TempleDllLocation(0x1001ecf0)]
    public static ObjList ListRect(in TileRect tileRect, ObjectListFilter filter)
    {
        Span<bool> returnTypes = stackalloc bool[ObjectTypes.Count];
        CreateTypeFilter(filter, returnTypes);

        var result = new ObjList();

        // When searching for statics, we need to actually iterate the sectors,
        // but if we're only searching for dynamics, we can use the spatial object index.
        // This is especially important for systems like AI, which search for OLC_CRITTER all the time.
        if ((filter & ObjectListFilter.OLC_STATIC) != 0)
        {
            foreach (var partialSector in tileRect)
            {
                foreach (var obj in partialSector.EnumerateObjects())
                {
                    // TODO: Other list functions check against the INVENTORY flag here too, but this one doesnt ?!
                    if (returnTypes[(int) obj.type] && !GameSystems.MapObject.IsHiddenByFlags(obj))
                    {
                        result.Add(obj);
                    }
                }
            }
        }
        else
        {
            var sectorEnumerator = new SectorEnumerator(tileRect, false);
            while (sectorEnumerator.MoveNext())
            {
                foreach (var obj in GameSystems.Object.SpatialIndex.EnumerateInSector(sectorEnumerator.Current
                             .SectorLoc))
                {
                    if (!returnTypes[(int) obj.type])
                    {
                        // This is usually a pretty fast check, so we do it first
                        continue;
                    }

                    if (obj.IsStatic())
                    {
                        continue;
                    }

                    var objLoc = obj.GetLocation();
                    if (objLoc.locx < tileRect.x1 || objLoc.locx > tileRect.x2
                                                  || objLoc.locy < tileRect.y1 || objLoc.locy > tileRect.y2)
                    {
                        // When the tile rect partially covers the sector, objects may still not be in it
                        continue;
                    }

                    if (!obj.HasFlag(ObjectFlag.INVENTORY) && !GameSystems.MapObject.IsHiddenByFlags(obj))
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
        I believe this searches for all objects that would be visible if the screen was
        centered on the given tile.
    */
    [TempleDllLocation(0x1001f1c0)]
    public static ObjList ListVicinity(locXY loc, ObjectListFilter filter)
    {
        if (GameViews.Primary == null)
        {
            return new ObjList();
        }

        var screenSize = GameViews.Primary.Camera.ViewportSize;
        var screenRect = new Rectangle(Point.Empty, screenSize);
        GameSystems.Location.ScreenToLoc(screenSize.Width / 2, screenSize.Height / 2, out var screenCenter);
        GameSystems.Location.GetTranslation(screenCenter.locx, screenCenter.locy, out var screenCenterX,
            out var screenCenterY);
        GameSystems.Location.GetTranslation(loc.locx, loc.locy, out var locScreenX, out var locScreenY);

        // screenRect now becomes "shifted" by half the screen's size. Why the screen size is not used directly,
        // i don't know...
        screenRect.X = locScreenX - screenCenterX;
        screenRect.Y = locScreenY - screenCenterY;
        if (GameSystems.Location.GetVisibleTileRect(GameViews.Primary, in screenRect, out var tileRect))
        {
            return ListRect(tileRect, filter);
        }
        else
        {
            return new ObjList();
        }
    }

    /// <summary>
    /// Same as <see cref="ListVicinity(locXY,ObjectListFilter)"/>
    /// but uses the location of the given object.
    /// </summary>
    [TempleDllLocation(0x100211d0)]
    public static ObjList ListVicinity(GameObject obj, ObjectListFilter flags)
    {
        return ListVicinity(obj.GetLocation(), flags);
    }

    /// <summary>
    /// Lists objects in a radius. This seems to be the radius in the X,Y 3D coordinate space.
    /// </summary>
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
    Lists objects in a tile radius.
    */
    [TempleDllLocation(0x100591f0)]
    public static ObjList ListRangeTiles(GameObject obj, int rangeTiles, ObjectListFilter filter)
    {
        var location = obj.GetLocationFull();
        var rangeInches = rangeTiles * locXY.INCH_PER_TILE;
        return ListRadius(location, rangeInches, filter);
    }

    /*
        Lists objects in a cone. This seems to be the radius in the X,Y 3D coordinate
        space.
    */
    [TempleDllLocation(0x10022e50)]
    public static ObjList ListCone(LocAndOffsets loc, float radiusInches, float coneStartAngleRad, float coneArcRad,
        ObjectListFilter flags)
    {
        Span<bool> returnTypes = stackalloc bool[ObjectTypes.Count];
        CreateTypeFilter(flags, returnTypes);

        var center = loc.ToInches2D();
        var topLeft = LocAndOffsets.FromInches(center - radiusInches * Vector2.One);
        var bottomRight = LocAndOffsets.FromInches(center + radiusInches * Vector2.One);

        var intersector = new Cone2dIntersectionTester(center, radiusInches,
            coneStartAngleRad, coneArcRad);

        var result = new ObjList();
        using var iterator = new SectorIterator(topLeft.location.locx, bottomRight.location.locx,
            topLeft.location.locy, bottomRight.location.locy);

        foreach (var obj in iterator.EnumerateObjects())
        {
            if (!intersector.Intersects(obj.GetWorldPos(), obj.GetRadius()))
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

    [TempleDllLocation(0x1010d740)]
    public static ObjList ListCone(GameObject critter, ObjectListFilter flags, float radiusFeet,
        float coneStartAngleDeg, float coneArcDeg)
    {
        // TODO: Should this not be extended by the critter's radius?
        var radiusInches = radiusFeet * locXY.INCH_PER_FEET;
        var coneStart = critter.Rotation + Angles.ToRadians(coneStartAngleDeg);
        var coneWidth = Angles.ToRadians(coneArcDeg);
        var location = critter.GetLocationFull();

        return ListCone(location, radiusInches, coneStart, coneWidth, flags);
    }

    public IEnumerator<GameObject> GetEnumerator()
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