using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.Pathfinding
{
    public class PathQuery
    {
        public PathQueryFlags flags;
        public int field_4;
        public LocAndOffsets from;
        public LocAndOffsets to;
        public int maxShortPathFindLength;
        public int field2c;
        public GameObject critter; // Set PQF_HAS_CRITTER

        public GameObject targetObj; // Set PQF_TARGET_OBJ

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