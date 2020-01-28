using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.GameObjects
{
    public enum HairStyleRace
    {
        Human = 0,
        Dwarf,
        Elf,
        Gnome,
        HalfElf,
        HalfOrc,
        Halfling
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
                        var filename = $"{Folder}{genderDir}/s{GetHairStyleStyleName(styleNr)}/"
                                       + GetHairStyleRaceName(race)
                                       + $"_{genderShortName}_s{GetHairStyleStyleName(styleNr)}_c{GetHairStyleColorName(Color)}_{GetHairStyleSizeName(size)}.{extension}";

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
            return size switch
            {
                HairStyleSize.Big => "big",
                HairStyleSize.Small => "small",
                HairStyleSize.None => "none",
                _ => throw new ArgumentOutOfRangeException("Unsupported hair style size: " + size)
            };
        }

        private static string GetHairStyleStyleName(HairStyle style)
        {
            return style switch
            {
                HairStyle.Longhair => "0",
                HairStyle.Ponytail => "1",
                HairStyle.Shorthair => "2",
                HairStyle.Topknot => "3",
                HairStyle.Mullet => "4",
                HairStyle.Bald => "5",
                HairStyle.Mohawk => "6",
                HairStyle.Medium => "7",
                _ => throw new ArgumentOutOfRangeException("Unsupported hair style: " + style)
            };
        }

        private static string GetHairStyleColorName(HairColor color)
        {
            return color switch
            {
                HairColor.Black => "0",
                HairColor.Blonde => "1",
                HairColor.Blue => "2",
                HairColor.Brown => "3",
                HairColor.LightBrown => "4",
                HairColor.Pink => "5",
                HairColor.Red => "6",
                HairColor.White => "7",
                _ => throw new ArgumentOutOfRangeException("Unsupported hair color: " + color)
            };
        }

        private static string GetHairStyleRaceName(HairStyleRace race)
        {
            return race switch
            {
                HairStyleRace.Human => "hu",
                HairStyleRace.Dwarf => "dw",
                HairStyleRace.Elf => "el",
                HairStyleRace.Gnome => "gn",
                HairStyleRace.HalfElf => "he",
                HairStyleRace.HalfOrc => "ho",
                HairStyleRace.Halfling => "hl",
                _ => throw new ArgumentOutOfRangeException("Unsupported hair style race: " + race)
            };
        }

        private static HairStyleSize GetHairStyleFallbackSize(HairStyleSize size)
        {
            return size switch
            {
                HairStyleSize.Big => HairStyleSize.Small,
                HairStyleSize.Small => HairStyleSize.None,
                HairStyleSize.None => HairStyleSize.None,
                _ => throw new ArgumentOutOfRangeException("Unsupported hair style size: " + size)
            };
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