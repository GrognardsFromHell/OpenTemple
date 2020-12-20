using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    // Oaken Resilience: Complete Divine, p. 82
    public class OakenResilience
    {
        private static readonly D20DispatcherKey OakenResilienceEnum = (D20DispatcherKey) 2701;

        public static void OakenResilienceRadial(in DispatcherCallbackArgs evt)
        {
            // adds the "Oaken Resilience" condition on first radial menu build
            var isAdded = evt.objHndCaller.AddCondition("Oaken Resilience Effect", 0, 0);
            var radialAction = RadialMenuEntry.CreatePythonAction(OakenResilienceEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void OnOakenResilienceEffectCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Don't allow if the effect is already enabled
            if (evt.GetConditionArg1() != 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            // Get the current number of wild shape charges
            var WildShapeCharges = evt.objHndCaller.GetWildshapeCharges();
            // Check for remaining wild shape attempts
            if ((WildShapeCharges < 1))
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                return;
            }
        }

        public static void OnOakenResilienceEffectPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Deduct a wild shape charge
            evt.objHndCaller.D20SendSignal("Deduct Wild Shape Charge");
            // Set to active
            evt.SetConditionArg1(1);
            // Lasts 10 minute duration in rounds
            evt.SetConditionArg2(100);
            return;
        }
        public static void OakenResilienceEffectBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // not active, nothing more to do
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // calculate the number of rounds left
            var numRounds = evt.GetConditionArg2();
            var roundsToReduce = dispIo.data1;
            if (numRounds - roundsToReduce > 0)
            {
                evt.SetConditionArg2(numRounds - roundsToReduce); // decrement the number of rounds
            }
            else
            {
                evt.SetConditionArg1(0); // set effect to inactive
            }

            return;
        }
        public static void OakenResilienceEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            dispIo.Append("Oaken Resilience");
            return;
        }
        public static void OakenResilienceEffectTooltipEffect(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(ElfHash.Hash("OAKEN_RESILIENCE"), $" ({evt.GetConditionArg2()} rounds)", -2);
        }

        public static void OakenResiliencePoisonImmunity(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            dispIo.return_val = 1;
            return;
        }

        public static void OakenResilienceCriticalImmunity(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            dispIo.return_val = 1;
            return;
        }

        public static void OakenResilienceAddPre(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var IsSleep = dispIo.condStruct == SpellEffects.SpellSleep;
            if (IsSleep)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatMesFileLine("mes/combat.mes", 5059, TextFloaterColor.Red); // "Sleep Immunity"
                                                                                                 /*FIXME*/
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(31, evt.objHndCaller, null);
                return;
            }

            var IsParalyzed = dispIo.condStruct == StatusEffects.Paralyzed;
            if (IsParalyzed)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatLine("Paralysis Immunity", TextFloaterColor.Red);
                return;
            }

            var IsStunned = dispIo.condStruct == StatusEffects.Stunned;
            if (IsStunned)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatLine("Stun Immunity", TextFloaterColor.Red);
                return;
            }

            // Don't know of any polymorph effects for providing immunity
            return;
        }

        public static void OakenResilienceTripDefenseBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Check that this is a trip before applying the bonus
            if (((dispIo.flags & SkillCheckFlags.UnderDuress) != 0 && (dispIo.flags & SkillCheckFlags.Unk2) != 0))
            {
                dispIo.bonlist.AddBonus(8, 0, "Oaken Resilience"); // untyped
            }
        }

        // Setup the feat
        // spare, spare
        [FeatCondition("Oaken Resilience")]
        [AutoRegister] public static readonly ConditionSpec OakenResilienceFeat = ConditionSpec.Create("Oaken Resilience Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, OakenResilienceRadial)
            .Build();

        // Setup the effect
        // active, rounds left, spare, spare
        [AutoRegister] public static readonly ConditionSpec OakenResilienceEffect = ConditionSpec.Create("Oaken Resilience Effect", 4)
            .SetUnique()
            .AddHandler(DispatcherType.BeginRound, OakenResilienceEffectBeginRound)
            .AddHandler(DispatcherType.PythonActionCheck, OakenResilienceEnum, OnOakenResilienceEffectCheck)
            .AddHandler(DispatcherType.PythonActionPerform, OakenResilienceEnum, OnOakenResilienceEffectPerform)
            .AddHandler(DispatcherType.Tooltip, OakenResilienceEffectTooltip)
            .AddHandler(DispatcherType.EffectTooltip, OakenResilienceEffectTooltipEffect)
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, OakenResilienceCriticalImmunity)
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Is_Immune_Poison, OakenResiliencePoisonImmunity)
            .AddHandler(DispatcherType.ConditionAddPre, OakenResilienceAddPre)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_DEXTERITY, OakenResilienceTripDefenseBonus)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH, OakenResilienceTripDefenseBonus)
            .Build();
    }
}
