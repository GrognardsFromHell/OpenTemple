using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
namespace SpicyTemple.Core.Systems.D20.Conditions {

public static partial class SpellEffects {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102d0160)]
  public static readonly ConditionSpec SpellHoldTouchSpell = ConditionSpec.Create("sp-Hold Touch Spell", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 0)
.AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
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
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Concentrating, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
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
  public static readonly ConditionSpec SpellAid = ConditionSpec.Create("sp-Aid", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellAnimalFriendship = ConditionSpec.Create("sp-Animal Friendship", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 4)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellAnimalFriendship)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellAnimalGrowth = ConditionSpec.Create("sp-Animal Growth", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellAnimalGrowth)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 5)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 5)
.AddHandler(DispatcherType.ConditionAdd, enlargeModelScaleInc)
.AddHandler(DispatcherType.ToHitBonus2, RighteousMightToHitBonus, 1, 274)
.AddHandler(DispatcherType.GetAC, sub_100C6050, 1, 274)
.AddHandler(DispatcherType.GetAC, sub_100C6080, 2, 274)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_AnimalGrowth, Stat.strength, 8)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_AnimalGrowth, Stat.constitution, 4)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_AnimalGrowth, Stat.dexterity, 2)
.AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 4, 274)
.AddHandler(DispatcherType.TakingDamage, sub_100C6020, 10, 4)
.AddHandler(DispatcherType.GetAttackDice, AttackDiceAnimalGrowth, 0, 274)
.AddHandler(DispatcherType.GetSizeCategory, EnlargeSizeCategory, 0, 274)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 5, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 5)
                    .Build();


[TempleDllLocation(0x102d07c0)]
  public static readonly ConditionSpec SpellAnimalTrance = ConditionSpec.Create("sp-Animal Trance", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 6)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellAnimalTrance)
.AddHandler(DispatcherType.ConditionAdd, AnimalTranceBeginSpell)
.AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellAnimateDead = ConditionSpec.Create("sp-Animate Dead", 4)
.AddHandler(DispatcherType.ConditionAddPre, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, AnimateDeadOnAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 7)
                    .Build();


[TempleDllLocation(0x102d0e00)]
  public static readonly ConditionSpec SpellBane = ConditionSpec.Create("sp-Bane", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellBless)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 8)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellBarkskin = ConditionSpec.Create("sp-Barkskin", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 9)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 9)
.AddHandler(DispatcherType.GetAC, SpellArmorBonus, 10, 141)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 9, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 9)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 9, 0)
                    .Build();


[TempleDllLocation(0x102d1360)]
  public static readonly ConditionSpec SpellBestowCurseAbility = ConditionSpec.Create("sp-Bestow Curse Ability", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
.AddHandler(DispatcherType.DispelCheck, BreakEnchantmentDispelCheck, 10)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100CC240)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBestowCurseAbility)
.AddHandler(DispatcherType.AbilityScoreLevel, BestowCurseAbilityMalus, 0, 243)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 51, 0)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltipBestowCurse, 106)
                    .Build();


[TempleDllLocation(0x102d1008)]
  public static readonly ConditionSpec SpellBestowCurseRolls = ConditionSpec.Create("sp-Bestow Curse Rolls", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
.AddHandler(DispatcherType.DispelCheck, BreakEnchantmentDispelCheck, 11)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBestowCurseRolls)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellBestowCurseActions = ConditionSpec.Create("sp-Bestow Curse Actions", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
.AddHandler(DispatcherType.DispelCheck, BreakEnchantmentDispelCheck, 12)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBestowCurseAbility)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100CC2A0)
.AddHandler(DispatcherType.TurnBasedStatusInit, BestowCurseActionsTurnBasedStatusInit)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 51, 0)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 106, 196)
                    .Build();


[TempleDllLocation(0x102d0cb8)]
  public static readonly ConditionSpec SpellBless = ConditionSpec.Create("sp-Bless", 3)
.Prevents(SpellVrockSpores)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellBane)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 13)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Obj_Is_Blessed, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellBlindness = ConditionSpec.Create("sp-Blindness", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.SetUnique()
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveBlindness)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBlindness)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
.AddHandler(DispatcherType.ConditionAdd, sub_100CC2E0)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 189)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
.AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
.AddHandler(DispatcherType.GetAttackerConcealmentMissChance, CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
.AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
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
  public static readonly ConditionSpec SpellBlink = ConditionSpec.Create("sp-Blink", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 15)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, BlinkSpellFailure)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellBlink)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellBlur = ConditionSpec.Create("sp-Blur", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 16)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellBreakEnchantment = ConditionSpec.Create("sp-Break Enchantment", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BreakEnchantmentInit, 17)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 17, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 17)
                    .Build();


[TempleDllLocation(0x102d1a68)]
  public static readonly ConditionSpec SpellBullsStrength = ConditionSpec.Create("sp-Bulls Strength", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 18)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 18)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.strength, 217)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 18, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 18)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 13, 0)
                    .Build();


[TempleDllLocation(0x102d1b50)]
  public static readonly ConditionSpec SpellCallLightning = ConditionSpec.Create("sp-Call Lightning", 3)
.SetUnique()
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 19)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Can_Call_Lightning, sub_100C6440)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCallLightningStorm = ConditionSpec.Create("sp-Call Lightning Storm", 3)
.SetUnique()
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 19)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Can_Call_Lightning, sub_100C6440)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCalmAnimals = ConditionSpec.Create("sp-Calm Animals", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 20)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCalmAnimals)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCalmEmotions = ConditionSpec.Create("sp-Calm Emotions", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 21)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCalmEmotions)
.AddQueryHandler(D20DispatcherKey.QUE_IsActionInvalid_CheckAction, CalmEmotionsActionInvalid, SpellCalmEmotions)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCatsGrace = ConditionSpec.Create("sp-Cats Grace", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 22)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 22)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.dexterity, 218)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 22, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 22)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                    .Build();


[TempleDllLocation(0x102d21f8)]
  public static readonly ConditionSpec SpellCauseFear = ConditionSpec.Create("sp-Cause Fear", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 23)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, IsCritterAfraidQuery)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellChaosHammer = ConditionSpec.Create("sp-Chaos Hammer", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 24)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.NONE)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellChaosHammer)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCharmMonster = ConditionSpec.Create("sp-Charm Monster", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellCharmMonster)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 25)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCharmMonster)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCharmPerson = ConditionSpec.Create("sp-Charm Person", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellCharmPerson)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 26)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCharmPerson)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCharmPersonorAnimal = ConditionSpec.Create("sp-Charm Person or Animal", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellCharmPersonorAnimal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 27)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCharmPersonorAnimal)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellChillMetal = ConditionSpec.Create("sp-Chill Metal", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeatMetal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 28)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellChillTouch = ConditionSpec.Create("sp-Chill Touch", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 29)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellClairaudienceClairvoyance = ConditionSpec.Create("sp-Clairaudience Clairvoyance", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 30)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellClairaudienceClairvoyance)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCloudkill = ConditionSpec.Create("sp-Cloudkill", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 31)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellCloudkill)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 31)
.AddHandler(DispatcherType.ObjectEvent, AoeObjEventCloudkill, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 31)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d2eb0)]
  public static readonly ConditionSpec SpellCloudkillDamage = ConditionSpec.Create("sp-Cloudkill-Damage", 3)
.AddHandler(DispatcherType.ConditionAddPre, CloudkillDamagePreAdd, SpellCloudkillDamage)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCloudkillDamage)
.AddHandler(DispatcherType.BeginRound, CloudkillBeginRound)
.AddHandler(DispatcherType.ObjectEvent, AoeObjEventCloudkill, 32)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 32, SpellCloudkill)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 32)
                    .Build();


[TempleDllLocation(0x102d2f98)]
  public static readonly ConditionSpec SpellColorSprayBlind = ConditionSpec.Create("sp-Color Spray Blind", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellColorSprayBlind)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellColorSprayBlind)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 33)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
.AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
.AddHandler(DispatcherType.GetAttackerConcealmentMissChance, CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
.AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
.AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 33, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 33)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 100, 0)
                    .Build();


[TempleDllLocation(0x102d3130)]
  public static readonly ConditionSpec SpellColorSprayStun = ConditionSpec.Create("sp-Color Spray Stun", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellColorSprayUnconscious = ConditionSpec.Create("sp-Color Spray Unconscious", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
.SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
.SetQueryResult(D20DispatcherKey.QUE_CoupDeGrace, true)
.SetQueryResult(D20DispatcherKey.QUE_Unconscious, true)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellCommand = ConditionSpec.Create("sp-Command", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 36)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellCommand)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellConfusion = ConditionSpec.Create("sp-Confusion", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.Prevents(SpellCalmEmotions)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 37)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_AI_Has_Spell_Override, ConfusionHasAiOverride)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Confused, sub_100C6D60)
.AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, DummyCallbacks.EmptyFunction)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100CC800)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 37)
.AddHandler(DispatcherType.TurnBasedStatusInit, sub_100C6DC0)
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
  public static readonly ConditionSpec SpellConsecrate = ConditionSpec.Create("sp-Consecrate", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 38)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellConsecrate)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 38)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__consecrate_sthg, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 38)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d3778)]
  public static readonly ConditionSpec SpellConsecrateHit = ConditionSpec.Create("sp-Consecrate Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellConsecrateHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellConsecrateHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground, sub_100C6F70, 3, 6)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__consecrate_sthg, 39)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 39, SpellConsecrate)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 39)
                    .Build();


[TempleDllLocation(0x102d3838)]
  public static readonly ConditionSpec SpellConsecrateHitUndead = ConditionSpec.Create("sp-Consecrate Hit Undead", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellConsecrateHitUndead)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellConsecrateHitUndead)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellControlPlants = ConditionSpec.Create("sp-Control Plants", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 41)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellControlPlants)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 41)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 41)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d3a30)]
  public static readonly ConditionSpec SpellControlPlantsTracking = ConditionSpec.Create("sp-Control Plants Tracking", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 42)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.SkillLevel, sub_100C7140, 10, 226)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 42, SpellControlPlants)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 42)
                    .Build();


[TempleDllLocation(0x102d3af0)]
  public static readonly ConditionSpec SpellControlPlantsCharm = ConditionSpec.Create("sp-Control Plants Charm", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsCharm)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellControlPlantsCharm)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100CCD80)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 43)
.AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DispCritterKilledRemoveSpellAndMod)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 43, SpellControlPlants)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 43)
                    .Build();


[TempleDllLocation(0x102d3c00)]
  public static readonly ConditionSpec SpellControlPlantsDisentangle = ConditionSpec.Create("sp-Control Plants Disentangle", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsDisentangle)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellControlPlantsDisentangle)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 44)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 44, SpellControlPlants)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 44)
                    .Build();


[TempleDllLocation(0x102d3cd0)]
  public static readonly ConditionSpec SpellControlPlantsEntanglePre = ConditionSpec.Create("sp-Control Plants Entangle Pre", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsEntanglePre)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellControlPlantsEntanglePre)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, ControlPlantsEntangleBeginRound, 45)
.AddHandler(DispatcherType.GetMoveSpeed, sub_100C7040, 0, 228)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__control_plants_sthg, 45)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 45, SpellControlPlants)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 45)
                    .Build();


[TempleDllLocation(0x102d3dc8)]
  public static readonly ConditionSpec SpellControlPlantsEntangle = ConditionSpec.Create("sp-Control Plants Entangle", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellControlPlantsEntangle)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, ControlPlantsEntangleSpellInterruptedCheck)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellControlPlantsEntangle)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDarkvision = ConditionSpec.Create("sp-Darkvision", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 47)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Darkvision, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 47)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 47, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 47)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 16, 0)
                    .Build();


[TempleDllLocation(0x102d4070)]
  public static readonly ConditionSpec SpellDaze = ConditionSpec.Create("sp-Daze", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 48)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDeathWard = ConditionSpec.Create("sp-Death Ward", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 50)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellNegativeEnergyProtection)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 50)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 50, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 50)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 18, 0)
                    .Build();


[TempleDllLocation(0x102d41b8)]
  public static readonly ConditionSpec SpellDeathKnell = ConditionSpec.Create("sp-Death Knell", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 49)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100CCE70)
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
  public static readonly ConditionSpec SpellDeafness = ConditionSpec.Create("sp-Deafness", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveDeafness)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Deafened, true)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, DeafnessSpellFailure)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDelayPoison = ConditionSpec.Create("sp-Delay Poison", 3)
.Prevents(SpellVrockSpores)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellDelayPoison)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 52)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 52, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 52)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 19, 0)
                    .Build();


[TempleDllLocation(0x102d47d0)]
  public static readonly ConditionSpec SpellDesecrate = ConditionSpec.Create("sp-Desecrate", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 53)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellDesecrate)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 53)
.AddHandler(DispatcherType.ObjectEvent, ObjEventAoEDesecrate, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 53)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d48a0)]
  public static readonly ConditionSpec SpellDesecrateHit = ConditionSpec.Create("sp-Desecrate Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellDesecrateHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellDesecrateHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground, sub_100C6F70, 3, 6)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ObjectEvent, ObjEventAoEDesecrate, 54)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 54, SpellDesecrate)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 54)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 109, 0)
                    .Build();


[TempleDllLocation(0x102d4998)]
  public static readonly ConditionSpec SpellDesecrateHitUndead = ConditionSpec.Create("sp-Desecrate Hit Undead", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellDesecrateHitUndead)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellDesecrateHitUndead)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectChaos = ConditionSpec.Create("sp-Detect Chaos", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 56)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Chaos, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectEvil = ConditionSpec.Create("sp-Detect Evil", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 57)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Evil, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectGood = ConditionSpec.Create("sp-Detect Good", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 58)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Good, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectLaw = ConditionSpec.Create("sp-Detect Law", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 59)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Law, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectMagic = ConditionSpec.Create("sp-Detect Magic", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 60)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Magic, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectSecretDoors = ConditionSpec.Create("sp-Detect Secret Doors", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 61)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDetectUndead = ConditionSpec.Create("sp-Detect Undead", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 62)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Undead, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDimensionalAnchor = ConditionSpec.Create("sp-Dimensional Anchor", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 63)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellDimensionalAnchor)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 63)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 63, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 63)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 161, 0)
                    .Build();


[TempleDllLocation(0x102d5438)]
  public static readonly ConditionSpec SpellDiscernLies = ConditionSpec.Create("sp-Discern Lies", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 64)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Discern_Lies, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 64)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 64, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 64)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 26, 0)
                    .Build();


[TempleDllLocation(0x102d5520)]
  public static readonly ConditionSpec SpellDispelAir = ConditionSpec.Create("sp-Dispel Air", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 65)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelEarth = ConditionSpec.Create("sp-Dispel Earth", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 66)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelFire = ConditionSpec.Create("sp-Dispel Fire", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 67)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelWater = ConditionSpec.Create("sp-Dispel Water", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 68)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelChaos = ConditionSpec.Create("sp-Dispel Chaos", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 69)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelEvil = ConditionSpec.Create("sp-Dispel Evil", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 70)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelGood = ConditionSpec.Create("sp-Dispel Good", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 71)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelLaw = ConditionSpec.Create("sp-Dispel Law", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 72)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDispelMagic = ConditionSpec.Create("sp-Dispel Magic", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, DispelMagicOnAdd, 73)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 73, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 73)
                    .Build();


[TempleDllLocation(0x102d6208)]
  public static readonly ConditionSpec SpellDisplacement = ConditionSpec.Create("sp-Displacement", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 74)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 74)
.AddHandler(DispatcherType.GetDefenderConcealmentMissChance, sub_100C5BE0, 50, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 74, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 74)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 27, 0)
                    .Build();


[TempleDllLocation(0x102d62f0)]
  public static readonly ConditionSpec SpellDivineFavor = ConditionSpec.Create("sp-Divine Favor", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 75)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDivinePower = ConditionSpec.Create("sp-Divine Power", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 76)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDominateAnimal = ConditionSpec.Create("sp-Dominate Animal", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 77)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellDominateAnimal)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDominatePerson = ConditionSpec.Create("sp-Dominate Person", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 78)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellDominatePerson)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100C4370)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDoom = ConditionSpec.Create("sp-Doom", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 79)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEaglesSplendor = ConditionSpec.Create("sp-Eagles Splendor", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 80)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 80)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.charisma, 292)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 80, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 80)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                    .Build();


[TempleDllLocation(0x102d6b30)]
  public static readonly ConditionSpec SpellEmotionDespair = ConditionSpec.Create("sp-Emotion Despair", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionHope)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 81)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEmotionDespair)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEmotionFear = ConditionSpec.Create("sp-Emotion Fear", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionRage)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 82)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEmotionFear)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEmotionFriendship = ConditionSpec.Create("sp-Emotion Friendship", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionHate)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 83)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEmotionFriendship)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEmotionHate = ConditionSpec.Create("sp-Emotion Hate", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionFriendship)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 84)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEmotionHate)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEmotionHope = ConditionSpec.Create("sp-Emotion Hope", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEmotionDespair)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 85)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEmotionHope)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEmotionRage = ConditionSpec.Create("sp-Emotion Rage", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 86)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEmotionRage)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEndurance = ConditionSpec.Create("sp-Endurance", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 87)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 87)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.constitution, 186)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 87, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 87)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 31, 0)
                    .Build();


[TempleDllLocation(0x102d7310)]
  public static readonly ConditionSpec SpellEndureElements = ConditionSpec.Create("sp-Endure Elements", 4)
.AddHandler(DispatcherType.ConditionAddPre, sub_100C77D0, SpellEndureElements)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 88)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Endure_Elements, sub_100C7820)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEnlarge = ConditionSpec.Create("sp-Enlarge", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellEnlarge)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellReduce)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 89)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEnlarge)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
.AddHandler(DispatcherType.ConditionAdd, enlargeModelScaleInc)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 89)
.AddHandler(DispatcherType.AbilityScoreLevel, EnlargeStatLevelGet, 0, 244)
.AddHandler(DispatcherType.AbilityScoreLevel, EnlargeStatLevelGet, 1, 244)
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
  public static readonly ConditionSpec SpellEntangle = ConditionSpec.Create("sp-Entangle", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 90)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEntangleOn = ConditionSpec.Create("sp-Entangle On", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellEntangleOn)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, sub_100C79E0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEntangleOn)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellEntangleOff = ConditionSpec.Create("sp-Entangle Off", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellEntangleOff)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEntangleOff)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, sub_100D4440, 92)
.AddHandler(DispatcherType.GetMoveSpeed, sub_100C7040, 0, 228)
.AddHandler(DispatcherType.ObjectEvent, ObjEventAoEEntangle, 92)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 92, SpellEntangle)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 92)
                    .Build();


[TempleDllLocation(0x102d7b68)]
  public static readonly ConditionSpec SpellEntropicShield = ConditionSpec.Create("sp-Entropic Shield", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 93)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellExpeditiousRetreat = ConditionSpec.Create("sp-Expeditious Retreat", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 94)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFaerieFire = ConditionSpec.Create("sp-Faerie Fire", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 95)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFaerieFire)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFalseLife = ConditionSpec.Create("sp-False Life", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 96)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFalseLife)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFeeblemind = ConditionSpec.Create("sp-Feeblemind", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Mute, true)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.SetQueryResult(D20DispatcherKey.QUE_CannotUseIntSkill, true)
.SetQueryResult(D20DispatcherKey.QUE_CannotUseChaSkill, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFear = ConditionSpec.Create("sp-Fear", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 97)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, IsCritterAfraidQuery)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFindTraps = ConditionSpec.Create("sp-Find Traps", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 99)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Find_Traps, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 99)
.AddHandler(DispatcherType.SkillLevel, SkillModifier_FindTraps_Callback, 0, 284)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 99, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 99)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 36, 0)
                    .Build();


[TempleDllLocation(0x102d8358)]
  public static readonly ConditionSpec SpellFireShield = ConditionSpec.Create("sp-Fire Shield", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 100)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFlare = ConditionSpec.Create("sp-Flare", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 101)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFogCloud = ConditionSpec.Create("sp-Fog Cloud", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 102)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellFogCloud)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 102)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__fog_cloud_sthg, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 102)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d85a0)]
  public static readonly ConditionSpec SpellFogCloudHit = ConditionSpec.Create("sp-Fog Cloud Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellFogCloudHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFogCloudHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellFoxsCunning = ConditionSpec.Create("sp-Foxs Cunning", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 104)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 104)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.intelligence, 293)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 104, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 104)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                    .Build();


[TempleDllLocation(0x102d88a8)]
  public static readonly ConditionSpec SpellFreedomofMovement = ConditionSpec.Create("sp-Freedom of Movement", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 105)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 105)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 105, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 105)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 38, 0)
                    .Build();


[TempleDllLocation(0x102d8990)]
  public static readonly ConditionSpec SpellGaseousForm = ConditionSpec.Create("sp-Gaseous Form", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 106)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, GaseousFormSpellInterruptedQuery)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGaseousForm)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGhoulTouch = ConditionSpec.Create("sp-Ghoul Touch", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 107)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGhoulTouchParalyzed = ConditionSpec.Create("sp-Ghoul Touch Paralyzed", 3)
.AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 108)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 108)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGhoulTouchStench = ConditionSpec.Create("sp-Ghoul Touch Stench", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 109)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellGhoulTouchStench)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 109)
.AddHandler(DispatcherType.ObjectEvent, Condition__36_ghoul_touch_stench_sthg, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 109)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d8f28)]
  public static readonly ConditionSpec SpellGhoulTouchStenchHit = ConditionSpec.Create("sp-Ghoul Touch Stench Hit", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGhoulTouchStenchHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGlibness = ConditionSpec.Create("sp-Glibness", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 111)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGlibness)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGlitterdustBlindness = ConditionSpec.Create("sp-Glitterdust Blindness", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveBlindness)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 14)
.AddHandler(DispatcherType.ConditionAdd, sub_100CC2E0)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 14)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
.AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
.AddHandler(DispatcherType.GetAttackerConcealmentMissChance, CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
.AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
.AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 14, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 14)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 122, 0)
                    .Build();


[TempleDllLocation(0x102d9370)]
  public static readonly ConditionSpec SpellGlitterdust = ConditionSpec.Create("sp-Glitterdust", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 113)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGlitterdust)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 113)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 113, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 113)
                    .Build();


[TempleDllLocation(0x102d9440)]
  public static readonly ConditionSpec SpellGoodberry = ConditionSpec.Create("sp-Goodberry", 3)
.AddHandler(DispatcherType.ConditionAdd, GoodberryAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102d9498)]
  public static readonly ConditionSpec SpellGoodberryTally = ConditionSpec.Create("sp-Goodberry Tally", 3)
.AddHandler(DispatcherType.ConditionAddPre, sub_100C8240, SpellGoodberry)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, sub_100CDC00)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100CC800)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 0)
                    .Build();


[TempleDllLocation(0x102d9540)]
  public static readonly ConditionSpec SpellGrease = ConditionSpec.Create("sp-Grease", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 116)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGreaseHit = ConditionSpec.Create("sp-Grease Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellGreaseHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellGreaseHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGreaterHeroism = ConditionSpec.Create("sp-Greater Heroism", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 118)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGreaterMagicFang = ConditionSpec.Create("sp-Greater Magic Fang", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 119)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGreaterMagicWeapon = ConditionSpec.Create("sp-Greater Magic Weapon", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 120)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Obj_Is_Blessed, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGuidance = ConditionSpec.Create("sp-Guidance", 5)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellGuidance)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 121)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellGustofWind = ConditionSpec.Create("sp-Gust of Wind", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 122)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHaste = ConditionSpec.Create("sp-Haste", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellSlow)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 123)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHaltUndead = ConditionSpec.Create("sp-Halt Undead", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 124)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHarm = ConditionSpec.Create("sp-Harm", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, HarmOnAdd)
                    .Build();


[TempleDllLocation(0x102d09c8)]
  public static readonly ConditionSpec SpellHeal = ConditionSpec.Create("sp-Heal", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpHealOnConditionAdd)
                    .Build();


[TempleDllLocation(0x102d2918)]
  public static readonly ConditionSpec SpellHeatMetal = ConditionSpec.Create("sp-Heat Metal", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellChillMetal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 127)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHeroism = ConditionSpec.Create("sp-Heroism", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 128)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHoldAnimal = ConditionSpec.Create("sp-Hold Animal", 3)
.AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 0)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 129)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHoldMonster = ConditionSpec.Create("sp-Hold Monster", 3)
.AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 0)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 130)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.NONE)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHoldPerson = ConditionSpec.Create("sp-Hold Person", 3)
.AddHandler(DispatcherType.ConditionAddPre, sub_100DB9C0, SpellRemoveParalysis, 0)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 131)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Held, sub_100C4300, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellHoldPortal = ConditionSpec.Create("sp-Hold Portal", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 132)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 132)
.AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 132, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 132)
                    .Build();


[TempleDllLocation(0x102da840)]
  public static readonly ConditionSpec SpellHolySmite = ConditionSpec.Create("sp-Holy Smite", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveBlindness)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100CC2E0)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 14)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
.AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
.AddHandler(DispatcherType.GetAttackerConcealmentMissChance, CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
.AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
.AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 14, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 14)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 127, 0)
                    .Build();


[TempleDllLocation(0x102da9d8)]
  public static readonly ConditionSpec SpellIceStorm = ConditionSpec.Create("sp-Ice Storm", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 134)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellIceStorm)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 134)
.AddHandler(DispatcherType.ObjectEvent, IceStormHitTrigger, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 134)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102daac0)]
  public static readonly ConditionSpec SpellIceStormHit = ConditionSpec.Create("sp-Ice Storm Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellIceStormHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellIceStormHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellInvisibility = ConditionSpec.Create("sp-Invisibility", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 136)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibility)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellInvisibilityPurge = ConditionSpec.Create("sp-Invisibility Purge", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 137)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellInvisibilityPurgeHit = ConditionSpec.Create("sp-Invisibility Purge Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellInvisibilityPurgeHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibilityPurgeHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.ObjectEvent, Condition__36__invisibility_purge, 138)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 138, SpellInvisibilityPurge)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 138)
                    .Build();


[TempleDllLocation(0x102daee8)]
  public static readonly ConditionSpec SpellInvisibilitySphere = ConditionSpec.Create("sp-Invisibility Sphere", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 139)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellInvisibilitySphereHit = ConditionSpec.Create("sp-Invisibility Sphere Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellInvisibilitySphereHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibilitySphereHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, InvisibilitySphereHitBegin)
.AddHandler(DispatcherType.ObjectEvent, InvisibilitySphereAoeEvent, 140)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 140, SpellInvisibility)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 140)
                    .Build();


[TempleDllLocation(0x102db0c8)]
  public static readonly ConditionSpec SpellInvisibilitytoAnimals = ConditionSpec.Create("sp-Invisibility to Animals", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 141)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibility)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellInvisibilitytoUndead = ConditionSpec.Create("sp-Invisibility to Undead", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 142)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibility)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellImprovedInvisibility = ConditionSpec.Create("sp-Improved Invisibility", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 143)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibility)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellKeenEdge = ConditionSpec.Create("sp-Keen Edge", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 144)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpWeaponKeenOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 144)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 144, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 144)
                    .Build();


[TempleDllLocation(0x102db528)]
  public static readonly ConditionSpec SpellLesserRestoration = ConditionSpec.Create("sp-Lesser Restoration", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpLesserRestorationOnConditionAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102db5a8)]
  public static readonly ConditionSpec SpellLongstrider = ConditionSpec.Create("sp-Longstrider", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 146)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMageArmor = ConditionSpec.Create("sp-Mage Armor", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 147)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMagicCircleOutward = ConditionSpec.Create("sp-Magic Circle Outward", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 149)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMagicFang = ConditionSpec.Create("sp-Magic Fang", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 150)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMagicMissile = ConditionSpec.Create("sp-Magic Missile", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, MagicMissileOnAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dbbd8)]
  public static readonly ConditionSpec SpellMagicStone = ConditionSpec.Create("sp-Magic Stone", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 152)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMagicVestment = ConditionSpec.Create("sp-Magic Vestment", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 153)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMagicWeapon = ConditionSpec.Create("sp-Magic Weapon", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 154)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Obj_Is_Blessed, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, MagicWeaponOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 154)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 154, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 154)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 52, 0)
                    .Build();


[TempleDllLocation(0x102dbec0)]
  public static readonly ConditionSpec SpellMeldIntoStone = ConditionSpec.Create("sp-Meld Into Stone", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 155)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellMeldIntoStone)
.SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMelfsAcidArrow = ConditionSpec.Create("sp-Melfs Acid Arrow", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 156)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, AcidDamage)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 156)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 156, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 156)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 128, 0)
                    .Build();


[TempleDllLocation(0x102dc140)]
  public static readonly ConditionSpec SpellMinorGlobeofInvulnerability = ConditionSpec.Create("sp-Minor Globe of Invulnerability", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 157)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMinorGlobeofInvulnerabilityHit = ConditionSpec.Create("sp-Minor Globe of Invulnerability Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellMinorGlobeofInvulnerabilityHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddHandler(DispatcherType.Unused63, MinorGlobeCallback3F, SpellMinorGlobeofInvulnerabilityHit)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellMinorGlobeofInvulnerabilityHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.ObjectEvent, d20_mods_spells__globe_of_inv_hit, 158)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 158, SpellMinorGlobeofInvulnerability)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 158)
                    .Build();


[TempleDllLocation(0x102dc370)]
  public static readonly ConditionSpec SpellMindFog = ConditionSpec.Create("sp-Mind Fog", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 159)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellMindFog)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 159)
.AddHandler(DispatcherType.ObjectEvent, MindFogAoeEvent, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 159)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dc440)]
  public static readonly ConditionSpec SpellMindFogHit = ConditionSpec.Create("sp-Mind Fog Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellMindFogHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellMindFogHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellMirrorImage = ConditionSpec.Create("sp-Mirror Image", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellMirrorImage)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 161)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Mirror_Image, sub_100C9160)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellMirrorImage)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
.AddHandler(DispatcherType.ConditionAdd, sub_100CEC40)
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
  public static readonly ConditionSpec SpellMordenkainensFaithfulHound = ConditionSpec.Create("sp-Mordenkainens Faithful Hound", 4)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Invisible, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellMordenkainensFaithfulHound)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 162)
.AddHandler(DispatcherType.GetMoveSpeedBase, sub_100C7E70, 0, 101)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 162, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 162)
                    .Build();


[TempleDllLocation(0x102d42d8)]
  public static readonly ConditionSpec SpellNegativeEnergyProtection = ConditionSpec.Create("sp-Negative Energy Protection", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 163)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellNegativeEnergyProtection)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 163)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 163, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 163)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 54, 0)
                    .Build();


[TempleDllLocation(0x102d0a20)]
  public static readonly ConditionSpec SpellNeutralizePoison = ConditionSpec.Create("sp-Neutralize Poison", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 164)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 164)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 164, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 164)
                    .Build();


[TempleDllLocation(0x102dc7b8)]
  public static readonly ConditionSpec SpellObscuringMist = ConditionSpec.Create("sp-Obscuring Mist", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 165)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellObscuringMist)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 165)
.AddHandler(DispatcherType.ObjectEvent, sub_100D5780, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 165)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dc8a0)]
  public static readonly ConditionSpec SpellObscuringMistHit = ConditionSpec.Create("sp-Obscuring Mist Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellObscuringMist)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellObscuringMist)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellOrdersWrath = ConditionSpec.Create("sp-Orders Wrath", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 167)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellOtilukesResilientSphere = ConditionSpec.Create("sp-Otilukes Resilient Sphere", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 168)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellOtilukesResilientSphere)
.SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellOwlsWisdom = ConditionSpec.Create("sp-Owls Wisdom", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 169)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 169)
.AddHandler(DispatcherType.AbilityScoreLevel, StatLevel_callback_SpellModifier, Stat.wisdom, 294)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 169, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 169)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 14, 0)
                    .Build();


[TempleDllLocation(0x102dcd68)]
  public static readonly ConditionSpec SpellPrayer = ConditionSpec.Create("sp-Prayer", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 170)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellProduceFlame = ConditionSpec.Create("sp-Produce Flame", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 171)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellProtectionFromArrows = ConditionSpec.Create("sp-Protection From Arrows", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 172)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpellAddDismissCondition)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 172)
.AddHandler(DispatcherType.TakingDamage, sub_100DD4D0, 10, 4)
.AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, SpellDismissSignalHandler, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 172, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 172)
.AddHandler(DispatcherType.Tooltip, Tooltip2Callback, 75, 172)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 57, 0)
                    .Build();


[TempleDllLocation(0x102dd140)]
  public static readonly ConditionSpec SpellProtectionFromAlignment = ConditionSpec.Create("sp-Protection From Alignment", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 173)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellProtectionFromElements = ConditionSpec.Create("sp-Protection From Elements", 4)
.AddHandler(DispatcherType.ConditionAddPre, sub_100C77D0, SpellProtectionFromElements)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 174)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements, sub_100C7820)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellRage = ConditionSpec.Create("sp-Rage", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 175)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellRage)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellRaiseDead = ConditionSpec.Create("sp-Raise Dead", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RaiseDeadOnConditionAdd)
                    .Build();


[TempleDllLocation(0x102dd5d8)]
  public static readonly ConditionSpec SpellRayofEnfeeblement = ConditionSpec.Create("sp-Ray of Enfeeblement", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 177)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellReduce = ConditionSpec.Create("sp-Reduce", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellReduce)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellEnlarge)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 179)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellReduce)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellReduceAnimal = ConditionSpec.Create("sp-Reduce Animal", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellReduceAnimal)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 178)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellReduceAnimal)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellRemoveBlindness = ConditionSpec.Create("sp-Remove Blindness", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 180)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 180)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 180, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 180)
                    .Build();


[TempleDllLocation(0x102d1250)]
  public static readonly ConditionSpec SpellRemoveCurse = ConditionSpec.Create("sp-Remove Curse", 3)
.Prevents(SpellBestowCurseAbility)
.Prevents(SpellBestowCurseRolls)
.Prevents(SpellBestowCurseActions)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 181)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 181)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 181, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 181)
                    .Build();


[TempleDllLocation(0x102d44b8)]
  public static readonly ConditionSpec SpellRemoveDeafness = ConditionSpec.Create("sp-Remove Deafness", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 182)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 182)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 182, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 182)
                    .Build();


[TempleDllLocation(0x102d0af0)]
  public static readonly ConditionSpec SpellRemoveDisease = ConditionSpec.Create("sp-Remove Disease", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 183)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100DBD90)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 183)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 183, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 183)
                    .Build();


[TempleDllLocation(0x102dd840)]
  public static readonly ConditionSpec SpellRemoveFear = ConditionSpec.Create("sp-Remove Fear", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 184)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 184)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 184, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 184)
                    .Build();


[TempleDllLocation(0x102d8ca0)]
  public static readonly ConditionSpec SpellRemoveParalysis = ConditionSpec.Create("sp-Remove Paralysis", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 185)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dd910)]
  public static readonly ConditionSpec SpellRepelVermin = ConditionSpec.Create("sp-Repel Vermin", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 186)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellRepelVermin)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 186)
.AddHandler(DispatcherType.ObjectEvent, sub_100D5950, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 186)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dd9f8)]
  public static readonly ConditionSpec SpellRepelVerminHit = ConditionSpec.Create("sp-Repel Vermin Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellRepelVerminHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellRepelVerminHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RepelVerminOnAdd)
.AddHandler(DispatcherType.ObjectEvent, sub_100D5950, 187)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 187, SpellRepelVermin)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 187)
                    .Build();


[TempleDllLocation(0x102ddae0)]
  public static readonly ConditionSpec SpellResistance = ConditionSpec.Create("sp-Resistance", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 188)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 188)
.AddHandler(DispatcherType.SaveThrowLevel, SavingThrowSpellResistanceBonusCallback, 1, 199)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 188, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 188)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 60, 0)
                    .Build();


[TempleDllLocation(0x102ddbc8)]
  public static readonly ConditionSpec SpellResistElements = ConditionSpec.Create("sp-Resist Elements", 4)
.AddHandler(DispatcherType.ConditionAddPre, sub_100C77D0, SpellResistElements)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 189)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Resist_Elements, sub_100C7820)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellRestoration = ConditionSpec.Create("sp-Restoration", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpRestorationOnConditionAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102ddd90)]
  public static readonly ConditionSpec SpellResurrection = ConditionSpec.Create("sp-Resurrection", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, RemoveSpellOnAdd)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dde10)]
  public static readonly ConditionSpec SpellRighteousMight = ConditionSpec.Create("sp-Righteous Might", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellRighteousMight)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 191)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSanctuary = ConditionSpec.Create("sp-Sanctuary", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellSanctuary)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 192)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_CanBeAffected_PerformAction, SanctuaryCanBeAffectedPerform, SpellSanctuary)
.AddQueryHandler(D20DispatcherKey.QUE_CanBeAffected_ActionFrame, CanBeAffectedActionFrame_Sanctuary, SpellSanctuary)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSanctuary)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSanctuarySaveSucceeded = ConditionSpec.Create("sp-Sanctuary Save Succeeded", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSanctuarySaveSucceeded)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 193, SpellSanctuary)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 193)
                    .Build();


[TempleDllLocation(0x102de1b0)]
  public static readonly ConditionSpec SpellSanctuarySaveFailed = ConditionSpec.Create("sp-Sanctuary Save Failed", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSanctuarySaveFailed)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 194, SpellSanctuary)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 194)
                    .Build();


[TempleDllLocation(0x102de258)]
  public static readonly ConditionSpec SpellSeeInvisibility = ConditionSpec.Create("sp-See Invisibility", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 195)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Invisible, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellShield = ConditionSpec.Create("sp-Shield", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 196)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellShield)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellShieldofFaith = ConditionSpec.Create("sp-Shield of Faith", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 197)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 197)
.AddHandler(DispatcherType.GetAC, SpellArmorBonus, 11, 200)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 197, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 197)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 64, 0)
                    .Build();


[TempleDllLocation(0x102de588)]
  public static readonly ConditionSpec SpellShillelagh = ConditionSpec.Create("sp-Shillelagh", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 198)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 198)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 198, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 198)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 66, 0)
                    .Build();


[TempleDllLocation(0x102de658)]
  public static readonly ConditionSpec SpellShockingGrasp = ConditionSpec.Create("sp-Shocking Grasp", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 199)
.AddHandler(DispatcherType.ConditionAdd, TouchAttackOnAdd)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellShout = ConditionSpec.Create("sp-Shout", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveDeafness)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Deafened, true)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, DeafnessSpellFailure)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSilence = ConditionSpec.Create("sp-Silence", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 201)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSilenceHit = ConditionSpec.Create("sp-Silence Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSilenceHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, SilenceSpellFailure)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSilenceHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSleep = ConditionSpec.Create("sp-Sleep", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 203)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Unconscious, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSleetStorm = ConditionSpec.Create("sp-Sleet Storm", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 204)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellSleetStorm)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 204)
.AddHandler(DispatcherType.ObjectEvent, SleetStormAoE, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 204)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102ded00)]
  public static readonly ConditionSpec SpellSleetStormHit = ConditionSpec.Create("sp-Sleet Storm Hit", 4)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSleetStormHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSleetStormHit)
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
  public static readonly ConditionSpec SpellSlow = ConditionSpec.Create("sp-Slow", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellHaste)
.SetUnique()
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 206)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSoftenEarthandStone = ConditionSpec.Create("sp-Soften Earth and Stone", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 207)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellSoftenEarthAndStone)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 207)
.AddHandler(DispatcherType.ObjectEvent, sub_100D5F60, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 207)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102def08)]
  public static readonly ConditionSpec SpellSoftenEarthandStoneHit = ConditionSpec.Create("sp-Soften Earth and Stone Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSoftenEarthandStoneHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSoftenEarthandStoneHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.GetMoveSpeedBase, sub_100CABC0, 0, 0)
.AddHandler(DispatcherType.ObjectEvent, sub_100D5F60, 208)
.AddHandler(DispatcherType.ConditionAdd, sub_100CA7A0)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 208, SpellSoftenEarthandStone)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 208)
                    .Build();


[TempleDllLocation(0x102df000)]
  public static readonly ConditionSpec SpellSoftenEarthandStoneHitSaveFailed = ConditionSpec.Create("sp-Soften Earth and Stone Hit Save Failed", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSoftenEarthandStoneHitSaveFailed)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSoftenEarthandStoneHitSaveFailed)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSolidFog = ConditionSpec.Create("sp-Solid Fog", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 210)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellSolidFog)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 210)
.AddHandler(DispatcherType.ObjectEvent, SolidFogAoEEvent, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 210)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102df208)]
  public static readonly ConditionSpec SpellSolidFogHit = ConditionSpec.Create("sp-Solid Fog Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSolidFogHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSolidFogHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSoundBurst = ConditionSpec.Create("sp-Sound Burst", 3)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSpellResistance = ConditionSpec.Create("sp-Spell Resistance", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 213)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddHandler(DispatcherType.ConditionAdd, sub_100CFD50)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 213)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, sub_100CA9F0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSpikeGrowth = ConditionSpec.Create("sp-Spike Growth", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 214)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSpikeGrowthHit = ConditionSpec.Create("sp-Spike Growth Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeGrowthHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSpikeGrowthHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSpikeGrowthDamage = ConditionSpec.Create("sp-Spike Growth Damage", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeGrowthDamage)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSpikeGrowthDamage)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.GetMoveSpeed, sub_100CABE0, SpellSpikeGrowthDamage)
.AddHandler(DispatcherType.ObjectEvent, SpikeGrowthHitTrigger, 216)
.AddSignalHandler(D20DispatcherKey.SIG_HealSkill, SpikeGrowthDamageHealingReceived)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, SpikeGrowthDamageHealingReceived)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 216)
                    .Build();


[TempleDllLocation(0x102df900)]
  public static readonly ConditionSpec SpellSpikeStones = ConditionSpec.Create("sp-Spike Stones", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 217)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSpikeStonesHit = ConditionSpec.Create("sp-Spike Stones Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeStonesHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSpikeStonesHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSpikeStonesDamage = ConditionSpec.Create("sp-Spike Stones Damage", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellSpikeStonesDamage)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSpikeStonesDamage)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.GetMoveSpeed, sub_100CABE0, SpellSpikeStonesDamage)
.AddHandler(DispatcherType.ObjectEvent, SpikeStonesHitTrigger, 219)
.AddSignalHandler(D20DispatcherKey.SIG_HealSkill, SpikeGrowthDamageHealingReceived)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_Cast, SpikeGrowthDamageHealingReceived)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 219)
                    .Build();


[TempleDllLocation(0x102dfc00)]
  public static readonly ConditionSpec SpellSpiritualWeapon = ConditionSpec.Create("sp-Spiritual Weapon", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 220)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddHandler(DispatcherType.ConditionAdd, SpiritualWeaponBeginSpellDismiss)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 220)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellStinkingCloud = ConditionSpec.Create("sp-Stinking Cloud", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 221)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellStinkingCloud)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 221)
.AddHandler(DispatcherType.ObjectEvent, AoeObjEventStinkingCloud, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 221)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102dfe30)]
  public static readonly ConditionSpec SpellStinkingCloudHit = ConditionSpec.Create("sp-Stinking Cloud Hit", 4)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellStinkingCloudHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellStinkingCloudHit)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellStinkingCloudHitPre = ConditionSpec.Create("sp-Stinking Cloud Hit Pre", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellStinkingCloudHitPre)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellStinkingCloudHitPre)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, StinkingCloudPreBeginRound, 223)
.AddHandler(DispatcherType.ObjectEvent, AoeObjEventStinkingCloud, 223)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 223, SpellStinkingCloud)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 223)
                    .Build();


[TempleDllLocation(0x102e0060)]
  public static readonly ConditionSpec SpellStoneskin = ConditionSpec.Create("sp-Stoneskin", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 224)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSuggestion = ConditionSpec.Create("sp-Suggestion", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellSuggestion)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 225)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSuggestion)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_AIControlled, SuggestionIsAiControlledQuery)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, sub_100CB1A0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellSummonSwarm = ConditionSpec.Create("sp-Summon Swarm", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 226)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellSummonSwarm)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellTashasHideousLaughter = ConditionSpec.Create("sp-Tashas Hideous Laughter", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 227)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellTreeShape = ConditionSpec.Create("sp-Tree Shape", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 228)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellTrueSeeing = ConditionSpec.Create("sp-True Seeing", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 229)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_See_Invisible, true)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Has_True_Seeing, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 229)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 229, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 229)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 69, 0)
                    .Build();


[TempleDllLocation(0x102e0788)]
  public static readonly ConditionSpec SpellTrueStrike = ConditionSpec.Create("sp-True Strike", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 230)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellTrueStrike)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 230)
.AddHandler(DispatcherType.ToHitBonus2, TrueStrikeAttackBonus, 20, 251)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 230, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 230)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 70, 0)
                    .Build();


[TempleDllLocation(0x102e0880)]
  public static readonly ConditionSpec SpellUnholyBlight = ConditionSpec.Create("sp-Unholy Blight", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveCurse)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellUnholyBlight)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellVampiricTouch = ConditionSpec.Create("sp-Vampiric Touch", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 232)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_HoldingCharge, CommonConditionCallbacks.D20QueryTrueGetCondArg0)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellVirtue = ConditionSpec.Create("sp-Virtue", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 233)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
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
  public static readonly ConditionSpec SpellWeb = ConditionSpec.Create("sp-Web", 5)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 234)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellWebOn = ConditionSpec.Create("sp-Web On", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellWebOn)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, WebSpellInterrupted)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellWebOn)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellWebOff = ConditionSpec.Create("sp-Web Off", 4)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellWebOff)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellWebOff)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellWindWall = ConditionSpec.Create("sp-Wind Wall", 4)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 237)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, BeginSpellWindWall)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 237)
.AddHandler(DispatcherType.ObjectEvent, WindWallAoeEvent, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, AoESpellRemove, 237)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102e1158)]
  public static readonly ConditionSpec SpellWindWallHit = ConditionSpec.Create("sp-Wind Wall Hit", 3)
.AddHandler(DispatcherType.ConditionAddPre, AoESpellPreAddCheck, SpellWindWallHit)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellWindWallHit)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 239)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 64, 0)
.SetQueryResult(D20DispatcherKey.QUE_ExperienceExempt, true)
                    .Build();


[TempleDllLocation(0x102e1350)]
  public static readonly ConditionSpec SpellFrogTongue = ConditionSpec.Create("sp-Frog Tongue", 3)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Grappling, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFrogTongue)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, FrogTongueOnAdd)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 240)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_mod, 240)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, sub_100DE090, 240)
.AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, sub_100DC800, 240)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 240, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 240)
                    .Build();


[TempleDllLocation(0x102e15c0)]
  public static readonly ConditionSpec SpellFrogTongueGrappled = ConditionSpec.Create("sp-Frog Tongue Grappled", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellFrogTongueSwallowed)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Grappling, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFrogTongueGrappled)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, true)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, OnSpellEndRemoveMod, 241, SpellFrogTongueGrappled)
.AddSignalHandler(D20DispatcherKey.SIG_BreakFree, FrogTongue_breakfree_callback, 241)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, sub_100DC800, 241)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 241, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 241)
.RemoveOnSignal(D20DispatcherKey.SIG_Combat_End)
                    .Build();


[TempleDllLocation(0x102e1460)]
  public static readonly ConditionSpec SpellFrogTongueSwallowed = ConditionSpec.Create("sp-Frog Tongue Swallowed", 3)
.AddHandler(DispatcherType.ConditionAdd, FrogTongueSwallowedOnAdd)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFrogTongueSwallowed)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, false)
.AddHandler(DispatcherType.BeginRound, FrongTongueSwallowedDamage, 242)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, sub_100DE090, 242)
.AddSignalHandler(D20DispatcherKey.SIG_BreakFree, FrogTongue_breakfree_callback, 242)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_spell, 242, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_mod, 242)
.AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, sub_100DC800, 242)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 242, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 242)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 242)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                    .Build();


[TempleDllLocation(0x102e16e0)]
  public static readonly ConditionSpec SpellFrogTongueSwallowing = ConditionSpec.Create("sp-Frog Tongue Swallowing", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellFrogTongueSwallowing)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.GetMoveSpeedBase, CommonConditionCallbacks.GrappledMoveSpeed, 0, 232)
.AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.GrappledMoveSpeed, 0, 232)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, sub_100DE090, 243)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 243, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 243)
.AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, sub_100DC800, 243)
.AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, Spell_remove_mod, 243)
.AddSignalHandler(D20DispatcherKey.SIG_Combat_End, ExpireSpell, 1)
                    .Build();


[TempleDllLocation(0x102e1800)]
  public static readonly ConditionSpec SpellVrockScreech = ConditionSpec.Create("sp-Vrock Screech", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellVrockSpores = ConditionSpec.Create("sp-Vrock Spores", 3)
.AddHandler(DispatcherType.ConditionAddPre, SpellRemovedBy, SpellHeal)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellBless)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellNeutralizePoison)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellRemoveDisease)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 245)
.AddHandler(DispatcherType.TurnBasedStatusInit, VrockSporesDamage, 245)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 245, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 245)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 55, 0)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 130, 0)
                    .Build();


[TempleDllLocation(0x102e1960)]
  public static readonly ConditionSpec SpellRingofFreedomofMovement = ConditionSpec.Create("sp-Ring of Freedom of Movement", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 246)
.AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Magical_Item_Deactivate, FreedomOfMovementRingDeactivate)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 246, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 246)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 38, 0)
                    .Build();


[TempleDllLocation(0x102e1a48)]
  public static readonly ConditionSpec SpellPotionofEnlarge = ConditionSpec.Create("sp-Potion of Enlarge", 3)
.Prevents(SpellEnlarge)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellReduce)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellEnlarge)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 247)
.AddHandler(DispatcherType.AbilityScoreLevel, EnlargeStatLevelGet, 0, 244)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 247, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 247)
.AddHandler(DispatcherType.EffectTooltip, EffectTooltip_Duration_Callback, 33, 0)
                    .Build();


[TempleDllLocation(0x102e1b40)]
  public static readonly ConditionSpec SpellPotionofHaste = ConditionSpec.Create("sp-Potion of Haste", 3)
.AddHandler(DispatcherType.ConditionAddPre, RemoveSpellWhenPreAddThis, SpellSlow)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 248)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellDustofDisappearance = ConditionSpec.Create("sp-Dust of Disappearance", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 250)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, HasConditionReturnSpellId, SpellInvisibility)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, SpellInvisibilityBegin)
.AddHandler(DispatcherType.BeginRound, SpellModCountdownRemove, 250)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_spell, 250, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Killed, Spell_remove_mod, 250)
                    .Build();


[TempleDllLocation(0x102e1ea0)]
  public static readonly ConditionSpec SpellPotionofcharisma = ConditionSpec.Create("sp-Potion of charisma", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, sub_100D2AC0)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
.AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102e1f48)]
  public static readonly ConditionSpec SpellPotionofglibness = ConditionSpec.Create("sp-Potion of glibness", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
.AddSkillLevelHandler(SkillId.bluff, PotionOfGlibnessSkillLevel)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
.AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102e1ff0)]
  public static readonly ConditionSpec SpellPotionofhiding = ConditionSpec.Create("sp-Potion of hiding", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
.AddSkillLevelHandler(SkillId.hide, PotionOfHidingSneaking)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
.AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102e2098)]
  public static readonly ConditionSpec SpellPotionofsneaking = ConditionSpec.Create("sp-Potion of sneaking", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
.AddSkillLevelHandler(SkillId.move_silently, PotionOfHidingSneaking)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
.AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102e2140)]
  public static readonly ConditionSpec SpellPotionofheroism = ConditionSpec.Create("sp-Potion of heroism", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellPotionofsuperheroism = ConditionSpec.Create("sp-Potion of super-heroism", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellPotionofprotectionfromenergy = ConditionSpec.Create("sp-Potion of protection from energy", 4)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100D2A90)
.AddHandler(DispatcherType.TakingDamage2, PotionOfProtectionFromEnergyDamageCallback)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 1)
.AddHandler(DispatcherType.ConditionRemove, sub_100D2C80)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Prepare, d20_mods_spells__teleport_prepare)
.AddSignalHandler(D20DispatcherKey.SIG_Teleport_Reconnect, DummyCallbacks.EmptyFunction)
                    .Build();


[TempleDllLocation(0x102e2388)]
  public static readonly ConditionSpec SpellProtectionFromMonster = ConditionSpec.Create("sp-Protection From Monster", 3)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 252)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
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
  public static readonly ConditionSpec SpellProtectionFromMagic = ConditionSpec.Create("sp-Protection From Magic", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Active, D20QHasSpellEffectActive)
.AddHandler(DispatcherType.ConditionAdd, sub_100D2E30)
.AddHandler(DispatcherType.SpellResistanceMod, SpellResistanceMod_ProtFromMagic_Callback, 5048)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, sub_100CA9F0)
.AddHandler(DispatcherType.Tooltip, SpellResistanceTooltipCallback, 5048)
.AddHandler(DispatcherType.DispelCheck, DispelCheck, 253)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPELL)
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


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
{
SpellBlindness,
SpellProtectionFromArrows,
SpellPotionofEnlarge,
SpellClairaudienceClairvoyance,
SpellDoom,
SpellNeutralizePoison,
SpellEmotionRage,
SpellReduce,
SpellDominateAnimal,
SpellSuggestion,
SpellGlitterdust,
SpellGrease,
SpellCommand,
SpellGoodberryTally,
SpellPotionofHaste,
SpellFireShield,
SpellGustofWind,
SpellVirtue,
SpellBane,
SpellDimensionalAnchor,
SpellSummoned,
SpellCharmMonster,
SpellRepelVermin,
SpellEndureElements,
SpellUnholyBlight,
SpellAid,
SpellVrockSpores,
SpellDeathKnell,
SpellRage,
SpellSanctuarySaveSucceeded,
SpellTrueStrike,
SpellRayofEnfeeblement,
SpellMagicFang,
SpellEmotionDespair,
SpellSpikeStones,
SpellControlPlants,
SpellResistElements,
SpellDispelGood,
SpellInvisibilityPurgeHit,
SpellPotionofsuperheroism,
SpellDetectChaos,
SpellHeatMetal,
SpellFalseLife,
SpellSpikeGrowthDamage,
SpellDeafness,
SpellControlPlantsEntanglePre,
SpellDarkvision,
SpellDesecrate,
SpellDominatePerson,
SpellMelfsAcidArrow,
SpellGreaterMagicWeapon,
SpellMagicWeapon,
SpellCalmEmotions,
SpellGlitterdustBlindness,
SpellSoftenEarthandStone,
SpellSleetStorm,
SpellGlibness,
SpellEntangle,
SpellDispelLaw,
SpellCloudkillDamage,
SpellRemoveFear,
SpellPotionofprotectionfromenergy,
SpellDisplacement,
SpellAnimalTrance,
SpellMinorGlobeofInvulnerabilityHit,
SpellFreedomofMovement,
SpellBreakEnchantment,
SpellGhoulTouchParalyzed,
SpellWeb,
SpellRaiseDead,
SpellDetectLaw,
SpellReduceAnimal,
SpellSlipperyMind,
SpellSleep,
SpellSpikeGrowth,
SpellFogCloudHit,
SpellColorSprayBlind,
SpellSanctuarySaveFailed,
SpellDelayPoison,
SpellOwlsWisdom,
SpellShield,
SpellExpeditiousRetreat,
SpellWebOn,
SpellInvisibilitySphere,
SpellMindFogHit,
SpellVrockScreech,
SpellCharmPerson,
SpellBestowCurseAbility,
SpellInvisibility,
SpellGaseousForm,
SpellWindWall,
SpellRemoveDisease,
SpellWindWallHit,
SpellEntangleOn,
SpellFear,
SpellFrogTongueGrappled,
SpellHoldAnimal,
SpellLesserRestoration,
SpellSeeInvisibility,
SpellShieldofFaith,
SpellDivineFavor,
SpellEndurance,
SpellRemoveDeafness,
SpellSpiritualWeapon,
SpellSoftenEarthandStoneHitSaveFailed,
SpellSoundBurst,
SpellFrogTongueSwallowing,
SpellSpellResistance,
SpellEntropicShield,
SpellConsecrate,
SpellMagicMissile,
SpellDustofDisappearance,
SpellResistance,
SpellHoldPortal,
SpellDispelEvil,
SpellColorSprayStun,
SpellCallLightning,
SpellRemoveParalysis,
SpellSolidFog,
SpellMagicVestment,
SpellFeeblemind,
SpellDesecrateHitUndead,
SpellHarm,
SpellCharmPersonorAnimal,
SpellAnimalFriendship,
SpellGreaseHit,
SpellDetectGood,
SpellDetectMagic,
SpellDetectEvil,
SpellMindFog,
SpellMagicStone,
SpellBlur,
SpellObscuringMistHit,
SpellChaosHammer,
SpellFrogTongueSwallowed,
SpellMirrorImage,
SpellEaglesSplendor,
SpellHoldMonster,
SpellNegativeEnergyProtection,
SpellSoftenEarthandStoneHit,
SpellHolySmite,
SpellCauseFear,
SpellControlPlantsDisentangle,
SpellShockingGrasp,
SpellEmotionHate,
SpellChillMetal,
SpellConsecrateHitUndead,
SpellGhoulTouchStench,
SpellLongstrider,
SpellFlare,
SpellMagicCircleOutward,
SpellResurrection,
SpellFindTraps,
SpellChillTouch,
SpellBestowCurseActions,
SpellHoldTouchSpell,
SpellFaerieFire,
SpellFrogTongue,
SpellStinkingCloud,
SpellSpikeStonesHit,
SpellDispelEarth,
SpellPotionofheroism,
SpellStinkingCloudHitPre,
SpellEmotionHope,
SpellStinkingCloudHit,
SpellStoneskin,
SpellHoldPerson,
SpellHaltUndead,
SpellShout,
SpellProduceFlame,
SpellAnimateDead,
SpellMagicCircleInward,
SpellTrueSeeing,
SpellDispelFire,
SpellPrayer,
SpellDesecrateHit,
SpellInvisibilitySphereHit,
SpellDetectSecretDoors,
SpellGhoulTouch,
SpellImprovedInvisibility,
SpellControlPlantsCharm,
SpellConfusion,
SpellCloudkill,
SpellMageArmor,
SpellGreaterHeroism,
SpellMeldIntoStone,
SpellGuidance,
SpellBlink,
SpellDiscernLies,
SpellInvisibilitytoAnimals,
SpellEntangleOff,
SpellInvisibilityPurge,
SpellHeroism,
SpellIceStormHit,
SpellBarkskin,
SpellProtectionFromMonster,
SpellFogCloud,
SpellDispelAir,
SpellProtectionFromAlignment,
SpellPotionofsneaking,
SpellSummonSwarm,
SpellCallLightningStorm,
SpellCatsGrace,
SpellMinorGlobeofInvulnerability,
SpellSleetStormHit,
SpellTreeShape,
SpellDispelMagic,
SpellRingofFreedomofMovement,
SpellRemoveBlindness,
SpellRepelVerminHit,
SpellControlPlantsEntangle,
SpellProtectionFromElements,
SpellHaste,
SpellSlow,
SpellBullsStrength,
SpellMordenkainensFaithfulHound,
SpellOrdersWrath,
SpellShillelagh,
SpellSpikeGrowthHit,
SpellPotionofglibness,
SpellKeenEdge,
SpellGreaterMagicFang,
SpellTashasHideousLaughter,
SpellSolidFogHit,
SpellSilenceHit,
SpellRestoration,
SpellHeal,
SpellDispelChaos,
SpellGoodberry,
SpellEmotionFear,
SpellVampiricTouch,
SpellPotionofhiding,
SpellObscuringMist,
SpellOtilukesResilientSphere,
SpellRemoveCurse,
SpellSilence,
SpellEmotionFriendship,
SpellAnimalGrowth,
SpellProtectionFromMagic,
SpellConsecrateHit,
SpellBestowCurseRolls,
SpellControlPlantsTracking,
SpellPotionofcharisma,
SpellCalmAnimals,
SpellEnlarge,
SpellColorSprayUnconscious,
SpellGhoulTouchStenchHit,
SpellConcentrating,
SpellDetectUndead,
SpellSpikeStonesDamage,
SpellIceStorm,
SpellSanctuary,
SpellDivinePower,
SpellFoxsCunning,
SpellBless,
SpellRighteousMight,
SpellDispelWater,
SpellInvisibilitytoUndead,
SpellDeathWard,
SpellWebOff,
SpellDaze,
};

}
}
/*

RemoveSpellCharmPersonOrAnimal @ 0x100d1670 = 1
RemoveAnimalTrance @ 0x100d0ca0 = 1
sub_100D20C0 @ 0x100d20c0 = 1
ObjListFree @ 0x1001f2c0 = 1
sub_100D1E80 @ 0x100d1e80 = 1
RemoveAnimalFriendship @ 0x100d0a40 = 1
OtilukesResilientSphereEnd @ 0x100d1fe0 = 1
SpiritualWeaponRemover @ 0x100d2990 = 1
RemoveSpellCharmPerson @ 0x100d13f0 = 1
LevelPacketDealloc @ 0x100f4780 = 1
RemoveSpellCalmAnimals @ 0x100d0db0 = 1
sub_100D1AC0 @ 0x100d1ac0 = 1
InsertToPartsysIdList @ 0x10075510 = 1
D20ActionBreaksConcentration @ 0x1008a180 = 1
IsPc @ 0x1007e570 = 1
NpcAiListAppendType1 @ 0x1005cd30 = 1
LevelPacketInit @ 0x100f5520 = 1
sub_100D21F0 @ 0x100d21f0 = 1
RemoveSpellSuggestion @ 0x100d2460 = 1
RemoveSpellSanctuary @ 0x100d2260 = 1
sub_100D1A50 @ 0x100d1a50 = 1
sub_100D19A0 @ 0x100d19a0 = 1
RemoveFromObjectEventTable @ 0x10044a10 = 1
sub_100D1DD0 @ 0x100d1dd0 = 1
KnowsArcaneSpells @ 0x100760e0 = 1
AddAttackPowerType @ 0x100e0520 = 1
FindInObjlist @ 0x10075600 = 1
MordenkainensFaithfulHoundEnder @ 0x100d1f60 = 1
sub_100D18F0 @ 0x100d18f0 = 1
NextLevel @ 0x10080300 = 1
RemoveSpellCharmMonster @ 0x100d1170 = 1
ProtectionFromArrowsEnd @ 0x100d2050 = 1
RemoveSpellCalmEmotions @ 0x100d1030 = 1
GetLevelPacket @ 0x100f5140 = 1
sub_100D26A0 @ 0x100d26a0 = 1
sub_100D1D60 @ 0x100d1d60 = 1
DispIoAttackBonusDebug @ 0x1004d9f0 = 1
CondNodeSetArg @ 0x100e1ad0 = 1
sub_10020A60 @ 0x10020a60 = 2
ActSeqCurGetCurrentAction @ 0x1008a090 = 2
IsActionOffensive @ 0x1008acc0 = 2
sub_100DA9BC @ 0x100da9bc = 2
Get_First_Byte @ 0x101ecc80 = 2
sub_100D2A10 @ 0x100d2a10 = 2
SpellCondStructPtrArray @ 0x102e2600 = 2
GetDamageTypeOverallDamage @ 0x100e1210 = 2
Float_Combat_mes_with_extra_Strings @ 0x100b4bf0 = 2
bonMesLinePrintf @ 0x100e63d0 = 2
CondNodeGetArgPtr @ 0x100e1af0 = 3
CondNodeGetArg @ 0x100e1ab0 = 3
pSpellEntry_Descriptor_Strings @ 0x102bfa90 = 3
sub_100D26F0 @ 0x100d26f0 = 3
EncodeSpellData @ 0x10075280 = 3
RemoveInvisibility @ 0x100d1b00 = 3
EffectTooltipAppend @ 0x100f4680 = 3
sub_100D3020 @ 0x100d3020 = 4
AddItemConditionToWielder @ 0x100d2f30 = 4
End_Particles_And_Send_Spell_End_Signal @ 0x100d09d0 = 5
nullsub_1 @ 0x100027f0 = 13
GetPackedDiceType @ 0x10038c40 = 17
DiceRoller @ 0x10038b60 = 18
EndSpellParticlesForTargetObj @ 0x100d2930 = 23
SpellPktTriggerAoeHitScript @ 0x100c37d0 = 24
GetPackedDiceNumDice @ 0x10038c30 = 25
GetPackedDiceBonus @ 0x10038c90 = 35
*/