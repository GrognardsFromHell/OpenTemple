using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;

#nullable enable

namespace OpenTemple.Core.Systems.D20.Actions;

public enum ActionCursor
{
    Undefined = 0,
    Sword = 1,
    Arrow = 2,
    FeetGreen = 3,
    FeetYellow = 4,
    SlidePortraits = 5,
    Locked = 6,
    HaveKey = 7,
    UseSkill = 8,
    UsePotion = 9,
    UseSpell = 10,
    UseTeleportIcon = 11,
    HotKeySelection = 12,
    Talk = 13,
    IdentifyCursor = 14,
    IdentifyCursor2 = 15,
    ArrowInvalid = 16,
    SwordInvalid = 17,
    ArrowInvalid2 = 18,
    FeetRed = 19,
    FeetRed2 = 20,
    InvalidSelection = 21,
    Locked2 = 22,
    HaveKey2 = 23,
    UseSkillInvalid = 24,
    UsePotionInvalid = 25,
    UseSpellInvalid = 26,
    PlaceFlag = 27,
    HotKeySelectionInvalid = 28,
    InvalidSelection2 = 29,
    InvalidSelection3 = 30,
    InvalidSelection4 = 31,
    AttackOfOpportunity = 32,
    AttackOfOpportunityGrey = 33,
}

public class D20ActionSystem : IDisposable
{
    public const int INV_IDX_INVALID = -1;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private AooIndicatorRenderer _aooIndicatorRenderer = new();

    /// <summary>
    /// Callback to listen to actions as they're being performed. For testing purposes to listen to
    /// move actions and such.
    /// </summary>
    public event Action<D20Action>? OnActionStarted;

    public event Action<D20Action>? OnActionEnded;

    private readonly Dictionary<int, PythonActionSpec> _pythonActions = new();

    public D20Action globD20Action = new();

    private D20DispatcherKey globD20ActionKey;

    [TempleDllLocation(0x118A09A0)] private List<ActionSequence> actSeqArray = new();

    [TempleDllLocation(0x118CD574)] private ActionSequence? actSeqInterrupt;

    [TempleDllLocation(0x1186A8F0)] internal ActionSequence? CurrentSequence { get; set; }

    [TempleDllLocation(0x1008a090)]
    internal D20Action CurrentAction
    {
        get
        {
            var sequence = CurrentSequence;
            return sequence?.d20ActArray[sequence.d20aCurIdx];
        }
    }

    [TempleDllLocation(0x118CD2A0)] internal int actSeqTargetsIdx => actSeqTargets.Count;

    [TempleDllLocation(0x118CD2A8)] internal List<GameObject> actSeqTargets = new();

    [TempleDllLocation(0x118CD3A8)] internal LocAndOffsets actSeqSpellLoc;

    [TempleDllLocation(0x118CD400)] private D20Action actSeqPickerAction;

    [TempleDllLocation(0x10B3D5A0)] internal bool actSeqPickerActive { get; private set; }

    [TempleDllLocation(0x1186A900)] private List<ReadiedActionPacket> _readiedActions = new();

    [TempleDllLocation(0x10B3BF48)] private Dictionary<int, string> _translations;

    /// <summary>
    /// This is from vanilla. In general we could go as high as we want.
    /// </summary>
    private const int MaxSimultPerformers = 8;

    [TempleDllLocation(0x118A06C0)] [TempleDllLocation(0x10B3D5B8)]
    private readonly List<GameObject> _simultPerformerQueue = new();

    // Vanilla stored the object who this was for in _simultPerformerQueue[numSimultPerformers],
    // which essentially reused the area past the actual simultaneous performers for storing
    // the object for which the turn based status was saved.
    // I think this saves the turn based status for a critter who had to abort their sequence during
    // a simultaneous turn, so that it can be restored later.
    [TempleDllLocation(0x118CD3C0)] private (GameObject actor, TurnBasedStatus status)? _abortedSimultaneousSequence;

    [TempleDllLocation(0x10B3D5BC)] private int simulsIdx;

    [TempleDllLocation(0x10B3D59C)] private int _aiTurnActions;

    [TempleDllLocation(0x1186A8E8)] private ResourceRef<ITexture> _aooIcon;

    [TempleDllLocation(0x1186A8EC)] private ResourceRef<ITexture> _aooGreyIcon;

    [TempleDllLocation(0x10092800)]
    public D20ActionSystem()
    {
        _translations = Tig.FS.ReadMesFile("mes/action.mes");

        _aooIcon = Tig.Textures.Resolve("art/interface/COMBAT_UI/Attack-of-Opportunity.tga", false);
        _aooGreyIcon = Tig.Textures.Resolve("art/interface/COMBAT_UI/Attack-of-Opportunity-Grey.tga", false);
    }

    [TempleDllLocation(0x10089ef0)]
    public void Dispose()
    {
        Stub.TODO();

        _aooIcon.Dispose();
        _aooGreyIcon.Dispose();
    }

    [TempleDllLocation(0x10097be0)]
    public void ActionSequencesResetOnCombatEnd()
    {
        ResetAll(null);
    }

    [TempleDllLocation(0x10095fd0)]
    public bool TurnBasedStatusInit(GameObject actor)
    {
        if (GameSystems.Combat.IsCombatActive())
        {
            return GameSystems.D20.Initiative.CurrentActor == actor;
        }

        if (IsCurrentlyPerforming(actor))
        {
            return false;
        }

        globD20ActnSetPerformer(actor);

        CurrentSequence = null;
        AssignSeq(actor);
        var curSeq = CurrentSequence;

        var tbStatus = new TurnBasedStatus();
        curSeq.tbStatus = tbStatus;
        tbStatus.hourglassState = HourglassState.FULL;
        tbStatus.tbsFlags = default;
        tbStatus.idxSthg = -1;
        tbStatus.baseAttackNumCode = 0;
        tbStatus.attackModeCode = 0;
        tbStatus.numBonusAttacks = 0;
        tbStatus.numAttacks = 0;
        tbStatus.errCode = 0;
        tbStatus.surplusMoveDistance = 0;

        var dispIOtB = new DispIOTurnBasedStatus();
        dispIOtB.tbStatus = tbStatus;
        dispatchTurnBasedStatusInit(actor, dispIOtB);
        curSeq.IsPerforming = false;
        return true;
    }

    [TempleDllLocation(0x10094eb0)]
    public void AssignSeq(GameObject performer)
    {
        var prevSeq = CurrentSequence;
        AllocSeq(performer);
        if (GameSystems.Combat.IsCombatActive())
        {
            if (prevSeq != null)
            {
                Logger.Debug("Pushing sequence from {0} to {1}", prevSeq.performer, performer);
            }
            else
            {
                Logger.Debug("Allocated sequence for {0}", performer);
            }
        }

        CurrentSequence.prevSeq = prevSeq;
        CurrentSequence.IsPerforming = true;
    }

    [TempleDllLocation(0x10094e20)]
    private void AllocSeq(GameObject performer)
    {
        var newSequence = new ActionSequence();
        actSeqArray.Add(newSequence);
        CurrentSequence = newSequence;
        if (GameSystems.Combat.IsCombatActive())
            Logger.Debug("AllocSeq: \t Sequence Allocate[{0}]({1}): Resetting Sequence. ", actSeqArray.Count,
                performer);
        CurSeqReset(performer);
    }

    [TempleDllLocation(0x1008a530)]
    public void globD20ActnSetPerformer(GameObject performer)
    {
        if (performer != globD20Action.d20APerformer)
        {
            SeqPickerTargetingTypeReset();
        }

        globD20Action.d20APerformer = performer;
    }

    [TempleDllLocation(0x10089f70)]
    public TurnBasedStatus curSeqGetTurnBasedStatus()
    {
        return CurrentSequence?.tbStatus;
    }

    // describes the new hourglass state when current state is i after doing an action that costs j
    [TempleDllLocation(0x102CC538)] private static readonly int[,] TurnBasedStatusTransitionMatrix = new int[7, 5]
    {
        {0, -1, -1, -1, -1},
        {1, 0, -1, -1, -1},
        {2, 0, 0, -1, -1},
        {3, 0, 0, 0, -1},
        {4, 2, 1, -1, 0},
        {5, 2, 2, -1, 0},
        {6, 5, 2, -1, 3}
    };

    [TempleDllLocation(0x1008b020)]
    public HourglassState GetHourglassTransition(HourglassState hourglassCurrent, ActionCostType hourglassCost)
    {
        if (hourglassCurrent == HourglassState.INVALID)
        {
            return hourglassCurrent;
        }

        return (HourglassState) TurnBasedStatusTransitionMatrix[(int) hourglassCurrent, (int) hourglassCost];
    }

    // initializes the sequence pointed to by actSeqCur and assigns it to objHnd
    [TempleDllLocation(0x10094A00)]
    public void CurSeqReset(GameObject performer)
    {
        var curSeq = CurrentSequence;

        // release path finding queries
        foreach (var action in curSeq.d20ActArray)
        {
            ReleasePooledPathQueryResult(ref action.path);
        }

        curSeq.d20ActArray.Clear();
        curSeq.d20aCurIdx = -1;
        curSeq.prevSeq = null;
        curSeq.interruptSeq = null;
        curSeq.IsPerforming = false;
        curSeq.IsInterrupted = false;

        globD20ActnSetPerformer(performer);
        GlobD20ActnInit();
        curSeq.performer = performer;
        curSeq.targetObj = null;
        curSeq.performerLoc = performer.GetLocationFull();
        curSeq.ignoreLos = false;
        performingDefaultAction = false;
    }

    [TempleDllLocation(0x100949e0)]
    public void GlobD20ActnInit()
    {
        globD20Action.Reset(globD20Action.d20APerformer);
    }

    [TempleDllLocation(0x10089f80)]
    public void GlobD20ActnSetTypeAndData1(D20ActionType type, int data1)
    {
        globD20Action.d20ActType = type;
        globD20Action.data1 = data1;
    }

    [TempleDllLocation(0x1008a450)]
    public void GlobD20ActnSetSpellData(D20SpellData d20SpellData)
    {
        globD20Action.d20SpellData = d20SpellData;
    }

    [TempleDllLocation(0x10089fd0)]
    public void GlobD20aSetActualArg(int arg)
    {
        globD20Action.radialMenuActualArg = arg;
    }

    [TempleDllLocation(0x10089fe0)]
    public void GlobD20ActnSetD20CAF(D20CAF d20caf)
    {
        globD20Action.d20Caf |= d20caf;
    }

    [TempleDllLocation(0x10097C20)]
    public ActionErrorCode ActionAddToSeq()
    {
        var curSeq = CurrentSequence;
        var d20ActnType = globD20Action.d20ActType;
        TurnBasedStatus tbStatus = curSeq.tbStatus;

        if (d20ActnType == D20ActionType.CAST_SPELL
            && curSeq.spellPktBody.spellEnum >= 600)
        {
            curSeq.tbStatus.tbsFlags |=
                TurnBasedStatusFlags.CritterSpell; // perhaps bug that it's not affecting the local copy?? TODO
            curSeq.spellPktBody.spellEnumOriginal = curSeq.spellPktBody.spellEnum;
        }

        if (d20ActnType == D20ActionType.PYTHON_ACTION)
        {
            globD20ActionKey = (D20DispatcherKey) globD20Action.data1;
        }

        var actnCheckFunc = D20ActionDefs.GetActionDef(d20ActnType).actionCheckFunc;

        var actnCheckResult = ActionErrorCode.AEC_OK;
        if (actnCheckFunc != null)
        {
            actnCheckResult = actnCheckFunc(globD20Action, tbStatus);
        }

        if (GameSystems.Party.IsPlayerControlled(curSeq.performer))
        {
            if (actnCheckResult != ActionErrorCode.AEC_OK)
            {
                if (actnCheckResult == ActionErrorCode.AEC_TARGET_INVALID)
                {
                    ActSeqGetPicker();
                    return ActionErrorCode.AEC_TARGET_INVALID;
                }

                if (actnCheckResult != ActionErrorCode.AEC_TARGET_TOO_FAR)
                {
                    var errorString = ActionErrorString(actnCheckResult);
                    GameSystems.TextFloater.FloatLine(curSeq.performer, TextFloaterCategory.Generic,
                        TextFloaterColor.Red, errorString);
                    return actnCheckResult;
                }
            }
            else if (!TargetCheck(globD20Action))
            {
                ActSeqGetPicker();
                return ActionErrorCode.AEC_TARGET_INVALID;
            }
        }
        else
        {
            if (actnCheckResult == ActionErrorCode.AEC_OUT_OF_CHARGES)
            {
                return actnCheckResult;
            }

            TargetCheck(globD20Action);
        }

        return AddActionToSequence(globD20Action, curSeq);
    }

    [TempleDllLocation(0x1008a100)]
    private ActionErrorCode AddActionToSequence(D20Action action, ActionSequence sequence)
    {
        var type = action.d20ActType;
        if (type == D20ActionType.NONE)
        {
            return ActionErrorCode.AEC_INVALID_ACTION;
        }

        var d20Def = D20ActionDefs.GetActionDef(type);
        actnProcState = d20Def.addToSeqFunc(action, sequence, sequence.tbStatus);

        // TODO: Why is this done if the addToSeqFunc might have failed???
        var curSeq = CurrentSequence;
        if (sequence.tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.FullAttack))
        {
            for (var i = curSeq.d20aCurIdx + 1; i < curSeq.d20ActArray.Count; i++)
            {
                curSeq.d20ActArray[i].d20Caf |= D20CAF.FULL_ATTACK;
            }
        }

        return actnProcState;
    }

    [TempleDllLocation(0x10096570)]
    private void ChooseTargetCallback(ref PickerResult result, object callbackArg)
    {
        bool hasValidTarget = false;
        actSeqPickerActive = false;
        if (result.HasMultipleResults)
        {
            actSeqTargets.Clear();
            foreach (var target in result.objList)
            {
                actSeqTargets.Add(target);
            }

            actSeqSpellLoc = result.location;
            hasValidTarget = true;
        }
        else if (result.HasSingleResult)
        {
            // TODO: Manually checking against the action type here to determine what a valid target is -> sucks
            if (actSeqPickerAction.d20ActType == D20ActionType.DISABLE_DEVICE
                || actSeqPickerAction.d20ActType == D20ActionType.OPEN_LOCK
                || result.handle.IsCritter())
            {
                hasValidTarget = true;
            }
        }

        if (hasValidTarget)
        {
            Logger.Info("Choose target callback: reseting sequence");
            CurSeqReset(actSeqPickerAction.d20APerformer);
            GlobD20ActnInit();
            if (!result.HasMultipleResults)
            {
                GlobD20ActnSetTarget(result.handle, null);
            }

            ActionAddToSeq();
            sequencePerform();
        }

        GlobD20ActnInit();
        ActSeqTargetsClear();
        GameUiBridge.FreeCurrentPicker();
    }


    [TempleDllLocation(0x100977a0)]
    private void ActSeqGetPicker()
    {
        var tgtClassif = TargetClassification(globD20Action);

        var d20adflags = GetActionFlags(globD20Action.d20ActType);

        if (tgtClassif == D20TargetClassification.ItemInteraction)
        {
            if (d20adflags.HasFlag(D20ADF.D20ADF_UseCursorForPicking))
            {
                seqPickerD20ActnType = globD20Action.d20ActType; // seqPickerD20ActnType
                seqPickerD20ActnData1 = globD20Action.data1;
                seqPickerTargetingType = D20TargetClassification.ItemInteraction;
                return;
            }

            var actSeqPicker = new PickerArgs();
            actSeqPicker.flagsTarget = UiPickerFlagsTarget.None;
            actSeqPicker.modeTarget = UiPickerType.Single;
            actSeqPicker.incFlags = UiPickerIncFlags.UIPI_NonCritter;
            actSeqPicker.excFlags = UiPickerIncFlags.UIPI_None;
            actSeqPicker.callback = ChooseTargetCallback;
            actSeqPicker.caster = globD20Action.d20APerformer;
            actSeqPickerActive = true;
            GameUiBridge.ShowPicker(actSeqPicker);
            actSeqPickerAction = globD20Action.Copy();
            return;
        }

        if (tgtClassif == D20TargetClassification.SingleIncSelf)
        {
            if (d20adflags.HasFlag(D20ADF.D20ADF_UseCursorForPicking))
            {
                seqPickerD20ActnType = globD20Action.d20ActType; //seqPickerD20ActnType
                seqPickerD20ActnData1 = globD20Action.data1;
                seqPickerTargetingType = D20TargetClassification.SingleIncSelf;
                return;
            }

            var actSeqPicker = new PickerArgs();
            actSeqPicker.flagsTarget = UiPickerFlagsTarget.None;
            actSeqPicker.modeTarget = UiPickerType.Single;
            actSeqPicker.incFlags =
                UiPickerIncFlags.UIPI_Self | UiPickerIncFlags.UIPI_Other | UiPickerIncFlags.UIPI_Dead;
            actSeqPicker.excFlags = UiPickerIncFlags.UIPI_NonCritter;
            actSeqPicker.callback = ChooseTargetCallback;
            actSeqPicker.spellEnum = 0;
            actSeqPicker.caster = globD20Action.d20APerformer;

            actSeqPickerActive = true;
            GameUiBridge.ShowPicker(actSeqPicker);
            actSeqPickerAction = globD20Action.Copy();
            return;
        }

        if (tgtClassif == D20TargetClassification.SingleExcSelf)
        {
            if (d20adflags.HasFlag(D20ADF.D20ADF_UseCursorForPicking))
            {
                seqPickerD20ActnType = globD20Action.d20ActType;
                seqPickerD20ActnData1 = globD20Action.data1;
                seqPickerTargetingType = D20TargetClassification.SingleExcSelf;
                return;
            }

            var actSeqPicker = new PickerArgs();
            actSeqPicker.flagsTarget = UiPickerFlagsTarget.None;
            actSeqPicker.modeTarget = UiPickerType.Single;
            actSeqPicker.incFlags = UiPickerIncFlags.UIPI_Other | UiPickerIncFlags.UIPI_Dead;
            actSeqPicker.excFlags = UiPickerIncFlags.UIPI_Self | UiPickerIncFlags.UIPI_NonCritter;
            actSeqPicker.callback = ChooseTargetCallback;
            actSeqPicker.spellEnum = 0;
            actSeqPicker.caster = globD20Action.d20APerformer;
            actSeqPickerActive = true;
            GameUiBridge.ShowPicker(actSeqPicker);
            actSeqPickerAction = globD20Action.Copy();
            return;
        }

        if (tgtClassif == D20TargetClassification.CallLightning)
        {
            var actSeqPicker = new PickerArgs();
            if (globD20Action.d20ActType == D20ActionType.SPELL_CALL_LIGHTNING)
            {
                var callLightningId = (int) GameSystems.D20.D20QueryReturnData(globD20Action.d20APerformer,
                    D20DispatcherKey.QUE_Critter_Can_Call_Lightning);
                var spellPkt = GameSystems.Spell.GetActiveSpell(callLightningId);
                var baseCasterLevelMod = globD20Action.d20APerformer.Dispatch35CasterLevelModify(spellPkt);
                actSeqPicker.range = GameSystems.Spell.GetSpellRangeExact(SpellRangeType.SRT_Medium,
                    baseCasterLevelMod, globD20Action.d20APerformer);
                actSeqPicker.radiusTarget = 5;
            }
            else
            {
                actSeqPicker.range = 0;
                actSeqPicker.radiusTarget = 0;
            }

            actSeqPicker.flagsTarget = UiPickerFlagsTarget.Range;
            actSeqPicker.modeTarget = UiPickerType.Area;
            actSeqPicker.incFlags = UiPickerIncFlags.UIPI_Self | UiPickerIncFlags.UIPI_Other;
            actSeqPicker.excFlags = UiPickerIncFlags.UIPI_Dead | UiPickerIncFlags.UIPI_NonCritter;
            actSeqPicker.callback = ChooseTargetCallback;
            actSeqPicker.spellEnum = 0;
            actSeqPicker.caster = globD20Action.d20APerformer;
            actSeqPickerActive = true;
            GameUiBridge.ShowPicker(actSeqPicker);
            actSeqPickerAction = globD20Action.Copy();
            return;
        }

        if (tgtClassif == D20TargetClassification.CastSpell)
        {
            var spellEnum = globD20Action.d20SpellData.SpellEnum;
            var metaMagicData = globD20Action.d20SpellData.metaMagicData;

            //Modify metamagic data for enlarge and widen
            metaMagicData = globD20Action.d20APerformer.DispatchMetaMagicModify(metaMagicData);

            var curSeq = CurrentSequence;
            curSeq.spellPktBody.spellRange *= metaMagicData.metaMagicEnlargeSpellCount + 1;
            var spellEntry = GameSystems.Spell.GetSpellEntry(spellEnum);
            var radiusTarget = spellEntry.radiusTarget * metaMagicData.metaMagicWidenSpellCount + 1;
            var pickArgs = new PickerArgs();
            GameSystems.Spell.PickerArgsFromSpellEntry(spellEntry, pickArgs, curSeq.spellPktBody.caster,
                curSeq.spellPktBody.casterLevel, radiusTarget);
            pickArgs.spellEnum = spellEnum;
            pickArgs.callback = (ref PickerResult result, object cbArgs) =>
            {
                SpellPickerCallback(ref result, curSeq);
            };

            // Modify the PickerArgs
            if (globD20Action.d20ActType == D20ActionType.PYTHON_ACTION)
            {
                GameSystems.Script.Actions.ModifyPicker(globD20Action.data1, pickArgs);
            }

            actSeqPickerActive = true;
            GameUiBridge.ShowPicker(pickArgs);
            actSeqPickerAction = globD20Action.Copy();
        }
    }

    [TempleDllLocation(0x10090af0)]
    private void ActSeqTargetsClear()
    {
        actSeqTargets.Clear();
        actSeqSpellLoc = LocAndOffsets.Zero;
    }

    [TempleDllLocation(0x10096cc0)]
    private void SpellPickerCallback(ref PickerResult result, ActionSequence sequence)
    {
        if (result.flags == default)
        {
            actSeqPickerActive = true;
            return;
        }

        actSeqPickerActive = false;

        if (result.flags.HasFlag(PickerResultFlags.PRF_CANCELLED))
        {
            GameUiBridge.FreeCurrentPicker();
            CurrentSequence?.ResetSpell();
            ActSeqTargetsClear();
            GlobD20ActnInit();
            return;
        }

        Logger.Info("SpellPickerCallback(): Resetting sequence");
        var performer = actSeqPickerAction.d20APerformer;

        if (!SequenceSwitch(performer) && CurrentSequence.IsPerforming)
            return;

        CurSeqReset(performer);
        globD20Action = actSeqPickerAction.Copy();

        // NOTE: Vanilla somehow directly accessed the sequence's action here, but why???
        var spellPacket = sequence.spellPktBody;
        if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ))
        {
            spellPacket.SetTargets(new[] {result.handle});
            spellPacket.InitialTargets = new[] {result.handle}.ToImmutableSortedSet();
            globD20Action.d20ATarget = result.handle;
            globD20Action.destLoc = result.handle.GetLocationFull();
        }
        else
        {
            spellPacket.SetTargets(Array.Empty<GameObject>());
            spellPacket.InitialTargets = ImmutableSortedSet<GameObject>.Empty;
        }

        if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ))
        {
            spellPacket.SetTargets(result.objList.ToArray());
            spellPacket.InitialTargets = result.objList.ToImmutableSortedSet();
        }

        if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_LOCATION))
        {
            spellPacket.aoeCenter = result.location;
            spellPacket.aoeCenterZ = result.offsetz;
            globD20Action.destLoc = result.location;
        }
        else
        {
            spellPacket.aoeCenter = LocAndOffsets.Zero;
            spellPacket.aoeCenterZ = 0.0f;
            globD20Action.destLoc = LocAndOffsets.Zero;
        }

        if (result.flags.HasFlag(PickerResultFlags.PRF_UNK8))
        {
            Logger.Warn("SpellPickerCallback: not implemented - BECAME_TOUCH_ATTACK");
        }

        spellPacket.pickerResult = result;
        GameUiBridge.FreeCurrentPicker();
        ActionAddToSeq();
        sequencePerform();
        ActSeqTargetsClear();
    }

    [TempleDllLocation(0x1008a0d0)]
    public void RadialMenuSetSeqPicker(D20ActionType d20Type, int data1, D20TargetClassification tgtingType)
    {
        seqPickerD20ActnType = d20Type;
        seqPickerD20ActnData1 = data1;
        seqPickerTargetingType = tgtingType;
    }

    [TempleDllLocation(0x1008b140)]
    public bool SequenceSwitch(GameObject performer)
    {
        ActionSequence? newSequence = null;
        foreach (var seq in actSeqArray)
        {
            if (seq.IsPerforming)
            {
                if (seq.performer == performer)
                {
                    newSequence = seq;
                }
            }
            else
            {
                if (seq.prevSeq != null && seq.prevSeq.performer == performer)
                    return false;
                if (seq.interruptSeq != null)
                {
                    if (seq.interruptSeq.performer == performer)
                        return false;
                }
            }
        }

        if (newSequence != null)
        {
            if (CurrentSequence != newSequence)
            {
                Logger.Debug("SequenceSwitch: \t doing for {0}. Previous Current Seq: {1}", performer,
                    CurrentSequence);
                CurrentSequence = newSequence;
            }

            Logger.Debug("SequenceSwitch: \t new Current Seq: {0}", CurrentSequence);
            return true;
        }

        return false;
    }

    [TempleDllLocation(0x1008a1b0)]
    public string ActionErrorString(ActionErrorCode actionErrorCode)
    {
        return _translations[1000 + (int) actionErrorCode];
    }

    [TempleDllLocation(0x100961C0)]
    public void sequencePerform()
    {
        // check if OK to perform
        if (actSeqPickerActive)
        {
            // should solve the issue when casting a spell in the presence of prebuffing NPCs
            // (i.e non-party members can now cast spells while the player's picker is active, so there shouldn't
            // be a lingering spell cast sequence fucking your shit up)
            if (CurrentSequence == null || GameSystems.Party.IsInParty(CurrentSequence.performer))
                return;
        }

        // is curSeq ok to perform?
        if (!actSeqOkToPerform())
        {
            Logger.Debug("SequencePerform: \t Sequence given while performing previous action - aborted.");
            GlobD20ActnInit();
            return;
        }

        // try to perform the sequence and its actions
        ActionSequence curSeq = CurrentSequence;
        if (GameSystems.Combat.IsCombatActive() || !ShouldTriggerCombat(curSeq) || !combatTriggerSthg(curSeq))
        {
            if (curSeq != CurrentSequence)
            {
                Logger.Debug("SequencePerform: Switched sequence slot from combat trigger!");
                curSeq = CurrentSequence;
            }

            Logger.Debug("SequencePerform: \t {0} performing sequence ({1})...",
                GameSystems.MapObject.GetDisplayName(curSeq.performer), curSeq);
            if (isSimultPerformer(curSeq.performer))
            {
                Logger.Debug("simultaneously...");
                if (!simulsOk(curSeq))
                {
                    if (simulsAbort(curSeq.performer))
                    {
                        Logger.Debug("sequence not allowed... aborting simuls (pending).");
                    }
                    else
                    {
                        Logger.Debug("sequence not allowed... aborting subsequent simuls.");
                    }

                    return;
                }

                Logger.Debug("succeeded...");
            }
            else
            {
                Logger.Debug("independently.");
            }

            actnProcState = ActionErrorCode.AEC_OK;
            curSeq.IsPerforming = true;
            ActionPerform();
            // I think actionPerform can modify the sequence, so better be safe
            for (curSeq = CurrentSequence; IsCurrentlyPerforming(curSeq.performer); curSeq = CurrentSequence)
            {
                if (curSeq.IsPerforming)
                {
                    var curIdx = curSeq.d20aCurIdx;
                    if (curIdx >= 0 && curIdx < curSeq.d20ActArrayNum)
                    {
                        var caflags = curSeq.d20ActArray[curIdx].d20Caf;
                        if (caflags.HasFlag(D20CAF.NEED_PROJECTILE_HIT) ||
                            caflags.HasFlag(D20CAF.NEED_ANIM_COMPLETED))
                        {
                            break;
                        }
                    }
                }

                ActionPerform();
            }
        }
    }

    [TempleDllLocation(0x100996e0)]
    public void ActionPerform()
    {
        // in principle this cycles through actions,
        // but if it succeeds in performing one it will return
        // note: the number of actions can generally change due to processing (e.g. if you cleave or incur AoOs)
        while (true)
        {
            var curSeq = CurrentSequence;
            ref var curIdx = ref curSeq.d20aCurIdx; // the action idx is inited to -1 for fresh sequences
            ++curIdx;

            void RemoveRemainingActions()
            {
                curSeq.d20ActArray.RemoveRange(curSeq.d20aCurIdx, curSeq.d20ActArray.Count - curSeq.d20aCurIdx);
            }

            // if the performer has become unconscious, abort
            GameObject performer = curSeq.performer;
            if (GameSystems.Critter.IsDeadOrUnconscious(performer))
            {
                RemoveRemainingActions();
                Logger.Debug("ActionPerform: \t Unconscious actor {0} - cutting sequence", performer);
            }

            // if have finished up the actions - do CurSeqNext
            if (curSeq.d20aCurIdx >= curSeq.d20ActArrayNum)
                break;

            var d20a = curSeq.d20ActArray[curIdx];

            // Make a copy so we can reset it later if an AOO occurs
            var tbStatus = curSeq.tbStatus.Copy();

            var errCode = CheckActionPreconditions(curSeq, curIdx, tbStatus);
            if (errCode != ActionErrorCode.AEC_OK)
            {
                var errorText = ActionErrorString(errCode);

                Logger.Debug("ActionPerform: \t Action unavailable for {0} ({1}): {2}",
                    GameSystems.MapObject.GetDisplayName(d20a.d20APerformer), d20a.d20APerformer, errorText);
                actnProcState = errCode;
                curSeq.tbStatus.errCode = errCode;
                GameSystems.TextFloater.FloatLine(performer, TextFloaterCategory.Generic, TextFloaterColor.Red,
                    errorText);
                RemoveRemainingActions();
                break;
            }

            if (performingDefaultAction)
            {
                if (d20a.d20ActType == D20ActionType.STANDARD_ATTACK ||
                    d20a.d20ActType == D20ActionType.STANDARD_RANGED_ATTACK)
                {
                    if (d20a.d20ATarget != null && GameSystems.Critter.IsDeadOrUnconscious(d20a.d20ATarget))
                    {
                        performedDefaultAction = false;
                        RemoveRemainingActions();
                        //curSeq.tbStatus.tbsFlags |= TurnBasedStatusFlags.FullAttack;
                        break;
                    }
                }

                performedDefaultAction = true;
            }

            D20aTriggerCombatCheck(curSeq, curIdx);

            if (d20a.d20ActType == D20ActionType.AOO_MOVEMENT)
            {
                if (CheckAooIncurRegardTumble(d20a))
                {
                    DoAoo(d20a.d20APerformer, d20a.d20ATarget);
                    curSeq = CurrentSequence; // DoAoo updates the curSeq
                    curSeq.d20ActArray[curSeq.d20ActArrayNum - 1].d20Caf |= D20CAF.AOO_MOVEMENT;
                    sequencePerform();
                    return;
                }
            }

            else
            {
                if (D20ActionTriggersAoO(d20a, tbStatus) && TriggerAoOsByAdjacentEnemies(d20a.d20APerformer))
                {
                    Logger.Debug("ActionPerform: \t Sequence Preempted {0}", d20a.d20APerformer);
                    --curIdx;
                    sequencePerform();
                }
                else
                {
                    // Commit the modified tbStatus now that we didn't get interrupted by AOOs
                    tbStatus.CopyTo(curSeq.tbStatus);
                    curSeq.tbStatus.tbsFlags |= TurnBasedStatusFlags.HasActedThisRound;
                    InterruptCounterspell(d20a);
                    Logger.Debug("ActionPerform: \t Performing action [{2}/{3}] for {0}: {1}", d20a.d20APerformer,
                        d20a.d20ActType, curSeq.d20aCurIdx + 1, curSeq.d20ActArray.Count);

                    OnActionStarted?.Invoke(d20a);
                    D20ActionDefs.GetActionDef(d20a.d20ActType).performFunc(d20a);
                    InterruptNonCounterspell(d20a);
                }

                return;
            }
        }

        if (SequenceHasActionsPendingProjectileHit())
        {
            curSeqNext();
        }
    }

    [TempleDllLocation(0x10098ed0)]
    private bool curSeqNext()
    {
        var curSeq = CurrentSequence;
        GameObject performer = curSeq.performer;
        curSeq.IsPerforming = false;
        Logger.Debug("CurSeqNext: \t Sequence Completed for {0} (sequence {1})", curSeq.performer, curSeq);

        if (curSeq.d20ActArray.Count > 0)
        {
            var lastPerformer = curSeq.d20ActArray[^1].d20APerformer;
            GameSystems.D20.D20SendSignal(lastPerformer, D20DispatcherKey.SIG_Sequence, CurrentSequence);
        }

        // do D20SendSignal for D20DispatcherKey.SIG_Action_Recipient
        for (int d20aIdx = 0; d20aIdx < CurrentSequence.d20ActArrayNum; d20aIdx++)
        {
            var d20a = CurrentSequence.d20ActArray[d20aIdx];
            var d20aType = d20a.d20ActType;
            if (d20aType == D20ActionType.CAST_SPELL)
            {
                var spellId = d20a.spellId;
                if (spellId != 0)
                {
                    if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPktBody))
                    {
                        foreach (var initialTarget in spellPktBody.InitialTargets)
                        {
                            GameSystems.D20.D20SendSignal(initialTarget,
                                D20DispatcherKey.SIG_Action_Recipient, d20a);
                        }
                    }
                    else
                    {
                        Logger.Warn("CurSeqNext(): \t  unable to retrieve spell packet!");
                    }
                }
            }
            else
            {
                var d20aTarget = d20a.d20ATarget;
                if (IsOffensive(d20aType, d20aTarget))
                {
                    if (d20aTarget != null)
                    {
                        GameSystems.D20.D20SendSignal(d20aTarget, D20DispatcherKey.SIG_Action_Recipient, d20a);
                    }

                    if (GameSystems.Critter.IsMovingSilently(performer))
                    {
                        GameSystems.Critter.SetMovingSilently(performer, false);
                    }
                }
                else
                {
                    GameSystems.D20.D20SendSignal(CurrentSequence.performer, D20DispatcherKey.SIG_Action_Recipient,
                        d20a);
                }
            }
        }

        // Maintain current TB status if this is the sequence's end
        // NOTE: This only worked by pure chance in Vanilla, because it'd reuse the same action sequence slot
        // as it had just freed, and never actually cleaned up the tbstatus...
        var tbStatus = CurrentSequence.tbStatus;

        if (SequencePop())
        {
            if (CurrentSequence.d20aCurIdx + 1 < CurrentSequence.d20ActArrayNum)
            {
                ActionPerform();
            }

            return false;
        }

        AssignSeq(performer);
        Logger.Debug("CurSeqNext: \t  Resetting Sequence ({0})", CurrentSequence);
        CurSeqReset(globD20Action.d20APerformer);
        tbStatus.CopyTo(CurrentSequence.tbStatus);

        if (GameSystems.Combat.IsCombatActive())
        {
            // look for stuff that terminates / interrupts the turn
            if (HasReadiedAction(globD20Action.d20APerformer))
            {
                Logger.Debug("CurSeqNext: \t Action for {0} ({1}) ending turn (readied action)...",
                    GameSystems.MapObject.GetDisplayName(globD20Action.d20APerformer),
                    globD20Action.d20APerformer);
                GameSystems.Combat.AdvanceTurn(GameSystems.D20.Initiative.CurrentActor);
                return true;
            }

            if (ShouldAutoendTurn(CurrentSequence.tbStatus))
            {
                Logger.Debug("CurSeqNext: \t Action for {0} ({1}) ending turn (autoend)...",
                    GameSystems.MapObject.GetDisplayName(globD20Action.d20APerformer),
                    globD20Action.d20APerformer);
                GameSystems.Combat.AdvanceTurn(GameSystems.D20.Initiative.CurrentActor);
                return true;
            }

            if (!GameSystems.Party.IsPlayerControlled(CurrentSequence.performer))
            {
                _aiTurnActions++;

                if (isSimultPerformer(CurrentSequence.performer) || actnProcState != ActionErrorCode.AEC_OK)
                {
                    GameSystems.Combat.AdvanceTurn(GameSystems.D20.Initiative.CurrentActor);
                }
                else if (_aiTurnActions >= Globals.Config.AITurnMaxActions)
                {
                    Logger.Debug(
                        "Advancing turn for {0} because it exceeded the max. number of follow-up AI actions per turn: {1}",
                        CurrentSequence.performer, _aiTurnActions);
                    GameSystems.Combat.AdvanceTurn(GameSystems.D20.Initiative.CurrentActor);
                }
                else
                {
                    CurrentSequence.IsPerforming = false;
                    GameSystems.AI.AiProcess(CurrentSequence.performer);
                }
            }

            if (GameSystems.Combat.IsCombatActive()
                && !actSeqPickerActive
                && CurrentSequence != null
                && GameSystems.Party.IsPlayerControlled(CurrentSequence.performer)
                && CurrentSequence.tbStatus.baseAttackNumCode
                + CurrentSequence.tbStatus.numBonusAttacks
                > CurrentSequence.tbStatus.attackModeCode
               )
            {
                // I think this is for doing full attack?
                //if (GameSystems.D20.d20Query(CurrentSequence.performer, D20DispatcherKey.QUE_Trip_AOO))
                //*seqPickerD20ActnType = D20ActionType.TRIP;
                //else
                if (seqPickerD20ActnType != D20ActionType.TRIP)
                    seqPickerD20ActnType = D20ActionType.STANDARD_ATTACK;
                seqPickerD20ActnData1 = 0;
                seqPickerTargetingType = D20TargetClassification.SingleExcSelf;
            }
        }

        return true;
    }

    [TempleDllLocation(0x10091540)]
    private bool HasReadiedAction(GameObject critter)
    {
        foreach (var readiedAction in _readiedActions)
        {
            if (readiedAction.interrupter == critter && readiedAction.flags == 1)
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldAutoendTurn(TurnBasedStatus tbStat)
    {
        var curActor = GameSystems.D20.Initiative.CurrentActor;
        if (Globals.Config.EndTurnDefault && performedDefaultAction
            || GameSystems.Critter.IsDeadOrUnconscious(curActor))
        {
            return true;
        }

        var endTurnTimeValue = Globals.Config.EndTurnTime;

        if (endTurnTimeValue != 0)
        {
            if (endTurnTimeValue == 1)
            {
                if (tbStat.hourglassState > HourglassState.EMPTY)
                    return false;
            }

            if (endTurnTimeValue == 2)
            {
                if (tbStat.hourglassState > HourglassState.MOVE)
                    return false;
            }
        }
        else if (tbStat.hourglassState > HourglassState.EMPTY)
        {
            return false;
        }

        if (tbStat.surplusMoveDistance > 4.0f)
        {
            if (endTurnTimeValue != 2
                || tbStat.hourglassState > HourglassState.EMPTY
                || curActor.Dispatch41GetMoveSpeed(out _) < tbStat.surplusMoveDistance)
            {
                return false;
            }
        }

        if (tbStat.attackModeCode < tbStat.baseAttackNumCode
            || tbStat.numBonusAttacks != 0
            || (tbStat.tbsFlags & (TurnBasedStatusFlags.UNK_1 | TurnBasedStatusFlags.Moved)) == 0 &&
            endTurnTimeValue == 0)
        {
            return false;
        }

        return true;
    }

    [TempleDllLocation(0x10098010)]
    private bool SequencePop()
    {
        var curSeq = CurrentSequence;
        var prevSeq = CurrentSequence.prevSeq;
        curSeq.IsPerforming = false;
        Logger.Debug("Popping sequence ( {0} )", curSeq);
        CurrentSequence = prevSeq;
        curSeq.prevSeq = null;
        if (prevSeq == null)
        {
            return false;
        }

        var curSeqPerformer = curSeq.performer;
        var prevSeqPerformer = prevSeq.performer;
        Logger.Debug("Popping sequence from {0} ({1}) to {2} ({3})",
            GameSystems.MapObject.GetDisplayName(curSeqPerformer),
            curSeqPerformer,
            GameSystems.MapObject.GetDisplayName(prevSeqPerformer),
            prevSeqPerformer);

        var tbStatNew = new TurnBasedStatus();
        tbStatNew.hourglassState = HourglassState.FULL;
        tbStatNew.tbsFlags = 0;
        tbStatNew.idxSthg = -1;
        tbStatNew.surplusMoveDistance = 0.0f;
        tbStatNew.attackModeCode = 0;
        tbStatNew.baseAttackNumCode = 0;
        tbStatNew.numBonusAttacks = 0;
        tbStatNew.errCode = 0;

        DispIOTurnBasedStatus dispIo = new DispIOTurnBasedStatus();
        dispIo.tbStatus = tbStatNew;
        globD20ActnSetPerformer(prevSeqPerformer);

        var dispatcher = prevSeqPerformer.GetDispatcher();
        dispatcher?.Process(DispatcherType.TurnBasedStatusInit, 0, dispIo);

        if (GameSystems.Critter.IsDeadOrUnconscious(prevSeqPerformer) && prevSeq.spellPktBody.spellEnum != 0)
        {
            prevSeq.ResetSpell();
        }

        if (GameSystems.D20.D20Query(prevSeqPerformer, D20DispatcherKey.QUE_Prone) &&
            prevSeq.tbStatus.hourglassState >= HourglassState.MOVE)
        {
            prevSeq.d20ActArray.Clear();
            globD20Action.Reset(globD20Action.d20APerformer);
            globD20Action.d20ActType = D20ActionType.STAND_UP;
            globD20Action.data1 = 0;
            ActionAddToSeq();
        }

        actSeqInterrupt = prevSeq.interruptSeq;
        prevSeq.interruptSeq = null;
        return true;
    }

    [TempleDllLocation(0x1008ac70)]
    private bool SequenceHasActionsPendingProjectileHit()
    {
        var curSeq = CurrentSequence;
        if (curSeq.d20aCurIdx < curSeq.d20ActArrayNum)
        {
            return false;
        }

        foreach (var action in curSeq.d20ActArray)
        {
            var actionDef = D20ActionDefs.GetActionDef(action.d20ActType);
            if (actionDef.projectileHitFunc != null && action.d20Caf.HasFlag(D20CAF.NEED_PROJECTILE_HIT))
            {
                return false;
            }
        }

        return true;
    }

    [TempleDllLocation(0x10099360)]
    private bool InterruptCounterspell(D20Action action)
    {
        var triggered = false;
        for (var readiedAction = ReadiedActionGetNext(null, action);
             readiedAction != null;
             readiedAction = ReadiedActionGetNext(readiedAction, action))
        {
            if (readiedAction.readyType == ReadyVsTypeEnum.RV_Counterspell)
            {
                InterruptSwitchActionSequence(readiedAction);
                triggered = true;
            }
        }

        return triggered;
    }

    [TempleDllLocation(0x1008a9c0)]
    internal bool D20ActionTriggersAoO(D20Action action, TurnBasedStatus tbStatus)
    {
        var actSeq = CurrentSequence;
        if (actSeq.tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.CritterSpell))
        {
            return false;
        }

        var flags = action.GetActionDefinitionFlags();
        if (flags.HasFlag(D20ADF.D20ADF_QueryForAoO)
            && GameSystems.D20.D20QueryWithObject(action.d20APerformer, D20DispatcherKey.QUE_ActionTriggersAOO,
                action) != 0)
        {
            if (action.d20ActType == D20ActionType.DISARM)
            {
                // TODO: This is this not in some condition somewhere???
                return GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.IMPROVED_DISARM) == 0;
            }

            return true;
        }

        if (!flags.HasFlag(D20ADF.D20ADF_TriggersAoO))
        {
            return false;
        }

        if (action.d20ActType == D20ActionType.TRIP)
        {
            var weaponUsed =
                GameSystems.D20.GetAttackWeapon(action.d20APerformer, action.data1, (D20CAF) action.d20Caf);

            if (weaponUsed != null && GameSystems.Item.IsTripWeapon(weaponUsed))
            {
                return false;
            }

            if (GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.IMPROVED_UNARMED_STRIKE) != 0)
            {
                return false;
            }

            return GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.IMPROVED_TRIP) == 0;
        }

        if (action.d20ActType == D20ActionType.SUNDER)
        {
            return GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.IMPROVED_SUNDER) == 0;
        }

        // From here on out, we just check as if the action is a standard action.
        // Check if the attack qualifies as "armed".
        if (action.d20Caf.HasFlag(D20CAF.TOUCH_ATTACK)
            || GameSystems.D20.GetAttackWeapon(action.d20APerformer, action.data1, action.d20Caf) != null
            || DispatchD20ActionCheck(action, tbStatus, DispatcherType.GetCritterNaturalAttacksNum) > 0)
        {
            return false;
        }

        return GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.IMPROVED_UNARMED_STRIKE) == 0;
    }

    [TempleDllLocation(0x100981c0)]
    public bool TriggerAoOsByAdjacentEnemies(GameObject obj)
    {
        var status = false;

        var enemies = GameSystems.Combat.GetEnemiesCanMelee(obj);

        foreach (var enemy in enemies)
        {
            bool okToAoo = true;
            if (enemy.HasFlag(ObjectFlag.INVULNERABLE)
               ) // bug? or maybe it also assumes the obj is "trapped" somehow, like in otiluke's resilient sphere
                continue;

            foreach (var sequence in actSeqArray)
            {
                if (sequence.IsPerforming && sequence.performer == enemy)
                {
                    okToAoo = false;
                    Logger.Debug(
                        "TriggerAoOsByAdjacentEnemies({0}({1})): Action Aoo for {2} ({3}) while they are performing...",
                        GameSystems.MapObject.GetDisplayName(obj), obj, GameSystems.MapObject.GetDisplayName(enemy),
                        enemy);
                }
            }

            if (!okToAoo)
                continue;
            if (GameSystems.Critter.IsFriendly(obj, enemy))
                continue;
            if (GameSystems.D20.D20QueryWithObject(enemy, D20DispatcherKey.QUE_AOOPossible, obj) == 0)
                continue;
            if (GameSystems.D20.D20QueryWithObject(enemy, D20DispatcherKey.QUE_AOOWillTake, obj) == 0)
                continue;
            DoAoo(enemy, obj);
            status = true;
        }

        return status;
    }

    [TempleDllLocation(0x10097d50)]
    public void DoAoo(GameObject obj, GameObject target)
    {
        AssignSeq(obj);
        var curSeq = CurrentSequence;
        curSeq.performer = obj; // TODO: AssignSeq should already have taken care of this

        Logger.Debug("AOO - {0} ({1}) is interrupting {2} ({3})",
            GameSystems.MapObject.GetDisplayName(obj), obj,
            GameSystems.MapObject.GetDisplayName(target), target);

        if (obj != globD20Action.d20APerformer)
        {
            SeqPickerTargetingTypeReset(); // TODO: Shouldn't this cancel !?
        }

        var tbStat = curSeq.tbStatus;
        tbStat.tbsFlags = TurnBasedStatusFlags.NONE;
        tbStat.surplusMoveDistance = 0.0f;
        tbStat.attackModeCode = 0;
        tbStat.baseAttackNumCode = 0;
        tbStat.numBonusAttacks = 0;
        tbStat.numAttacks = 0;
        tbStat.errCode = 0;
        tbStat.hourglassState = HourglassState.STD;
        tbStat.idxSthg = -1;
        curSeq.IsInterrupted = true;

        globD20Action.Reset(obj);
        if (GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary) != null)
        {
            globD20Action.data1 = AttackPacket.ATTACK_CODE_PRIMARY + 1;
        }
        else
        {
            if (DispatchD20ActionCheck(globD20Action, tbStat, DispatcherType.GetCritterNaturalAttacksNum) <= 0)
            {
                globD20Action.data1 = AttackPacket.ATTACK_CODE_PRIMARY + 1;
            }
            else
            {
                globD20Action.data1 = AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1;
            }
        }

        globD20Action.d20Caf |= D20CAF.ATTACK_OF_OPPORTUNITY;
        globD20Action.d20ActType = D20ActionType.ATTACK_OF_OPPORTUNITY;
        GlobD20ActnSetTarget(target, target.GetLocationFull());
        ActionAddToSeq();
        GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_AOOPerformed, target);
    }

    [TempleDllLocation(0x1008aa90)]
    private bool CheckAooIncurRegardTumble(D20Action action)
    {
        if (GameSystems.D20.D20Query(action.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                WellKnownSpells.Sanctuary, 0))
        {
            return false; // spell_sanctuary active
        }

        if (IsCurrentlyPerforming(action.d20APerformer))
        {
            Logger.Info("movement aoo while performing...\n");
            return false;
        }

        if (GameSystems.D20.D20QueryWithObject(action.d20ATarget, D20DispatcherKey.QUE_AOOIncurs,
                action.d20APerformer) == 0)
        {
            return false;
        }

        if (GameSystems.D20.D20QueryWithObject(action.d20APerformer, D20DispatcherKey.QUE_AOOPossible,
                action.d20ATarget) == 0)
        {
            return false;
        }

        if (GameSystems.D20.D20QueryWithObject(action.d20APerformer, D20DispatcherKey.QUE_AOOWillTake,
                action.d20ATarget) == 0)
        {
            return false;
        }

        // Allow the AoO victim to tumble out of the way if they have ranks in tumble
        if (action.d20ATarget.IsWearingLightArmorOrLess())
        {
            if (action.d20ATarget.HasRanksIn(SkillId.tumble))
            {
                if (GameSystems.Skill.SkillRoll(action.d20ATarget, SkillId.tumble, 15, out _,
                        SkillCheckFlags.UnderDuress))
                {
                    GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer,
                        D20CombatSystem.MesTumbleSuccessful);
                    return false;
                }

                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatSystem.MesTumbleUnsuccessful);
            }
        }

        return true;
    }

    [TempleDllLocation(0x10096450)]
    private ActionErrorCode CheckActionPreconditions(ActionSequence actSeq, int idx, TurnBasedStatus tbStat)
    {
        if (idx < 0 || idx >= actSeq.d20ActArrayNum)
        {
            return ActionErrorCode.AEC_INVALID_ACTION;
        }

        var action = actSeq.d20ActArray[idx];
        var performerLoc = action.d20APerformer.GetLocationFull();
        var actionDef = D20ActionDefs.GetActionDef(action.d20ActType);

        // target
        var tgtChecker = actionDef.tgtCheckFunc;
        if (tgtChecker != null)
        {
            var targetCheckResult = tgtChecker(action, tbStat);
            if (targetCheckResult != ActionErrorCode.AEC_OK)
            {
                return targetCheckResult;
            }
        }

        // hourglass & action check
        var actionCheckResult = seqCheckAction(action, tbStat);
        if (actionCheckResult != ActionErrorCode.AEC_OK)
        {
            return actionCheckResult;
        }

        // location
        var locChecker = actionDef.locCheckFunc;
        if (locChecker != null)
        {
            var locCheckResult = locChecker(action, tbStat, performerLoc);
            if (locCheckResult != ActionErrorCode.AEC_OK)
            {
                return locCheckResult;
            }
        }

        foreach (var d20Action in actSeq.d20ActArray)
        {
            var otherActionDef = D20ActionDefs.GetActionDef(d20Action.d20ActType);
            if (otherActionDef.flags.HasFlag(D20ADF.D20ADF_DoLocationCheckAtDestination))
            {
                var path = action.path;
                if (path != null)
                {
                    performerLoc = path.to;
                }

                return ActionSequenceChecksRegardLoc(performerLoc, tbStat, idx + 1, actSeq);
            }
        }

        return ActionErrorCode.AEC_OK;
    }

    [TempleDllLocation(0x100960b0)]
    private ActionErrorCode ActionSequenceChecksRegardLoc(LocAndOffsets loc, TurnBasedStatus tbStatus,
        int startIndex, ActionSequence actSeq)
    {
        TurnBasedStatus tbStatCopy = tbStatus.Copy();
        LocAndOffsets locCopy = loc;
        ActionErrorCode result = ActionErrorCode.AEC_OK;

        for (int i = startIndex; i < actSeq.d20ActArrayNum; i++)
        {
            var d20a = actSeq.d20ActArray[i];
            var d20aType = d20a.d20ActType;
            var actionDef = D20ActionDefs.GetActionDef(d20aType);

            var tgtCheckFunc = actionDef.tgtCheckFunc;
            if (tgtCheckFunc != null)
            {
                result = tgtCheckFunc(d20a, tbStatCopy);
                if (result != ActionErrorCode.AEC_OK)
                {
                    return result;
                }
            }

            result = TurnBasedStatusUpdate(d20a, tbStatCopy);
            if (result != ActionErrorCode.AEC_OK)
            {
                tbStatCopy.errCode = result; // ??? WTF maybe debug leftover
                return result;
            }

            var actionCheckFunc = actionDef.actionCheckFunc;
            if (actionCheckFunc != null)
            {
                result = actionCheckFunc(d20a, tbStatCopy);
                if (result != ActionErrorCode.AEC_OK)
                {
                    return result;
                }
            }

            var locCheckFunc = actionDef.locCheckFunc;
            if (locCheckFunc != null)
            {
                result = locCheckFunc(d20a, tbStatCopy, locCopy);
                if (result != ActionErrorCode.AEC_OK)
                {
                    return result;
                }
            }

            var path = d20a.path;
            if (path != null)
            {
                locCopy = path.to;
            }
        }

        return result;
    }

    [TempleDllLocation(0x10093890)]
    private bool combatTriggerSthg(ActionSequence actSeq)
    {
        var performer = actSeq.performer;
        // TODO: This is a rather incomplete copy, really...
        var seqCopy = actSeq.Copy();

        GameSystems.Combat.EnterCombat(performer);

        for (var i = 0; i < actSeq.d20ActArrayNum; i++)
        {
            D20aTriggerCombatCheck(actSeq, i);
        }

        GameSystems.Combat.StartCombat(performer, true);

        if (GameSystems.Party.IsPlayerControlled(performer))
        {
            CurrentSequence = seqCopy;
            return false;
        }

        return true;
    }

    [TempleDllLocation(0x1008AE90)]
    private void D20aTriggerCombatCheck(ActionSequence actSeq, int idx)
    {
        var performer = actSeq.performer;
        var d20a = actSeq.d20ActArray[idx];
        var tgt = d20a.d20ATarget;
        var flags = d20a.GetActionDefinitionFlags();
        if (flags.HasFlag(D20ADF.D20ADF_TriggersCombat))
        {
            GameSystems.Combat.EnterCombat(actSeq.performer);
            if (tgt != null)
            {
                GameSystems.Combat.EnterCombat(tgt);
                GameSystems.AI.ProvokeHostility(performer, tgt, 1, 0);
            }
        }

        if (idx == 0 && (d20a.d20ActType == D20ActionType.CAST_SPELL || d20a.d20ActType == D20ActionType.USE_ITEM))
        {
            var spellPkt = actSeq.spellPktBody;
            var spEnum = spellPkt.spellEnum;
            if (spEnum == 0)
                return;

            var caster = spellPkt.caster;
            foreach (var spTgt in spellPkt.Targets)
            {
                if (!spTgt.Object.IsCritter())
                    continue;
                if (GameSystems.Spell.IsSpellHarmful(spEnum, caster, spTgt.Object))
                {
                    GameSystems.Combat.EnterCombat(performer);
                    GameSystems.Combat.EnterCombat(spTgt.Object);
                }
            }
        }
    }

    [TempleDllLocation(0x1008ad10)]
    private bool ShouldTriggerCombat(ActionSequence actSeq)
    {
        // check if any of the actions triggers combat
        for (var i = 0; i < actSeq.d20ActArrayNum; i++)
        {
            var d20a = actSeq.d20ActArray[i];
            var flags = d20a.GetActionDefinitionFlags();
            if (flags.HasFlag(D20ADF.D20ADF_TriggersCombat))
            {
                if (GameSystems.Party.IsInParty(actSeq.performer))
                    return true;
                if (actSeq.targetObj != null && GameSystems.Party.IsInParty(actSeq.targetObj))
                    return true;
            }
        }

        // check spell targets
        var spPkt = actSeq.spellPktBody;
        if (spPkt.spellEnum == 0)
            return false;
        foreach (var spTgt in spPkt.Targets)
        {
            if (spTgt.Object == null)
                continue;
            if (!spTgt.Object.IsCritter())
                continue;
            if (GameSystems.Spell.IsSpellHarmful(spPkt.spellEnum, spPkt.caster, spTgt.Object))
            {
                if (GameSystems.Party.IsInParty(spTgt.Object) || GameSystems.Party.IsInParty(actSeq.performer))
                    return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x100925e0)]
    public bool isSimultPerformer(GameObject objHnd)
    {
        return _simultPerformerQueue.Contains(objHnd);
    }

    /// <summary>
    /// Checks if the given action sequence can be performed simultaneously with other actions by other
    /// actors.
    /// </summary>
    [TempleDllLocation(0x100926a0)]
    private bool simulsOk(ActionSequence actSeq)
    {
        // Check if all actions in the sequence can be performed concurrently with others,
        // this is also true if the action sequence is empty
        if (actSeq.d20ActArray.TrueForAll(action =>
                action.GetActionDefinitionFlags().HasFlag(D20ADF.D20ADF_SimulsCompatible)))
        {
            return true;
        }

        if (isSomeoneAlreadyActingSimult(actSeq.performer))
        {
            return false;
        }
        else
        {
            _abortedSimultaneousSequence = null;
            _simultPerformerQueue.Clear();
            Logger.Debug("first simul actor, proceeding"); // aborts simuls
        }

        return true;
    }

    [TempleDllLocation(0x100924e0)]
    public bool simulsAbort(GameObject obj)
    {
        // aborts sequence; returns 1 if objHnd is not the first in queue
        if (!GameSystems.Combat.IsCombatActive())
        {
            return false;
        }

        var isFirstInQueue = true;
        foreach (var simultPerformer in _simultPerformerQueue)
        {
            if (obj == simultPerformer)
            {
                if (isFirstInQueue)
                {
                    _abortedSimultaneousSequence = null;
                    _simultPerformerQueue.Clear();
                    return false;
                }
                else
                {
                    _abortedSimultaneousSequence = (_simultPerformerQueue[simulsIdx], CurrentSequence.tbStatus.Copy());
                    _simultPerformerQueue.RemoveRange(simulsIdx, _simultPerformerQueue.Count - simulsIdx);
                    Logger.Debug("Simul aborted {0} ({1})", obj, simulsIdx);
                    return true;
                }
            }

            if (IsCurrentlyPerforming(simultPerformer))
            {
                isFirstInQueue = false;
            }
        }

        return false;
    }

    private bool isSomeoneAlreadyActingSimult(GameObject objHnd)
    {
        foreach (var simultPerformer in _simultPerformerQueue)
        {
            if (objHnd == simultPerformer)
            {
                return false;
            }

            foreach (var actionSequence in actSeqArray)
            {
                if (actionSequence.IsPerforming && actionSequence.performer == simultPerformer)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [TempleDllLocation(0x1008a980)]
    private bool actSeqOkToPerform()
    {
        var curSeq = CurrentSequence;
        if ((curSeq.IsPerforming) && (curSeq.d20aCurIdx >= 0) && curSeq.d20aCurIdx < curSeq.d20ActArrayNum)
        {
            var caflags = curSeq.d20ActArray[curSeq.d20aCurIdx].d20Caf;
            if (caflags.HasFlag(D20CAF.NEED_PROJECTILE_HIT))
            {
                return false;
            }

            return !caflags.HasFlag(D20CAF.NEED_ANIM_COMPLETED);
        }

        return true;
    }

    [TempleDllLocation(0x10098c90)]
    public bool DoUseItemAction(GameObject holder, GameObject aiObj, GameObject item)
    {
        // Save to be able to revert
        var d20ActNum = CurrentSequence.d20ActArrayNum;

        if (GameSystems.Item.GetItemSpellCharges(item) == -1)
        {
            return false;
        }

        var spData_1 = item.GetSpell(obj_f.item_spell_idx, 0);
        if (spData_1.spellEnum <= 0)
        {
            return false;
        }

        var spellPktBody = new SpellPacketBody();
        spellPktBody.spellKnownSlotLevel = spData_1.spellLevel;
        spellPktBody.spellEnum = spData_1.spellEnum;
        spellPktBody.spellEnumOriginal = spData_1.spellEnum;
        spellPktBody.caster = holder;
        spellPktBody.spellClass = spData_1.classCode;
        spellPktBody.metaMagicData = spData_1.metaMagicData;
        GameSystems.Spell.SpellPacketSetCasterLevel(spellPktBody);

        var itemInvIdx = item.GetInt32(obj_f.item_inv_location);
        D20SpellData spData = new D20SpellData(spellPktBody.spellEnum, spellPktBody.spellClass,
            spellPktBody.spellKnownSlotLevel,
            itemInvIdx, spellPktBody.metaMagicData);
        if (aiObj != null)
        {
            GameSystems.Spell.GetSpellTargets(holder, holder, spellPktBody, spellPktBody.spellEnum);
        }

        GlobD20ActnInit();
        globD20Action.data1 = itemInvIdx;
        globD20Action.d20ActType = D20ActionType.USE_POTION;
        globD20Action.d20SpellData = spData;
        if (RunUseItemActionCheck /*0x10096390*/(holder, D20ActionType.USE_ITEM, itemInvIdx, spData) !=
            ActionErrorCode.AEC_OK)
        {
            return false;
        }

        if (aiObj != null)
        {
            var locAndOffOut = holder.GetLocationFull();
            CurrentSequence.spellPktBody = spellPktBody;
            CurrentSequence.ignoreLos = false;
            GlobD20ActnSetTarget(holder, locAndOffOut);
        }

        ActionAddToSeq();

        if (aiObj != null && ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
        {
            ActionSequenceRevertPath(d20ActNum);
            ActSeqSpellReset();
            return false;
        }

        return true;
    }

    [TempleDllLocation(0x100930a0)]
    public void ActSeqSpellReset()
    {
        CurrentSequence?.ResetSpell();
        ActSeqTargetsClear();
    }

    [TempleDllLocation(0x10096390)]
    public ActionErrorCode RunUseItemActionCheck(GameObject obj, D20ActionType actionType, int invIdx,
        D20SpellData spellData)
    {
        var tbStatus = CurrentSequence.tbStatus.Copy();
        var d20a = new D20Action(actionType, obj);
        d20a.data1 = invIdx;
        d20a.d20SpellData = spellData; // TODO: We might want to copy here
        var result = TurnBasedStatusUpdate(d20a, tbStatus);
        if (result == ActionErrorCode.AEC_OK)
        {
            var actionDef = D20ActionDefs.GetActionDef(d20a.d20ActType);
            if (actionDef.actionCheckFunc != null)
            {
                result = actionDef.actionCheckFunc(d20a, tbStatus);
            }
        }

        return result;
    }

    [TempleDllLocation(0x10099cf0)]
    public void PerformOnAnimComplete(GameObject obj, int animId)
    {
        // do checks:

        // obj is performing
        if (!IsCurrentlyPerforming(obj))
        {
            if (animId != 0)
            {
                Logger.Debug("PerformOnAnimComplete: \t Animation {0} Completed for {1}; Not performing.",
                    animId, obj);
            }

            return;
        }

        Logger.Debug("PerformOnAnimComplete: \t Animation {0} Completed for {1}", animId, obj);

        // does the Current Sequence belong to obj?
        var curSeq = CurrentSequence;
        var savedSequence = curSeq; // Save the sequence so we can restore it if something goes wrong

        if (curSeq == null || curSeq.performer != obj || !curSeq.IsPerforming)
        {
            Logger.Debug("\tCurrent sequence performer is {0}, Switching sequence...", curSeq.performer);
            if (!SequenceSwitch(obj))
            {
                Logger.Debug("\tFailed.");
                return;
            }

            curSeq = CurrentSequence;
            Logger.Debug("\tNew Current sequence performer is {0}", curSeq.performer);
        }

        // is the animId ok?
        var action = curSeq.d20ActArray[curSeq.d20aCurIdx];

        if (animId != -1 && animId != 0xccccCCCC && (animId != 0) && action.animID != animId)
        {
            Logger.Debug("PerformOnAnimComplete: \t Wrong anim ID: {0} != {1}",
                animId, action.animID);
            CurrentSequence = savedSequence;
            return;
        }

        // is the action performer correct?
        if (obj != action.d20APerformer)
        {
            Logger.Debug("PerformOnAnimComplete: \t Wrong performer: {0} != {1}",
                obj, action.d20APerformer);
            CurrentSequence = savedSequence;
            return;
        }

        // Is the action even waiting for an animation to complete?
        if (!action.d20Caf.HasFlag(D20CAF.NEED_ANIM_COMPLETED))
        {
            return;
        }

        Logger.Debug("PerformOnAnimComplete: \t Performing.");
        action.d20Caf &= ~D20CAF.NEED_ANIM_COMPLETED;

        if (!action.d20Caf.HasFlag(D20CAF.ACTIONFRAME_PROCESSED))
        {
            ActionFrameProcess(obj);
        }

        OnActionEnded?.Invoke(action);

        if (actSeqOkToPerform())
        {
            ActionBroadcastAndSignalMoved();
            ActionPerform();
            while (CurrentSequence != null && IsCurrentlyPerforming(CurrentSequence.performer) && actSeqOkToPerform())
            {
                ActionPerform();
            }
        }
    }

    [TempleDllLocation(0x1008abf0)]
    private void ActionBroadcastAndSignalMoved()
    {
        var action = CurrentSequence.d20ActArray[CurrentSequence.d20aCurIdx];
        GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Broadcast_Action, action);

        var distTrav = (int) action.distTraversed;
        switch (action.d20ActType)
        {
            case D20ActionType.UNSPECIFIED_MOVE:
            case D20ActionType.FIVEFOOTSTEP:
            case D20ActionType.MOVE:
            case D20ActionType.DOUBLE_MOVE:
            case D20ActionType.RUN:
            case D20ActionType.CHARGE:
                GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Combat_Critter_Moved,
                    distTrav);
                break;
            default:
                return;
        }
    }

    [TempleDllLocation(0x100933F0)]
    public bool ActionFrameProcess(GameObject obj)
    {
        Logger.Debug("ActionFrameProcess: \t for {0}", obj);
        if (!IsCurrentlyPerforming(obj))
        {
            Logger.Debug("Not performing!");
            return false;
        }

        var curSeq = CurrentSequence;
        if (curSeq == null)
        {
            Logger.Debug("No sequence!");
            return false;
        }

        if (curSeq.performer != obj)
        {
            Logger.Debug("..Switching sequence from {0}", CurrentSequence);
            if (!SequenceSwitch(obj))
            {
                Logger.Debug("..failed!");
                return false;
            }

            curSeq = CurrentSequence;
            Logger.Debug("..to {0}", curSeq);
        }

        var d20a = curSeq.d20ActArray[curSeq.d20aCurIdx];
        if (obj != d20a.d20APerformer)
        {
            return false;
        }

        if (d20a.d20Caf.HasFlag(D20CAF.ACTIONFRAME_PROCESSED))
        {
            Logger.Debug("ActionFrameProcess: \t Action frame already processed.");
            return false;
        }

        d20a.d20Caf |= D20CAF.ACTIONFRAME_PROCESSED;
        var actFrameFunc = D20ActionDefs.GetActionDef(d20a.d20ActType).actionFrameFunc;
        if (actFrameFunc == null)
        {
            Logger.Debug("Has no action frame function");
            return false;
        }

        Logger.Debug("ActionFrameProcess: \t Calling action frame function for {0}", d20a.d20ActType);
        return actFrameFunc(d20a);
    }

    [TempleDllLocation(0x10089f00)]
    public bool WasInterrupted(GameObject obj)
    {
        for (var sequence = actSeqInterrupt; sequence != null; sequence = sequence.interruptSeq)
        {
            if (sequence.performer == obj)
            {
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x1008b7e0)]
    public GameObject GetInterruptee()
    {
        GameObject result = null;
        var interruptedSeq = actSeqInterrupt;
        for (; interruptedSeq != null; interruptedSeq = interruptedSeq.interruptSeq)
        {
            result = interruptedSeq.performer;
        }

        return result;
    }

    [TempleDllLocation(0x1004e790)]
    public void dispatchTurnBasedStatusInit(GameObject obj, DispIOTurnBasedStatus dispIo)
    {
        var dispatcher = obj.GetDispatcher();
        if (dispatcher != null)
        {
            dispatcher.Process(DispatcherType.TurnBasedStatusInit, D20DispatcherKey.NONE, dispIo);
            if (dispIo.tbStatus != null)
            {
                if (dispIo.tbStatus.hourglassState > 0)
                {
                    GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_BeginTurn);
                }
            }
        }
    }

    [TempleDllLocation(0x10099b10)]
    public void ProjectileHit(GameObject projectile, GameObject critter)
    {
        throw new NotImplementedException();
    }


    [TempleDllLocation(0x118CD3B8)]
    internal D20TargetClassification seqPickerTargetingType = D20TargetClassification.Invalid;

    [TempleDllLocation(0x118A0980)] internal D20ActionType seqPickerD20ActnType = D20ActionType.UNSPECIFIED_ATTACK;

    [TempleDllLocation(0x118CD570)] internal int seqPickerD20ActnData1; // init to 0

    [TempleDllLocation(0x1008a0f0)]
    public bool SeqPickerHasTargetingType()
    {
        return seqPickerTargetingType != D20TargetClassification.Invalid;
    }

    [TempleDllLocation(0x1008a0b0)]
    public void SeqPickerTargetingTypeReset()
    {
        seqPickerTargetingType = D20TargetClassification.Invalid;
        seqPickerD20ActnType = D20ActionType.UNSPECIFIED_ATTACK;
        seqPickerD20ActnData1 = 0;
    }

    [TempleDllLocation(0x1008a050)]
    public bool IsCurrentlyPerforming(GameObject performer) => IsCurrentlyPerforming(performer, out _);

    [TempleDllLocation(0x1008a050)]
    public bool IsCurrentlyPerforming(GameObject performer, [NotNullWhen(true)] out ActionSequence? sequenceOut)
    {
        foreach (var actionSequence in actSeqArray)
        {
            if (actionSequence.performer == performer && actionSequence.IsPerforming)
            {
                sequenceOut = actionSequence;
                return true;
            }
        }

        sequenceOut = null;
        return false;
    }

    [TempleDllLocation(0x1008a4b0)]
    public D20TargetClassification TargetClassification(D20Action action)
    {
        // direct access to action flags - intentional
        var d20DefFlags = action.GetActionDefinitionFlags();

        if (d20DefFlags.HasFlag(D20ADF.D20ADF_Python))
        {
            return GetPythonAction(action.data1).tgtClass;
        }

        if (d20DefFlags.HasFlag(D20ADF.D20ADF_Movement))
        {
            return D20TargetClassification.Movement;
        }

        if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetSingleIncSelf))
            return D20TargetClassification.SingleIncSelf;
        if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetSingleExcSelf))
            return D20TargetClassification.SingleExcSelf;
        if (d20DefFlags.HasFlag(D20ADF.D20ADF_MagicEffectTargeting))
            return D20TargetClassification.CastSpell;
        if (d20DefFlags.HasFlag(D20ADF.D20ADF_CallLightningTargeting))
            return D20TargetClassification.CallLightning;
        if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetContainer))
            return D20TargetClassification.ItemInteraction;
        if (d20DefFlags.HasFlag(D20ADF.D20ADF_TargetingBasedOnD20Data))
        {
            switch ((BardicMusicSongType) action.data1)
            {
                case BardicMusicSongType.BM_FASCINATE:
                case BardicMusicSongType.BM_INSPIRE_COMPETENCE:
                case BardicMusicSongType.BM_SUGGESTION:
                case BardicMusicSongType.BM_INSPIRE_GREATNESS:
                    return D20TargetClassification.SingleExcSelf;
                case BardicMusicSongType.BM_INSPIRE_HEROICS:
                    return D20TargetClassification.SingleIncSelf;
                default:
                    return D20TargetClassification.Target0;
            }
        }

        return D20TargetClassification.Target0;
    }

    [TempleDllLocation(0x10092e50)]
    public ActionErrorCode GlobD20ActnSetTarget(GameObject? target, LocAndOffsets? location)
    {
        switch (TargetClassification(globD20Action))
        {
            case D20TargetClassification.Movement:
                if (location.HasValue)
                {
                    globD20Action.destLoc = location.Value;
                }
                else
                {
                    globD20Action.destLoc = target.GetLocationFull();
                    globD20Action.d20ATarget = target;
                }

                globD20Action.distTraversed = GameSystems.D20.Combat.EstimateDistance(globD20Action.d20APerformer,
                    globD20Action.destLoc.location, 0, 0.0);
                return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
            default:
                return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
            case D20TargetClassification.SingleExcSelf:
            case D20TargetClassification.SingleIncSelf:
            case D20TargetClassification.ItemInteraction:
                if (location.HasValue)
                {
                    globD20Action.destLoc = location.Value;
                }
                else if (target != null)
                {
                    globD20Action.destLoc = target.GetLocationFull();
                }

                globD20Action.d20ATarget = target;
                return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
            case D20TargetClassification.Target0:
                globD20Action.d20ATarget = target;
                if (target != null)
                {
                    globD20Action.destLoc = target.GetLocationFull();
                    globD20Action.distTraversed = globD20Action.d20APerformer.DistanceToObjInFeet(target);
                }
                else
                {
                    if (location.HasValue)
                    {
                        globD20Action.destLoc = location.Value;
                    }

                    globD20Action.distTraversed = 0;
                }

                return TargetCheck(globD20Action) ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_INVALID;
            case D20TargetClassification.CastSpell:
                if (location.HasValue)
                {
                    globD20Action.destLoc = location.Value;
                }
                else if (target != null)
                {
                    globD20Action.destLoc = target.GetLocationFull();
                }

                globD20Action.d20ATarget = target;
                return ActionErrorCode.AEC_OK;
        }
    }

    [TempleDllLocation(0x1008a580)]
    public bool TargetCheck(D20Action action)
    {
        var target = action.d20ATarget;

        var curSeq = CurrentSequence;
        Debug.Assert(curSeq != null);
        switch (TargetClassification(action))
        {
            case D20TargetClassification.SingleExcSelf:
                if (target == action.d20APerformer)
                    return false;
                goto case D20TargetClassification.SingleIncSelf;
            case D20TargetClassification.SingleIncSelf:
                if (target == null)
                    return false;
                return target.IsCritter();
            case D20TargetClassification.ItemInteraction:
                if (target == null)
                    return false;
                if (target.type == ObjectType.container)
                    return true;
                if (target.IsCritter())
                    return GameSystems.Critter.IsDeadNullDestroyed(target);
                if (target.type == ObjectType.portal)
                    return true;
                return false;
            case D20TargetClassification.CallLightning:
                return actSeqTargetsIdx >= 0;

            case D20TargetClassification.CastSpell:
                // TODO: Why the hell is it setting up a new spell packet here rather than relying on the spell system !?
                curSeq.castSpellAction = action;
                if (curSeq.spellPktBody.caster != null || curSeq.spellPktBody.spellEnum != 0)
                    return true;

                var spellPacket = new SpellPacketBody();
                curSeq.spellPktBody = spellPacket;
                var spellEnum = action.d20SpellData.SpellEnum;
                spellPacket.spellEnum = spellEnum;
                spellPacket.spellEnumOriginal = action.d20SpellData.spellEnumOrg;
                spellPacket.caster = action.d20APerformer;
                spellPacket.spellClass = action.d20SpellData.spellClassCode;
                var spellSlotLevel = action.d20SpellData.spellSlotLevel;
                spellPacket.spellKnownSlotLevel = spellSlotLevel;
                spellPacket.metaMagicData = action.d20SpellData.metaMagicData;
                spellPacket.invIdx = action.d20SpellData.itemSpellData;

                if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spellEntry))
                {
                    Logger.Warn("Perform Cast Spell: failed to retrieve spell entry {0}!", spellEnum);
                    return true;
                }

                // set caster level
                if (spellPacket.invIdx == INV_IDX_INVALID)
                {
                    GameSystems.Spell.SpellPacketSetCasterLevel(spellPacket);
                }
                else
                {
                    // item spell
                    spellPacket.casterLevel =
                        Math.Max(1, 2 * spellSlotLevel - 1); // todo special handling for Magic domain
                }

                spellPacket.spellRange = GameSystems.Spell.GetSpellRange(spellEntry,
                    spellPacket.casterLevel, spellPacket.caster);

                if (spellEntry.modeTargetSemiBitmask.GetBaseMode() != UiPickerType.Personal
                    || spellEntry.radiusTarget < 0
                    || spellEntry.flagsTargetBitmask.HasFlag(UiPickerFlagsTarget.Radius))
                    return false;
                spellPacket.SetTargets(new[] {spellPacket.caster});
                spellPacket.InitialTargets = new[] {spellPacket.caster}.ToImmutableSortedSet();

                spellPacket.aoeCenter = spellPacket.caster.GetLocationFull();
                spellPacket.aoeCenterZ = spellPacket.caster.OffsetZ;
                if (spellEntry.radiusTarget > 0)
                    spellPacket.spellRange = spellEntry.radiusTarget;
                return true;

            default:
                return true;
        }
    }

    public void ActionTypeAutomatedSelection(GameObject? obj)
    {
        void SetGlobD20Action(D20ActionType actType, int data1)
        {
            globD20Action.d20ActType = actType;
            globD20Action.data1 = data1;
        }

        D20TargetClassification targetingType = D20TargetClassification.Movement; // default
        if (obj != null)
        {
            switch (obj.type)
            {
                case ObjectType.portal:
                case ObjectType.scenery:
                    targetingType = D20TargetClassification.Movement;
                    break;

                case ObjectType.container:
                case ObjectType.projectile:
                case ObjectType.weapon:
                case ObjectType.ammo:
                case ObjectType.armor:
                case ObjectType.money:
                case ObjectType.food:
                case ObjectType.scroll:
                case ObjectType.key:
                case ObjectType.written:
                case ObjectType.generic:
                case ObjectType.trap:
                case ObjectType.bag:
                    targetingType = D20TargetClassification.ItemInteraction;
                    break;
                case ObjectType.pc:
                case ObjectType.npc:
                    targetingType = D20TargetClassification.SingleExcSelf;
                    break;
                default:
                    targetingType = D20TargetClassification.Movement;
                    break;
            }
        }

        if (seqPickerTargetingType == targetingType
            || seqPickerTargetingType == D20TargetClassification.SingleIncSelf &&
            targetingType == D20TargetClassification.SingleExcSelf
            || seqPickerTargetingType != D20TargetClassification.Invalid)
        {
            SetGlobD20Action(seqPickerD20ActnType, seqPickerD20ActnData1);
            return;
        }


        // if no targeting type defined for picker:
        if (obj == null)
        {
            SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
            return;
        }

        switch (obj.type)
        {
            case ObjectType.portal:
            case ObjectType.scenery:
                SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                return;
            case ObjectType.projectile:
            case ObjectType.weapon:
            case ObjectType.ammo:
            case ObjectType.armor:
            case ObjectType.money:
            case ObjectType.food:
            case ObjectType.scroll:
            case ObjectType.key:
            case ObjectType.written:
            case ObjectType.generic:
            case ObjectType.trap:
            case ObjectType.bag:
                SetGlobD20Action(D20ActionType.PICKUP_OBJECT, 0);
                return;
            case ObjectType.container:
                SetGlobD20Action(D20ActionType.OPEN_CONTAINER, 0);
                return;
            case ObjectType.pc:
            case ObjectType.npc:
                break;
            default:
                return;
        }

        // Critters

        var performer = globD20Action.d20APerformer;
        if (GameSystems.Critter.IsFriendly(obj, performer))
        {
            // || GameSystems.Critter.NpcAllegianceShared(handle, performer)){ // NpcAllegianceShared check is currently not a good idea since ToEE has poor Friend or Foe tracking when it comes to faction mates...

            var performerLeader = GameSystems.Critter.GetLeader(performer);
            if (performerLeader == null || GameSystems.Critter.IsFriendly(obj, performerLeader))
            {
                if (!GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_HoldingCharge))
                {
                    SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                    return;
                }
            }
        }
        else if (GameSystems.Critter.IsDeadNullDestroyed(obj) && GameSystems.Critter.IsLootableCorpse(obj))
        {
            SetGlobD20Action(D20ActionType.OPEN_CONTAINER, 0);
            return;
        }

        SetGlobD20Action(D20ActionType.UNSPECIFIED_ATTACK, 0);
    }

    private struct ProjectileEntry
    {
        public D20Action d20a;
        public int pad4;
        public GameObject projectile;
        public GameObject ammoItem;
    }

    [TempleDllLocation(0x118A0720)] private readonly List<ProjectileEntry> _projectiles = new();

    [TempleDllLocation(0x10B3D5A4)] private ActionErrorCode actnProcState;

    [TempleDllLocation(0x10B3D5C4)] private bool performedDefaultAction;

    [TempleDllLocation(0x1008B1E0)]
    public bool ProjectileAppend(D20Action action, GameObject? projectile, GameObject thrownItem)
    {
        if (projectile == null)
        {
            return false;
        }

        _projectiles.Add(new ProjectileEntry
        {
            d20a = action,
            ammoItem = thrownItem,
            projectile = projectile
        });

        return true;
    }

    public D20ADF GetActionFlags(D20ActionType d20ActionType)
    {
        var flags = D20ActionDefs.GetActionDef(d20ActionType).flags;
        if (flags.HasFlag(D20ADF.D20ADF_Python))
        {
            // enforce python flag in case it's not defined elsewhere
            return _pythonActions[(int) globD20ActionKey].flags | D20ADF.D20ADF_Python;
        }

        return flags;
    }

    [TempleDllLocation(0x1004e0d0)]
    public int DispatchD20ActionCheck(D20Action action, TurnBasedStatus turnBasedStatus,
        DispatcherType dispType)
    {
        var dispatcherKey = D20DispatcherKey.D20A_UNSPECIFIED_MOVE + (int) action.d20ActType;

        var dispatcher = action.d20APerformer.GetDispatcher();
        if (dispatcher != null)
        {
            var dispIo = new DispIoD20ActionTurnBased(action);
            dispIo.tbStatus = turnBasedStatus;
            if (dispType == DispatcherType.GetBonusAttacks)
            {
                dispIo.bonlist = BonusList.Default;
                dispatcher.Process(dispType, dispatcherKey, dispIo);
                return (int) (dispIo.returnVal + dispIo.bonlist.OverallBonus);
            }
            else
            {
                dispatcher.Process(dispType, dispatcherKey, dispIo);
                return (int) dispIo.returnVal;
            }
        }

        return 0;
    }

    public PythonActionSpec GetPythonAction(int id) => _pythonActions[id];

    [TempleDllLocation(0x10095450)]
    public ActionErrorCode AddToSeqWithTarget(D20Action action, ActionSequence sequence, TurnBasedStatus tbStatus)
    {
        var target = action.d20ATarget;
        var actNum = sequence.d20ActArrayNum;
        if (target == null)
        {
            return ActionErrorCode.AEC_TARGET_INVALID;
        }

        // check if target is within reach
        var reach = action.d20APerformer.GetReach(action.d20ActType);
        if (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) < reach)
        {
            sequence.d20ActArray.Add(action);
            return ActionErrorCode.AEC_OK;
        }

        // if not, add a move sequence
        var actionCopy = action.Copy();
        actionCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
        actionCopy.destLoc = target.GetLocationFull();
        var result = MoveSequenceParse(actionCopy, sequence, tbStatus, 0.0f, reach, true);
        if (result == ActionErrorCode.AEC_OK)
        {
            var tbStatusCopy = tbStatus.Copy();
            sequence.d20ActArray.Add(action);
            
            for (; actNum < sequence.d20ActArrayNum; actNum++)
            {
                var otherAction = sequence.d20ActArray[actNum];
                result = TurnBasedStatusUpdate(otherAction, tbStatusCopy);
                if (result != ActionErrorCode.AEC_OK)
                {
                    tbStatusCopy.errCode = result;
                    return result;
                }

                var actionType = otherAction.d20ActType;
                var actionCheckFunc = D20ActionDefs.GetActionDef(actionType).actionCheckFunc;
                result = actionCheckFunc(otherAction, tbStatusCopy);
                if (result != ActionErrorCode.AEC_OK)
                {
                    return result;
                }
            }

            tbStatusCopy.errCode = result;
        }

        return result;
    }

    [TempleDllLocation(0x10B3D5C8)]
    [TempleDllLocation(0x100927f0)]
    public bool rollbackSequenceFlag { get; private set; }

    [TempleDllLocation(0x10B3D5C0)] public bool performingDefaultAction { get; set; }

    [TempleDllLocation(0x10094f70)]
    internal ActionErrorCode MoveSequenceParse(D20Action d20aIn, ActionSequence actSeq, TurnBasedStatus tbStat,
        float distToTgtMin, float reach, bool nonspecificMoveType)
    {
        D20Action d20aCopy;

        seqCheckFuncs(out var tbStatCopy);

        // if prone, add a standup action
        if (GameSystems.D20.D20Query(d20aIn.d20APerformer, D20DispatcherKey.QUE_Prone))
        {
            d20aCopy = d20aIn.Copy();
            d20aCopy.d20ActType = D20ActionType.STAND_UP;
            actSeq.d20ActArray.Add(d20aCopy);
        }

        d20aCopy = d20aIn.Copy();
        var d20a = d20aIn;

        var pathQ = new PathQuery();
        pathQ.critter = d20aCopy.d20APerformer;
        pathQ.from = actSeq.performerLoc;

        if (d20a.d20ATarget != null)
        {
            pathQ.targetObj = d20a.d20ATarget;
            if (pathQ.critter == pathQ.targetObj)
                return ActionErrorCode.AEC_TARGET_INVALID;

            pathQ.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_800
                          | PathQueryFlags.PQF_TARGET_OBJ | PathQueryFlags.PQF_ADJUST_RADIUS |
                          PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS;

            if (reach < 0.1f)
            {
                reach = 3.0f; // 3 feet minimum reach
            }

            actSeq.targetObj = d20a.d20ATarget;
            pathQ.distanceToTargetMin = distToTgtMin * 12.0f; // Feet to inches
            var fourPointSevenPlusEight = locXY.INCH_PER_SUBTILE / 2 + 8.0f;
            pathQ.tolRadius = reach * 12.0f - fourPointSevenPlusEight; // Feet to inches
        }
        else
        {
            pathQ.to = d20aIn.destLoc;
            pathQ.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_800
                          | PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
        }

        if (d20a.d20Caf.HasFlag(D20CAF.UNNECESSARY))
        {
            pathQ.flags |= PathQueryFlags.PQF_A_STAR_TIME_CAPPED;
        }

        // frees the last path used in the d20a
        ReleasePooledPathQueryResult(ref d20aCopy.path);

        // get new path result slot
        // find path
        rollbackSequenceFlag = false;
        if (d20aCopy.d20Caf.HasFlag(D20CAF.CHARGE) || d20a.d20ActType == D20ActionType.RUN ||
            d20a.d20ActType == D20ActionType.DOUBLE_MOVE)
        {
            pathQ.flags |= PathQueryFlags.PQF_DONT_USE_PATHNODES; // so it runs in a straight line
        }

        var pathResult = GameSystems.PathX.FindPath(pathQ, out d20aCopy.path);
        var pqResult = d20aCopy.path;
        if (!pathResult)
        {
            if (pqResult.flags.HasFlag(PathFlags.PF_TIMED_OUT))
            {
                rollbackSequenceFlag = true;
            }

            if (pathQ.targetObj != null)
                Logger.Debug("MoveSequenceParse: FAILED PATH... {0} attempted from {1} to {2} ({3})",
                    GameSystems.MapObject.GetDisplayName(pqResult.mover), pqResult.from, pqResult.to,
                    GameSystems.MapObject.GetDisplayName(pathQ.targetObj));
            else
                Logger.Debug("MoveSequenceParse: FAILED PATH... {0} attempted from {1} to {2}",
                    GameSystems.MapObject.GetDisplayName(pqResult.mover), pqResult.from, pqResult.to);

            ReleasePooledPathQueryResult(ref d20aCopy.path);
            return ActionErrorCode.AEC_TARGET_INVALID;
        }

        var pathLength = pqResult.GetPathResultLength();
        d20aCopy.destLoc = pqResult.to;
        d20aCopy.distTraversed = pathLength;

        if (pathLength < 0.1f)
        {
            ReleasePooledPathQueryResult(ref d20aCopy.path);
            return ActionErrorCode.AEC_OK;
        }

        if (!GameSystems.Combat.IsCombatActive())
        {
            d20aCopy.distTraversed = 0;
            pathLength = 0.0f;
        }

        // deducting moves that have already been spent, but also a raw calculation (not taking 5' step and such into account)
        if (GetRemainingMaxMoveLength(d20a, tbStatCopy, out var remainingMaxMoveLength))
        {
            if (remainingMaxMoveLength < 0.1)
            {
                ReleasePooledPathQueryResult(ref d20aCopy.path);
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            if (remainingMaxMoveLength < pathLength)
            {
                TrimPathToRemainingMoveLength(d20aCopy, remainingMaxMoveLength, pathQ);
                pqResult = d20aCopy.path;
                pathLength = remainingMaxMoveLength;
            }
        }

        /*
        this is 0 for specific move action types like 5' step, Move, Run, Withdraw;
        */
        if (nonspecificMoveType)
        {
            var chosenActionType = D20ActionType.MOVE;

            float baseMoveDist = d20aCopy.d20APerformer.Dispatch41GetMoveSpeed(out _);
            if (!d20aCopy.d20Caf.HasFlag(D20CAF.CHARGE))
            {
                if (pathLength > tbStatCopy.surplusMoveDistance)
                {
                    // distance greater than twice base distance . cannot do
                    if (2 * baseMoveDist + tbStatCopy.surplusMoveDistance < pathLength)
                    {
                        ReleasePooledPathQueryResult(ref d20aCopy.path);
                        return ActionErrorCode.AEC_TARGET_TOO_FAR;
                    }

                    // distance greater than base move distance . do double move
                    if (tbStatCopy.surplusMoveDistance + baseMoveDist < pathLength)
                    {
                        chosenActionType = D20ActionType.DOUBLE_MOVE;
                    }

                    // check if under 5'
                    else if (pathLength <= 5.0f)
                    {
                        if (d20a.d20ActType != D20ActionType.UNSPECIFIED_MOVE)
                        {
                            var actCost = new ActionCostPacket();
                            D20ActionDefs.GetActionDef(d20a.d20ActType).actionCost(d20a, tbStatCopy, actCost);

                            if (actCost.hourglassCost == ActionCostType.FullRound ||
                                tbStatCopy.hourglassState == HourglassState.EMPTY)
                            {
                                chosenActionType = D20ActionType.FIVEFOOTSTEP;
                            }
                        }
                        else
                        {
                            // added for AI to take 5' steps when it still has full round action to exploit
                            if (tbStatCopy.hourglassState == HourglassState.EMPTY ||
                                tbStatCopy.hourglassState == HourglassState.FULL &&
                                !GameSystems.Party.IsPlayerControlled(d20a.d20APerformer))
                            {
                                chosenActionType = D20ActionType.FIVEFOOTSTEP;
                                if ((tbStatCopy.tbsFlags &
                                     (TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.Moved5FootStep)) != 0)
                                {
                                    chosenActionType = D20ActionType.MOVE;
                                }
                            }
                        }
                    }
                }

                d20aCopy.d20ActType = chosenActionType;
            }
            else if (2 * baseMoveDist >= pathLength)
            {
                chosenActionType = D20ActionType.RUN;
            }
            else
            {
                ReleasePooledPathQueryResult(ref d20aCopy.path);
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            d20aCopy.d20ActType = chosenActionType;
        }

        actSeq.performerLoc = pqResult.to;

        ProcessPathForReadiedActions(d20aCopy, out var readiedAction);
        ProcessSequenceForAoOs(actSeq, d20aCopy);
        if (readiedAction != null)
        {
            AddReadiedInterrupt(actSeq, readiedAction);
        }

        UpdateDistTraversed(actSeq);
        return 0;
    }

    [TempleDllLocation(0x1008bb40)]
    private void ProcessSequenceForAoOs(ActionSequence actSeq, D20Action action)
    {
        float distFeet = 0.0f;
        var pqr = action.path;
        var d20ActionTypePostAoO = action.d20ActType;
        bool addingAoOStatus = true;
        if (d20ActionTypePostAoO == D20ActionType.FIVEFOOTSTEP)
        {
            actSeq.d20ActArray.Add(action.Copy());
            return;
        }

        if (d20ActionTypePostAoO == D20ActionType.DOUBLE_MOVE)
        {
            distFeet = 5.0f;
            d20ActionTypePostAoO = D20ActionType.MOVE;
        }

        if (!GameSystems.D20.Combat.FindAttacksOfOpportunity(action.d20APerformer, action.path, distFeet,
                out var attacks))
        {
            actSeq.d20ActArray.Add(action.Copy());
            return;
        }

        var d20aAoOMovement = new D20Action(D20ActionType.AOO_MOVEMENT);
        var startDistFeet = 0.0f;
        var endDistFeet = action.path.GetPathResultLength();
        foreach (var attack in attacks)
        {
            if (attack.Distance != startDistFeet)
            {
                d20aAoOMovement = action.Copy();
                var aooDistFeet = attack.Distance;

                GameSystems.PathX.GetPartialPath(pqr, out var pqrTrunc, startDistFeet, aooDistFeet);
                d20aAoOMovement.path = pqrTrunc;

                startDistFeet = attack.Distance;
                d20aAoOMovement.destLoc = attack.Location;
                d20aAoOMovement.distTraversed = pqrTrunc.GetPathResultLength();
                if (!addingAoOStatus)
                {
                    d20aAoOMovement.d20ActType = d20ActionTypePostAoO;
                }

                addingAoOStatus = false;
                actSeq.d20ActArray.Add(d20aAoOMovement);
            }

            d20aAoOMovement.d20APerformer = attack.Interrupter;
            d20aAoOMovement.d20ATarget = action.d20APerformer;
            d20aAoOMovement.destLoc = attack.Location;
            d20aAoOMovement.d20Caf = 0;
            d20aAoOMovement.distTraversed = 0;
            d20aAoOMovement.path = null;
            d20aAoOMovement.d20ActType = D20ActionType.AOO_MOVEMENT;
            d20aAoOMovement.data1 = 1;
            actSeq.d20ActArray.Add(d20aAoOMovement.Copy());
        }

        d20aAoOMovement = action.Copy();

        GameSystems.PathX.GetPartialPath(pqr, out var pqrLastStretch, startDistFeet, endDistFeet);
        d20aAoOMovement.path = pqrLastStretch;

        d20aAoOMovement.destLoc = action.destLoc;
        d20aAoOMovement.distTraversed = pqrLastStretch.GetPathResultLength();
        if (!addingAoOStatus)
            d20aAoOMovement.d20ActType = d20ActionTypePostAoO;
        actSeq.d20ActArray.Add(d20aAoOMovement);
        ReleasePooledPathQueryResult(ref action.path);
    }

    [TempleDllLocation(0x1008bac0)]
    private void AddReadiedInterrupt(ActionSequence actSeq, ReadiedActionPacket readied)
    {
        var readiedAction = new D20Action();
        readiedAction.d20ATarget = actSeq.performer;
        readiedAction.d20Caf = default;
        readiedAction.d20ActType = D20ActionType.READIED_INTERRUPT;
        readiedAction.data1 = 1;
        readiedAction.path = null;
        readiedAction.d20APerformer = readied.interrupter;
        actSeq.d20ActArray.Add(readiedAction);
    }

    // The size of increments in which we search for interrupts along the path
    private const float InterruptSearchIncrement = 4.0f;

    [TempleDllLocation(0x100939d0)]
    private void ProcessPathForReadiedActions(D20Action action, out ReadiedActionPacket? triggeredReadiedAction)
    {
        Span<int> actionIndices = stackalloc int[_readiedActions.Count];
        FindHostileReadyVsMoveActions(action.d20APerformer, ref actionIndices);
        if (actionIndices.Length == 0)
        {
            triggeredReadiedAction = null;
            return;
        }

        var pathLength = action.path.GetPathResultLength();
        var startLocation = action.path.from;

        // Search along the path and look for changes of interrupts
        for (var dist = 0.0f; dist < pathLength + InterruptSearchIncrement / 2; dist += InterruptSearchIncrement)
        {
            var truncateLengthFeet = MathF.Min(dist, pathLength);
            GameSystems.PathX.TruncatePathToDistance(action.path, out var newLocation, truncateLengthFeet);

            // Process every approach/withdraw action along the path
            foreach (var readiedActionIdx in actionIndices)
            {
                var readiedAction = _readiedActions[readiedActionIdx];

                var wasInRange =
                    GameSystems.D20.Combat.CanMeleeTargetAtLocation(readiedAction.interrupter, action.d20APerformer,
                        startLocation);
                var willBeInRange =
                    GameSystems.D20.Combat.CanMeleeTargetAtLocation(readiedAction.interrupter, action.d20APerformer,
                        newLocation);

                if (readiedAction.readyType == ReadyVsTypeEnum.RV_Approach)
                {
                    // This essentially checks if the action performer moves from a spot that the interrupter
                    // can attack it into a spot that it cannot attack it.
                    if (wasInRange)
                    {
                        continue;
                    }

                    if (!willBeInRange && !GameSystems.D20.Combat.IsWithinThreeFeet(readiedAction.interrupter,
                            action.d20APerformer,
                            newLocation))
                    {
                        continue;
                    }
                }
                else if (readiedAction.readyType == ReadyVsTypeEnum.RV_Withdrawal)
                {
                    if (!wasInRange)
                    {
                        continue;
                    }

                    if (willBeInRange)
                    {
                        if (!GameSystems.D20.Combat.IsWithinThreeFeet(readiedAction.interrupter,
                                action.d20APerformer,
                                newLocation))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    continue;
                }

                triggeredReadiedAction = readiedAction;

                // Truncate the path so it ends where the interrupt happens
                GameSystems.PathX.GetPartialPath(action.path, out var trimmedPath, 0, dist);
                ReleasePooledPathQueryResult(ref action.path);
                action.path = trimmedPath;
                return;
            }
        }

        triggeredReadiedAction = null;
    }

    [TempleDllLocation(0x10091580)]
    private void FindHostileReadyVsMoveActions(GameObject mover, ref Span<int> actionIndices)
    {
        int count = 0;
        for (var index = 0; index < _readiedActions.Count; index++)
        {
            var readiedAction = _readiedActions[index];
            if (readiedAction.flags == 1 && (readiedAction.readyType == ReadyVsTypeEnum.RV_Approach ||
                                             readiedAction.readyType == ReadyVsTypeEnum.RV_Withdrawal))
            {
                // Ignore actions that would not actually trigger against the moving critter
                if (readiedAction.interrupter == mover ||
                    GameSystems.Critter.IsFriendly(readiedAction.interrupter, mover))
                {
                    continue;
                }

                actionIndices[count++] = index;
            }
        }

        actionIndices = actionIndices.Slice(0, count);
    }

    [TempleDllLocation(0x1008ba80)]
    private void UpdateDistTraversed(ActionSequence actSeq)
    {
        foreach (var d20Action in actSeq.d20ActArray)
        {
            var path = d20Action.path;
            if (path != null)
            {
                d20Action.distTraversed = path.GetPathResultLength();
            }
        }
    }

    [TempleDllLocation(0x10091580)]
    int ReadyVsApproachOrWithdrawalCount(GameObject mover)
    {
        int result = 0;
        for (int i = 0; i < _readiedActions.Count; i++)
        {
            if (_readiedActions[i].flags == 1
                && _readiedActions[i].interrupter != null
                && (_readiedActions[i].readyType == ReadyVsTypeEnum.RV_Approach
                    || _readiedActions[i].readyType == ReadyVsTypeEnum.RV_Withdrawal))
                result++;
        }

        return result;
    }

    [TempleDllLocation(0x10091500)]
    void ReadyVsRemoveForObj(GameObject obj)
    {
        for (int i = 0; i < _readiedActions.Count; i++)
        {
            if (_readiedActions[i].flags == 1 && _readiedActions[i].interrupter == obj)
            {
                _readiedActions[i].flags = 0;
                _readiedActions[i].interrupter = null;
            }
        }
    }

    [TempleDllLocation(0x10091650)]
    private ReadiedActionPacket? ReadiedActionGetNext(ReadiedActionPacket? prevReadiedAction, D20Action action)
    {
        int i0 = 0;

        // find the prevReadiedAction in the array (kinda retarded since it's a pointer???)
        if (prevReadiedAction != null)
        {
            for (i0 = 0; i0 < _readiedActions.Count; i0++)
            {
                if (_readiedActions[i0] == prevReadiedAction)
                {
                    i0++;
                    break;
                }
            }

            if (i0 >= _readiedActions.Count)
                return null;
        }

        for (int i = i0; i < _readiedActions.Count; i++)
        {
            if ((_readiedActions[i].flags & 1) == 0)
                continue;
            switch (_readiedActions[i].readyType)
            {
                case ReadyVsTypeEnum.RV_Spell:
                case ReadyVsTypeEnum.RV_Counterspell:
                    if (GameSystems.Critter.IsFriendly(action.d20APerformer, _readiedActions[i].interrupter))
                        continue;
                    if (action.d20ActType == D20ActionType.CAST_SPELL)
                        return _readiedActions[i];
                    break;
                case ReadyVsTypeEnum.RV_Approach:
                case ReadyVsTypeEnum.RV_Withdrawal:
                default:
                    if (action.d20ActType != D20ActionType.READIED_INTERRUPT)
                        continue;
                    if (action.d20APerformer != _readiedActions[i].interrupter)
                        continue;
                    return _readiedActions[i];
            }
        }

        return null;
    }

    [TempleDllLocation(0x1008B9A0)]
    private void TrimPathToRemainingMoveLength(D20Action action, float remainingMoveLength, PathQuery pathQ)
    {
        var pathLengthTrimmed = remainingMoveLength - 0.1f;

        GameSystems.PathX.GetPartialPath(action.path, out var pqrTrimmed, 0.0f, pathLengthTrimmed);

        ReleasePooledPathQueryResult(ref action.path);
        action.path = pqrTrimmed;
        var destination = pqrTrimmed.to;
        if (!GameSystems.PathX.PathDestIsClear(pathQ, action.d20APerformer, destination))
        {
            action.d20Caf |= D20CAF.ALTERNATE;
        }

        action.d20Caf |= D20CAF.TRUNCATED;
    }

    internal bool GetRemainingMaxMoveLength(D20Action action, TurnBasedStatus tbStat, out float moveLen)
    {
        var surplusMoves = tbStat.surplusMoveDistance;
        var moveSpeed = action.d20APerformer.Dispatch41GetMoveSpeed(out _);
        if (action.d20ActType == D20ActionType.UNSPECIFIED_MOVE)
        {
            if (tbStat.hourglassState >= HourglassState.FULL)
            {
                moveLen = 2 * moveSpeed + surplusMoves;
                return true;
            }

            if (tbStat.hourglassState >= HourglassState.MOVE)
            {
                moveLen = moveSpeed + surplusMoves;
                return true;
            }

            moveLen = surplusMoves;
            return true;
        }

        if (action.d20ActType == D20ActionType.FIVEFOOTSTEP
            && (tbStat.tbsFlags & (TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.Moved5FootStep)) == default)
        {
            if (surplusMoves <= 0.001f)
            {
                moveLen = 5.0f;
                return true;
            }

            moveLen = surplusMoves;
            return true;
        }

        if (action.d20ActType == D20ActionType.MOVE)
        {
            moveLen = moveSpeed + surplusMoves;
            return true;
        }

        if (action.d20ActType == D20ActionType.DOUBLE_MOVE)
        {
            moveLen = 2 * moveSpeed + surplusMoves;
            return true;
        }

        moveLen = 0;
        return false;
    }

    [TempleDllLocation(0x1008b850)]
    public void ReleasePooledPathQueryResult(ref PathQueryResult? result)
    {
        // Will not be implemented either, but we might use it as markers to pool the query in another way
        result = null;
    }

    [TempleDllLocation(0x1008b810)]
    public PathQueryResult GetPooledPathQueryResult()
    {
        throw new NotImplementedException(); // Will not be implemented either
    }

    [TempleDllLocation(0x10094ca0)]
    public ActionErrorCode seqCheckFuncs(out TurnBasedStatus tbStatus)
    {
        Debug.Assert(CurrentSequence != null);
        var curSeq = CurrentSequence;
        var result = ActionErrorCode.AEC_OK;

        tbStatus = curSeq.tbStatus.Copy();
        var seqPerfLoc = curSeq.performer.GetLocationFull();

        for (int i = 0; i < curSeq.d20ActArray.Count; i++)
        {
            var action = curSeq.d20ActArray[i];
            if (curSeq.d20ActArrayNum <= 0) return 0;

            var d20Def = D20ActionDefs.GetActionDef(action.d20ActType);
            var tgtCheckFunc = d20Def.tgtCheckFunc;
            if (tgtCheckFunc != null)
            {
                result = tgtCheckFunc(action, tbStatus);
                if (result != ActionErrorCode.AEC_OK)
                {
                    break;
                }
            }

            result = TurnBasedStatusUpdate(action, tbStatus);
            if (result != ActionErrorCode.AEC_OK)
            {
                tbStatus.errCode = result;
                break;
            }

            result = d20Def.actionCheckFunc(action, tbStatus);
            if (result != ActionErrorCode.AEC_OK)
            {
                break;
            }

            var locCheckFunc = d20Def.locCheckFunc;
            if (locCheckFunc != null)
            {
                result = locCheckFunc(action, tbStatus, seqPerfLoc);
                if (result != 0)
                {
                    break;
                }
            }

            var path = curSeq.d20ActArray[i].path;
            if (path != null)
            {
                seqPerfLoc = path.to;
            }
        }

        if (result != ActionErrorCode.AEC_OK)
        {
            tbStatus = CurrentSequence.tbStatus.Copy();
        }

        return result;
    }

    [TempleDllLocation(0x10093950)]
    internal ActionErrorCode TurnBasedStatusUpdate(D20Action action, TurnBasedStatus tbStatus)
    {
        var tbStatCopy = tbStatus.Copy();

        var actProcResult = ActionCostProcess(tbStatCopy, action);
        if (actProcResult == ActionErrorCode.AEC_OK)
        {
            tbStatCopy.CopyTo(tbStatus);
            return actProcResult;
        }

        var tbsCheckFunc = D20ActionDefs.GetActionDef(action.d20ActType).turnBasedStatusCheck;
        if (tbsCheckFunc != null)
        {
            if (tbsCheckFunc(action, tbStatCopy) != ActionErrorCode.AEC_OK)
            {
                return actProcResult;
            }

            actProcResult = ActionCostProcess(tbStatCopy, action);
        }

        if (actProcResult == ActionErrorCode.AEC_OK)
        {
            tbStatCopy.CopyTo(tbStatus);
            return actProcResult;
        }

        return actProcResult;
    }

    [TempleDllLocation(0x1008b040)]
    private ActionErrorCode ActionCostProcess(TurnBasedStatus tbStat, D20Action action)
    {
        var actCost = new ActionCostPacket();

        var result = D20ActionDefs.GetActionDef(action.d20ActType).actionCost(action, tbStat, actCost);
        if (result != ActionErrorCode.AEC_OK)
        {
            return result;
        }

        // Adding action cost modification facility
        actCost.hourglassCost = action.d20APerformer.DispatchActionCostMod(actCost, tbStat, action);

        var hourglassCost = actCost.hourglassCost;
        tbStat.hourglassState = GetHourglassTransition(tbStat.hourglassState, actCost.hourglassCost);

        if (tbStat.hourglassState == HourglassState.INVALID) // this is an error state I think
        {
            if (hourglassCost <= ActionCostType.FullRound)
            {
                switch (hourglassCost)
                {
                    case ActionCostType.Move:
                        tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
                        return ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
                    case ActionCostType.Standard:
                        tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                        return ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                    case ActionCostType.FullRound:
                        tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                        return ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                    case ActionCostType.PartialCharge:
                        tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                        break;
                }
            }
        }

        if (tbStat.surplusMoveDistance >= actCost.moveDistCost)
        {
            tbStat.surplusMoveDistance -= actCost.moveDistCost;
            if (actCost.chargeAfterPicker <= 0
                || actCost.chargeAfterPicker + tbStat.attackModeCode <=
                tbStat.baseAttackNumCode + tbStat.numBonusAttacks)
            {
                if (tbStat.numBonusAttacks < actCost.chargeAfterPicker)
                    tbStat.attackModeCode += actCost.chargeAfterPicker;
                else
                    tbStat.numBonusAttacks -= actCost.chargeAfterPicker;
                if (tbStat.attackModeCode == tbStat.baseAttackNumCode && tbStat.numBonusAttacks == 0)
                    tbStat.tbsFlags &= ~TurnBasedStatusFlags.FullAttack;
                result = ActionErrorCode.AEC_OK;
            }
            else
            {
                result = ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
            }
        }
        else
        {
            result = (tbStat.tbsFlags & TurnBasedStatusFlags.Moved5FootStep) != 0
                ? ActionErrorCode.AEC_ALREADY_MOVED
                : ActionErrorCode.AEC_TARGET_OUT_OF_RANGE;
        }

        return result;
    }

    [TempleDllLocation(0x10094c60)]
    public ActionErrorCode seqCheckAction(D20Action action, TurnBasedStatus tbStat)
    {
        var errorCode = TurnBasedStatusUpdate(action, tbStat);
        if (errorCode != ActionErrorCode.AEC_OK)
        {
            tbStat.errCode = errorCode;
            return errorCode;
        }

        var actionDef = D20ActionDefs.GetActionDef(action.d20ActType);
        return actionDef.actionCheckFunc(action, tbStat);
    }

    [TempleDllLocation(0x1008c580)]
    public void FullAttackCostCalculate(D20Action action, TurnBasedStatus tbStatus, out int baseAttackNumCode,
        out int bonusAttacks, out int numAttacks, out int attackModeCode)
    {
        GameObject performer = action.d20APerformer;
        int usingOffhand = 0;
        int _attackTypeCodeHigh = 1;
        int _attackTypeCodeLow = 0;
        int numAttacksBase = 0;
        var mainWeapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
        var offhand = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);

        if (offhand != null)
        {
            if (mainWeapon != null)
            {
                if (offhand.type != ObjectType.armor)
                {
                    _attackTypeCodeHigh = AttackPacket.ATTACK_CODE_OFFHAND + 1; // originally 5
                    _attackTypeCodeLow = AttackPacket.ATTACK_CODE_OFFHAND; // originally 4
                    usingOffhand = 1;
                }
            }
            else
            {
                mainWeapon = offhand;
            }
        }

        if (mainWeapon != null && GameSystems.Item.IsRangedWeapon(mainWeapon))
        {
            action.d20Caf |= D20CAF.RANGED;
        }

        // if unarmed check natural attacks (for monsters)
        if (mainWeapon == null && offhand == null)
        {
            numAttacksBase = DispatchD20ActionCheck(action, tbStatus, DispatcherType.GetCritterNaturalAttacksNum);

            if (numAttacksBase > 0)
            {
                _attackTypeCodeHigh = AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1; // originally 10
                _attackTypeCodeLow = AttackPacket.ATTACK_CODE_NATURAL_ATTACK; // originally 9
            }
        }

        if (numAttacksBase <= 0)
        {
            numAttacksBase = DispatchD20ActionCheck(action, tbStatus, DispatcherType.GetNumAttacksBase);
        }

        bonusAttacks = DispatchD20ActionCheck(action, tbStatus, DispatcherType.GetBonusAttacks);
        numAttacks = usingOffhand + numAttacksBase + bonusAttacks;
        attackModeCode = _attackTypeCodeLow;
        baseAttackNumCode = numAttacksBase + _attackTypeCodeHigh - 1 + usingOffhand;
    }

    public bool UsingSecondaryWeapon(D20Action action)
    {
        return GameSystems.D20.UsingSecondaryWeapon(action.d20APerformer, action.data1);
    }

    [TempleDllLocation(0x10092da0)]
    public void ResetAll(GameObject? critter)
    {
        // resets the readied action cache
        // Clear TBUiIntgameFocus
        // Set handle to initiative list first entry
        // Free all act seq array occupancies
        // Set Turn Based Actor to Initiative List first entry, and use their initiative
        // Clear the picker targeting type, action type and data
        // Clear pathfinding cache occupancies

        _readiedActions.Clear();
        GameUiBridge.ResetSelectionInput();

        if (critter != null && (GameSystems.Party.IsPlayerControlled(critter) ||
                                GameSystems.Critter.IsConcealed(critter)))
        {
            GameSystems.D20.Initiative.Move(critter, 0);
        }

        actSeqArray.Clear();
        CurrentSequence = null; // NOTE: Added on top of vanilla to avoid IsPerforming going out of sync

        GameSystems.D20.Initiative.RewindCurrentActor();
        SeqPickerTargetingTypeReset();
    }
    
    [TempleDllLocation(0x10099430)]
    public void TurnStart(GameObject obj)
    {
        Logger.Debug("*** NEXT TURN *** starting for {0}. CurSeq: {1}", obj, CurrentSequence);

        // NOTE: Previously wrote 0 to 0x1189FB60, but found no other uses for it
        _aiTurnActions = 0;
        performedDefaultAction = false;

        if (globD20Action.d20APerformer != null && globD20Action.d20APerformer != obj)
        {
            GameSystems.D20.D20SendSignal(globD20Action.d20APerformer, D20DispatcherKey.SIG_EndTurn);
        }

        if (!GameSystems.Combat.IsCombatActive())
            return;

        // check for interrupter sequence
        if (actSeqInterrupt != null)
        {
            // switch sequences
            var actSeq = actSeqInterrupt;
            CurrentSequence = actSeq;
            actSeqInterrupt = actSeq.interruptSeq;
            var curIdx = actSeq.d20aCurIdx;
            Logger.Info("Switching to Interrupt sequence, actor {0}",
                GameSystems.MapObject.GetDisplayName(actSeq.performer));
            if (curIdx < actSeq.d20ActArrayNum)
            {
                var action = actSeq.d20ActArray[curIdx];

                if (InterruptNonCounterspell(action))
                    return;

                if (action.d20ActType == D20ActionType.CAST_SPELL)
                {
                    var d20SpellData = action.d20SpellData;
                    if (GameSystems.D20.D20QueryWithObject(actSeq.performer, D20DispatcherKey.QUE_SpellInterrupted,
                            d20SpellData) != 0)
                    {
                        action.d20Caf &= ~D20CAF.NEED_ANIM_COMPLETED;
                        GameSystems.Anim.Interrupt(actSeq.performer, AnimGoalPriority.AGP_5);
                    }
                }
            }

            sequencePerform();
            return;
        }

        // clean readied actions for obj
        ReadyVsRemoveForObj(obj);

        // clean previously occupied sequences
        globD20ActnSetPerformer(obj);

        foreach (var actionSequence in actSeqArray)
        {
            if (actionSequence.IsPerforming && actionSequence.performer == obj)
            {
                // TODO: Remove from array
                actionSequence.IsPerforming = false;
                Logger.Info("Clearing outstanding sequence [{0}]", actionSequence);
            }
        }

        // allocate new sequence
        AllocSeq(obj);

        // apply newround condition and do BeginRound stuff
        if (!GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_NewRound_This_Turn))
        {
            GameSystems.Combat.DispatchBeginRound(obj, 1);
            obj.AddCondition("NewRound_This_Turn");
        }

        // dispatch TurnBasedStatusInit
        var tbStat = CurrentSequence.tbStatus;
        tbStat.hourglassState = HourglassState.FULL;
        tbStat.tbsFlags = 0;
        tbStat.idxSthg = -1;
        tbStat.surplusMoveDistance = 0;
        tbStat.attackModeCode = 0;
        tbStat.baseAttackNumCode = 0;
        tbStat.numBonusAttacks = 0;
        tbStat.numAttacks = 0;
        tbStat.errCode = ActionErrorCode.AEC_OK;

        DispIOTurnBasedStatus dispIo = new DispIOTurnBasedStatus();
        dispIo.tbStatus = tbStat;
        dispatchTurnBasedStatusInit(obj, dispIo);

        // Enqueue simuls
        SimulsEnqueue();

        if (GameSystems.Party.IsPlayerControlled(obj) && GameSystems.Critter.IsDeadOrUnconscious(obj))
        {
            Logger.Info("Action for {0} ending turn (unconscious)...",
                GameSystems.MapObject.GetDisplayName(globD20Action.d20APerformer));
            GameSystems.Combat.AdvanceTurn(obj);
            return;
        }

        if (obj.HasFlag(ObjectFlag.OFF))
        {
            Logger.Info("Action for {0} ending turn (ObjectFlag.OFF)",
                GameSystems.MapObject.GetDisplayName(globD20Action.d20APerformer));
            GameSystems.Combat.AdvanceTurn(obj);
            return;
        }

        if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Prone))
        {
            if (CurrentSequence.tbStatus.hourglassState >= HourglassState.MOVE)
            {
                GlobD20ActnInit();
                globD20Action.d20ActType = D20ActionType.STAND_UP;
                globD20Action.data1 = 0;
                ActionAddToSeq();
                sequencePerform();
            }
        }
    }

    private static bool HasCustomCombatScript(GameObject obj)
    {
        var combatStartScript = obj.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.StartCombat);
        return combatStartScript.scriptId != 0;
    }

    [TempleDllLocation(0x100922B0)]
    private void SimulsEnqueue()
    {
        if (!Globals.Config.ConcurrentTurnsEnabled)
        {
            return;
        }

        var obj = GameSystems.D20.Initiative.CurrentActor;

        if (_simultPerformerQueue.Contains(obj))
        {
            return;
        }

        ResetSimultaneousTurn();

        // If any actions are readied, do not allow simultaneous turns
        if (_readiedActions.Count > 0)
        {
            return;
        }

        if (actSeqInterrupt == null && !GameSystems.Party.IsInParty(obj) && !HasCustomCombatScript(obj))
        {
            Logger.Info("Building List for simultaneous turn: ");
            _simultPerformerQueue.Add(obj);

            var currentInitiativeIndex = GameSystems.D20.Initiative.CurrentActorIndex;

            // Iterate through the initiative list starting at the position of the current actor
            // and find anyone in order of their initiative who might go at the same time as the actor.
            var idx = (currentInitiativeIndex + 1) % GameSystems.D20.Initiative.Count;

            while (idx != currentInitiativeIndex)
            {
                var nextInInitiative = GameSystems.D20.Initiative[idx];
                if (GameSystems.Party.IsInParty(nextInInitiative) || HasCustomCombatScript(nextInInitiative))
                {
                    // Seek through the initiative until we find someone that cannot take their turn simultaneously
                    break;
                }

                // If the critter is hostile towards any of the currently queued simulatenous performers,
                // he cannot go at the same time.
                if (_simultPerformerQueue.Any(performer =>
                        !GameSystems.Critter.IsFriendly(performer, nextInInitiative)))
                {
                    break;
                }

                _simultPerformerQueue.Add(nextInInitiative);

                if (_simultPerformerQueue.Count >= MaxSimultPerformers)
                {
                    break;
                }

                // Wrap around to the start of the initiative list, if needed
                idx = (idx + 1) % GameSystems.D20.Initiative.Count;
            }

            foreach (var performer in _simultPerformerQueue)
            {
                Logger.Info("    {0} (Initiative: {1})", obj, GameSystems.D20.Initiative.GetInitiative(performer));
            }

            // Don't need to perform simultaneously with just one critter
            simulsIdx = 0;
            if (_simultPerformerQueue.Count == 1)
            {
                ResetSimultaneousTurn();
            }
        }
    }

    private void ResetSimultaneousTurn()
    {
        _simultPerformerQueue.Clear();
        _abortedSimultaneousSequence = null;
    }

    [TempleDllLocation(0x10099320)]
    private bool InterruptNonCounterspell(D20Action action)
    {
        var readiedAction = ReadiedActionGetNext(null, action);
        if (readiedAction == null)
        {
            return false;
        }

        while (true)
        {
            if (readiedAction.readyType != ReadyVsTypeEnum.RV_Counterspell)
            {
                var isInterruptibleAction = action.d20ActType == D20ActionType.CAST_SPELL;
                // added interruption to use of scrolls and such
                if (action.d20ActType == D20ActionType.USE_ITEM && action.d20SpellData.spellEnumOrg != 0)
                {
                    isInterruptibleAction = true;
                }

                if (isInterruptibleAction)
                {
                    if (action.d20APerformer != null && readiedAction.interrupter != null)
                    {
                        var isFriendly =
                            GameSystems.Critter.IsFriendly(readiedAction.interrupter, action.d20APerformer);
                        var sharedAlleg =
                            GameSystems.Critter.NpcAllegianceShared(readiedAction.interrupter,
                                action.d20APerformer);
                        if (!sharedAlleg && !isFriendly)
                            break;
                    }
                }

                if (action.d20ActType == D20ActionType.READIED_INTERRUPT)
                {
                    if (action.d20ATarget != null && readiedAction.interrupter != null)
                    {
                        var isFriendly =
                            GameSystems.Critter.IsFriendly(readiedAction.interrupter, action.d20ATarget);
                        var sharedAlleg =
                            GameSystems.Critter.NpcAllegianceShared(readiedAction.interrupter, action.d20ATarget);
                        if (!sharedAlleg && !isFriendly)
                            break;
                    }
                }
            }

            readiedAction = ReadiedActionGetNext(readiedAction, action);
            if (readiedAction == null)
            {
                return false;
            }
        }

        InterruptSwitchActionSequence(readiedAction);
        return true;
    }

    [TempleDllLocation(0x10098ac0)]
    void InterruptSwitchActionSequence(ReadiedActionPacket readiedAction)
    {
        if (readiedAction.readyType == ReadyVsTypeEnum.RV_Counterspell)
        {
            PerformCounterspell(readiedAction);
            return;
        }

        CurrentSequence.interruptSeq = actSeqInterrupt;
        actSeqInterrupt = CurrentSequence;
        GameSystems.D20.Combat.FloatCombatLine(CurrentSequence.performer, 158); // Action Interrupted
        Logger.Debug("{0} interrupted by {1}!", GameSystems.MapObject.GetDisplayName(CurrentSequence.performer),
            GameSystems.MapObject.GetDisplayName(readiedAction.interrupter));
        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(7, readiedAction.interrupter,
            actSeqInterrupt.performer);
        AssignSeq(readiedAction.interrupter);
        CurrentSequence.prevSeq = null;
        var curActor = GameSystems.D20.Initiative.CurrentActor;
        GameSystems.D20.Initiative.SetInitiativeTo(readiedAction.interrupter, curActor);
        GameSystems.D20.Initiative.CurrentActor = readiedAction.interrupter;
        CurrentSequence.performer = readiedAction.interrupter;
        globD20ActnSetPerformer(readiedAction.interrupter);
        CurrentSequence.tbStatus.hourglassState = HourglassState.PARTIAL;
        CurrentSequence.tbStatus.tbsFlags = 0;
        CurrentSequence.tbStatus.idxSthg = -1;
        CurrentSequence.tbStatus.surplusMoveDistance = 0;
        CurrentSequence.tbStatus.attackModeCode = 0;
        CurrentSequence.tbStatus.baseAttackNumCode = 0;
        CurrentSequence.tbStatus.numBonusAttacks = 0;
        CurrentSequence.tbStatus.numAttacks = 0;
        CurrentSequence.tbStatus.errCode = 0;
        CurrentSequence.IsPerforming = false;
        GlobD20ActnInit();
        readiedAction.flags = 0;
        GameSystems.Party.ClearSelection();
        if (GameSystems.Party.IsPlayerControlled(readiedAction.interrupter))
        {
            GameSystems.Party.AddToSelection(readiedAction.interrupter);
        }
        else
        {
            GameSystems.Combat.TurnProcessAi(readiedAction.interrupter);
        }

        if (GameSystems.D20.D20Query(readiedAction.interrupter, D20DispatcherKey.QUE_Prone))
        {
            if (CurrentSequence.tbStatus.hourglassState >= HourglassState.MOVE)
            {
                GlobD20ActnInit();
                globD20Action.d20ActType = D20ActionType.STAND_UP;
                globD20Action.data1 = 0;
                ActionAddToSeq();
                sequencePerform();
            }
        }

        GameUiBridge.UpdateInitiativeUi();
    }

    [TempleDllLocation(0x10091710)]
    private void PerformCounterspell(ReadiedActionPacket readiedAction)
    {
        var action = CurrentSequence.d20ActArray[CurrentSequence.d20aCurIdx];
        var spellEnum = action.d20SpellData.SpellEnum;
        var spellLvl = action.d20SpellData.spellSlotLevel;

        if (action.d20ActType == D20ActionType.CAST_SPELL && !action.d20Caf.HasFlag(D20CAF.COUNTERSPELLED))
        {
            var spellcraftDc = 11 + spellLvl;
            var dispelDc = 11 + spellLvl;
            var spellSchool = GameSystems.Spell.GetSpellSchoolEnum(spellEnum);
            if (!GameSystems.Skill.SkillRoll(readiedAction.interrupter, SkillId.spellcraft, spellcraftDc, out _,
                    GameSystems.Skill.GetSkillCheckFlagsForSchool(spellSchool)))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(6, action.d20APerformer,
                    action.d20ATarget);
                GameSystems.D20.Combat.FloatCombatLine(readiedAction.interrupter, 167);
                return;
            }

            var counterSpellEnum =
                GameSystems.Spell.FindCounterSpellId(readiedAction.interrupter, spellEnum, spellLvl);
            if (counterSpellEnum == 0)
            {
                // Nothing to counterspell with
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(5, action.d20APerformer,
                    action.d20ATarget);
                GameSystems.D20.Combat.FloatCombatLine(readiedAction.interrupter, 168);
                return;
            }

            if (counterSpellEnum == WellKnownSpells.DispelMagic)
            {
                if (!GameSystems.Spell.TryGetMemorizedSpell(readiedAction.interrupter, WellKnownSpells.DispelMagic,
                        out var memorizedSpell))
                {
                    return;
                }

                // Use a spell packet to calculate the effective caster level for the counter spell we
                // are about to use
                var spellPacket = new SpellPacketBody();
                spellPacket.caster = readiedAction.interrupter;
                spellPacket.spellClass = memorizedSpell.classCode;
                spellPacket.spellKnownSlotLevel = memorizedSpell.spellLevel;
                GameSystems.Spell.SpellPacketSetCasterLevel(spellPacket);

                // Then use that effective caster level to make a caster level check
                var roll = Dice.D20.Roll();
                var text = GameSystems.D20.Combat.GetCombatMesLine(169); // Counterspell

                var bonlist = BonusList.Default;
                bonlist.AddBonus(spellPacket.casterLevel, 0, 203);
                var rollHistId = GameSystems.RollHistory.AddMiscCheck(readiedAction.interrupter, dispelDc,
                    text, Dice.D20, roll, bonlist);
                GameSystems.RollHistory.CreateRollHistoryString(rollHistId);
                var casterLevelCheck = roll + bonlist.OverallBonus;

                GameSystems.Spell.DeleteMemorizedSpell(readiedAction.interrupter, WellKnownSpells.DispelMagic);

                if (casterLevelCheck < dispelDc)
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(5, action.d20APerformer,
                        action.d20ATarget);
                    GameSystems.D20.Combat.FloatCombatLine(readiedAction.interrupter, 168);
                }
                else
                {
                    action.d20Caf |= D20CAF.COUNTERSPELLED;
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(3, action.d20APerformer,
                        action.d20ATarget);
                    GameSystems.D20.Combat.FloatCombatLine(readiedAction.interrupter, 170);
                }
            }
            else
            {
                action.d20Caf |= D20CAF.COUNTERSPELLED;
                GameSystems.D20.Combat.FloatCombatLine(readiedAction.interrupter, 170);
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(3, action.d20APerformer,
                    action.d20ATarget);
            }

            GameSystems.Spell.DeleteMemorizedSpell(readiedAction.interrupter, counterSpellEnum);

            var goal = new AnimSlotGoalStackEntry(readiedAction.interrupter, AnimGoalType.throw_spell_w_cast_anim,
                true);
            goal.target.obj = readiedAction.interrupter;
            goal.animIdPrevious.number = GameSystems.Spell.GetSpellSchoolAnimId(SchoolOfMagic.Conjuration);
            goal.skillData.number = 0;
            GameSystems.Anim.PushGoal(goal, out _);

            readiedAction.flags = 0; // Marks it as done
        }
    }

    [TempleDllLocation(0x100920e0)]
    public GameObject? getNextSimulsPerformer()
    {
        if (simulsIdx + 1 < _simultPerformerQueue.Count)
        {
            return _simultPerformerQueue[simulsIdx + 1];
        }
        else
        {
            return null;
        }
    }

    [TempleDllLocation(0x10092110)]
    public bool IsSimulsCompleted()
    {
        var currentActor = GameSystems.D20.Initiative.CurrentActor;
        for (var index = 0; index < _simultPerformerQueue.Count; index++)
        {
            var performer = _simultPerformerQueue[index];
            if (currentActor == performer && index < _simultPerformerQueue.Count - 1)
            {
                var actorName = GameSystems.MapObject.GetDisplayName(performer);
                Logger.Info("Actor {0} not completed issuing instructions", actorName);
                return false;
            }

            foreach (var actionSequence in actSeqArray)
            {
                if (actionSequence.performer == performer && actionSequence.IsPerforming)
                {
                    var actorName = GameSystems.MapObject.GetDisplayName(performer);
                    Logger.Info("Actor {0} not completed", actorName);
                    return false;
                }
            }
        }

        return true;
    }

    [TempleDllLocation(0x100925b0)]
    public bool IsLastSimultPopped(GameObject obj)
    {
        return _abortedSimultaneousSequence.HasValue && _abortedSimultaneousSequence.Value.actor == obj;
    }

    [TempleDllLocation(0x10096fa0)]
    public bool IsLastSimulsPerformer(GameObject critter)
    {
        if (_simultPerformerQueue.Count > 0)
        {
            if (critter == _simultPerformerQueue[^1] && !IsSimulsCompleted())
            {
                return true;
            }

            if (restoreSeqTo(critter))
            {
                GameSystems.AI.AiProcess(critter);
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x10095ca0)]
    private bool restoreSeqTo(GameObject critter)
    {
        if (_abortedSimultaneousSequence.HasValue && _abortedSimultaneousSequence.Value.actor == critter)
        {
            Logger.Info("Restore Aborted: Reseting Sequence");
            CurSeqReset(critter);
            CurrentSequence.tbStatus = _abortedSimultaneousSequence.Value.status;
            ResetSimultaneousTurn();
            return true;
        }
        else
        {
            return false;
        }
    }

    [TempleDllLocation(0x100999e0)]
    public void GreybarReset()
    {
        if (!GameSystems.Combat.IsCombatActive())
            return;
        var actor = GameSystems.D20.Initiative.CurrentActor;
        if (!IsCurrentlyPerforming(actor) && IsSimulsCompleted() && !IsLastSimultPopped(actor))
        {
            Logger.Debug("GREYBAR DEHANGER for {0} ending turn...", actor);
            GameSystems.Combat.AdvanceTurn(actor);
            return;
        }

        Logger.Debug("Greybar reset function");

        // Defensive copy, since we'll likely modify this array below
        var sequences = new List<ActionSequence>(actSeqArray);

        foreach (var seq in sequences)
        {
            if (!seq.IsPerforming || !SequenceSwitch(seq.performer))
                continue;

            foreach (var action in seq.d20ActArray)
            {
                if ((action.d20Caf & D20CAF.NEED_ANIM_COMPLETED) != 0)
                {
                    // Cancel the corresponding animation in the animation system...
                    GameSystems.Anim.Interrupt(action.d20APerformer, AnimGoalPriority.AGP_HIGHEST);
                }

                action.d20Caf &= ~(D20CAF.NEED_ANIM_COMPLETED | D20CAF.NEED_PROJECTILE_HIT);
            }

            while (true)
            {
                ActionPerform();
                bool foundOtherSeq = false;
                foreach (var otherSequence in sequences)
                {
                    if (otherSequence.IsPerforming && otherSequence.performer == seq.performer)
                    {
                        foundOtherSeq = true;
                        break;
                    }
                }

                if (!foundOtherSeq)
                    break;

                var curSeq = CurrentSequence;
                if (curSeq != null && curSeq.IsPerforming)
                {
                    int curSeqActionIdx = curSeq.d20aCurIdx;
                    if (curSeqActionIdx >= 0 && curSeqActionIdx < curSeq.d20ActArrayNum)
                    {
                        var d20caf = curSeq.d20ActArray[curSeqActionIdx].d20Caf;
                        if ((d20caf & (D20CAF.NEED_PROJECTILE_HIT | D20CAF.NEED_ANIM_COMPLETED)) != default)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    [TempleDllLocation(0x10092720)]
    public bool SimulsAdvance()
    {
        simulsIdx = _simultPerformerQueue.Count - 1;
        var actor = GameSystems.D20.Initiative.CurrentActor;
        for (var i = 0; i < _simultPerformerQueue.Count; i++)
        {
            if (actor == _simultPerformerQueue[i])
            {
                simulsIdx = i;
                break;
            }
        }

        if (simulsIdx < _simultPerformerQueue.Count - 1)
        {
            Logger.Debug("Advancing to simul current {0}", ++simulsIdx);
            return true;
        }

        return false;
    }

    [TempleDllLocation(0x10097ef0)]
    public void RangedCounterAttack(GameObject attacker, GameObject defender, D20CAF flags)
    {
        AssignSeq(attacker);
        CurrentSequence.performer = attacker;
        if (attacker != globD20Action.d20APerformer)
        {
            SeqPickerTargetingTypeReset();
        }

        globD20Action.d20APerformer = attacker;

        CurrentSequence.tbStatus.Reset();
        CurrentSequence.tbStatus.hourglassState = HourglassState.STD;
        CurrentSequence.IsInterrupted = true;

        globD20Action.Reset(globD20Action.d20APerformer);
        globD20Action.d20ActType = D20ActionType.STANDARD_RANGED_ATTACK;
        globD20Action.data1 = 1;
        globD20Action.d20Caf |= flags | D20CAF.THROWN | D20CAF.ATTACK_OF_OPPORTUNITY | D20CAF.RANGED;

        GlobD20ActnSetTarget(defender, defender.GetLocationFull());
        ActionAddToSeq();
    }

    #region Cursor management

    public ActionCursor CurrentCursor => _currentCursor;

    [TempleDllLocation(0x10B3D5A8)] private ActionCursor _currentCursor;

    [TempleDllLocation(0x11869244)] [TempleDllLocation(0x11869294)] [TempleDllLocation(0x1186926c)]
    private readonly List<string> _currentSequenceTooltips = new();

    public void AddCurrentSequenceTooltip(string text)
    {
        _currentSequenceTooltips.Add(text);
    }

    #endregion

    private struct AttackOfOpportunityIndicator
    {
        public readonly LocAndOffsets Location;
        public readonly ITexture Texture;

        public AttackOfOpportunityIndicator(LocAndOffsets location, ITexture texture)
        {
            Texture = texture;
            Location = location;
        }
    }

    [TempleDllLocation(0x10b3d598)]
    private readonly List<AttackOfOpportunityIndicator> _aooIndicators = new();

    [TempleDllLocation(0x1008b660)]
    [TempleDllLocation(0x10b3b948)]
    public void AddAttackOfOpportunityIndicator(LocAndOffsets loc)
    {
        _aooIndicators.Add(new AttackOfOpportunityIndicator(loc, _aooIcon.Resource));
    }

    [TempleDllLocation(0x100914b0)]
    public void AddReadyAction(GameObject critter, ReadyVsTypeEnum readyVsType)
    {
        var readiedAction = new ReadiedActionPacket
        {
            flags = 1,
            interrupter = critter,
            readyType = readyVsType
        };
        _readiedActions.Add(readiedAction);
    }

    [TempleDllLocation(0x10097000)]
    public ActionErrorCode ActionSequenceChecksWithPerformerLocation()
    {
        var loc = CurrentSequence.performer.GetLocationFull();
        if (CurrentSequence.d20ActArray.Count > 0)
        {
            return ActionSequenceChecksRegardLoc(loc, CurrentSequence.tbStatus, 0, CurrentSequence);
        }

        return ActionErrorCode.AEC_NO_ACTIONS;
    }

    [TempleDllLocation(0x10089fa0)]
    public void ActSeqCurSetSpellPacket(SpellPacketBody spellPktBody, bool ignoreLos)
    {
        CurrentSequence.spellPktBody = spellPktBody;
        if (ignoreLos)
        {
            Logger.Info("Set CurSeq ignoreLos");
        }

        CurrentSequence.ignoreLos = ignoreLos;
    }

    /// <summary>
    /// Gets the pathing target location of the last added D20 action.
    /// </summary>
    [TempleDllLocation(0x1008a470)]
    public bool GetPathTargetLocFromCurD20Action(out LocAndOffsets loc)
    {
        int actNum = CurrentSequence.d20ActArrayNum;
        if (actNum <= 0)
        {
            loc = LocAndOffsets.Zero;
            return false;
        }

        var d20Path = CurrentSequence.d20ActArray[actNum - 1].path;
        if (d20Path == null)
        {
            loc = LocAndOffsets.Zero;
            return false;
        }

        loc = d20Path.to;
        return true;
    }

    [TempleDllLocation(0x10093180)]
    public void ActionSequenceRevertPath(int d20ActNum)
    {
        // Reset all paths for the given action and beyond
        for (var i = d20ActNum; i < CurrentSequence.d20ActArrayNum; i++)
        {
            ReleasePooledPathQueryResult(ref CurrentSequence.d20ActArray[i].path);
        }

        // Remove the given action and every action beyond it
        CurrentSequence.d20ActArray.RemoveRange(d20ActNum, CurrentSequence.d20ActArray.Count - d20ActNum);

        performingDefaultAction = false;

        // Set the performer loc to the latest valid path in the sequence, fall back to the performer's position
        CurrentSequence.performerLoc = CurrentSequence.performer.GetLocationFull();
        for (var i = 0; i < d20ActNum; i++)
        {
            var path = CurrentSequence.d20ActArray[i].path;
            if (path != null)
            {
                CurrentSequence.performerLoc = path.to;
            }
        }
    }

    [TempleDllLocation(0x100b8530)]
    public List<GameObject> GetEnemiesWithinReach(GameObject critter)
    {
        var result = new List<GameObject>();

        var reach = critter.GetReach();

        foreach (var combatant in GameSystems.D20.Initiative)
        {
            if (combatant != critter && !GameSystems.Combat.AffiliationSame(critter, combatant))
            {
                var dist = critter.DistanceToObjInFeet(combatant);
                if (dist < reach)
                {
                    result.Add(combatant);
                }
            }
        }

        return result;
    }

    [TempleDllLocation(0x11869240)] private float _movementFeet;

    [TempleDllLocation(0x11869298)] private ActionErrorCode _uiIntgameActionErrorCode;

    [TempleDllLocation(0x10097320)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:1044")]
    public void HourglassUpdate(IGameViewport viewport, bool intgameAcquireOn, bool intgameSelectionConfirmed,
        bool showPathPreview)
    {
        float greenMoveLength = 0.0f;
        float totalMoveLength = 0.0f;
        var actSeq = CurrentSequence;
        float moveSpeed = 0.0f;
        var actionType = globD20Action.d20ActType;

        if (actionType != D20ActionType.NONE && actionType >= D20ActionType.UNSPECIFIED_MOVE &&
            (globD20Action.GetActionDefinitionFlags() & D20ADF.D20ADF_DrawPathByDefault) != default)
        {
            showPathPreview = true;
            intgameAcquireOn = true;
            intgameSelectionConfirmed = true;
        }

        if (GameUiBridge.IsRadialMenuOpen())
        {
            CursorRenderUpdate();
            return;
        }

        if (!GameSystems.Combat.IsCombatActive())
            CursorRenderUpdate();

        var renderSequencePreview = false;
        var sequenceConfirmed = false;
        if (showPathPreview || intgameAcquireOn)
        {
            renderSequencePreview = true;
            if (intgameAcquireOn && intgameSelectionConfirmed)
            {
                sequenceConfirmed = true;
            }
        }

        _aooIndicators.Clear();
        _movementFeet = 0;
        _currentSequenceTooltips.Clear();
        _uiIntgameActionErrorCode = 0;

        if (actSeq == null)
        {
            return;
        }

        var actor = GameSystems.D20.Initiative.CurrentActor;

        var pathPreviewState = PathPreviewMode.None;
        if (IsCurrentlyPerforming(actor))
        {
            pathPreviewState = PathPreviewMode.IsMoving;
        }
        else if (intgameAcquireOn)
        {
            pathPreviewState = intgameSelectionConfirmed ? PathPreviewMode.One : PathPreviewMode.Two;
        }

        if (GameSystems.Combat.IsCombatActive())
        {
            var tbStat = actSeq.tbStatus;
            var hourglassState = tbStat.hourglassState;

            if (tbStat.surplusMoveDistance >= 0.01) // has surplus moves
            {
                // no move action remaining
                if (hourglassState == HourglassState.INVALID ||
                    GetHourglassTransition(hourglassState, ActionCostType.Move) == HourglassState.INVALID)
                {
                    greenMoveLength = -1.0f;
                    totalMoveLength = tbStat.surplusMoveDistance;
                }
                else // has a move function remaining, thus the surplus move distance must leftover from the "green"
                {
                    greenMoveLength = tbStat.surplusMoveDistance;
                    moveSpeed = actSeq.performer.Dispatch41GetMoveSpeed(out _) + greenMoveLength;
                    totalMoveLength = moveSpeed;
                }
            }
            else // no surplus move dist
            {
                // no move action remaining
                if (hourglassState == HourglassState.INVALID ||
                    GetHourglassTransition(hourglassState, ActionCostType.Move) == HourglassState.INVALID)
                {
                    totalMoveLength = -1.0f;
                    if ((tbStat.tbsFlags & (TurnBasedStatusFlags.UNK_1 | TurnBasedStatusFlags.Moved)) == 0)
                    {
                        greenMoveLength = 5.0f; // five foot step remaining
                    }
                    else
                    {
                        greenMoveLength = -1.0f;
                    }
                }
                else
                {
                    greenMoveLength = actSeq.performer.Dispatch41GetMoveSpeed(out _);
                    if ((tbStat.tbsFlags & (TurnBasedStatusFlags.UNK_1 | TurnBasedStatusFlags.Moved)) != 0)
                    {
                        greenMoveLength = -1.0f;
                    }
                    else if (GetHourglassTransition(tbStat.hourglassState, ActionCostType.FullRound) ==
                             HourglassState.INVALID)
                    {
                        totalMoveLength = greenMoveLength;
                        greenMoveLength = -1.0f;
                    }
                    else
                    {
                        moveSpeed = greenMoveLength + greenMoveLength;
                        totalMoveLength = moveSpeed;
                    }
                }
            }

            actionType = globD20Action.d20ActType;
            if (actionType != D20ActionType.NONE && actionType >= D20ActionType.UNSPECIFIED_MOVE &&
                (globD20Action.GetActionDefinitionFlags() & D20ADF.D20ADF_DrawPathByDefault) != 0)
            {
                var tbStat1 = new TurnBasedStatus();
                tbStat1.tbsFlags = tbStat.tbsFlags;
                tbStat1.surplusMoveDistance = tbStat.surplusMoveDistance;
                var d20ActionDef = D20ActionDefs.GetActionDef(actionType);
                if (actionType != D20ActionType.NONE && actionType >= D20ActionType.UNSPECIFIED_MOVE &&
                    d20ActionDef.turnBasedStatusCheck != null)
                {
                    if (d20ActionDef.turnBasedStatusCheck(globD20Action, tbStat1) != ActionErrorCode.AEC_OK)
                    {
                        // error in the check
                        totalMoveLength = 0.0f;
                        greenMoveLength = 0.0f;
                    }
                    else
                    {
                        totalMoveLength = tbStat1.surplusMoveDistance;
                        greenMoveLength = tbStat1.surplusMoveDistance;
                    }
                }

                // D20CAF.ALTERNATE indicates an invalid path (perhaps due to truncation)
                if (actSeq.d20ActArray.Count > 0
                    && (actSeq.d20ActArray[0].d20Caf & D20CAF.ALTERNATE) != D20CAF.NONE)
                {
                    totalMoveLength = 0.0f;
                    greenMoveLength = 0.0f;
                }
            }

            if ((actionType == D20ActionType.RUN || actionType == D20ActionType.CHARGE) && actSeq.d20ActArrayNum > 0)
            {
                for (int i = 0; i < actSeq.d20ActArrayNum; i++)
                {
                    var pathQueryResult = actSeq.d20ActArray[i].path;
                    if (actSeq.d20ActArray[i].d20ActType == D20ActionType.RUN && pathQueryResult != null &&
                        pathQueryResult.nodeCount != 1)
                    {
                        greenMoveLength = 0.0f;
                        totalMoveLength = 0.0f;
                    }
                }
            }
        }

        GameSystems.PathXRender.UiIntgameInitPathpreviewVars(pathPreviewState, greenMoveLength, totalMoveLength);

        if (GameSystems.Combat.IsCombatActive() && (!intgameAcquireOn || intgameSelectionConfirmed))
        {
            if (!IsCurrentlyPerforming(actor) && renderSequencePreview)
            {
                if (!GameSystems.Party.IsPlayerControlled(actor))
                    return;

                // Find the last move action
                D20Action lastActionWithPath = null;
                foreach (var action in actSeq.d20ActArray)
                {
                    if (action.path != null)
                    {
                        lastActionWithPath = action;
                    }
                }

                foreach (var action in actSeq.d20ActArray)
                {
                    SequenceRenderFlag previewFlags = 0;
                    if (sequenceConfirmed)
                    {
                        previewFlags |= SequenceRenderFlag.Confirmed;
                    }

                    if (action == lastActionWithPath)
                    {
                        previewFlags |= SequenceRenderFlag.FinalMovement;
                    }

                    if (action.d20ActType != D20ActionType.NONE)
                    {
                        var d20ActionDef = D20ActionDefs.GetActionDef(action.d20ActType);
                        d20ActionDef.seqRenderFunc?.Invoke(viewport, action, previewFlags);
                    }
                }
            }
        }
        else if (intgameAcquireOn && !intgameSelectionConfirmed &&
                 (actSeq == null || !GameSystems.Party.IsPlayerControlled(actor) || actSeq.targetObj != null))
        {
            return;
        }

        foreach (var aooIndicator in _aooIndicators)
        {
            _aooIndicatorRenderer.Render(
                viewport,
                aooIndicator.Location,
                aooIndicator.Texture
            );
        }

        CursorRenderUpdate();
    }

    [TempleDllLocation(0x1008A240)]
    private readonly CursorDrawCallback DrawAttacksRemainingDelegate = DrawAttacksRemaining;

    private static void DrawAttacksRemaining(int x, int y, object userarg)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10097060)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:952")]
    public void CursorRenderUpdate()
    {
        ActionCursor cursorState = 0;

        var intGameCursor = CursorHandleIntgameFocusObj();
        if (intGameCursor.HasValue)
        {
            cursorState = intGameCursor.Value;
        }

        var widgetEnteredGameplay = GameUiBridge.GetIntgameWidgetEnteredForGameplay();
        var widgetEnteredRender = GameUiBridge.GetIntgameWidgetEnteredForRender();
        var intgameTarget = GameUiBridge.GetIntgameTargetFromRaycast();

        var curSeq = CurrentSequence;

        if ((widgetEnteredGameplay || widgetEnteredRender)
            && curSeq != null
            && GameSystems.Party.IsPlayerControlled(curSeq.performer)
            && !IsCurrentlyPerforming(curSeq.performer)
            && curSeq.d20ActArray.Count > 0)
        {
            // Use the last action in the sequence
            var lastAction = curSeq.d20ActArray[^1];
            var getCursor = D20ActionDefs.GetActionDef(lastAction.d20ActType).getCursor;
            var actionCursor = getCursor(lastAction);
            if (actionCursor.HasValue)
            {
                cursorState = actionCursor.Value;
            }

            // TODO: Using the hourglass depletion here sucks ass and we should instead check the actual
            // distance traveled via the sequence, preferrably in the GetCursor function
            if (cursorState == ActionCursor.FeetGreen && GameSystems.PathXRender.HourglassDepletionState == 1)
            {
                cursorState = ActionCursor.FeetYellow;
            }
        }

        if ((widgetEnteredGameplay || widgetEnteredRender)
            && seqPickerTargetingType != D20TargetClassification.Invalid
            && GameSystems.Party.GetConsciousLeader() != null
            && GameSystems.Party.IsPlayerControlled(curSeq.performer))
        {
            var getCursor = D20ActionDefs.GetActionDef(seqPickerD20ActnType).getCursor;
            if (getCursor != D20ActionDefs.GetActionDef(D20ActionType.MOVE).getCursor)
            {
                var d20a = new D20Action();
                d20a.d20APerformer = GameSystems.Party.GetConsciousLeader();
                var actionCursor = getCursor(d20a);
                if (actionCursor.HasValue)
                {
                    cursorState = actionCursor.Value;
                }
            }
        }

        if (actSeqPickerActive && widgetEnteredGameplay)
        {
            var getCursor = D20ActionDefs.GetActionDef(actSeqPickerAction.d20ActType).getCursor;
            if (getCursor != D20ActionDefs.GetActionDef(D20ActionType.MOVE).getCursor)
            {
                var actionCursor = getCursor(actSeqPickerAction);
                if (actionCursor.HasValue)
                {
                    cursorState = actionCursor.Value;
                }
            }
        }

        if ((widgetEnteredGameplay || widgetEnteredRender)
            && curSeq != null
            && GameSystems.Party.IsPlayerControlled(curSeq.performer)
            && !IsCurrentlyPerforming(curSeq.performer))
        {
            var seqResultCheck = ActionSequenceChecksWithPerformerLocation();
            if (seqResultCheck != ActionErrorCode.AEC_OK && seqResultCheck != ActionErrorCode.AEC_NO_ACTIONS)
            {
                _uiIntgameActionErrorCode = seqResultCheck;
                _movementFeet = 0;
                _currentSequenceTooltips.Clear();
                if (cursorState < ActionCursor.ArrowInvalid)
                {
                    cursorState += 16; // Makes it an icon for an invalid action
                    if (cursorState >= ActionCursor.AttackOfOpportunity)
                        cursorState = ActionCursor.InvalidSelection4;
                }
            }
        }

        // stuff like dragging party/initiative portraits, wiki help etc.
        var specialCursor = GameUiBridge.GetCursor();
        if (specialCursor.HasValue)
        {
            cursorState = specialCursor.Value;
        }

        if (cursorState != _currentCursor)
        {
            Logger.Debug("Changing cursor from {0} to {1}", cursorState, _currentCursor);
            _currentCursor = cursorState;
        }

        if (Tig.Mouse.CursorDrawCallback == DrawAttacksRemainingDelegate)
        {
            Tig.Mouse.SetCursorDrawCallback(null);
        }

        if ((widgetEnteredGameplay || widgetEnteredRender || intgameTarget != null)
            && GameSystems.Combat.IsCombatActive()
            && !actSeqPickerActive
            && curSeq != null
            && GameSystems.Party.IsPlayerControlled(curSeq.performer)
            && !IsCurrentlyPerforming(curSeq.performer)
            && (seqPickerD20ActnType == D20ActionType.STANDARD_ATTACK
                || seqPickerD20ActnType == D20ActionType.TRIP)
            && (curSeq.tbStatus.attackModeCode <
                curSeq.tbStatus.baseAttackNumCode + curSeq.tbStatus.numBonusAttacks))
        {
            Tig.Mouse.SetCursorDrawCallback(DrawAttacksRemainingDelegate);
        }
        else if ((seqPickerD20ActnType == D20ActionType.UNSPECIFIED_ATTACK)
                 && intgameTarget != null
                 && cursorState != 0
                 && curSeq != null
                 && (curSeq.tbStatus.attackModeCode <
                     curSeq.tbStatus.baseAttackNumCode + curSeq.tbStatus.numBonusAttacks))
        {
            Tig.Mouse.SetCursorDrawCallback(DrawAttacksRemainingDelegate);
        }
    }

    [TempleDllLocation(0x10b3d5b0)]
    [TempleDllLocation(0x100936d0)]
    private ActionCursor? CursorHandleIntgameFocusObj()
    {
        var focus = GameUiBridge.GetUiFocus();
        var partyLeader = GameSystems.Party.GetConsciousLeader();
        if (focus == null || partyLeader == null)
        {
            return null;
        }

        switch (focus.type)
        {
            case ObjectType.portal when !focus.IsUndetectedSecretDoor():
            {
                if (focus.NeedsToBeUnlocked())
                {
                    var locked = GameSystems.AI.DryRunAttemptOpenContainer(partyLeader, focus) ==
                                 LockStatus.PLS_OPEN;
                    return locked ? ActionCursor.HaveKey : ActionCursor.Locked;
                }

                return ActionCursor.UseTeleportIcon;
            }
            case ObjectType.portal:
                return null;
            case ObjectType.container when focus.NeedsToBeUnlocked():
            {
                var locked = GameSystems.AI.DryRunAttemptOpenContainer(partyLeader, focus) == LockStatus.PLS_OPEN;
                return locked ? ActionCursor.HaveKey : ActionCursor.Locked;
            }
            case ObjectType.container:
                return ActionCursor.UseTeleportIcon;
            case ObjectType.scenery:
            {
                var teleportTo = focus.GetInt32(obj_f.scenery_teleport_to);
                if (teleportTo != 0 || focus.ProtoId == WellKnownProtos.GuestBook)
                {
                    return ActionCursor.UseTeleportIcon;
                }

                return null;
            }
            case ObjectType.npc:
            {
                if (GameSystems.Critter.IsFriendly(focus, partyLeader))
                {
                    if (!GameSystems.Party.IsInParty(focus))
                    {
                        return ActionCursor.Talk;
                    }
                }

                return null;
            }
            default:
                return null;
        }
    }

    [TempleDllLocation(0x10097310)]
    public void ResetCursor()
    {
        CursorRenderUpdate();
    }

    [TempleDllLocation(0x1008b6b0)]
    public bool GetTooltipTextFromIntgameUpdateOutput(out string tooltipText)
    {
        if (_currentCursor != 0)
        {
            if (_uiIntgameActionErrorCode != ActionErrorCode.AEC_OK)
            {
                tooltipText = ActionErrorString(_uiIntgameActionErrorCode);
                return tooltipText != null;
            }
            else
            {
                // TODO: Movement is actually never set
                tooltipText = "";
                if (_movementFeet > 1.0)
                {
                    var prefix = GameSystems.D20.Combat.GetCombatMesLine(131);
                    var movementFeetStr = (int) (_movementFeet + 0.95f);
                    var suffix = GameSystems.D20.Combat.GetCombatMesLine(132);
                    tooltipText = $"{prefix} {movementFeetStr} {suffix}";
                }

                foreach (var tooltipLine in _currentSequenceTooltips)
                {
                    tooltipText += tooltipLine;
                }

                if (tooltipText == "")
                {
                    tooltipText = null;
                }

                return tooltipText != null;
            }
        }
        else
        {
            tooltipText = null;
            return false;
        }
    }

    [TempleDllLocation(0x10094910)]
    [TemplePlusLocation("action_sequence.cpp:3770")]
    public HourglassState GetNewHourglassState(GameObject performer, D20ActionType d20ActionType, int d20Data1,
        int radMenuActualArg, D20SpellData d20SpellData)
    {
        if (CurrentSequence == null)
        {
            return HourglassState.INVALID;
        }

        var tbStatus = CurrentSequence.tbStatus.Copy();

        var fakeAction = new D20Action(d20ActionType);
        fakeAction.d20APerformer = performer;
        fakeAction.radialMenuActualArg = radMenuActualArg;
        fakeAction.data1 = d20Data1;
        fakeAction.d20Caf = D20CAF.NONE;
        fakeAction.d20ATarget = null;
        fakeAction.distTraversed = 0;
        fakeAction.spellId = 0;
        fakeAction.path = null;

        // TODO: Removed some python action specific logic here

        if (d20SpellData != null)
        {
            fakeAction.d20SpellData = d20SpellData;
        }

        if (TurnBasedStatusUpdate(fakeAction, tbStatus) != ActionErrorCode.AEC_OK)
        {
            return HourglassState.INVALID;
        }

        return tbStatus.hourglassState;
    }

    [TempleDllLocation(0x1008a1e0)]
    public string GetRemainingTimeDescription(HourglassState hourglassState)
    {
        return _translations[2001 + (int) hourglassState];
    }

    [TempleDllLocation(0x100921f0)]
    public bool IsCurrentlyActing(GameObject obj)
    {
        return GameSystems.D20.Initiative.CurrentActor == obj || _simultPerformerQueue.Contains(obj);
    }

    [TempleDllLocation(0x1008a210)]
    public bool ActorCanChangeInitiative(GameObject obj)
    {
        if (obj == GameSystems.D20.Initiative.CurrentActor)
        {
            return (CurrentSequence.tbStatus.tbsFlags & TurnBasedStatusFlags.HasActedThisRound) == 0;
        }
        else
        {
            return false;
        }
    }

    [TempleDllLocation(0x100996b0)]
    public void SwapInitiativeWith(GameObject obj, int targetIndex)
    {
        GameSystems.D20.Initiative.Move(obj, targetIndex);
        var actor = GameSystems.D20.Initiative.CurrentActor;
        TurnStart(actor);
        var actorIndex = GameSystems.D20.Initiative.CurrentActorIndex;
        GameSystems.Combat.TurnStart2(actorIndex);
    }

    [TempleDllLocation(0x1008b870)]
    public bool IsOkayToCleave(GameObject critter)
    {
        // Per the rules you cannot cleave (or use any other extra attacks for that matter) when using whirlwind attack
        return CurrentSequence == null
               || critter != CurrentSequence.performer
               || CurrentSequence.d20ActArray[0].d20ActType != D20ActionType.WHIRLWIND_ATTACK;
    }

    [TempleDllLocation(0x1008acc0)]
    public bool IsOffensive(D20ActionType actionType, GameObject target)
    {
        if (actionType == D20ActionType.LAY_ON_HANDS_USE && GameSystems.Critter.IsUndead(target))
        {
            return true;
        }

        var actionFlags = GetActionFlags(actionType);
        return (actionFlags & D20ADF.D20ADF_TriggersCombat) != 0;
    }

    [TempleDllLocation(0x10095d10)]
    public void Load(SavedD20State savedD20State)
    {
        // TODO: I am doubtful it's sensible to restore the global action
        globD20Action = ActionSequencesLoader.LoadAction(savedD20State.GlobalAction);
        var sequences = ActionSequencesLoader.LoadSequences(savedD20State.ActionSequences);
        if (savedD20State.CurrentSequenceIndex != -1)
        {
            CurrentSequence = sequences[savedD20State.CurrentSequenceIndex];
        }
        else
        {
            CurrentSequence = null;
        }

        LoadProjectiles(savedD20State.Projectiles);
        LoadReadiedActions(savedD20State.ReadiedActions);

        // Now filter out the null sequences which are an artifact of the index based references
        sequences.RemoveAll(x => x == null);
        actSeqArray = sequences!;
        
        // Ensure current sequence is populated if we're in combat and there's an actor
        // We're not loading action sequences that aren't performing so for us, current sequence would be null,
        // while the rest of the system depends on it being not-null
        if (CurrentSequence == null && GameSystems.Combat.IsCombatActive() && GameSystems.D20.Initiative.CurrentActor != null)
        {
            AllocSeq(GameSystems.D20.Initiative.CurrentActor);
        }
        
        actSeqPickerActive = false;
        actnProcState = 0;
        GameSystems.Combat.OnAfterActionsLoaded();
    }

    private void LoadProjectiles(ICollection<SavedProjectile> savedProjectiles)
    {
        _projectiles.Clear();
        _projectiles.Capacity = savedProjectiles.Count;

        foreach (var savedProjectile in savedProjectiles)
        {
            var projectile = GameSystems.Object.GetObject(savedProjectile.ProjectileId);
            if (projectile == null)
            {
                throw new CorruptSaveException($"Failed to restore projectile {savedProjectile.ProjectileId}");
            }

            var sequence = actSeqArray[savedProjectile.SequenceIndex];
            var action = sequence.d20ActArray[savedProjectile.ActionIndex];

            // TODO: Ammo item was not saved in Vanilla. As a workaround we might try to restore it heuristically by searching for it in the world
            _projectiles.Add(new ProjectileEntry
            {
                projectile = projectile,
                d20a = action
            });
        }
    }

    private void LoadReadiedActions(ICollection<SavedReadiedAction> savedReadiedActions)
    {
        _readiedActions.Clear();
        _readiedActions.Capacity = savedReadiedActions.Count;

        foreach (var savedReadiedAction in savedReadiedActions)
        {
            var interrupter = GameSystems.Object.GetObject(savedReadiedAction.Interrupter);
            if (interrupter == null)
            {
                throw new CorruptSaveException($"Failed to restore projectile {savedReadiedAction.Interrupter}");
            }

            _readiedActions.Add(new ReadiedActionPacket
            {
                interrupter = interrupter,
                readyType = savedReadiedAction.Type,
                flags = savedReadiedAction.IsActive ? 1 : 0
            });
        }
    }

    [TempleDllLocation(0x10092b00)]
    public void Save(SavedD20State savedD20State)
    {
        // TODO: I am doubtful it's sensible to restore the global action
        savedD20State.GlobalAction = ActionSequencesSaver.SaveAction(globD20Action);
        savedD20State.ActionSequences = ActionSequencesSaver.SaveSequences(actSeqArray);
        savedD20State.CurrentSequenceIndex = actSeqArray.IndexOf(CurrentSequence);

        savedD20State.Projectiles = SaveProjectiles();
        savedD20State.ReadiedActions = SaveReadiedActions();
    }

    private SavedProjectile[] SaveProjectiles()
    {
        return _projectiles.Select(SaveProjectile).ToArray();
    }

    private SavedProjectile SaveProjectile(ProjectileEntry projectile)
    {
        FindAction(projectile.d20a, out var sequenceIndex, out var actionIndex);

        return new SavedProjectile
        {
            // TODO: Ammo item was not saved in Vanilla. As a workaround we might try to restore it heuristically by searching for it in the world

            ProjectileId = projectile.projectile.id,
            SequenceIndex = sequenceIndex,
            ActionIndex = actionIndex
        };
    }

    private void FindAction(D20Action action, out int sequenceIndex, out int actionIndex)
    {
        for (var i = 0; i < actSeqArray.Count; i++)
        {
            var sequence = actSeqArray[i];
            for (var j = 0; j < sequence.d20ActArray.Count; j++)
            {
                if (sequence.d20ActArray[j] == action)
                {
                    sequenceIndex = i;
                    actionIndex = j;
                    return;
                }
            }
        }

        throw new ArgumentException("Unable to find action associated with projectile in current sequences!");
    }

    private List<SavedReadiedAction> SaveReadiedActions()
    {
        return _readiedActions.Select(SaveReadiedAction).ToList();
    }

    private static SavedReadiedAction SaveReadiedAction(ReadiedActionPacket readiedAction)
    {
        return new SavedReadiedAction
        {
            Interrupter = readiedAction.interrupter.id,
            Type = readiedAction.readyType,
            IsActive = (readiedAction.flags & 1) != 0
        };
    }

    [TempleDllLocation(0x10092a50)]
    public void ResetTurnBased()
    {
        rollbackSequenceFlag = false;
        globD20Action.d20APerformer = null;
        actSeqArray.Clear();
        actSeqTargets.Clear();
        CurrentSequence = null;
        actSeqSpellLoc = default;
        SeqPickerTargetingTypeReset();
    }
}