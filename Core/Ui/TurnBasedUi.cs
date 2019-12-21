using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui
{
    [Flags]
    enum UiIntgameTurnbasedFlags
    {
        ShowPathPreview = 0x1,
        IsLastSequenceActionWithPath = 0x2
    }

    public class TurnBasedUi : IResetAwareSystem, IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly WidgetContainer _container;

        [TempleDllLocation(0x10c040e8)]
        public GameObjectBody intgameTargetFromRaycast { get; private set; }

        [TempleDllLocation(0x102fc640)]
        public bool uiIntgameWidgetEnteredForRender { get; private set; }

        [TempleDllLocation(0x102fc644)]
        [TempleDllLocation(0x10173ce0)]
        public bool WidgetEnteredForGameplay { get; private set; }

        [TempleDllLocation(0x10174d70)]
        public TurnBasedUi()
        {
            _container = new WidgetContainer(Globals.UiManager.ScreenSize);
            _container.OnBeforeRender += Render;
            _container.OnHandleMessage += HandleMessage;
            Globals.UiManager.SendToBack(_container);
        }

        [TempleDllLocation(0x10c04114)]
        private bool uiIntgameWaypointMode;


        [TempleDllLocation(0x10c0410c)]
        private bool uiIntgameAcquireByRaycastOn;

        [TempleDllLocation(0x10c04110)]
        private bool uiIntgameSelectionConfirmed;

        [TempleDllLocation(0x10c040f0)]
        private int screenXfromMouseEvent;

        [TempleDllLocation(0x10c040e0)]
        private int screenYfromMouseEvent;

        [TempleDllLocation(0x10C040F8)]
        private LocAndOffsets uiIntgameWaypointLoc;

        [TempleDllLocation(0x10173f70)]
        [TemplePlusLocation("ui_intgame_turnbased.cpp:178")]
        private void Render()
        {
            var widEntered = uiIntgameWidgetEnteredForRender;
            if (!widEntered)
            {
                UiSystems.InGameSelect.Focus = null;
            }

            if (UiSystems.InGameSelect.IsPicking)
            {
                var flags = uiIntgameWaypointMode;
                if (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LMENU) || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RMENU))
                {
                    flags = true;
                }

                GameSystems.D20.Actions.HourglassUpdate(uiIntgameAcquireByRaycastOn, uiIntgameSelectionConfirmed,
                    flags);
                return;
            }

            if (widEntered)
            {
                var showPreview = uiIntgameWaypointMode;
                if (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LMENU) || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RMENU))
                {
                    showPreview = true;
                }

                GameSystems.D20.Actions.HourglassUpdate(uiIntgameAcquireByRaycastOn, uiIntgameSelectionConfirmed,
                    showPreview);

                if (GameSystems.Combat.IsCombatActive())
                {
                    // display hovered character tooltip
                    if (GameSystems.D20.Actions.GetTooltipTextFromIntgameUpdateOutput(out var tooltipText))
                    {
                        // trim last \n
                        tooltipText = tooltipText.Trim('\n');

                        int x = screenXfromMouseEvent;
                        int y = screenYfromMouseEvent;

                        var ttStyle = UiSystems.Tooltip.GetStyle(0);
                        Tig.Fonts.PushFont(ttStyle.Font);

                        TigTextStyle ttTextStyle = new TigTextStyle();
                        ttTextStyle.kerning = 2;
                        ttTextStyle.tracking = 2;
                        ttTextStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW | TigTextStyleFlag.TTSF_BACKGROUND |
                                            TigTextStyleFlag.TTSF_BORDER;
                        ttTextStyle.field2c = -1;
                        ttTextStyle.colors4 = null;
                        ttTextStyle.colors2 = null;
                        ttTextStyle.field0 = 0;
                        ttTextStyle.leading = 0;
                        ttTextStyle.textColor = new ColorRect(PackedLinearColorA.White);
                        ttTextStyle.shadowColor = new ColorRect(new PackedLinearColorA(0xFF000000));
                        ttTextStyle.bgColor = new ColorRect(new PackedLinearColorA(0x99111111));

                        Rectangle extents = Tig.Fonts.MeasureTextSize(tooltipText, ttTextStyle);
                        extents.X = x;
                        if (y > extents.Height)
                            extents.Y = y - extents.Height;
                        else
                            extents.Y = y;
                        Tig.Fonts.RenderText(tooltipText, extents, ttTextStyle);
                        Tig.Fonts.PopFont();
                    }

                    RenderThreatRanges();

                    // draw circle at waypoint / destination location
                    if (uiIntgameWaypointMode)
                    {
                        var actor = GameSystems.D20.Initiative.CurrentActor;
                        var actorRadius = actor.GetRadius();
                        var loc = uiIntgameWaypointLoc;
                        var fillColor = new PackedLinearColorA(0x80008000);
                        var borderColor = new PackedLinearColorA(0xFF00FF00);
                        GameSystems.PathXRender.DrawCircle3d(loc, 1.0f, fillColor, borderColor, actorRadius, false);
                    }
                }

                return;
            }

            // else
            GameSystems.D20.Actions.ResetCursor();
        }

        [TempleDllLocation(0x10C04118)]
        private GameObjectBody intgameActor;

        private int _panicKeys = 0;

        [TempleDllLocation(0x10174A30)]
        private bool HandleMessage(Message msg)
        {
            // TODO: DM System

            var initialSeq = GameSystems.D20.Actions.CurrentSequence;
            var result = false;

            if (msg.type == MessageType.MOUSE)
            {
                screenXfromMouseEvent = msg.MouseArgs.X;
                screenYfromMouseEvent = msg.MouseArgs.Y;
            }

            if (!GameSystems.Combat.IsCombatActive() || UiSystems.RadialMenu.IsOpen)
            {
                intgameActor = null;
                uiIntgameAcquireByRaycastOn = false;
                uiIntgameSelectionConfirmed = false;
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
                        UiIntgameGenerateSequence(false);
                    }
                }

                if (GameSystems.Party.IsPlayerControlled(intgameActor))
                {
                    var tigMsgType = msg.type;
                    if (tigMsgType == MessageType.MOUSE)
                    {
                        var mouseArgs = msg.MouseArgs;

                        if ((mouseArgs.flags & MouseEventFlag.LeftClick) != 0)
                        {
                            if (ToggleAcquisition(msg.MouseArgs))
                                result = true;
                        }

                        if ((mouseArgs.flags & MouseEventFlag.LeftReleased) != 0)
                        {
                            if (UiIntgamePathSequenceHandler(msg.MouseArgs))
                                result = true;
                        }

                        if ((mouseArgs.flags & MouseEventFlag.RightClick) != 0)
                        {
                            if (HandleRightMousePressed(mouseArgs))
                                result = true;
                        }

                        if ((mouseArgs.flags & MouseEventFlag.RightReleased) != 0)
                        {
                            if (ResetViaRmb(mouseArgs))
                                result = true;
                        }

                        if ((mouseArgs.flags & MouseEventFlag.PosChange) != 0)
                        {
                            IntgameValidateMouseSelection(mouseArgs);
                        }
                    }
                    else
                    {
                        // widget or keyboard msg
                        if (tigMsgType == MessageType.KEYSTATECHANGE && !msg.KeyStateChangeArgs.down)
                        {
                            var keyArgs = msg.KeyStateChangeArgs;
                            Logger.Debug("UiIntgameMsgHandler (KEYSTATECHANGE): msg key={0} down={1}", keyArgs.key,
                                keyArgs.down);
                            var leader = GameSystems.Party.GetConsciousLeader();
                            if (GameSystems.D20.Actions.IsCurrentlyPerforming(leader))
                            {
                                if (keyArgs.key == DIK.DIK_T)
                                {
                                    _panicKeys++;
                                }

                                if (_panicKeys >= 4)
                                {
                                    GameSystems.Anim.Interrupt(leader, AnimGoalPriority.AGP_HIGHEST, true);
                                    GameSystems.Anim.Interrupt(leader, AnimGoalPriority.AGP_1, true);
                                    GameSystems.D20.Actions.CurrentSequence.IsPerforming = false;
                                }

                                return true;
                            }
                            else
                            {
                                _panicKeys = 0;
                            }

                            // bind hotkey
                            if (GameSystems.D20.Hotkeys.IsNormalNonreservedHotkey(keyArgs.key)
                                && (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LCONTROL) ||
                                    Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RCONTROL)))
                            {
                                var leaderLoc = leader.GetLocationFull();

                                var pnt = leaderLoc.ToInches3D();
                                var screenPos = Tig.RenderingDevice.GetCamera().WorldToScreen(pnt);
                                UiSystems.RadialMenu.Spawn((int) screenPos.X, (int) screenPos.Y);
                                return UiSystems.RadialMenu.HandleMessage(msg);
                            }

                            GameSystems.D20.Actions.TurnBasedStatusInit(leader);
                            if (uiIntgameWaypointMode)
                            {
                                UiIntgameRestoreSeqBackup();
                            }
                            else
                            {
                                Logger.Info("Intgame: Resetting sequence.");
                                GameSystems.D20.Actions.CurSeqReset(leader);
                            }

                            GameSystems.D20.Actions.GlobD20ActnInit();
                            if (GameSystems.D20.Hotkeys.RadmenuHotkeySthg(leader, keyArgs.key))
                            {
                                GameSystems.D20.Actions.ActionAddToSeq();
                                GameSystems.D20.Actions.sequencePerform();

                                var comrade = GameSystems.Dialog.GetListeningPartyMember(leader);
                                if (GameSystems.Dialog.TryGetOkayVoiceLine(actor, comrade, out var text, out var soundId))
                                {
                                    GameSystems.Dialog.PlayCritterVoiceLine(actor, comrade, text, soundId);
                                }

                                result = true;
                            }
                        }
                        else if (tigMsgType == MessageType.WIDGET)
                        {
                            var widgetArgs = msg.WidgetArgs;
                            if (widgetArgs.widgetEventType == TigMsgWidgetEvent.Exited)
                            {
                                uiIntgameAcquireByRaycastOn = false;
                                uiIntgameSelectionConfirmed = false;
                                intgameTargetFromRaycast = null;
                                WidgetEnteredForGameplay = false;
                            }
                            else if (widgetArgs.widgetEventType == TigMsgWidgetEvent.Entered)
                            {
                                WidgetEnteredForGameplay = true;
                            }
                        }
                    }
                }
            }

            if (msg.type == MessageType.WIDGET)
            {
                var widgetArgs = msg.WidgetArgs;
                if (widgetArgs.widgetEventType == TigMsgWidgetEvent.Exited)
                {
                    uiIntgameWidgetEnteredForRender = false;
                    return result;
                }

                if (widgetArgs.widgetEventType == TigMsgWidgetEvent.Entered)
                {
                    uiIntgameWidgetEnteredForRender = true;
                }
            }

            if (GameSystems.D20.Actions.CurrentSequence != initialSeq)
            {
                Logger.Info("Sequence switch from Ui Intgame Msg Handler to {0}",
                    GameSystems.D20.Actions.CurrentSequence);
            }

            return result;
        }

        [TempleDllLocation(0x10174930)]
        private bool ResetViaRmb(MessageMouseArgs mouseArgs)
        {
            if (UiSystems.InGameSelect.IsPicking)
            {
                return false;
            }

            if (uiIntgameWaypointMode)
            {
                uiIntgameWaypointMode = false;
                IntgameValidateMouseSelection(mouseArgs);
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
        private void IntgameValidateMouseSelection(MessageMouseArgs mouseArgs) {
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
                UiIntgameGenerateSequence(true);
                return;
            }

            float distSqr = 0;
            if (!UiIntgameRaycast(mouseArgs.X, mouseArgs.Y, GameRaycastFlags.HITTEST_3D, out var objFromRaycast)) {
                var mouseTile = Tig.RenderingDevice.GetCamera().ScreenToTile(mouseArgs.X, mouseArgs.Y);
                var prevPntNode = locFromScreenLoc.ToInches2D();
                var pntNode = mouseTile.ToInches2D();
                objFromRaycast = null;
                distSqr = (prevPntNode - pntNode).LengthSquared();
            }

            if (uiIntgameSelectionConfirmed) {
                if (objFromRaycast != intgameTargetFromRaycast
                    || intgameTargetFromRaycast == null && distSqr > actorRadiusSqr)
                {
                    uiIntgameSelectionConfirmed = false;
                    return;
                }
            } else if (objFromRaycast == intgameTargetFromRaycast
                       && (intgameTargetFromRaycast != null || distSqr < actorRadiusSqr))
            {
                uiIntgameSelectionConfirmed = true;
                return;
            }
        }

        [TempleDllLocation(0x10173f30)]
        private bool UiIntgameRaycast(int screenX, int screenY, GameRaycastFlags flags, out GameObjectBody obj)
        {
            if ( uiIntgameTargetObjFromPortraits != null )
            {
                obj = uiIntgameTargetObjFromPortraits;
                return true;
            }

            return GameSystems.Raycast.PickObjectOnScreen(screenX, screenY, out obj, flags);
        }

        [TempleDllLocation(0x10174790)]
        [TemplePlusLocation("ui_intgame_turnbased.cpp:180")]
        private bool UiIntgamePathSequenceHandler(MessageMouseArgs mouseArgs)
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
                    var altIsPressed = Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LMENU) ||
                                       Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RMENU);

                    if ((altIsPressed || uiIntgameWaypointMode) && intgameTargetFromRaycast == null)
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
            IntgameValidateMouseSelection(mouseArgs);
            return true;
        }

        [TempleDllLocation(0x1008a020)]
        [TemplePlusLocation("ui_intgame_turnbased.cpp:290")]
        private void UiIntgameBackupCurSeq()
        {
            uiIntgameCurSeqBackup = GameSystems.D20.Actions.CurrentSequence.Copy();
        }

        [TempleDllLocation(0x10c04120)]
        private GameObjectBody uiIntgameTargetObjFromPortraits;

        [TempleDllLocation(0x10174750)]
        private bool ToggleAcquisition(MessageMouseArgs mouseArgs)
        {
            if (UiSystems.InGameSelect.IsPicking)
            {
                return false;
            }

            if (uiIntgameAcquireByRaycastOn)
            {
                if (mouseArgs.flags == (MouseEventFlag.PosChange | MouseEventFlag.LeftDown))
                {
                    IntgameValidateMouseSelection(mouseArgs);
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
        private void UiIntgameGenerateSequence(bool isUnnecessary)
        {
            var curSeq = GameSystems.D20.Actions.CurrentSequence;
            // replacing this just for debug purposes really

            var actor = GameSystems.D20.Initiative.CurrentActor;

            if (GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
            {
                return;
            }

            CurSeqBackup();

            var altIsPressed = Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LMENU) ||
                               Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RMENU);

            var isWaypointMode = uiIntgameWaypointMode;

            var tgtFromPortraits = uiIntgameTargetObjFromPortraits;

            var x = screenXfromMouseEvent;
            var y = screenYfromMouseEvent;

            var actionLoc = locFromScreenLoc; //

            if (isWaypointMode || altIsPressed)
            {
                if (tgtFromPortraits != null)
                {
                    intgameTargetFromRaycast = tgtFromPortraits;
                }
                else
                {
                    var raycastFlags = GameRaycastFlags.ExcludeUnconscious | GameRaycastFlags.ExcludePortals |
                                       GameRaycastFlags.ExcludeItems | GameRaycastFlags.HITTEST_SEL_CIRCLE;

                    if (!GameSystems.Raycast.PickObjectOnScreen(x, y, out var pickedObject, raycastFlags))
                    {
                        intgameTargetFromRaycast = null;
                        locFromScreenLoc = Tig.RenderingDevice.GetCamera().ScreenToTile(x, y);
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

                if (!GameSystems.Raycast.PickObjectOnScreen(x, y, out var pickedObject, raycastFlags))
                {
                    intgameTargetFromRaycast = null;
                    locFromScreenLoc = Tig.RenderingDevice.GetCamera().ScreenToTile(x, y);
                    actionLoc = locFromScreenLoc;
                }
                else
                {
                    intgameTargetFromRaycast = pickedObject;
                }
            }


            var mouseLoc = Tig.RenderingDevice.GetCamera().ScreenToTile(x, y);

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
                int dummy = 1;
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
        private bool HandleRightMousePressed(MessageMouseArgs mouseArgs)
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
                    UiSystems.InGame.radialmenu_ignore_close_till_move(mouseArgs.X, mouseArgs.Y);
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
        public void RenderThreatRanges()
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
                            GameSystems.PathXRender.DrawCircle3d(center, 1.0f, fillColor, borderColor, circleRadius,
                                false);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10173b30)]
        [TemplePlusLocation("ui_intgame_turnbased.cpp:186")]
        private bool ShouldRenderThreatRange(GameObjectBody obj)
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

            var showPreview = Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LMENU)
                              || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RMENU)
                              || uiIntgameWaypointMode;
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
                    var mouseArgs = new MessageMouseArgs(0, 0, 0, MouseEventFlag.LeftReleased);
                    UiSystems.TurnBased.ToggleAcquisition(mouseArgs);
                    UiSystems.TurnBased.UiIntgamePathSequenceHandler(mouseArgs);
                }
            }
        }

        [TempleDllLocation(0x10174970)]
        public void TargetFromPortrait(GameObjectBody obj)
        {
            if ( GameSystems.D20.Actions.SeqPickerHasTargetingType() )
            {
                UiSystems.TurnBased.uiIntgameTargetObjFromPortraits = obj;
                if ( obj != null )
                {
                    UiSystems.InGameSelect.Focus = obj;
                    var msg = new MessageMouseArgs(0, 0, 0, MouseEventFlag.LeftReleased);
                    UiSystems.TurnBased.IntgameValidateMouseSelection(msg);
                }
            }
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        [TempleDllLocation(0x10173ac0)]
        public void Reset()
        {
            uiIntgameTargetObjFromPortraits = null;
        }
    }
}