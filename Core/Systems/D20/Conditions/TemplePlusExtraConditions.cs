using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    /// <summary>
    /// Contains stuff from conditions.cpp function AddConditionsToTable.
    /// </summary>
    [AutoRegister]
    public static class TemplePlusExtraConditions
    {
        public static readonly ConditionSpec SpecialEquipmentSkillBonus = ConditionSpec
            .Create("Special Equipment Skill Bonus", 3)
            .AddSkillLevelHandler(SkillId.appraise, ItemEffects.SkillBonusCallback, 99)
            .Build();

        public static readonly ConditionSpec Ethereal = ConditionSpec
            .Create("Ethereal", 3)
            .SetUnique()
            .SetQueryResult(D20DispatcherKey.QUE_Is_Ethereal, true)
            .AddHandler(DispatcherType.BeginRound, CountDownEthereal)
            .AddHandler(DispatcherType.TakingDamage2, AddEtherealDamageImmunity)
            .AddHandler(DispatcherType.DealingDamage2, NullifyEtherealDamageDealt)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .SetQueryResult(D20DispatcherKey.QUE_AOOWillTake, false)
            .AddHandler(DispatcherType.EffectTooltip, EtherealTooltip)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Invisible, true)
            .AddHandler(DispatcherType.ConditionAdd, InitEthereal)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, InitEtherealFromD20Status)
            .AddHandler(DispatcherType.ConditionRemove, EtherealOnRemove)
            .AddUniqueTooltip(210)
            .SetQueryResult(D20DispatcherKey.QUE_IsActionInvalid_CheckAction, true)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOIncurs, false)
            .SetQueryResult(D20DispatcherKey.QUE_ActionTriggersAOO, false)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
            .SetQueryResult(D20DispatcherKey.QUE_CanBeAffected_PerformAction, false)
            .SetQueryResult(D20DispatcherKey.QUE_CanBeAffected_ActionFrame, false)
            .Build();

        [TemplePlusLocation("GenericCallbacks::EtherealOnAdd")]
        private static void InitEthereal(in DispatcherCallbackArgs evt)
        {
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 210); // Ethereal
            InitEtherealFromD20Status(in evt);
        }

        [TemplePlusLocation("GenericCallbacks::EtherealOnD20StatusInit")]
        private static void InitEtherealFromD20Status(in DispatcherCallbackArgs evt)
        {
            // TODO: This approach sucks because it ignores multiple sources of transparency
            evt.objHndCaller.FadeTo(70, 10, 30);
        }

        [TemplePlusLocation("GenericCallbacks::EtherealOnRemove")]
        private static void EtherealOnRemove(in DispatcherCallbackArgs evt)
        {
            // TODO: This approach sucks because it ignores multiple sources of transparency
            evt.objHndCaller.FadeTo(255, 0, 5);
        }

        [TemplePlusLocation("ClassAbilityCallbacks::TimedEffectCountdown")]
        private static void CountDownEthereal(in DispatcherCallbackArgs evt)
        {
            var numRoundsRem = evt.GetConditionArg3();
            if (numRoundsRem <= 1)
            {
                evt.RemoveThisCondition();
            }
            else
            {
                evt.SetConditionArg3(numRoundsRem - 1);
            }
        }

        [TemplePlusLocation("GenericCallbacks::AddEtherealDamageImmunity")]
        private static void AddEtherealDamageImmunity(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddEtherealImmunity();
        }

        [TemplePlusLocation("GenericCallbacks::EtherealDamageDealingNull")]
        private static void NullifyEtherealDamageDealt(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddEtherealImmunity();
        }

        [TemplePlusLocation("GenericCallbacks::EffectTooltipDuration")]
        private static void EtherealTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            var numRounds = evt.GetConditionArg3();

            var etherealText = GameSystems.D20.Combat.GetCombatMesLine(210);
            var durationText = GameSystems.D20.Combat.GetCombatMesLine(175);
            var text = $"{etherealText}\n{durationText}: {numRounds}";
            dispIo.bdb.AddEntry(82, text);
        }

    }
}