using System.Collections.Generic;
using SpicyTemple.Core.Ui.InGameSelect;

namespace SpicyTemple.Core.Systems.Spells
{

    public enum SpellRangeType
    {
        SRT_Specified = 0,
        SRT_Personal = 1,
        SRT_Touch,
        SRT_Close,
        SRT_Medium,
        SRT_Long,
        SRT_Unlimited,
        SRT_Special_Inivibility_Purge
    }

    public struct SpellEntryLevelSpec
    {
        public int spellClass;
        public int slotLevel;
    }

    public class SpellEntry {
        public readonly int spellEnum;
        public readonly int spellSchoolEnum;
        public readonly uint spellSubSchoolEnum;
        public readonly uint spellDescriptorBitmask;
        public readonly uint spellComponentBitmask;
        public readonly int costGp;
        public readonly uint costXp;
        public readonly uint castingTimeType;
        public readonly SpellRangeType spellRangeType;
        public readonly int spellRange;
        public readonly uint savingThrowType;
        public readonly uint spellResistanceCode;
        public readonly List<SpellEntryLevelSpec> spellLvls = new List<SpellEntryLevelSpec>();
        // spellLvlsNum replaced by spellLvls.Count
        public readonly uint projectileFlag;
        public readonly UiPickerFlagsTarget flagsTargetBitmask;
        public readonly ulong incFlagsTargetBitmask;
        public readonly ulong excFlagsTargetBitmask;
        public readonly ulong modeTargetSemiBitmask; // UiPickerType
        public readonly int minTarget;
        public readonly int maxTarget;
        public readonly int radiusTarget; //note:	if it's negative, then its absolute value is used as SpellRangeType for mode_target personal; if it's positive, it's a specified number(in feet ? )
        public readonly int degreesTarget;
        public readonly uint aiTypeBitmask; // see AiSpellType in spell_structs.h
        public readonly uint pad;

        public SpellEntry()
        {
        }

        public bool IsBaseModeTarget(UiPickerType type)
        {
            var _type = (ulong)type;
            return (modeTargetSemiBitmask & 0xFF) == _type;
        }

        // returns -1 if none
        public int SpellLevelForSpellClass(int spellClass)
        {
            // search the spell list extension first
            // PrC support
            // for Assassin/Blackguard etc who have their own unique spell tables
            foreach (var it in GameSystems.Spell.GetSpellListExtension(spellEnum)){
                if (it.spellClass == spellClass)
                {
                    return it.slotLevel;
                }
            }

            // Search the definition from the .txt file if none found
            foreach (var spec in spellLvls)
            {
                if (spec.spellClass == spellClass)
                {
                    return spec.slotLevel;
                }
            }

            // default
            return -1;
        }
    }

}