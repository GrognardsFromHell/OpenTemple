using System;
using System.Collections.Generic;

namespace SpicyTemple.Core.Systems.D20
{
    public enum DispatcherType
    {
        None = 0,
        ConditionAdd = 1,
        ConditionRemove = 2,
        ConditionAddPre = 3,
        ConditionRemove2 = 4,
        ConditionAddFromD20StatusInit = 5,
        D20AdvanceTime = 6,
        TurnBasedStatusInit = 7,
        Initiative = 8,
        NewDay = 9, // refers both to an uninterrupted 8 hour rest period (key 0x91), or a 1 day rest period (key 0x91), or a new calendarical day (with key 0x92)
        AbilityScoreLevel = 10,
        GetAC = 11,
        AcModifyByAttacker = 12, // modifies defender's Armor Class by attacker conditions (e.g. if the attacker is Invisible, they will have a hook that nullifies the defender's dexterity bonus using this event type)
        SaveThrowLevel = 13, // goes with keys DK_SAVE_X
        SaveThrowSpellResistanceBonus = 14, // only used for Inward Magic Circle
        ToHitBonusBase = 15,
        ToHitBonus2 = 16,
        ToHitBonusFromDefenderCondition = 17, // e.g. if the defender has Blindness, the attacker will get a bonus for his To Hit
        DealingDamage = 18,
        TakingDamage = 19,
        DealingDamage2 = 20,
        TakingDamage2 = 21,
        ReceiveHealing = 22, // Healing
        GetCriticalHitRange = 23, // first hit roll that is a critical hit
        GetCriticalHitExtraDice = 24, // runs for the attacker's dispatcher
        CurrentHP = 25,
        MaxHP = 26,
        InitiativeMod = 27,
        D20Signal = 28,
        D20Query = 29,
        SkillLevel = 30,
        RadialMenuEntry = 31,
        Tooltip = 32,
        DispelCheck = 33,
        GetDefenderConcealmentMissChance = 34, // defender bonus (e.g. if defender is invisible)
        BaseCasterLevelMod = 35,
        D20ActionCheck = 36,
        D20ActionPerform = 37,
        D20ActionOnActionFrame = 38,
        DestructionDomain = 39,
        GetMoveSpeedBase = 40,
        GetMoveSpeed = 41,
        AbilityCheckModifier = 42, // only used in Trip as far as I can tell for stuff like Sickened condition
        GetAttackerConcealmentMissChance = 43, // attacker penalty (e.g. if attacker is blind). Also applies to stuff like Blink spell
        CountersongSaveThrow = 44,
        SpellResistanceMod = 45,
        SpellDcBase = 46, // haven't seen this actually used, just the mod dispatch (for Spell Focus and the Gnome bonus for Illusion spells)
        SpellDcMod = 47,
        BeginRound = 48, // immediately followed by the OnBeginRound spell trigger. Commonly used for spell countdown / removal when finished
        ReflexThrow = 49,
        DeflectArrows = 50,
        GetNumAttacksBase = 51,
        GetBonusAttacks = 52,
        GetCritterNaturalAttacksNum = 53,
        ObjectEvent = 54, // Enter or leaving the area of effect of an object event
        ProjectileCreated = 55, // Used to create the particle effects for arrows and such
        ProjectileDestroyed = 56, // Used to stop the particle effects for arrows
        Unused57 = 57, // Unused
        Unused58 = 58, // Unused
        GetAbilityLoss = 59,

        GetAttackDice = 60,
        GetLevel = 61, // Class or Character Level (using stat enum)
        ImmunityTrigger = 62,
        Unused63 = 63,
        SpellImmunityCheck = 64,
        EffectTooltip = 65, // for those little bonus flags on top of portraits
        StatBaseGet = 66, // looks like this is intended to replace StatBaseGet function for Critters with Dispatchers
        WeaponGlowType = 67, // Returns the ID of the weapon glow to use (0 = no glow, 1-10 are specific glow types, check mapobjrenderer)
        ItemForceRemove = 68, // has a single function associated with this - 10104410 int __cdecl ItemForceRemoveCallback_SetItemPadWielderArgs(Dispatcher_Callback_Args args);
        ArmorCheckPenalty = 69, // none exist apparently

        MaxDexAcBonus = 70,
        GetSizeCategory = 71,
        BucklerAcPenalty = 72,

        GetModelScale = 73, // NEW! used for modifying the model scale with messing with internal fields
        PythonQuery = 74, // NEW! for handling python dispatcher queries
        PythonSignal = 75,
        PythonActionCheck = 76,
        PythonActionPerform = 77,
        PythonActionFrame = 78,
        PythonActionAdd = 79, // add to sequence
        PythonAdf = 80, // for expansion
        PythonUnused3 = 81, // for expansion
        PythonUnused4 = 82, // for expansion
        PythonUnused5 = 83, // for expansion
        PythonUnused6 = 84, // for expansion
        PythonUnused7 = 85, // for expansion
        PythonUnused8 = 86, // for expansion
        PythonUnused9 = 87, // for expansion
        SpellListExtension = 88, // NEW! used for extending spell-casting classes by other classes (as with Prestige Classes)
        GetBaseCasterLevel = 89,
        LevelupSystemEvent = 90,


        DealingDamageWeaponlikeSpell = 91,
        ActionCostMod = 92,
        MetaMagicMod = 93,
        SpecialAttack = 94,
        ConfirmCriticalBonus = 95,
        RangeIncrementBonus = 96,
        DealingDamageSpell = 97,
        SpellResistanceCasterLevelCheck = 98,
        TargetSpellDCBonus = 99,
        Count = 100
// used just for size definition purposes
    }

    public static class DispatcherTypes
    {

        private static readonly Dictionary<DispatcherType, Type> IOTypes =
            new Dictionary<DispatcherType, Type>
            {
                {DispatcherType.ConditionAdd, null},
                {DispatcherType.ConditionRemove, null},
                {DispatcherType.ConditionAddPre, typeof(DispIoCondStruct)},
                {DispatcherType.ConditionRemove2, null},
                {DispatcherType.ConditionAddFromD20StatusInit, null},
                {DispatcherType.D20AdvanceTime, typeof(DispIoD20Signal)},
                {DispatcherType.TurnBasedStatusInit, typeof(DispIOTurnBasedStatus)},
                {DispatcherType.Initiative, null},
                {DispatcherType.NewDay, null},
                {DispatcherType.AbilityScoreLevel, typeof(DispIoBonusList)},
                {DispatcherType.GetAC, typeof(DispIoAttackBonus)},
                {DispatcherType.AcModifyByAttacker, typeof(DispIoAttackBonus)},
                {DispatcherType.SaveThrowLevel, typeof(DispIoSavingThrow)},
                {DispatcherType.SaveThrowSpellResistanceBonus, typeof(DispIoSavingThrow)},
                {DispatcherType.ToHitBonusBase, typeof(DispIoAttackBonus)},
                {DispatcherType.ToHitBonus2, typeof(DispIoAttackBonus)},
                {DispatcherType.ToHitBonusFromDefenderCondition, typeof(DispIoAttackBonus)},
                {DispatcherType.DealingDamage, typeof(DispIoDamage)},

                {DispatcherType.TakingDamage, typeof(DispIoDamage)},
                {DispatcherType.DealingDamage2, typeof(DispIoDamage)},
                {DispatcherType.TakingDamage2, typeof(DispIoDamage)},
                {DispatcherType.ReceiveHealing, typeof(DispIoDamage)},
                {DispatcherType.GetCriticalHitRange, typeof(DispIoAttackBonus)},
                {DispatcherType.GetCriticalHitExtraDice, typeof(DispIoAttackBonus)},
                {DispatcherType.CurrentHP, typeof(DispIoBonusList)},
                {DispatcherType.MaxHP, typeof(DispIoBonusList)},
                {DispatcherType.InitiativeMod, typeof(DispIoObjBonus)},
                {DispatcherType.D20Signal, typeof(DispIoD20Signal)},
                {DispatcherType.D20Query, typeof(DispIoD20Query)},
                {DispatcherType.SkillLevel, typeof(DispIoObjBonus)},
                {DispatcherType.RadialMenuEntry, null},
                {DispatcherType.Tooltip, typeof(DispIoTooltip)},
                {DispatcherType.DispelCheck, typeof(DispIoDispelCheck)},
                {DispatcherType.GetDefenderConcealmentMissChance, typeof(DispIoAttackBonus)},
                {DispatcherType.BaseCasterLevelMod, typeof(DispIoD20Query)},
                {DispatcherType.D20ActionCheck, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.D20ActionPerform, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.D20ActionOnActionFrame, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.DestructionDomain, typeof(DispIoD20Signal)},
                {DispatcherType.GetMoveSpeedBase, typeof(DispIoMoveSpeed)},
                {DispatcherType.GetMoveSpeed, typeof(DispIoMoveSpeed)},
                {DispatcherType.AbilityCheckModifier, typeof(DispIoObjBonus)},
                {DispatcherType.GetAttackerConcealmentMissChance, typeof(DispIoObjBonus)},
                {DispatcherType.CountersongSaveThrow, typeof(DispIoSavingThrow)},
                {DispatcherType.SpellResistanceMod, typeof(DispIoBonusAndSpellEntry)},
                {DispatcherType.SpellDcBase, typeof(DispIoBonusAndSpellEntry)},
                {DispatcherType.SpellDcMod, typeof(DispIoBonusAndSpellEntry)},
                {DispatcherType.BeginRound, typeof(DispIoD20Signal)},
                {DispatcherType.ReflexThrow, typeof(DispIoReflexThrow)},
                {DispatcherType.DeflectArrows, typeof(DispIoAttackBonus)},
                {DispatcherType.GetNumAttacksBase, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.GetBonusAttacks, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.GetCritterNaturalAttacksNum, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.ObjectEvent, typeof(DispIoObjEvent)},
                {DispatcherType.ProjectileCreated, typeof(DispIoAttackBonus)},
                {DispatcherType.ProjectileDestroyed, typeof(DispIoAttackBonus)},
                {DispatcherType.GetAbilityLoss, typeof(DispIoAbilityLoss)},
                {DispatcherType.GetAttackDice, typeof(DispIoAbilityLoss)},
                {DispatcherType.GetLevel, typeof(DispIoObjBonus)},
                {DispatcherType.ImmunityTrigger, typeof(DispIoTypeImmunityTrigger)},
                {DispatcherType.SpellImmunityCheck, typeof(DispIoImmunity)},
                {DispatcherType.EffectTooltip, typeof(DispIoEffectTooltip)},
                {DispatcherType.StatBaseGet, typeof(DispIoBonusList)},
                {DispatcherType.WeaponGlowType, typeof(DispIoD20Query)},
                {DispatcherType.ItemForceRemove, null},
                {DispatcherType.ArmorCheckPenalty, typeof(DispIoObjBonus)},
                {DispatcherType.MaxDexAcBonus, typeof(DispIoObjBonus)},
                {DispatcherType.GetSizeCategory, typeof(DispIoD20Query)},
                {DispatcherType.BucklerAcPenalty, typeof(DispIoAttackBonus)},
                {DispatcherType.GetModelScale, typeof(DispIoMoveSpeed)},
                {DispatcherType.PythonQuery, typeof(DispIoD20Query)},
                {DispatcherType.PythonSignal, typeof(DispIoD20Signal)},
                {DispatcherType.PythonActionCheck, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.PythonActionPerform, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.PythonActionFrame, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.PythonActionAdd, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.PythonAdf, typeof(DispIoD20ActionTurnBased)},
                {DispatcherType.SpellListExtension, typeof(EvtObjSpellCaster)},
                {DispatcherType.GetBaseCasterLevel, typeof(EvtObjSpellCaster)},
                {DispatcherType.DealingDamageWeaponlikeSpell, typeof(DispIoDamage)},
                {DispatcherType.ActionCostMod, typeof(EvtObjActionCost)},
                {DispatcherType.MetaMagicMod, typeof(EvtObjMetaMagic)},
                {DispatcherType.SpecialAttack, typeof(EvtObjSpecialAttack)},
                {DispatcherType.ConfirmCriticalBonus, typeof(DispIoAttackBonus)},
                {DispatcherType.RangeIncrementBonus, typeof(EvtObjRangeIncrementBonus)},
                {DispatcherType.DealingDamageSpell, typeof(EvtObjDealingSpellDamage)},
                {DispatcherType.SpellResistanceCasterLevelCheck, typeof(EvtObjSpellTargetBonus)},
                {DispatcherType.LevelupSystemEvent, typeof(EvtObjSpellCaster)},
                {DispatcherType.TargetSpellDCBonus, typeof(EvtObjSpellTargetBonus)},

            };

        public static Type GetDispIoType(DispatcherType type) => IOTypes[type];

    }

}