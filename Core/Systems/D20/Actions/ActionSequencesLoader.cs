using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.D20.Actions;

internal static class ActionSequencesLoader
{
    internal static List<ActionSequence?> LoadSequences(SavedD20ActionSequence?[] savedSequences)
    {
        var result = new List<ActionSequence?>(savedSequences.Length);
        foreach (var savedSequence in savedSequences)
        {
            result.Add(savedSequence == null ? null : LoadSequence(savedSequence));
        }

        // restore previous/interrupted sequence references
        for (var i = 0; i < result.Count; i++)
        {
            var resultSequence = result[i];
            if (resultSequence == null)
            {
                continue; // Nothing to restore for actions that will be deleted anyway
            }

            var savedSequence = savedSequences[i]!; // savedSequences[i] cannot be null if result[i] isn't
            var previousSequenceIndex = savedSequence.PreviousSequenceIndex;
            if (previousSequenceIndex != -1)
            {
                resultSequence.prevSeq = result[previousSequenceIndex];
            }

            var interruptedSequenceIndex = savedSequence.InterruptedSequenceIndex;
            if (interruptedSequenceIndex != -1)
            {
                resultSequence.interruptSeq = result[interruptedSequenceIndex];
            }
        }

        return result;
    }

    private static ActionSequence LoadSequence(SavedD20ActionSequence savedSequence)
    {
        var sequence = new ActionSequence
        {
            d20ActArray = savedSequence.Actions.Select(LoadAction).ToList(),
            IsPerforming = savedSequence.IsPerforming,
            IsInterrupted = savedSequence.IsInterrupted,
            d20aCurIdx = savedSequence.CurrentActionIndex,
            tbStatus = LoadTurnStatus(savedSequence.TurnStatus),
            performerLoc = savedSequence.PerformerLocation,
            ignoreLos = savedSequence.IgnoreLineOfSight,
            spellPktBody = ActiveSpellLoader.LoadActiveSpell(savedSequence.Spell)
        };
        if (!savedSequence.Performer.IsNull)
        {
            var performer = GameSystems.Object.GetObject(savedSequence.Performer);
            if (performer == null)
            {
                throw new CorruptSaveException(
                    $"Failed to restore action sequence performer {savedSequence.Performer}");
            }

            sequence.performer = performer;
        }

        return sequence;
    }

    internal static TurnBasedStatus LoadTurnStatus(SavedD20TurnBasedStatus savedTurnStatus)
    {
        return new TurnBasedStatus
        {
            hourglassState = savedTurnStatus.HourglassState,
            tbsFlags = savedTurnStatus.Flags,
            idxSthg = savedTurnStatus.IndexSth,
            surplusMoveDistance = savedTurnStatus.SurplusMoveDistance,
            baseAttackNumCode = savedTurnStatus.BaseAttackNumCode,
            attackModeCode = savedTurnStatus.AttackModeCount,
            numBonusAttacks = savedTurnStatus.NumBonusAttacks,
            numAttacks = savedTurnStatus.NumAttacks,
            errCode = savedTurnStatus.ErrorCode
        };
    }

    internal static D20Action LoadAction(SavedD20Action savedAction)
    {
        var action = new D20Action
        {
            d20ActType = savedAction.Type,
            data1 = savedAction.Data,
            d20Caf = savedAction.Flags,
            destLoc = savedAction.TargetLocation,
            distTraversed = savedAction.DistanceTraveled,
            radialMenuActualArg = savedAction.RadialMenuArg,
            rollHistId0 = savedAction.RollHistoryId0,
            rollHistId1 = savedAction.RollHistoryId1,
            rollHistId2 = savedAction.RollHistoryId2,
            d20SpellData = savedAction.SpellData,
            spellId = savedAction.SpellId,
            animID = savedAction.AnimActionId
        };
        if (!savedAction.Performer.IsNull)
        {
            var performer = GameSystems.Object.GetObject(savedAction.Performer);
            if (performer == null)
            {
                throw new CorruptSaveException($"Unable to restore action performer: {savedAction.Performer}");
            }

            action.d20APerformer = performer;
        }

        if (!savedAction.Target.IsNull)
        {
            var target = GameSystems.Object.GetObject(savedAction.Target);
            if (target == null)
            {
                throw new CorruptSaveException($"Unable to restore action target: {savedAction.Target}");
            }

            action.d20ATarget = target;
        }

        return action;
    }
}