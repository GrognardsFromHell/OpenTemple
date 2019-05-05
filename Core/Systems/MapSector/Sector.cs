using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.MapSector
{
    public struct SectorLightPartSys
    {
        public int hashCode;
        public int handle;
    };

    public struct SectorLightNight
    {
        public int type;
        public PackedLinearColorA color;
        public Vector3 direction;
        public uint padding;
        public float phi;
        public SectorLightPartSys partSys;
    };

    public struct SectorLight
    {
        public GameObjectBody obj;
        public int flags; // 0x40 -> light2 is present
        public int type;
        public PackedLinearColorA color;
        public int field14;
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
        public bool enabled;
    }

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
        public int field00;
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
        public int field0;
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
    }

    public class SectorTilePacket
    {
        public SectorTile[] tiles = new SectorTile[Sector.TilesPerSector];

        /// this is probably a 64x64 bitmap, designating some tile state (changed? valid?)
        public byte[] unk10000 = new byte[Sector.TilesPerSector / 8];

        public int changedFlagMaybe; // probably a worlded thing
    }

    public class SectorObjects
    {
        public List<GameObjectBody>[,] tiles = new List<GameObjectBody>[Sector.SectorSideSize, Sector.SectorSideSize];
        public bool staticObjsDirty;
        public int objectsRead;
    }

    public class Sector
    {
        // this is the number of tiles per sector in each direction (so the total is this squared i.e. 4096 in toee)
        public const int SectorSideSize = 64;

        public const int TilesPerSector = SectorSideSize * SectorSideSize;

        // 1 - townmapinfo  2 - aptitude  4 - lightscheme
        public int flags;
        public int field4;
        public SectorLoc secLoc;
        public GameTime timeElapsed;
        public SectorLights lights;
        public SectorTilePacket tilePkt;
        public bool tileScriptsDirty;
        public SectorTileScript[] tileScripts;
        public SectorScript sectorScript;
        public int townmapinfo;
        public int aptitudeAdj;
        public int lightScheme;
        public SectorSoundList soundList;
        public SectorObjects objects;

        public int field1425C;

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
    }
}