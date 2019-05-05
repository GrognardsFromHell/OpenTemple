using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Feats;

namespace SpicyTemple.Core.Systems.D20
{
    [Flags]
    public enum ClassDefinitionFlag
    {
        // denotes whether class is base class (can be taken at level 1, and factors into multiclass calculations; unlike Prestige Classes for instance)
        CDF_BaseClass = 0x1,

        // is the class drawn from Core 3.5 rules?
        CDF_CoreClass = 0x2
    }

    public enum BABProgressionType
    {
        Martial = 0, // +1 every level
        SemiMartial = 1, // + 3/4
        NonMartial = 2 // + 1/2
    }

    public class D20ClassSpec
    {
        public Stat classEnum;
        public ClassDefinitionFlag flags;
        public BABProgressionType babProgression;
        public Stat deityClass; // emulate deity compatibility of the vanilla classes
        public string helpTopic; // e.g. TAG_BARDS

        public bool fortitudeSaveIsFavored;
        public bool reflexSaveIsFavored;
        public bool willSaveIsFavored;
        public int hitDice; // HD side (4,6,8 etc)
        public int skillPts; // skill point per level

        public Dictionary<SkillId, bool> classSkills; // dictionary denoting if a skill is a class skill
        public Dictionary<FeatId, int> classFeats; // dictionary denoting which level the feat is granted
        public string conditionName; // name of the accompanying condition (e.g. "Bard", "Sorcerer", "Mystic Theurge")

        // spell casting

        public string
            spellCastingConditionName; // name of the accompanying Spell Casting condition (e.g. "Bard Spellcasting")

        public SpellListType spellListType;

        public Dictionary<int, int>
            spellList; // mapping Spell Enum . Spell Level for this class. This information is also stored in spellSystem under spellEntryExt

        public SpellReadyingType spellMemorizationType;
        public SpellSourceType spellSourceType;

        public Dictionary<int, List<int>>
            spellsPerDay; // index is class level, vector enumerates spells per day for each spell level

        public Stat spellStat; // stat that determines maximum spell level
        public Stat spellDcStat = Stat.strength; // stat that determines spell DC level
    }
}