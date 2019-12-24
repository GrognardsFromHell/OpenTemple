using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.D20.Actions
{
    public readonly struct AttackOfOpportunity
    {
        public readonly GameObjectBody Interrupter;
        public readonly float Distance;
        public readonly LocAndOffsets Location;

        public AttackOfOpportunity(GameObjectBody interrupter, float distance, LocAndOffsets location)
        {
            Interrupter = interrupter;
            Distance = distance;
            Location = location;
        }
    }
}