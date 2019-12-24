using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.D20.Classes.Prereq
{

    internal class RestrictedAlignmentPrereq : ICritterRequirement
    {
        private readonly Alignment _restrictedBitmask;

        public RestrictedAlignmentPrereq(Alignment restrictedBitmask)
        {
            _restrictedBitmask = restrictedBitmask;
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            var alignment = critter.GetAlignment();

            return (alignment & _restrictedBitmask) == 0;
        }
    }

    public static class AlignmentPrereqs
    {
        public static readonly ICritterRequirement NonLawful = new RestrictedAlignmentPrereq(Alignment.LAWFUL);
    }
}