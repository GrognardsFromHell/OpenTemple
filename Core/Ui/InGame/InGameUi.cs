using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.CharSheet;

namespace SpicyTemple.Core.Ui.InGame
{
    public class InGameUi : AbstractUi, IDisposable, ISaveGameAwareUi
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x10BD3B50)]
        private bool partyMembersMoving;

        [TempleDllLocation(0x10BD3B54)]
        private bool isCombatModeMessage;

        [TempleDllLocation(0x10112e70)]
        public InGameUi()
        {
            _translations = Tig.FS.ReadMesFile("mes/intgame.mes");
        }

        [TempleDllLocation(0x10112eb0)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10112f10)]
        public void ResetInput()
        {
            uiDragSelectOn = false;
            _normalLmbClicked = false;
            Globals.UiManager.IsMouseInputEnabled = true;

            UiSystems.InGameSelect.FocusClear();
        }

        public bool SaveGame()
        {
            return true;
        }

        [TempleDllLocation(0x10113280)]
        private void radialmenu_ignore_close_till_move(int x, int y)
        {
            UiSystems.RadialMenu.Spawn(x, y);
            UiSystems.RadialMenu.HandleRightMouseClick(x, y);
            Logger.Info("intgame_radialmenu_ignore_close_till_move()");
            UiSystems.RadialMenu.dword_10BE6D70 = true;
        }

        [TempleDllLocation(0x101140c0)]
        public bool LoadGame()
        {
            UiSystems.InGameSelect.FocusClear();
            return true;
        }

        [TempleDllLocation(0x10112ec0)]
        public override void ResizeViewport(Size size)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101140b0)]
        public override void Reset()
        {
            partyMembersMoving = false;
        }

        [TempleDllLocation(0x10114EF0)]
        public void HandleMessage(Message msg)
        {
            DoKeyboardScrolling();

            if (UiSystems.RadialMenu.HandleMessage(msg))
            {
                return;
            }

            if (UiSystems.InGameSelect.HandleMessage(msg))
            {
                return;
            }

            if (GameSystems.D20.RadialMenu.GetCurrentNode() == -1)
            {
                if (UiSystems.Dialog.IsActive)
                {
                    if (msg.type == MessageType.KEYSTATECHANGE)
                    {
                        UiSystems.Manager.AlwaysFalse = false;
                        UiSystems.Manager.HandleKeyEvent(msg.KeyStateChangeArgs);
                    }
                }
                else
                {
                    if (UiSystems.CharSheet.Inventory.DraggedObject != null)
                    {
                        Stub.TODO();
                        // TODO: Check if this is actually needed!!!
                        /*if (msg.type == MessageType.MOUSE)
                        {
                            var flags = msg.MouseArgs.flags;
                            if (flags.HasFlag(MouseEventFlag.LeftReleased) ||
                                flags.HasFlag(MouseEventFlag.RightReleased))
                            {
                                UiSystems.CharSheet.Inventory.DraggedObject = null;
                                Tig.Mouse.ClearDraggedIcon();
                            }
                        }*/
                    }

                    // TODO if ( !viewportIds[viewportIdx + 1] )
                    // TODO {

                    if (GameSystems.Combat.IsCombatActive())
                    {
                        if (!isCombatModeMessage)
                            partyMembersMoving = false;
                        isCombatModeMessage = true;
                        HandleCombatModeMessage(msg);
                    }
                    else
                    {
                        isCombatModeMessage = false;
                        HandleNormalModeMessage(msg);
                    }

                    // TODO }
                }
            }
        }

        [TempleDllLocation(0x10114eb0)]
        private void HandleCombatModeMessage(Message msg)
        {
            if (msg.type == MessageType.MOUSE)
            {
                var mouseArgs = msg.MouseArgs;
                if (mouseArgs.flags.HasFlag(MouseEventFlag.PosChange)
                    || mouseArgs.flags.HasFlag(MouseEventFlag.PosChangeSlow))
                {
                    CombatMouseHandler(mouseArgs);
                }
            }
            else if (msg.type == MessageType.KEYSTATECHANGE)
            {
                CombatKeyHandler(msg.KeyStateChangeArgs);
            }
        }

        [TempleDllLocation(0x101132b0)]
        private void CombatKeyHandler(MessageKeyStateChangeArgs args)
        {
            if (!args.down)
            {
                UiSystems.Manager.AlwaysFalse = false;
                UiSystems.Manager.HandleKeyEvent(args);

                // End turn for current player
                if (args.key == DIK.DIK_RETURN || args.key == DIK.DIK_SPACE)
                {
                    var currentActor = GameSystems.D20.Initiative.CurrentActor;
                    UiSystems.CharSheet.CurrentPage = 0;
                    UiSystems.CharSheet.Hide(UiSystems.CharSheet.State);

                    if (GameSystems.Party.IsInParty(currentActor))
                    {
                        if (!GameSystems.D20.Actions.IsCurrentlyPerforming(currentActor))
                        {
                            if (!UiSystems.InGameSelect.IsPicking && !UiSystems.RadialMenu.IsOpen)
                            {
                                GameSystems.Combat.AdvanceTurn(currentActor);
                                Logger.Info("Advancing turn for {0}", currentActor);
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10113370)]
        public void CenterOnParty()
        {
            GameObjectBody centerOn;
            if (GameSystems.Combat.IsCombatActive())
            {
                centerOn = GameSystems.D20.Initiative.CurrentActor;
                if (centerOn == null)
                {
                    return;
                }
            }
            else
            {
                centerOn = GameSystems.Party.GetConsciousLeader();
                if (centerOn == null)
                {
                    centerOn = GameSystems.Party.GetLeader();
                    if (centerOn == null)
                    {
                        return;
                    }
                }
            }

            var loc = centerOn.GetLocation();
            GameSystems.Scroll.CenterOnSmooth(loc.locx, loc.locy);
        }

        [TempleDllLocation(0x10113ce0)]
        public void CenterOnPartyLeader()
        {
            var leader = GameSystems.Party.GetConsciousLeader();
            if (leader != null)
            {
                var location = leader.GetLocation();
                GameSystems.Location.CenterOn(location.locx, location.locy);
            }
        }

        [TempleDllLocation(0x10114e30)]
        private void HandleNormalModeMessage(Message msg)
        {
            if (msg.type == MessageType.MOUSE)
            {
                if (UiSystems.CharSheet.HasCurrentCritter)
                {
                    return;
                }

                var args = msg.MouseArgs;
                var flags = args.flags;
                if (flags.HasFlag(MouseEventFlag.LeftReleased))
                {
                    HandleNormalLeftMouseReleased(args);
                }
                else if (flags.HasFlag(MouseEventFlag.LeftClick) || flags.HasFlag(MouseEventFlag.PosChange) &&
                         flags.HasFlag(MouseEventFlag.LeftDown))
                {
                    HandleNormalLeftMouseDragHandler(args);
                }
                else if (flags.HasFlag(MouseEventFlag.RightClick))
                {
                    HandleNormalRightMouseButton(args);
                }
                else if (flags.HasFlag(MouseEventFlag.PosChange) || flags.HasFlag(MouseEventFlag.PosChangeSlow))
                {
                    CombatMouseHandler(args);
                }
            }
            else if (msg.type == MessageType.KEYSTATECHANGE)
            {
                HandleNormalKeyStateChange(msg.KeyStateChangeArgs);
            }
        }

        [TempleDllLocation(0x101130B0)]
        private void HandleNormalKeyStateChange(MessageKeyStateChangeArgs args)
        {
            if (args.down)
            {
                return;
            }

            var leader = GameSystems.Party.GetConsciousLeader();
            if (leader != null)
            {
                if (GameSystems.D20.Hotkeys.IsReservedHotkey(args.key))
                {
                    if (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LCONTROL) ||
                        Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RCONTROL))
                    {
                        // trying to assign hotkey to reserved hotkey
                        GameSystems.D20.Hotkeys.HotkeyReservedPopup(args.key);
                        return;
                    }
                }
                else if (GameSystems.D20.Hotkeys.IsNormalNonreservedHotkey(args.key))
                {
                    if (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LCONTROL) ||
                        Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RCONTROL))
                    {
                        // assign hotkey
                        var leaderLoc = leader.GetLocationFull();
                        var screenPos = Tig.RenderingDevice.GetCamera().WorldToScreenUi(leaderLoc.ToInches3D());

                        UiSystems.RadialMenu.Spawn((int) screenPos.X, (int) screenPos.Y);
                        UiSystems.RadialMenu.HandleKeyMessage(args);
                        return;
                    }

                    GameSystems.D20.Actions.TurnBasedStatusInit(leader);
                    leader = GameSystems.Party.GetConsciousLeader(); // in case the leader changes somehow...
                    Logger.Info("Intgame: Resetting sequence.");
                    GameSystems.D20.Actions.CurSeqReset(leader);

                    GameSystems.D20.Actions.GlobD20ActnInit();
                    if (GameSystems.D20.Hotkeys.RadmenuHotkeySthg(GameSystems.Party.GetConsciousLeader(), args.key))
                    {
                        GameSystems.D20.Actions.ActionAddToSeq();
                        GameSystems.D20.Actions.sequencePerform();
                        var fellowPc = GameSystems.Party.GetFellowPc(leader);

                        if (GameSystems.Dialog.TryGetOkayVoiceLine(leader, fellowPc, out var voicelineText,
                            out var soundId))
                        {
                            GameSystems.Dialog.PlayCritterVoiceLine(leader, fellowPc, voicelineText, soundId);
                        }

                        GameSystems.D20.RadialMenu.ClearActiveRadialMenu();
                        return;
                    }
                }
            }

            UiSystems.Manager.AlwaysFalse = false;
            UiSystems.Manager.HandleKeyEvent(args);
        }

        [TempleDllLocation(0x10BD3B5C)]
        private bool _normalLmbClicked;

        [TempleDllLocation(0x10BD3B60)]
        private bool uiDragSelectOn;

        [TempleDllLocation(0x10113f30)]
        public GameObjectBody GetMouseTarget(int x, int y)
        {
            // Pick the object using the screen coordinates first
            var mousedOver = PickObject(x, y, GameRaycastFlags.HITTEST_3D);
            if (mousedOver == null)
            {
                return null;
            }

            if (GameSystems.MapObject.IsUntargetable(mousedOver))
            {
                return null;
            }

            if (mousedOver.IsCritter() && GameSystems.Critter.IsConcealed(mousedOver))
            {
                return null;
            }

            return mousedOver;
        }

        private GameObjectBody PickObject(int x, int y, GameRaycastFlags flags)
        {
            if (GameSystems.Raycast.PickObjectOnScreen(x, y, out var result, flags))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x10113010)]
        private void PartySelectedStandUpAndMoveToPosition(LocAndOffsets loc, bool walkFlag)
        {
            // make Prone party members do a Stand Up action first if they're prone
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (GameSystems.D20.D20Query(partyMember, D20DispatcherKey.QUE_Prone) == 0)
                {
                    continue;
                }

                GameSystems.D20.Actions.TurnBasedStatusInit(partyMember);
                GameSystems.D20.Actions.CurSeqReset(partyMember);
                GameSystems.D20.Actions.GlobD20ActnInit();
                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.STAND_UP, 0);
                GameSystems.D20.Actions.ActionAddToSeq();
                GameSystems.D20.Actions.sequencePerform();
            }

            GameSystems.Formation.PartySelectedFormationMoveToPosition(loc, walkFlag);
        }

        [TempleDllLocation(0x10BD3AC8)]
        private bool uiDragViewport;

        [TempleDllLocation(0x10BD3AD8)]
        private GameObjectBody mouseDragTgt;

        [TempleDllLocation(0x10BD3AE0)]
        private GameObjectBody qword_10BD3AE0;

        [TempleDllLocation(0x10BD3ACC)]
        private int uiDragSelectXMax = 0;

        [TempleDllLocation(0x10BD3AD0)]
        private int uiDragSelectYMax = 0;

        [TempleDllLocation(0x10114af0)]
        private void HandleNormalLeftMouseReleased(MessageMouseArgs args)
        {
            if (UiSystems.CharSheet.HasCurrentCritter)
            {
                return;
            }

            GameSystems.Anim.StartFidgetTimer();
            UiSystems.InGameSelect.FocusClear();
            partyMembersMoving = false;
            if (!_normalLmbClicked)
                return;
            _normalLmbClicked = false;
            if (uiDragSelectOn)
            {
                if (!Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LSHIFT) &&
                    !Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RSHIFT))
                {
                    GameSystems.Party.ClearSelection();
                }

                var rect = new Rectangle(uiDragSelectXMax, uiDragSelectYMax, args.X, args.Y);
                UiSystems.InGameSelect.SelectInRectangle(rect);
                uiDragSelectOn = false;
                Globals.UiManager.IsMouseInputEnabled = true;
                return;
            }

            var mouseTgt = GetMouseTarget(args.X, args.Y);
            if (mouseTgt == null)
            {
                if (GameSystems.Party.GetConsciousLeader() == null)
                {
                    return;
                }

                var worldPos = Tig.RenderingDevice.GetCamera().ScreenToTile(args.X, args.Y);

                if (!Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LSHIFT) &&
                    !Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RSHIFT))
                {
                    var alwaysRun = Globals.Config.AlwaysRun;
                    PartySelectedStandUpAndMoveToPosition(worldPos, !alwaysRun);
                    partyMembersMoving = true;
                }

                if (uiDragViewport || mouseDragTgt != null)
                {
                    // TODO I think this is always true
                    return;
                }
            }

            var tgtIsInParty = GameSystems.Party.IsInParty(mouseTgt);
            if (tgtIsInParty && UiSystems.CharSheet.State != CharInventoryState.LevelUp
                             && !GameSystems.D20.Actions.SeqPickerHasTargetingType())
            {
                var currentChar = UiSystems.CharSheet.CurrentCritter;
                if (currentChar != null && mouseTgt != currentChar)
                {
                    var state = UiSystems.CharSheet.Looting.GetLootingState();
                    UiSystems.CharSheet.ShowInState(state, mouseTgt);
                }

                if (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LSHIFT) || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RSHIFT))
                {
                    if (GameSystems.Party.IsSelected(mouseTgt))
                    {
                        GameSystems.Party.RemoveFromSelection(mouseTgt);
                        return;
                    }
                }
                else
                {
                    GameSystems.Party.ClearSelection();
                }

                GameSystems.Party.AddToSelection(mouseTgt);
                return;
            }

            Stub.TODO();
            // TODO: Call to 0x101140f0
        }

        [TempleDllLocation(0x101148b0)]
        private void HandleNormalLeftMouseDragHandler(MessageMouseArgs args)
        {
            GameObjectBody mouseTgt_ = null;
            var leftClick = args.flags.HasFlag(MouseEventFlag.LeftClick);
            var leftDown = args.flags.HasFlag(MouseEventFlag.LeftDown);
            if (leftClick || leftDown)
            {
                mouseTgt_ = GetMouseTarget(args.X, args.Y);
            }

            if (leftClick)
            {
                if (_normalLmbClicked)
                    return;
                mouseDragTgt = mouseTgt_;
                _normalLmbClicked = true;
                uiDragSelectXMax = args.X;
                uiDragSelectYMax = args.Y;
                qword_10BD3AE0 = mouseTgt_;
                uiDragViewport = mouseTgt_ == null;
            }

            if (args.flags.HasFlag(MouseEventFlag.PosChange) && leftDown && _normalLmbClicked)
            {
                if (!UiSystems.Dialog.IsActive2 && !UiSystems.RadialMenu.IsOpen)
                {
                    if (GetEstimatedSelectionSize(args.X, args.Y) > 25)
                    {
                        uiDragSelectOn = true;
                        Globals.UiManager.IsMouseInputEnabled = false;
                    }
                }

                if (uiDragSelectOn)
                {
                    UiSystems.InGameSelect.FocusClear();
                    UiSystems.InGameSelect.SetFocusToRect(uiDragSelectXMax, uiDragSelectYMax, args.X, args.Y);
                }

                if (mouseTgt_ != null
                    && !uiDragViewport
                    && mouseTgt_ == mouseDragTgt
                    && UiSystems.CharSheet.State != CharInventoryState.LevelUp
                    && (UiSystems.CharSheet.Looting.GetLootingState() != CharInventoryState.Looting
                        && UiSystems.CharSheet.Looting.GetLootingState() != CharInventoryState.Bartering
                        || UiSystems.CharSheet.Looting.Target != mouseTgt_
                        || mouseTgt_.type.IsEquipment()))
                {
                    UiSystems.InGameSelect.AddToFocusGroup(mouseTgt_);
                    UiSystems.Party.ForcePressed = mouseTgt_;
                }
            }
        }

        [TempleDllLocation(0x10113C80)]
        private int GetEstimatedSelectionSize(int x, int y)
        {
            return Math.Abs(uiDragSelectXMax - x) + Math.Abs(uiDragSelectYMax - y);
        }

        [TempleDllLocation(0x10114d90)]
        private void HandleNormalRightMouseButton(MessageMouseArgs args)
        {
            if ( GetMouseTarget(args.X, args.Y) != null )
            {
                var partyLeader = GameSystems.Party.GetConsciousLeader();
                if ( GameSystems.Party.IsPlayerControlled(partyLeader) )
                {
                    if ( GameSystems.D20.Actions.SeqPickerHasTargetingType() )
                    {
                        GameSystems.D20.Actions.SeqPickerTargetingTypeReset();
                    }
                    else
                    {
                        Logger.Info("state_default_process_mouse_right_down");
                        radialmenu_ignore_close_till_move(args.X, args.Y);
                    }
                }
            }
        }

        [TempleDllLocation(0x10114690)]
        private void CombatMouseHandler(MessageMouseArgs msg)
        {
            UiSystems.Party.ForcePressed = null;
            UiSystems.Party.ForceHovered = null;
            UiSystems.InGameSelect.FocusClear();

            if (!GameSystems.Map.IsClearingMap())
            {
                var mouseTarget = GetMouseTarget(msg.X, msg.Y);
                if (mouseTarget != null)
                {
                    if (!UiSystems.CharSheet.HasCurrentCritter
                        && !UiSystems.TownMap.IsVisible
                        && !UiSystems.Logbook.IsVisible
                        && UiSystems.CharSheet.State != CharInventoryState.LevelUp)
                    {
                        var type = mouseTarget.type;
                        if (_normalLmbClicked)
                        {
                            if (!uiDragViewport && mouseTarget == mouseDragTgt)
                            {
                                UiSystems.InGameSelect.AddToFocusGroup(mouseTarget);
                                UiSystems.Party.ForcePressed = mouseTarget;
                            }
                        }
                        else
                        {
                            if (type.IsCritter()
                                || type.IsEquipment()
                                || type == ObjectType.container
                                || type == ObjectType.portal
                                || mouseTarget.ProtoId == 2064 /* Guest Book */)
                            {
                                UiSystems.InGameSelect.Focus = mouseTarget;
                            }
                            else if (type == ObjectType.scenery)
                            {
                                var teleportTo = mouseTarget.GetInt32(obj_f.scenery_teleport_to);
                                if (teleportTo != 0)
                                {
                                    UiSystems.InGameSelect.Focus = mouseTarget;
                                }
                            }

                            if (type.IsCritter() && GameSystems.Party.IsInParty(mouseTarget)){
                                UiSystems.Party.ForceHovered = mouseTarget;
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10113fb0)]
        private void DoKeyboardScrolling()
        {
            // TODO: This is very stupid...
            if (Tig.Console.IsVisible || UiSystems.ItemCreation.IsVisible || UiSystems.TextDialog.IsVisible)
            {
                return;
            }

            if (Tig.Keyboard.IsPressed(DIK.DIK_UP))
            {
                if (Tig.Keyboard.IsPressed(DIK.DIK_LEFT))
                {
                    GameSystems.Scroll.SetScrollDirection(ScrollDirection.UP_LEFT);
                }
                else if (Tig.Keyboard.IsPressed(DIK.DIK_RIGHT))
                {
                    GameSystems.Scroll.SetScrollDirection(ScrollDirection.UP_RIGHT);
                }
                else
                {
                    GameSystems.Scroll.SetScrollDirection(ScrollDirection.UP);
                }
            }
            else if (Tig.Keyboard.IsPressed(DIK.DIK_DOWN))
            {
                if (Tig.Keyboard.IsPressed(DIK.DIK_LEFT))
                {
                    GameSystems.Scroll.SetScrollDirection(ScrollDirection.DOWN_LEFT);
                }
                else if (Tig.Keyboard.IsPressed(DIK.DIK_RIGHT))
                {
                    GameSystems.Scroll.SetScrollDirection(ScrollDirection.DOWN_RIGHT);
                }
                else
                {
                    GameSystems.Scroll.SetScrollDirection(ScrollDirection.DOWN);
                }
            }
            else if (Tig.Keyboard.IsPressed(DIK.DIK_LEFT))
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.LEFT);
            }
            else if (Tig.Keyboard.IsPressed(DIK.DIK_RIGHT))
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.RIGHT);
            }
        }

        [TempleDllLocation(0x10BD3B68)]
        private bool _isRecovering;

        [TempleDllLocation(0x10BD3AFC)]
        private int[] objRecovery_10BD3AFC = new int[10];

        [TempleDllLocation(0x10BD3B44)]
        private int idx_10BD3B44;

        [TempleDllLocation(0x102F6A28)]
        private int[] dword_102F6A28 = new int[10];

        [TempleDllLocation(0x10113CD0)]
        public int sub_10113CD0()
        {
            return objRecovery_10BD3AFC[idx_10BD3B44];
        }

        /**
         * Main loop will only do mouse scrolling when this function returns true.
         * The argument is what is returned by the sub above (sub_10113CD0).
         */
        [TempleDllLocation(0x10113D40)]
        public int sub_10113D40(int a1)
        {
            return dword_102F6A28[a1];
        }

        [TempleDllLocation(0x10115040)]
        public void Recovery(int a1)
        {
            // TODO

            if (_isRecovering)
            {
                return;
            }

            int v4; // edi@2
            int v5; // eax@3
            int v6; // ecx@5

            var v1 = false;
            var v2 = false;
            var v3 = false;
            v4 = a1;
            while (true)
            {
                v5 = objRecovery_10BD3AFC[idx_10BD3B44];
                _isRecovering = true;

                if (v4 != 0)
                {
                    idx_10BD3B44++;
                    v1 = true;
                }
                else if (idx_10BD3B44 > 0)
                {
                    v4 = objRecovery_10BD3AFC[idx_10BD3B44 - 1];
                    idx_10BD3B44--;
                    v2 = true;
                }

                switch (v5)
                {
                    case 1:
                    case 2:
                        v3 = true;
                        break;
                    case 3:
                        CenterOnPartyLeader();
                        if (v2)
                        {
                            var v7 = GameSystems.Party.GetPCGroupMemberN(0);
                            UiSystems.Dialog.CancelDialog(v7);
                        }
                        else
                        {
                            var v8 = GameSystems.Party.GetPCGroupMemberN(0);
                            UiSystems.Dialog.sub_1014BAD0(v8);
                        }

                        break;
                    case 5:
                        // The scroll hook was reset here, but it was never set to anything
                        break;
                }

                switch (v4)
                {
                    case 0:
                    case 1:
                    case 2:
                        Tig.Mouse.GetState(out var mouseState);
                        var args = new MessageMouseArgs(mouseState.x, mouseState.y, 0, default);
                        CombatMouseHandler(args);
                        break;
                    case 3:
                        if (!v1)
                        {
                            var v9 = GameSystems.Party.GetPCGroupMemberN(0);
                            UiSystems.Dialog.sub_1014BFF0(v9);
                        }

                        break;
                }

                objRecovery_10BD3AFC[idx_10BD3B44] = v4;
                _isRecovering = false;
                if (!v3)
                    return;
                v4 = 0;
                v1 = false;
                v2 = false;
                v3 = false;
            }
        }

        [TempleDllLocation(0x10112fc0)]
        public CursorType? GetCursor()
        {
            var cursor = UiSystems.Combat.GetCursor();
            if (cursor.HasValue)
            {
                return cursor;
            }

            cursor = UiSystems.Party.GetCursor();
            if (cursor.HasValue)
            {
                return cursor;
            }

            cursor = UiSystems.CharSheet.Looting.GetCursor();
            if (cursor.HasValue)
            {
                return cursor;
            }

            cursor = UiSystems.RadialMenu.GetCursor();
            if (cursor.HasValue)
            {
                return cursor;
            }

            cursor = UiSystems.HelpManager.GetCursor();
            if (cursor.HasValue)
            {
                return cursor;
            }

            return null;
        }
    }
}