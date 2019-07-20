using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public delegate ActionErrorCode AddToSeqCallback(D20Action d20a, ActionSequence actSeq, TurnBasedStatus tbStat);

    public delegate ActionErrorCode TurnBasedStatusCheckCallback(D20Action d20a, TurnBasedStatus tbStat);

    public delegate ActionErrorCode ActionCheckCallback(D20Action d20a, TurnBasedStatus tbStat);

    public delegate ActionErrorCode TgtCheckCallback(D20Action d20a, TurnBasedStatus tbStat);

    // also seems to double as a generic check function (e.g. for move silently it checks if combat is active and nothing to do with location)
    public delegate ActionErrorCode LocCheckCallback(D20Action d20a, TurnBasedStatus tbStat, LocAndOffsets locAndOff);

    public delegate ActionErrorCode PerformCallback(D20Action d20a);

    public delegate bool ActionFrameCallback(D20Action d20a);

    public delegate bool ProjectileHitCallback(D20Action d20a, GameObjectBody projectile, GameObjectBody ammoItem);

    public delegate ActionErrorCode ActionCostCallback(D20Action d20a, TurnBasedStatus tbStat,
        ActionCostPacket actionCostPacket);

    public delegate int SeqRenderFuncCallback(D20Action d20a, int flags);

    public class D20ActionDef
    {
        public AddToSeqCallback addToSeqFunc;
        public TurnBasedStatusCheckCallback turnBasedStatusCheck;
        public ActionCheckCallback actionCheckFunc = (action, stat) => ActionErrorCode.AEC_OK;
        public TgtCheckCallback tgtCheckFunc;
        public LocCheckCallback locCheckFunc;
        public PerformCallback performFunc = action => ActionErrorCode.AEC_OK;
        public ActionFrameCallback actionFrameFunc;
        public ProjectileHitCallback projectileHitFunc;
        public ActionCostCallback actionCost;
        public SeqRenderFuncCallback seqRenderFunc;
        public D20ADF flags;
    }

    public static class D20ActionDefs
    {
        private static Dictionary<D20ActionType, D20ActionDef> actions;

        static D20ActionDefs()
        {
            actions = new Dictionary<D20ActionType, D20ActionDef>();

            AddVanillaActions(actions);

            ModVanillaActions(actions);

            AddTemplePlusActions(actions);
        }

        public static D20ActionDef GetActionDef(D20ActionType type) => actions[type];

        private static void AddVanillaActions(Dictionary<D20ActionType, D20ActionDef> actions)
        {
            actions[D20ActionType.UNSPECIFIED_MOVE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.UnspecifiedMoveAddToSeq,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_Movement | D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.UNSPECIFIED_ATTACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.UnspecifiedAttackAddToSeq,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.STANDARD_ATTACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.StandardAttackAddToSeq,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.StandardAttackTBStatusCheck,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionVanillaCallbacks.StandardAttackPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.StandardAttackActionFrame,
                actionCost = D20ActionVanillaCallbacks.StandardAttackActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.AttackSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_Unk20 | D20ADF.D20ADF_TriggersAoO |
                        D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_UseCursorForPicking |
                        D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.FULL_ATTACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.FullAttackPerform,
                actionCost = D20ActionVanillaCallbacks.FullAttackActionCost,
                flags = D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_SimulsCompatible |
                        D20ADF.D20ADF_DoLocationCheckAtDestination
            };


            actions[D20ActionType.STANDARD_RANGED_ATTACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.StandardAttackTBStatusCheck,
                actionCheckFunc = D20ActionVanillaCallbacks.RangedAttackActionCheck,
                performFunc = D20ActionVanillaCallbacks.RangedAttackPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.RangedAttackActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.RangedAttackProjectileHit,
                actionCost = D20ActionVanillaCallbacks.StandardAttackActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.AttackSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_QueryForAoO | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.RELOAD] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.ReloadPerform,
                actionFrameFunc = action =>
                {
                    D20ActionVanillaCallbacks.ReloadPerform(action);
                    return false;
                },
                actionCost = D20ActionVanillaCallbacks.ReloadActionCost,
                flags = D20ADF.D20ADF_QueryForAoO | D20ADF.D20ADF_SimulsCompatible |
                        D20ADF.D20ADF_DoLocationCheckAtDestination | D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.FIVEFOOTSTEP] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddMovement,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.FivefootstepTBStatusCheck,
                actionCheckFunc = D20ActionVanillaCallbacks.FivefootstepActionCheck,
                performFunc = D20ActionVanillaCallbacks.RunPerform,
                actionCost = D20ActionVanillaCallbacks.FivefootstepActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.MovementSequenceRender,
                flags = D20ADF.D20ADF_Unk1 | D20ADF.D20ADF_Unk2 | D20ADF.D20ADF_Movement |
                        D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_DrawPathByDefault |
                        D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.MOVE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddMovement,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.MoveTBStatusCheck,
                actionCheckFunc = D20ActionVanillaCallbacks.MoveActionCheck,
                performFunc = D20ActionVanillaCallbacks.MovePerform,
                actionCost = D20ActionVanillaCallbacks.MoveActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.MovementSequenceRender,
                flags = D20ADF.D20ADF_Unk1 | D20ADF.D20ADF_Unk2 | D20ADF.D20ADF_Movement |
                        D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_DrawPathByDefault |
                        D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.DOUBLE_MOVE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddMovement,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.DoubleMoveTBStatusCheck,
                actionCheckFunc = D20ActionVanillaCallbacks.DoubleMoveActionCheck,
                performFunc = D20ActionVanillaCallbacks.DoubleMovePerform,
                actionCost = D20ActionVanillaCallbacks.DoubleMoveActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.MovementSequenceRender,
                flags = D20ADF.D20ADF_Unk1 | D20ADF.D20ADF_Unk2 | D20ADF.D20ADF_Movement |
                        D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_DrawPathByDefault
            };


            actions[D20ActionType.RUN] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddMovement,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.RunTBStatusCheck,
                actionCheckFunc = D20ActionVanillaCallbacks.RunActionCheck,
                performFunc = D20ActionVanillaCallbacks.RunPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostRun,
                seqRenderFunc = D20ActionVanillaCallbacks.MovementSequenceRender,
                flags = D20ADF.D20ADF_Unk1 | D20ADF.D20ADF_Unk2 | D20ADF.D20ADF_Movement |
                        D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_DrawPathByDefault
            };


            actions[D20ActionType.CAST_SPELL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithSpell,
                actionCheckFunc = D20ActionVanillaCallbacks.CastSpellActionCheck,
                tgtCheckFunc = D20ActionVanillaCallbacks.CastSpellTargetCheck,
                performFunc = D20ActionVanillaCallbacks.CastSpellPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.CastSpellActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.CastSpellProjectileHit,
                actionCost = D20ActionVanillaCallbacks.ActionCostSpell,
                seqRenderFunc = D20ActionVanillaCallbacks.CastSpellSequenceRender,
                flags = D20ADF.D20ADF_MagicEffectTargeting | D20ADF.D20ADF_QueryForAoO |
                        D20ADF.D20ADF_DoLocationCheckAtDestination
            };


            actions[D20ActionType.HEAL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.HealPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.HealActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                seqRenderFunc = D20ActionVanillaCallbacks.HealSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.CLEAVE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.StandardAttackPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.StandardAttackActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.ATTACK_OF_OPPORTUNITY] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionVanillaCallbacks.AttackOfOpportunityPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.StandardAttackActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.WHIRLWIND_ATTACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.WhirlwindAttackAddToSeq,
                turnBasedStatusCheck = (a, stat) => ActionErrorCode.AEC_OK,
                actionCheckFunc = D20ActionVanillaCallbacks.TargetCheckActionValid,
                
                actionCost = D20ActionVanillaCallbacks.WhirlwindAttackActionCost,
                flags = D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.TOUCH_ATTACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.TouchAttackAddToSeq,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.StandardAttackTBStatusCheck,
                locCheckFunc = D20ActionVanillaCallbacks._Check_StandardAttack,
                performFunc = D20ActionVanillaCallbacks.TouchAttackPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.TouchAttackActionFrame,
                actionCost = D20ActionVanillaCallbacks.StandardAttackActionCost,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.TOTAL_DEFENSE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.TotalDefensePerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.CHARGE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ChargeAddToSeq,
                tgtCheckFunc = D20ActionVanillaCallbacks.TargetCheckActionValid,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.ChargePerform,
                actionFrameFunc = D20ActionVanillaCallbacks.ChargeActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostRun,
                flags = D20ADF.D20ADF_Unk1 | D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersAoO |
                        D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_Unk4000 | D20ADF.D20ADF_UseCursorForPicking |
                        D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_DrawPathByDefault |
                        D20ADF.D20ADF_DoLocationCheckAtDestination
            };


            actions[D20ActionType.FALL_TO_PRONE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.FallToProneActionCheck,
                performFunc = D20ActionVanillaCallbacks.FallToPronePerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.STAND_UP] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.StandUpActionCheck,
                performFunc = D20ActionVanillaCallbacks.StandUpPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostMoveAction,
                flags = D20ADF.D20ADF_QueryForAoO | D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.TURN_UNDEAD] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.PerformActionClericPaladin,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_TriggersCombat
            };


            actions[D20ActionType.DEATH_TOUCH] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.TouchAttackPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_Unk4000 |
                        D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.PROTECTIVE_WARD] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.PerformActionClericPaladin,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_TargetSingleIncSelf | D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.FEAT_OF_STRENGTH] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.PerformActionClericPaladin,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_SimulsCompatible | D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.BARDIC_MUSIC] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.PerformActionClericPaladin,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_QueryForAoO | D20ADF.D20ADF_TargetingBasedOnD20Data |
                        D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.PICKUP_OBJECT] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                tgtCheckFunc = D20ActionVanillaCallbacks.PickupObjectTargetCheck,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.PickupObjectPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostMoveAction,
                flags = D20ADF.D20ADF_UseCursorForPicking | D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.COUP_DE_GRACE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                tgtCheckFunc = D20ActionVanillaCallbacks.CoupDeGraceTargetCheck,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.CoupDeGracePerform,
                actionFrameFunc = D20ActionVanillaCallbacks.CoupDeGraceActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_QueryForAoO | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_Unk4000 | D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.USE_ITEM] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithSpell,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.UseItemPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.CastSpellActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.CastSpellProjectileHit,
                actionCost = D20ActionVanillaCallbacks.ActionCostSpell,
                seqRenderFunc = D20ActionVanillaCallbacks.UseItemSequenceRender,
                flags = D20ADF.D20ADF_MagicEffectTargeting
            };


            actions[D20ActionType.BARBARIAN_RAGE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
            };


            actions[D20ActionType.STUNNING_FIST] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSmite,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_Unk20 | D20ADF.D20ADF_Unk4000
            };


            actions[D20ActionType.SMITE_EVIL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSmite,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_TargetSingleIncSelf | D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.LAY_ON_HANDS_SET] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                seqRenderFunc = D20ActionVanillaCallbacks.CastSpellSequenceRender,
            };


            actions[D20ActionType.DETECT_EVIL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.PerformActionClericPaladin,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.STOP_CONCENTRATION] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.StopConcentrationPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
            };


            actions[D20ActionType.BREAK_FREE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.BreakFreePerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.TRIP] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.StandardAttackTBStatusCheck,
                actionCheckFunc = D20ActionVanillaCallbacks.TripActionCheck,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionVanillaCallbacks.TripPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.TripActionFrame,
                actionCost = D20ActionVanillaCallbacks.StandardAttackActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersAoO | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_UseCursorForPicking | D20ADF.D20ADF_SimulsCompatible
            };


            actions[D20ActionType.REMOVE_DISEASE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.ITEM_CREATION] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                locCheckFunc = D20ActionVanillaCallbacks.ItemCreationLocationCheck,
                performFunc = D20ActionVanillaCallbacks.ItemCreationPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
            };


            actions[D20ActionType.WHOLENESS_OF_BODY_SET] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
            };


            actions[D20ActionType.USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.UseMagicDeviceDecipherWrittenSpellPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
            };


            actions[D20ActionType.TRACK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
            };


            actions[D20ActionType.ACTIVATE_DEVICE_STANDARD] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.SPELL_CALL_LIGHTNING] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                locCheckFunc = D20ActionVanillaCallbacks.SpellCallLightningLocationCheck,
                performFunc = D20ActionVanillaCallbacks.SpellCallLightningPerform,
                actionFrameFunc = a => false,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_CallLightningTargeting
            };


            actions[D20ActionType.AOO_MOVEMENT] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                seqRenderFunc = D20ActionVanillaCallbacks.AooMovementSequenceRender,
                flags = D20ADF.D20ADF_Unk2 | D20ADF.D20ADF_Unk2000 | D20ADF.D20ADF_SimulsCompatible |
                        D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.CLASS_ABILITY_SA] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.ACTIVATE_DEVICE_FREE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
            };


            actions[D20ActionType.OPEN_INVENTORY] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.OpenInventoryPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.ACTIVATE_DEVICE_SPELL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithSpell,
                performFunc = D20ActionVanillaCallbacks.UseItemPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.CastSpellActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.CastSpellProjectileHit,
                actionCost = D20ActionVanillaCallbacks.ActionCostSpell,
                flags = D20ADF.D20ADF_MagicEffectTargeting
            };


            actions[D20ActionType.DISABLE_DEVICE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.DisableDevicePerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_UseCursorForPicking | D20ADF.D20ADF_TargetContainer
            };


            actions[D20ActionType.SEARCH] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.SearchPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
            };


            actions[D20ActionType.SNEAK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                locCheckFunc = D20ActionVanillaCallbacks.SneakLocationCheck,
                performFunc = D20ActionVanillaCallbacks.SneakPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
                flags = D20ADF.D20ADF_Breaks_Concentration
            };


            actions[D20ActionType.TALK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                actionCheckFunc = D20ActionVanillaCallbacks.TalkActionCheck,
                performFunc = D20ActionVanillaCallbacks.TalkPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
                seqRenderFunc = D20ActionVanillaCallbacks.TalkSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleExcSelf
            };


            actions[D20ActionType.OPEN_LOCK] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.OpenLockPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_UseCursorForPicking | D20ADF.D20ADF_TargetContainer
            };


            actions[D20ActionType.SLEIGHT_OF_HAND] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.SleightOfHandPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.OPEN_CONTAINER] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                locCheckFunc = D20ActionVanillaCallbacks.OpenContainerLocationCheck,
                performFunc = D20ActionVanillaCallbacks.OpenContainerPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.OpenContainerActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                seqRenderFunc = D20ActionVanillaCallbacks.OpenContainerSequenceRender,
                flags = D20ADF.D20ADF_UseCursorForPicking | D20ADF.D20ADF_TargetContainer
            };


            actions[D20ActionType.THROW] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ThrowAddToSeq,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.StandardAttackTBStatusCheck,
                locCheckFunc = (action, tbStatus, location) => D20ActionVanillaCallbacks.RangedAttackActionCheck(action, tbStatus),
                performFunc = D20ActionVanillaCallbacks.RangedAttackPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.RangedAttackActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.RangedAttackProjectileHit,
                actionCost = D20ActionVanillaCallbacks.StandardAttackActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.ThrowSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_Unk20 | D20ADF.D20ADF_QueryForAoO |
                        D20ADF.D20ADF_TriggersCombat
            };


            actions[D20ActionType.THROW_GRENADE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ThrowGrenadeAddToSeq,
                turnBasedStatusCheck = D20ActionVanillaCallbacks.StandardAttackTBStatusCheck,
                locCheckFunc = D20ActionVanillaCallbacks.ThrowGrenadeLocationCheck,
                performFunc = D20ActionVanillaCallbacks.ThrowGrenadePerform,
                actionFrameFunc = D20ActionVanillaCallbacks.ThrowGrenadeActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.ThrowGrenadeProjectileHit,
                actionCost = D20ActionVanillaCallbacks.StandardAttackActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.ThrowSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_Unk20 | D20ADF.D20ADF_QueryForAoO |
                        D20ADF.D20ADF_TriggersCombat
            };


            actions[D20ActionType.FEINT] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.FeintPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.FeintActionFrame,
                actionCost = D20ActionVanillaCallbacks.FeintActionCost,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.READY_SPELL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.ReadySpellPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.READY_COUNTERSPELL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.ReadyCounterspellPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.READY_ENTER] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.ReadyEnterPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.READY_EXIT] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.ReadyExitPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.COPY_SCROLL] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.CopyScrollActionCheck,
                performFunc = D20ActionVanillaCallbacks.CopyScrollPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.READIED_INTERRUPT] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.ReadiedInterruptPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
            };


            actions[D20ActionType.LAY_ON_HANDS_USE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithTarget,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                locCheckFunc = D20ActionVanillaCallbacks.LocationCheckWithinReach,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
                seqRenderFunc = D20ActionVanillaCallbacks.CastSpellSequenceRender,
                flags = D20ADF.D20ADF_TargetSingleIncSelf | D20ADF.D20ADF_UseCursorForPicking
            };


            actions[D20ActionType.WHOLENESS_OF_BODY_USE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.Dispatch37OnActionPerformSimple,
                actionFrameFunc = D20ActionVanillaCallbacks.Dispatch38OnActionFrame,
                actionCost = D20ActionVanillaCallbacks.ActionCostStandardAction,
            };


            actions[D20ActionType.DISMISS_SPELLS] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                performFunc = D20ActionVanillaCallbacks.DismissSpellsPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostNone,
            };


            actions[D20ActionType.FLEE_COMBAT] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddSimple,
                actionCheckFunc = D20ActionVanillaCallbacks.FleeCombatActionCheck,
                performFunc = D20ActionVanillaCallbacks.FleeCombatPerform,
                actionCost = D20ActionVanillaCallbacks.ActionCostFullRound,
                flags = D20ADF.D20ADF_QueryForAoO
            };


            actions[D20ActionType.USE_POTION] = new D20ActionDef
            {
                addToSeqFunc = D20ActionVanillaCallbacks.ActionSequencesAddWithSpell,
                actionCheckFunc = D20ActionVanillaCallbacks.Dispatch36D20ActnCheck_,
                performFunc = D20ActionVanillaCallbacks.UseItemPerform,
                actionFrameFunc = D20ActionVanillaCallbacks.CastSpellActionFrame,
                projectileHitFunc = D20ActionVanillaCallbacks.CastSpellProjectileHit,
                actionCost = D20ActionVanillaCallbacks.ActionCostSpell,
                seqRenderFunc = D20ActionVanillaCallbacks.UseItemSequenceRender,
                flags = D20ADF.D20ADF_MagicEffectTargeting | D20ADF.D20ADF_QueryForAoO
            };
        }

        private static void ModVanillaActions(Dictionary<D20ActionType, D20ActionDef> actions)
        {
            actions[D20ActionType.UNSPECIFIED_ATTACK].addToSeqFunc = D20ActionCallbacks.UnspecifiedAttackAddToSeq;

            actions[D20ActionType.STANDARD_ATTACK].performFunc = D20ActionCallbacks.PerformStandardAttack;
            actions[D20ActionType.STANDARD_ATTACK].actionFrameFunc = D20ActionCallbacks.ActionFrameStandardAttack;
            actions[D20ActionType.STANDARD_ATTACK].addToSeqFunc = D20ActionCallbacks.AddToStandardAttack;
            actions[D20ActionType.STANDARD_ATTACK].turnBasedStatusCheck =
                D20ActionCallbacks.StdAttackTurnBasedStatusCheck;
            actions[D20ActionType.STANDARD_ATTACK].actionCost = D20ActionCallbacks.ActionCostStandardAttack;
            actions[D20ActionType.STANDARD_ATTACK].actionCheckFunc = D20ActionCallbacks.ActionCheckStdAttack;

            actions[D20ActionType.STANDARD_RANGED_ATTACK].turnBasedStatusCheck =
                D20ActionCallbacks.StdAttackTurnBasedStatusCheck;
            actions[D20ActionType.STANDARD_RANGED_ATTACK].actionCost = D20ActionCallbacks.ActionCostStandardAttack;

            actions[D20ActionType.FULL_ATTACK].performFunc = D20ActionCallbacks.PerformFullAttack;
            actions[D20ActionType.FULL_ATTACK].actionCost = D20ActionCallbacks.ActionCostFullAttack;

            actions[D20ActionType.MOVE].flags &= ~D20ADF.D20ADF_Breaks_Concentration;

            actions[D20ActionType.FIVEFOOTSTEP].actionCheckFunc = D20ActionCallbacks.ActionCheckFiveFootStep;
            actions[D20ActionType.FIVEFOOTSTEP].flags &= ~D20ADF.D20ADF_Breaks_Concentration;

            actions[D20ActionType.RUN].flags |= D20ADF.D20ADF_Breaks_Concentration;

            actions[D20ActionType.CAST_SPELL].addToSeqFunc = D20ActionCallbacks.AddToSeqSpellCast;
            actions[D20ActionType.CAST_SPELL].performFunc = D20ActionCallbacks.PerformCastSpell;
            actions[D20ActionType.CAST_SPELL].actionCheckFunc = D20ActionCallbacks.ActionCheckCastSpell;
            actions[D20ActionType.CAST_SPELL].projectileHitFunc = D20ActionCallbacks.ProjectileHitSpell;
            actions[D20ActionType.CAST_SPELL].actionCost = D20ActionCallbacks.ActionCostCastSpell;
            // casting spells should break concentration since active concentration requires a standard action!
            actions[D20ActionType.CAST_SPELL].flags |= D20ADF.D20ADF_Breaks_Concentration;

            actions[D20ActionType.USE_ITEM].addToSeqFunc = D20ActionCallbacks.AddToSeqSpellCast;
            actions[D20ActionType.USE_ITEM].performFunc = D20ActionCallbacks.PerformUseItem;
            actions[D20ActionType.USE_ITEM].flags = D20ADF.D20ADF_QueryForAoO | D20ADF.D20ADF_MagicEffectTargeting;

            actions[D20ActionType.USE_POTION].addToSeqFunc = D20ActionCallbacks.AddToSeqSpellCast;
            actions[D20ActionType.USE_POTION].performFunc = D20ActionCallbacks.PerformUseItem;

            actions[D20ActionType.ACTIVATE_DEVICE_SPELL].addToSeqFunc = D20ActionCallbacks.AddToSeqSpellCast;
            actions[D20ActionType.ACTIVATE_DEVICE_SPELL].performFunc = D20ActionCallbacks.PerformUseItem;

            actions[D20ActionType.COPY_SCROLL].performFunc = D20ActionCallbacks.PerformCopyScroll;

            actions[D20ActionType.DISMISS_SPELLS].performFunc = D20ActionCallbacks.PerformDismissSpell;

            actions[D20ActionType.STOP_CONCENTRATION].performFunc = D20ActionCallbacks.PerformStopConcentration;

            actions[D20ActionType.BARDIC_MUSIC].flags =
                D20ADF.D20ADF_MagicEffectTargeting | D20ADF.D20ADF_Breaks_Concentration;

            actions[D20ActionType.TRIP].addToSeqFunc = D20ActionCallbacks.AddToSeqTripAttack;
            actions[D20ActionType.TRIP].actionCheckFunc = D20ActionCallbacks.ActionCheckTripAttack;
            actions[D20ActionType.TRIP].turnBasedStatusCheck = D20ActionCallbacks.StdAttackTurnBasedStatusCheck;
            actions[D20ActionType.TRIP].actionFrameFunc = D20ActionCallbacks.ActionFrameTripAttack;

            actions[D20ActionType.ATTACK_OF_OPPORTUNITY].performFunc = D20ActionCallbacks.PerformAoo;
            actions[D20ActionType.ATTACK_OF_OPPORTUNITY].actionFrameFunc = D20ActionCallbacks.ActionFrameAoo;

            actions[D20ActionType.CHARGE].performFunc = D20ActionCallbacks.PerformCharge;
            actions[D20ActionType.CHARGE].actionFrameFunc = D20ActionCallbacks.ActionFrameCharge;

            actions[D20ActionType.TOUCH_ATTACK].actionFrameFunc = D20ActionCallbacks.ActionFrameTouchAttack;

            actions[D20ActionType.PICKUP_OBJECT].flags |= D20ADF.D20ADF_TriggersAoO;

            actions[D20ActionType.SNEAK].actionCheckFunc = D20ActionCallbacks.ActionCheckSneak;
            actions[D20ActionType.SNEAK].locCheckFunc = null;
            actions[D20ActionType.SNEAK].actionCost = D20ActionCallbacks.ActionCostMoveAction;
            actions[D20ActionType.SNEAK].performFunc = D20ActionCallbacks.PerformSneak;

            actions[D20ActionType.WHIRLWIND_ATTACK].addToSeqFunc = D20ActionCallbacks.AddToSeqWhirlwindAttack;
            actions[D20ActionType.WHIRLWIND_ATTACK].actionCost = D20ActionCallbacks.ActionCostWhirlwindAttack;
        }

        private static void AddTemplePlusActions(Dictionary<D20ActionType, D20ActionDef> actions)
        {
            actions[D20ActionType.PYTHON_ACTION] = new D20ActionDef
            {
                actionCost = D20ActionCallbacks.ActionCostPython,
                actionCheckFunc = D20ActionCallbacks.ActionCheckPython,
                addToSeqFunc = D20ActionCallbacks.AddToSeqPython,
                performFunc = D20ActionCallbacks.PerformPython,
                actionFrameFunc = D20ActionCallbacks.ActionFramePython,
                turnBasedStatusCheck = D20ActionCallbacks.TurnBasedStatusCheckPython,
                locCheckFunc = D20ActionCallbacks.LocationCheckPython,
                projectileHitFunc = D20ActionCallbacks.ProjectileHitPython,
                flags = D20ADF.D20ADF_Python
            };

            actions[D20ActionType.DIVINE_MIGHT] = new D20ActionDef
            {
                // actionFrameFunc = _DivineMightPerform;
                addToSeqFunc = D20ActionCallbacks.AddToSeqSimple,
                actionCheckFunc = D20ActionCallbacks.ActionCheckDivineMight,
                performFunc = D20ActionCallbacks.PerformDivineMight,
                actionCost = D20ActionCallbacks.ActionCostNull,
                flags = D20ADF.D20ADF_None
            };

            actions[D20ActionType.DISARM] = new D20ActionDef
            {
                addToSeqFunc = D20ActionCallbacks.AddToSeqWithTarget,
                turnBasedStatusCheck = D20ActionCallbacks.StdAttackTurnBasedStatusCheck,
                actionCheckFunc = D20ActionCallbacks.ActionCheckDisarm,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionCallbacks.PerformDisarm,
                actionFrameFunc = D20ActionCallbacks.ActionFrameDisarm,
                actionCost = D20ActionCallbacks.ActionCostStandardAttack,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersAoO | D20ADF.D20ADF_QueryForAoO |
                        D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_UseCursorForPicking |
                        D20ADF.D20ADF_SimulsCompatible
            };
            // 0x28908; // same as Trip // note : queryForAoO is used for resetting a flag

            actions[D20ActionType.DISARMED_WEAPON_RETRIEVE] = new D20ActionDef
            {
                addToSeqFunc = D20ActionCallbacks.AddToSeqSimple,
                turnBasedStatusCheck = null,
                actionCheckFunc = D20ActionCallbacks.ActionCheckDisarmedWeaponRetrieve,
                locCheckFunc = D20ActionCallbacks.LocationCheckDisarmedWeaponRetrieve,
                performFunc = D20ActionCallbacks.PerformDisarmedWeaponRetrieve,
                actionFrameFunc = null,
                actionCost = D20ActionCallbacks.ActionCostMoveAction,
                seqRenderFunc = null,
                flags = D20ADF.D20ADF_TriggersAoO | D20ADF.D20ADF_SimulsCompatible |
                        D20ADF.D20ADF_Breaks_Concentration
            };
            // 0x28908; // largely same as Pick Up Object (but not | D20ADF.D20ADF_UseCursorForPicking)

            actions[D20ActionType.SUNDER] = new D20ActionDef
            {
                addToSeqFunc = D20ActionCallbacks.AddToSeqWithTarget,
                turnBasedStatusCheck = D20ActionCallbacks.StdAttackTurnBasedStatusCheck,
                actionCheckFunc = D20ActionCallbacks.ActionCheckSunder,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionCallbacks.PerformDisarm,
                actionFrameFunc = D20ActionCallbacks.ActionFrameSunder,
                actionCost = D20ActionCallbacks.ActionCostStandardAttack,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersAoO |
                        D20ADF.D20ADF_TriggersCombat | D20ADF.D20ADF_UseCursorForPicking |
                        D20ADF.D20ADF_SimulsCompatible
            };
            // 0x28908; // same as Trip

            actions[D20ActionType.AID_ANOTHER_WAKE_UP] = new D20ActionDef
            {
                addToSeqFunc = D20ActionCallbacks.AddToSeqWithTarget,
                turnBasedStatusCheck = D20ActionCallbacks.StdAttackTurnBasedStatusCheck,
                actionCheckFunc = D20ActionCallbacks.ActionCheckAidAnotherWakeUp,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionCallbacks.PerformAidAnotherWakeUp,
                actionFrameFunc = D20ActionCallbacks.ActionFrameAidAnotherWakeUp,
                actionCost = D20ActionCallbacks.ActionCostStandardAttack,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_UseCursorForPicking |
                        D20ADF.D20ADF_SimulsCompatible
            };
            // 0x28908; // same as Trip // note : queryForAoO is used for resetting a flag

            actions[D20ActionType.EMPTY_BODY] = new D20ActionDef
            {
                addToSeqFunc = D20ActionCallbacks.AddToSeqSimple,
                actionCheckFunc = D20ActionCallbacks.ActionCheckEmptyBody,
                performFunc = D20ActionCallbacks.PerformEmptyBody,
                actionCost = D20ActionCallbacks.ActionCostStandardAction
            };

            actions[D20ActionType.QUIVERING_PALM] = new D20ActionDef
            {
                addToSeqFunc = D20ActionCallbacks.AddToSeqWithTarget,
                turnBasedStatusCheck = D20ActionCallbacks.StdAttackTurnBasedStatusCheck,
                actionCheckFunc = D20ActionCallbacks.ActionCheckQuiveringPalm,
                locCheckFunc = D20ActionVanillaCallbacks.StandardAttackLocationCheck,
                performFunc = D20ActionCallbacks.PerformQuiveringPalm,
                actionFrameFunc = D20ActionCallbacks.ActionFrameQuiveringPalm,
                actionCost = D20ActionCallbacks.ActionCostStandardAttack,
                seqRenderFunc = D20ActionVanillaCallbacks.PickerFuncTooltipToHitChance,
                flags = D20ADF.D20ADF_TargetSingleExcSelf | D20ADF.D20ADF_TriggersCombat |
                        D20ADF.D20ADF_UseCursorForPicking
            };
        }
    }
}