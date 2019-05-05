using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.TimeEvents
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