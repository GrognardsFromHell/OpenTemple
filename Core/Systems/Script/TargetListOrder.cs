using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.Script
{
    public enum TargetListOrder
    {
        HitDice = 1,
        HitDiceThenDist = 2,
        Dist = 3,
        DistFromCaster = 4
    }
    public enum TargetListOrderDirection
    {
        Ascending,
        Descending
    }
}