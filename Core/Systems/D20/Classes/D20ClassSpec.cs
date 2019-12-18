using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.D20.Classes.Prereq;
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

    public class D20ClassSpec
    {
        public Stat classEnum;
        public ClassDefinitionFlag flags;
        public Stat deityClass; // emulate deity compatibility of the vanilla classes
        public string helpTopic; // e.g. TAG_BARDS

        public string category;

        public BaseAttackProgressionType BaseAttackBonusProgression { get; set; }

        public SavingThrowProgressionType FortitudeSaveProgression { get; set; }

        public SavingThrowProgressionType ReflexSaveProgression { get; set; }

        public SavingThrowProgressionType WillSaveProgression { get; set; }

        public int hitDice; // HD side (4,6,8 etc)
        public int skillPts; // skill point per level

        // dictionary denoting if a skill is a class skill
        public IImmutableSet<SkillId> classSkills;

        // dictionary denoting which level the feat is granted
        public IImmutableDictionary<FeatId, int> classFeats;

        // name of the accompanying condition (e.g. "Bard", "Sorcerer", "Mystic Theurge")
        public string conditionName;

        // spell casting

        // name of the accompanying Spell Casting condition (e.g. "Bard Spellcasting")
        // TODO I think this is unused
        public string spellCastingConditionName;

        public SpellListType spellListType;

        // mapping Spell Enum . Spell Level for this class. This information is also stored in spellSystem under spellEntryExt
        public Dictionary<int, int> spellList;

        public SpellReadyingType spellMemorizationType;
        public SpellSourceType spellSourceType;

        // index is class level, vector enumerates spells per day for each spell level
        public IImmutableDictionary<int, IImmutableList<int>> spellsPerDay =
            ImmutableDictionary<int, IImmutableList<int>>.Empty;

        public Stat spellStat; // stat that determines maximum spell level
        public Stat spellDcStat = Stat.strength; // stat that determines spell DC level
        public bool hasArmoredArcaneCasterFeature;

        public List<ICritterRequirement> Requirements { get; set; } = new List<ICritterRequirement>();

        public SavingThrowProgressionType GetSavingThrowProgression(SavingThrowType savingThrowType)
        {
            switch (savingThrowType)
            {
                case SavingThrowType.Fortitude:
                    return FortitudeSaveProgression;
                case SavingThrowType.Reflex:
                    return ReflexSaveProgression;
                case SavingThrowType.Will:
                    return WillSaveProgression;
                default:
                    throw new ArgumentOutOfRangeException(nameof(savingThrowType), savingThrowType, null);
            }
        }
    }
}