using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    public class PathQuery
    {
        public PathQueryFlags flags;
        public int field_4;
        public LocAndOffsets from;
        public LocAndOffsets to;
        public int maxShortPathFindLength;
        public int field2c;
        public GameObjectBody critter; // Set PQF_HAS_CRITTER

        public GameObjectBody targetObj; // Set PQF_TARGET_OBJ

        /*
         When ADJ_RADIUS is set, (usually when there's a TARGET_OBJ)
         this is the minimum distance required. Should be equal
         to the sum of radii of critter + target.
        */
        public float distanceToTargetMin;
        public float tolRadius; // Tolerance (How far away from the exact destination you are allowed to be)
        public int flags2;
        public int field_4c;

        public PathQuery Copy()
        {
            return (PathQuery) MemberwiseClone();
        }
    }

}