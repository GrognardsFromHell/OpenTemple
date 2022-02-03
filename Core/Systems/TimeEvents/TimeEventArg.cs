using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.TimeEvents;

public struct TimeEventArg
{
    public int int32;
    public float float32;
    public GameObject handle;
    public object pyobj; // TODO Python
    public LocAndOffsets location;
    public TimePoint timePoint;
}