using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.TimeEvents
{
    public struct TimeEventArg
    {
        public int int32;
        public float float32;
        public GameObjectBody handle;
        public object pyobj; // TODO Python
        public LocAndOffsets location;
        public TimePoint timePoint;
    }
}