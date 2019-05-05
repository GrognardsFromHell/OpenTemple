namespace SpicyTemple.Core.Systems.D20
{
    public enum D20DispatcherKey
    {
        NONE = 0x0,
        STAT_STRENGTH = 1,
        STAT_DEXTERITY = 2,
        STAT_CONSTITUTION = 3,
        STAT_INTELLIGENCE = 4,
        STAT_WISDOM = 5,
        STAT_CHARISMA = 6,
        SAVE_FORTITUDE = 7,
        SAVE_REFLEX = 8,
        SAVE_WILL = 9,
        IMMUNITY_SPELL = 10,
        IMMUNITY_11 = 11,

        IMMUNITY_12 =
            12, // used in AI Controlled, Blindness, and Dominate. Might be a bug, but it doesn't seem to be handled in the immunity handler anyway
        IMMUNITY_COURAGE = 13, // used in Aura of Courage
        IMMUNITY_RACIAL = 14, // actually just Undead and Ooze use this
        IMMUNITY_15 = 15,
        IMMUNITY_SPECIAL = 16,
        OnEnterAoE = 18,
        OnLeaveAoE = 19,
        SKILL_APPRAISE = 20,
        SKILL_BLUFF = 21,
        SKILL_CONCENTRATION = 22,
        SKILL_RIDE = 59,
        SKILL_SWIM = 60,
        SKILL_USE_ROPE = 61,

        CL_Level = 63, // used for queries that regard negative levels
        CL_Barbarian = 64,
        CL_Bard = 65,
        CL_Cleric = 66,
        CL_Druid = 67,
        CL_Fighter = 68,
        CL_Monk = 69,
        CL_Paladin = 70,
        CL_Ranger = 71,
        CL_Rogue = 72,
        CL_Sorcerer = 73,
        CL_Wizard = 74,

        D20A_UNSPECIFIED_MOVE = 75,
        D20A_UNSPECIFIED_ATTACK = 17,
        D20A_STANDARD_ATTACK = 23,
        D20A_FULL_ATTACK = 24,
        D20A_STANDARD_RANGED_ATTACK = 25,
        D20A_RELOAD = 26,
        D20A_5FOOTSTEP = 27,
        D20A_MOVE = 28,
        D20A_DOUBLE_MOVE = 29,
        D20A_RUN = 30,
        D20A_CAST_SPELL = 31,
        D20A_HEAL = 32,
        D20A_CLEAVE = 33,
        D20A_ATTACK_OF_OPPORTUNITY = 34,
        D20A_WHIRLWIND_ATTACK = 35,
        D20A_TOUCH_ATTACK = 36,
        D20A_TOTAL_DEFENSE = 37,
        D20A_CHARGE = 38,
        D20A_FALL_TO_PRONE = 39,
        D20A_STAND_UP = 40,
        D20A_TURN_UNDEAD = 41,
        D20A_DEATH_TOUCH = 42,
        D20A_PROTECTIVE_WARD = 43,
        D20A_FEAT_OF_STRENGTH = 44,
        D20A_BARDIC_MUSIC = 45,
        D20A_PICKUP_OBJECT = 46,
        D20A_COUP_DE_GRACE = 47,
        D20A_USE_ITEM = 48,
        D20A_BARBARIAN_RAGE = 49,
        D20A_STUNNING_FIST = 50,
        D20A_SMITE_EVIL = 51,
        D20A_LAY_ON_HANDS_SET = 52,
        D20A_DETECT_EVIL = 53,
        D20A_STOP_CONCENTRATION = 54,
        D20A_BREAK_FREE = 55,
        D20A_TRIP = 56,
        D20A_REMOVE_DISEASE = 57,
        D20A_ITEM_CREATION = 58,
        D20A_WHOLENESS_OF_BODY_SET = 62,
        D20A_USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL = 76,
        D20A_TRACK = 77,
        D20A_ACTIVATE_DEVICE_STANDARD = 78,
        D20A_SPELL_CALL_LIGHTNING = 79,
        D20A_AOO_MOVEMENT = 80,
        D20A_CLASS_ABILITY_SA = 81,
        D20A_ACTIVATE_DEVICE_FREE = 82,
        D20A_OPEN_INVENTORY = 83,
        D20A_ACTIVATE_DEVICE_SPELL = 84,
        D20A_DISABLE_DEVICE = 85,
        D20A_SEARCH = 86,
        D20A_SNEAK = 87,
        D20A_TALK = 88,
        D20A_OPEN_LOCK = 89,
        D20A_SLEIGHT_OF_HAND = 90,
        D20A_OPEN_CONTAINER = 91,
        D20A_THROW = 92,
        D20A_THROW_GRENADE = 93,
        D20A_FEINT = 94,
        D20A_READY_SPELL = 95,
        D20A_READY_COUNTERSPELL = 96,
        D20A_READY_ENTER = 97,
        D20A_READY_EXIT = 98,
        D20A_COPY_SCROLL = 99,
        D20A_READIED_INTERRUPT = 103,
        D20A_LAY_ON_HANDS_USE = 104,
        D20A_WHOLENESS_OF_BODY_USE = 105,
        D20A_DISMISS_SPELLS = 106,
        D20A_FLEE_COMBAT = 107,
        D20A_USE_POTION = 108,
        D20A_DIVINE_MIGHT = 144,
        D20A_EMPTY_BODY = 145,
        D20A_QUIVERING_PALM = 146,

        NEWDAY_REST = 145, // for successfully resting (is also triggered for an 8 hour uninterrupted rest period)

        NEWDAY_CALENDARICAL =
            146, // for starting a new calendarical day (or artificially adding a days period); I think it's only used for disease timers
        SIG_HP_Changed = 147,
        SIG_HealSkill = 0x94,
        SIG_Sequence = 0x95,
        SIG_Pre_Action_Sequence = 0x96,
        SIG_Action_Recipient = 0x97,
        SIG_BeginTurn = 0x98,
        SIG_EndTurn = 0x99,
        SIG_Dropped_Enemy = 0x9A,
        SIG_Concentration_Broken = 0x9B,
        SIG_Remove_Concentration = 0x9C,
        SIG_BreakFree = 0x9D,
        SIG_Spell_Cast = 0x9E,
        SIG_Spell_End = 0x9F,
        SIG_Spell_Grapple_Removed = 0xA0,
        SIG_Killed = 0xA1,
        SIG_AOOPerformed = 0xA2,
        SIG_Aid_Another = 0xA3,
        SIG_TouchAttackAdded = 0xA4,
        SIG_TouchAttack = 0xA5,
        SIG_Temporary_Hit_Points_Removed = 0xA6,
        SIG_Standing_Up = 0xA7,
        SIG_Bardic_Music_Completed = 0xA8,
        SIG_Combat_End = 0xA9,
        SIG_Initiative_Update = 0xAA,
        SIG_RadialMenu_Clear_Checkbox_Group = 0xAB,
        SIG_Combat_Critter_Moved = 0xAC,
        SIG_Hide = 0xAD,
        SIG_Show = 0xAE,
        SIG_Feat_Remove_Slippery_Mind = 0xAF,
        SIG_Broadcast_Action = 0xB0,
        SIG_Remove_Disease = 0xB1,
        SIG_Rogue_Skill_Mastery_Init = 0xB2,
        SIG_Spell_Call_Lightning = 0xB3,
        SIG_Magical_Item_Deactivate = 0xB4,
        SIG_Spell_Mirror_Image_Struck = 0xB5,
        SIG_Spell_Sanctuary_Attempt_Save = 0xB6,
        SIG_Experience_Awarded = 0xB7,
        SIG_Pack = 0xB8,
        SIG_Unpack = 0xB9,
        SIG_Teleport_Prepare = 0xBA,
        SIG_Teleport_Reconnect = 0xBB,
        SIG_Atone_Fallen_Paladin = 0xBC,
        SIG_Summon_Creature = 0xBD,
        SIG_Attack_Made = 0xBE,
        SIG_Golden_Skull_Combine = 0xBF,
        SIG_Inventory_Update = 0xC0,
        SIG_Critter_Killed = 0xC1,
        SIG_SetPowerAttack = 0xC2,
        SIG_SetExpertise = 0xC3,
        SIG_SetCastDefensively = 0xC4,
        SIG_Resurrection = 0xC5,
        SIG_Dismiss_Spells = 0xC6,
        SIG_DealNormalDamage = 0xC7,
        SIG_Update_Encumbrance = 0xC8,
        SIG_Remove_AI_Controlled = 0xC9,
        SIG_Verify_Obj_Conditions = 0xCA,
        SIG_Web_Burning = 0xCB,
        SIG_Anim_CastConjureEnd = 0xCC,
        SIG_Item_Remove_Enhancement = 0xCD,
        SIG_Disarmed_Weapon_Retrieve = 0xCE, // NEW
        SIG_Disarm = 0xCF, // NEW; resets the "took damage -> abort" flag
        SIG_AID_ANOTHER_WAKE_UP = 0xD0,

        QUE_Helpless = 0xCF,
        QUE_SneakAttack = 0xD0,
        QUE_OpponentSneakAttack = 0xD1,
        QUE_CoupDeGrace = 0xD2,
        QUE_Mute = 0xD3,
        QUE_CannotCast = 0xD4,
        QUE_CannotUseIntSkill = 0xD5,
        QUE_CannotUseChaSkill = 0xD6,
        QUE_RapidShot = 0xD7,
        QUE_Critter_Is_Concentrating = 0xD8,
        QUE_Critter_Is_On_Consecrate_Ground = 0xD9,
        QUE_Critter_Is_On_Desecrate_Ground = 0xDA,
        QUE_Critter_Is_Held = 0xDB,
        QUE_Critter_Is_Invisible = 0xDC,
        QUE_Critter_Is_Afraid = 0xDD,
        QUE_Critter_Is_Blinded = 0xDE,
        QUE_Critter_Is_Charmed = 0xDF,
        QUE_Critter_Is_Confused = 0xE0,
        QUE_Critter_Is_AIControlled = 0xE1,
        QUE_Critter_Is_Cursed = 0xE2,
        QUE_Critter_Is_Deafened = 0xE3,
        QUE_Critter_Is_Diseased = 0xE4,
        QUE_Critter_Is_Poisoned = 0xE5,
        QUE_Critter_Is_Stunned = 0xE6,
        QUE_Critter_Is_Immune_Critical_Hits = 0xE7,
        QUE_Critter_Is_Immune_Poison = 0xE8,
        QUE_Critter_Has_Spell_Resistance = 0xE9,
        QUE_Critter_Has_Condition = 0xEA,
        QUE_Critter_Has_Freedom_of_Movement = 0xEB,
        QUE_Critter_Has_Endure_Elements = 0xEC,
        QUE_Critter_Has_Protection_From_Elements = 0xED,
        QUE_Critter_Has_Resist_Elements = 0xEE,
        QUE_Critter_Has_True_Seeing = 0xEF,
        QUE_Critter_Has_Spell_Active = 0xF0,
        QUE_Critter_Can_Call_Lightning = 0xF1,
        QUE_Critter_Can_See_Invisible = 0xF2,
        QUE_Critter_Can_See_Darkvision = 0xF3,
        QUE_Critter_Can_See_Ethereal = 0xF4,
        QUE_Critter_Can_Discern_Lies = 0xF5,
        QUE_Critter_Can_Detect_Chaos = 0xF6,
        QUE_Critter_Can_Detect_Evil = 0xF7,
        QUE_Critter_Can_Detect_Good = 0xF8,
        QUE_Critter_Can_Detect_Law = 0xF9,
        QUE_Critter_Can_Detect_Magic = 0xFA,
        QUE_Critter_Can_Detect_Undead = 0xFB,
        QUE_Critter_Can_Find_Traps = 0xFC,
        QUE_Critter_Can_Dismiss_Spells = 0xFD,
        QUE_Obj_Is_Blessed = 0xFE,
        QUE_Unconscious = 0xFF,
        QUE_Dying = 0x100,
        QUE_Dead = 0x101,
        QUE_AOOPossible = 0x102,
        QUE_AOOIncurs = 0x103,
        QUE_HoldingCharge = 0x104,
        QUE_Has_Temporary_Hit_Points = 0x105,
        QUE_SpellInterrupted = 0x106,
        QUE_ActionTriggersAOO = 0x107,
        QUE_ActionAllowed = 0x108,
        QUE_Prone = 0x109,
        QUE_RerollSavingThrow = 0x10A,
        QUE_RerollAttack = 0x10B,
        QUE_RerollCritical = 0x10C,
        QUE_Commanded = 0x10D,
        QUE_Turned = 0x10E,
        QUE_Rebuked = 0x10F,
        QUE_CanBeFlanked = 0x110,
        QUE_Critter_Is_Grappling = 0x111,
        QUE_Barbarian_Raged = 0x112,
        QUE_Barbarian_Fatigued = 0x113,
        QUE_NewRound_This_Turn = 0x114,
        QUE_Flatfooted = 0x115,
        QUE_Masterwork = 0x116,
        QUE_FailedDecipherToday = 0x117,
        QUE_Polymorphed = 0x118,
        QUE_IsActionInvalid_CheckAction = 0x119,
        QUE_CanBeAffected_PerformAction = 0x11A,
        QUE_CanBeAffected_ActionFrame = 0x11B,
        QUE_AOOWillTake = 0x11C,
        QUE_Weapon_Is_Mighty_Cleaving = 0x11D,
        QUE_Autoend_Turn = 0x11E,
        QUE_ExperienceExempt = 0x11F,
        QUE_FavoredClass = 0x120,
        QUE_IsFallenPaladin = 0x121,
        QUE_WieldedTwoHanded = 0x122,
        QUE_Critter_Is_Immune_Energy_Drain = 0x123,
        QUE_Critter_Is_Immune_Death_Touch = 0x124,
        QUE_Failed_Copy_Scroll = 0x125,
        QUE_Armor_Get_AC_Bonus = 0x126,
        QUE_Armor_Get_Max_DEX_Bonus = 0x127,
        QUE_Armor_Get_Max_Speed = 0x128,
        QUE_FightingDefensively = 0x129,
        QUE_Elemental_Gem_State = 0x12A,
        QUE_Untripable = 0x12B,
        QUE_Has_Thieves_Tools = 0x12C,
        QUE_Critter_Is_Encumbered_Light = 0x12D,
        QUE_Critter_Is_Encumbered_Medium = 0x12E,
        QUE_Critter_Is_Encumbered_Heavy = 0x12F,
        QUE_Critter_Is_Encumbered_Overburdened = 0x130,
        QUE_Has_Aura_Of_Courage = 0x131,
        QUE_BardicInstrument = 0x132,
        QUE_EnterCombat = 0x133,
        QUE_AI_Fireball_OK = 0x134,
        QUE_Critter_Cannot_Loot = 0x135,
        QUE_Critter_Cannot_Wield_Items = 0x136,
        QUE_Critter_Is_Spell_An_Ability = 0x137,
        QUE_Play_Critical_Hit_Anim = 0x138,
        QUE_Is_BreakFree_Possible = 0x139,
        QUE_Critter_Has_Mirror_Image = 0x13A,
        QUE_Wearing_Ring_of_Change = 0x13B,
        QUE_Critter_Has_No_Con_Score = 0x13C,
        QUE_Item_Has_Enhancement_Bonus = 0x13D,
        QUE_Item_Has_Keen_Bonus = 0x13E,
        QUE_AI_Has_Spell_Override = 0x13F,
        QUE_Weapon_Get_Keen_Bonus = 0x140,
        QUE_Disarmed = 0x141,
        SIG_Destruction_Domain_Smite = 0x142,
        QUE_Can_Perform_Disarm = 0x143,
        QUE_Craft_Wand_Spell_Level = 0x144,
        QUE_Is_Ethereal = 0x145,
        QUE_Empty_Body_Num_Rounds = 0x146, // returns number of rounds set for Monk's Empty Body
        QUE_Quivering_Palm_Can_Perform = 0x147,
        QUE_Trip_AOO = 0x148,
        QUE_Get_Arcane_Spell_Failure = 0x149, // returns additive spell failure chance

        QUE_Is_Preferring_One_Handed_Wield =
            0x14A, // e.g. a character with a Buckler can opt to wield a sword one handed so as to not take the -1 to hit penalty

        LVL_Stats_Activate = 100,
        LVL_Stats_Check_Complete = 101,
        LVL_Stats_Finalize = 102,

        LVL_Features_Activate = 200,
        LVL_Features_Check_Complete = 201,
        LVL_Features_Finalize = 202,

        LVL_Skills_Activate = 300,
        LVL_Skills_Check_Complete = 301,
        LVL_Skills_Finalize = 302,

        LVL_Feats_Activate = 400,
        LVL_Feats_Check_Complete = 401,
        LVL_Feats_Finalize = 402,

        LVL_Spells_Activate = 500,
        LVL_Spells_Check_Complete = 501,
        LVL_Spells_Finalize = 502,

        SPELL_Base_Caster_Level = 0x1000
    }
}