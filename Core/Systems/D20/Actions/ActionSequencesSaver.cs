using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.D20.Actions
{
    internal static class ActionSequencesSaver
    {
        internal static SavedD20ActionSequence[] SaveSequences(List<ActionSequence> sequences)
        {

            var result = new SavedD20ActionSequence[sequences.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var sequence = sequences[i];
                result[i] = new SavedD20ActionSequence
                {
                    Actions = sequence.d20ActArray.Select(SaveAction).ToArray(),
                    IsPerforming = sequence.IsPerforming,
                    IsInterrupted = sequence.IsInterrupted,
                    CurrentActionIndex = sequence.d20aCurIdx,
                    TurnStatus = SaveTurnStatus(sequence.tbStatus),
                    PerformerLocation = sequence.performerLoc,
                    IgnoreLineOfSight = sequence.ignoreLos,
                    Spell = ActiveSpellSaver.SaveActiveSpell(sequence.spellPktBody, false),
                    Performer = sequence.performer?.id ?? ObjectId.CreateNull(),
                    PreviousSequenceIndex = sequence.prevSeq != null ? sequences.IndexOf(sequence.prevSeq) : -1,
                    InterruptedSequenceIndex = sequence.interruptSeq != null ? sequences.IndexOf(sequence.interruptSeq) : -1,
                };
            }

            return result;
        }

        internal static SavedD20TurnBasedStatus SaveTurnStatus(TurnBasedStatus turnStatus)
        {
            return new SavedD20TurnBasedStatus
            {
                HourglassState = turnStatus.hourglassState,
                Flags = turnStatus.tbsFlags,
                IndexSth = turnStatus.idxSthg,
                SurplusMoveDistance = turnStatus.surplusMoveDistance,
                BaseAttackNumCode = turnStatus.baseAttackNumCode,
                AttackModeCount = turnStatus.attackModeCode,
                NumBonusAttacks = turnStatus.numBonusAttacks,
                NumAttacks = turnStatus.numAttacks,
                ErrorCode = turnStatus.errCode
            };
        }

        internal static SavedD20Action SaveAction(D20Action action)
        {
            return new SavedD20Action
            {
                Type = action.d20ActType,
                Data = action.data1,
                Flags = action.d20Caf,
                TargetLocation = action.destLoc,
                DistanceTraveled = action.distTraversed,
                RadialMenuArg = action.radialMenuActualArg,
                RollHistoryId0 = action.rollHistId0,
                RollHistoryId1 = action.rollHistId1,
                RollHistoryId2 = action.rollHistId2,
                SpellData = action.d20SpellData,
                SpellId = action.spellId,
                AnimActionId = action.animID,
                Performer = action.d20APerformer?.id ?? ObjectId.CreateNull(),
                Target = action.d20ATarget?.id ?? ObjectId.CreateNull(),
            };
        }
    }
}