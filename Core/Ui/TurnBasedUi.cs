using System;
using System.Collections;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using SDL2;

namespace OpenTemple.Core.Ui;

[Flags]
enum UiIntgameTurnbasedFlags
{
    ShowPathPreview = 0x1,
    IsLastSequenceActionWithPath = 0x2
}

public class TurnBasedUi : IResetAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10c040e8)]
    public GameObject? intgameTargetFromRaycast { get; private set; }

    [TempleDllLocation(0x102fc640)]
    public bool uiIntgameWidgetEnteredForRender { get; private set; }

    [TempleDllLocation(0x102fc644)]
    [TempleDllLocation(0x10173ce0)]
    public bool WidgetEnteredForGameplay { get; private set; }

    [TempleDllLocation(0x10174d70)]
    public TurnBasedUi()
    {
    }

    [TempleDllLocation(0x10c04114)]
    private bool uiIntgameWaypointMode;

    [TempleDllLocation(0x10c0410c)]
    private bool uiIntgameAcquireByRaycastOn;

    [TempleDllLocation(0x10c04110)]
    private bool uiIntgameSelectionConfirmed;

    [TempleDllLocation(0x10c040f0)]
    private float screenXfromMouseEvent;

    [TempleDllLocation(0x10c040e0)]
    private float screenYfromMouseEvent;

    [TempleDllLocation(0x10C040F8)]
    private LocAndOffsets uiIntgameWaypointLoc;

    private WidgetTooltipRenderer _tooltipRenderer = new();

    [TempleDllLocation(0x10173f70)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:178")]
    public void Render(IGameViewport viewport)
    {
        var widEntered = uiIntgameWidgetEnteredForRender;
        if (!widEntered)
        {
            UiSystems.InGameSelect.Focus = null;
        }

        if (UiSystems.InGameSelect.IsPicking)
        {
            var flags = uiIntgameWaypointMode;
            if (Tig.Keyboard.IsAltPressed)
            {
                flags = true;
            }

            GameSystems.D20.Actions.HourglassUpdate(viewport, uiIntgameAcquireByRaycastOn, uiIntgameSelectionConfirmed,
                flags);
            return;
        }

        if (widEntered)
        {
            var showPreview = uiIntgameWaypointMode;
            if (Tig.Keyboard.IsAltPressed)
            {
                showPreview = true;
            }

            GameSystems.D20.Actions.HourglassUpdate(viewport, uiIntgameAcquireByRaycastOn, uiIntgameSelectionConfirmed,
                showPreview);

            if (GameSystems.Combat.IsCombatActive())
            {
                // display hovered character tooltip
                if (GameSystems.D20.Actions.GetTooltipTextFromIntgameUpdateOutput(out var tooltipText))
                {
                    // trim last \n
                    tooltipText = tooltipText.Trim('\n');

                    var x = screenXfromMouseEvent;
                    var y = screenYfromMouseEvent;

                    _tooltipRenderer.TooltipText = tooltipText;
                    _tooltipRenderer.Render((int) x, (int) y);
                }

                RenderThreatRanges(viewport); // TODO: This shit needs to be moved into a scene-render-only method querying this state

                // draw circle at waypoint / destination location
                if (uiIntgameWaypointMode)
                {
                    var actor = GameSystems.D20.Initiative.CurrentActor;
                    var actorRadius = actor.GetRadius();
                    var loc = uiIntgameWaypointLoc;
                    var fillColor = new PackedLinearColorA(0x80008000);
                    var borderColor = new PackedLinearColorA(0xFF00FF00);
                    GameSystems.PathXRender.DrawCircle3d(viewport, loc, 1.0f, fillColor, borderColor, actorRadius, false);
                }
            }

            return;
        }

        // else
        GameSystems.D20.Actions.ResetCursor();
    }

    [TempleDllLocation(0x10C04118)]
    private GameObject? intgameActor;

    private int _panicKeys;

    private bool ShouldHandleEvent(IGameViewport viewport)
    {
        if (!GameSystems.Combat.IsCombatActive() || UiSystems.RadialMenu.IsOpen)
        {
            intgameActor = null;
            uiIntgameAcquireByRaycastOn = false;
            uiIntgameSelectionConfirmed = false;
            return false;
        }
        else
        {
            var actor = GameSystems.D20.Initiative.CurrentActor;
            if (actor != intgameActor)
            {
                intgameActor = actor;
                uiIntgameWaypointMode = false;
                uiIntgameAcquireByRaycastOn = false;
                uiIntgameSelectionConfirmed = false;
                if (GameSystems.Party.IsPlayerControlled(actor))
                {
                    UiIntgameGenerateSequence(viewport, false);
                }
            }

            return GameSystems.Party.IsPlayerControlled(intgameActor);
        }
    }

    [TempleDllLocation(0x10174A30)]
    private void HandleMouseMove(IGameViewport viewport, MouseEvent e)
    {
        screenXfromMouseEvent = e.X;
        screenYfromMouseEvent = e.Y;

        if (ShouldHandleEvent(viewport))
        {
            IntgameValidateMouseSelection(viewport, e.X, e.Y);
        }
    }

    [TempleDllLocation(0x10174A30)]
    private void HandleMouseDown(IGameViewport viewport, MouseEvent e)
    {
        screenXfromMouseEvent = e.X;
        screenYfromMouseEvent = e.Y;

        if (ShouldHandleEvent(viewport))
        {
            var initialSeq = GameSystems.D20.Actions.CurrentSequence;

            if (e.Button == MouseButton.Left)
            {
                if (ToggleAcquisition(viewport, e))
                    e.StopImmediatePropagation();
            }
            else if (e.Button == MouseButton.Right)
            {
                if (HandleRightMousePressed(viewport, e))
                    e.StopImmediatePropagation();
            }

            if (GameSystems.D20.Actions.CurrentSequence != initialSeq)
            {
                Logger.Info("Turn-Based UI switched sequence to {0} via mousedown-handler",
                    GameSystems.D20.Actions.CurrentSequence);
            }
        }
    }

    [TempleDllLocation(0x10174A30)]
    private void HandleMouseUp(IGameViewport viewport, MouseEvent e)
    {
        screenXfromMouseEvent = e.X;
        screenYfromMouseEvent = e.Y;

        if (ShouldHandleEvent(viewport))
        {
            var initialSeq = GameSystems.D20.Actions.CurrentSequence;

            if (e.Button == MouseButton.Left)
            {
                if (UiIntgamePathSequenceHandler(viewport, e.X, e.Y))
                    e.StopImmediatePropagation();
            }
            else if (e.Button == MouseButton.Right)
            {
                if (ResetViaRmb(viewport, e))
                    e.StopImmediatePropagation();
            }

            if (GameSystems.D20.Actions.CurrentSequence != initialSeq)
            {
                Logger.Info("Turn-Based UI switched sequence to {0} via mouseup-handler",
                    GameSystems.D20.Actions.CurrentSequence);
            }
        }
    }

    private void HandleMouseEntered(IGameViewport viewport, MouseEvent e)
    {
        if (ShouldHandleEvent(viewport))
        {
            WidgetEnteredForGameplay = true;
        }

        uiIntgameWidgetEnteredForRender = true;
    }

    private void HandleMouseLeave(IGameViewport viewport, MouseEvent e)
    {
        if (ShouldHandleEvent(viewport))
        {
            uiIntgameAcquireByRaycastOn = false;
            uiIntgameSelectionConfirmed = false;
            intgameTargetFromRaycast = null;
            WidgetEnteredForGameplay = false;
        }

        uiIntgameWidgetEnteredForRender = false;
    }

    [TempleDllLocation(0x10174930)]
    private bool ResetViaRmb(IGameViewport viewport, MouseEvent e)
    {
        if (UiSystems.InGameSelect.IsPicking)
        {
            return false;
        }

        if (uiIntgameWaypointMode)
        {
            uiIntgameWaypointMode = false;
            IntgameValidateMouseSelection(viewport, e.X, e.Y);
            return true;
        }

        if (GameSystems.D20.Actions.seqPickerTargetingType == D20TargetClassification.Invalid)
        {
            return false;
        }

        GameSystems.D20.Actions.SeqPickerTargetingTypeReset();
        return true;
    }

    [TempleDllLocation(0x101745e0)]
    private void IntgameValidateMouseSelection(IGameViewport viewport, float x, float y)
    {
        if (UiSystems.InGameSelect.IsPicking || UiSystems.RadialMenu.IsOpen)
        {
            return;
        }

        var actor = GameSystems.D20.Initiative.CurrentActor;
        var actorRadiusSqr = actor.GetRadius();
        actorRadiusSqr *= actorRadiusSqr;

        GameSystems.TimeEvent.RemoveAll(TimeEventType.IntgameTurnbased);
        if (!uiIntgameAcquireByRaycastOn)
        {
            UiIntgameGenerateSequence(viewport, true);
            return;
        }

        float distSqr = 0;
        if (!UiIntgameRaycast(viewport, x, y, GameRaycastFlags.HITTEST_3D, out var objFromRaycast))
        {
            // TODO: This should be moved to the event handlers of an actual game view widget
            var mouseTile = viewport.ScreenToTile(x, y);
            var prevPntNode = locFromScreenLoc.ToInches2D();
            var pntNode = mouseTile.ToInches2D();
            objFromRaycast = null;
            distSqr = (prevPntNode - pntNode).LengthSquared();
        }

        if (uiIntgameSelectionConfirmed)
        {
            if (objFromRaycast != intgameTargetFromRaycast
                || intgameTargetFromRaycast == null && distSqr > actorRadiusSqr)
            {
                uiIntgameSelectionConfirmed = false;
                return;
            }
        }
        else if (objFromRaycast == intgameTargetFromRaycast
                 && (intgameTargetFromRaycast != null || distSqr < actorRadiusSqr))
        {
            uiIntgameSelectionConfirmed = true;
            return;
        }
    }

    [TempleDllLocation(0x10173f30)]
    private bool UiIntgameRaycast(IGameViewport viewport, float screenX, float screenY, GameRaycastFlags flags, out GameObject? obj)
    {
        if (uiIntgameTargetObjFromPortraits != null)
        {
            obj = uiIntgameTargetObjFromPortraits;
            return true;
        }

        return GameSystems.Raycast.PickObjectOnScreen(viewport, screenX, screenY, out obj, flags);
    }

    [TempleDllLocation(0x10174790)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:180")]
    private bool UiIntgamePathSequenceHandler(IGameViewport viewport, float x, float y)
    {
        if (UiSystems.InGameSelect.IsPicking)
        {
            return false;
        }

        bool performSeq = true;
        var actor = GameSystems.D20.Initiative.CurrentActor;

        if (uiIntgameAcquireByRaycastOn)
        {
            if (uiIntgameSelectionConfirmed &&
                !GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
            {
                if ((Tig.Keyboard.IsAltPressed || uiIntgameWaypointMode) && intgameTargetFromRaycast == null)
                {
                    GameSystems.D20.Actions.GetPathTargetLocFromCurD20Action(out var curd20aTgtLoc);
                    if (curd20aTgtLoc.DistanceTo(locFromScreenLoc) >= 24.0f)
                    {
                        performSeq = false;
                    }
                    else if (!uiIntgameWaypointMode
                             || locFromScreenLoc.location != uiIntgameWaypointLoc.location)
                    {
                        // this initiates waypoint mode
                        uiIntgameWaypointMode = true;
                        uiIntgameWaypointLoc = locFromScreenLoc;
                        UiIntgameBackupCurSeq();
                        performSeq = false;
                    }
                }
            }
            else
            {
                performSeq = false;
            }
        }

        if (performSeq)
        {
            UiSystems.Combat.ActionBar.StartMovement();
            Logger.Info("UiIntgame: \t Issuing Sequence for current actor {0} ({1}), cur seq: {2}",
                GameSystems.MapObject.GetDisplayName(actor), actor, GameSystems.D20.Actions.CurrentSequence);
            GameSystems.D20.Actions.sequencePerform();
            uiIntgameTargetObjFromPortraits = null;
            uiIntgameWaypointMode = false;
            GameSystems.D20.Actions.SeqPickerTargetingTypeReset();

            var comrade = GameSystems.Dialog.GetListeningPartyMember(actor);
            if (GameSystems.Dialog.TryGetOkayVoiceLine(actor, comrade, out var text, out var soundId))
            {
                GameSystems.Dialog.PlayCritterVoiceLine(actor, comrade, text, soundId);
            }
        }

        uiIntgameAcquireByRaycastOn = false;
        intgameTargetFromRaycast = null;
        IntgameValidateMouseSelection(viewport, x, y);
        return true;
    }

    [TempleDllLocation(0x1008a020)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:290")]
    private void UiIntgameBackupCurSeq()
    {
        uiIntgameCurSeqBackup = GameSystems.D20.Actions.CurrentSequence.Copy();
    }

    [TempleDllLocation(0x10c04120)]
    private GameObject? uiIntgameTargetObjFromPortraits;

    [TempleDllLocation(0x10174750)]
    private bool ToggleAcquisition(IGameViewport viewport, MouseEvent? e)
    {
        if (UiSystems.InGameSelect.IsPicking)
        {
            return false;
        }

        if (uiIntgameAcquireByRaycastOn)
        {
            if (e is {IsLeftButtonHeld: true})
            {
                IntgameValidateMouseSelection(viewport, e.X, e.Y);
            }

            return true;
        }

        uiIntgameAcquireByRaycastOn = true;
        uiIntgameSelectionConfirmed = true;
        return true;
    }

    [TempleDllLocation(0x10C040D0)]
    private LocAndOffsets locFromScreenLoc;

    [TempleDllLocation(0x10174100)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:179")]
    private void UiIntgameGenerateSequence(IGameViewport viewport, bool isUnnecessary)
    {
        var curSeq = GameSystems.D20.Actions.CurrentSequence;
        // replacing this just for debug purposes really

        var actor = GameSystems.D20.Initiative.CurrentActor;

        if (GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
        {
            return;
        }

        CurSeqBackup();

        var isWaypointMode = uiIntgameWaypointMode;

        var tgtFromPortraits = uiIntgameTargetObjFromPortraits;

        var x = screenXfromMouseEvent;
        var y = screenYfromMouseEvent;

        var actionLoc = locFromScreenLoc; //

        if (isWaypointMode || Tig.Keyboard.IsAltPressed)
        {
            if (tgtFromPortraits != null)
            {
                intgameTargetFromRaycast = tgtFromPortraits;
            }
            else
            {
                var raycastFlags = GameRaycastFlags.ExcludeUnconscious | GameRaycastFlags.ExcludePortals |
                                   GameRaycastFlags.ExcludeItems | GameRaycastFlags.HITTEST_SEL_CIRCLE;

                if (!GameSystems.Raycast.PickObjectOnScreen(viewport, x, y, out var pickedObject, raycastFlags))
                {
                    intgameTargetFromRaycast = null;
                    locFromScreenLoc = viewport.ScreenToTile(x, y);
                    actionLoc = locFromScreenLoc;

                    if (IsWithinRadiusOfWaypointLoc(locFromScreenLoc))
                    {
                        UiIntgameRestoreSeqBackup();
                        locFromScreenLoc = uiIntgameWaypointLoc;
                        return;
                    }
                }
                else
                {
                    intgameTargetFromRaycast = pickedObject;
                }
            }
        }

        else if (tgtFromPortraits != null)
        {
            intgameTargetFromRaycast = tgtFromPortraits;
        }

        else
        {
            var raycastFlags = GameRaycastFlags.HITTEST_3D | GameRaycastFlags.ExcludePortals;

            if (!GameSystems.Raycast.PickObjectOnScreen(viewport, x, y, out var pickedObject, raycastFlags))
            {
                intgameTargetFromRaycast = null;
                locFromScreenLoc = viewport.ScreenToTile(x, y);
                actionLoc = locFromScreenLoc;
            }
            else
            {
                intgameTargetFromRaycast = pickedObject;
            }
        }

        var mouseLoc = viewport.ScreenToTile(x, y);

        var canGenerate = intgameTargetFromRaycast != null;
        if (!canGenerate)
        {
            if (!GameSystems.Tile.IsSubtileBlocked(mouseLoc, true))
            {
                var fogFlags = GameSystems.MapFogging.GetFogStatus(mouseLoc);
                if ((fogFlags & 4) != 0)
                {
                    // explored
                    canGenerate = true;
                }
            }
        }

        if (!canGenerate)
        {
            actor = GameSystems.D20.Initiative.CurrentActor;
            if (GameSystems.Party.IsPlayerControlled(actor))
            {
                if (isWaypointMode)
                {
                    UiIntgameRestoreSeqBackup();
                }
                else
                {
                    GameSystems.D20.Actions.CurSeqReset(actor);
                }

                GameSystems.D20.Actions.TurnBasedStatusInit(actor);
                GameSystems.D20.Actions.GlobD20ActnInit();
                intgameTargetFromRaycast = null;
            }

            return;
        }


        if (!GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
        {
            if (intgameTargetFromRaycast != null)
            {
                if (!GameSystems.Critter.IsCombatModeActive(actor))
                {
                    GameSystems.Combat.EnterCombat(actor);
                }


                switch (intgameTargetFromRaycast.type)
                {
                    case ObjectType.container:
                        if (isWaypointMode)
                            UiIntgameRestoreSeqBackup();
                        else
                            GameSystems.D20.Actions.CurSeqReset(actor);
                        if (isUnnecessary)
                            GameSystems.D20.Actions.GlobD20ActnSetD20CAF(D20CAF.UNNECESSARY);
                        GameSystems.D20.Actions.TurnBasedStatusInit(actor);
                        GameSystems.D20.Actions.GlobD20ActnInit();
                        GameSystems.D20.Actions.ActionTypeAutomatedSelection(intgameTargetFromRaycast);
                        if (GameSystems.D20.Actions.GlobD20ActnSetTarget(intgameTargetFromRaycast, null) ==
                            ActionErrorCode.AEC_OK)
                        {
                            GameSystems.D20.Actions.ActionAddToSeq();
                        }

                        break;
                    case ObjectType.weapon:
                    case ObjectType.ammo:
                    case ObjectType.armor:
                    case ObjectType.money:
                    case ObjectType.food:
                    case ObjectType.scroll:
                    case ObjectType.key:
                    case ObjectType.written:
                    case ObjectType.generic:
                        if (GameSystems.D20.Actions.SeqPickerHasTargetingType())
                            return;
                        if (isWaypointMode)
                        {
                            UiIntgameRestoreSeqBackup();
                        }
                        else
                        {
                            Logger.Debug("Generate Sequence: Reseting sequence");
                            GameSystems.D20.Actions.CurSeqReset(actor);
                        }

                        if (isUnnecessary)
                            GameSystems.D20.Actions.GlobD20ActnSetD20CAF(D20CAF.UNNECESSARY);
                        GameSystems.D20.Actions.TurnBasedStatusInit(actor);
                        GameSystems.D20.Actions.GlobD20ActnInit();
                        GameSystems.D20.Actions.ActionTypeAutomatedSelection(intgameTargetFromRaycast);
                        if (GameSystems.D20.Actions.GlobD20ActnSetTarget(intgameTargetFromRaycast, null) ==
                            ActionErrorCode.AEC_OK)
                        {
                            GameSystems.D20.Actions.ActionAddToSeq();
                        }

                        break;
                    case ObjectType.pc:
                    case ObjectType.npc:
                        if (isWaypointMode)
                        {
                            UiIntgameRestoreSeqBackup();
                        }
                        else
                        {
                            GameSystems.D20.Actions.CurSeqReset(actor);
                        }

                        if (isUnnecessary)
                            GameSystems.D20.Actions.GlobD20ActnSetD20CAF(D20CAF.UNNECESSARY);
                        GameSystems.D20.Actions.TurnBasedStatusInit(actor);
                        GameSystems.D20.Actions.GlobD20ActnInit();
                        GameSystems.D20.Actions.ActionTypeAutomatedSelection(intgameTargetFromRaycast);
                        if (GameSystems.D20.Actions.GlobD20ActnSetTarget(intgameTargetFromRaycast, null) ==
                            ActionErrorCode.AEC_OK)
                        {
                            GameSystems.D20.Actions.ActionAddToSeq();
                        }

                        break;
                    default:
                        break;
                }
            }
            else if (!GameSystems.Critter.IsDeadOrUnconscious(actor))
            {
                if (isWaypointMode)
                {
                    UiIntgameRestoreSeqBackup();
                }
                else
                {
                    GameSystems.D20.Actions.CurSeqReset(actor);
                }

                GameSystems.D20.Actions.TurnBasedStatusInit(actor);
                GameSystems.D20.Actions.GlobD20ActnInit();
                if (isUnnecessary)
                    GameSystems.D20.Actions.GlobD20ActnSetD20CAF(D20CAF.UNNECESSARY);

                GameSystems.D20.Actions.ActionTypeAutomatedSelection(null);
                if (GameSystems.D20.Actions.GlobD20ActnSetTarget(null, actionLoc) == ActionErrorCode.AEC_OK)
                {
                    GameSystems.D20.Actions.ActionAddToSeq();
                }
            }
        }

        if (GameSystems.D20.Actions.rollbackSequenceFlag)
        {
            actSeqCurBackup_GenerateSequence.CopyTo(GameSystems.D20.Actions.CurrentSequence);
        }

        // orgUiIntgameGenerateSequence(isUnnecessary);
        if (GameSystems.D20.Actions.CurrentSequence != curSeq)
        {
            Logger.Info("Sequence switch from Generate Sequence to {0}", GameSystems.D20.Actions.CurrentSequence);
        }
    }

    [TempleDllLocation(0x10173cf0)]
    private bool IsWithinRadiusOfWaypointLoc(LocAndOffsets loc)
    {
        if (uiIntgameWaypointMode)
        {
            var actor = GameSystems.D20.Initiative.CurrentActor;
            var actorRadius = actor.GetRadius();
            var waypointPos = uiIntgameWaypointLoc.ToInches2D();
            var locPos = loc.ToInches2D();
            if ((waypointPos - locPos).LengthSquared() <= actorRadius * actorRadius)
            {
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x10B3BF50)]
    private ActionSequence actSeqCurBackup_GenerateSequence;

    [TempleDllLocation(0x10092790)]
    private bool CurSeqBackup()
    {
        if (GameSystems.D20.Actions.CurrentSequence != null)
        {
            actSeqCurBackup_GenerateSequence = GameSystems.D20.Actions.CurrentSequence.Copy();
            return true;
        }
        else
        {
            return false;
        }
    }

    [TempleDllLocation(0x10173d70)]
    private bool HandleRightMousePressed(IGameViewport viewport, MouseEvent e)
    {
        if (UiSystems.InGameSelect.IsPicking)
        {
            return false;
        }

        if (!uiIntgameWaypointMode)
        {
            var partyLeader = GameSystems.Party.GetConsciousLeader();
            if (!GameSystems.Combat.IsCombatActive() || !GameSystems.D20.Actions.IsCurrentlyPerforming(partyLeader))
            {
                if (GameSystems.D20.Actions.SeqPickerHasTargetingType())
                {
                    GameSystems.D20.Actions.SeqPickerTargetingTypeReset();
                    return true;
                }

                if (uiIntgameWaypointMode)
                {
                    UiIntgameRestoreSeqBackup();
                }
                else
                {
                    GameSystems.D20.Actions.CurSeqReset(partyLeader);
                }

                Logger.Info("intgame_turnbased: _mouse_right_down");
                UiSystems.RadialMenu.SpawnFromMouse(viewport, e);
            }
        }

        return true;
    }

    [TempleDllLocation(0x118692a0)]
    private ActionSequence uiIntgameCurSeqBackup;

    [TempleDllLocation(0x10093110)]
    private void UiIntgameRestoreSeqBackup()
    {
        // Free paths in the current sequence
        foreach (var action in GameSystems.D20.Actions.CurrentSequence.d20ActArray)
        {
            GameSystems.D20.Actions.ReleasePooledPathQueryResult(ref action.path);
        }

        uiIntgameCurSeqBackup.CopyTo(GameSystems.D20.Actions.CurrentSequence);
        GameSystems.D20.Actions.performingDefaultAction = false;
    }

    [TempleDllLocation(0x10173c00)]
    public void RenderThreatRanges(IGameViewport viewport)
    {
        var currentActor = GameSystems.D20.Initiative.CurrentActor;

        if (!GameSystems.D20.Actions.IsCurrentlyPerforming(currentActor))
        {
            if (GameSystems.Party.IsPlayerControlled(currentActor))
            {
                foreach (var combatant in GameSystems.D20.Initiative)
                {
                    if (ShouldRenderThreatRange(combatant))
                    {
                        var reach = combatant.GetReach() * locXY.INCH_PER_FEET;
                        var radius = combatant.GetRadius();
                        var center = combatant.GetLocationFull();
                        var circleRadius = radius + reach;

                        var fillColor = new PackedLinearColorA(0x40808000);
                        var borderColor = new PackedLinearColorA(0xFF808000);
                        GameSystems.PathXRender.DrawCircle3d(viewport, center, 1.0f, fillColor, borderColor, circleRadius,
                            false);
                    }
                }
            }
        }
    }

    [TempleDllLocation(0x10173b30)]
    [TemplePlusLocation("ui_intgame_turnbased.cpp:186")]
    private bool ShouldRenderThreatRange(GameObject obj)
    {
        var isFocus = obj == UiSystems.InGameSelect.Focus;
        if (isFocus)
        {
            if (GameSystems.D20.D20QueryWithObject(obj, D20DispatcherKey.QUE_AOOPossible, obj) == 0)
            {
                return false;
            }

            return GameSystems.D20.Combat.CanMeleeTargetAtLocation(obj, obj, obj.GetLocationFull());
        }

        if (GameSystems.Party.IsPlayerControlled(obj))
        {
            return false;
        }

        var showPreview = Tig.Keyboard.IsAltPressed || uiIntgameWaypointMode;
        if (!showPreview && (!uiIntgameAcquireByRaycastOn || !uiIntgameSelectionConfirmed))
        {
            return false;
        }

        if (GameSystems.Critter.IsConcealed(obj)
            || GameSystems.D20.D20QueryWithObject(obj, D20DispatcherKey.QUE_AOOPossible, obj) == 0)
        {
            return false;
        }

        return GameSystems.D20.Combat.CanMeleeTargetAtLocation(obj, obj, obj.GetLocationFull());
    }

    [TempleDllLocation(0x101749D0)]
    public void sub_101749D0()
    {
        if (uiIntgameTargetObjFromPortraits != null)
        {
            var actor = GameSystems.D20.Initiative.CurrentActor;
            if (GameSystems.Party.IsPlayerControlled(actor))
            {
                ToggleAcquisition(GameViews.Primary, null);
                UiIntgamePathSequenceHandler(GameViews.Primary, 0, 0);
            }
        }
    }

    [TempleDllLocation(0x10174970)]
    public void TargetFromPortrait(GameObject obj)
    {
        if (GameSystems.D20.Actions.SeqPickerHasTargetingType())
        {
            uiIntgameTargetObjFromPortraits = obj;
            if (obj != null)
            {
                UiSystems.InGameSelect.Focus = obj;
                IntgameValidateMouseSelection(GameViews.Primary, 0, 0);
            }
        }
    }

    [TempleDllLocation(0x10173ac0)]
    public void Reset()
    {
        uiIntgameTargetObjFromPortraits = null;
    }

    public void AddEventListeners<T>(T viewport) where T : WidgetBase, IGameViewport
    {
        viewport.OnMouseDown += e => HandleMouseDown(viewport, e);
        viewport.OnMouseUp += e => HandleMouseUp(viewport, e);
        viewport.OnMouseEnter += e => HandleMouseEntered(viewport, e);
        viewport.OnMouseLeave += e => HandleMouseLeave(viewport, e);
        viewport.OnMouseMove += e => HandleMouseMove(viewport, e);
    }

    private void PanicCancelAction()
    {
        if (++_panicKeys >= 4)
        {
            _panicKeys = 0;
            var actor = GameSystems.D20.Initiative.CurrentActor;
            GameSystems.Anim.Interrupt(actor, AnimGoalPriority.AGP_HIGHEST, true);
            GameSystems.Anim.Interrupt(actor, AnimGoalPriority.AGP_1, true);
            GameSystems.D20.Actions.CurrentSequence.IsPerforming = false;
        }
    }

    public IEnumerable<HotkeyAction> EnumerateHotkeyActions()
    {
        // Hotkeys for turn-based combat are only active during turn-based combat,
        // while it's a party members turn.
        bool Condition()
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                return false;
            }

            var actor = GameSystems.D20.Initiative.CurrentActor;
            return GameSystems.Party.IsPlayerControlled(actor);
        }

        yield return new HotkeyAction(InGameHotKey.PanicCancelAction, PanicCancelAction, () =>
        {
            // Panic button is available to cancel out of an action a party member is performing
            var leader = GameSystems.Party.GetConsciousLeader();
            return Condition() && GameSystems.D20.Actions.IsCurrentlyPerforming(leader);
        });

        // Add all user-assignable hotkeys
        foreach (var userHotkey in InGameHotKey.UserAssignableHotkeys)
        {
            yield return new HotkeyAction(
                userHotkey,
                e => TriggerUserHotkey(userHotkey, e),
                Condition
            );
        }
    }

    private void TriggerUserHotkey(Hotkey hotkey, KeyboardEvent e)
    {
        // assign hotkey
        var leader = GameSystems.Party.GetConsciousLeader();
        var viewport = GameViews.Primary;
        if (leader == null || viewport == null)
        {
            return;
        }

        if (e.IsCtrlHeld)
        {
            UiSystems.RadialMenu.SpawnAndStartHotkeyAssignment(viewport, leader, hotkey);
        }
        else if (GameSystems.D20.Hotkeys.IsAssigned(hotkey))
        {
            GameSystems.D20.Actions.TurnBasedStatusInit(leader);
            if (uiIntgameWaypointMode)
            {
                UiIntgameRestoreSeqBackup();
            }
            else
            {
                GameSystems.D20.Actions.CurSeqReset(leader);
            }

            GameSystems.D20.Hotkeys.AddHotkeyActionToSequence(hotkey);
        }
    }
}
