using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.D20
{
    public static class AlignmentExtensions
    {

        public static Alignment GetAlignment(this GameObject critter)
        {
            return (Alignment)critter.GetInt32(obj_f.critter_alignment);
        }

        public static bool HasLawfulAlignment(this GameObject critter)
        {
            return (critter.GetAlignment() & Alignment.LAWFUL) != 0;
        }

        public static bool HasChaoticAlignment(this GameObject critter)
        {
            return (critter.GetAlignment() & Alignment.CHAOTIC) != 0;
        }

        public static bool HasGoodAlignment(this GameObject critter)
        {
            return (critter.GetAlignment() & Alignment.GOOD) != 0;
        }

        public static bool HasEvilAlignment(this GameObject critter)
        {
            return (critter.GetAlignment() & Alignment.EVIL) != 0;
        }

        public static bool IsLawful(this Alignment alignment)
        {
            return (alignment & Alignment.LAWFUL) != 0;
        }

        public static bool IsChaotic(this Alignment alignment)
        {
            return (alignment & Alignment.CHAOTIC) != 0;
        }

        public static bool IsGood(this Alignment alignment)
        {
            return (alignment & Alignment.GOOD) != 0;
        }

        public static bool IsEvil(this Alignment alignment)
        {
            return (alignment & Alignment.EVIL) != 0;
        }

    }

}