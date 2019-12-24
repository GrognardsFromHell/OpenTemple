using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    [AutoRegister]
    public static partial class SpellEffects
    {
        private static ConditionSpec.Builder CreateSpellEffect(string name, int spellEnum, int numArgs)
        {
            Trace.Assert(numArgs >= 1); // Arg1 is always the spell id

            return ConditionSpec.Create(name, numArgs)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, HasSpellEffectActive, spellEnum);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100d3620)]
        private static void HasSpellEffectActive(in DispatcherCallbackArgs evt, int spellEnum)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1 && dispIo.data1 == spellEnum)
            {
                dispIo.return_val = 1;
            }
        }

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x102d0160)]
        public static readonly ConditionSpec SpellHoldTouchSpell = ConditionSpec.Create("sp-Hold Touch Spell", 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 0)
            .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, HoldTouchSpellTouchAttackHandler)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 0, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 0)
            .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 0)
            .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
            .Build();


        [TempleDllLocation(0x102d02c0)]
        public static readonly ConditionSpec SpellConcentrating = ConditionSpec.Create("sp-Concentrating", 3)
            .RemovedBy(SpellConcentrating)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Concentrating,
                CommonConditionCallbacks.QueryReturnSpellId)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellConcentrating)
            .AddHandler(DispatcherType.TakingDamage2, ConcentratingOnDamage2, 2)
            .AddSignalHandler(D20DispatcherKey.SIG_Sequence, OnSequenceConcentrating, 2)
            .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, ConcentratingActionRecipient, 2)
            .AddSignalHandler(D20DispatcherKey.SIG_Remove_Concentration, Spell_remove_mod, 2)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 2)
            .AddHandler(DispatcherType.Tooltip, ConcentratingTooltipCallback, 98)
            .AddHandler(DispatcherType.RadialMenuEntry, ConcentratingRadialMenu, 2)
            .Build();


        [TempleDllLocation(0x102d03b8)]
        public static readonly ConditionSpec SpellAid = CreateSpellEffect("sp-Aid", WellKnownSpells.Aid, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 3)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpAidOnAdd)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 3)
            .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 1, 142)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowModifierCallback, 1, 142)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, Spell_remove_spell, 3, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, Spell_remove_mod, 3)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 3, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 3)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 8, 0)
            .Build();


        [TempleDllLocation(0x102d04f0)]
        public static readonly ConditionSpec SpellAnimalFriendship =
            CreateSpellEffect("sp-Animal Friendship", WellKnownSpells.AnimalFriendship, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 4)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellAnimalFriendship)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CBF10)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 4, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 4, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 4)
                .Build();


        [TempleDllLocation(0x102d0610)]
        public static readonly ConditionSpec SpellAnimalGrowth =
            CreateSpellEffect("sp-Animal Growth", WellKnownSpells.AnimalGrowth, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellAnimalGrowth)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 5)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 5)
                .AddHandler(DispatcherType.ConditionAdd, enlargeModelScaleInc)
                .AddHandler(DispatcherType.ToHitBonus2, RighteousMightToHitBonus, 1, 274)
                .AddHandler(DispatcherType.GetAC, sub_100C6050, 1, 274)
                .AddHandler(DispatcherType.GetAC, sub_100C6080, 2, 274)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_AnimalGrowth, Stat.strength, 8)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_AnimalGrowth, Stat.constitution, 4)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_AnimalGrowth, Stat.dexterity, 2)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 4, 274)
                .AddHandler(DispatcherType.TakingDamage, AnimalGrowthDamageResistance)
                .AddHandler(DispatcherType.GetAttackDice, AttackDiceAnimalGrowth, 0, 274)
                .AddHandler(DispatcherType.GetSizeCategory, EnlargeSizeCategory, 0, 274)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 5, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 5)
                .Build();


        [TempleDllLocation(0x102d07c0)]
        public static readonly ConditionSpec SpellAnimalTrance =
            CreateSpellEffect("sp-Animal Trance", WellKnownSpells.AnimalTrance, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 6)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellAnimalTrance)
                .AddHandler(DispatcherType.ConditionAdd, AnimalTranceBeginSpell)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 6, SpellAnimalTrance)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 6, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 6)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 6, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 6)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 89, 0)
                .Build();


        [TempleDllLocation(0x102d0920)]
        public static readonly ConditionSpec SpellAnimateDead =
            CreateSpellEffect("sp-Animate Dead", WellKnownSpells.AnimateDead, 4)
                .AddHandler(DispatcherType.ConditionAddPre, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, AnimateDeadOnAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 7)
                .Build();


        [TempleDllLocation(0x102d0e00)]
        public static readonly ConditionSpec SpellBane = CreateSpellEffect("sp-Bane", WellKnownSpells.Bane, 3)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellBless)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 8)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CC220)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 8)
            .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 1, 152)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowModifierCallback, 1, 152)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 8, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 8)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 92, 0)
            .Build();


        [TempleDllLocation(0x102d0f20)]
        public static readonly ConditionSpec SpellBarkskin =
            CreateSpellEffect("sp-Barkskin", WellKnownSpells.Barkskin, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 9)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 9)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 10, 141)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 9, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 9)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 9, 0)
                .Build();


        [TempleDllLocation(0x102d1360)]
        public static readonly ConditionSpec SpellBestowCurseAbility =
            CreateSpellEffect("sp-Bestow Curse Ability", WellKnownSpells.BestowCurse, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
                .AddHandler(DispatcherType.DispelCheck, BreakEnchantmentDispelCheck, 10)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC240)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellBestowCurseAbility)
                .AddHandler(DispatcherType.AbilityScoreLevel, BestowCurseAbilityMalus, 0, 243)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 51, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltipBestowCurse, 106)
                .Build();


        [TempleDllLocation(0x102d1008)]
        public static readonly ConditionSpec SpellBestowCurseRolls =
            CreateSpellEffect("sp-Bestow Curse Rolls", WellKnownSpells.BestowCurse, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
                .AddHandler(DispatcherType.DispelCheck, BreakEnchantmentDispelCheck, 11)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellBestowCurseRolls)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC2A0)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C6200, 4, 243)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_sp_BestowCurseRolls_Callback, 4, 243)
                .AddHandler(DispatcherType.AbilityCheckModifier, CommonConditionCallbacks.EncumbranceSkillLevel, 4, 243)
                .AddHandler(DispatcherType.SkillLevel, SkillModifier_BestowCurseRolls_Callback, 4, 243)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 51, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltipBlindnessDeafness, 106, 197)
                .Build();


        [TempleDllLocation(0x102d1140)]
        public static readonly ConditionSpec SpellBestowCurseActions =
            CreateSpellEffect("sp-Bestow Curse Actions", WellKnownSpells.BestowCurse, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
                .AddHandler(DispatcherType.DispelCheck, BreakEnchantmentDispelCheck, 12)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellBestowCurseAbility)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC2A0)
                .AddHandler(DispatcherType.TurnBasedStatusInit, BestowCurseActionsTurnBasedStatusInit)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 51, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 106, 196)
                .Build();


        [TempleDllLocation(0x102d0cb8)]
        public static readonly ConditionSpec SpellBless = CreateSpellEffect("sp-Bless", WellKnownSpells.Bless, 3)
            .Prevents(SpellVrockSpores)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellBane)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 13)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .SetQueryResult(D20DispatcherKey.QUE_Obj_Is_Blessed, true)
            .AddHandler(DispatcherType.ConditionAdd, BlessOnAdd)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 13)
            .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 1, 142)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowModifierCallback, 1, 142)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 13, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 13)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 10, 0)
            .Build();


        [TempleDllLocation(0x102d1528)]
        public static readonly ConditionSpec SpellBlindness =
            CreateSpellEffect("sp-Blindness", WellKnownSpells.BlindnessDeafness, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .SetUnique()
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveBlindness)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBlindness)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC2E0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 189)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
                .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
                .AddHandler(DispatcherType.GetAttackerConcealmentMissChance,
                    CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition,
                    CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 14, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 14)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltipBlindnessDeafness, 93, 76)
                .Build();


        [TempleDllLocation(0x102d1728)]
        public static readonly ConditionSpec SpellBlink = CreateSpellEffect("sp-Blink", WellKnownSpells.Blink, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 15)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, BlinkSpellFailure)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBlink)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 15)
            .AddHandler(DispatcherType.GetMoveSpeed, sub_100C6280)
            .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, BlinkMissChance, 0, 254)
            .AddHandler(DispatcherType.GetAttackerConcealmentMissChance, sub_100C62A0, 20)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 15, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 15)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 110, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 11, 0)
            .Build();


        [TempleDllLocation(0x102d18b0)]
        public static readonly ConditionSpec SpellBlur = CreateSpellEffect("sp-Blur", WellKnownSpells.Blur, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 16)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 16)
            .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, sub_100C5BE0, 20, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 16, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 16)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 12, 0)
            .Build();


        [TempleDllLocation(0x102d19c0)]
        public static readonly ConditionSpec SpellBreakEnchantment =
            CreateSpellEffect("sp-Break Enchantment", WellKnownSpells.BreakEnchantment, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BreakEnchantmentInit, 17)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 17, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 17)
                .Build();


        [TempleDllLocation(0x102d1a68)]
        public static readonly ConditionSpec SpellBullsStrength =
            CreateSpellEffect("sp-Bulls Strength", WellKnownSpells.BullsStrength, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 18)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 18)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.strength, 217)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 18, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 18)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 13, 0)
                .Build();


        [TempleDllLocation(0x102d1b50)]
        public static readonly ConditionSpec SpellCallLightning =
            CreateSpellEffect("sp-Call Lightning", WellKnownSpells.CallLightning, 3)
                .SetUnique()
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 19)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Can_Call_Lightning, sub_100C6440)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellCastLightning)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 19)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Call_Lightning, sub_100DCA80, 19)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 19, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 19)
                .AddHandler(DispatcherType.Tooltip, CallLightningTooltipCallback, 108, 19)
                .AddHandler(DispatcherType.RadialMenuEntry, CallLightningRadial, 19)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 87, 0)
                .Build();


        [TempleDllLocation(0x102d1c98)]
        public static readonly ConditionSpec SpellCallLightningStorm =
            CreateSpellEffect("sp-Call Lightning Storm", WellKnownSpells.CallLightningStorm, 3)
                .SetUnique()
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 19)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Can_Call_Lightning, sub_100C6440)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellCastLightning)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 19)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Call_Lightning, sub_100DCA80, 19)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 19, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 19)
                .AddHandler(DispatcherType.Tooltip, CallLightningTooltipCallback, 108, 19)
                .AddHandler(DispatcherType.RadialMenuEntry, CallLightningStormRadial, 19)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 90, 0)
                .Build();


        [TempleDllLocation(0x102d1de0)]
        public static readonly ConditionSpec SpellCalmAnimals =
            CreateSpellEffect("sp-Calm Animals", WellKnownSpells.CalmAnimals, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 20)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellCalmAnimals)
                .AddHandler(DispatcherType.ConditionAdd, StunnedFloatMessage)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, sub_100CB8E0, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 224)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 20, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 20)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 20, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 20, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 20)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 89, 0)
                .Build();


        [TempleDllLocation(0x102d1f78)]
        public static readonly ConditionSpec SpellCalmEmotions =
            CreateSpellEffect("sp-Calm Emotions", WellKnownSpells.CalmEmotions, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 21)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellCalmEmotions)
                .AddQueryHandler(D20DispatcherKey.QUE_IsActionInvalid_CheckAction, CalmEmotionsActionInvalid,
                    SpellCalmEmotions)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, CalmEmotionsBeginSpell)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 21, SpellCalmEmotions)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 21, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 21)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 21, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 21, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 21)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 105, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 94, 0)
                .Build();


        [TempleDllLocation(0x102d2110)]
        public static readonly ConditionSpec SpellCatsGrace =
            CreateSpellEffect("sp-Cats Grace", WellKnownSpells.CatsGrace, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 22)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 22)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.dexterity, 218)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 22, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 22)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                .Build();


        [TempleDllLocation(0x102d21f8)]
        public static readonly ConditionSpec SpellCauseFear =
            CreateSpellEffect("sp-Cause Fear", WellKnownSpells.CauseFear, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 23)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, IsCritterAfraidQuery)
                .AddHandler(DispatcherType.ConditionAdd, FloatMessageAfraid)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 23)
                .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 2, 172)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowModifierCallback, 2, 172)
                .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 2, 172)
                .AddHandler(DispatcherType.AbilityCheckModifier, AbilityCheckModifierEmotion, 2, 172)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 23, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 23)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 52, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 117, 0)
                .Build();


        [TempleDllLocation(0x102d2358)]
        public static readonly ConditionSpec SpellChaosHammer =
            CreateSpellEffect("sp-Chaos Hammer", WellKnownSpells.ChaosHammer, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 24)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.NONE)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellChaosHammer)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC500)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 24)
                .AddHandler(DispatcherType.TurnBasedStatusInit, ChaosHammerTurnBasedStatusInit)
                .AddHandler(DispatcherType.ToHitBonus2, ChaosHammer_ToHit_Callback, 2, 282)
                .AddHandler(DispatcherType.DealingDamage, sub_100C40F0, 2, 282)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowPenaltyCallback, 2, 282)
                .AddHandler(DispatcherType.GetAC, ChaosHammerAcBonus, 2, 282)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 24, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 24)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 65, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 166, 0)
                .Build();


        [TempleDllLocation(0x102d24c8)]
        public static readonly ConditionSpec SpellCharmMonster =
            CreateSpellEffect("sp-Charm Monster", WellKnownSpells.CharmMonster, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellCharmMonster)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 25)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellCharmMonster)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellCharmMonster)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 25)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 25, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 25, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 25)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 25, SpellCharmMonster)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 95, 0)
                .Build();


        [TempleDllLocation(0x102d2638)]
        public static readonly ConditionSpec SpellCharmPerson =
            CreateSpellEffect("sp-Charm Person", WellKnownSpells.CharmPerson, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellCharmPerson)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 26)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellCharmPerson)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellCharmPerson)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 26)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 26, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 26, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 26)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 26, SpellCharmPerson)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 97, 0)
                .Build();


        [TempleDllLocation(0x102d27a8)]
        public static readonly ConditionSpec SpellCharmPersonorAnimal =
            CreateSpellEffect("sp-Charm Person or Animal", WellKnownSpells.CharmPersonOrAnimal, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellCharmPersonorAnimal)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 27)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellCharmPersonorAnimal)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC640)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 27)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 27, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 27, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 27)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 27, SpellCharmPersonorAnimal)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 96, 0)
                .Build();


        [TempleDllLocation(0x102d2a28)]
        public static readonly ConditionSpec SpellChillMetal =
            CreateSpellEffect("sp-Chill Metal", WellKnownSpells.ChillMetal, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeatMetal)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 28)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 28)
                .AddHandler(DispatcherType.BeginRound, ChillMetalDamage, 28)
                .AddHandler(DispatcherType.TakingDamage, ChillMetalDamageResistance)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 28, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 28)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 165, 0)
                .Build();


        [TempleDllLocation(0x102d2b38)]
        public static readonly ConditionSpec SpellChillTouch =
            CreateSpellEffect("sp-Chill Touch", WellKnownSpells.ChillTouch, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 29)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, ChillTouchAttackHandler, 29)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 29, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 29)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 29)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 29, SpellChillTouch)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 29)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 29)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .Build();


        [TempleDllLocation(0x102d2ca8)]
        public static readonly ConditionSpec SpellClairaudienceClairvoyance =
            CreateSpellEffect("sp-Clairaudience Clairvoyance", WellKnownSpells.ClairaudienceClairvoyance, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 30)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellClairaudienceClairvoyance)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC6D0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 30)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 30, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 30)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 86, 0)
                .Build();


        [TempleDllLocation(0x102d2dc8)]
        public static readonly ConditionSpec SpellCloudkill =
            CreateSpellEffect("sp-Cloudkill", WellKnownSpells.Cloudkill, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 31)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellCloudkill)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 31)
                .AddHandler(DispatcherType.ObjectEvent, AoeObjEventCloudkill, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 31)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d2eb0)]
        public static readonly ConditionSpec SpellCloudkillDamage =
            CreateSpellEffect("sp-Cloudkill-Damage", WellKnownSpells.Cloudkill, 3)
                .AddHandler(DispatcherType.ConditionAddPre, CloudkillDamagePreAdd, SpellCloudkillDamage)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellCloudkillDamage)
                .AddHandler(DispatcherType.BeginRound, CloudkillBeginRound)
                .AddHandler(DispatcherType.ObjectEvent, AoeObjEventCloudkill, 32)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 32, SpellCloudkill)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 32)
                .Build();


        [TempleDllLocation(0x102d2f98)]
        public static readonly ConditionSpec SpellColorSprayBlind =
            CreateSpellEffect("sp-Color Spray Blind", WellKnownSpells.ColorSpray, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellColorSprayBlind)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellColorSprayBlind)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 33)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
                .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
                .AddHandler(DispatcherType.GetAttackerConcealmentMissChance,
                    CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition,
                    CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 33, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 33)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 100, 0)
                .Build();


        [TempleDllLocation(0x102d3130)]
        public static readonly ConditionSpec SpellColorSprayStun =
            CreateSpellEffect("sp-Color Spray Stun", WellKnownSpells.ColorSpray, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC930)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 34)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, sub_100CB8E0, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 224)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 34, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 34)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 89, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 101, 0)
                .Build();


        [TempleDllLocation(0x102d3278)]
        public static readonly ConditionSpec SpellColorSprayUnconscious =
            CreateSpellEffect("sp-Color Spray Unconscious", WellKnownSpells.ColorSpray, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
                .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
                .SetQueryResult(D20DispatcherKey.QUE_CoupDeGrace, true)
                .SetQueryResult(D20DispatcherKey.QUE_Unconscious, true)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.ConditionAdd, ColorsprayUnconsciousOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 35)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 35, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 35)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 38, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 102, 0)
                .Build();


        [TempleDllLocation(0x102d3400)]
        public static readonly ConditionSpec SpellCommand = CreateSpellEffect("sp-Command", WellKnownSpells.Command, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 36)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCommand)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, sub_100C6B80)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommandTurnBasedStatusInit)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 36)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 36, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 36)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 163, 0)
            .Build();


        [TempleDllLocation(0x102d3510)]
        public static readonly ConditionSpec SpellConfusion =
            CreateSpellEffect("sp-Confusion", WellKnownSpells.Confusion, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .Prevents(SpellCalmEmotions)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 37)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_AI_Has_Spell_Override, ConfusionHasAiOverride)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Confused, sub_100C6D60)
                .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC800)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 37)
                .AddHandler(DispatcherType.TurnBasedStatusInit, ConfusionStartTurn)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 37, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 37)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 113, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 104, 0)
                .Build();


        [TempleDllLocation(0x102d36a8)]
        public static readonly ConditionSpec SpellConsecrate =
            CreateSpellEffect("sp-Consecrate", WellKnownSpells.Consecrate, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 38)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellConsecrate)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 38)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__consecrate_sthg, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 38)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d3778)]
        public static readonly ConditionSpec SpellConsecrateHit =
            CreateSpellEffect("sp-Consecrate Hit", WellKnownSpells.Consecrate, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellConsecrateHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellConsecrateHit)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground, sub_100C6F70, 3, 6)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__consecrate_sthg, 39)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 39, SpellConsecrate)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 39)
                .Build();


        [TempleDllLocation(0x102d3838)]
        public static readonly ConditionSpec SpellConsecrateHitUndead =
            CreateSpellEffect("sp-Consecrate Hit Undead", WellKnownSpells.Consecrate, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellConsecrateHitUndead)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellConsecrateHitUndead)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C6F10, 1, 236)
                .AddHandler(DispatcherType.DealingDamage, ConsecrateHitUndeadDealingDamage, 1, 236)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_sp_ConsecrateHitUndead_Callback, 1, 236)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__consecrate_sthg, 40)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 40, SpellConsecrate)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 40)
                .Build();


        [TempleDllLocation(0x102d3948)]
        public static readonly ConditionSpec SpellControlPlants =
            CreateSpellEffect("sp-Control Plants", WellKnownSpells.ControlPlants, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 41)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellControlPlants)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 41)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 41)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d3a30)]
        public static readonly ConditionSpec SpellControlPlantsTracking =
            CreateSpellEffect("sp-Control Plants Tracking", WellKnownSpells.ControlPlants, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 42)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.SkillLevel, sub_100C7140, 10, 226)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 42, SpellControlPlants)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 42)
                .Build();


        [TempleDllLocation(0x102d3af0)]
        public static readonly ConditionSpec SpellControlPlantsCharm =
            CreateSpellEffect("sp-Control Plants Charm", WellKnownSpells.ControlPlants, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsCharm)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellControlPlantsCharm)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CCD80)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 43)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 43, SpellControlPlants)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 43)
                .Build();


        [TempleDllLocation(0x102d3c00)]
        public static readonly ConditionSpec SpellControlPlantsDisentangle =
            CreateSpellEffect("sp-Control Plants Disentangle", WellKnownSpells.ControlPlants, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsDisentangle)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellControlPlantsDisentangle)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 44)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 44, SpellControlPlants)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 44)
                .Build();


        [TempleDllLocation(0x102d3cd0)]
        public static readonly ConditionSpec SpellControlPlantsEntanglePre =
            CreateSpellEffect("sp-Control Plants Entangle Pre", WellKnownSpells.ControlPlants, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsEntanglePre)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellControlPlantsEntanglePre)
                .AddHandler(DispatcherType.BeginRound, ControlPlantsEntangleBeginRound, 45)
                .AddHandler(DispatcherType.GetMoveSpeed, sub_100C7040, 0, 228)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 45)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 45, SpellControlPlants)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 45)
                .Build();


        [TempleDllLocation(0x102d3dc8)]
        public static readonly ConditionSpec SpellControlPlantsEntangle =
            CreateSpellEffect("sp-Control Plants Entangle", WellKnownSpells.ControlPlants, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsEntangle)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, ControlPlantsEntangleSpellInterruptedCheck)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellControlPlantsEntangle)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CCE10)
                .AddHandler(DispatcherType.TurnBasedStatusInit, OnBeginRoundDisableMovement)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, true)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C6200, 2, 228)
                .AddHandler(DispatcherType.AbilityScoreLevel, EntangleAttributeMalus, Stat.dexterity, 228)
                .AddHandler(DispatcherType.GetMoveSpeed, ControlPlantsEntangleMovementSpeed)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 46)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_BreakFree, ControlPlantsEntagleBreakFree, 46)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 46, SpellControlPlants)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 46)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 99, 0)
                .AddHandler(DispatcherType.RadialMenuEntry, CommonConditionCallbacks.BreakFreeRadial, 46)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 105, 0)
                .Build();


        [TempleDllLocation(0x102d3f88)]
        public static readonly ConditionSpec SpellDarkvision =
            CreateSpellEffect("sp-Darkvision", WellKnownSpells.Darkvision, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 47)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Darkvision, true)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 47)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 47, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 47)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 16, 0)
                .Build();


        [TempleDllLocation(0x102d4070)]
        public static readonly ConditionSpec SpellDaze = CreateSpellEffect("sp-Daze", WellKnownSpells.Daze, 3)
            .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 48)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CCE50)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 48)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 48, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 48)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 72, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 107, 0)
            .Build();


        [TempleDllLocation(0x102d43c0)]
        public static readonly ConditionSpec SpellDeathWard =
            CreateSpellEffect("sp-Death Ward", WellKnownSpells.DeathWard, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 50)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellNegativeEnergyProtection)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 50)
                .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 50, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 50)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 18, 0)
                .Build();


        [TempleDllLocation(0x102d41b8)]
        public static readonly ConditionSpec SpellDeathKnell =
            CreateSpellEffect("sp-Death Knell", WellKnownSpells.DeathKnell, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 49)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, DeathKnellBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 49)
                .AddHandler(DispatcherType.BaseCasterLevelMod, sub_100C71A0, 49, 1)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.strength, 188)
                .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, sub_100C71C0, 49)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 49, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 49)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 17, 0)
                .Build();


        [TempleDllLocation(0x102d4588)]
        public static readonly ConditionSpec SpellDeafness =
            CreateSpellEffect("sp-Deafness", WellKnownSpells.BlindnessDeafness, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveDeafness)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Deafened, true)
                .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, DeafnessSpellFailure)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, DeafenedFloatMsg)
                .AddHandler(DispatcherType.InitiativeMod, DeafnessInitiativeMod, 4, 190)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 51, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 51)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 77, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltipBlindnessDeafness, 162, 77)
                .Build();


        [TempleDllLocation(0x102d46e8)]
        public static readonly ConditionSpec SpellDelayPoison =
            CreateSpellEffect("sp-Delay Poison", WellKnownSpells.DelayPoison, 3)
                .Prevents(SpellVrockSpores)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellDelayPoison)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 52)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 52, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 52)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 19, 0)
                .Build();


        [TempleDllLocation(0x102d47d0)]
        public static readonly ConditionSpec SpellDesecrate =
            CreateSpellEffect("sp-Desecrate", WellKnownSpells.Desecrate, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 53)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellDesecrate)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 53)
                .AddHandler(DispatcherType.ObjectEvent, ObjEventAoEDesecrate, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 53)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d48a0)]
        public static readonly ConditionSpec SpellDesecrateHit =
            CreateSpellEffect("sp-Desecrate Hit", WellKnownSpells.Desecrate, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellDesecrateHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellDesecrateHit)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground, sub_100C6F70, 3, 6)
                .AddHandler(DispatcherType.ObjectEvent, ObjEventAoEDesecrate, 54)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 54, SpellDesecrate)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 54)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 109, 0)
                .Build();


        [TempleDllLocation(0x102d4998)]
        public static readonly ConditionSpec SpellDesecrateHitUndead =
            CreateSpellEffect("sp-Desecrate Hit Undead", WellKnownSpells.Desecrate, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellDesecrateHitUndead)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellDesecrateHitUndead)
                .AddHandler(DispatcherType.ConditionAdd, d20_mods_spells__desecrate_undead_temp_hp, 1, 2)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C7330, 1, 235)
                .AddHandler(DispatcherType.DealingDamage, AddBonusType17, 1, 235)
                .AddHandler(DispatcherType.SaveThrowLevel, sub_100C7300, 1, 235)
                .AddHandler(DispatcherType.ObjectEvent, ObjEventAoEDesecrate, 55)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, sub_100C71C0, 55)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 55, SpellDesecrate)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 55)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 108, 0)
                .Build();


        [TempleDllLocation(0x102d4ae0)]
        public static readonly ConditionSpec SpellDetectChaos =
            CreateSpellEffect("sp-Detect Chaos", WellKnownSpells.DetectChaos, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 56)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Chaos, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 56)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 56, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 56)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 56, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 56)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 20, 0)
                .Build();


        [TempleDllLocation(0x102d4c18)]
        public static readonly ConditionSpec SpellDetectEvil =
            CreateSpellEffect("sp-Detect Evil", WellKnownSpells.DetectEvil, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 57)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Evil, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 57)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 57, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 57)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 57, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 57)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 21, 0)
                .Build();


        [TempleDllLocation(0x102d4d50)]
        public static readonly ConditionSpec SpellDetectGood =
            CreateSpellEffect("sp-Detect Good", WellKnownSpells.DetectGood, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 58)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Good, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 58)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 58, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 58)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 58, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 58)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 21, 0)
                .Build();


        [TempleDllLocation(0x102d4e88)]
        public static readonly ConditionSpec SpellDetectLaw =
            CreateSpellEffect("sp-Detect Law", WellKnownSpells.DetectLaw, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 59)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Law, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 59)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 59, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 59)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 59, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 59)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 22, 0)
                .Build();


        [TempleDllLocation(0x102d4fc0)]
        public static readonly ConditionSpec SpellDetectMagic =
            CreateSpellEffect("sp-Detect Magic", WellKnownSpells.DetectMagic, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 60)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Magic, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 60)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 60, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 60)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 60, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 60)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 23, 0)
                .Build();


        [TempleDllLocation(0x102d50f8)]
        public static readonly ConditionSpec SpellDetectSecretDoors =
            CreateSpellEffect("sp-Detect Secret Doors", WellKnownSpells.DetectSecretDoors, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 61)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 61)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 61, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 61)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 61, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 61)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 24, 0)
                .Build();


        [TempleDllLocation(0x102d5218)]
        public static readonly ConditionSpec SpellDetectUndead =
            CreateSpellEffect("sp-Detect Undead", WellKnownSpells.DetectUndead, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 62)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Undead, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 62)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 62, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 62)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 62, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 62)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 25, 0)
                .Build();


        [TempleDllLocation(0x102d5350)]
        public static readonly ConditionSpec SpellDimensionalAnchor =
            CreateSpellEffect("sp-Dimensional Anchor", WellKnownSpells.DimensionalAnchor, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 63)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellDimensionalAnchor)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 63)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 63, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 63)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 161, 0)
                .Build();


        [TempleDllLocation(0x102d5438)]
        public static readonly ConditionSpec SpellDiscernLies =
            CreateSpellEffect("sp-Discern Lies", WellKnownSpells.DiscernLies, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 64)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Discern_Lies, true)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 64)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 64, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 64)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 26, 0)
                .Build();


        [TempleDllLocation(0x102d5520)]
        public static readonly ConditionSpec SpellDispelAir =
            CreateSpellEffect("sp-Dispel Air", WellKnownSpells.DispelAir, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 65)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 65)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 268)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 65)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 65, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 65)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 65)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 65)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 65)
                .Build();


        [TempleDllLocation(0x102d56a8)]
        public static readonly ConditionSpec SpellDispelEarth =
            CreateSpellEffect("sp-Dispel Earth", WellKnownSpells.DispelEarth, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 66)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 66)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 269)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 66)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 66, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 66)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 66)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 66)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 66)
                .Build();


        [TempleDllLocation(0x102d5830)]
        public static readonly ConditionSpec SpellDispelFire =
            CreateSpellEffect("sp-Dispel Fire", WellKnownSpells.DispelFire, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 67)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 67)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 270)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 67)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 67, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 67)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 67)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 67)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 67)
                .Build();


        [TempleDllLocation(0x102d59b8)]
        public static readonly ConditionSpec SpellDispelWater =
            CreateSpellEffect("sp-Dispel Water", WellKnownSpells.DispelWater, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 68)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 68)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 271)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 68)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 68, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 68)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 68)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 68)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 68)
                .Build();


        [TempleDllLocation(0x102d5b40)]
        public static readonly ConditionSpec SpellDispelChaos =
            CreateSpellEffect("sp-Dispel Chaos", WellKnownSpells.DispelChaos, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 69)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 69)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 175)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 69)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 69, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 69)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 69)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 69)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 69)
                .Build();


        [TempleDllLocation(0x102d5cc8)]
        public static readonly ConditionSpec SpellDispelEvil =
            CreateSpellEffect("sp-Dispel Evil", WellKnownSpells.DispelEvil, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 70)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 70)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 176)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 70)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 70, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 70)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 70)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 70)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 70)
                .Build();


        [TempleDllLocation(0x102d5e50)]
        public static readonly ConditionSpec SpellDispelGood =
            CreateSpellEffect("sp-Dispel Good", WellKnownSpells.DispelGood, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 71)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 71)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 177)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 71)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 71, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 71)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 71)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 71)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 71)
                .Build();


        [TempleDllLocation(0x102d5fd8)]
        public static readonly ConditionSpec SpellDispelLaw =
            CreateSpellEffect("sp-Dispel Law", WellKnownSpells.DispelLaw, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 72)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 72)
                .AddHandler(DispatcherType.GetAC, DispelAlignmentAcBonus, 4, 178)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, DispelAlignmentTouchAttackSignalHandler, 72)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 72, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 72)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 72)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 72)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 72)
                .Build();


        [TempleDllLocation(0x102d6160)]
        public static readonly ConditionSpec SpellDispelMagic =
            CreateSpellEffect("sp-Dispel Magic", WellKnownSpells.DispelMagic, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, DispelMagicOnAdd, 73)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 73, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 73)
                .Build();


        [TempleDllLocation(0x102d6208)]
        public static readonly ConditionSpec SpellDisplacement =
            CreateSpellEffect("sp-Displacement", WellKnownSpells.Displacement, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 74)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 74)
                .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, sub_100C5BE0, 50, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 74, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 74)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 27, 0)
                .Build();


        [TempleDllLocation(0x102d62f0)]
        public static readonly ConditionSpec SpellDivineFavor =
            CreateSpellEffect("sp-Divine Favor", WellKnownSpells.DivineFavor, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 75)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CD0E0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 75)
                .AddHandler(DispatcherType.ToHitBonus2, DivineFavorToHitBonus2, 0, 170)
                .AddHandler(DispatcherType.DealingDamage, sub_100C4AF0, 0, 170)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 75, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 75)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 28, 0)
                .Build();


        [TempleDllLocation(0x102d6400)]
        public static readonly ConditionSpec SpellDivinePower =
            CreateSpellEffect("sp-Divine Power", WellKnownSpells.DivinePower, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 76)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellDivinePower)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 76)
                .AddHandler(DispatcherType.ToHitBonusBase, DivinePowerToHitBonus, 0, 250)
                .AddHandler(DispatcherType.AbilityScoreLevel, DivinePowerStrengthBonus, Stat.strength, 250)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 76, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 76)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 29, 0)
                .Build();


        [TempleDllLocation(0x102d6510)]
        public static readonly ConditionSpec SpellDominateAnimal =
            CreateSpellEffect("sp-Dominate Animal", WellKnownSpells.DominateAnimal, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 77)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellDominateAnimal)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, DominateAnimal)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 77)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 77, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 77)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 160, 0)
                .Build();


        [TempleDllLocation(0x102d6630)]
        public static readonly ConditionSpec SpellDominatePerson =
            CreateSpellEffect("sp-Dominate Person", WellKnownSpells.DominatePerson, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 78)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellDominatePerson)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
                .AddHandler(DispatcherType.ConditionAdd, Dominate_Person)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 78)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 78, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 78)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 160, 0)
                .Build();


        [TempleDllLocation(0x102d6750)]
        public static readonly ConditionSpec SpellDoom = CreateSpellEffect("sp-Doom", WellKnownSpells.Doom, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 79)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CD370)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 79)
            .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 2, 169)
            .AddHandler(DispatcherType.DealingDamage, EmotionDamageBonus, 2, 169)
            .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 2, 169)
            .AddHandler(DispatcherType.AbilityCheckModifier, AbilityCheckModifierEmotion, 2, 169)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowPenaltyCallback, 2, 169)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 79, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 79)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 111, 0)
            .Build();


        [TempleDllLocation(0x102d6898)]
        public static readonly ConditionSpec SpellEaglesSplendor =
            CreateSpellEffect("sp-Eagles Splendor", WellKnownSpells.EaglesSplendor, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 80)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 80)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.charisma, 292)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 80, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 80)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                .Build();


        [TempleDllLocation(0x102d6b30)]
        public static readonly ConditionSpec SpellEmotionDespair =
            CreateSpellEffect("sp-Emotion Despair", WellKnownSpells.CrushingDespair, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionHope)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 81)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEmotionDespair)
                .AddHandler(DispatcherType.ConditionAdd, EmotionBeginSpell, 81, 0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 81)
                .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 2, 259)
                .AddHandler(DispatcherType.DealingDamage, EmotionDamageBonus, 2, 259)
                .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 2, 259)
                .AddHandler(DispatcherType.AbilityCheckModifier, AbilityCheckModifierEmotion, 2, 259)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowEmotionModifierCallback, 2, 259)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 81, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 81, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 81)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 81, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 81)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 112, 0)
                .Build();


        [TempleDllLocation(0x102d6e78)]
        public static readonly ConditionSpec SpellEmotionFear =
            CreateSpellEffect("sp-Emotion Fear", WellKnownSpells.Emotion, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionRage)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 82)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEmotionFear)
                .AddHandler(DispatcherType.ConditionAdd, EmotionBeginSpell, 82, 0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 82)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 82, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 82, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 82)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 82, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 82)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 116, 0)
                .Build();


        [TempleDllLocation(0x102d7108)]
        public static readonly ConditionSpec SpellEmotionFriendship =
            CreateSpellEffect("sp-Emotion Friendship", WellKnownSpells.Emotion, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionHate)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 83)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEmotionFriendship)
                .AddHandler(DispatcherType.ConditionAdd, EmotionBeginSpell, 83, 0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 83)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 83, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 83, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 83)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 83, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 83)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 113, 0)
                .Build();


        [TempleDllLocation(0x102d6fc0)]
        public static readonly ConditionSpec SpellEmotionHate =
            CreateSpellEffect("sp-Emotion Hate", WellKnownSpells.Emotion, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionFriendship)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 84)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEmotionHate)
                .AddHandler(DispatcherType.ConditionAdd, EmotionBeginSpell, 84, 0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 84)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 84, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 84, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 84)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 84, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 84)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 114, 0)
                .Build();


        [TempleDllLocation(0x102d6980)]
        public static readonly ConditionSpec SpellEmotionHope =
            CreateSpellEffect("sp-Emotion Hope", WellKnownSpells.GoodHope, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionDespair)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 85)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEmotionHope)
                .AddHandler(DispatcherType.ConditionAdd, EmotionBeginSpell, 85, 0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 85)
                .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 2, 260)
                .AddHandler(DispatcherType.DealingDamage, EmotionDamageBonus, 2, 260)
                .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 2, 260)
                .AddHandler(DispatcherType.AbilityCheckModifier, AbilityCheckModifierEmotion, 2, 260)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowEmotionModifierCallback, 2, 260)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 85, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 85, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 85)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 85, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 85)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 30, 0)
                .Build();


        [TempleDllLocation(0x102d6ce0)]
        public static readonly ConditionSpec SpellEmotionRage =
            CreateSpellEffect("sp-Emotion Rage", WellKnownSpells.Emotion, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 86)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEmotionRage)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .AddHandler(DispatcherType.ConditionAdd, EmotionBeginSpell, 86, 0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 86)
                .AddHandler(DispatcherType.AbilityScoreLevel, EmotionRageAbilityScore, Stat.strength, 261)
                .AddHandler(DispatcherType.AbilityScoreLevel, EmotionRageAbilityScore, Stat.constitution, 261)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 0, 261)
                .AddHandler(DispatcherType.SaveThrowLevel, sub_100C7490, 1, 261)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 86, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 86, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 86)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 86, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 86)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 115, 0)
                .Build();


        [TempleDllLocation(0x102d7250)]
        public static readonly ConditionSpec SpellEndurance =
            CreateSpellEffect("sp-Endurance", WellKnownSpells.Endurance, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 87)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 87)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.constitution, 186)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 87, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 87)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 31, 0)
                .Build();


        [TempleDllLocation(0x102d7310)]
        public static readonly ConditionSpec SpellEndureElements =
            CreateSpellEffect("sp-Endure Elements", WellKnownSpells.EndureElements, 4)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100C77D0, SpellEndureElements)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 88)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Endure_Elements, sub_100C7820)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CD4B0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 88)
                .AddHandler(DispatcherType.BeginRound, sub_100C7510, 88)
                .AddHandler(DispatcherType.TakingDamage, EndureElementsDamageResistance, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 88, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 88)
                .AddHandler(DispatcherType.Tooltip, Tooltip2Callback, 92, 88)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 32, 0)
                .AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, sub_100C7860)
                .Build();


        [TempleDllLocation(0x102d7608)]
        public static readonly ConditionSpec SpellEnlarge = CreateSpellEffect("sp-Enlarge", WellKnownSpells.Enlarge, 3)
            .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellEnlarge)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellReduce)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 89)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEnlarge)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, enlargeModelScaleInc)
            .AddHandler(DispatcherType.ConditionRemove, EnlargeEnded)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 89)
            .AddHandler(DispatcherType.AbilityScoreLevel, EnlargeStatLevelGet, Stat.strength, 244)
            .AddHandler(DispatcherType.AbilityScoreLevel, EnlargeStatLevelGet, Stat.dexterity, 244)
            .AddHandler(DispatcherType.GetAttackDice, AttackDiceEnlargePerson, 0, 244)
            .AddHandler(DispatcherType.GetSizeCategory, EnlargeSizeCategory, 0, 244)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 89, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 89)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 33, 0)
            .Build();


        [TempleDllLocation(0x102d77a0)]
        public static readonly ConditionSpec SpellEntangle =
            CreateSpellEffect("sp-Entangle", WellKnownSpells.Entangle, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 90)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellEntangle)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 90)
                .AddHandler(DispatcherType.ObjectEvent, ObjEventAoEEntangle, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 90)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d78b0)]
        public static readonly ConditionSpec SpellEntangleOn =
            CreateSpellEffect("sp-Entangle On", WellKnownSpells.Entangle, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellEntangleOn)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, sub_100C79E0)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEntangleOn)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CCE10)
                .AddHandler(DispatcherType.TurnBasedStatusInit, OnBeginRoundDisableMovement)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, true)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C6200, 2, 228)
                .AddHandler(DispatcherType.AbilityScoreLevel, EntangleAttributeMalus, Stat.dexterity, 228)
                .AddHandler(DispatcherType.GetMoveSpeedBase, entangleMoveRestrict, 0, 228)
                .AddHandler(DispatcherType.ObjectEvent, ObjEventAoEEntangle, 91)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_BreakFree, EntangleBreakFree, 91)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 91, SpellEntangle)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 91)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 99, 0)
                .AddHandler(DispatcherType.RadialMenuEntry, CommonConditionCallbacks.BreakFreeRadial, 91)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 105, 0)
                .Build();


        [TempleDllLocation(0x102d7a70)]
        public static readonly ConditionSpec SpellEntangleOff =
            CreateSpellEffect("sp-Entangle Off", WellKnownSpells.Entangle, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellEntangleOff)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellEntangleOff)
                .AddHandler(DispatcherType.BeginRound, sub_100D4440, 92)
                .AddHandler(DispatcherType.GetMoveSpeed, sub_100C7040, 0, 228)
                .AddHandler(DispatcherType.ObjectEvent, ObjEventAoEEntangle, 92)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 92, SpellEntangle)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 92)
                .Build();


        [TempleDllLocation(0x102d7b68)]
        public static readonly ConditionSpec SpellEntropicShield =
            CreateSpellEffect("sp-Entropic Shield", WellKnownSpells.EntropicShield, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 93)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 93)
                .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, sub_100C7A00, 20, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 93, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 93)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 34, 0)
                .Build();


        [TempleDllLocation(0x102d7c78)]
        public static readonly ConditionSpec SpellExpeditiousRetreat =
            CreateSpellEffect("sp-Expeditious Retreat", WellKnownSpells.ExpeditiousRetreat, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 94)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 94)
                .AddHandler(DispatcherType.GetMoveSpeed, sub_100C8B00, 30, 283)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 94, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 94)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 35, 0)
                .Build();


        [TempleDllLocation(0x102d7d88)]
        public static readonly ConditionSpec SpellFaerieFire =
            CreateSpellEffect("sp-Faerie Fire", WellKnownSpells.FaerieFire, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 95)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFaerieFire)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 95)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 95, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 95)
                .Build();


        [TempleDllLocation(0x102d7e98)]
        public static readonly ConditionSpec SpellFalseLife =
            CreateSpellEffect("sp-False Life", WellKnownSpells.FalseLife, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 96)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFalseLife)
                .AddHandler(DispatcherType.ConditionAdd, Condition_sp_False_Life_Init)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 96)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, Spell_remove_spell, 96, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, Spell_remove_mod, 96)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 96, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 96)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 73, 0)
                .Build();


        [TempleDllLocation(0x102d7fb8)]
        public static readonly ConditionSpec SpellFeeblemind =
            CreateSpellEffect("sp-Feeblemind", WellKnownSpells.Feeblemind, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Mute, true)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .SetQueryResult(D20DispatcherKey.QUE_CannotUseIntSkill, true)
                .SetQueryResult(D20DispatcherKey.QUE_CannotUseChaSkill, true)
                .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_INTELLIGENCE, sub_100C7A60, 1)
                .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, sub_100C7A60, 1)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowPenalty_sp_Feeblemind_Callback)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 98, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 98)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 118, 0)
                .Build();


        [TempleDllLocation(0x102d8100)]
        public static readonly ConditionSpec SpellFear = CreateSpellEffect("sp-Fear", WellKnownSpells.Fear, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 97)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, IsCritterAfraidQuery)
            .AddHandler(DispatcherType.ConditionAdd, FloatMessageAfraid)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 97)
            .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 2, 172)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowModifierCallback, 2, 172)
            .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 2, 172)
            .AddHandler(DispatcherType.AbilityCheckModifier, AbilityCheckModifierEmotion, 2, 172)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 97, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 97)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 52, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 116, 0)
            .Build();


        [TempleDllLocation(0x102d8260)]
        public static readonly ConditionSpec SpellFindTraps =
            CreateSpellEffect("sp-Find Traps", WellKnownSpells.FindTraps, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 99)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Find_Traps, true)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 99)
                .AddHandler(DispatcherType.SkillLevel, SkillModifier_FindTraps_Callback, 0, 284)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 99, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 99)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 36, 0)
                .Build();


        [TempleDllLocation(0x102d8358)]
        public static readonly ConditionSpec SpellFireShield =
            CreateSpellEffect("sp-Fire Shield", WellKnownSpells.FireShield, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 100)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 100)
                .AddHandler(DispatcherType.ReflexThrow, FireShield_callback_31h)
                .AddHandler(DispatcherType.TakingDamage, FireShieldDamageResistance)
                .AddHandler(DispatcherType.TakingDamage2, FireShieldCounterDamage, 100)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 100, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 100)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 95, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 37, 0)
                .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                .Build();


        [TempleDllLocation(0x102d8798)]
        public static readonly ConditionSpec SpellFlare = CreateSpellEffect("sp-Flare", WellKnownSpells.Flare, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 101)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CCE50)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 101)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 1, 171)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 101, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 101)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 72, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 159, 0)
            .Build();


        [TempleDllLocation(0x102d84b8)]
        public static readonly ConditionSpec SpellFogCloud =
            CreateSpellEffect("sp-Fog Cloud", WellKnownSpells.FogCloud, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 102)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellFogCloud)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 102)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__fog_cloud_sthg, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 102)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d85a0)]
        public static readonly ConditionSpec SpellFogCloudHit =
            CreateSpellEffect("sp-Fog Cloud Hit", WellKnownSpells.FogCloud, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellFogCloudHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellFogCloudHit)
                .AddHandler(DispatcherType.ConditionAdd, ShowConcealedMessage)
                .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, FogCloudConcealmentMissChance)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__fog_cloud_sthg, 103)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 103, SpellFogCloud)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 103)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 88, 0)
                .Build();


        [TempleDllLocation(0x102d86b0)]
        public static readonly ConditionSpec SpellFoxsCunning =
            CreateSpellEffect("sp-Foxs Cunning", WellKnownSpells.FoxsCunning, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 104)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 104)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.intelligence, 293)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 104, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 104)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                .Build();


        [TempleDllLocation(0x102d88a8)]
        public static readonly ConditionSpec SpellFreedomofMovement =
            CreateSpellEffect("sp-Freedom of Movement", WellKnownSpells.FreedomOfMovement, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 105)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 105)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 105, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 105)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 38, 0)
                .Build();


        [TempleDllLocation(0x102d8990)]
        public static readonly ConditionSpec SpellGaseousForm =
            CreateSpellEffect("sp-Gaseous Form", WellKnownSpells.GaseousForm, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 106)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
                .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, GaseousFormSpellInterruptedQuery)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellGaseousForm)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CD8F0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 106)
                .AddHandler(DispatcherType.TakingDamage, GaseousFormTakingDamage, 10, D20AttackPower.MAGIC)
                .AddHandler(DispatcherType.GetAC, GaseousFormAcBonusCapper)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100C7E70, 10, 229)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 106, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 106)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 39, 0)
                .Build();


        [TempleDllLocation(0x102d8b40)]
        public static readonly ConditionSpec SpellGhoulTouch =
            CreateSpellEffect("sp-Ghoul Touch", WellKnownSpells.GhoulTouch, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 107)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, GhoulTouchAttackHandler, 107)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 107, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 107)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 107)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 107)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 107)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .Build();


        [TempleDllLocation(0x102d8d38)]
        public static readonly ConditionSpec SpellGhoulTouchParalyzed =
            CreateSpellEffect("sp-Ghoul Touch Paralyzed", WellKnownSpells.GhoulTouch, 3)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 108)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 108)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellHold)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 108)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 108, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 108)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 120, 0)
                .Build();


        [TempleDllLocation(0x102d8e58)]
        public static readonly ConditionSpec SpellGhoulTouchStench =
            CreateSpellEffect("sp-Ghoul Touch Stench", WellKnownSpells.GhoulTouch, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 109)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellGhoulTouchStench)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 109)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36_ghoul_touch_stench_sthg, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 109)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d8f28)]
        public static readonly ConditionSpec SpellGhoulTouchStenchHit =
            CreateSpellEffect("sp-Ghoul Touch Stench Hit", WellKnownSpells.GhoulTouch, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellGhoulTouchStenchHit)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellNeutralizePoison)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellGhoulTouchStenchHit)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D03B0)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36_ghoul_touch_stench_sthg, 110)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 2, 256)
                .AddHandler(DispatcherType.DealingDamage, sub_100C5990, 2, 256)
                .AddHandler(DispatcherType.SkillLevel, sub_100C5A30, 2, 256)
                .AddHandler(DispatcherType.AbilityCheckModifier, sub_100C5A30, 2, 256)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 110, SpellGhoulTouchStench)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 110)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 97, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 141, 0)
                .Build();


        [TempleDllLocation(0x102d90b0)]
        public static readonly ConditionSpec SpellGlibness =
            CreateSpellEffect("sp-Glibness", WellKnownSpells.Glibness, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 111)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGlibness)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 111)
                .AddSkillLevelHandler(SkillId.bluff, GlibnessSkillLevel, 30, 296)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 111, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 111)
                .Build();


        [TempleDllLocation(0x102d91c0)]
        public static readonly ConditionSpec SpellGlitterdustBlindness =
            CreateSpellEffect("sp-Glitterdust Blindness", WellKnownSpells.Glitterdust, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveBlindness)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 14)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC2E0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 14)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
                .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
                .AddHandler(DispatcherType.GetAttackerConcealmentMissChance,
                    CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition,
                    CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 14, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 14)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 122, 0)
                .Build();


        [TempleDllLocation(0x102d9370)]
        public static readonly ConditionSpec SpellGlitterdust =
            CreateSpellEffect("sp-Glitterdust", WellKnownSpells.Glitterdust, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 113)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellGlitterdust)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 113)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 113, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 113)
                .Build();


        [TempleDllLocation(0x102d9440)]
        public static readonly ConditionSpec SpellGoodberry =
            CreateSpellEffect("sp-Goodberry", WellKnownSpells.Goodberry, 3)
                .AddHandler(DispatcherType.ConditionAdd, GoodberryAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102d9498)]
        public static readonly ConditionSpec SpellGoodberryTally =
            CreateSpellEffect("sp-Goodberry Tally", WellKnownSpells.Goodberry, 3)
                .AddHandler(DispatcherType.ConditionAddPre, GoodberryTallyPreAdd, SpellGoodberry)
                .SetUnique()
                .AddHandler(DispatcherType.ConditionAdd, sub_100CDC00)
                .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100CC800)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 0)
                .Build();


        [TempleDllLocation(0x102d9540)]
        public static readonly ConditionSpec SpellGrease = CreateSpellEffect("sp-Grease", WellKnownSpells.Grease, 4)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 116)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, BeginSpellGrease)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 116)
            .AddHandler(DispatcherType.ObjectEvent, GreaseAoeEvent, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 116)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .Build();


        [TempleDllLocation(0x102d9638)]
        public static readonly ConditionSpec SpellGreaseHit =
            CreateSpellEffect("sp-Grease Hit", WellKnownSpells.Grease, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellGreaseHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGreaseHit)
                .AddHandler(DispatcherType.ConditionAdd, GreaseSlippage)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 234)
                .AddHandler(DispatcherType.ObjectEvent, GreaseAoeEvent, 117)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_Critter_Moved, GreaseSlippage)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 117, SpellGrease)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 117)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 158, 0)
                .Build();


        [TempleDllLocation(0x102d9758)]
        public static readonly ConditionSpec SpellGreaterHeroism =
            CreateSpellEffect("sp-Greater Heroism", WellKnownSpells.GreaterHeroism, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 118)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CDD50)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 118)
                .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
                .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 4, 299)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowEmotionModifierCallback, 4, 299)
                .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 4, 299)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 118, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 118)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 61, 0)
                .Build();


        [TempleDllLocation(0x102d9890)]
        public static readonly ConditionSpec SpellGreaterMagicFang =
            CreateSpellEffect("sp-Greater Magic Fang", WellKnownSpells.GreaterMagicFang, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 119)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 119)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C4850, 0, 210)
                .AddHandler(DispatcherType.DealingDamage, sub_100C4970, 0, 210)
                .AddHandler(DispatcherType.DealingDamage, ApplyEnchantedAttackPower, 0, 210)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 119, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 119)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 40, 0)
                .Build();


        [TempleDllLocation(0x102d99a0)]
        public static readonly ConditionSpec SpellGreaterMagicWeapon =
            CreateSpellEffect("sp-Greater Magic Weapon", WellKnownSpells.GreaterMagicWeapon, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 120)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Obj_Is_Blessed, true)
                .AddHandler(DispatcherType.ConditionAdd, WeaponEnhBonusOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 120)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C4850, 0, 212)
                .AddHandler(DispatcherType.DealingDamage, sub_100C4970, 0, 212)
                .AddHandler(DispatcherType.DealingDamage, ApplyEnchantedAttackPower, 0, 212)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 120, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 120)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 41, 0)
                .Build();


        [TempleDllLocation(0x102d9ad8)]
        public static readonly ConditionSpec SpellGuidance =
            CreateSpellEffect("sp-Guidance", WellKnownSpells.Guidance, 5)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellGuidance)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 121)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CDE50)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 121)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100DD240, 1, 219)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_sp_Guidance_Callback, 1, 219)
                .AddHandler(DispatcherType.SkillLevel, GuidanceSkillLevel, 1, 219)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 121, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 121)
                .AddHandler(DispatcherType.RadialMenuEntry, Guidance_RadialMenuEntry_Callback, 121)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 42, 0)
                .Build();


        [TempleDllLocation(0x102d9c20)]
        public static readonly ConditionSpec SpellGustofWind =
            CreateSpellEffect("sp-Gust of Wind", WellKnownSpells.GustOfWind, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 122)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 122)
                .AddHandler(DispatcherType.TurnBasedStatusInit, GustOfWindTurnBasedStatusInit)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.SkillLevel, sub_100C8530, 4, 277)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C8570, 4, 277)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 122, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 122)
                .Build();


        [TempleDllLocation(0x102d9ea0)]
        public static readonly ConditionSpec SpellHaste = CreateSpellEffect("sp-Haste", WellKnownSpells.Haste, 3)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellSlow)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 123)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CDE90)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 123)
            .AddHandler(DispatcherType.SaveThrowLevel, sub_100C8780, 1, 174)
            .AddHandler(DispatcherType.GetBonusAttacks, HasteBonusAttack)
            .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 8, 174)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 1, 174)
            .AddHandler(DispatcherType.GetMoveSpeedBase, HasteMoveSpeed, 0, 174)
            .AddHandler(DispatcherType.GetMoveSpeed, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 123, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 123)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 43, 0)
            .Build();


        [TempleDllLocation(0x102da010)]
        public static readonly ConditionSpec SpellHaltUndead =
            CreateSpellEffect("sp-Halt Undead", WellKnownSpells.HaltUndead, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 124)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellHold)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 124)
                .AddHandler(DispatcherType.TakingDamage2, Spell_remove_spell, 124, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 124, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 124, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 124)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 123, 0)
                .Build();


        [TempleDllLocation(0x102da158)]
        public static readonly ConditionSpec SpellHarm = CreateSpellEffect("sp-Harm", WellKnownSpells.Harm2, 3)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, HarmOnAdd)
            .Build();


        [TempleDllLocation(0x102d09c8)]
        public static readonly ConditionSpec SpellHeal = CreateSpellEffect("sp-Heal", WellKnownSpells.Heal, 3)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpHealOnConditionAdd)
            .Build();


        [TempleDllLocation(0x102d2918)]
        public static readonly ConditionSpec SpellHeatMetal =
            CreateSpellEffect("sp-Heat Metal", WellKnownSpells.HeatMetal, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellChillMetal)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 127)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 127)
                .AddHandler(DispatcherType.TurnBasedStatusInit, HeatMetalTurnBasedStatusInit, 127)
                .AddHandler(DispatcherType.TakingDamage, HeatMetalDamageResistance)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 127, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 127)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 164, 0)
                .Build();


        [TempleDllLocation(0x102da1b0)]
        public static readonly ConditionSpec SpellHeroism = CreateSpellEffect("sp-Heroism", WellKnownSpells.Heroism, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 128)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 128)
            .AddHandler(DispatcherType.ToHitBonus2, EmotionToHitBonus2, 2, 298)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowEmotionModifierCallback, 2, 298)
            .AddHandler(DispatcherType.SkillLevel, EmotionSkillBonus, 2, 298)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 128, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 128)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 61, 0)
            .Build();


        [TempleDllLocation(0x102da2c0)]
        public static readonly ConditionSpec SpellHoldAnimal =
            CreateSpellEffect("sp-Hold Animal", WellKnownSpells.HoldAnimal, 3)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 0)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 129)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellHold)
                .AddHandler(DispatcherType.BeginRound, sub_100C3FE0, 129)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 129)
                .AddHandler(DispatcherType.TurnBasedStatusInit, HoldXTurnBasedStatusInit)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 129, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 129)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 124, 0)
                .Build();


        [TempleDllLocation(0x102da448)]
        public static readonly ConditionSpec SpellHoldMonster =
            CreateSpellEffect("sp-Hold Monster", WellKnownSpells.HoldMonster, 3)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 0)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 130)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.NONE)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellHold)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, sub_100C3FE0, 130)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 130)
                .AddHandler(DispatcherType.TurnBasedStatusInit, HoldXTurnBasedStatusInit)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 130, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 130)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 125, 0)
                .Build();


        [TempleDllLocation(0x102da5d0)]
        public static readonly ConditionSpec SpellHoldPerson =
            CreateSpellEffect("sp-Hold Person", WellKnownSpells.HoldPerson, 3)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 0)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 131)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellHold)
                .AddHandler(DispatcherType.BeginRound, sub_100C3FE0, 131)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 131)
                .AddHandler(DispatcherType.TurnBasedStatusInit, HoldXTurnBasedStatusInit)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 131, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 131)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 126, 0)
                .Build();


        [TempleDllLocation(0x102da758)]
        public static readonly ConditionSpec SpellHoldPortal =
            CreateSpellEffect("sp-Hold Portal", WellKnownSpells.HoldPortal, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 132)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 132)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 132, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 132)
                .Build();


        [TempleDllLocation(0x102da840)]
        public static readonly ConditionSpec SpellHolySmite =
            CreateSpellEffect("sp-Holy Smite", WellKnownSpells.HolySmite, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveBlindness)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CC2E0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 14)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
                .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
                .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
                .AddHandler(DispatcherType.GetAttackerConcealmentMissChance,
                    CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition,
                    CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 14, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 14)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 127, 0)
                .Build();


        [TempleDllLocation(0x102da9d8)]
        public static readonly ConditionSpec SpellIceStorm =
            CreateSpellEffect("sp-Ice Storm", WellKnownSpells.IceStorm, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 134)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellIceStorm)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 134)
                .AddHandler(DispatcherType.ObjectEvent, IceStormHitTrigger, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 134)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102daac0)]
        public static readonly ConditionSpec SpellIceStormHit =
            CreateSpellEffect("sp-Ice Storm Hit", WellKnownSpells.IceStorm, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellIceStormHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellIceStormHit)
                .AddHandler(DispatcherType.ConditionAdd, IceStormDamage)
                .AddHandler(DispatcherType.ObjectEvent, IceStormHitTrigger, 135)
                .AddHandler(DispatcherType.SkillLevel, sub_100C8530, 4, 287)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 287)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 135, SpellIceStorm)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 135)
                .Build();


        [TempleDllLocation(0x102dabd0)]
        public static readonly ConditionSpec SpellInvisibility =
            CreateSpellEffect("sp-Invisibility", WellKnownSpells.Invisibility, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 136)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibility)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, SpellInvisibilityBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 136)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Sequence, Spell_remove_spell, 136, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 136, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 136)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 48, 0)
                .Build();


        [TempleDllLocation(0x102dad08)]
        public static readonly ConditionSpec SpellInvisibilityPurge =
            CreateSpellEffect("sp-Invisibility Purge", WellKnownSpells.InvisibilityPurge, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 137)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellInvisibilityPurge)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 137)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__invisibility_purge, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 137)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dae00)]
        public static readonly ConditionSpec SpellInvisibilityPurgeHit =
            CreateSpellEffect("sp-Invisibility Purge Hit", WellKnownSpells.InvisibilityPurge, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellInvisibilityPurgeHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibilityPurgeHit)
                .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.ObjectEvent, Condition__36__invisibility_purge, 138)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 138, SpellInvisibilityPurge)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 138)
                .Build();


        [TempleDllLocation(0x102daee8)]
        public static readonly ConditionSpec SpellInvisibilitySphere =
            CreateSpellEffect("sp-Invisibility Sphere", WellKnownSpells.InvisibilitySphere, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 139)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, InvisibilitySphereBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 139)
                .AddHandler(DispatcherType.ObjectEvent, InvisibilitySphereAoeEvent, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 139)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dafe0)]
        public static readonly ConditionSpec SpellInvisibilitySphereHit =
            CreateSpellEffect("sp-Invisibility Sphere Hit", WellKnownSpells.InvisibilitySphere, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellInvisibilitySphereHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibilitySphereHit)
                .AddHandler(DispatcherType.ConditionAdd, InvisibilitySphereHitBegin)
                .AddHandler(DispatcherType.ObjectEvent, InvisibilitySphereAoeEvent, 140)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 140, SpellInvisibility)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 140)
                .Build();


        [TempleDllLocation(0x102db0c8)]
        public static readonly ConditionSpec SpellInvisibilitytoAnimals =
            CreateSpellEffect("sp-Invisibility to Animals", WellKnownSpells.InvisibilityToAnimals, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 141)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibility)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, SpellInvisibilityBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 141)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Sequence, Spell_remove_spell, 141, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 141, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 141)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 48, 0)
                .Build();


        [TempleDllLocation(0x102db200)]
        public static readonly ConditionSpec SpellInvisibilitytoUndead =
            CreateSpellEffect("sp-Invisibility to Undead", WellKnownSpells.InvisibilityToUndead, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 142)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibility)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, SpellInvisibilityBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 142)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Sequence, Spell_remove_spell, 142, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 142, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 142)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 48, 0)
                .Build();


        [TempleDllLocation(0x102db338)]
        public static readonly ConditionSpec SpellImprovedInvisibility =
            CreateSpellEffect("sp-Improved Invisibility", WellKnownSpells.ImprovedInvisibility, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 143)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibility)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, SpellInvisibilityBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 143)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 143, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 143)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 44, 0)
                .Build();


        [TempleDllLocation(0x102db458)]
        public static readonly ConditionSpec SpellKeenEdge =
            CreateSpellEffect("sp-Keen Edge", WellKnownSpells.KeenEdge, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 144)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpWeaponKeenOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 144)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 144, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 144)
                .Build();


        [TempleDllLocation(0x102db528)]
        public static readonly ConditionSpec SpellLesserRestoration =
            CreateSpellEffect("sp-Lesser Restoration", WellKnownSpells.LesserRestoration, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpLesserRestorationOnConditionAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102db5a8)]
        public static readonly ConditionSpec SpellLongstrider =
            CreateSpellEffect("sp-Longstrider", WellKnownSpells.Longstrider, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 146)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 146)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100C8B00, 10, 297)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 146, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 146)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 35, 0)
                .Build();


        [TempleDllLocation(0x102db6b8)]
        public static readonly ConditionSpec SpellMageArmor =
            CreateSpellEffect("sp-Mage Armor", WellKnownSpells.MageArmor, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 147)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 147)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 28, 149)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 147, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 147)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 49, 0)
                .Build();


        [TempleDllLocation(0x102db7c8)]
        public static readonly ConditionSpec SpellMagicCircleInward = ConditionSpec.Create("sp-Magic Circle Inward", 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 148)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CE6D0)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 148)
            .AddHandler(DispatcherType.TurnBasedStatusInit, MagicCircleTurnBasedStatusInit)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, MagicCircleInwardAooPossible)
            .AddHandler(DispatcherType.AcModifyByAttacker, SpellArmorBonus, 11, 205)
            .AddHandler(DispatcherType.SaveThrowSpellResistanceBonus, SavingThrowSpellResistanceBonusCallback, 2, 205)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 148, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 148)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 91, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 65, 0)
            .Build();


        [TempleDllLocation(0x102db928)]
        public static readonly ConditionSpec SpellMagicCircleOutward = ConditionSpec
            .Create("sp-Magic Circle Outward", 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 149)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 149)
            .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 11, 206)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 2, 206)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
            .AddHandler(DispatcherType.TakingDamage2, MagicCircleTakingDamage, 149)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 149, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 149)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 65, 0)
            .Build();


        [TempleDllLocation(0x102dba48)]
        public static readonly ConditionSpec SpellMagicFang =
            CreateSpellEffect("sp-Magic Fang", WellKnownSpells.MagicFang, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 150)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 150)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C4850, 1, 209)
                .AddHandler(DispatcherType.DealingDamage, sub_100C4970, 1, 209)
                .AddHandler(DispatcherType.DealingDamage, ApplyEnchantedAttackPower, 1, 209)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 150, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 150)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 50, 0)
                .Build();


        [TempleDllLocation(0x102dbb58)]
        public static readonly ConditionSpec SpellMagicMissile =
            CreateSpellEffect("sp-Magic Missile", WellKnownSpells.MagicMissile, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, MagicMissileOnAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dbbd8)]
        public static readonly ConditionSpec SpellMagicStone =
            CreateSpellEffect("sp-Magic Stone", WellKnownSpells.MagicStone, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 152)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 152)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C4850, 1, 237)
                .AddHandler(DispatcherType.DealingDamage, sub_100C4970, 1, 237)
                .AddHandler(DispatcherType.DealingDamage, ApplyEnchantedAttackPower, 1, 237)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 152, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 152)
                .Build();


        [TempleDllLocation(0x102dbcd0)]
        public static readonly ConditionSpec SpellMagicVestment =
            CreateSpellEffect("sp-Magic Vestment", WellKnownSpells.MagicVestment, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 153)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, MagicVestmentOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 153)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 12, 213)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 153, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 153)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 51, 0)
                .Build();


        [TempleDllLocation(0x102dbdc8)]
        public static readonly ConditionSpec SpellMagicWeapon =
            CreateSpellEffect("sp-Magic Weapon", WellKnownSpells.MagicWeapon, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 154)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Obj_Is_Blessed, true)
                .AddHandler(DispatcherType.ConditionAdd, MagicWeaponOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 154)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 154, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 154)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 52, 0)
                .Build();


        [TempleDllLocation(0x102dbec0)]
        public static readonly ConditionSpec SpellMeldIntoStone =
            CreateSpellEffect("sp-Meld Into Stone", WellKnownSpells.MeldIntoStone, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 155)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellMeldIntoStone)
                .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .AddHandler(DispatcherType.ConditionAdd, MeldIntoStoneBeginSpell)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 155)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.TakingDamage, sub_100C8F40)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 155, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 155)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 155, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 155)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 83, 0)
                .Build();


        [TempleDllLocation(0x102dc058)]
        public static readonly ConditionSpec SpellMelfsAcidArrow =
            CreateSpellEffect("sp-Melfs Acid Arrow", WellKnownSpells.MelfsAcidArrow, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 156)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, AcidDamage)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 156)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 156, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 156)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 128, 0)
                .Build();


        [TempleDllLocation(0x102dc140)]
        public static readonly ConditionSpec SpellMinorGlobeofInvulnerability =
            CreateSpellEffect("sp-Minor Globe of Invulnerability", WellKnownSpells.LesserGlobeOfInvulnerability, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 157)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellMinorGlobeOfInvulnerability)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 157)
                .AddHandler(DispatcherType.ObjectEvent, d20_mods_spells__globe_of_inv_hit, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 157)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 82, 0)
                .Build();


        [TempleDllLocation(0x102dc260)]
        public static readonly ConditionSpec SpellMinorGlobeofInvulnerabilityHit =
            CreateSpellEffect("sp-Minor Globe of Invulnerability Hit", WellKnownSpells.LesserGlobeOfInvulnerability, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellMinorGlobeofInvulnerabilityHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.Unused63, MinorGlobeCallback3F, SpellMinorGlobeofInvulnerabilityHit)
                .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellMinorGlobeofInvulnerabilityHit)
                .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.ObjectEvent, d20_mods_spells__globe_of_inv_hit, 158)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 158,
                    SpellMinorGlobeofInvulnerability)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 158)
                .Build();


        [TempleDllLocation(0x102dc370)]
        public static readonly ConditionSpec SpellMindFog = CreateSpellEffect("sp-Mind Fog", WellKnownSpells.MindFog, 4)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 159)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, BeginSpellMindFog)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 159)
            .AddHandler(DispatcherType.ObjectEvent, MindFogAoeEvent, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 159)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .Build();


        [TempleDllLocation(0x102dc440)]
        public static readonly ConditionSpec SpellMindFogHit =
            CreateSpellEffect("sp-Mind Fog Hit", WellKnownSpells.MindFog, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellMindFogHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellMindFogHit)
                .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.ObjectEvent, MindFogAoeEvent, 160)
                .AddHandler(DispatcherType.AbilityCheckModifier, sub_100C9060, 10, 265)
                .AddHandler(DispatcherType.SaveThrowLevel, sub_100C9020, 10, 265)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 160, SpellMindFog)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 160)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 104, 0)
                .Build();


        [TempleDllLocation(0x102dc560)]
        public static readonly ConditionSpec SpellMirrorImage =
            CreateSpellEffect("sp-Mirror Image", WellKnownSpells.MirrorImage, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellMirrorImage)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 161)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Mirror_Image, sub_100C9160)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellMirrorImage)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, MirrorImageAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 161)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Mirror_Image_Struck, MirrorImageStruck, 161)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 161, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 161)
                .AddHandler(DispatcherType.Tooltip, MirrorImageTooltipCallback, 109)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 53, 0)
                .Build();


        [TempleDllLocation(0x102dc6d0)]
        public static readonly ConditionSpec SpellMordenkainensFaithfulHound =
            CreateSpellEffect("sp-Mordenkainens Faithful Hound", WellKnownSpells.MordenkainensFaithfulHound, 4)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Invisible, true)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellMordenkainensFaithfulHound)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 162)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100C7E70, 0, 101)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 162, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 162)
                .Build();


        [TempleDllLocation(0x102d42d8)]
        public static readonly ConditionSpec SpellNegativeEnergyProtection =
            CreateSpellEffect("sp-Negative Energy Protection", WellKnownSpells.NegativeEnergyProtection, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 163)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellNegativeEnergyProtection)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 163)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 163, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 163)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 54, 0)
                .Build();


        [TempleDllLocation(0x102d0a20)]
        public static readonly ConditionSpec SpellNeutralizePoison =
            CreateSpellEffect("sp-Neutralize Poison", WellKnownSpells.NeutralizePoison, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 164)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 164)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 164, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 164)
                .Build();


        [TempleDllLocation(0x102dc7b8)]
        public static readonly ConditionSpec SpellObscuringMist =
            CreateSpellEffect("sp-Obscuring Mist", WellKnownSpells.ObscuringMist, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 165)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellObscuringMist)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 165)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5780, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 165)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dc8a0)]
        public static readonly ConditionSpec SpellObscuringMistHit =
            CreateSpellEffect("sp-Obscuring Mist Hit", WellKnownSpells.ObscuringMist, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellObscuringMist)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellObscuringMist)
                .AddHandler(DispatcherType.ConditionAdd, ShowConcealedMessage)
                .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, ObscuringMist_Concealment_Callback)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5780, 166)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 166, SpellObscuringMist)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 166)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 88, 0)
                .Build();


        [TempleDllLocation(0x102dc9b0)]
        public static readonly ConditionSpec SpellOrdersWrath =
            CreateSpellEffect("sp-Orders Wrath", WellKnownSpells.OrdersWrath, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 167)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CCE50)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 167)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 167, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 167)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 72, 0)
                .Build();


        [TempleDllLocation(0x102dcae8)]
        public static readonly ConditionSpec SpellOtilukesResilientSphere =
            CreateSpellEffect("sp-Otilukes Resilient Sphere", WellKnownSpells.OtilukesResilientSphere, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 168)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellOtilukesResilientSphere)
                .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 168)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.TakingDamage, OtilukesSphereOnDamage)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 168, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 168)
                .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 81, 0)
                .Build();


        [TempleDllLocation(0x102dcc80)]
        public static readonly ConditionSpec SpellOwlsWisdom =
            CreateSpellEffect("sp-Owls Wisdom", WellKnownSpells.OwlsWisdom, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 169)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 169)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.wisdom, 294)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 169, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 169)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                .Build();


        [TempleDllLocation(0x102dcd68)]
        public static readonly ConditionSpec SpellPrayer = CreateSpellEffect("sp-Prayer", WellKnownSpells.Prayer, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 170)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 170)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100C9280, 1)
            .AddHandler(DispatcherType.DealingDamage, sub_100C92D0, 1)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowPenalty_sp_Prayer_Callback, 1)
            .AddHandler(DispatcherType.SkillLevel, SkillLevelPrayer, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 170, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 170)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 55, 0)
            .Build();


        [TempleDllLocation(0x102dce88)]
        public static readonly ConditionSpec SpellProduceFlame =
            CreateSpellEffect("sp-Produce Flame", WellKnownSpells.ProduceFlame, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 171)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 171)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, ProduceFlameTouchAttackHandler, 171)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 171, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 171)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 171)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 171)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 171)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .Build();


        [TempleDllLocation(0x102dd020)]
        public static readonly ConditionSpec SpellProtectionFromArrows =
            CreateSpellEffect("sp-Protection From Arrows", WellKnownSpells.ProtectionFromArrows, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 172)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 172)
                .AddHandler(DispatcherType.TakingDamage, ProtectionFromArrowsTakingDamage, 10)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 172, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 172)
                .AddHandler(DispatcherType.Tooltip, Tooltip2Callback, 75, 172)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 57, 0)
                .Build();


        [TempleDllLocation(0x102dd140)]
        public static readonly ConditionSpec SpellProtectionFromAlignment = ConditionSpec
            .Create("sp-Protection From Alignment", 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 173)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 173)
            .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 11, 207)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 2, 207)
            .AddHandler(DispatcherType.TakingDamage2, ProtectionFromAlignmentPreventDamage, 173)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 173, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 173)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 56, 0)
            .Build();


        [TempleDllLocation(0x102dd288)]
        public static readonly ConditionSpec SpellProtectionFromElements =
            CreateSpellEffect("sp-Protection From Elements", WellKnownSpells.ProtectionFromElements, 4)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100C77D0, SpellProtectionFromElements)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 174)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements, sub_100C7820)
                .AddHandler(DispatcherType.ConditionAdd, D20ModsSpells_ProtectionElementsDamageReductionRestore)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 174)
                .AddHandler(DispatcherType.TakingDamage, ProtFromElementsDamageResistance, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 174, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 174)
                .AddHandler(DispatcherType.Tooltip, Tooltip2Callback, 94, 174)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 58, 0)
                .AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, sub_100C7860)
                .Build();


        [TempleDllLocation(0x102dd3d0)]
        public static readonly ConditionSpec SpellRage = CreateSpellEffect("sp-Rage", WellKnownSpells.Rage, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 175)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellRage)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, RageBeginSpell)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, sub_100C9760, 2, 272)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, sub_100C9760, 2, 272)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, AddBonusType13, 1, 272)
            .AddHandler(DispatcherType.GetAC, sub_100C97C0, 2, 272)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 175, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 175)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 175, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 175)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 127, 0)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 0, 0)
            .Build();


        [TempleDllLocation(0x102dd580)]
        public static readonly ConditionSpec SpellRaiseDead =
            CreateSpellEffect("sp-Raise Dead", WellKnownSpells.RaiseDead, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RaiseDeadOnConditionAdd)
                .Build();


        [TempleDllLocation(0x102dd5d8)]
        public static readonly ConditionSpec SpellRayofEnfeeblement =
            CreateSpellEffect("sp-Ray of Enfeeblement", WellKnownSpells.RayOfEnfeeblement, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 177)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RayOfEnfeeblementOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 177)
                .AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.strength, 198)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 177, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 177)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 131, 0)
                .Build();


        [TempleDllLocation(0x102d7470)]
        public static readonly ConditionSpec SpellReduce = CreateSpellEffect("sp-Reduce", WellKnownSpells.Reduce, 3)
            .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellReduce)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEnlarge)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 179)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellReduce)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, SpellReduceSetModelScale)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 179)
            .AddHandler(DispatcherType.AbilityScoreLevel, ReduceAbilityModifier, Stat.dexterity, 245)
            .AddHandler(DispatcherType.AbilityScoreLevel, ReduceAbilityModifier, Stat.strength, 245)
            .AddHandler(DispatcherType.GetAttackDice, AttackDiceReducePerson, 0, 245)
            .AddHandler(DispatcherType.GetSizeCategory, sub_100C97F0, 0, 245)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 179, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 179)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 132, 0)
            .Build();


        [TempleDllLocation(0x102dd6d0)]
        public static readonly ConditionSpec SpellReduceAnimal =
            CreateSpellEffect("sp-Reduce Animal", WellKnownSpells.ReduceAnimal, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellReduceAnimal)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 178)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellReduceAnimal)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, SpellReduceSetModelScale)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 178)
                .AddHandler(DispatcherType.AbilityScoreLevel, ReduceAbilityModifier, Stat.strength, 295)
                .AddHandler(DispatcherType.AbilityScoreLevel, ReduceAbilityModifier, Stat.dexterity, 295)
                .AddHandler(DispatcherType.GetAttackDice, AttackDiceReducePerson, 0, 295)
                .AddHandler(DispatcherType.GetSizeCategory, sub_100C97F0, 0, 295)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 178, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 178)
                .Build();


        [TempleDllLocation(0x102d1458)]
        public static readonly ConditionSpec SpellRemoveBlindness =
            CreateSpellEffect("sp-Remove Blindness", WellKnownSpells.RemoveBlindnessDeafness, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 180)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 180)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 180, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 180)
                .Build();


        [TempleDllLocation(0x102d1250)]
        public static readonly ConditionSpec SpellRemoveCurse =
            CreateSpellEffect("sp-Remove Curse", WellKnownSpells.RemoveCurse, 3)
                .Prevents(SpellBestowCurseAbility)
                .Prevents(SpellBestowCurseRolls)
                .Prevents(SpellBestowCurseActions)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 181)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 181)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 181, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 181)
                .Build();


        [TempleDllLocation(0x102d44b8)]
        public static readonly ConditionSpec SpellRemoveDeafness =
            CreateSpellEffect("sp-Remove Deafness", WellKnownSpells.RemoveBlindnessDeafness, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 182)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 182)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 182, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 182)
                .Build();


        [TempleDllLocation(0x102d0af0)]
        public static readonly ConditionSpec SpellRemoveDisease =
            CreateSpellEffect("sp-Remove Disease", WellKnownSpells.RemoveDisease, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 183)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100DBD90)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 183)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 183, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 183)
                .Build();


        [TempleDllLocation(0x102dd840)]
        public static readonly ConditionSpec SpellRemoveFear =
            CreateSpellEffect("sp-Remove Fear", WellKnownSpells.RemoveFear, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 184)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 184)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 184, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 184)
                .Build();


        [TempleDllLocation(0x102d8ca0)]
        public static readonly ConditionSpec SpellRemoveParalysis =
            CreateSpellEffect("sp-Remove Paralysis", WellKnownSpells.RemoveParalysis, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 185)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dd910)]
        public static readonly ConditionSpec SpellRepelVermin =
            CreateSpellEffect("sp-Repel Vermin", WellKnownSpells.RepelVermin, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 186)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellRepelVermin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 186)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5950, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 186)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dd9f8)]
        public static readonly ConditionSpec SpellRepelVerminHit =
            CreateSpellEffect("sp-Repel Vermin Hit", WellKnownSpells.RepelVermin, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellRepelVerminHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellRepelVerminHit)
                .AddHandler(DispatcherType.ConditionAdd, RepelVerminOnAdd)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5950, 187)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 187, SpellRepelVermin)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 187)
                .Build();


        [TempleDllLocation(0x102ddae0)]
        public static readonly ConditionSpec SpellResistance =
            CreateSpellEffect("sp-Resistance", WellKnownSpells.Resistance, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 188)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 188)
                .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 1, 199)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 188, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 188)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 60, 0)
                .Build();


        [TempleDllLocation(0x102ddbc8)]
        public static readonly ConditionSpec SpellResistElements =
            CreateSpellEffect("sp-Resist Elements", WellKnownSpells.ResistElements, 4)
                .AddHandler(DispatcherType.ConditionAddPre, sub_100C77D0, SpellResistElements)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 189)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Resist_Elements, sub_100C7820)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CF460)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 189)
                .AddHandler(DispatcherType.TakingDamage, ResistElementsDamageResistance)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 189, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 189)
                .AddHandler(DispatcherType.Tooltip, Tooltip2Callback, 93, 189)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 59, 0)
                .AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, sub_100C7860)
                .Build();


        [TempleDllLocation(0x102ddd10)]
        public static readonly ConditionSpec SpellRestoration =
            CreateSpellEffect("sp-Restoration", WellKnownSpells.Restoration, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpRestorationOnConditionAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102ddd90)]
        public static readonly ConditionSpec SpellResurrection =
            CreateSpellEffect("sp-Resurrection", WellKnownSpells.Resurrection, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dde10)]
        public static readonly ConditionSpec SpellRighteousMight =
            CreateSpellEffect("sp-Righteous Might", WellKnownSpells.RighteousMight, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellRighteousMight)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 191)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 191)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, enlargeModelScaleInc)
                .AddHandler(DispatcherType.ToHitBonus2, RighteousMightToHitBonus, 0, 255)
                .AddHandler(DispatcherType.GetAC, sub_100C6050, 2, 255)
                .AddHandler(DispatcherType.AbilityScoreLevel, RighteousMightAbilityBonus, Stat.strength, 255)
                .AddHandler(DispatcherType.AbilityScoreLevel, RighteousMightAbilityBonus, Stat.constitution, 255)
                .AddHandler(DispatcherType.TakingDamage, sub_100CA3F0, 5)
                .AddHandler(DispatcherType.GetAttackDice, AttackDiceEnlargePerson, 0, 255)
                .AddHandler(DispatcherType.GetSizeCategory, EnlargeSizeCategory, 0, 255)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 191, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 191)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 61, 0)
                .Build();


        [TempleDllLocation(0x102ddfc0)]
        public static readonly ConditionSpec SpellSanctuary =
            CreateSpellEffect("sp-Sanctuary", WellKnownSpells.Sanctuary, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellSanctuary)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 192)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_CanBeAffected_PerformAction, SanctuaryCanBeAffectedPerform,
                    SpellSanctuary)
                .AddQueryHandler(D20DispatcherKey.QUE_CanBeAffected_ActionFrame, CanBeAffectedActionFrame_Sanctuary,
                    SpellSanctuary)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSanctuary)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Sanctuary_Attempt_Save, SanctuaryAttemptSave)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 192)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Sequence, Spell_remove_spell, 192, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 192, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 192)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 62, 0)
                .Build();


        [TempleDllLocation(0x102de108)]
        public static readonly ConditionSpec SpellSanctuarySaveSucceeded =
            CreateSpellEffect("sp-Sanctuary Save Succeeded", WellKnownSpells.Sanctuary, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSanctuarySaveSucceeded)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 193, SpellSanctuary)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 193)
                .Build();


        [TempleDllLocation(0x102de1b0)]
        public static readonly ConditionSpec SpellSanctuarySaveFailed =
            CreateSpellEffect("sp-Sanctuary Save Failed", WellKnownSpells.Sanctuary, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSanctuarySaveFailed)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 194, SpellSanctuary)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 194)
                .Build();


        [TempleDllLocation(0x102de258)]
        public static readonly ConditionSpec SpellSeeInvisibility =
            CreateSpellEffect("sp-See Invisibility", WellKnownSpells.SeeInvisibility, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 195)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Invisible, true)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 195)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 195, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 195)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 63, 0)
                .Build();


        [TempleDllLocation(0x102de368)]
        public static readonly ConditionSpec SpellShield = CreateSpellEffect("sp-Shield", WellKnownSpells.Shield, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 196)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellShield)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 196)
            .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 29, 253)
            .AddHandler(DispatcherType.TakingDamage, sub_100CA4A0)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 196, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 196)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 65, 0)
            .Build();


        [TempleDllLocation(0x102de4a0)]
        public static readonly ConditionSpec SpellShieldofFaith =
            CreateSpellEffect("sp-Shield of Faith", WellKnownSpells.ShieldOfFaith, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 197)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 197)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 11, 200)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 197, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 197)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 64, 0)
                .Build();


        [TempleDllLocation(0x102de588)]
        public static readonly ConditionSpec SpellShillelagh =
            CreateSpellEffect("sp-Shillelagh", WellKnownSpells.Shillelagh, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 198)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 198)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 198, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 198)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 66, 0)
                .Build();


        [TempleDllLocation(0x102de658)]
        public static readonly ConditionSpec SpellShockingGrasp =
            CreateSpellEffect("sp-Shocking Grasp", WellKnownSpells.ShockingGrasp, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 199)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, ShockingGraspTouchAttack, 199)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_spell, 0, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, Spell_remove_mod, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 199, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 199)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 199)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 199)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 199)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 2, 0)
                .Build();


        [TempleDllLocation(0x102de7b8)]
        public static readonly ConditionSpec SpellShout = CreateSpellEffect("sp-Shout", WellKnownSpells.Shout, 3)
            .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveDeafness)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Deafened, true)
            .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, DeafnessSpellFailure)
            .AddHandler(DispatcherType.ConditionAdd, DeafenedFloatMsg)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 51)
            .AddHandler(DispatcherType.InitiativeMod, DeafnessInitiativeMod, 4, 190)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 51, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 51)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 77, 0)
            .Build();


        [TempleDllLocation(0x102de8f0)]
        public static readonly ConditionSpec SpellSilence = CreateSpellEffect("sp-Silence", WellKnownSpells.Silence, 4)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 201)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, BeginSpellSilence)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 201)
            .AddHandler(DispatcherType.ObjectEvent, SilenceObjectEvent, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 201)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .Build();


        [TempleDllLocation(0x102de9e8)]
        public static readonly ConditionSpec SpellSilenceHit =
            CreateSpellEffect("sp-Silence Hit", WellKnownSpells.Silence, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSilenceHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, SilenceSpellFailure)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSilenceHit)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CA4D0)
                .AddHandler(DispatcherType.ObjectEvent, SilenceObjectEvent, 202)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 202, SpellSilence)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 202)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 116, 202)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 133, 0)
                .Build();


        [TempleDllLocation(0x102deb08)]
        public static readonly ConditionSpec SpellSleep = CreateSpellEffect("sp-Sleep", WellKnownSpells.Sleep, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 203)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .SetQueryResult(D20DispatcherKey.QUE_Unconscious, true)
            .AddHandler(DispatcherType.ConditionAdd, SleepOnAdd)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 203)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, SleepHpChanged)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 203, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 203)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 135, 0)
            .Build();


        [TempleDllLocation(0x102dec18)]
        public static readonly ConditionSpec SpellSleetStorm =
            CreateSpellEffect("sp-Sleet Storm", WellKnownSpells.SleetStorm, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 204)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellSleetStorm)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 204)
                .AddHandler(DispatcherType.ObjectEvent, SleetStormAoE, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 204)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102ded00)]
        public static readonly ConditionSpec SpellSleetStormHit =
            CreateSpellEffect("sp-Sleet Storm Hit", WellKnownSpells.SleetStorm, 4)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSleetStormHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSleetStormHit)
                .AddHandler(DispatcherType.ConditionAdd, SleetStormBeginSpell)
                .AddHandler(DispatcherType.TurnBasedStatusInit, SleetStormTurnBasedStatusInit)
                .AddHandler(DispatcherType.GetMoveSpeedBase, SleetStormHitMovementSpeed, 0, 257)
                .AddHandler(DispatcherType.ObjectEvent, SleetStormAoE, 205)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 205, SpellSleetStorm)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 205)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 136, 0)
                .Build();


        [TempleDllLocation(0x102d9d30)]
        public static readonly ConditionSpec SpellSlow = CreateSpellEffect("sp-Slow", WellKnownSpells.Slow, 3)
            .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellHaste)
            .SetUnique()
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 206)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100CFAF0)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 206)
            .AddHandler(DispatcherType.TurnBasedStatusInit, SlowTurnBasedStatusInit)
            .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 173)
            .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 0, 173)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 1, 173)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_sp_Slow_Callback, 1, 173)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 206, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 206)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 137, 0)
            .Build();


        [TempleDllLocation(0x102dee20)]
        public static readonly ConditionSpec SpellSoftenEarthandStone =
            CreateSpellEffect("sp-Soften Earth and Stone", WellKnownSpells.SoftenEarthAndStone, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 207)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellSoftenEarthAndStone)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 207)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5F60, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 207)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102def08)]
        public static readonly ConditionSpec SpellSoftenEarthandStoneHit =
            CreateSpellEffect("sp-Soften Earth and Stone Hit", WellKnownSpells.SoftenEarthAndStone, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSoftenEarthandStoneHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSoftenEarthandStoneHit)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 0)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5F60, 208)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CA7A0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 208, SpellSoftenEarthandStone)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 208)
                .Build();


        [TempleDllLocation(0x102df000)]
        public static readonly ConditionSpec SpellSoftenEarthandStoneHitSaveFailed =
            CreateSpellEffect("sp-Soften Earth and Stone Hit Save Failed", WellKnownSpells.SoftenEarthAndStone, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSoftenEarthandStoneHitSaveFailed)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSoftenEarthandStoneHitSaveFailed)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .AddHandler(DispatcherType.ObjectEvent, sub_100D5F60, 209)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 209)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 209, SpellSoftenEarthandStone)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 209)
                .Build();


        [TempleDllLocation(0x102df120)]
        public static readonly ConditionSpec SpellSolidFog =
            CreateSpellEffect("sp-Solid Fog", WellKnownSpells.SolidFog, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 210)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellSolidFog)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 210)
                .AddHandler(DispatcherType.ObjectEvent, SolidFogAoEEvent, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 210)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102df208)]
        public static readonly ConditionSpec SpellSolidFogHit =
            CreateSpellEffect("sp-Solid Fog Hit", WellKnownSpells.SolidFog, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSolidFogHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSolidFogHit)
                .AddHandler(DispatcherType.ConditionAdd, ShowConcealedMessage)
                .AddHandler(DispatcherType.TurnBasedStatusInit, OnBeginRoundDisableMovement)
                .AddHandler(DispatcherType.GetMoveSpeedBase, solidFogMoveRestriction, 0, 0)
                .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, sub_100CA920)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 2, 258)
                .AddHandler(DispatcherType.DealingDamage, sub_100C5990, 2, 258)
                .AddHandler(DispatcherType.TakingDamage, SolidFogDamageResistanceVsRanged, 0, 258)
                .AddHandler(DispatcherType.ObjectEvent, SolidFogAoEEvent, 211)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 211, SpellSolidFog)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 211)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 138, 0)
                .Build();


        [TempleDllLocation(0x102df378)]
        public static readonly ConditionSpec SpellSoundBurst =
            CreateSpellEffect("sp-Sound Burst", WellKnownSpells.SoundBurst, 3)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, StunnedFloatMessage)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 212)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, sub_100CB8E0, 2)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 224)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 212, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 212)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 89, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 172, 0)
                .Build();


        [TempleDllLocation(0x102df4b0)]
        public static readonly ConditionSpec SpellSpellResistance =
            CreateSpellEffect("sp-Spell Resistance", WellKnownSpells.SpellResistance, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 213)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CFD50)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 213)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, QueryHasSpellResistance)
                .AddHandler(DispatcherType.SpellResistanceMod, SpellResistanceMod_spSpellResistance_Callback, 5048)
// NOTE: DLL Hack removed immunity trigger for spell resistance
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 213, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 213)
                .AddHandler(DispatcherType.Tooltip, SpellResistanceTooltipCallback, 5048)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 60, 0)
                .Build();


        [TempleDllLocation(0x102df5e8)]
        public static readonly ConditionSpec SpellSpikeGrowth =
            CreateSpellEffect("sp-Spike Growth", WellKnownSpells.SpikeGrowth, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 214)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellSpikeGrowth)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 214)
                .AddHandler(DispatcherType.ObjectEvent, SpikeGrowthHitTrigger, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 214)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102df6f8)]
        public static readonly ConditionSpec SpellSpikeGrowthHit =
            CreateSpellEffect("sp-Spike Growth Hit", WellKnownSpells.SpikeGrowth, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeGrowthHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSpikeGrowthHit)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 0)
                .AddHandler(DispatcherType.ObjectEvent, SpikeGrowthHitTrigger, 215)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_Critter_Moved, SpikeGrowthHit)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 215, SpellSpikeGrowth)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 215)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 139, 0)
                .Build();


        [TempleDllLocation(0x102df808)]
        public static readonly ConditionSpec SpellSpikeGrowthDamage =
            CreateSpellEffect("sp-Spike Growth Damage", WellKnownSpells.SpikeGrowth, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeGrowthDamage)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSpikeGrowthDamage)
                .AddHandler(DispatcherType.GetMoveSpeed, sub_100CABE0, SpellSpikeGrowthDamage)
                .AddHandler(DispatcherType.ObjectEvent, SpikeGrowthHitTrigger, 216)
                .AddSignalHandler(D20DispatcherKey.SIG_HealSkill, SpikeGrowthDamageHealingReceived)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, SpikeGrowthDamageHealingReceived)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 216)
                .Build();


        [TempleDllLocation(0x102df900)]
        public static readonly ConditionSpec SpellSpikeStones =
            CreateSpellEffect("sp-Spike Stones", WellKnownSpells.SpikeStones, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 217)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellSpikeStones)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 217)
                .AddHandler(DispatcherType.ObjectEvent, SpikeStonesHitTrigger, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 217)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102df9f8)]
        public static readonly ConditionSpec SpellSpikeStonesHit =
            CreateSpellEffect("sp-Spike Stones Hit", WellKnownSpells.SpikeStones, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeStonesHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSpikeStonesHit)
                .AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 0)
                .AddHandler(DispatcherType.ObjectEvent, SpikeStonesHitTrigger, 218)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_Critter_Moved, SpikeStonesHitCombatCritterMovedHandler)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 218, SpellSpikeStones)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 218)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 140, 0)
                .Build();


        [TempleDllLocation(0x102dfb08)]
        public static readonly ConditionSpec SpellSpikeStonesDamage =
            CreateSpellEffect("sp-Spike Stones Damage", WellKnownSpells.SpikeStones, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeStonesDamage)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSpikeStonesDamage)
                .AddHandler(DispatcherType.GetMoveSpeed, sub_100CABE0, SpellSpikeStonesDamage)
                .AddHandler(DispatcherType.ObjectEvent, SpikeStonesHitTrigger, 219)
                .AddSignalHandler(D20DispatcherKey.SIG_HealSkill, SpikeGrowthDamageHealingReceived)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, SpikeGrowthDamageHealingReceived)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 219)
                .Build();


        [TempleDllLocation(0x102dfc00)]
        public static readonly ConditionSpec SpellSpiritualWeapon =
            CreateSpellEffect("sp-Spiritual Weapon", WellKnownSpells.SpiritualWeapon, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 220)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpiritualWeaponBeginSpellDismiss)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 220)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
                .AddHandler(DispatcherType.ToHitBonusBase, SpiritualWeaponBaseAttackBonus, 0, 101)
                .AddHandler(DispatcherType.DealingDamage, sub_100CAD50, 0, 101)
                .AddHandler(DispatcherType.GetCriticalHitRange, SpiritualWeapon_Callback23)
                .AddHandler(DispatcherType.GetCriticalHitExtraDice, sub_100CAEA0)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 220, SpellSpiritualWeapon)
                .Build();


        [TempleDllLocation(0x102dfd60)]
        public static readonly ConditionSpec SpellStinkingCloud =
            CreateSpellEffect("sp-Stinking Cloud", WellKnownSpells.StinkingCloud, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 221)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellStinkingCloud)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 221)
                .AddHandler(DispatcherType.ObjectEvent, AoeObjEventStinkingCloud, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 221)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102dfe30)]
        public static readonly ConditionSpec SpellStinkingCloudHit =
            CreateSpellEffect("sp-Stinking Cloud Hit", WellKnownSpells.StinkingCloud, 4)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellStinkingCloudHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellStinkingCloudHit)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .AddHandler(DispatcherType.ConditionAdd, StinkingCloudRemoveConcentration)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 222)
                .AddHandler(DispatcherType.TurnBasedStatusInit, StinkingCloudNausea_TurnbasedInit)
                .AddHandler(DispatcherType.ObjectEvent, AoeObjEventStinkingCloud, 222)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 222, SpellStinkingCloud)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 222)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 97, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 141, 0)
                .Build();


        [TempleDllLocation(0x102dff78)]
        public static readonly ConditionSpec SpellStinkingCloudHitPre =
            CreateSpellEffect("sp-Stinking Cloud Hit Pre", WellKnownSpells.StinkingCloud, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellStinkingCloudHitPre)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellStinkingCloudHitPre)
                .AddHandler(DispatcherType.BeginRound, StinkingCloudPreBeginRound, 223)
                .AddHandler(DispatcherType.ObjectEvent, AoeObjEventStinkingCloud, 223)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 223, SpellStinkingCloud)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 223)
                .Build();


        [TempleDllLocation(0x102e0060)]
        public static readonly ConditionSpec SpellStoneskin =
            CreateSpellEffect("sp-Stoneskin", WellKnownSpells.Stoneskin, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 224)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 224)
                .AddHandler(DispatcherType.TakingDamage, StoneskinTakingDamage, 10, D20AttackPower.ADAMANTIUM)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 224, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 224)
                .AddHandler(DispatcherType.Tooltip, Tooltip2Callback, 75, 224)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 67, 0)
                .Build();


        [TempleDllLocation(0x102e0158)]
        public static readonly ConditionSpec SpellSuggestion =
            CreateSpellEffect("sp-Suggestion", WellKnownSpells.Suggestion, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellSuggestion)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 225)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSuggestion)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_AIControlled, SuggestionIsAiControlledQuery)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, SuggestionIsCharmed)
                .AddHandler(DispatcherType.ConditionAdd, SuggestionOnAdd)
                .AddHandler(DispatcherType.BeginRound, sub_100CB1F0, 225)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 225)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Action_Recipient, Spell_remove_spell, 225, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 0, (ConditionSpec) null)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 225, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 225)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 149, 0)
                .Build();


        [TempleDllLocation(0x102e02e0)]
        public static readonly ConditionSpec SpellSummonSwarm =
            CreateSpellEffect("sp-Summon Swarm", WellKnownSpells.SummonSwarm, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 226)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellSummonSwarm)
                .AddHandler(DispatcherType.ConditionAdd, SummonSwarmBeginSpell)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 226)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 226, SpellSummonSwarm)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 226, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 226)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 226, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 226)
                .Build();


        [TempleDllLocation(0x102e0400)]
        public static readonly ConditionSpec SpellTashasHideousLaughter =
            CreateSpellEffect("sp-Tashas Hideous Laughter", WellKnownSpells.TashasHideousLaughter, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 227)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
                .AddHandler(DispatcherType.ConditionAdd, TashasHideousLaughterAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 227)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 227, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 227)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 142, 0)
                .Build();


        [TempleDllLocation(0x102e0520)]
        public static readonly ConditionSpec SpellTreeShape =
            CreateSpellEffect("sp-Tree Shape", WellKnownSpells.TreeShape, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 228)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
                .AddHandler(DispatcherType.ConditionAdd, TreeShapeBeginSpell)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 228)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 9, 222)
                .AddHandler(DispatcherType.AbilityScoreLevel, treeshapeStatRestriction, Stat.dexterity, 222)
                .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_spell, 228, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Concentration_Broken, Spell_remove_mod, 228)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 228, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 228)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 68, 0)
                .Build();


        [TempleDllLocation(0x102e0690)]
        public static readonly ConditionSpec SpellTrueSeeing =
            CreateSpellEffect("sp-True Seeing", WellKnownSpells.TrueSeeing, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 229)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Invisible, true)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_True_Seeing, true)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 229)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 229, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 229)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 69, 0)
                .Build();


        [TempleDllLocation(0x102e0788)]
        public static readonly ConditionSpec SpellTrueStrike =
            CreateSpellEffect("sp-True Strike", WellKnownSpells.TrueStrike, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 230)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellTrueStrike)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 230)
                .AddHandler(DispatcherType.ToHitBonus2, TrueStrikeAttackBonus, 20, 251)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 230, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 230)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 70, 0)
                .Build();


        [TempleDllLocation(0x102e0880)]
        public static readonly ConditionSpec SpellUnholyBlight =
            CreateSpellEffect("sp-Unholy Blight", WellKnownSpells.UnholyBlight, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellUnholyBlight)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D03B0)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 231)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 2, 223)
                .AddHandler(DispatcherType.DealingDamage, sub_100C5990, 2, 223)
                .AddHandler(DispatcherType.SkillLevel, sub_100C5A30, 2, 223)
                .AddHandler(DispatcherType.AbilityCheckModifier, sub_100C5A30, 2, 223)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 231, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 231)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 97, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 152, 0)
                .Build();


        [TempleDllLocation(0x102e09f0)]
        public static readonly ConditionSpec SpellVampiricTouch =
            CreateSpellEffect("sp-Vampiric Touch", WellKnownSpells.VampiricTouch, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 232)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.QueryReturnSpellId)
                .AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 232)
                .AddHandler(DispatcherType.DealingDamage2, d20_mods_spells_vampiric_touch_add_temp_hp, 232)
                .AddHandler(DispatcherType.TakingDamage2, vampiric_touch_taking_damage, 232)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, VampiricTouchSignalTouchAttack, 232)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 232, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 232)
                .AddSignalHandler(D20DispatcherKey.SIG_TouchAttackAdded, sub_100DBE40, 232)
                .AddHandler(DispatcherType.Tooltip, TooltipHoldingCharges, 70, 232)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TempHPTooltipCallback, 74)
                .AddHandler(DispatcherType.RadialMenuEntry, TouchAttackDischargeRadialMenu, 232)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 71, 0)
                .Build();


        [TempleDllLocation(0x102e0b78)]
        public static readonly ConditionSpec SpellVirtue = CreateSpellEffect("sp-Virtue", WellKnownSpells.Virtue, 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 233)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, sub_100D03D0)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 233)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, Spell_remove_spell, 233, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, Spell_remove_mod, 233)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 233, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 233)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 72, 0)
            .Build();


        [TempleDllLocation(0x102e0c70)]
        public static readonly ConditionSpec SpellWeb = CreateSpellEffect("sp-Web", WellKnownSpells.Web, 5)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 234)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
            .AddHandler(DispatcherType.ConditionAdd, BeginSpellWeb)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 234)
            .AddHandler(DispatcherType.ObjectEvent, WebObjEvent, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Web_Burning, WebBurningDamage, 234)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 234)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .Build();


        [TempleDllLocation(0x102e0eb0)]
        public static readonly ConditionSpec SpellWebOn = CreateSpellEffect("sp-Web On", WellKnownSpells.Web, 3)
            .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellWebOn)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, WebSpellInterrupted)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellWebOn)
            .SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, true)
            .AddHandler(DispatcherType.ConditionAdd, WebHit)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100CB6B0, 2, 230)
            .AddHandler(DispatcherType.AbilityScoreLevel, WebAbilityScoreMalus, Stat.dexterity, 230)
            .AddHandler(DispatcherType.GetMoveSpeedBase, WebOnSpeedNull, 0, 230)
            .AddHandler(DispatcherType.ObjectEvent, WebObjEvent, 235)
            .AddHandler(DispatcherType.TakingDamage2, WebOnBurningCallback, 235)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_BreakFree, TurnBasedStatus_web_Callback, 235, SpellWebOff)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 235, SpellWeb)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 235)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 101, 0)
            .AddHandler(DispatcherType.RadialMenuEntry, WebBreakfreeRadial, 235)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 145, 0)
            .Build();


        [TempleDllLocation(0x102e0d90)]
        public static readonly ConditionSpec SpellWebOff = CreateSpellEffect("sp-Web Off", WellKnownSpells.Web, 4)
            .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellWebOff)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellWebOff)
            .AddHandler(DispatcherType.TurnBasedStatusInit, TurnBasedStatus_web_Callback, 236, SpellWebOff)
            .AddHandler(DispatcherType.GetMoveSpeedBase, WebOffMovementSpeed, 0, 230)
            .AddHandler(DispatcherType.ObjectEvent, WebObjEvent, 236)
            .AddHandler(DispatcherType.TakingDamage2, WebOnBurningCallback, 236)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 236, SpellWeb)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 236)
            .Build();


        [TempleDllLocation(0x102e1070)]
        public static readonly ConditionSpec SpellWindWall =
            CreateSpellEffect("sp-Wind Wall", WellKnownSpells.WindWall, 4)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 237)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddHandler(DispatcherType.ConditionAdd, BeginSpellWindWall)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 237)
                .AddHandler(DispatcherType.ObjectEvent, WindWallAoeEvent, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 237)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e1158)]
        public static readonly ConditionSpec SpellWindWallHit =
            CreateSpellEffect("sp-Wind Wall Hit", WellKnownSpells.WindWall, 3)
                .AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellWindWallHit)
                .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                    CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellWindWallHit)
                .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.ObjectEvent, WindWallAoeEvent, 238)
                .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, WindWall_Concealment_Chance, 30, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 238, SpellWindWall)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 238)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 89, 0)
                .Build();


        [TempleDllLocation(0x102e1268)]
        public static readonly ConditionSpec SpellSummoned = ConditionSpec.Create("sp-Summoned", 3)
            .AddHandler(DispatcherType.ConditionAdd, spSummonedOnAdd)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 239)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSummoned)
            .SetQueryResult(D20DispatcherKey.QUE_ExperienceExempt, true)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 239)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 64, 0)
            .SetQueryResult(D20DispatcherKey.QUE_ExperienceExempt, true)
            .Build();


        [TempleDllLocation(0x102e1350)]
        public static readonly ConditionSpec SpellFrogTongue =
            CreateSpellEffect("sp-Frog Tongue", WellKnownSpells.SpellMonsterFrogTongue, 3)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Grappling, true)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFrogTongue)
                .AddHandler(DispatcherType.ConditionAdd, FrogTongueOnAdd)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 240)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_mod, 240)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, FrogTongueHpChanged, 240)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, FrogTongueCritterKilled, 240)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 240, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 240)
                .Build();


        [TempleDllLocation(0x102e15c0)]
        public static readonly ConditionSpec SpellFrogTongueGrappled =
            CreateSpellEffect("sp-Frog Tongue Grappled", WellKnownSpells.SpellMonsterFrogTongue, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellFrogTongueSwallowed)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Grappling, true)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellFrogTongueGrappled)
                .SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, true)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 241, SpellFrogTongueGrappled)
                .AddSignalHandler(D20DispatcherKey.SIG_BreakFree, FrogTongue_breakfree_callback, 241)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, FrogTongueCritterKilled, 241)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 241, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 241)
                .RemoveOnSignal(D20DispatcherKey.SIG_Combat_End)
                .Build();


        [TempleDllLocation(0x102e1460)]
        public static readonly ConditionSpec SpellFrogTongueSwallowed =
            CreateSpellEffect("sp-Frog Tongue Swallowed", WellKnownSpells.SpellMonsterFrogTongue, 3)
                .AddHandler(DispatcherType.ConditionAdd, FrogTongueSwallowedOnAdd)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellFrogTongueSwallowed)
                .SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, false)
                .AddHandler(DispatcherType.BeginRound, FrongTongueSwallowedDamage, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, FrogTongueHpChanged, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_BreakFree, FrogTongue_breakfree_callback, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_spell, 242, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_mod, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, FrogTongueCritterKilled, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 242, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 242)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .Build();


        [TempleDllLocation(0x102e16e0)]
        public static readonly ConditionSpec SpellFrogTongueSwallowing =
            CreateSpellEffect("sp-Frog Tongue Swallowing", WellKnownSpells.SpellMonsterFrogTongue, 3)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellFrogTongueSwallowing)
                .AddHandler(DispatcherType.GetMoveSpeedBase, CommonConditionCallbacks.GrappledMoveSpeed, 0, 232)
                .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.GrappledMoveSpeed, 0, 232)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, FrogTongueHpChanged, 243)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 243, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 243)
                .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, FrogTongueCritterKilled, 243)
                .AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_mod, 243)
                .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                .Build();


        [TempleDllLocation(0x102e1800)]
        public static readonly ConditionSpec SpellVrockScreech =
            CreateSpellEffect("sp-Vrock Screech", WellKnownSpells.SpellMonsterVrockScreech, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 244)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Stunned, true)
                .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
                .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
                .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
                .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
                .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 0)
                .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, sub_100CB8E0, 2)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 244, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 244)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 89, 0)
                .Build();


        [TempleDllLocation(0x102d0bc0)]
        public static readonly ConditionSpec SpellVrockSpores =
            CreateSpellEffect("sp-Vrock Spores", WellKnownSpells.SpellMonsterVrockSpores, 3)
                .AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellBless)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellNeutralizePoison)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveDisease)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 245)
                .AddHandler(DispatcherType.TurnBasedStatusInit, VrockSporesDamage, 245)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 245, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 245)
                .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 55, 0)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 130, 0)
                .Build();


        [TempleDllLocation(0x102e1960)]
        public static readonly ConditionSpec SpellRingofFreedomofMovement =
            CreateSpellEffect("sp-Ring of Freedom of Movement", WellKnownSpells.RingOfFreedomOfMovement, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 246)
                .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
                .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Magical_Item_Deactivate, FreedomOfMovementRingDeactivate)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 246, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 246)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 38, 0)
                .Build();


        [TempleDllLocation(0x102e1a48)]
        public static readonly ConditionSpec SpellPotionofEnlarge =
            CreateSpellEffect("sp-Potion of Enlarge", WellKnownSpells.PotionOfEnlarge, 3)
                .Prevents(SpellEnlarge)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellReduce)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEnlarge)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 247)
                .AddHandler(DispatcherType.AbilityScoreLevel, EnlargeStatLevelGet, Stat.strength, 244)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 247, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 247)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 33, 0)
                .Build();


        [TempleDllLocation(0x102e1b40)]
        public static readonly ConditionSpec SpellPotionofHaste =
            CreateSpellEffect("sp-Potion of Haste", WellKnownSpells.PotionOfHaste, 3)
                .AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellSlow)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 248)
                .AddHandler(DispatcherType.ConditionAdd, sub_100CDE90)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 248)
                .AddHandler(DispatcherType.GetBonusAttacks, HasteBonusAttack)
                .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 8, 174)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100C58A0, 1, 174)
                .AddHandler(DispatcherType.GetMoveSpeedBase, HasteMoveSpeed, 0, 174)
                .AddHandler(DispatcherType.GetMoveSpeed, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 248, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 248)
                .AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 43, 0)
                .Build();


        [TempleDllLocation(0x102e1dd0)]
        public static readonly ConditionSpec SpellDustofDisappearance =
            CreateSpellEffect("sp-Dust of Disappearance", WellKnownSpells.DustOfDisappearance, 3)
                .AddHandler(DispatcherType.DispelCheck, DispelCheck, 250)
                .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId,
                    SpellInvisibility)
                .AddHandler(DispatcherType.ConditionAdd, SpellInvisibilityBegin)
                .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 250)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 250, 0)
                .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 250)
                .Build();


        [TempleDllLocation(0x102e1ea0)]
        public static readonly ConditionSpec SpellPotionofcharisma =
            CreateSpellEffect("sp-Potion of charisma", WellKnownSpells.PotionOfCharisma, 3)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
                .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, sub_100D2AC0)
                .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
                .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e1f48)]
        public static readonly ConditionSpec SpellPotionofglibness =
            CreateSpellEffect("sp-Potion of glibness", WellKnownSpells.PotionOfGlibness, 3)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
                .AddSkillLevelHandler(SkillId.bluff, PotionOfGlibnessSkillLevel)
                .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
                .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e1ff0)]
        public static readonly ConditionSpec SpellPotionofhiding =
            CreateSpellEffect("sp-Potion of hiding", WellKnownSpells.PotionOfHiding, 3)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
                .AddSkillLevelHandler(SkillId.hide, PotionOfHidingSneaking)
                .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
                .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e2098)]
        public static readonly ConditionSpec SpellPotionofsneaking =
            CreateSpellEffect("sp-Potion of sneaking", WellKnownSpells.PotionOfSneaking, 3)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
                .AddSkillLevelHandler(SkillId.move_silently, PotionOfHidingSneaking)
                .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
                .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e2140)]
        public static readonly ConditionSpec SpellPotionofheroism =
            CreateSpellEffect("sp-Potion of heroism", WellKnownSpells.PotionOfHeroism, 3)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
                .AddHandler(DispatcherType.SkillLevel, PotionOfHeroismSkillBonus, 2)
                .AddHandler(DispatcherType.SaveThrowLevel, sub_100D2BA0, 2)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100D2B70, 2)
                .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
                .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e2210)]
        public static readonly ConditionSpec SpellPotionofsuperheroism =
            CreateSpellEffect("sp-Potion of super-heroism", WellKnownSpells.PotionOfSuperHeroism, 3)
                .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
                .AddHandler(DispatcherType.SkillLevel, PotionOfHeroismSkillBonus, 4)
                .AddHandler(DispatcherType.SaveThrowLevel, sub_100D2BA0, 4)
                .AddHandler(DispatcherType.ToHitBonus2, sub_100D2B70, 4)
                .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
                .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
                .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                .Build();


        [TempleDllLocation(0x102e22e0)]
        public static readonly ConditionSpec SpellPotionofprotectionfromenergy = ConditionSpec
            .Create("sp-Potion of protection from energy", 4)
            .AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
            .AddHandler(DispatcherType.TakingDamage2, PotionOfProtectionFromEnergyDamageCallback)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
            .AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .Build();


        [TempleDllLocation(0x102e2388)]
        public static readonly ConditionSpec SpellProtectionFromMonster = ConditionSpec
            .Create("sp-Protection From Monster", 3)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 252)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 252)
            .AddHandler(DispatcherType.GetAC, SpellArmorBonus, 11, 264)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 2, 264)
            .AddHandler(DispatcherType.TakingDamage2, ProtectionFromAlignmentDamageCallback, 173)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 252, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 252)
            .Build();


        [TempleDllLocation(0x102e2498)]
        public static readonly ConditionSpec SpellProtectionFromMagic = ConditionSpec
            .Create("sp-Protection From Magic", 3)
            .AddHandler(DispatcherType.ConditionAdd, sub_100D2E30)
            .AddHandler(DispatcherType.SpellResistanceMod, SpellResistanceMod_ProtFromMagic_Callback, 5048)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, QueryHasSpellResistance)
            .AddHandler(DispatcherType.Tooltip, SpellResistanceTooltipCallback, 5048)
            .AddHandler(DispatcherType.DispelCheck, DispelCheck, 253)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
            .AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 253)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
            .AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 253, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 253)
            .Build();


        [TempleDllLocation(0x102e25a8)]
        public static readonly ConditionSpec SpellSlipperyMind = ConditionSpec.Create("sp-Slippery Mind", 3)
            .AddHandler(DispatcherType.ConditionAdd, SlipperyMindInit)
            .AddHandler(DispatcherType.BeginRound, SlipperyMindActivate, 251)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 251)
            .Build();

    }
}