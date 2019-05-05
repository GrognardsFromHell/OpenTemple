using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
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
            foreach (var race in VanillaRaceIds)
            {
                _races[race] = new RaceSpec();
            }

            _races[RaceId.dwarf].statModifiers[(int) Stat.constitution] = 2;
            _races[RaceId.dwarf].statModifiers[(int) Stat.charisma] = -2;

            _races[RaceId.elf].statModifiers[(int) Stat.constitution] = -2;
            _races[RaceId.elf].statModifiers[(int) Stat.dexterity] = 2;

            _races[RaceId.gnome].statModifiers[(int) Stat.constitution] = 2;
            _races[RaceId.gnome].statModifiers[(int) Stat.strength] = -2;

            _races[RaceId.half_orc].statModifiers[(int) Stat.charisma] = -2;
            _races[RaceId.half_orc].statModifiers[(int) Stat.strength] = 2;

            _races[RaceId.halfling].statModifiers[(int) Stat.dexterity] = 2;
            _races[RaceId.halfling].statModifiers[(int) Stat.strength] = -2;
        }

        public static bool HasFeat(RaceId raceId, FeatId faceId)
        {
            throw new System.NotImplementedException();
        }

        public static Dice GetHitDice(RaceId race)
        {
            var raceSpec = GetRaceSpec(race);
            return raceSpec.hitDice;
        }

        private static RaceSpec GetRaceSpec(RaceId raceId) => _races[raceId];
    }

    [Flags]
    internal enum RaceDefinitionFlags
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

    internal class RaceSpec
    {
        public List<int> statModifiers = new List<int> {0, 0, 0, 0, 0, 0};
        public int effectiveLevel = 0; // modifier for Effective Character Level (determines XP requirement)
        public string helpTopic; // helpsystem id ("TAG_xxx")
        public RaceDefinitionFlags flags;
        public Dice hitDice = new Dice(0, 0, 0);
        public int naturalArmor = 0;
        public List<int> feats = new List<int>(); // feat enums; for entering new-style feats in python, use tpdp.hash
        public Dictionary<SpellStoreData, int> spellLikeAbilities = new Dictionary<SpellStoreData, int>();

        public int protoId; // protos.tab entry (male; female is +1)
        public int materialOffset = 0; // offset for rules/materials.mes (or materials_ext.mes for non-vanilla races)

        public List<int> weightMale = new List<int> {80, 100};
        public List<int> weightFemale = new List<int> {80, 100};
        public List<int> heightMale = new List<int> {70, 80};
        public List<int> heightFemale = new List<int> {70, 80};

        public bool bonusFirstLevelFeat = false; //Grants an extra feat at first level if true
        public bool useBaseRaceForDeity = false; //Treats the subrace as the main race for deity selection if true

        // Main Race
        public RaceBase baseRace = RaceBase.human;
        public bool isBaseRace = true;

        public List<RaceId> subraces = new List<RaceId>();

        // Subrace
        public Subrace subrace = Subrace.none;

        public string conditionName;

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