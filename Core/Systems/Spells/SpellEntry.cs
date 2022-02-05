using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.Spells;

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

    public SpellEntryLevelSpec(Stat castingClass, int level) : this()
    {
        spellClass = 0x80 | ((int) castingClass);
        slotLevel = level;
    }
    public SpellEntryLevelSpec(DomainId domain, int level) : this()
    {
        spellClass = (int) domain;
        slotLevel = level;
    }

    public bool Equals(SpellEntryLevelSpec other)
    {
        return spellClass == other.spellClass && slotLevel == other.slotLevel;
    }

    public override bool Equals(object obj)
    {
        return obj is SpellEntryLevelSpec other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (spellClass * 397) ^ slotLevel;
        }
    }

    public static bool operator ==(SpellEntryLevelSpec left, SpellEntryLevelSpec right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SpellEntryLevelSpec left, SpellEntryLevelSpec right)
    {
        return !left.Equals(right);
    }
}

[Flags]
public enum SpellDescriptor
{
    ACID = 0x1,
    CHAOTIC = 0x2,
    COLD = 0x4,
    DARKNESS = 0x8,
    DEATH = 0x10,
    ELECTRICITY = 0x20,
    EVIL = 0x40,
    FEAR = 0x80,
    FIRE = 0x100,
    FORCE = 0x200,
    GOOD = 0x400,
    LANGUAGE_DEPENDENT = 0x800,
    LAWFUL = 0x1000,
    LIGHT = 0x2000,
    MIND_AFFECTING = 0x4000,
    SONIC = 0x8000,
    TELEPORTATION = 0x10000,
    AIR = 0x20000,
    EARTH = 0x40000,
    WATER = 0x80000
}

[Flags]
public enum SpellComponent
{
    Verbal = 1,
    Somatic = 2,
    Experience = 4,
    Material = 8,
}

public enum SpellCastingTime
{
    StandardAction = 0,
    FullRoundAction = 1,
    OutOfCombat = 2,
    Safe = 3,
    FreeAction = 4
}

public enum SpellSavingThrow {
    None = 0,
    Reflex = 1,
    Willpower = 2,
    Fortitude = 3
}

public enum SpellResistanceType
{
    No = 0,
    Yes = 1,
    InCode = 2
}

public class SpellEntry {
    public int spellEnum;
    public SchoolOfMagic spellSchoolEnum;
    public SubschoolOfMagic spellSubSchoolEnum;
    public SpellDescriptor spellDescriptorBitmask;
    public SpellComponent spellComponentBitmask;
    public int costGp;
    public int costXp;
    public SpellCastingTime castingTimeType;
    public SpellRangeType spellRangeType;
    public int spellRange;
    public SpellSavingThrow savingThrowType;
    public SpellResistanceType spellResistanceCode;
    public List<SpellEntryLevelSpec> spellLvls = new();
    // spellLvlsNum replaced by spellLvls.Count
    public bool projectileFlag; // TODO: Might be a bool
    public UiPickerFlagsTarget flagsTargetBitmask;
    public UiPickerIncFlags incFlagsTargetBitmask;
    public UiPickerIncFlags excFlagsTargetBitmask;
    public UiPickerType modeTargetSemiBitmask;
    public int minTarget;
    public int maxTarget;
    //note:	if it's negative, then its absolute value is used as SpellRangeType for mode_target personal; if it's positive, it's a specified number(in feet ? )
    public int radiusTarget; // TODO: This mixed mode shit of negative value == enum needs to go away
    public int degreesTarget;
    public AiSpellType aiTypeBitmask; // see AiSpellType in spell_structs.h

    public SpellEntry()
    {
    }

    public bool IsBaseModeTarget(UiPickerType type) => modeTargetSemiBitmask.IsBaseMode(type);

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

    public bool HasAiType(AiSpellType aiSpellType)
    {
        return (aiTypeBitmask & aiSpellType) != 0;
    }

    public bool HasDescriptor(SpellDescriptor descriptor)
    {
        return (spellDescriptorBitmask & descriptor) != 0;
    }
}