namespace SpicyTemple.Core.Systems.D20.Classes
{
    public interface IAlignmentRequirement
    {
        bool IsCompatible(Alignment alignment);
    }

    internal class RestrictedAlignmentRequirement : IAlignmentRequirement
    {
        private readonly Alignment _restrictedBitmask;

        public RestrictedAlignmentRequirement(Alignment restrictedBitmask)
        {
            _restrictedBitmask = restrictedBitmask;
        }

        public bool IsCompatible(Alignment alignment)
        {
            return (alignment & _restrictedBitmask) == 0;
        }
    }

    public static class AlignmentRequirements
    {

        public static readonly IAlignmentRequirement NonLawful = new RestrictedAlignmentRequirement(Alignment.LAWFUL);

    }
}