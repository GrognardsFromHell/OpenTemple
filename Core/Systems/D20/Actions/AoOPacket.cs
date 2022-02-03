using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.D20.Actions
{
    public readonly struct AttackOfOpportunity
    {
        public readonly GameObject Interrupter;
        public readonly float Distance;
        public readonly LocAndOffsets Location;

        public AttackOfOpportunity(GameObject interrupter, float distance, LocAndOffsets location)
        {
            Interrupter = interrupter;
            Distance = distance;
            Location = location;
        }
    }
}