using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20.Classes.Prereq
{
    public interface ICritterRequirement
    {
        bool FullfillsRequirements(GameObjectBody critter);
    }
}