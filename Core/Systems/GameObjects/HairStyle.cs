using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public enum HairStyleRace
    {
        Human = 0,
        Dwarf,
        Elf,
        Gnome,
        HalfElf,
        HalfOrc,
        Halfling,
        Invalid
    }

    public enum HairStyleSize
    {
        Big = 0,
        Small,
        None
    }

    public struct HairSettings
    {
        public HairStyleRace Race;
        public Gender Gender;
        public HairStyleSize Size;
        public HairColor Color;
        public HairStyle Style;

        public static HairSettings Unpack(int packed)
        {
            return new HairSettings
            {
                Race = (HairStyleRace) (packed & 7),
                Gender = (Gender) ((packed >> 3) & 1),
                Size = (HairStyleSize) ((packed >> 10) & 3),
                Style = (HairStyle) ((packed >> 4) & 7),
                Color = (HairColor) ((packed >> 7) & 7)
            };
        }

        public int Pack()
        {
            return ((int) Race & 7)
                   | (((int) Gender) & 1) << 3
                   | (((int) Size) & 3) << 10
                   | ((int) Style & 7) << 4
                   | ((int) Color & 7) << 7;
        }
    }
}