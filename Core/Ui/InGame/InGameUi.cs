using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;

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

        public void ResetInput()
        {
            // TODO throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10139400)]
        public void FocusClear()
        {
            Stub.TODO();
        }

        public bool SaveGame()
        {
            return true;
        }

        [TempleDllLocation(0x101140c0)]
        public bool LoadGame()
        {
            FocusClear();
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
            Stub.TODO();

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
                        Stub.TODO();
                        /*TigKeyboardMsgInit(&kbMsg); TODO
                        kbMsg.tigmsg = msg;
                        UiManStateSet(0);
                        UiManagerKeyEventHandler(&kbMsg);*/
                    }
                }
                else
                {
                    if (UiSystems.CharSheet.Inventory.DraggedObject != null)
                    {
                        if (msg.type == MessageType.MOUSE)
                        {
                            var flags = msg.MouseArgs.flags;
                            if (flags.HasFlag(MouseEventFlag.LeftReleased) ||
                                flags.HasFlag(MouseEventFlag.RightReleased))
                            {
                                UiSystems.CharSheet.Inventory.DraggedObject = null;
                                Tig.Mouse.ClearDraggedIcon();
                            }
                        }
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
            Stub.TODO();
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

        [TempleDllLocation(0x10114af0)]
        private void HandleNormalLeftMouseReleased(MessageMouseArgs args)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101148b0)]
        private void HandleNormalLeftMouseDragHandler(MessageMouseArgs args)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10114d90)]
        private void HandleNormalRightMouseButton(MessageMouseArgs args)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10114690)]
        private void CombatMouseHandler(MessageMouseArgs args)
        {
            Stub.TODO();
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
                            UiSystems.Dialog.sub_1014BA40(v7);
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