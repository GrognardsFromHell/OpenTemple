using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.MapSector;

public struct SectorLightPartSys
{
    public int hashCode;
    public object handle;
};

public struct SectorLightNight
{
    public int type;
    public LinearColor color;
    public Vector3 direction;
    public float range;
    public float phi;
    public SectorLightPartSys partSys;
};

public struct SectorLight
{
    public GameObject obj;
    public int flags; // 0x40 -> light2 is present
    public int type;
    public LinearColor color;
    public LocAndOffsets position;
    public float offsetZ;
    public Vector3 direction;
    public float range;
    public float phi;
    public SectorLightPartSys partSys;
    public SectorLightNight light2;
}

public struct SectorLights
{
    public SectorLight[] list;
    public bool dirty;
}

[Flags]
public enum TileFlags : uint
{
    TILEFLAG_NONE = 0,
    TF_Blocks = 1, // the range of flags 0x1 to 0x100 are obsolete / arcanum leftovers
    TF_Sinks = 2,
    TF_CanFlyOver = 4,
    TF_Icy = 8,
    TF_Natural = 0x10,
    TF_SoundProof = 0x20,
    TF_Indoor = 0x40,
    TF_Reflective = 0x80,
    TF_BlocksVision = 0x100, // up to here is obsolete
    BlockX0Y0 = 0x200,
    BlockX1Y0 = 0x400,
    BlockX2Y0 = 0x800,
    BlockX0Y1 = 0x1000,
    BlockX1Y1 = 0x2000,
    BlockX2Y1 = 0x4000,
    BlockX0Y2 = 0x8000,
    BlockX1Y2 = 0x10000,
    BlockX2Y2 = 0x20000,

    BlockMask = BlockX0Y0 | BlockX1Y0 | BlockX2Y0
                | BlockX0Y1 | BlockX1Y1 | BlockX2Y1
                | BlockX0Y2 | BlockX1Y2 | BlockX2Y2,

    FlyOverX0Y0 =
        0x40000, //indices denote the subtile locations (using the same axis directions as the normal tiles)
    FlyOverX1Y0 = 0x80000,
    FlyOverX2Y0 = 0x100000,
    FlyOverX0Y1 = 0x200000,
    FlyOverX1Y1 = 0x400000,
    FlyOverX2Y1 = 0x800000,
    FlyOverX0Y2 = 0x1000000,
    FlyOverX1Y2 = 0x2000000,
    FlyOverX2Y2 = 0x4000000,

    FlyOverMask = FlyOverX0Y0 | FlyOverX1Y0 | FlyOverX2Y0
                  | FlyOverX0Y1 | FlyOverX1Y1 | FlyOverX2Y1
                  | FlyOverX0Y2 | FlyOverX1Y2 | FlyOverX2Y2,

    ProvidesCover = 0x8000000, //applies to the whole tile apparently
    TF_10000000 = 0x10000000,
    TF_20000000 = 0x20000000,
    TF_40000000 = 0x40000000,
    TF_80000000 = 0x80000000
};

public enum TileMaterial : byte
{
    ReservedBlocked = 0,
    ReservedFlyOver = 1,
    Dirt = 2,
    Grass = 3, // Default
    Water = 4,
    DeepWater = 5,
    Ice = 6,
    Fire = 7,
    Wood = 8,
    Stone = 9,
    Metal = 10,
    Marsh = 11
}

public struct SectorTileScript
{
    /// Dirty flag most likely
    public bool dirty;

    public int tileIndex;
    public int scriptUnk1;
    public uint scriptCounters;
    public int scriptId;

    private sealed class TileIndexRelationalComparer : IComparer<SectorTileScript>
    {
        public int Compare(SectorTileScript x, SectorTileScript y)
        {
            return x.tileIndex.CompareTo(y.tileIndex);
        }
    }

    public static IComparer<SectorTileScript> TileIndexComparer { get; } = new TileIndexRelationalComparer();
}

public struct SectorScript
{
    public bool dirty;

    // These fields are equivalent to ObjectScript
    public int data1;
    public uint data2;
    public int data3;
}

public struct SectorSoundList
{
    public int field00;
    public int scheme1;
    public int scheme2;
}

public struct SectorTile
{
    public TileMaterial material; // for footsteps
    public byte padding1;
    public byte padding2;
    public byte padding3;
    public TileFlags flags;
    public uint padding4;
    public uint padding5;

    public static TileFlags GetBlockingFlag(int subtileX, int subtileY)
    {
        var baseFlag = (uint) TileFlags.BlockX0Y0;
        baseFlag <<= subtileX + subtileY * 3;
        return (TileFlags) baseFlag;
    }

    public static TileFlags GetFlyOverFlag(int subtileX, int subtileY)
    {
        var baseFlag = (uint) TileFlags.FlyOverX0Y0;
        baseFlag <<= subtileX + subtileY * 3;
        return (TileFlags) baseFlag;
    }
}

public class SectorTilePacket
{
    public SectorTile[] tiles = new SectorTile[Sector.TilesPerSector];
}

public class SectorObjects : IDisposable, IEnumerable<GameObject>
{
    public List<GameObject>[,] tiles = new List<GameObject>[Sector.SectorSideSize, Sector.SectorSideSize];
    public bool staticObjsDirty;
    public int objectsRead;

    [TempleDllLocation(0x100c1740)]
    public void Insert(GameObject obj)
    {
        var locFull = obj.GetLocationFull();

        Sector.GetSectorTileCoords(locFull.location, out var tileX, out var tileY);

        ref var objectList = ref tiles[tileX, tileY];
        if (objectList == null)
        {
            objectList = new List<GameObject> {obj};
            return;
        }
        else if (objectList.Count == 0)
        {
            objectList.Add(obj);
            return;
        }

        // TODO: All of this sorting is most likely unnecessary since it's only interesting for 2D most of the time

        var newXOffset = locFull.off_x;
        var newYOffset = locFull.off_y;
        var newObjIsFlat = obj.HasFlag(ObjectFlag.FLAT);

        int insertionIndex = 0;
        while (true)
        {
            var oldObjIsFlat = objectList[insertionIndex].HasFlag(ObjectFlag.FLAT);
            if (newObjIsFlat)
            {
                if (!oldObjIsFlat)
                    break;
                if (obj.type == ObjectType.scenery)
                {
                    var v10 = objectList[insertionIndex].GetSceneryFlags();
                    if (v10.HasFlag(SceneryFlag.UNDER_ALL))
                    {
                        objectList.Insert(insertionIndex, obj);
                        return;
                    }
                }
            }

            if (!oldObjIsFlat)
            {
                var existingXOffset = objectList[insertionIndex].GetFloat(obj_f.offset_x);
                var existingYOffset = objectList[insertionIndex].GetFloat(obj_f.offset_y);
                if (newYOffset < existingYOffset)
                {
                    objectList.Insert(insertionIndex, obj);
                    return;
                }

                if ((newYOffset == existingYOffset) && (newXOffset < existingXOffset))
                {
                    break;
                }
            }

            if (++insertionIndex >= objectList.Count)
            {
                objectList.Add(obj);
                return;
            }
        }

        objectList.Insert(insertionIndex, obj);
    }

    public bool Remove(GameObject obj)
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                ref var objList = ref tiles[x, y];
                if (objList != null)
                {
                    for (var i = objList.Count - 1; i >= 0; i--)
                    {
                        if (objList[i] == obj)
                        {
                            objList.RemoveAt(i);
                            if (objList.Count == 0)
                            {
                                objList = null;
                            }

                            if (obj.IsStatic())
                            {
                                staticObjsDirty = true;
                            }

                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    [TempleDllLocation(0x100c1360)]
    public void Dispose()
    {
        if (tiles == null)
        {
            return;
        }

        foreach (var objList in tiles)
        {
            if (objList == null)
            {
                continue;
            }

            foreach (var obj in objList)
            {
                GameSystems.Light.RemoveAttachedTo(obj);

                if (obj.IsStatic())
                {
                    GameSystems.MapObject.RemoveMapObj(obj);
                }
                else
                {
                    obj.DestroyRendering();
                }
            }
        }

        tiles = null;
    }

    public IEnumerator<GameObject> GetEnumerator()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                var objList = tiles[x, y];
                if (objList != null)
                {
                    for (var i = objList.Count - 1; i >= 0; i--)
                    {
                        yield return objList[i];
                    }
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class Sector
{
    // this is the number of tiles per sector in each direction (so the total is this squared i.e. 4096 in toee)
    public const int SectorSideSize = 64;

    public const int TilesPerSector = SectorSideSize * SectorSideSize;

    // 1 - townmapinfo  2 - aptitude  4 - lightscheme
    public int flags;
    public SectorLoc secLoc { get; }
    public GameTime timeElapsed;
    public SectorLights lights;
    public SectorTilePacket tilePkt;
    public bool tileScriptsDirty;
    public SectorTileScript[] tileScripts;
    public SectorScript sectorScript;
    public int townmapInfo; // TODO I think this shit is unused along with the entire townmap gamesystem
    public int aptitudeAdj;
    public int lightScheme;
    public SectorSoundList soundList;
    public SectorObjects objects;
    /// <summary>
    /// Stores the game objects read from the sector file in exactly the order in which they were read.
    /// This is used to make storing object diffs easier (since they are matched by order).
    /// </summary>
    public ImmutableArray<GameObject> StaticObjects { get; set; }

    public Sector(SectorLoc loc)
    {
        secLoc = loc;
        lights.list = Array.Empty<SectorLight>();
        StaticObjects = ImmutableArray<GameObject>.Empty;
    }

    /// <summary>
    /// Gets the sector-local coordinates of a given tile.
    /// </summary>
    public static void GetSectorTileCoords(locXY tile, out int sectorTileX, out int sectorTileY)
    {
        sectorTileX = tile.locx % SectorSideSize;
        sectorTileY = tile.locy % SectorSideSize;
    }

    /*
    return an offset for getting a proper index in the TilePacket
    */
    public int GetTileOffset(in locXY loc)
    {
        var baseLoc = secLoc.GetBaseTile();
        return loc.locx - baseLoc.locx + SectorSideSize * (loc.locy - baseLoc.locy);
    }

    public TileFlags GetTileFlags(in locXY loc)
    {
        int tileOffset = GetTileOffset(loc);
        return tilePkt.tiles[tileOffset].flags;
    }

    public static int GetSectorTileIndex(int x, int y)
    {
        return x % SectorSideSize + SectorSideSize * (y % SectorSideSize);
    }

    public static void GetSectorTileFromIndex(int idx, out int x, out int y)
    {
        x = idx % SectorSideSize;
        y = idx / SectorSideSize;
    }

    public bool HasLightChanges
    {
        [TempleDllLocation(0x10105e00)]
        get
        {
            if (lights.dirty)
            {
                return true;
            }

            foreach (var sectorLight in lights.list)
            {
                if (sectorLight.obj == null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool HasStaticObjectChanges
    {
        [TempleDllLocation(0x100c14b0)]
        get
        {
            if (objects.staticObjsDirty)
            {
                return true;
            }

            foreach (var obj in StaticObjects)
            {
                if (obj.hasDifs)
                {
                    return true;
                }
            }

            return false;
        }
    }
}