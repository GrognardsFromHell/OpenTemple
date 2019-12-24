using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.GameObjects
{
    public enum StandPointType : uint {
        Day = 0,
        Night = 1,
        Scout = 2
    };

    public struct StandPoint
    {
        public int mapId;
        public int _pad1;
        public LocAndOffsets location;
        public int jumpPointId;
        public int _pad2;
    }
}