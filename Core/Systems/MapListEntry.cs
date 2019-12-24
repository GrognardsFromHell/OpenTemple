namespace OpenTemple.Core.Systems
{
    public class MapListEntry
    {
        public int id;
        public int flags = 0;
        public string name;
        public string description;
        public int worldmap = 0;
        public int area = 0;
        public int movie = 0;
        public int startPosX = 0;
        public int startPosY = 0;
        public MapType type = MapType.None;

        public bool IsOutdoors => (flags & 2) == 2;

        public bool IsBedrest => (flags & 8) == 8;

        public bool IsUnfogged => (flags & 4) == 4;
    }
}