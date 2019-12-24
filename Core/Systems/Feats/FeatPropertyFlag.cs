using System;

namespace OpenTemple.Core.Systems.Feats
{
    [Flags]
    public enum FeatPropertyFlag : uint {
        CAN_GAIN_MULTIPLE_TIMES = 0x1,
        DISABLED = 0x2,
        RACE_AUTOMATIC = 0x4,
        CLASS_AUTMATIC = 0x8 ,
        FIGHTER_BONUS = 0x10,
        MONK_BONUS_1st = 0x20,
        MONK_BONUS_2nd = 0x40,
        MONK_BONUS_6th = 0x80,
        MULTI_SELECT_ITEM = 0x100,
        EXOTIC_WEAP_ITEM = 0x300,
        IMPR_CRIT_ITEM = 0x500,
        MARTIAL_WEAP_ITEM = 0x900,
        SKILL_FOCUS_ITEM = 0x1100,
        WEAP_FINESSE_ITEM = 0x2100,
        WEAP_FOCUS_ITEM = 0x4100,
        WEAP_SPEC_ITEM = 0x8100,
        GREATER_WEAP_FOCUS_ITEM = 0x10100,
        WIZARD_BONUS = 0x20000,
        ROGUE_BONUS = 0x40000, // rogue bonus at 10th level

        MULTI_MASTER = 0x80000, // head of multiselect class of feats (NEW)
        GREAT_WEAP_SPEC_ITEM = 0x100100, // NEW
        PSIONIC = 0x200000,
        NON_CORE =0x400000,
        CUSTOM_REQ = 0x800000, // signifies that requirements should be checked via script
        POWER_CRIT_ITEM = 0x1000000
    }

}
