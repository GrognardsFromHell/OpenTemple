using System;
using SpicyTemple.Core.Location;

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
        OLC_IMMOBILE = 0x2000A,
        OLC_ALL = 0x3FFFE,
        OLC_PATH_BLOCKER = 0x18006 // added for pathfinding purposes
    }

    public class ObjList : IDisposable
    {
        private ObjList()
        {
        }

        public void Dispose()
        {
        }

        public int Count => 0; // TODO

        public ObjHndl this[int index] => ObjHndl.Null; // TODO

        /*
            Searches for everything on a single tile that matches the given search flags.
        */
        public static ObjList ListTile(locXY loc, ObjectListFilter flags) {throw new NotImplementedException();}

        /*
            search within worldspace rect
        */
        public static ObjList ListRect(in TileRect trect, ObjectListFilter olcCritters) {throw new NotImplementedException();}

        /*
            I believe this searches for all objects that would be visible if the screen was
            centered on the given tile.
        */
        public static ObjList  ListVicinity(locXY loc, int flags) {throw new NotImplementedException();}
        public static ObjList  ListVicinity(ObjHndl handle, int flags) {throw new NotImplementedException();} // using the object's location

        /*
            Lists objects in a radius. This seems to be the radius in the X,Y 3D coordinate
            space.
        */
        public static ObjList  ListRadius(LocAndOffsets loc, float radiusInches, int flags) {throw new NotImplementedException();}

        /*
        Lists objects in a radius + angles. This seems to be the radius in the X,Y 3D coordinate
        space. flags - ObjectListFilter
        */
        public static ObjList  ListRange(LocAndOffsets loc, float radius, float angleMin, float angleMax, int flags) {throw new NotImplementedException();}

        /*
        Lists objects in a tile radius.
        */
        public static ObjList  ListRangeTiles(ObjHndl handle, int rangeTiles, ObjectListFilter filter) {throw new NotImplementedException();}


        /*
            Lists objects in a cone. This seems to be the radius in the X,Y 3D coordinate
            space.
        */
        public static ObjList  ListCone(LocAndOffsets loc, float radius, float coneStartAngleRad, float coneArcRad, int flags) {throw new NotImplementedException();}

        /*
            Lists all followers (and their followers).
        */
        public static ObjList  ListFollowers(ObjHndl critter) {throw new NotImplementedException();}


    }
}