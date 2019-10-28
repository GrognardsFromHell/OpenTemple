using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Particles.Instances;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class StatusEffects
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x102e4cb0)]
        public static readonly ConditionSpec Unconscious = ConditionSpec.Create("Unconscious", 0)
            .SetUnique()
            .RemovedBy(Dying)
            .RemovedBy(Disabled)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, BonusCap0Add)
            .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_CoupDeGrace, true)
            .SetQueryResult(D20DispatcherKey.QUE_Unconscious, true)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, UnconsciousHpChanged)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 38, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, DyingOnCombatEnd)
            .AddHandler(DispatcherType.ConditionRemove2, UnconsciousConditionRemoved2_StandUp)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 168)
            .Build();


        [TempleDllLocation(0x102e4b40)]
        public static readonly ConditionSpec Dying = ConditionSpec.Create("Dying", 0)
            .SetUnique()
            .Prevents(Disabled)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddHandler(DispatcherType.Initiative, DyingBleedingOut)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, BonusCap0Add)
            .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_CoupDeGrace, true)
            .SetQueryResult(D20DispatcherKey.QUE_Unconscious, true)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, DyingHpChange)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddSignalHandler(D20DispatcherKey.SIG_HealSkill, DyingHealSkillUsed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 27, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, DyingOnCombatEnd)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 169)
            .Build();


        [TempleDllLocation(0x102e4a70)]
        public static readonly ConditionSpec Disabled = ConditionSpec.Create("Disabled", 0)
            .SetUnique()
            .RemovedBy(Dying)
            .AddHandler(DispatcherType.TurnBasedStatusInit,
                CommonConditionCallbacks.turnBasedStatusInitSingleActionOnly)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, ConditionDisabledHpChange)
            .AddSignalHandler(D20DispatcherKey.SIG_Sequence, DisabledSequence)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 39, 0)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 170)
            .Build();


        [TempleDllLocation(0x102e4e20)]
        public static readonly ConditionSpec Dead = ConditionSpec.Create("Dead", 0)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .SetQueryResult(D20DispatcherKey.QUE_Dead, true)
            .RemoveOnSignal(D20DispatcherKey.SIG_Resurrection)
            .Build();


        [TempleDllLocation(0x102e4e90)]
        public static readonly ConditionSpec StunningFistAttacking = ConditionSpec.Create("StunningFist_Attacking", 1)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage2, StunningFistDamage)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 5047, 0)
            .Build();


        [TempleDllLocation(0x102e4f10)]
        public static readonly ConditionSpec AIControlled = ConditionSpec.Create("AI Controlled", 4)
            .SupportHasConditionQuery()
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_12,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_12)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_AIControlled, QueryIsAiControlled)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, AiControlledKilled)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .RemoveOnSignal(D20DispatcherKey.SIG_Remove_AI_Controlled)
            .AddHandler(DispatcherType.Tooltip, TooltipSimpleCallback, 190)
            .Build();


        [TempleDllLocation(0x102e4ff8)]
        public static readonly ConditionSpec Dominate = ConditionSpec.Create("Dominate", 5)
            .SupportHasConditionQuery()
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_12,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_12)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, DominateIsCharmed)
            .AddHandler(DispatcherType.ConditionAdd, DominateConditionAdd)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, DominateCritterKilled)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, TooltipSimpleCallback, 189)
            .Build();


        [TempleDllLocation(0x102e50c8)]
        public static readonly ConditionSpec Held = ConditionSpec.Create("Held", 3)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_12)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 5, 1)
            .AddHandler(DispatcherType.TurnBasedStatusInit, ParalyzedTurnBasedStatusInit)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 5, SpellEffects.SpellHoldPerson)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .AddQueryHandler(D20DispatcherKey.QUE_Helpless, QueryRetTrueIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_CannotCast, QueryRetTrueIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, QueryRetFalseIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_CoupDeGrace, QueryRetTrueIfNoFreedomOfMovement)
            .AddHandler(DispatcherType.Tooltip, TooltipSimpleCallback, 40)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 124)
            .Build();


        [TempleDllLocation(0x102e52e8)]
        public static readonly ConditionSpec Invisible = ConditionSpec.Create("Invisible", 3)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 6, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 6, SpellEffects.SpellInvisibility)
            .AddHandler(DispatcherType.ConditionAdd, InvisibleOnAdd_SetFade128)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Invisible, true)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOIncurs, InvisibilityAooIncurs)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOWillTake, InvisibilityAooWilltake)
            .AddHandler(DispatcherType.ToHitBonus2, InvisibleToHitBonus)
            .AddHandler(DispatcherType.AcModifyByAttacker, InvisibilityAcBonus2Cap)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 41, 0)
            .AddHandler(DispatcherType.GetDefenderConcealmentMissChance, InvisibleDefenderConcealmentMissChance, 50)
            .AddSignalHandler(D20DispatcherKey.SIG_Magical_Item_Deactivate, InvisibleOnDeactivate)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .Build();


        [TempleDllLocation(0x102e53f8)]
        public static readonly ConditionSpec Sleeping = ConditionSpec.Create("Sleeping", 3)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 7, 1)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 7, SpellEffects.SpellSleep)
            .AddSignalHandler(D20DispatcherKey.SIG_Aid_Another, D20ModCountdownEndHandler, 0)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .SetQueryResult(D20DispatcherKey.QUE_CoupDeGrace, true)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 47, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 135)
            .Build();


        [TempleDllLocation(0x102e5518)]
        public static readonly ConditionSpec HoldingCharge = ConditionSpec.Create("HoldingCharge", 0)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Sequence, HoldingChargeSequence)
            .SetQueryResult(D20DispatcherKey.QUE_HoldingCharge, true)
            .AddSignalHandler(D20DispatcherKey.SIG_TouchAttack, sub_100E9AC0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 2)
            .Build();


        [TempleDllLocation(0x102e5698)]
        public static readonly ConditionSpec Surprised = ConditionSpec.Create("Surprised", 0)
            .RemovedBy(SurpriseRound)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 49, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 171)
            .Build();


        [TempleDllLocation(0x102e5718)]
        public static readonly ConditionSpec SurpriseRound = ConditionSpec.Create("SurpriseRound", 0)
            .Prevents(Surprised)
            .AddHandler(DispatcherType.TurnBasedStatusInit,
                CommonConditionCallbacks.turnBasedStatusInitSingleActionOnly)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .Build();


        [TempleDllLocation(0x102e55b0)]
        public static readonly ConditionSpec Flatfooted = ConditionSpec.Create("Flatfooted", 0)
            .SetUnique()
            .RemoveOnSignal(D20DispatcherKey.SIG_BeginTurn)
            .RemoveOnSignal(D20DispatcherKey.SIG_Combat_End)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, FlatfootedAooPossible)
            .AddHandler(DispatcherType.GetAC, FlatfootedAcBonus)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_Flatfooted, true)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 48, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 173)
            .Build();


        [TempleDllLocation(0x102e57b8)]
        public static readonly ConditionSpec TemporaryHitPoints = ConditionSpec.Create("Temporary_Hit_Points", 3)
            .SetQueryResult(D20DispatcherKey.QUE_Has_Temporary_Hit_Points, true)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 12, 1)
            .AddHandler(DispatcherType.TakingDamage2, Temporary_Hit_Points_Disp15h_Taking_Damage, 12)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 12, SpellEffects.SpellAid)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TempHPTooltipCallback, 74)
            .Build();


        [TempleDllLocation(0x102e5770)]
        public static readonly ConditionSpec SpellInterrupted = ConditionSpec.Create("Spell Interrupted", 3)
            .AddSignalHandler(D20DispatcherKey.SIG_Anim_CastConjureEnd, SpellInterruptedAnimCastConjureEnd)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .Build();


        [TempleDllLocation(0x102e5860)]
        public static readonly ConditionSpec Damaged = ConditionSpec.Create("Damaged", 1)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.ConditionAddPre, CondPreventIncArg, Damaged)
            .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, DamagedSpellInterrupted)
            .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, CommonConditionCallbacks.conditionRemoveCallback)
            .RemoveOnSignal(D20DispatcherKey.SIG_Combat_Critter_Moved)
            .Build();


        [TempleDllLocation(0x102e58e0)]
        public static readonly ConditionSpec Cursed = ConditionSpec.Create("Cursed", 3)
            .AddHandler(DispatcherType.ConditionAddPre, AfflictedPreadd_100E9A70, SpellEffects.SpellRemoveCurse)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 13, 1)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Cursed, true)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 51, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 106)
            .Build();


        [TempleDllLocation(0x102e5988)]
        public static readonly ConditionSpec Afraid = ConditionSpec.Create("Afraid", 3)
            .AddHandler(DispatcherType.ConditionAddPre, AfflictedPreadd_100E9A70, SpellEffects.SpellRemoveFear)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 14, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 14, SpellEffects.SpellFear)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Afraid, true)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 52, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 116)
            .Build();


        [TempleDllLocation(0x102e5a70)]
        public static readonly ConditionSpec Diseased = ConditionSpec.Create("Diseased", 3)
            .AddHandler(DispatcherType.ConditionAddPre, AfflictedPreadd_100E9A70, SpellEffects.SpellRemoveDisease)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 15, 1)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Diseased, true)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Diseased, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 53, 0)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, DiseasedApplyStrengthMalus,
                53)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltipDiseased, 110)
            .Build();


        [TempleDllLocation(0x102e5b40)]
        public static readonly ConditionSpec SpellPoisoned = ConditionSpec.Create("Spell-Poisoned", 3)
            .RemovedBy(SpellEffects.SpellHeal)
            .AddHandler(DispatcherType.ConditionAddPre, AfflictedPreadd_100E9A70, SpellEffects.SpellNeutralizePoison)
            .AddHandler(DispatcherType.ConditionAddPre, sub_100EA020)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 16, 1)
            .AddHandler(DispatcherType.ConditionAdd, RemoveIfImmunePoison)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Poisoned, true)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 55, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 130)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltipPoison, 130)
            .Build();


        [TempleDllLocation(0x102e5cf8)]
        public static readonly ConditionSpec TotalDefense = ConditionSpec.Create("Total Defense", 0)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .SetUnique()
            .AddHandler(DispatcherType.GetAC, TotalDefenseCallback)
            .SetQueryResult(D20DispatcherKey.QUE_FightingDefensively, true)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 77)
            .Build();


        [TempleDllLocation(0x102e5d78)]
        public static readonly ConditionSpec Charging = ConditionSpec.Create("Charging", 1)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.GetAC, ChargingAcPenalty)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100E9D30)
            .Build();


        [TempleDllLocation(0x102e5df8)]
        public static readonly ConditionSpec Prone = ConditionSpec.Create("Prone", 0)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.GetAC, ProneApplyACModifier)
            .AddHandler(DispatcherType.ToHitBonus2, sub_100E9DB0)
            .AddQueryHandler(D20DispatcherKey.QUE_ActionAllowed, ProneActionAllowed)
            .RemoveOnSignal(D20DispatcherKey.SIG_Standing_Up)
            .SetQueryResult(D20DispatcherKey.QUE_Prone, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 63, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 176)
            .Build();


        [TempleDllLocation(0x102e5ee0)]
        public static readonly ConditionSpec TimedDisappear = ConditionSpec.Create("Timed-Disappear", 3)
            .AddHandler(DispatcherType.BeginRound, D20ModCountdownHandler, 20, 1)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .Build();


        [TempleDllLocation(0x102e5c28)]
        public static readonly ConditionSpec Poisoned = ConditionSpec.Create("Poisoned", 3)
            .RemovedBy(SpellEffects.SpellHeal)
            .RemovedBy(SpellEffects.SpellNeutralizePoison)
            .AddHandler(DispatcherType.ConditionAddPre, sub_100EA020)
            .AddHandler(DispatcherType.BeginRound, PoisonedBeginRound, 16)
            .AddHandler(DispatcherType.ConditionAdd, PoisonedOnAdd)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Poisoned, true)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 55, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltipPoison, 130)
            .Build();


        [TempleDllLocation(0x102e5f28)]
        public static readonly ConditionSpec TempAbilityLoss = ConditionSpec.Create("Temp_Ability_Loss", 2)
            .AddHandler(DispatcherType.ConditionAddPre, ApplyTempAbilityLoss, TempAbilityLoss)
            .AddHandler(DispatcherType.ConditionAdd, TempAbilityLoss2)
            .AddHandler(DispatcherType.AbilityScoreLevel, AbilityDamageStatLevel)
            .AddHandler(DispatcherType.GetAbilityLoss, Dispatch59Ability_Damage)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, TempAbilityLossHeal)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, KilledWithAbilityLoss)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltipAbilityDamage, 144)
            .Build();


        [TempleDllLocation(0x102e5fd0)]
        public static readonly ConditionSpec DamageAbilityLoss = ConditionSpec.Create("Damage_Ability_Loss", 2)
            .RemovedBy(SpellEffects.SpellHeal)
            .AddHandler(DispatcherType.ConditionAddPre, ApplyTempAbilityLoss, DamageAbilityLoss)
            .AddHandler(DispatcherType.ConditionAdd, TempAbilityLoss2)
            .AddHandler(DispatcherType.AbilityScoreLevel, AbilityDamageStatLevel)
            .AddHandler(DispatcherType.GetAbilityLoss, Dispatch59Ability_Damage)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, DamageAbilityLossHealing)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, KilledWithAbilityLoss)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltipAbilityDamage, 144)
            .Build();


        [TempleDllLocation(0x102e6260)]
        public static readonly ConditionSpec InspiredCourage = ConditionSpec.Create("Inspired_Courage", 4)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 2,
                "Bardic-Inspire Courage-hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 2, "Bardic-Inspire Courage-hit")
            .AddHandler(DispatcherType.ConditionAddPre, BardicMusicInspireRefresh, InspiredCourage)
            .AddHandler(DispatcherType.BeginRound, BardicMusicInspireBeginRound, 0, 1)
            .AddHandler(DispatcherType.ConditionAdd, InspiredCourageInit)
            .AddHandler(DispatcherType.ToHitBonus2, InspiredCourageToHitBon)
            .AddHandler(DispatcherType.DealingDamage, InspiredCourageDamBon)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_InspiredCourage_Callback)
            .AddHandler(DispatcherType.Tooltip, BardicMusicTooltipCallback, 80)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 4)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 2)
            .Build();


        [TempleDllLocation(0x102e6090)]
        public static readonly ConditionSpec Charmed = ConditionSpec.Create("Charmed", 3)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 25, SpellEffects.SpellCharmPerson)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Charmed, IsCharmedQueryHandler)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 73, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 98)
            .Build();


        [TempleDllLocation(0x102e6358)]
        public static readonly ConditionSpec Countersong = ConditionSpec.Create("Countersong", 3)
            .AddHandler(DispatcherType.ConditionAddPre, BardicEffectPreAdd, Countersong)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Bardic_Music_Completed)
            .AddHandler(DispatcherType.CountersongSaveThrow, SavingThrow_CounterSong_Callback)
            .AddHandler(DispatcherType.Tooltip, BardicMusicTooltipCallback, 5043)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 78)
            .Build();


        [TempleDllLocation(0x102e6400)]
        public static readonly ConditionSpec Fascinate = ConditionSpec.Create("Fascinate", 2)
            .AddHandler(DispatcherType.ConditionAddPre, BardicEffectPreAdd, Fascinate)
            .AddHandler(DispatcherType.ConditionAdd, FascinateOnAdd, 1)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 2)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .RemoveOnSignal(D20DispatcherKey.SIG_Action_Recipient)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 81, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Bardic_Music_Completed)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "Bardic-Fascinate-hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "Bardic-Fascinate-hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .Build();


        [TempleDllLocation(0x102e6560)]
        public static readonly ConditionSpec Competence = ConditionSpec.Create("Competence", 2)
            .AddHandler(DispatcherType.ConditionAddPre, BardicEffectPreAdd, Competence)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.CompetenceBonus, 2, 193)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 83, 0)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "Bardic-Inspire Competence-hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "Bardic-Inspire Competence-hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 4)
            .Build();


        [TempleDllLocation(0x102e6630)]
        public static readonly ConditionSpec Suggestion = ConditionSpec.Create("Suggestion", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, BardicSuggestionAdd, 1)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 84, 0)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Afraid, true)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 52, 0)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "Bardic-Suggestion-hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "Bardic-Suggestion-hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .Build();


        [TempleDllLocation(0x102e6728)]
        public static readonly ConditionSpec Greatness = ConditionSpec.Create("Greatness", 4)
            .AddHandler(DispatcherType.ConditionAddPre, BardicMusicInspireRefresh, InspiredCourage)
            .AddHandler(DispatcherType.BeginRound, BardicMusicInspireBeginRound, 0, 1)
            .AddHandler(DispatcherType.ConditionAdd, BardicGreatnessOnAdd)
            .AddHandler(DispatcherType.ToHitBonus2, BardicGreatnessToHitBonus)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, BardicGreatnessSaveBonus)
            .AddHandler(DispatcherType.Tooltip, BardicMusicTooltipCallback, 82)
            .AddHandler(DispatcherType.TakingDamage2, BardicGreatnessTakingDamage2, 12)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 2)
            .AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 2,
                "Bardic-Inspire Greatness-hit")
            .SetQueryResult(D20DispatcherKey.QUE_Has_Temporary_Hit_Points, true)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 5)
            .Build();


        [TempleDllLocation(0x102e6128)]
        public static readonly ConditionSpec Grappled = ConditionSpec.Create("Grappled", 3)
            .AddHandler(DispatcherType.BeginRound, GrappledOnBeginRound, 26)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_Grapple_Removed, D20ModCountdownEndHandler, 26)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Grappling, true)
            .AddHandler(DispatcherType.GetMoveSpeedBase, CommonConditionCallbacks.GrappledMoveSpeed, 0, 232)
            .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.GrappledMoveSpeed, 0, 232)
            .AddSignalHandler(D20DispatcherKey.SIG_BreakFree, DummyCallbacks.EmptyFunction)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 88, 0)
            .AddHandler(DispatcherType.RadialMenuEntry, CommonConditionCallbacks.BreakFreeRadial, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 174)
            .Build();


        [TempleDllLocation(0x102e6820)]
        public static readonly ConditionSpec Stunned = ConditionSpec.Create("Stunned", 2)
            .AddHandler(DispatcherType.ConditionAddPre, StunnedPreAdd, Stunned)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Stunned, true)
            .SetQueryResult(D20DispatcherKey.QUE_Helpless, true)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddSignalHandler(D20DispatcherKey.SIG_Initiative_Update, StunnedInitiativeUpdate)
            .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 0)
            .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, StunnedToHitBonus, 2)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 89, 0)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 172)
            .Build();


        [TempleDllLocation(0x102e6930)]
        public static readonly ConditionSpec Dismiss = ConditionSpec.Create("Dismiss", 3)
            .AddSignalHandler(D20DispatcherKey.SIG_Dismiss_Spells, DismissSignalHandler)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 28, (ConditionSpec) null)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Can_Dismiss_Spells,
                CommonConditionCallbacks.QueryReturnSpellId)
            .Build();


        [TempleDllLocation(0x102e69a0)]
        public static readonly ConditionSpec BarbarianRaged = ConditionSpec.Create("Barbarian_Raged", 2)
            .SetUnique()
            .RemovedBy(Unconscious)
            .RemovedBy(Dead)
            .AddHandler(DispatcherType.ConditionAdd, BarbarianRageOnAdd)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "Barbarian Rage")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "Barbarian Rage")
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, BarbarianRageStatBonus)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, BarbarianRageStatBonus)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, BarbarianRageSaveBonus)
            .AddHandler(DispatcherType.GetAC, BarbarianRageACMalus)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 5045, 0)
            .AddHandler(DispatcherType.BeginRound, BarbarianRageBeginRound)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_Barbarian_Raged, true)
            .AddHandler(DispatcherType.ConditionRemove2, BarbarianRageAfterRemove, 1)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, Barbarian_RagedHpChanged)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .Build();


        [TempleDllLocation(0x102e6b38)]
        public static readonly ConditionSpec BarbarianFatigued = ConditionSpec.Create("Barbarian_Fatigued", 2)
            .RemovedBy(SpellEffects.SpellHeal)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "barbarian fatigue")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "barbarian fatigue")
            .AddHandler(DispatcherType.ConditionAdd, BarbarianFatigueInit)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, ApplyBarbarianRageFatigue)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, ApplyBarbarianRageFatigue)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 5046, 0)
            .AddHandler(DispatcherType.BeginRound, BarbarianFatigueCountdown)
            .SetQueryResult(D20DispatcherKey.QUE_Barbarian_Fatigued, true)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddHandler(DispatcherType.ConditionRemove2, sub_100EAE60)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 146)
            .Build();


        [TempleDllLocation(0x102e6c70)]
        public static readonly ConditionSpec SmitingEvil = ConditionSpec.Create("Smiting_Evil", 0)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, SmiteEvilOnDamage)
            .AddHandler(DispatcherType.ToHitBonus2, SmiteEvilToHitBonus)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 5049, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 7)
            .Build();


        [TempleDllLocation(0x102e6d18)]
        public static readonly ConditionSpec Test = ConditionSpec.Create("Test", 1)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "Fizzle")
            .Build();


        [TempleDllLocation(0x102e6d48)]
        public static readonly ConditionSpec SpellResistance = ConditionSpec.Create("Spell Resistance", 3)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.SpellResistanceDebug)
            .AddHandler(DispatcherType.SpellResistanceMod, CommonConditionCallbacks.SpellResistanceMod_Callback, 5048)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance,
                CommonConditionCallbacks.SpellResistanceQuery)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipSpellResistanceCallback, 5048)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 60)
            .Build();


        [TempleDllLocation(0x102e6dc8)]
        public static readonly ConditionSpec CouragedAura = ConditionSpec.Create("Couraged_Aura", 5)
            .AddHandler(DispatcherType.ConditionAddPre, CouragedAuraOnPreAdd, CouragedAura)
            .AddHandler(DispatcherType.ConditionAdd, CouragedAuraOnAdd)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 4)
            .AddHandler(DispatcherType.SaveThrowLevel, CouragedAuraSavingThrow, 0, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .Build();


        [TempleDllLocation(0x102e6e88)]
        public static readonly ConditionSpec NewRoundThisTurn = ConditionSpec.Create("NewRound_This_Turn", 0)
            .SetUnique()
            .AddHandler(DispatcherType.Initiative, CommonConditionCallbacks.conditionRemoveCallback)
            .SetQueryResult(D20DispatcherKey.QUE_NewRound_This_Turn, true)
            .Build();


        [TempleDllLocation(0x102e6ee0)]
        public static readonly ConditionSpec IncubatingDisease = ConditionSpec.Create("Incubating_Disease", 3)
            .RemovedBy(SpellEffects.SpellHeal)
            .AddHandler(DispatcherType.ConditionAdd, IncubatingDiseaseOnAdd)
            .AddHandler(DispatcherType.ConditionAddPre, DiseasedPreAdd, IncubatingDisease)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_CALENDARICAL, IncubatingDiseaseNewday, 1)
            .RemoveOnSignal(D20DispatcherKey.SIG_Remove_Disease)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Diseased, true)
            .Build();


        [TempleDllLocation(0x102e6f78)]
        public static readonly ConditionSpec NSDiseased = ConditionSpec.Create("NSDiseased", 3)
            .RemovedBy(SpellEffects.SpellHeal)
            .AddHandler(DispatcherType.ConditionAdd, sub_100EB500)
            .AddHandler(DispatcherType.ConditionAddPre, DiseasedPreAdd, NSDiseased)
            .AddHandler(DispatcherType.BeginRound, DiseaseBeginRound)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_CALENDARICAL, DiseasedNewDay)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 53, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Remove_Disease)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Diseased, true)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltipDiseased, 110)
            .Build();


        [TempleDllLocation(0x102e7048)]
        public static readonly ConditionSpec DetectingEvil = ConditionSpec.Create("Detecting Evil", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 3)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Detect_Evil, true)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.BeginRound, DetectingEvilBeginRound)
            .RemoveOnSignal(D20DispatcherKey.SIG_Concentration_Broken)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "sp-Detect Alignment")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "sp-Detect Alignment")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 21)
            .Build();


        [TempleDllLocation(0x102e7140)]
        public static readonly ConditionSpec DetectedEvil = ConditionSpec.Create("Detected Evil", 2)
            .AddHandler(DispatcherType.ConditionAddPre, CondPreventIncArg, DetectedEvil)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "sp-Detect Alignment Evil")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "sp-Detect Alignment Evil")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .Build();


        [TempleDllLocation(0x102e71c0)]
        public static readonly ConditionSpec KilledByDeathEffect = ConditionSpec.Create("Killed By Death Effect", 2)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, KilledByDeathEffectHasCond,
                KilledByDeathEffect, 0)
            .Build();


        [TempleDllLocation(0x102e71f0)]
        public static readonly ConditionSpec TempNegativeLevel = ConditionSpec.Create("Temp Negative Level", 3)
            .RemovedBy(SpellEffects.SpellRestoration)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.TempNegativeLvlOnAdd, 0, 0)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_CALENDARICAL, TempNegativeLevelNewday)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 266, 0)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 266, 0)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 266, 0)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 266, 0)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 266, 0)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 147)
            .Build();


        [TempleDllLocation(0x102e72d8)]
        public static readonly ConditionSpec PermNegativeLevel = ConditionSpec.Create("Perm Negative Level", 3)
            .AddHandler(DispatcherType.ConditionAdd, PermanentNegativeLevelOnAdd)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 267, 0)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 267, 0)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 267, 0)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 267, 0)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 267, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Experience_Awarded, PermNegLevelExpGained)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 148)
            .Build();


        [TempleDllLocation(0x102e73a8)]
        public static readonly ConditionSpec FallenPaladin = ConditionSpec.Create("Fallen_Paladin", 0)
            .AddHandler(DispatcherType.ConditionAdd, sub_100EB7C0)
            .SetUnique()
            .SetQueryResult(D20DispatcherKey.QUE_IsFallenPaladin, true)
            .RemoveOnSignal(D20DispatcherKey.SIG_Atone_Fallen_Paladin)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 175)
            .Build();


        [TempleDllLocation(0x102e51d8)]
        public static readonly ConditionSpec Paralyzed = ConditionSpec.Create("Paralyzed", 3)
            .SetUnique()
            .AddHandler(DispatcherType.BeginRound, ParalyzedOnBeginRound, 5)
            .AddHandler(DispatcherType.TurnBasedStatusInit, ParalyzedTurnBasedStatusInit)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, D20ModsSpellEndHandler, 5, SpellEffects.SpellHoldPerson)
            .AddQueryHandler(D20DispatcherKey.QUE_Helpless, QueryRetTrueIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_CannotCast, QueryRetTrueIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, QueryRetFalseIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_CoupDeGrace, QueryRetTrueIfNoFreedomOfMovement)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, TooltipSimpleCallback, 149)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 120)
            .Build();


        [TempleDllLocation(0x102e74c0)]
        public static readonly ConditionSpec Feinting = ConditionSpec.Create("Feinting", 5)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, sub_100EB860)
            .AddHandler(DispatcherType.AcModifyByAttacker, FeintAcBonus2Cap)
            .AddSignalHandler(D20DispatcherKey.SIG_EndTurn, FeintingEndTurn)
            .AddQueryHandler(D20DispatcherKey.QUE_OpponentSneakAttack, FeintingOpponentSneakAttack)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .Build();


        [TempleDllLocation(0x102e7568)]
        public static readonly ConditionSpec AugmentSummoningEnhancement = ConditionSpec
            .Create("Augment Summoning Enhancement", 0)
            .SetUnique()
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, AugmentSummoningStatBonus)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, AugmentSummoningStatBonus)
            .Build();


        [TempleDllLocation(0x102e75c0)]
        public static readonly ConditionSpec FailedDecipherScript = ConditionSpec.Create("Failed Decipher Script", 6)
            .SetUnique()
            .AddQueryHandler(D20DispatcherKey.QUE_FailedDecipherToday, FailedDecipherScriptTodayQuery)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.conditionRemoveCallback)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .Build();


        [TempleDllLocation(0x102e7640)]
        public static readonly ConditionSpec EncumberedMedium = ConditionSpec.Create("Encumbered Medium", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium, EncumbranceQuery, 0, 320)
            .AddSignalHandler(D20DispatcherKey.SIG_Inventory_Update, UpdateEncumbrance)
            .AddSignalHandler(D20DispatcherKey.SIG_Update_Encumbrance, UpdateEncumbrance)
            .AddHandler(DispatcherType.MaxDexAcBonus, MaxDexAcBonusCallback, 3, 320)
            .AddHandler(DispatcherType.GetMoveSpeed, EncumberedMoveSpeedCallback, 0, 320)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 172, 0)
            .AddSkillLevelHandler(SkillId.balance, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.climb, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.escape_artist, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.hide, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.jump, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.move_silently, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.pick_pocket, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddSkillLevelHandler(SkillId.tumble, CommonConditionCallbacks.EncumbranceSkillLevel, 3, 320)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 177)
            .Build();


        [TempleDllLocation(0x102e7788)]
        public static readonly ConditionSpec EncumberedHeavy = ConditionSpec.Create("Encumbered Heavy", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy, EncumbranceQuery, 0, 321)
            .AddSignalHandler(D20DispatcherKey.SIG_Inventory_Update, UpdateEncumbrance)
            .AddSignalHandler(D20DispatcherKey.SIG_Update_Encumbrance, UpdateEncumbrance)
            .AddHandler(DispatcherType.MaxDexAcBonus, MaxDexAcBonusCallback, 3, 321)
            .AddHandler(DispatcherType.GetMoveSpeed, EncumberedMoveSpeedCallback, 0, 321)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 173, 0)
            .AddSkillLevelHandler(SkillId.balance, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.climb, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.escape_artist, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.hide, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.jump, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.move_silently, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.pick_pocket, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddSkillLevelHandler(SkillId.tumble, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 321)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 178)
            .Build();


        [TempleDllLocation(0x102e78d0)]
        public static readonly ConditionSpec EncumberedOverburdened = ConditionSpec.Create("Encumbered Overburdened", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened, EncumbranceQuery, 0, 324)
            .AddSignalHandler(D20DispatcherKey.SIG_Inventory_Update, UpdateEncumbrance)
            .AddSignalHandler(D20DispatcherKey.SIG_Update_Encumbrance, UpdateEncumbrance)
            .AddHandler(DispatcherType.MaxDexAcBonus, MaxDexAcBonusCallback, 3, 324)
            .AddHandler(DispatcherType.GetMoveSpeed, EncumberedMoveSpeedCallback, 0, 324)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 186, 0)
            .AddSkillLevelHandler(SkillId.balance, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.climb, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.escape_artist, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.hide, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.jump, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.move_silently, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.pick_pocket, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddSkillLevelHandler(SkillId.tumble, CommonConditionCallbacks.EncumbranceSkillLevel, 6, 324)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 179)
            .Build();


        [TempleDllLocation(0x102e7a18)]
        public static readonly ConditionSpec BrawlPlayer = ConditionSpec.Create("Brawl Player", 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Broadcast_Action, BrawlPlayerBroadcastAction)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, BrawlHpChanged)
            .AddSignalHandler(D20DispatcherKey.SIG_EndTurn, BrawlOnEndTurn)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, BrawlPlayerCombatEnd)
            .Build();


        [TempleDllLocation(0x102e7a88)]
        public static readonly ConditionSpec BrawlSpectator = ConditionSpec.Create("Brawl Spectator", 0)
            .SetQueryResult(D20DispatcherKey.QUE_EnterCombat, false)
            .RemoveOnSignal(D20DispatcherKey.SIG_Combat_End)
            .Build();


        [TempleDllLocation(0x102e7ad0)]
        public static readonly ConditionSpec BrawlOpponent = ConditionSpec.Create("Brawl Opponent", 0)
            .AddHandler(DispatcherType.TakingDamage2, BrawlTakingDamage)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, BrawlHpChanged)
            .AddSignalHandler(D20DispatcherKey.SIG_EndTurn, BrawlOnEndTurn)
            .RemoveOnSignal(D20DispatcherKey.SIG_Combat_End)
            .Build();


        [TempleDllLocation(0x102e7b40)]
        public static readonly ConditionSpec Blindness = ConditionSpec.Create("Blindness", 2)
            .RemovedBy(SpellEffects.SpellHeal)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAddPre, sub_100EC220, SpellEffects.SpellRemoveBlindness)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_12,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_12)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Blinded, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 0, 4)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.SightImpairmentSkillPenalty, 1, 4)
            .AddHandler(DispatcherType.GetMoveSpeed, CommonConditionCallbacks.sub_100EFD60)
            .AddHandler(DispatcherType.GetAttackerConcealmentMissChance,
                CommonConditionCallbacks.AddAttackerInvisibleBonusWithCustomMessage, 50, 189)
            .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition,
                CommonConditionCallbacks.AddAttackerInvisibleBonus, 2)
            .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 189)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 76, 0)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 93)
            .Build();


        [TempleDllLocation(0x102e7ca0)]
        public static readonly ConditionSpec ElixerTimedSkillBonus = ConditionSpec.Create("Elixer Timed Skill Bonus", 3)
            .AddHandler(DispatcherType.ConditionAddPre, ElixerTimedSkillBonusGuard)
            .AddHandler(DispatcherType.ConditionAdd, ElixerTimedBonusInit)
            .AddHandler(DispatcherType.BeginRound, ElixerTimedSkillBonusBeginRound)
            .AddHandler(DispatcherType.SkillLevel, SkillModifier_ElixerTimedSkillBonus_Callback, 323)
            .AddHandler(DispatcherType.Tooltip, ElixerTimeSkillBonusTooltipCallback)
            .AddHandler(DispatcherType.EffectTooltip, ExlixirTimedBonusEffectTooltip)
            .Build();


        [TempleDllLocation(0x102e7d38)]
        public static readonly ConditionSpec ParalyzedAbilityScore = ConditionSpec
            .Create("Paralyzed - Ability Score", 0)
            .SetUnique()
            .AddHandler(DispatcherType.TurnBasedStatusInit, ParalyzedTurnBasedStatusInit)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .AddQueryHandler(D20DispatcherKey.QUE_Helpless, QueryRetTrueIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_CannotCast, QueryRetTrueIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, QueryRetFalseIfNoFreedomOfMovement)
            .AddQueryHandler(D20DispatcherKey.QUE_CoupDeGrace, QueryRetTrueIfNoFreedomOfMovement)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.Tooltip, TooltipSimpleCallback, 149)
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 167)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, Condition_Paralyzed_Ability_Score__HP_Changed)
            .Build();


        public static IReadOnlyList<ConditionSpec> Conditions { get; } = new List<ConditionSpec>
        {
            Damaged,
            Afraid,
            SpellResistance,
            Dying,
            Flatfooted,
            TempAbilityLoss,
            DetectingEvil,
            PermNegativeLevel,
            FailedDecipherScript,
            ParalyzedAbilityScore,
            FallenPaladin,
            Grappled,
            Feinting,
            Greatness,
            Test,
            Dominate,
            SpellPoisoned,
            Charging,
            Surprised,
            Prone,
            Disabled,
            IncubatingDisease,
            CouragedAura,
            Unconscious,
            BarbarianRaged,
            Countersong,
            TempNegativeLevel,
            Dismiss,
            Competence,
            AIControlled,
            BrawlPlayer,
            Diseased,
            TimedDisappear,
            KilledByDeathEffect,
            Invisible,
            Blindness,
            BarbarianFatigued,
            SpellInterrupted,
            Paralyzed,
            Charmed,
            StunningFistAttacking,
            NewRoundThisTurn,
            EncumberedMedium,
            TemporaryHitPoints,
            Fascinate,
            SmitingEvil,
            BrawlOpponent,
            Suggestion,
            DetectedEvil,
            Poisoned,
            AugmentSummoningEnhancement,
            TotalDefense,
            Stunned,
            EncumberedOverburdened,
            EncumberedHeavy,
            BrawlSpectator,
            Dead,
            Sleeping,
            Cursed,
            Held,
            InspiredCourage,
            NSDiseased,
            ElixerTimedSkillBonus,
            HoldingCharge,
            SurpriseRound,
            DamageAbilityLoss,
        };

        [DispTypes(DispatcherType.CountersongSaveThrow)]
        [TempleDllLocation(0x100ea760)]
        public static void SavingThrow_CounterSong_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if ((dispIo.flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_SONIC) != 0)
            {
                ref var bonlist = ref dispIo.bonlist;
                var condArg2 = evt.GetConditionArg2();
                var bonusTotal = dispIo.bonlist.OverallBonus;
                var rollRes = dispIo.rollResult + bonusTotal;
                if (rollRes < condArg2)
                {
                    bonlist.AddBonus(condArg2 - rollRes, 0, 192);
                }
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ea040)]
        [TemplePlusLocation("poison.cpp:32")]
        public static void PoisonedBeginRound(in DispatcherCallbackArgs evt, int data)
        {
            var critter = evt.objHndCaller;

            var dispIo = evt.GetDispIoD20Signal();
            var poisonId = evt.GetConditionArg1();
            var poison = GameSystems.Poison.GetPoison(poisonId);
            if (poison == null)
            {
                evt.RemoveThisCondition();
                return;
            }

            // decrement duration
            var dur = evt.GetConditionArg2();
            var durRem = dur - dispIo.data1;
            if (durRem >= 0)
            {
                evt.SetConditionArg2(durRem);
                return;
            }

            // make saving throw
            var dc = poison.DC;
            if (dc <= 0)
            {
                dc = evt.GetConditionArg3();
            }

            if (dc < 0 || dc > 100) // failsafe
            {
                dc = 15;
            }

            if (poison.DelayedEffects.Count == 0)
            {
                evt.RemoveThisCondition();
                return;
            }

            // success - remove condition
            if (GameSystems.D20.Combat.SavingThrow(critter, null, dc, poison.SavingThrowType,
                D20SavingThrowFlag.POISON))
            {
                evt.RemoveThisCondition();
                return;
            }

            // failure

            // check delay poison
            if (evt.objHndCaller.HasCondition("sp-Delay Poison"))
            {
                GameSystems.Spell.FloatSpellLine(critter, 20033, TextFloaterColor.White);
                evt.RemoveThisCondition();
                return;
            }

            foreach (var delayedEffect in poison.DelayedEffects)
            {
                GameSystems.Poison.ApplyPoisonEffect(critter, delayedEffect);
            }

            evt.RemoveThisCondition();
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e87d0)]
        [TemplePlusLocation("spell_condition.cpp:106")]
        public static void InvisibilityAooWilltake(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (spellId != 0)
            {
                if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    var dispIo = evt.GetDispIoD20Query();
                    if (spellPkt.spellEnum == WellKnownSpells.Invisibility
                        || spellPkt.spellEnum == WellKnownSpells.InvisibilityToAnimals
                        || spellPkt.spellEnum == WellKnownSpells.InvisibilityToUndead)
                    {
                        dispIo.return_val = 0;
                    }
                    else
                    {
                        dispIo.return_val = 1;
                    }
                }
                else
                {
                    Logger.Info(
                        "d20_mods_spells.c / _spell_query_cause_fear(): unabled to get spell_packet for spell_id {0}",
                        spellId);
                }
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100e9d30)]
        public static void sub_100E9D30(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoAttackBonus();
            if (condArg1 >= 1)
            {
                evt.SetConditionArg1(condArg1 - 1);
                dispIo.bonlist.AddBonus(2, 0, 160);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ea810)]
        public static void FascinateOnAdd(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg1() == -1)
            {
                evt.RemoveThisCondition();
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100eb9b0)]
        public static void AugmentSummoningStatBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonusFromFeat(4, 12, 114,
                FeatId.AUGMENT_SUMMONING);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100e9310)]
        public static void RemoveIfImmunePoison(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Poison))
            {
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e8890)]
        public static void InvisibilityAooIncurs(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoD20Query();
            var obj = (GameObjectBody) dispIo.obj;
            if (obj != null)
            {
                if (!GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                {
                    dispIo.return_val = 0;
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eb0a0)]
        public static void CouragedAuraOnAdd(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.AURA_OF_COURAGE))
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
            else
            {
                evt.SetConditionArg(4, 2);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec1b0)]
        public static void BrawlPlayerCombatEnd(in DispatcherCallbackArgs evt)
        {
            GameSystems.Combat.BrawlStatus = 1;
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100e85c0)]
        public static void ParalyzedTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                var dispIo = evt.GetDispIOTurnBasedStatus();
                if (dispIo != null)
                {
                    dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                    dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                }
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100eb3f0)]
        public static void DiseaseBeginRound(in DispatcherCallbackArgs evt)
        {
            int v2;

            var diseaseId = evt.GetConditionArg1();
            var disease = GameSystems.Disease.GetDisease(diseaseId);

            if (evt.GetConditionArg3() != 0)
            {
                var critter = evt.objHndCaller;
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(46, critter, null);
                GameSystems.D20.Combat.FloatCombatLine(critter, 53);
                var abilityLossAmt = disease.AbilityLossDice.Roll();
                evt.SetConditionArg3(0);
                if (diseaseId != 0)
                {
                    if (diseaseId != 2 && diseaseId != 9)
                    {
                        critter.AddCondition(TempAbilityLoss,
                            disease.AbilityLossStat, abilityLossAmt);
                    }
                    else if (!GameSystems.D20.Combat.SavingThrow(critter, null,
                                 disease.DC, 0, 0) && abilityLossAmt > 0)
                    {
                        critter.AddCondition(TempAbilityLoss,
                            disease.AbilityLossStat, abilityLossAmt);
                    }
                }
                else
                {
                    if (abilityLossAmt >= 2 && !GameSystems.D20.Combat.SavingThrow(critter, null, 16, 0, 0))
                    {
                        critter.AddCondition(SpellEffects.SpellBlindness);
                    }

                    critter.AddCondition(TempAbilityLoss, 0, abilityLossAmt);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e7fa0)]
        public static void UnconsciousHpChanged(in DispatcherCallbackArgs evt)
        {
            var hpCur = evt.objHndCaller.GetStat(Stat.hp_current);
            var subDual = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);

            if (hpCur > 0 && subDual < hpCur)
            {
                evt.RemoveThisCondition();
                evt.objHndCaller.AddCondition(Prone);
            }

            if (hpCur == 0)
            {
                evt.objHndCaller.AddCondition(Disabled);
            }
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ea020)]
        public static void sub_100EA020(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg1();
            evt.GetDispIoCondStruct();
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ea510)]
        [TemplePlusLocation("generalfixes.cpp:318")]
        public static void BardicMusicInspireRefresh(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                evt.SetConditionArg1(5);
                evt.SetConditionArg2(1);
                dispIo.outputFlag = false;
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e80e0)]
        public static void DyingHpChange(in DispatcherCallbackArgs evt)
        {
            var currentHp = evt.objHndCaller.GetStat(Stat.hp_current);
            var subdualDamage = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);
            var dispIo = evt.GetDispIoD20Signal();

            var v4 = dispIo.data2;
            if (v4 >= 0 && (v4 > 0 || dispIo.data1 != 0))
            {
                evt.RemoveThisCondition();
                if (currentHp < 0 || subdualDamage > currentHp)
                {
                    if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.DIEHARD))
                    {
                        evt.objHndCaller.AddCondition(StatusEffects.Unconscious);
                        return;
                    }
                }
                else if (currentHp != 0)
                {
                    evt.objHndCaller.AddCondition(StatusEffects.Prone);
                    return;
                }

                evt.objHndCaller.AddCondition(StatusEffects.Disabled);
            }
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ec220)]
        public static void sub_100EC220(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                dispIo.outputFlag = false;
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ec9b0)]
        [TemplePlusLocation("condition.cpp:499")]
        public static void D20ModCountdownHandler(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var durationArgIdx = data2;
            var durRem = evt.GetConditionArg(data2);
            var durNew = durRem - evt.GetDispIoD20Signal().data1;
            if (durNew >= 0)
            {
                evt.SetConditionArg(durationArgIdx, durNew);
            }
            else if (data1 != 6 || evt.dispType != DispatcherType.BeginRound)
            {
                D20ModCountdownEndHandler(in evt, data1);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eb860)]
        public static void sub_100EB860(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg(4, 1);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec170)]
        public static void BrawlHpChanged(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious))
            {
                GameSystems.Combat.BrawlStatus = GameSystems.Party.IsInParty(evt.objHndCaller) ? 1 : 0;
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100eb020)]
        public static void CouragedAuraOnPreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                if (dispIo.arg1 != condArg1 || dispIo.arg2 != condArg2)
                {
                    Logger.Info("two paladins!");
                }
                else
                {
                    evt.SetConditionArg(4, 2);
                    dispIo.outputFlag = false;
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e98b0)]
        [TemplePlusLocation("condition.cpp:500")]
        public static void D20ModCountdownEndHandler(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Signal();

            if (dispIo != null)
            {
                if (evt.dispType != DispatcherType.BeginRound && dispIo.data1 != evt.GetConditionArg1())
                {
                    return;
                }
            }

            if (data <= -1 || data >= 29)
            {
                return;
            }

            RemoveStatusEffect(in evt, data, dispIo);
        }

        private static void RemoveStatusEffect(in DispatcherCallbackArgs evt, int type, DispIoD20Signal dispIo)
        {
            switch (type)
            {
                case 7:
                    RemoveSleeping(in evt, dispIo);
                    break;
                case 12:
                    RemoveTempHp(in evt, dispIo);
                    break;
                case 25:
                    RemoveCharmed(in evt, dispIo);
                    break;
                case 13:
                    RemoveCursed(in evt, dispIo);
                    break;
                case 14:
                    RemoveFear(in evt, dispIo);
                    break;
                case 15:
                    RemoveDisease(in evt, dispIo);
                    break;
                case 16:
                    RemovePoison(in evt, dispIo);
                    break;
                case 5:
                    RemoveHeld(in evt, dispIo);
                    break;
                case 6:
                    RemoveInvisible(in evt, dispIo);
                    break;
                case 20:
                    RemoveConditionTimedDisappear(in evt, dispIo);
                    break;
            }

            evt.RemoveThisCondition();
        }

        [TempleDllLocation(0x100e8ee0)]
        private static void RemoveSleeping(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_sleeping(): forcibly removing ({0})",
                    evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e8f80)]
        private static void RemoveTempHp(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_temp_hp(): forcibly removing ({0})",
                    evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e8f30)]
        private static void RemoveCharmed(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_charmed(): forcibly removing ({0})",
                    evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e91f0)]
        private static void RemoveCursed(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_cursed(): forcibly removing ({0})", evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e9240)]
        private static void RemoveFear(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_fear(): forcibly removing ({0})", evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e9290)]
        private static void RemoveDisease(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_disease(): forcibly removing ({0})",
                    evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e9350)]
        private static void RemovePoison(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_poison(): forcibly removing ({0})", evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e8510)]
        private static void RemoveHeld(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_held(): forcibly removing ({0})", evt.GetConditionName());
            }
        }

        [TempleDllLocation(0x100e8750)]
        private static void RemoveInvisible(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            var critter = evt.objHndCaller;
            if (dispIo == null || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_invisibile(): forcibly removing ({0})",
                    evt.GetConditionName());
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Invisible))
                {
                    GameSystems.ObjFade.FadeTo(critter, 255, 0, 5, 0);
                }
            }
        }

        [TempleDllLocation(0x100e93a0)]
        private static void RemoveConditionTimedDisappear(in DispatcherCallbackArgs evt, DispIoD20Signal dispIo)
        {
            var critter = evt.objHndCaller;
            if (evt.dispType == DispatcherType.BeginRound
                || dispIo == null
                || dispIo.data1 == evt.GetConditionArg1())
            {
                Logger.Info("d20_mods_condition.c / _remove_condition_timed_disappear(): forcibly removing ({0})",
                    evt.GetConditionName());
                GameSystems.ParticleSys.CreateAtObj("Fizzle", critter);
                critter.AiFlags |= AiFlag.RunningOff;
                GameSystems.ObjFade.FadeTo(critter, 0, 2, 5, FadeOutResult.Destroy);
            }
        }

        [DispTypes(DispatcherType.GetAbilityLoss)]
        [TempleDllLocation(0x100ea2d0)]
        public static void Dispatch59Ability_Damage(in DispatcherCallbackArgs evt)
        {
            var stat = (Stat) evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoAbilityLoss();
            if (dispIo.statDamaged == stat)
            {
                if ((dispIo.flags & 0x10) != 0 || dispIo.result > 0)
                {
                    if ((dispIo.flags & 1) != 0)
                    {
                        if ((dispIo.flags & 4) != 0)
                        {
                            evt.SetConditionArg2(condArg2 + dispIo.result);
                            dispIo.result = 0;
                        }
                        else if ((dispIo.flags & 8) != 0)
                        {
                            var tmp = condArg2 - dispIo.result;
                            if (tmp <= 0 || (dispIo.flags & 0x10) == 0x10)
                            {
                                evt.RemoveThisCondition();
                                dispIo.result = -tmp;
                            }
                            else
                            {
                                dispIo.result -= Math.Abs(condArg2 - tmp);
                                evt.SetConditionArg2(tmp);
                            }
                        }
                    }
                }
            }

            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100eb310)]
        public static void DiseasedNewDay(in DispatcherCallbackArgs evt)
        {
            var diseaseId = evt.GetConditionArg1();
            var disease = GameSystems.Disease.GetDisease(diseaseId);

            var consecutiveSaves = evt.GetConditionArg2();
            if (GameSystems.D20.Combat.SavingThrow(evt.objHndCaller, null, disease.DC, SavingThrowType.Fortitude))
            {
                if (diseaseId == 6)
                {
                    evt.SetConditionArg2(0);
                }
                else
                {
                    consecutiveSaves++;
                    if (consecutiveSaves < disease.ConsecutiveSavesNeeded)
                    {
                        evt.SetConditionArg2(consecutiveSaves);
                    }
                    else
                    {
                        CommonConditionCallbacks.conditionRemoveCallback(in evt);
                    }
                }
            }
            else
            {
                evt.SetConditionArg2(0);
                evt.SetConditionArg3(1);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100e9db0)]
        public static void sub_100E9DB0(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) == 0)
            {
                dispIo.bonlist.AddBonus(-4, 0, 162);
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100eadf0)]
        public static void BarbarianFatigueCountdown(in DispatcherCallbackArgs evt)
        {
            int condArg1;
            int v2;

            condArg1 = evt.GetConditionArg1();
            v2 = condArg1 - evt.GetDispIoD20Signal().data1;
            if (v2 >= 0)
            {
                evt.SetConditionArg1(v2);
            }
            else if (!GameSystems.Combat.IsCombatActive())
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e81a0)]
        public static void DyingHealSkillUsed(in DispatcherCallbackArgs evt)
        {
            var currentHp = evt.objHndCaller.GetStat(Stat.hp_current);
            var subdualDamage = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);
            evt.RemoveThisCondition();

            if (currentHp >= 0)
            {
                if (subdualDamage > currentHp)
                {
                    // Positive HP, but still unconscious due to subdual damage
                    evt.objHndCaller.AddCondition(Unconscious);
                }
                else if (currentHp == 0)
                {
                    evt.objHndCaller.AddCondition(Disabled);
                }
            }
            else
            {
                // Still negative HP
                if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.DIEHARD))
                {
                    evt.objHndCaller.AddCondition(Disabled);
                }
                else
                {
                    evt.objHndCaller.AddCondition(Unconscious);
                }
            }

            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0xF, evt.objHndCaller, null);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 26);
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100eabe0)]
        [TemplePlusLocation("condition.cpp:395")]
        public static void BarbarianRageSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(2, 0, 195);
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100eb1b0)]
        public static void DiseasedPreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var diseaseId = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();

            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                if (data == IncubatingDisease)
                {
                    if (dispIo.arg2 == condArg2)
                    {
                        dispIo.outputFlag = false;
                    }
                }
                else
                {
                    if (data != StatusEffects.NSDiseased)
                    {
                        return;
                    }

                    if (dispIo.arg1 == diseaseId)
                    {
                        dispIo.outputFlag = false;
                    }
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ea5c0)]
        [TemplePlusLocation("generalfixes.cpp:319")]
        public static void InspiredCourageInit(in DispatcherCallbackArgs evt)
        {
            var bonusRounds = BardicInspiredCourageInitGetBonusRounds();
            evt.SetConditionArg1(5 + bonusRounds);
            evt.SetConditionArg2(0);
            // TODO: TP doesn't set arg3, but so neither does vanilla ?!
        }

        // Helper Function that finds the number of extra rounds to allow inspire courage to last after the bard stops singing
        private static int BardicInspiredCourageInitGetBonusRounds()
        {
            int bonusRounds = 0;

            // Find the highest level bard
            var brdLvl = 0;
            GameObjectBody highBardDude = null;
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var dudeBrdLvl = partyMember.GetStat(Stat.level_bard);
                if (dudeBrdLvl > brdLvl)
                {
                    brdLvl = dudeBrdLvl;
                    highBardDude = partyMember;
                }
            }

            // Query the highest level bard for the number of bonus rounds
            if (highBardDude != null)
            {
                bonusRounds = GameSystems.D20.D20QueryPython(highBardDude, "Bardic Ability Duration Bonus");
            }

            return bonusRounds;
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100eac80)]
        public static void ApplyBarbarianRageFatigue(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(-2, 0, 196);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec1c0)]
        public static void BrawlOnEndTurn(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.Combat.BrawlStatus != -1)
            {
                throw new NotImplementedException();
                // BrawlProcess /*0x100ebe20*/(GameSystems.Combat.BrawlStatus);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ea4b0)]
        public static void KilledWithAbilityLoss(in DispatcherCallbackArgs evt)
        {
            var critter = evt.objHndCaller;
            var maxHpWithLoss = critter.GetStat(Stat.hp_max);
            evt.RemoveThisCondition();
            var maxHpWithoutLoss = critter.GetStat(Stat.hp_max);
            if (maxHpWithoutLoss > maxHpWithLoss)
            {
                var damage = critter.GetInt32(obj_f.hp_damage);
                GameSystems.MapObject.ChangeTotalDamage(critter, maxHpWithoutLoss - maxHpWithLoss + damage);
            }
        }


        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100ec530)]
        public static void ExlixirTimedBonusEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            var condArg1 = evt.GetConditionArg1();
            if (condArg1 != 0)
            {
                switch (condArg1)
                {
                    case 0:
                        dispIo.bdb.AddEntry(74);
                        break;
                    case 1:
                        dispIo.bdb.AddEntry(75);
                        break;
                    default:
                        dispIo.bdb.AddEntry(76);
                        break;
                }
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100eb9e0)]
        public static void FailedDecipherScriptTodayQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var condArg1 = evt.GetConditionArg1();
            if (dispIo.data1 == evt.GetConditionArg2() && dispIo.data2 == condArg1)
            {
                dispIo.return_val = 1;
            }
        }

        [DispTypes(DispatcherType.ConditionRemove2)]
        [TempleDllLocation(0x100eacb0)]
        public static void BarbarianRageAfterRemove(in DispatcherCallbackArgs evt, int data)
        {
            evt.subDispNode.condNode.IsExpired = true;
            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, -2);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100eace0)]
        public static void Barbarian_RagedHpChanged(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.Critter.IsDeadOrUnconscious(evt.objHndCaller) ||
                GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller))
            {
                evt.objHndCaller.AddCondition(BarbarianFatigued);
                D20ModCountdownEndHandler(in evt, 0);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e8610)]
        public static void QueryRetTrueIfNoFreedomOfMovement(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoD20Query();
            if (!GameSystems.D20.D20Query(args.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.MaxDexAcBonus)]
        [TempleDllLocation(0x100eba40)]
        public static void MaxDexAcBonusCallback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.SetOverallCap(1, data1, 0, data2);
        }


        [DispTypes(DispatcherType.AcModifyByAttacker)]
        [TempleDllLocation(0x100eb880)]
        public static void FeintAcBonus2Cap(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (evt.GetConditionObjArg(0) == dispIo.attackPacket.victim
                && (dispIo.attackPacket.flags & D20CAF.RANGED) == 0)
            {
                dispIo.bonlist.AddCap(3, 0, 289);
                dispIo.bonlist.AddCap(8, 0, 289);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100eae90)]
        public static void StunnedPreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                var newStunnedArg1 = dispIo.arg1;
                if (condArg1 < newStunnedArg1 || condArg1 == newStunnedArg1 && condArg2 > dispIo.arg2)
                {
                    evt.SetConditionArg1(dispIo.arg1);
                    evt.SetConditionArg2(dispIo.arg2);
                }

                dispIo.outputFlag = false;
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ead40)]
        public static void BarbarianRageBeginRound(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 <= 1)
            {
                var roundsRemaining = condArg1 - dispIo.data1;
                if (roundsRemaining >= 0)
                {
                    evt.SetConditionArg1(roundsRemaining);
                }
                else
                {
                    if (!GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller) &&
                        !GameSystems.Teleport.IsProcessing)
                    {
                        evt.objHndCaller.AddCondition(BarbarianFatigued);
                    }

                    evt.RemoveThisCondition();
                }
            }
            else
            {
                D20ModCountdownEndHandler(in evt, 0);
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100ec2d0)]
        public static void SkillModifier_ElixerTimedSkillBonus_Callback(in DispatcherCallbackArgs evt, int data)
        {
            var elixirType = evt.GetConditionArg1();
            SkillId skillId;
            switch (elixirType)
            {
                case 0:
                    skillId = SkillId.hide;
                    break;
                case 1:
                    skillId = SkillId.move_silently;
                    break;
                default:
                    skillId = SkillId.spot;
                    break;
            }

            if (skillId == evt.GetSkillIdFromDispatcherKey())
            {
                var dispIo = evt.GetDispIoObjBonus();
                dispIo.bonOut.AddBonus(10, 0, data);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100eaf10)]
        public static void SmiteEvilOnDamage(in DispatcherCallbackArgs evt)
        {
            if (evt.objHndCaller.GetStat(Stat.level_paladin) != 0)
            {
                var bonlist = BonusList.Create();
                var classLvl = evt.objHndCaller.DispatchGetLevel((int) Stat.level_paladin, bonlist, null);
                if (classLvl < 1)
                {
                    classLvl = 1;
                }

                var dispIo = evt.GetDispIoDamage();
                var featName = GameSystems.Feat.GetFeatName(FeatId.SMITE_EVIL);
                dispIo.damage.AddDamageBonus(classLvl, 0, 114, featName);
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }

        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100ec7e0)]
        public static void EffectTooltipAbilityDamage(in DispatcherCallbackArgs evt, int data)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoEffectTooltip();
            var name = GameSystems.D20.RadialMenu.GetAbilityReducedName(condArg1 + 161);
            dispIo.bdb.AddEntry(data, $": {name}");
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100eb760)]
        public static void PermNegLevelExpGained(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var xpRequired = GameSystems.Level.GetExperienceForLevel(condArg2);
            if (evt.GetDispIoD20Signal().data1 >= xpRequired)
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e9b20)]
        public static void HoldingChargeSequence(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Signal();
            if (condArg1 != 0)
            {
                bool found = false;
                var actionSequence = (ActionSequence) dispIo.obj;
                foreach (var action in actionSequence.d20ActArray)
                {
                    if (action.d20ActType < D20ActionType.RELOAD
                        || action.d20ActType > D20ActionType.DOUBLE_MOVE)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    Logger.Info("Lost Charge");
                    CommonConditionCallbacks.conditionRemoveCallback(in evt);
                }
            }
            else
            {
                evt.SetConditionArg1(1);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ecfe0)]
        public static void CondPreventIncArg(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                evt.SetConditionArg1(condArg1 + dispIo.arg1);
                dispIo.outputFlag = false;
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100eced0)]
        public static void KilledByDeathEffectHasCond(in DispatcherCallbackArgs evt, ConditionSpec data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.data1 == data2 && dispIo.data2 == 0)
            {
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100e92e0)]
        public static void DiseasedApplyStrengthMalus(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(-5, 0, 168);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ec3a0)]
        public static void ElixerTimedBonusInit(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg2(600);
            var partSys = GameSystems.ParticleSys.CreateAtObj("Elixir of Hiding", evt.objHndCaller);
            evt.SetConditionPartSysArg(2, (PartSys) partSys);
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100e88d0)]
        [TemplePlusLocation("critter.cpp:123")]
        public static void InvisibleToHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var victim = dispIo.attackPacket.victim;
            if (victim == null || !GameSystems.Feat.HasFeat(victim, FeatId.BLIND_FIGHT))
            {
                if (evt.objHndCaller.HasCondition(SpellEffects.SpellGlitterdust))
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(220);
                }
                else if (evt.objHndCaller.HasCondition(SpellEffects.SpellFaerieFire))
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(239);
                }
                else if (evt.objHndCaller.HasCondition(SpellEffects.SpellInvisibilityPurgeHit))
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(247);
                }
                else
                {
                    // Check for conditional invisibility
                    var spellId = evt.GetConditionArg1();
                    if (victim != null && !ConditionalInvisibilityApplies(spellId, victim))
                    {
                        return;
                    }

                    if (victim != null)
                    {
                        if (!GameSystems.D20.D20Query(victim, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                        {
                            dispIo.bonlist.AddBonus(2, 0, 161);
                        }
                    }
                }
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100eab20)]
        public static void GrappledOnBeginRound(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg2() == 1)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21002, TextFloaterColor.Red);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e9e20)]
        public static void ProneActionAllowed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((D20ActionType) dispIo.data1 != D20ActionType.STAND_UP)
            {
                dispIo.return_val = (int) ActionErrorCode.AEC_CANT_WHILE_PRONE;
            }
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100ea6e0)]
        public static void BardicMusicTooltipCallback(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg2() != 0)
            {
                var dispIo = evt.GetDispIoTooltip();
                var text = GameSystems.D20.Combat.GetCombatMesLine(data);
                dispIo.Append(text);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100eca30)]
        [TemplePlusLocation("generalfixes.cpp:177")]
        public static void EncumbranceQuery(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed))
            {
                return;
            }

            // TODO: I think this function needs a double-check...
            // I just reused GetCarryingCapacityByLoad :|

            var strength = evt.objHndCaller.GetStat(Stat.strength);
            var totalWeight = GameSystems.Item.GetTotalCarriedWeight(evt.objHndCaller);
            var v3 = data2;
            var dispIo = evt.GetDispIoD20Query();
            if (v3 == 320 /* Medium Encumbrance */)
            {
                dispIo.return_val = GameSystems.Stat.GetCarryingCapacityByLoad(strength, EncumbranceType.MediumLoad);
                return;
            }

            if (v3 == 321 /* Heavy Encumbrance */
                || v3 == 324 /* Overburdened Encumbrance */)
            {
                dispIo.return_val = GameSystems.Stat.GetCarryingCapacityByLoad(strength, EncumbranceType.HeavyLoad);
                return;
            }

            var limit = GameSystems.Stat.GetCarryingCapacityByLoad(strength, EncumbranceType.LightLoad);
            if (totalWeight <= limit)
            {
                dispIo.return_val = limit;
            }
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100eb100)]
        [TemplePlusLocation("condition.cpp:458")]
        public static void CouragedAuraSavingThrow(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if ((dispIo.flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) == 0)
            {
                return;
            }

            var auraSource = evt.GetConditionObjArg(0);
            if (auraSource == null)
            {
                return;
            }

            if (GameSystems.D20.D20Query(auraSource, D20DispatcherKey.QUE_Unconscious))
            {
                evt.RemoveThisCondition();
            }

            // if distance < 10 ft
            if (auraSource.DistanceToObjInFeet(evt.objHndCaller) < 10.0f)
            {
                dispIo.bonlist.AddBonus(4, 13, 204);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec140)]
        public static void BrawlPlayerBroadcastAction(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var actionType = (D20ActionType) dispIo.data1;
            if (actionType == D20ActionType.CAST_SPELL || actionType == D20ActionType.SPELL_CALL_LIGHTNING)
            {
                GameSystems.Combat.BrawlStatus = 2;
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eb500)]
        public static void sub_100EB500(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg1();
            evt.SetConditionArg3(1);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e8e60)]
        public static void InvisibleOnDeactivate(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == condArg1)
            {
                evt.RemoveThisCondition();
                if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Invisible))
                {
                    GameSystems.ObjFade.FadeTo(evt.objHndCaller, 255, 0, 5, 0);
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ea3b0)]
        [TemplePlusLocation("condition.cpp:341")]
        public static void TempAbilityLoss2(in DispatcherCallbackArgs evt)
        {
            var statDamaged = (Stat) evt.GetConditionArg1();
            if (statDamaged >= Stat.strength && statDamaged <= Stat.charisma)
            {
                int scoreLevel = evt.objHndCaller.GetStat(statDamaged);
                if (scoreLevel < 0)
                {
                    var amountDamaged = evt.GetConditionArg2();
                    // dont let stat go below zero
                    amountDamaged += scoreLevel;
                    if (amountDamaged <= 0)
                    {
                        // remove condition, as it should have no effect
                        evt.RemoveThisCondition();
                    }
                    else
                    {
                        evt.SetConditionArg2(amountDamaged);
                    }
                }
            }

            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100ec430)]
        public static void ElixerTimeSkillBonusTooltipCallback(in DispatcherCallbackArgs evt)
        {
            var elixirType = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoTooltip();

            int meslineKey;
            switch (elixirType)
            {
                case 0:
                    meslineKey = 176;
                    break;
                case 1:
                    meslineKey = 177;
                    break;
                default:
                case 2:
                    meslineKey = 178;
                    break;
            }

            var text = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            dispIo.AppendUnique(text);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100eb900)]
        public static void FeintingEndTurn(in DispatcherCallbackArgs evt)
        {
            var remainingTurns = evt.GetConditionArg(4) - 1;
            evt.SetConditionArg(4, remainingTurns);
            if (remainingTurns < 0)
            {
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100eabb0)]
        [TemplePlusLocation("condition.cpp:394")]
        public static void BarbarianRageStatBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(4, 0, 195);
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100eac10)]
        [TemplePlusLocation("condition.cpp:396")]
        public static void BarbarianRageACMalus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(-2, 0, 195);
        }

        private enum NewEncumrance
        {
            LightLoad,
            MediumLoad,
            HeavyLoad,
            Overburdened
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ecb20)]
        public static void UpdateEncumbrance(in DispatcherCallbackArgs evt)
        {
            var critter = evt.objHndCaller;
            var strScore = critter.GetStat(0);

            var limitLight = GameSystems.Stat.GetCarryingCapacityByLoad(strScore, EncumbranceType.LightLoad);
            var limitMedium = GameSystems.Stat.GetCarryingCapacityByLoad(strScore, EncumbranceType.MediumLoad);
            var limitHeavy = GameSystems.Stat.GetCarryingCapacityByLoad(strScore, EncumbranceType.HeavyLoad);

            // Determine once, what the new limit should be
            var carriedWeight = GameSystems.Item.GetTotalCarriedWeight(critter);
            NewEncumrance newEncumrance;
            string newEncumbranceCondition;
            if (carriedWeight > limitHeavy)
            {
                newEncumrance = NewEncumrance.Overburdened;
                newEncumbranceCondition = "Encumbered Overburdened";
            }
            else if (carriedWeight > limitMedium)
            {
                newEncumrance = NewEncumrance.HeavyLoad;
                newEncumbranceCondition = "Encumbered Heavy";
            }
            else if (carriedWeight > limitLight)
            {
                newEncumrance = NewEncumrance.MediumLoad;
                newEncumbranceCondition = "Encumbered Medium";
            }
            else
            {
                newEncumrance = NewEncumrance.LightLoad;
                newEncumbranceCondition = null;
            }

            var complain = false;

            // Handle transitions from existing encumrance levels and play voice lines for increasing encumbrance
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium))
            {
                if (newEncumrance == NewEncumrance.MediumLoad)
                {
                    return; // Same as before
                }

                if (newEncumrance > NewEncumrance.MediumLoad)
                {
                    complain = true;
                }

                evt.RemoveThisCondition();
            }
            else if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy))
            {
                if (newEncumrance == NewEncumrance.HeavyLoad)
                {
                    return; // Same as before
                }

                if (newEncumrance > NewEncumrance.HeavyLoad)
                {
                    complain = true;
                }

                evt.RemoveThisCondition();
            }
            else if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened))
            {
                if (newEncumrance == NewEncumrance.Overburdened)
                {
                    return; // Same as before
                }

                evt.RemoveThisCondition();
            }

            if (complain)
            {
                GameSystems.Dialog.PlayOverburdenedVoiceLine(critter);
            }

            if (newEncumbranceCondition != null)
            {
                critter.AddCondition(newEncumbranceCondition);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ea880)]
        public static void BardicGreatnessOnAdd(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(5);
            evt.SetConditionArg2(0);
            var conMod = evt.objHndCaller.GetStat(Stat.con_mod);
            var tempHp = Dice.Roll(2, 10);
            evt.SetConditionArg4(tempHp + 2 * conMod);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e9450)]
        public static void SpellInterruptedAnimCastConjureEnd(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.PlayFizzle(evt.objHndCaller);
            CommonConditionCallbacks.conditionRemoveCallback(in evt);
        }

        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100eb2a0)]
        public static void IncubatingDiseaseNewday(in DispatcherCallbackArgs evt, int data)
        {
            var incubatingDiseaseId = evt.GetConditionArg2();
            var remainingDays = evt.GetConditionArg3();

            if (remainingDays > 1)
            {
                evt.SetConditionArg3(remainingDays - 1);
            }
            else
            {
                evt.RemoveThisCondition();
                evt.objHndCaller.AddCondition(NSDiseased, incubatingDiseaseId, 0);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e9c70)]
        public static void DamagedSpellInterrupted(in DispatcherCallbackArgs evt)
        {
            var damageTaken = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1)
            {
                var critter = evt.objHndCaller;
                var dc = 10 + damageTaken;
                if (!GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration, dc, out _, SkillCheckFlags.UnderDuress))
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x20, critter, null);
                    GameSystems.D20.Combat.FloatCombatLine(critter, 54);
                    dispIo.return_val = 1;
                }
            }
        }

        // TODO: This table seems to fuck over high level chars because it somehow ... wraps at level 10?
        private static readonly int[] PermanentNegativeLevelXpTable =
        {
            -1, -1, 500, 2000, 4500, 8000, 12500, 18000, 24500, 32000, 40500,
            0, 0, 1000, 3000, 6000, 10000, 15000, 21000, 28000, 36000, 45000
        };

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eb6a0)]
        public static void PermanentNegativeLevelOnAdd(in DispatcherCallbackArgs evt)
        {
            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, -5);

            var lvl = evt.objHndCaller.DispatchGetLevel(6, BonusList.Create(), null) + 1;
            if (lvl > 1)
            {
                evt.objHndCaller.SetInt32(obj_f.critter_experience, PermanentNegativeLevelXpTable[lvl]);
            }
            else
            {
                GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
            }

            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x16, evt.objHndCaller, null);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 126);
            evt.SetConditionArg2(lvl);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e8560)]
        public static void IsCharmedQueryHandler(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                var dispIo = evt.GetDispIoD20Query();
                dispIo.return_val = 1;
                dispIo.obj = spellPkt.caster;
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x100ebaa0)]
        [TemplePlusLocation("generalfixes.cpp:77")]
        public static void EncumberedMoveSpeedCallback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            //  in case the cap has already been set (e.g. by web/entangle) - recreating the spellslinger fix
            if (dispIo.bonlist.bonFlags == 3)
            {
                return;
            }

            if (data2 == 324) // overburdened
            {
                dispIo.bonlist.SetOverallCap(5, 5, 0, 324);
                dispIo.bonlist.SetOverallCap(6, 5, 0, 324);
                return;
            }

            // dwarves do not suffer movement penalty for medium/heavy encumbrance
            if (GameSystems.Critter.GetRace(evt.objHndCaller, true) == RaceId.dwarf)
            {
                return;
            }

            // this is probably the explicit form for base speed...
            if (dispIo.bonlist.bonusEntries[0].bonValue <= 20)
            {
                dispIo.bonlist.AddBonus(-5, 0, data2);
            }
            else
            {
                dispIo.bonlist.AddBonus(-10, 0, data2);
            }
        }

        [DispTypes(DispatcherType.ConditionRemove2)]
        [TempleDllLocation(0x100eae60)]
        public static void sub_100EAE60(in DispatcherCallbackArgs evt)
        {
            GameSystems.ParticleSys.CreateAtObj("barbarian fatigue-end", evt.objHndCaller);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eb7c0)]
        public static void sub_100EB7C0(in DispatcherCallbackArgs evt)
        {
            if (evt.objHndCaller.GetStat(Stat.level_paladin) == 0)
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec8c0)]
        public static void Condition_Paralyzed_Ability_Score__HP_Changed(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.Critter.HasZeroAttributeExceptConstitution(evt.objHndCaller))
            {
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100e9c00)]
        public static void FlatfootedAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var victim = dispIo.attackPacket.victim;
            if (victim != null && GameSystems.Feat.HasFeat(victim, FeatId.UNCANNY_DODGE))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(165);
            }
            else
            {
                dispIo.bonlist.AddCap(8, 0, 153);
                dispIo.bonlist.AddCap(38, 0, 153);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eac40)]
        public static void BarbarianFatigueInit(in DispatcherCallbackArgs evt)
        {
            var consMod = evt.objHndCaller.GetStat(Stat.con_mod);
            evt.SetConditionArg1(consMod + 5);
        }

        [DispTypes(DispatcherType.ConditionRemove2)]
        [TempleDllLocation(0x100e8090)]
        public static void UnconsciousConditionRemoved2_StandUp(in DispatcherCallbackArgs args)
        {
            if (!GameSystems.Combat.IsCombatActive() && !GameSystems.Critter.IsDeadNullDestroyed(args.objHndCaller))
            {
                GameSystems.D20.Actions.TurnBasedStatusInit(args.objHndCaller);
                GameSystems.D20.Actions.CurSeqReset(args.objHndCaller);
                GameSystems.D20.Actions.GlobD20ActnInit();
                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.STAND_UP, 0);
                GameSystems.D20.Actions.ActionAddToSeq();
                GameSystems.D20.Actions.sequencePerform();
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100e9e50)]
        [TemplePlusLocation("poison.cpp:33")]
        public static void PoisonedOnAdd(in DispatcherCallbackArgs args)
        {
            var poisonId = args.GetConditionArg1();
            var poison = GameSystems.Poison.GetPoison(poisonId);
            if (poison == null)
            {
                args.RemoveThisCondition();
                return;
            }

            args.SetConditionArg2(10); // set initial 10 rounds countdown for secondary damage

            if (poison.ImmediateEffects.Count == 0)
            {
                return;
            }

            var dc = poison.DC;
            if (dc <= 0)
            {
                dc = args.GetConditionArg3();
            }

            var critter = args.objHndCaller;
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(20, critter,
                null); // [ACTOR] is ~poisoned~[TAG_POISON]!
            GameSystems.D20.Combat.FloatCombatLine(critter, 55); // Poisoned!
            if (GameSystems.D20.Combat.SavingThrow(critter, null, dc, SavingThrowType.Fortitude,
                D20SavingThrowFlag.POISON))
            {
                return;
            }

            if (critter.HasCondition("sp-Delay Poison"))
            {
                GameSystems.Spell.FloatSpellLine(critter, 20033,
                    TextFloaterColor.White); // Effects delayed due to Delay Poison!
                return;
            }

            foreach (var effect in poison.ImmediateEffects)
            {
                GameSystems.Poison.ApplyPoisonEffect(critter, effect);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e9500)]
        public static void StunnedInitiativeUpdate(in DispatcherCallbackArgs evt)
        {
            // NOTE: Removed data1/data2 because they only ever were 0 and 1
            var remaining = evt.GetConditionArg1(); // TODO This seemed borked in vanilla code

            // I believe this is the initiatve when the stun ocurred???
            var v5 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoD20Signal();

            // TODO: not clear what this does
            var nextInitiative = dispIo.data1;
            var currentInitiative = dispIo.data2;
            if (currentInitiative > nextInitiative)
            {
                if (currentInitiative <= v5 || v5 < nextInitiative)
                {
                    return;
                }
            }
            else if (nextInitiative > v5 && v5 >= currentInitiative)
            {
                return;
            }

            if (remaining > 1)
            {
                evt.SetConditionArg1(remaining - 1);
            }
            else
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100e7f80)]
        public static void BonusCap0Add(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddCap(0, 0, 109);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ec590)]
        public static void DominateIsCharmed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            dispIo.obj = evt.GetConditionObjArg(1);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e8230)]
        public static void ConditionDisabledHpChange(in DispatcherCallbackArgs evt)
        {
            var currentHp = evt.objHndCaller.GetStat(Stat.hp_current);
            var subdualDamage = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);
            var dispIo = evt.GetDispIoD20Signal();

            // TODO: I think this is wrong since the signal onyl has data1 set with the amount of damage dealt
            var v4 = dispIo.data1;
            var v5 = dispIo.data2;
            if (v5 >= 0)
            {
                if (v5 > 0 || v4 != 0)
                {
                    if (currentHp > 0)
                    {
                        evt.RemoveThisCondition();
                    }

                    return;
                }

                if (v5 >= 0)
                {
                    return;
                }
            }

            if (currentHp < 0)
            {
                if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.DIEHARD))
                {
                    evt.RemoveThisCondition();
                    evt.objHndCaller.AddCondition(Dying);
                }
            }
            else if (currentHp > 0)
            {
                if (subdualDamage < currentHp)
                {
                    evt.RemoveThisCondition();
                    evt.objHndCaller.AddCondition(Prone);
                }
                else
                {
                    evt.RemoveThisCondition();
                    evt.objHndCaller.AddCondition(Unconscious);
                }
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100eb530)]
        public static void DetectingEvilBeginRound(in DispatcherCallbackArgs evt)
        {
            var critter = evt.objHndCaller;
            var location = critter.GetLocationFull();

            var startAngle = critter.Rotation - Angles.ToRadians(45);
            var rangeInches = 60F * locXY.INCH_PER_FEET;

            using var critters = ObjList.ListCone(location, rangeInches, startAngle, Angles.ToRadians(90),
                ObjectListFilter.OLC_CRITTERS);

            foreach (var candidate in critters)
            {
                if (candidate.HasEvilAlignment())
                {
                    if (candidate != critter)
                    {
                        candidate.AddCondition(DetectedEvil, 1, 0);
                    }
                }
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100e9de0)]
        public static void ProneApplyACModifier(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v2 = dispIo.attackPacket.flags;
            if ((v2 & D20CAF.RANGED) != 0)
            {
                dispIo.bonlist.AddBonus(4, 0, 162);
            }
            else
            {
                dispIo.bonlist.AddBonus(-4, 0, 162);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eb220)]
        public static void IncubatingDiseaseOnAdd(in DispatcherCallbackArgs evt)
        {
            var diseaseId = evt.GetConditionArg2();
            if (evt.GetConditionArg1() <= 0)
            {
                var disease = GameSystems.Disease.GetDisease(diseaseId);
                var daysLeft = Dice.Roll(1, disease.DaysIncubation);
                evt.SetConditionArg3(daysLeft);
            }
            else
            {
                evt.RemoveThisCondition();
                evt.objHndCaller.AddCondition(NSDiseased, diseaseId, 0);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e9bc0)]
        public static void FlatfootedAooPossible(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.COMBAT_REFLEXES))
            {
                evt.GetDispIoD20Query().return_val = 0;
            }
        }

        [DispTypes(DispatcherType.Initiative)]
        [TempleDllLocation(0x100ec920)]
        public static void DyingBleedingOut(in DispatcherCallbackArgs evt)
        {
            if (Dice.D10.Roll() == 1)
            {
                DyingHealSkillUsed(in evt);
            }
            else
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0xE, evt.objHndCaller, null);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 27);
                var dice = Dice.Constant(1);
                GameSystems.D20.Combat.DoUnclassifiedDamage(evt.objHndCaller, null, dice, DamageType.BloodLoss,
                    D20AttackPower.UNSPECIFIED, D20ActionType.NONE);
            }
        }

        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100eb620)]
        public static void TempNegativeLevelNewday(in DispatcherCallbackArgs evt)
        {
            var amount = evt.GetConditionArg1();
            var dc = evt.GetConditionArg2();
            if (dc == 0)
            {
                dc = 14;
            }

            if (!GameSystems.D20.Combat.SavingThrow(evt.objHndCaller, null, dc, 0))
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
                evt.objHndCaller.AddCondition(PermNegativeLevel, amount, 0);
            }
        }

        [DispTypes(DispatcherType.ToHitBonusFromDefenderCondition)]
        [TempleDllLocation(0x100eaaf0)]
        public static void StunnedToHitBonus(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data, 0, 136);
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100e9d80)]
        public static void ChargingAcPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(-2, 0, 160);
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100e9490)]
        public static void ParalyzedOnBeginRound(in DispatcherCallbackArgs evt, int data)
        {
            var condArg1 = evt.GetConditionArg1();
            var newRounds = condArg1 - evt.GetDispIoD20Signal().data1;
            if (newRounds >= 0)
            {
                evt.SetConditionArg1(newRounds);
            }
            else
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100eb950)]
        public static void FeintingOpponentSneakAttack(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var dispIoDamage = (DispIoDamage) dispIo.obj;
            var victim = evt.GetConditionObjArg(0);
            if (victim == dispIoDamage.attackPacket.victim
                && (dispIoDamage.attackPacket.flags & D20CAF.RANGED) == 0)
            {
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ec330)]
        public static void ElixerTimedSkillBonusBeginRound(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var remainingRounds = condArg2 - evt.GetDispIoD20Signal().data1;
            if (remainingRounds >= 0)
            {
                evt.SetConditionArg2(remainingRounds);
            }
            else
            {
                evt.RemoveThisCondition();
                var partSys = evt.GetConditionPartSysArg(2);
                GameSystems.ParticleSys.Remove(partSys);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e95c0)]
        [TemplePlusLocation("condition.cpp:3592")]
        public static void DismissSignalHandler(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoD20Signal();
            var spellId = args.GetConditionArg1();
            // used to check dispIo.data1 == 0 too; doesn't seem to make sense, why would spellId be 0?
            if (dispIo.data2 == 0 && spellId == dispIo.data1)
            {
                if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt))
                {
                    args.RemoveThisCondition();
                    return;
                }

                if (spPkt.aoeObj != null && spPkt.aoeObj != args.objHndCaller)
                {
                    GameSystems.D20.D20SendSignal(spPkt.aoeObj, D20DispatcherKey.SIG_Dismiss_Spells, spPkt.spellId);
                }

                // Spell ObjectHandles. Added in Temple+ for Wall spells
                foreach (var spellObj in spPkt.spellObjs)
                {
                    if (spellObj.obj == null || spellObj.obj == args.objHndCaller)
                        continue;
                    GameSystems.D20.D20SendSignal(spellObj.obj, D20DispatcherKey.SIG_Dismiss_Spells, spPkt.spellId, 0);
                }

                foreach (var tgt in spPkt.Targets)
                {
                    if (tgt.Object == null || tgt.Object == args.objHndCaller)
                        continue;
                    GameSystems.D20.D20SendSignal(tgt.Object, D20DispatcherKey.SIG_Dismiss_Spells, spPkt.spellId, 0);
                }

                // in case the dismiss didn't take care of that (e.g. grease)
                if (spPkt.aoeObj != null)
                {
                    GameSystems.D20.D20SendSignal(spPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spPkt.spellId, 0);
                }

                // Spell ObjectHandles. Added in Temple+ for Wall spells
                foreach (var spellObj in spPkt.spellObjs)
                {
                    if (spellObj.obj == null)
                    {
                        continue;
                    }

                    GameSystems.D20.D20SendSignal(spellObj.obj, D20DispatcherKey.SIG_Spell_End, spPkt.spellId, 0);
                }

                // adding this speciically for grease because I want to be careful
                var SP_GREASE_ENUM = 200;
                if (spPkt.spellEnum == SP_GREASE_ENUM)
                {
                    args.RemoveThisCondition();
                }

                // By now all effects should have been removed. Cross your fingers!
                GameSystems.D20.D20SendSignal(args.objHndCaller, D20DispatcherKey.SIG_Spell_End, spPkt.spellId, 0);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ea630)]
        [TemplePlusLocation("generalfixes.cpp:321")]
        public static void InspiredCourageDamBon(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            if (evt.GetConditionArg2() == 0)
            {
                return;
            }

            var bonVal = 1;
            if (GameSystems.Party.IsInParty(evt.objHndCaller))
            {
                var brdLvl = 0;
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    var dudeBrdLvl = partyMember.GetStat(Stat.level_bard);
                    if (dudeBrdLvl > brdLvl)
                        brdLvl = dudeBrdLvl;
                }

                bonVal = GetInspireCourageBonus(brdLvl);
            }

            dispIo.damage.AddDamageBonus(bonVal, 13, 191);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e8010)]
        public static void DyingOnCombatEnd(in DispatcherCallbackArgs evt)
        {
            var currentHp = evt.objHndCaller.GetStat(Stat.hp_current);
            if (GameSystems.Party.IsInParty(evt.objHndCaller))
            {
                if (evt.subDispNode.condNode.condStruct != Unconscious)
                {
                    CommonConditionCallbacks.conditionRemoveCallback(in evt);
                    evt.objHndCaller.AddCondition(Unconscious);
                }
            }
            else if (currentHp < 0)
            {
                // TODO: It would make sense to retrieve the last-hit-critter here to correctly attribute the killer
                GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100eab60)]
        public static void BarbarianRageOnAdd(in DispatcherCallbackArgs evt)
        {
            var conMod = evt.objHndCaller.GetStat(Stat.con_mod);
            evt.SetConditionArg1(conMod + 5);
            GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.SkillBarbarianRage);
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100e9a70)]
        public static void AfflictedPreadd_100E9A70(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            if (evt.GetDispIoCondStruct().condStruct == data)
            {
                // TODO I think this actually does nothing today!!! data1 is the condstruct and is outside the range
                D20ModCountdownEndHandler(in evt, 0);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100e8650)]
        public static void QueryRetFalseIfNoFreedomOfMovement(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.return_val = 0;
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100ec1e0)]
        public static void BrawlTakingDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            if (dispIo.damage.GetOverallLethalDamage() > 0)
            {
                GameSystems.Combat.BrawlStatus = 2;
            }

            if (dispIo.attackPacket.weaponUsed != null)
            {
                GameSystems.Combat.BrawlStatus = 2;
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100ea670)]
        public static void SavingThrow_InspiredCourage_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.GetConditionArg2() != 0)
            {
                var flags = dispIo.flags;
                if ((flags & D20SavingThrowFlag.CHARM) != 0 || (flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) != 0)
                {
                    // TODO NOTE: There was a spellslinger fix here
                    var bonusValue = evt.GetConditionArg4();
                    dispIo.bonlist.AddBonus(bonusValue, 13, 191);
                }
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100ea560)]
        public static void BardicMusicInspireBeginRound(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var remainingRounds = evt.GetConditionArg1();
            var newRemainingRounds = remainingRounds - evt.GetDispIoD20Signal().data1;
            if (newRemainingRounds >= 0)
            {
                evt.SetConditionArg1(newRemainingRounds);
            }
            else
            {
                evt.RemoveThisCondition();
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ec5e0)]
        public static void DominateConditionAdd(in DispatcherCallbackArgs evt)
        {
            var caster = evt.GetConditionObjArg(1);
            GameSystems.Critter.AddFollower(evt.objHndCaller, caster, true, false);
            GameUiBridge.UpdateInitiativeUi();
        }

        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100ea450)]
        public static void DamageAbilityLossHealing(in DispatcherCallbackArgs evt)
        {
            var abilityDamage = evt.GetConditionArg2() - 1;
            if (abilityDamage > 0)
            {
                evt.SetConditionArg2(abilityDamage);
            }
            else
            {
                evt.RemoveThisCondition();
            }

            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100ea960)]
        [TemplePlusLocation("condition.cpp:489")]
        public static void BardicGreatnessTakingDamage2(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var dam = dispIo.damage;
            ApplyTemporaryHitpoints(in evt, dam, 0, 3);
        }

        private static void ApplyTemporaryHitpoints(in DispatcherCallbackArgs evt, DamagePacket dam, int signalArg,
            int tempHpArg)
        {
            var critter = evt.objHndCaller;
            var finalDam = dam.finalDamage;
            var tempHp = evt.GetConditionArg(tempHpArg);

            if (tempHp > 0 && finalDam > 0)
            {
                Logger.Info("took {0} damage, temp_hp = {1}", finalDam, tempHp);

                var hpLeft = tempHp - finalDam;
                if (tempHp - finalDam <= 0)
                {
                    hpLeft = 0;
                }

                Logger.Info("({0}) temp_hp left", hpLeft);
                evt.SetConditionArg(tempHpArg, hpLeft);
                if (tempHp - finalDam > 0)
                {
                    dam.AddDamageBonus(-finalDam, 0, 154);
                    dam.finalDamage = 0;
                    Logger.Info(", absorbed {0} points of damage", finalDam);

                    var extraString = $"[{finalDam}] ";
                    GameSystems.D20.Combat.FloatCombatLine(critter, 50, extraString);
                }
                else
                {
                    var dmgRemaining = finalDam - tempHp;
                    Logger.Info(", taking modified damage {0}", dmgRemaining);
                    var extraString = $"[{tempHp}] ";
                    GameSystems.D20.Combat.FloatCombatLine(critter, 50, extraString);
                    dam.AddDamageBonus(-tempHp, 0, 154);

                    var condArg1 = evt.GetConditionArg(signalArg);
                    GameSystems.D20.D20SendSignal(critter, D20DispatcherKey.SIG_Temporary_Hit_Points_Removed,
                        condArg1, 0);
                    dam.finalDamage = dmgRemaining;
                }
            }
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100ea270)]
        public static void AbilityDamageStatLevel(in DispatcherCallbackArgs evt)
        {
            var affectedStat = (Stat) evt.GetConditionArg1();
            var amount = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoBonusList();
            if (evt.GetAttributeFromDispatcherKey() == affectedStat)
            {
                dispIo.bonlist.AddBonus(-amount, 0, 179);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ea850)]
        public static void BardicSuggestionAdd(in DispatcherCallbackArgs evt, int data)
        {
            var bardLvl = evt.objHndCaller.GetStat(Stat.level_bard);
            evt.SetConditionArg1(bardLvl);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ec6b0)]
        public static void QueryIsAiControlled(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            // TODO: I think this is an object result
            dispIo.data1 = evt.GetConditionArg2();
            dispIo.data2 = evt.GetConditionArg1();
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(269386560)]
        public static void DisabledSequence(in DispatcherCallbackArgs evt)
        {
            // Looks for an action in the sequence that is exhausting to perform
            var exhausting = false;
            var sequence = (ActionSequence) evt.GetDispIoD20Signal().obj;

            foreach (var action in sequence.d20ActArray)
            {
                // TODO: Move this as a flag into the action definition
                switch (action.d20ActType)
                {
                    default:
                        exhausting = true;
                        break;
                    case D20ActionType.FULL_ATTACK:
                    case D20ActionType.RELOAD:
                    case D20ActionType.FIVEFOOTSTEP:
                    case D20ActionType.MOVE:
                    case D20ActionType.DOUBLE_MOVE:
                    case D20ActionType.STAND_UP:
                    case D20ActionType.PICKUP_OBJECT:
                    case D20ActionType.DETECT_EVIL:
                    case D20ActionType.STOP_CONCENTRATION:
                    case D20ActionType.BREAK_FREE:
                    case D20ActionType.REMOVE_DISEASE:
                    case D20ActionType.ITEM_CREATION:
                    case D20ActionType.TRACK:
                    case D20ActionType.OPEN_INVENTORY:
                    case D20ActionType.DISABLE_DEVICE:
                    case D20ActionType.SEARCH:
                    case D20ActionType.SNEAK:
                    case D20ActionType.TALK:
                    case D20ActionType.OPEN_LOCK:
                    case D20ActionType.READY_SPELL:
                    case D20ActionType.READY_COUNTERSPELL:
                    case D20ActionType.READY_ENTER:
                    case D20ActionType.READY_EXIT:
                    case D20ActionType.DISMISS_SPELLS:
                        break;
                }
            }

            if (exhausting)
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(18, evt.objHndCaller, null);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 28);
                var dice = Dice.Constant(1);
                GameSystems.D20.Combat.DoUnclassifiedDamage(evt.objHndCaller, null, dice, DamageType.BloodLoss,
                    D20AttackPower.UNSPECIFIED, D20ActionType.NONE);
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100e8fd0)]
        public static void Temporary_Hit_Points_Disp15h_Taking_Damage(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            ApplyTemporaryHitpoints(evt, dispIo.damage, 0, 2);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec700)]
        public static void AiControlledKilled(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == condArg2 && dispIo.data2 == condArg1)
            {
                CommonConditionCallbacks.conditionRemoveCallback(evt.WithoutIO);
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100ea920)]
        public static void BardicGreatnessSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.GetConditionArg2() != 0)
            {
                dispIo.bonlist.AddBonus(1, 34, 194);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100e8710)]
        public static void InvisibleOnAdd_SetFade128(in DispatcherCallbackArgs evt)
        {
            var targetOpacity = evt.GetConditionArg3();
            if (targetOpacity == 0)
            {
                targetOpacity = 128;
            }

            GameSystems.ObjFade.FadeTo(evt.objHndCaller, targetOpacity, 0, 5, 0);
        }


        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100e8ca0)]
        public static void InvisibleDefenderConcealmentMissChance(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (GameSystems.Critter.IsOoze(dispIo.attackPacket.attacker))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(288);
            }
            else if (evt.objHndCaller.HasCondition(SpellEffects.SpellGlitterdust))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(220);
            }
            else if (evt.objHndCaller.HasCondition(SpellEffects.SpellFaerieFire))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(239);
            }
            else if (evt.objHndCaller.HasCondition(SpellEffects.SpellInvisibilityPurgeHit))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(247);
            }
            else if (!GameSystems.D20.D20Query(dispIo.attackPacket.victim,
                D20DispatcherKey.QUE_Critter_Can_See_Invisible))
            {
                var spellId = evt.GetConditionArg1();
                var attacker = dispIo.attackPacket.attacker;
                if (attacker != null && !ConditionalInvisibilityApplies(spellId, attacker))
                {
                    return;
                }

                dispIo.bonlist.AddBonus(data, 8, 161);
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100e9ce0)]
        public static void TotalDefenseCallback(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoAttackBonus();

            dispIo.bonlist.AddBonus(4, 8, 158);
            if (GameSystems.Skill.GetSkillRanks(args.objHndCaller, SkillId.tumble) > 5)
            {
                dispIo.bonlist.AddBonus(2, 8, 159);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ea1f0)]
        [TemplePlusLocation("condition.cpp:316")]
        public static void ApplyTempAbilityLoss(in DispatcherCallbackArgs args, ConditionSpec data)
        {
            var statDamaged = (Stat) args.GetConditionArg1();
            var amountDamaged = args.GetConditionArg2();
            var dispIo = args.GetDispIoCondStruct();

            // The new condition is of the same type, and it also applies to the same stat.
            if (dispIo.condStruct == data && (Stat) dispIo.arg1 == statDamaged)
            {
                int amountDamagedByNew = dispIo.arg2;
                int scoreLevel = args.objHndCaller.GetStat(statDamaged);
                if (scoreLevel - amountDamagedByNew < 0)
                {
                    amountDamagedByNew = scoreLevel;
                    if (amountDamagedByNew <= 0)
                    {
                        dispIo.outputFlag = false;
                        return;
                    }
                }

                amountDamaged += amountDamagedByNew;
                args.SetConditionArg2(amountDamaged);
                dispIo.outputFlag = false;
                GameSystems.Critter.CritterHpChanged(args.objHndCaller, null, 0);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ec270)]
        public static void ElixerTimedSkillBonusGuard(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (evt.GetDispIoCondStruct().arg1 == condArg1)
            {
                evt.RemoveThisCondition();
                var partSys = evt.GetConditionPartSysArg(2);
                GameSystems.ParticleSys.Remove(partSys);
            }
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100e8680)]
        public static void TooltipSimpleCallback(in DispatcherCallbackArgs evt, int data)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                var dispIo = evt.GetDispIoTooltip();
                var text = GameSystems.D20.Combat.GetCombatMesLine(data);
                dispIo.Append(text);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ea5f0)]
        [TemplePlusLocation("generalfixes.cpp:320")]
        public static void InspiredCourageToHitBon(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoAttackBonus();
            if (args.GetConditionArg2() == 0)
            {
                return;
            }

            var bonVal = 1;
            if (GameSystems.Party.IsInParty(args.objHndCaller))
            {
                var brdLvl = 0;

                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    var dudeBrdLvl = partyMember.GetStat(Stat.level_bard);
                    if (dudeBrdLvl > brdLvl)
                    {
                        brdLvl = dudeBrdLvl;
                    }
                }

                bonVal = GetInspireCourageBonus(brdLvl);
            }

            dispIo.bonlist.AddBonus(bonVal, 13, 191);
        }

        private static int GetInspireCourageBonus(int brdLvl)
        {
            int bonVal;
            if (brdLvl < 8)
                bonVal = 1;
            else if (brdLvl < 14)
                bonVal = 2;
            else if (brdLvl < 20)
                bonVal = 3;
            else
                bonVal = 4;
            return bonVal;
        }

        private static bool ConditionalInvisibilityApplies(int spellId, GameObjectBody observer)
        {
            if (spellId == 0 || !GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return true;
            }

            if (spellPkt.spellEnum == WellKnownSpells.InvisibilityToAnimals)
            {
                return GameSystems.Critter.IsAnimal(observer);
            }

            if (spellPkt.spellEnum == WellKnownSpells.InvisibilityToUndead)
            {
                return GameSystems.Critter.IsUndead(observer);
            }

            return true;
        }

        [DispTypes(DispatcherType.AcModifyByAttacker)]
        [TempleDllLocation(0x100e8a80)]
        public static void InvisibilityAcBonus2Cap(in DispatcherCallbackArgs args)
        {
            DispIoAttackBonus dispIo;
            int condArg1;
            SpellPacketBody spellPkt;

            dispIo = args.GetDispIoAttackBonus();
            if (GameSystems.Feat.HasFeat(dispIo.attackPacket.victim, FeatId.BLIND_FIGHT))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(164);
            }
            else if (GameSystems.D20.D20Query(dispIo.attackPacket.victim,
                D20DispatcherKey.QUE_Critter_Can_See_Invisible))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(163);
            }
            else if (GameSystems.Feat.HasFeat(dispIo.attackPacket.victim, FeatId.UNCANNY_DODGE))
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(165);
            }
            else
            {
                var spellId = args.GetConditionArg1();
                if (!ConditionalInvisibilityApplies(spellId, dispIo.attackPacket.victim))
                {
                    return;
                }

                if (args.objHndCaller.HasCondition(SpellEffects.SpellGlitterdust))
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(221);
                }
                else if (args.objHndCaller.HasCondition(SpellEffects.SpellFaerieFire))
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(240);
                }
                else if (args.objHndCaller.HasCondition(SpellEffects.SpellInvisibilityPurgeHit))
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(248);
                }
                else
                {
                    dispIo.bonlist.AddCap(8, 0, 161);
                    dispIo.bonlist.AddCap(3, 0, 0xA1);
                }
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e9ac0)]
        public static void sub_100E9AC0(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var action = (D20Action) dispIo.obj;
            if ((action.d20Caf & D20CAF.HIT) != 0)
            {
                Logger.Info("Touch Attack Hit");
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
            else
            {
                Logger.Info("Touch Attack Miss");
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ea8e0)]
        public static void BardicGreatnessToHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (evt.GetConditionArg2() != 0)
            {
                dispIo.bonlist.AddBonus(2, 34, 194);
            }
        }

        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100ea3f0)]
        public static void TempAbilityLossHeal(in DispatcherCallbackArgs evt)
        {
            var remainingDamage = evt.GetConditionArg2() - 2; // TODO: Why -2 ??? (But it stores -1???)
            if (remainingDamage > 0)
            {
                evt.SetConditionArg2(remainingDamage - 1);
            }
            else
            {
                evt.RemoveThisCondition();
            }

            GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100ea7d0)]
        public static void BardicEffectPreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                evt.SetConditionArg1(1);
                dispIo.outputFlag = false;
            }
        }


        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100ec850)]
        public static void EffectTooltipPoison(in DispatcherCallbackArgs evt, int data)
        {
            var poisonId = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoEffectTooltip();
            var poison = GameSystems.Poison.GetPoison(poisonId);
            var poisonName = GameSystems.Poison.GetPoisonName(poison);

            var suffix = $": {poisonName}";
            dispIo.bdb.AddEntry(BuffDebuffType.Debuff, data, suffix);
        }

        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100ec770)]
        public static void EffectTooltipDiseased(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoEffectTooltip();

            var diseaseId = evt.GetConditionArg1();
            var disease = GameSystems.Disease.GetDisease(diseaseId);
            var diseaseName = GameSystems.Disease.GetName(disease);

            var extraString = $": {diseaseName}";

            dispIo.bdb.AddEntry(BuffDebuffType.Debuff, data, extraString);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100e9680)]
        public static void D20ModsSpellEndHandler(in DispatcherCallbackArgs evt, int data1, ConditionSpec data2)
        {
            // check correct spellId
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == evt.GetConditionArg1() && dispIo.data2 == 0)
            {
                RemoveStatusEffect(in evt, data1, dispIo);
            }
        }

        /// <summary>
        /// This removes the domination effect when the dominating critter is killed.
        /// </summary>
        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ec630)]
        public static void DominateCritterKilled(in DispatcherCallbackArgs evt)
        {
            var charmedBy = evt.GetConditionObjArg(1);
            var dispIo = evt.GetDispIoD20Signal();
            var killedCritter = (GameObjectBody) dispIo.obj;

            if (charmedBy == killedCritter)
            {
                // TODO: This is borked because it overlaps with the charming character
                var partSys = evt.GetConditionPartSysArg(2);
                GameSystems.ParticleSys.End(partSys);
                CommonConditionCallbacks.conditionRemoveCallback(evt.WithoutIO);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100eafd0)]
        public static void SmiteEvilToHitBonus(in DispatcherCallbackArgs evt)
        {
            var v1 = evt.objHndCaller.GetStat(Stat.charisma);
            var v2 = D20StatSystem.GetModifierForAbilityScore(v1);
            var dispIo = evt.GetDispIoAttackBonus();
            var v4 = GameSystems.Feat.GetFeatName(FeatId.SMITE_EVIL);
            dispIo.bonlist.AddBonus(v2, 0, 114, v4);
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100e8420)]
        public static void StunningFistDamage(in DispatcherCallbackArgs evt)
        {
            // TODO: Why is this not using the monk level...?
            var lvl = evt.objHndCaller.DispatchGetLevel((int) Stat.level, BonusList.Create(), null);
            if (lvl < 1)
            {
                lvl = 1;
            }

            var wisScore = evt.objHndCaller.GetStat(Stat.wisdom);
            var wisMod = D20StatSystem.GetModifierForAbilityScore(wisScore) + lvl / 2 + 10;
            var dispIo = evt.GetDispIoDamage();
            var initiative = GameSystems.D20.Initiative.GetInitiative(evt.objHndCaller);
            if (!GameSystems.D20.Combat.SavingThrow(dispIo.attackPacket.victim, dispIo.attackPacket.attacker, wisMod, 0))
            {
                dispIo.attackPacket.victim.AddCondition(Stunned, 1, initiative);
            }

            CommonConditionCallbacks.conditionRemoveCallback(in evt);
        }
    }
}
