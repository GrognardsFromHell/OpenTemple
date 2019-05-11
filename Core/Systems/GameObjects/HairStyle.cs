using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.TigSubsystems;

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

        [TempleDllLocation(0x100e17f0)]
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

        [TempleDllLocation(0x100e17b0)]
        public int Pack()
        {
            return ((int) Race & 7)
                   | (((int) Gender) & 1) << 3
                   | (((int) Size) & 3) << 10
                   | ((int) Style & 7) << 4
                   | ((int) Color & 7) << 7;
        }

        [TempleDllLocation(0x100e1970)]
        public string PreviewTexturePath => GetHairStyleFile("tga");

        [TempleDllLocation(0x100e1940)]
        public string ModelPath => GetHairStyleFile("skm");

        private const string Folder = "art/meshes/hair/";

        [TempleDllLocation(0x100e1850)]
        private string GetHairStyleFile(string extension)
        {
            string genderShortName;
            string genderDir;
            if (Gender == Gender.Male)
            {
                genderShortName = "m";
                genderDir = "male";
            }
            else
            {
                genderShortName = "f";
                genderDir = "female";
            }

            // These will be modified if a fallback is needed, so copy them here
            var race = Race;
            var size = Size;
            var styleNr = Style;

            while (true)
            {
                while (true)
                {
                    while (true)
                    {
                        var filename = $"{Folder}{genderDir}/s{styleNr}/"
                                       + GetHairStyleRaceName(race)
                                       + $"_{genderShortName}_s{styleNr}_c{Color}_{GetHairStyleSizeName(size)}.{extension}";

                        if (Tig.FS.FileExists(filename))
                        {
                            return filename;
                        }

                        // Fall back to a compatible race, if no model for this
                        // race exists
                        var fallbackRace = GetHairStyleFallbackRace(race);
                        if (fallbackRace == race)
                        {
                            break;
                        }

                        race = fallbackRace;
                    }

                    // Fall back to a smaller size of the model
                    var fallbackSize = GetHairStyleFallbackSize(size);
                    if (fallbackSize == size)
                    {
                        break;
                    }

                    size = fallbackSize;
                }

                // Fall back to bald if no other options exist
                if (styleNr == HairStyle.Bald)
                    break;
                styleNr = HairStyle.Bald;
            }

            return null;
        }

        private static string GetHairStyleSizeName(HairStyleSize size)
        {
            switch (size)
            {
                case HairStyleSize.Big:
                    return "big";
                case HairStyleSize.Small:
                    return "small";
                case HairStyleSize.None:
                    return "none";
                default:
                    throw new ArgumentOutOfRangeException("Unsupported hair style size: " + size);
            }
        }

        private static string GetHairStyleRaceName(HairStyleRace race)
        {
            switch (race)
            {
                case HairStyleRace.Human:
                    return "hu";
                case HairStyleRace.Dwarf:
                    return "dw";
                case HairStyleRace.Elf:
                    return "el";
                case HairStyleRace.Gnome:
                    return "gn";
                case HairStyleRace.HalfElf:
                    return "he";
                case HairStyleRace.HalfOrc:
                    return "ho";
                case HairStyleRace.Halfling:
                    return "hl";
                default:
                    throw new ArgumentOutOfRangeException("Unsupported hair style race: " + race);
            }
        }

        private static HairStyleSize GetHairStyleFallbackSize(HairStyleSize size)
        {
            switch (size)
            {
                case HairStyleSize.Big:
                    return HairStyleSize.Small;
                case HairStyleSize.Small:
                    return HairStyleSize.None;
                case HairStyleSize.None:
                    return HairStyleSize.None;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported hair style size: " + size);
            }
        }

        /// <summary>
        /// Gets the race that should be fallen back to if there's
        /// no model for the given race. If it returns race again
        /// there's no more fallback
        /// </summary>
        private static HairStyleRace GetHairStyleFallbackRace(HairStyleRace race)
        {
            switch (race)
            {
                case HairStyleRace.Human:
                    return HairStyleRace.Human;
                case HairStyleRace.Dwarf:
                    return HairStyleRace.Dwarf;
                case HairStyleRace.Elf:
                    return HairStyleRace.Human;
                case HairStyleRace.Gnome:
                    return HairStyleRace.Human;
                case HairStyleRace.HalfElf:
                    return HairStyleRace.Human;
                case HairStyleRace.HalfOrc:
                    return HairStyleRace.HalfOrc;
                case HairStyleRace.Halfling:
                    return HairStyleRace.Human;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported hair style race: " + race);
            }
        }
    }
}