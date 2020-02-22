using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Conditions.TemplePlus;
using OpenTemple.Core.Systems.D20.Conditions.TemplePlus.Races;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Utils;
using SharpDX;

namespace OpenTemple.Core.Systems.D20
{
    public static class D20RaceSystem
    {
        private static readonly Dictionary<RaceId, RaceSpec> _races = new Dictionary<RaceId, RaceSpec>();

        public static readonly RaceId[] VanillaRaceIds =
        {
            RaceId.human, RaceId.dwarf, RaceId.elf,
            RaceId.gnome, RaceId.halfelf, RaceId.half_orc,
            RaceId.halfling
        };

        static D20RaceSystem()
        {
            _races[RaceId.human] = new RaceSpec(RaceId.human, RaceBase.human) {
                conditionName = "Human",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                ModelScaleMale = 1.1f,
                ModelScaleFemale = 1.1f,
                ProtoId = 13000,
                HairStyleRace = HairStyleRace.Human
            };

            _races[RaceId.dwarf] = new RaceSpec(RaceId.dwarf, RaceBase.dwarf) {
                conditionName = "Dwarf",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.constitution, 2), (Stat.charisma, -2)},
                ModelScaleMale = 1.1f,
                ModelScaleFemale = 1.1f,
                ProtoId = 13002,
                HairStyleRace = HairStyleRace.Dwarf
            };

            _races[RaceId.elf] = new RaceSpec(RaceId.elf, RaceBase.elf) {
                conditionName = "Elf",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.constitution, -2), (Stat.dexterity, 2)},
                ModelScaleMale = 0.95f,
                ModelScaleFemale = 0.95f,
                ProtoId = 13004,
                HairStyleRace = HairStyleRace.Elf
            };

            _races[RaceId.gnome] = new RaceSpec(RaceId.gnome, RaceBase.gnome) {
                conditionName = "Gnome",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.constitution, 2), (Stat.strength, -2)},
                ModelScaleMale = 0.8f,
                ModelScaleFemale = 0.8f,
                ProtoId = 13006,
                HairStyleRace = HairStyleRace.Gnome
            };

            _races[RaceId.halfelf] = new RaceSpec(RaceId.half_elf, RaceBase.half_elf) {
                conditionName = "Halfelf",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                ModelScaleMale = 1.05f,
                ModelScaleFemale = 1.05f,
                ProtoId = 13008,
                HairStyleRace = HairStyleRace.HalfElf
            };

            _races[RaceId.half_orc] = new RaceSpec(RaceId.half_orc, RaceBase.half_orc) {
                conditionName = "Halforc",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.strength, 2), (Stat.charisma, -2)},
                ModelScaleMale = 1.1f,
                ModelScaleFemale = 1.1f,
                ProtoId = 13010,
                HairStyleRace = HairStyleRace.HalfOrc
            };

            _races[RaceId.halfling] = new RaceSpec(RaceId.halfling, RaceBase.halfling) {
                conditionName = "Halfling",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.dexterity, 2), (Stat.strength, -2)},
                ModelScaleMale = 0.6f,
                ModelScaleFemale = 0.6f,
                ProtoId = 13012,
                HairStyleRace = HairStyleRace.Halfling
            };

            // Extension races
            foreach (var race in ContentDiscovery.Races)
            {
                if (!_races.TryAdd(race.Id, race))
                {
                    throw new ArgumentException("Duplicate Race definition: " + race.Id);
                }
            }
        }

        public static string GetConditionName(RaceId raceId)
        {
            return _races[raceId].conditionName;
        }

        public static Dictionary<SpellStoreData, int> GetSpellLikeAbilities(RaceId raceId)
        {
            return _races[raceId].spellLikeAbilities;
        }

        public static bool HasFeat(RaceId raceId, FeatId featId)
        {
            var raceSpec = GetRaceSpec(raceId);
            return raceSpec.feats.Contains(featId);
        }

        public static Dice GetHitDice(RaceId race)
        {
            var raceSpec = GetRaceSpec(race);
            return raceSpec.hitDice;
        }

        public static RaceSpec GetRaceSpec(RaceId raceId) => _races[raceId];

        public static bool IsVanillaRace(RaceId race) => race >= RaceId.human && race >= RaceId.halfling;

        public static int GetRaceMaterialOffset(RaceId race)
        {
            switch (race)
            {
                case RaceId.human:
                    return 0;
                case RaceId.dwarf:
                    return 6;
                case RaceId.elf:
                    return 2;
                case RaceId.gnome:
                    return 8;
                case RaceId.half_elf:
                    return 10;
                case RaceId.half_orc:
                    return 4;
                case RaceId.halfling:
                    return 12;
                default:
                    var raceSpec = GetRaceSpec(race);
                    return raceSpec.materialOffset;
            }
        }

        public static int GetLevelAdjustment(RaceId race)
        {
            return GetRaceSpec(race).effectiveLevel;
        }

        public static IEnumerable<RaceId> EnumerateBaseRaces()
        {
            return _races
                .Where(kvp => kvp.Value.SubRace == Subrace.none)
                .Select(kvp => kvp.Key);
        }

        public static IEnumerable<RaceId> EnumerateEnabledBaseRaces()
        {
            return _races
                .Where(kvp => kvp.Value.SubRace == Subrace.none)
                .Where(kvp => kvp.Value.IsEnabled())
                .Select(kvp => kvp.Key);
        }

        public static IEnumerable<RaceId> EnumerateSubRaces(RaceBase raceBase)
        {
            return _races
                .Where(kvp => kvp.Value.BaseRace == raceBase)
                .Where(kvp => kvp.Value.IsEnabled())
                .Select(kvp => kvp.Key);
        }

        public static bool IsBaseRace(RaceId raceId) => _races[raceId].IsBaseRace;

        public static int GetProtoId(RaceId raceId, Gender gender)
        {
            var genderOffset = (gender == Gender.Male) ? 0 : 1;
            return _races[raceId].ProtoId + genderOffset;
        }

        public static int GetMinHeight(RaceId race, Gender gender)
        {
            if (gender == Gender.Male)
            {
                return _races[race].heightMale.Item1;
            }
            else
            {
                return _races[race].heightFemale.Item1;
            }
        }

        public static int GetMaxHeight(RaceId race, Gender gender)
        {
            if (gender == Gender.Male)
            {
                return _races[race].heightMale.Item2;
            }
            else
            {
                return _races[race].heightFemale.Item2;
            }
        }

        public static int GetMinWeight(RaceId race, Gender gender)
        {
            if (gender == Gender.Male)
            {
                return _races[race].weightMale.Item1;
            }
            else
            {
                return _races[race].weightFemale.Item1;
            }
        }

        public static int GetMaxWeight(RaceId race, Gender gender)
        {
            if (gender == Gender.Male)
            {
                return _races[race].weightMale.Item2;
            }
            else
            {
                return _races[race].weightFemale.Item2;
            }
        }

        public static float GetModelScale(RaceId raceId, Gender gender)
        {
            var race = _races[raceId];
            var raceScale = gender == Gender.Male ? race.ModelScaleMale : race.ModelScaleFemale;
            if (raceScale.HasValue)
            {
                return raceScale.Value;
            }

            // Fall back to the base race or 100% scale
            if (race.IsBaseRace)
            {
                return 1.0f;
            }
            else
            {
                return GetModelScale((RaceId) race.BaseRace, gender);
            }
        }

        public static HairStyleRace GetHairStyle(RaceId raceId)
        {
            var race = _races[raceId];
            var raceStyle = race.HairStyleRace;
            if (raceStyle.HasValue)
            {
                return raceStyle.Value;
            }

            if (race.IsBaseRace)
            {
                // Default to human
                return HairStyleRace.Human;
            }

            return GetHairStyle((RaceId) race.BaseRace);
        }

        public static bool UseBaseRaceForDeity(RaceId raceId)
        {
            return _races[raceId].useBaseRaceForDeity;
        }

        public static RaceId GetBaseRace(RaceId raceId)
        {
            return (RaceId) _races[raceId].BaseRace;
        }

        public static bool BonusFirstLevelFeat(RaceId raceId)
        {
            return _races[raceId].bonusFirstLevelFeat;
        }
    }

    [Flags]
    public enum RaceDefinitionFlags
    {
        RDF_Vanilla = 1,
        RDF_Monstrous = 2,
        RDF_ForgottenRealms = 4
    }

    public enum RaceBase
    {
        human = 0,
        dwarf = 1,
        elf = 2,
        gnome = 3,
        halfelf = 4,
        half_elf = 4,
        halforc = 5,
        half_orc = 5,
        halfling = 6,

        goblin = 7,
        bugbear = 8,
        gnoll = 9,
        hill_giant = 10,
        troll = 11,
        hobgoblin = 12,
        lizardman = 13,
    }

    public class RaceSpec
    {
        public RaceId Id { get; }
        // Pairs of the modified Stat, and the modifier
        public List<(Stat, int)> statModifiers = new List<(Stat, int)>();
        public int effectiveLevel = 0; // modifier for Effective Character Level (determines XP requirement)
        public string helpTopic; // helpsystem id ("TAG_xxx")
        public RaceDefinitionFlags flags;
        public Dice hitDice = new Dice(0, 0, 0);
        public int naturalArmor = 0;
        public List<FeatId> feats = new List<FeatId>(); // feat enums; for entering new-style feats in python, use tpdp.hash
        public Dictionary<SpellStoreData, int> spellLikeAbilities = new Dictionary<SpellStoreData, int>();

        public int ProtoId; // protos.tab entry (male; female is +1)
        public int materialOffset = 0; // offset for rules/materials.mes (or materials_ext.mes for non-vanilla races)

        public (int, int) weightMale = (80, 100);
        public (int, int) weightFemale = (80, 100);
        public (int, int) heightMale = (70, 80);
        public (int, int) heightFemale = (70, 80);

        // Was originally @ 0x102FE188 in Vanilla
        // If null, it'll inherit from the base race or default to 1.0f (100%)
        public float? ModelScaleMale { get; set; } = null;
        public float? ModelScaleFemale { get; set; } = null;

        // Which hair style models is this race using
        public HairStyleRace? HairStyleRace { get; set; }

        public bool bonusFirstLevelFeat = false; //Grants an extra feat at first level if true
        public bool useBaseRaceForDeity = false; //Treats the subrace as the main race for deity selection if true

        // Main Race
        public RaceBase BaseRace { get; }
        public bool IsBaseRace => SubRace == Subrace.none;

        // Subrace
        public Subrace SubRace { get; }

        public string conditionName;

        public RaceSpec(RaceId id, RaceBase baseRace, Subrace subRace = Subrace.none)
        {
            Id = id;
            BaseRace = baseRace;
            SubRace = subRace;
        }

        public bool IsEnabled()
        {
            if (flags.HasFlag(RaceDefinitionFlags.RDF_Vanilla))
                return true;
            if (flags.HasFlag(RaceDefinitionFlags.RDF_Monstrous) && !Globals.Config.monstrousRaces)
                return false;
            if (flags.HasFlag(RaceDefinitionFlags.RDF_ForgottenRealms) && !Globals.Config.forgottenRealmsRaces)
                return false;
            return Globals.Config.newRaces;
        }
    }
}