using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.Pathfinding;

public struct MapPathNode
{
    public int id;
    public PathNodeFlag flags;
    public LocAndOffsets nodeLoc;

    public int[] neighbours;

    // distances to the neighbours; is the negative of the distance if straight line is possible
    public float[] neighDistances;
}