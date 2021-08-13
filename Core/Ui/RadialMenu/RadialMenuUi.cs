using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Utils;
using Vector3 = System.Numerics.Vector3;

namespace OpenTemple.Core.Ui.RadialMenu
{
    public class RadialMenuUi : IDisposable, IViewportAwareUi
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private RadialMenuSystem RadialMenus => GameSystems.D20.RadialMenu;

        // Keep the delegate around so we can compare against it later
        private readonly CursorDrawCallback _hotkeyAssignCursorDrawDelegate;

        [TempleDllLocation(0x10be6d64)]
        private ResourceRef<ITexture> _portraitAlphaMask;

        [TempleDllLocation(0x10be6d6c)]
        private ResourceRef<ITexture> _checkboxUnchecked;

        [TempleDllLocation(0x10be6d68)]
        private ResourceRef<ITexture> _checkboxChecked;

        [TempleDllLocation(0x102F95E0)]
        private readonly Dictionary<RadialMenuStandardNode, ResourceRef<ITexture>> _standardNodeIcons =
            new();

        [TempleDllLocation(0x10be6cd0)]
        private readonly PackedLinearColorA[] _radMenuColors = new PackedLinearColorA[24];

        [TempleDllLocation(0x10be6d48)]
        private int radMenuWidth => Globals.UiManager.CanvasSize.Width;

        [TempleDllLocation(0x10be67c4)]
        private int radMenuHeight => Globals.UiManager.CanvasSize.Height;

        private readonly TextEngine _textEngine = Tig.RenderingDevice.TextEngine;

        [TempleDllLocation(0x1013d230)]
        public RadialMenuUi()
        {
            _portraitAlphaMask = Tig.Textures.Resolve("art/interface/radial_menu/portrait_alpha.tga", false);
            _checkboxChecked = Tig.Textures.Resolve("art/interface/radial_menu/checked.tga", false);
            _checkboxUnchecked = Tig.Textures.Resolve("art/interface/radial_menu/unchecked.tga", false);

            _standardNodeIcons[RadialMenuStandardNode.Spells] =
                Tig.Textures.Resolve("art/interface/radial_menu/icon_spells.tga", false);
            _standardNodeIcons[RadialMenuStandardNode.Skills] =
                Tig.Textures.Resolve("art/interface/radial_menu/icon_skills.tga", false);
            _standardNodeIcons[RadialMenuStandardNode.Feats] =
                Tig.Textures.Resolve("art/interface/radial_menu/icon_feats.tga", false);
            _standardNodeIcons[RadialMenuStandardNode.Class] =
                Tig.Textures.Resolve("art/interface/radial_menu/icon_class.tga", false);
            _standardNodeIcons[RadialMenuStandardNode.Combat] =
                Tig.Textures.Resolve("art/interface/radial_menu/icon_combat.tga", false);
            _standardNodeIcons[RadialMenuStandardNode.Items] =
                Tig.Textures.Resolve("art/interface/radial_menu/icon_items.tga", false);

            _radMenuColors = LoadColorTable();

            RadialMenuSliderWndInit();

            _hotkeyAssignCursorDrawDelegate = HotkeyAssignMouseTextCreate;

            Globals.UiManager.OnCanvasSizeChanged += ResizeViewport;
            ResizeViewport(Globals.UiManager.CanvasSize);
        }

        [TempleDllLocation(0x1013cbc0)]
        private void RadialMenuSliderWndInit()
        {
            // This initializes the little popup window to set the slider value of an entry
            Stub.TODO();
        }

        private static PackedLinearColorA[] LoadColorTable()
        {
            var colorTable = Tig.FS.ReadMesFile("art/interface/radial_menu/colors.mes");

            PackedLinearColorA ParseColor(int index)
            {
                var colorArr = colorTable[index].Split(' ').Select(byte.Parse).ToArray();
                // Alpha will be set automatically based on fade-in
                return new PackedLinearColorA(colorArr[0], colorArr[1], colorArr[2], 0);
            }

            var colors = new PackedLinearColorA[24];
            for (var i = 0; i < 24; i++)
            {
                colors[i] = ParseColor(i);
            }

            return colors;
        }

        [TempleDllLocation(0x10BE6D9C)]
        private bool _assigningHotkey; // TODO: probably bool

        [TempleDllLocation(0x10be6da0)]
        private DIK _dikToBeHotkeyed;

        [TempleDllLocation(0x10BE6D70)]
        public bool dword_10BE6D70;

        [TempleDllLocation(0x10be6d80)]
        private bool _ignoreClose;

        [TempleDllLocation(0x10be67ac)]
        private int _lastRmbClickX;

        [TempleDllLocation(0x10be6d30)]
        private int _lastRmbClickY;

        [TempleDllLocation(0x10be67c8)]
        private LocAndOffsets _openedAtLocation;

        [TempleDllLocation(0x10be6d78)]
        private GameObjectBody _openedAtTarget;

        /// <summary>
        /// The current on-screen position of the opened Radial Menu's center.
        /// </summary>
        private Point CurrentMenuCenterOnScreen
        {
            get
            {
                var worldPos = Vector3.Zero;
                var worldPos2D = RadialMenus.ActiveMenuWorldPosition;
                worldPos.X = worldPos2D.X;
                worldPos.Z = worldPos2D.Y;

                // TODO If this is the case, the radial menu should actually be bound to the game view it was opened in
                var screenPos = GameViews.Primary.WorldToScreen(worldPos);
                return new Point((int) screenPos.X, (int) screenPos.Y);
            }
        }

        [TempleDllLocation(0x1013dc90)]
        [TemplePlusLocation("radialmenu.cpp:128")]
        public bool HandleMessage(Message message)
        {
            var shiftPressed = Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LSHIFT)
                               || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RSHIFT);
            RadialMenus.ShiftPressed = shiftPressed;

            if (RadialMenus.GetCurrentNode() == -1)
            {
                return false;
            }

            if (message.type == MessageType.KEYSTATECHANGE)
            {
                return HandleKeyMessage(message.KeyStateChangeArgs);
            }
            else if (message.type == MessageType.MOUSE)
            {
                return HandleMouseMessage(message.MouseArgs);
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x1013dc90)]
        [TemplePlusLocation("radialmenu.cpp:128")]
        private bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
            {
                var clickedNodeIdx = UiRadialGetNodeClick(msg.X, msg.Y);
                if (clickedNodeIdx == -1)
                {
                    _assigningHotkey = false;
                    if (Tig.Mouse.CursorDrawCallback == _hotkeyAssignCursorDrawDelegate)
                    {
                        Tig.Mouse.SetCursorDrawCallback(null);
                    }

                    RadialMenus.ClearActiveRadialMenu();
                    return true;
                }

                var clickedNode = RadialMenus.GetActiveRadMenuNodeRegardMorph(clickedNodeIdx);
                if (_assigningHotkey
                    && GameSystems.D20.Hotkeys.HotkeyAssignCreatePopupUi(ref clickedNode.entry, _dikToBeHotkeyed))
                {
                    _assigningHotkey = false;
                    RadialMenus.ClearActiveRadialMenu();
                    if (Tig.Mouse.CursorDrawCallback == _hotkeyAssignCursorDrawDelegate)
                    {
                        Tig.Mouse.SetCursorDrawCallback(null);
                    }

                    return true;
                }

                if (UiSystems.HelpManager.IsSelectingHelpTarget)
                {
                    var helpTopic = clickedNode.entry.helpSystemHashkey;
                    if (helpTopic != null)
                    {
                        UiSystems.HelpManager.ClickForHelpCallback(helpTopic);
                    }

                    return true;
                }

                if (RadialMenus.RadialMenuSetActiveNode(clickedNodeIdx))
                {
                    ActivateActiveNode();
                    return true;
                }

                return false;
            }

            if ((msg.flags & MouseEventFlag.RightReleased) != 0)
            {
                return UiRadialMenuRmbReleased(msg);
            }

            if ((msg.flags & MouseEventFlag.RightClick) != 0)
            {
                return HandleRightMouseClick(msg.X, msg.Y);
            }

            if ((msg.flags & MouseEventFlag.PosChange) != 0)
            {
                return RadialMenu_Processing(msg.X, msg.Y);
            }

            return false;
        }

        private void ActivateActiveNode()
        {
            if (RadialMenus.RadialMenuActiveNodeExecuteCallback())
            {
                GameSystems.D20.Actions.GlobD20ActnSetTarget(_openedAtTarget, _openedAtLocation);
                GameSystems.D20.Actions.ActionAddToSeq();
                GameSystems.D20.Actions.sequencePerform();

                // Play a confirm voice line
                var leader = GameSystems.Party.GetConsciousLeader();
                var listener = GameSystems.Dialog.GetListeningPartyMember(leader);
                GameSystems.Dialog.TryGetOkayVoiceLine(leader, listener, out var text, out var soundId);
                GameSystems.Dialog.PlayCritterVoiceLine(leader, listener, text, soundId);
            }
        }

        [TempleDllLocation(0x1013d910)]
        private bool UiRadialMenuRmbReleased(MessageMouseArgs msg)
        {
            if (_ignoreClose)
            {
                if (!dword_10BE6D70)
                {
                    if (msg.X != _lastRmbClickX || msg.Y != _lastRmbClickY)
                    {
                        // NOTE: Vanilla previously didn't check whether the target was untargetable here...
                        UpdateRadialMenuTarget(msg.X, msg.Y);
                    }
                    else
                    {
                        // Clear the active menu when we're in the smack center of the menu (within the portrait)
                        var currentMenuPos = CurrentMenuCenterOnScreen;
                        var deltaX = msg.X - currentMenuPos.X;
                        var deltaY = msg.Y - currentMenuPos.Y;
                        var distanceFromCenter = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

                        if (distanceFromCenter < 32.0)
                        {
                            RadialMenus.ClearActiveRadialMenu();
                        }
                    }
                }

                Logger.Info("intgame_radialmenu: _mouse_right_up: ignore_close = 0");
                dword_10BE6D70 = false;
                _ignoreClose = false;
                return true;
            }

            var nodeIdxClicked = UiRadialGetNodeClick(msg.X, msg.Y);
            if (nodeIdxClicked == -1)
            {
                if (_assigningHotkey)
                {
                    _assigningHotkey = false;
                    if (Tig.Mouse.CursorDrawCallback == _hotkeyAssignCursorDrawDelegate)
                    {
                        Tig.Mouse.SetCursorDrawCallback(null);
                    }
                }

                RadialMenus.ClearActiveRadialMenu();
            }
            else if (RadialMenus.RadialMenuSetActiveNode(nodeIdxClicked))
            {
                // TODO: This seems HIGHLY suspect!!!
                var currentArg = RadialMenus.RadialMenuGetActualArg(nodeIdxClicked);
                RadialMenus.RadialMenuSetActiveNodeArg(currentArg + 1);
            }

            return true;
        }

        [TempleDllLocation(0x1013c130)]
        public void HotkeyAssignMouseTextCreate(int x, int y, object userArg)
        {
            if (_assigningHotkey)
            {
                var keyName = GameSystems.D20.Hotkeys.GetKeyDisplayName(_dikToBeHotkeyed);
                var assignText = GameSystems.D20.Combat.GetCombatMesLine(191);

                var paragraph = new Paragraph() {
                    Host = UiSystems.InGameSelect.TextHost
                };
                paragraph.AddStyle("radialmenu-hotkey");
                paragraph.AppendContent($"{assignText} '{keyName}'");
                using var layout = _textEngine.CreateTextLayout(paragraph, 0, 0);

                _textEngine.RenderTextLayout(x + 32, y + 32, layout);
            }
        }

        [TempleDllLocation(0x10139e50)]
        public bool IsOpen => RadialMenus.GetCurrentNode() != -1;

        [TempleDllLocation(0x1013c9c0)]
        public bool HandleKeyMessage(MessageKeyStateChangeArgs args)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1013b250)]
        public void Spawn(int screenX, int screenY)
        {
            UiSystems.InGame.ResetInput();
            if (RadialMenus.GetCurrentNode() != -1 /* Already opened */
                || UiSystems.InGameSelect.IsPicking
                || UiSystems.Dialog.IsVisible)
            {
                return;
            }

            _ignoreClose = false;

            UpdateRadialMenuTarget(screenX, screenY);

            var leader = GameSystems.Party.GetConsciousLeader();
            GameSystems.D20.Actions.TurnBasedStatusInit(leader);
            GameSystems.D20.Actions.ActSeqSpellReset();
            Logger.Info("Radial menu: Reseting sequence");
            GameSystems.D20.Actions.CurSeqReset(leader);
            GameSystems.D20.Actions.GlobD20ActnInit();

            var worldPos = _openedAtLocation.ToInches2D();
            RadialMenus.BuildRadialMenuAndSetToActive(leader, worldPos);

            UiSystems.InGameSelect.ClearFocusGroup();
            UiSystems.InGameSelect.Focus = null;
            UiSystems.Party.ForceHovered = null;
        }

        private void UpdateRadialMenuTarget(int screenX, int screenY)
        {
            // TODO: This is not quite correct, it should be tied into which viewport was clicked
            var viewport = GameViews.Primary;

            // Remember where we're opening the radial menu at / on which object, to auto-target chosen abilities
            if (GameSystems.Raycast.PickObjectOnScreen(viewport, screenX, screenY, out var objUnderMouse,
                    GameRaycastFlags.HITTEST_3D)
                && !GameSystems.MapObject.IsUntargetable(objUnderMouse))
            {
                _openedAtTarget = objUnderMouse;
                _openedAtLocation = _openedAtTarget.GetLocationFull();
            }
            else
            {
                _openedAtTarget = null;
                _openedAtLocation = GameViews.Primary.ScreenToTile(screenX, screenY);
            }
        }

        [TempleDllLocation(0x10139e60)]
        public CursorType? GetCursor()
        {
            if (_assigningHotkey)
            {
                return CursorType.HotKeySelection;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x1013dd10)]
        [TempleDllLocation(0x1013dad0)]
        public bool HandleRightMouseClick(int x, int y)
        {
            if (!_ignoreClose || dword_10BE6D70)
            {
                if (UiRadialGetNodeClick(x, y) != -1 || IsWithin25PixelsOfCenter(x, y))
                {
                    var screenPos = CurrentMenuCenterOnScreen;

                    _lastRmbClickX = x;
                    _lastRmbClickY = y;
                    RadialMenus.RelativeMousePosX = screenPos.X - x;
                    RadialMenus.RelativeMousePosY = screenPos.Y - y;
                    _ignoreClose = true;
                }

                return true;
            }
            else
            {
                return RadialMenu_Processing(x, y);
            }
        }

        [TempleDllLocation(0x1013aee0)]
        private bool IsWithin25PixelsOfCenter(int x, int y)
        {
            var screenCenter = CurrentMenuCenterOnScreen;
            var deltaX = screenCenter.X - x;
            var deltaY = screenCenter.Y - y;
            var distFromCenter = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
            return distFromCenter < 25.0f;
        }

        [TempleDllLocation(0x1013d640)]
        private bool RadialMenu_Processing(int x, int y)
        {
            if (_ignoreClose)
            {
                Logger.Info("intgame_radialmenu: _mouse_moved: ignore_close = 0");
                var screenX = x + RadialMenus.RelativeMousePosX;
                var screenY = y + RadialMenus.RelativeMousePosY;
                dword_10BE6D70 = false;
                var mouseLoc = GameViews.Primary.ScreenToTile(screenX, screenY);
                RadialMenus.ActiveMenuWorldPosition = mouseLoc.ToInches2D();
                return false;
            }

            _mouseOverNodeIdx = UiRadialGetNodeClick(x, y);
            // TODO: This condition seems fucked, shouldn't it only activate when the node has NO children???
            if (!Globals.Config.RadialMenuClickToActivate
                && _mouseOverNodeIdx != -1
                && RadialMenus.RadialMenuSetActiveNode(_mouseOverNodeIdx)
                && RadialMenus.GetRadialActiveMenuNodeChildrenCount(_mouseOverNodeIdx) != 0)
            {
                ActivateActiveNode();
            }

            return true;
        }

        [TempleDllLocation(0x1013c790)]
        private int UiRadialGetNodeClick(int mouseX, int mouseY)
        {
            var activeRootNodeIdx = -1;
            var activeRootNodeAngle = 0.0f;

            var screenPos = CurrentMenuCenterOnScreen;
            int menuScreenX = screenPos.X;
            int menuScreenY = screenPos.Y;
            var currentNode = RadialMenus.GetCurrentNode();

            // Calculate the angles (sizes) of the nodes up front, since it only depends on the number
            // of visible nodes
            var visibleRootNodes = RadialMenuCountChildren(0);
            var divByNumRootNodes = 1.0f / visibleRootNodes;
            var halfAngleOfNode = MathF.PI * divByNumRootNodes;

            var visibleRootNodeIdx = 0;
            var rootNodeCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(0);
            for (var rootChildIndex = 0; rootChildIndex < rootNodeCount; rootChildIndex++)
            {
                var rootNodeIndex = RadialMenus.RadialMenuGetChild(0, rootChildIndex);
                // Check if the root node is actually visible
                if (RadialMenus.ActiveRadialHasChildrenWithCallback(rootNodeIndex))
                {
                    var centerOfNodeAngle = 2 * MathF.PI * divByNumRootNodes * visibleRootNodeIdx;
                    if (rootNodeIndex == currentNode ||
                        RadialMenus.RadialMenuNodeContainsChildQuery(rootNodeIndex, currentNode))
                    {
                        activeRootNodeIdx = rootNodeIndex;
                        activeRootNodeAngle = centerOfNodeAngle;
                    }

                    var mouseDistFromMenuCenterX = mouseX - (float) menuScreenX;
                    var mouseDistFromMenuCenterY = mouseY - (float) menuScreenY;
                    var mouseDistFromMenuCenter = MathF.Sqrt(mouseDistFromMenuCenterY * mouseDistFromMenuCenterY
                                                             + mouseDistFromMenuCenterX * mouseDistFromMenuCenterX);

                    // Calculate whether the mouse position is within this root node's slice of the menu
                    var sliceStartAngle = centerOfNodeAngle - halfAngleOfNode;
                    var v25 = -MathF.Sin(sliceStartAngle);
                    var v24 = MathF.Cos(sliceStartAngle);

                    var sliceEndAngle = centerOfNodeAngle + halfAngleOfNode;
                    var sth2 = MathF.Sin(sliceEndAngle);
                    var sth = -MathF.Cos(sliceEndAngle);

                    if (mouseDistFromMenuCenter >= 25.0f
                        && mouseDistFromMenuCenter <= 57.0f
                        && v24 * mouseDistFromMenuCenterY + v25 * mouseDistFromMenuCenterX > 0.0f
                        && sth * mouseDistFromMenuCenterY + sth2 * mouseDistFromMenuCenterX > 0.0f)
                    {
                        return rootNodeIndex;
                    }

                    ++visibleRootNodeIdx;
                }
            }

            if (activeRootNodeIdx == -1)
            {
                return -1;
            }

            return sub_1013AF70 /*0x1013af70*/(activeRootNodeIdx, activeRootNodeAngle, 1, 57, 0, mouseX, mouseY);
        }

        // TODO: Clean this up / refactor
        [TempleDllLocation(0x1013af70)]
        public int sub_1013AF70 /*0x1013af70*/(int parentNodeIdx, float parentNodeAngle, int menuDepth, float width1pad,
            float width2pad, int mouseX, int mouseY)
        {
            int result;

            var parentOfActiveNodeIdx = -1;
            float parentOfActiveNodeSliceCenter = 0.0f;

            var v21 = 0;
            var currentNodeIdx = RadialMenus.GetCurrentNode();
            var screenPos = CurrentMenuCenterOnScreen;
            int v27 = screenPos.X;
            int v28 = screenPos.Y;
            var v16 = mouseX - (float) v27;
            float v10 = mouseY - (float) v28;
            var distFromCenter = MathF.Sqrt(v10 * v10 + v16 * v16);

            var visibleChildCount = RadialMenuCountChildren(parentNodeIdx);
            var radiusMin = width1pad;
            var widthPerChild = 16.0f / width1pad;
            var overallWidthHalf = visibleChildCount * widthPerChild * 0.5f;
            var radmenuX = parentNodeAngle - overallWidthHalf;
            var radmenuY = parentNodeAngle + overallWidthHalf;
            RadialMenuAnglesSthg_10139FE0(ref parentNodeAngle, ref radmenuY, ref radmenuX);

            var childrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(parentNodeIdx);

            float radiusMax = width1pad + 15;
            float widthout2 = width2pad;
            for (var v7 = 0; v7 < childrenCount; v7++)
            {
                int childNodeIdx = RadialMenus.RadialMenuGetChild /*0x100f0890*/(parentNodeIdx, v7);
                if (RadialMenus.ActiveRadialHasChildrenWithCallback /*0x100f0a50*/(childNodeIdx))
                {
                    RadialMenus.GetActiveRadMenuNodeRegardMorph /*0x100f08f0*/(childNodeIdx);
                    float v18 = widthPerChild * 0.5f;
                    var childNodeSliceCenter = v21 * widthPerChild + parentNodeAngle - overallWidthHalf + v18;
                    RadialMenuGetNodeWidth(parentNodeIdx, menuDepth, width1pad, width2pad,
                        out radiusMax, out widthout2, widthPerChild);
                    if (currentNodeIdx == childNodeIdx ||
                        RadialMenus.RadialMenuNodeContainsChildQuery /*0x100f0930*/(childNodeIdx, currentNodeIdx))
                    {
                        parentOfActiveNodeIdx = childNodeIdx;
                        parentOfActiveNodeSliceCenter = childNodeSliceCenter;
                    }

                    var v29 = childNodeSliceCenter - v18;
                    var v11 = v29;
                    float v31 = -MathF.Sin(v29);
                    float v30 = MathF.Cos(v11);
                    v29 = childNodeSliceCenter + v18;
                    var v12 = v29;
                    float v20 = MathF.Sin(v29);
                    v29 = -MathF.Cos(v12);
                    if (distFromCenter >= radiusMin
                        && distFromCenter <= radiusMax
                        && v30 * v10 + v31 * v16 > 0.0
                        && v29 * v10 + v20 * v16 > 0.0)
                    {
                        return childNodeIdx;
                    }

                    ++v21;
                }
            }

            if (parentOfActiveNodeIdx == -1)
            {
                return -1;
            }

            result = sub_1013AF70(parentOfActiveNodeIdx, parentOfActiveNodeSliceCenter, menuDepth + 1, radiusMax,
                widthout2, mouseX,
                mouseY);
            return result;
        }

        private const float TWO_PI = 2.0f * MathF.PI;
        private const float PI_HALF = MathF.PI / 2.0f;
        private const float PI_THIRD = MathF.PI / 3.0f;

        // TODO: This function needs to be cleaned up
        [TempleDllLocation(0x10139fe0)]
        public void RadialMenuAnglesSthg_10139FE0(ref float a1, ref float a2, ref float a3)
        {
            float i;
            float v11;

            var v3 = a1;
            var v16 = a3;
            var v15 = a2;
            if (v3 < PI_HALF || v3 > MathF.PI + PI_HALF)
            {
                for (; v3 < -MathF.PI; v3 += TWO_PI)
                {
                    ;
                }

                for (; v3 >= MathF.PI; v3 -= TWO_PI)
                {
                    ;
                }

                for (i = v16; i < -MathF.PI; i += TWO_PI)
                {
                    ;
                }

                for (; i >= MathF.PI; i -= TWO_PI)
                {
                    ;
                }

                var v10 = v15;
                if (v15 < -MathF.PI)
                {
                    do
                    {
                        v10 += TWO_PI;
                    } while (v10 < -MathF.PI);

                    v15 = v10;
                }

                if (v10 >= MathF.PI)
                {
                    do
                    {
                        v10 = v10 - TWO_PI;
                    } while (v10 >= MathF.PI);

                    v15 = v10;
                }

                if (v10 <= PI_THIRD)
                {
                    if (i >= -PI_THIRD)
                    {
                        goto LABEL_28;
                    }

                    float v12 = i + PI_THIRD;
                    v15 = v15 - v12;
                    v3 = v3 - v12;
                    i = -PI_THIRD;
                    if (v15 <= PI_THIRD)
                    {
                        goto LABEL_28;
                    }

                    float v13 = PI_THIRD - v15;
                    var v14 = v13;
                    i = v13 - PI_THIRD;
                    v11 = v14;
                }
                else
                {
                    v11 = PI_THIRD - v10;
                    i = i + v11;
                }

                v3 = v3 + v11;
                v15 = PI_THIRD;
            }
            else
            {
                i = v16;
                if (v16 < MathF.PI - PI_THIRD)
                {
                    float v5 = MathF.PI - PI_THIRD - i;
                    a3 = MathF.PI - PI_THIRD;
                    a1 = v5 + v3;
                    a2 = v5 + v15;
                    return;
                }

                if (v15 > MathF.PI + PI_THIRD)
                {
                    float v7 = MathF.PI + PI_THIRD - v15;
                    v15 = MathF.PI + PI_THIRD;
                    v3 = v3 + v7;
                    i = i + v7;
                    if (i < MathF.PI - PI_THIRD)
                    {
                        float v8 = MathF.PI - PI_THIRD - i;
                        float v9 = v8 + MathF.PI + PI_THIRD;
                        a3 = MathF.PI - PI_THIRD;
                        a1 = v3 + v8;
                        a2 = v9;
                        return;
                    }
                }
            }

            LABEL_28:
            a3 = i;
            a1 = v3;
            a2 = v15;
        }

        private TextLayout GetNodeDisplayText(RadialMenuNode node)
        {
            // We're not always using a stringbuilder because for most entries, the text will be displayed as-is
            var displayText = new Paragraph();
            displayText.AddStyle("radialmenu-text");
            displayText.AppendContent(node.entry.text);
            if (node.entry.HasMinArg && node.entry.HasMaxArg)
            {
                var maxarg = node.entry.maxArg;
                var minarg = node.entry.minArg;
                if (minarg >= maxarg)
                {
                    displayText.AppendContent($" ({minarg}/{maxarg})");
                }
                else
                {
                    displayText.AppendContent(" (");
                    displayText.AppendContent($"{minarg}", new StyleDefinition()
                    {
                        Color = new PackedLinearColorA(255, 102, 102, 255)
                    });
                    displayText.AppendContent($"/{maxarg})");
                }
            }

            var hotkeyLetter = GameSystems.D20.Hotkeys.GetHotkeyLetter(ref node.entry);
            if (hotkeyLetter != null)
            {
                displayText.AppendContent($" [{hotkeyLetter}]");
            }

            return _textEngine.CreateTextLayout(displayText,
                0,
                0
            );
        }

        [TempleDllLocation(0x1013ac50)]
        public void RadialMenuGetNodeWidth(int activeNodeIdx, int _280, float width1pad, float width2pad, out float widthout1,
            out float widthout2, float widthfactor)
        {
            var style = new TigTextStyle();
            style.textColor = new ColorRect(PackedLinearColorA.White);
            style.shadowColor = new ColorRect(PackedLinearColorA.White);
            style.colors4 = new ColorRect(PackedLinearColorA.White);
            style.colors2 = new ColorRect(PackedLinearColorA.White);
            style.flags = 0;
            style.field2c = -1;
            style.field0 = 0;
            style.kerning = 2;
            style.leading = 0;
            style.tracking = 5;

            // Default values apparently
            widthout1 = width1pad + 15;
            widthout2 = width2pad;

            // Enumerate all visible children
            var childrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(activeNodeIdx);
            for (var childIdx = 0; childIdx < childrenCount; childIdx++)
            {
                var childNodeIdx = RadialMenus.RadialMenuGetChild(activeNodeIdx, childIdx);
                if (!RadialMenus.ActiveRadialHasChildrenWithCallback(childNodeIdx))
                {
                    // Skip invisible child nodes
                    continue;
                }

                var radEntry = RadialMenus.GetActiveRadMenuNodeRegardMorph(childNodeIdx);
                using var displayText = GetNodeDisplayText(radEntry);

                var expandedWidth = displayText.OverallWidth + 4;

                var entryType = radEntry.entry.type;
                if (entryType == RadialMenuEntryType.Toggle || entryType == RadialMenuEntryType.Choice)
                {
                    expandedWidth += (int) (width1pad * widthfactor);
                }

                if (width1pad != 0 && widthout1 < expandedWidth + width1pad)
                {
                    widthout1 = expandedWidth + width1pad;
                }

                if (width2pad != 0 && widthout2 < expandedWidth + width2pad)
                {
                    widthout2 = expandedWidth + width2pad;
                }
            }
        }

        // TODO: Only counts visible children, should be reflected in the name
        [TempleDllLocation(0x10139f40)]
        private int RadialMenuCountChildren(int nodeIdx)
        {
            var visibleChildren = 0;

            var directChildrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(nodeIdx);
            for (int i = 0; i < directChildrenCount; i++)
            {
                var childNodeIdx = RadialMenus.RadialMenuGetChild(nodeIdx, i);
                if (RadialMenus.ActiveRadialHasChildrenWithCallback(childNodeIdx))
                {
                    ++visibleChildren;
                }
            }

            return visibleChildren;
        }

        [TempleDllLocation(0x10be6d74)]
        private int dword_10BE6D74; // Flags it seems , 1 and 2 seem to be fading in some alpha value

        // Probably a color value, but only seems to be alpha???
        [TempleDllLocation(0x10be68a4)]
        private uint dword_10BE68A4;

        // Another color value whose alpha is faded in/out
        [TempleDllLocation(0x10be67c0)]
        private uint dword_10BE67C0;

        // TODO Only seems to have writes
        [TempleDllLocation(0x10be6d34)]
        private uint dword_10BE6D34;

        [TempleDllLocation(0x10be67a4)]
        private float dword_10BE67A4;

        [TempleDllLocation(0x102f9550)]
        private int dword_102F9550;

        [TempleDllLocation(0x10be689c)]
        private TimePoint dword_10BE689C;

        [TempleDllLocation(0x10BE6B44)]
        private TimePoint dword_10BE6B44;

        [TempleDllLocation(0x10be67b8)]
        private bool radialMenuIsCombat;

        [TempleDllLocation(0x10BE6228)]
        private TimePoint dword_10BE6228;

        [TempleDllLocation(0x10BE6D44)]
        private int dword_10BE6D44;

        [TempleDllLocation(0x10BE6CCC)]
        private int dword_10BE6CCC;

        [TempleDllLocation(0x1013dba0)]
        public void Render()
        {
            sub_1013C600();
            if (RadialMenus.GetCurrentNode() == -1)
            {
                if ((dword_10BE6D74 & 4) != 0)
                {
                    dword_10BE6D74 = 0;
                    dword_10BE68A4 = 0;
                    dword_10BE67C0 = 0;
                    dword_10BE6D34 = 0;
                    dword_10BE67A4 = 0;
                    dword_102F9550 = -1;
                }
            }
            else
            {
                if ((dword_10BE6D74 & 4) == 0)
                {
                    dword_10BE6D74 |= 5;
                    dword_10BE689C = TimePoint.Now;
                    dword_10BE68A4 = 0;
                }

                radialMenuIsCombat = GameSystems.Combat.IsCombatActive();

                // TODO: It'd make more sense to track for whom the menu was opened rather than use the party leader
                var leader = GameSystems.Party.GetConsciousLeader();
                DrawLineFromMenuToCritter(CurrentMenuCenterOnScreen, leader);
                RadialRender_1013D370(CurrentMenuCenterOnScreen);
                DrawPortrait(CurrentMenuCenterOnScreen, leader);
            }
        }

        [TempleDllLocation(0x1013a230)]
        private void DrawPortrait(Point menuCenter, GameObjectBody critter)
        {
            var portraitId = critter.GetInt32(obj_f.critter_portrait);
            var portraitPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
            using var texture = Tig.Textures.Resolve(portraitPath, false);

            var srcRect = texture.Resource.GetContentRect();

            var args = new Render2dArgs();
            args.flags = Render2dFlag.MASK | Render2dFlag.BUFFERTEXTURE;
            args.srcRect = srcRect;
            args.customTexture = texture.Resource;
            args.maskTexture = _portraitAlphaMask.Resource;
            args.destRect = new Rectangle(
                menuCenter.X - srcRect.Width / 2,
                menuCenter.Y - srcRect.Height / 2,
                srcRect.Width,
                srcRect.Height
            );

            Tig.ShapeRenderer2d.DrawRectangle(ref args);
        }

        private RadialMenuStandardNode GetStandardNodeTypeForIndex(int nodeIndex)
        {
            for (var nodeType = RadialMenuStandardNode.Root; nodeType < RadialMenuStandardNode.SomeCount; nodeType++)
            {
                if (RadialMenus.GetStandardNode(nodeType) == nodeIndex)
                {
                    return nodeType;
                }
            }

            return RadialMenuStandardNode.Root;
        }

        // TODO: This is probably the hovered node index
        [TempleDllLocation(0x102f9554)]
        private int _mouseOverNodeIdx;

        // This encodes the color selection logic
        private PackedLinearColorA GetColor(HourglassState hourglassState, bool combat, bool active, bool inner)
        {
            // This translates essentially to 0, 8, 16
            var remainingTime = (int) (hourglassState + 1);
            if (remainingTime > 2)
            {
                remainingTime = 2;
            }

            var index = (inner ? 1 : 0)
                        | (active ? 2 : 0)
                        | (combat ? 4 : 0)
                        | remainingTime * 8;

            var result = _radMenuColors[index];
            result.A = (byte) (dword_10BE68A4 >> 24);
            return result;
        }

        [TempleDllLocation(0x1013d370)]
        public void RadialRender_1013D370(Point menuPos)
        {
            var currentNodeIndex = RadialMenus.GetCurrentNode();
            var nodeOnPathToCurrentNode = -1;
            float angleOnPathToCurrentNode = 0;

            var visibleChildrenCount = RadialMenuCountChildren(0);
            var visibleChildIndex = 0;
            var sliceAnglePerNode = (2 * MathF.PI) / visibleChildrenCount;

            var childrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(0);
            for (var childIdx = 0; childIdx < childrenCount; childIdx++)
            {
                var nodeIdx = RadialMenus.RadialMenuGetChild(0, childIdx);
                if (!RadialMenus.ActiveRadialHasChildrenWithCallback(nodeIdx))
                {
                    // Skip invisible nodes
                    continue;
                }

                RadialMenus.GetActiveRadMenuNodeRegardMorph(nodeIdx);

                var sliceCenterAngle = visibleChildIndex * sliceAnglePerNode;
                if (currentNodeIndex == nodeIdx ||
                    RadialMenus.RadialMenuNodeContainsChildQuery(nodeIdx, currentNodeIndex))
                {
                    nodeOnPathToCurrentNode = nodeIdx;
                    angleOnPathToCurrentNode = sliceCenterAngle;
                }

                var nodeActive = nodeIdx == _mouseOverNodeIdx || nodeIdx == nodeOnPathToCurrentNode;
                Tig.ShapeRenderer2d.DrawPieSegment(
                    50,
                    menuPos.X,
                    menuPos.Y,
                    sliceCenterAngle,
                    sliceAnglePerNode,
                    25,
                    0,
                    57,
                    0,
                    GetColor(HourglassState.MOVE, radialMenuIsCombat, nodeActive, true),
                    GetColor(HourglassState.MOVE, radialMenuIsCombat, nodeActive, false)
                );

                // Position the node icon appropriately

                var destRect = new Rectangle();
                destRect.X = (int) ((menuPos.X - 16) + MathF.Cos(sliceCenterAngle) * 41.0f);
                destRect.Y = (int) ((menuPos.Y - 16) + MathF.Sin(sliceCenterAngle) * 41.0f);
                destRect.Width = 32;
                destRect.Height = 32;

                // Get the icon used for the root nodes
                var standardNodeType = GetStandardNodeTypeForIndex(nodeIdx);
                if (_standardNodeIcons.TryGetValue(standardNodeType, out var icon))
                {
                    var renderArgs = new Render2dArgs();
                    renderArgs.flags = Render2dFlag.UNK | Render2dFlag.VERTEXCOLORS | Render2dFlag.BUFFERTEXTURE;
                    renderArgs.customTexture = icon.Resource;
                    renderArgs.srcRect = icon.Resource.GetContentRect();
                    renderArgs.destRect = destRect;
                    renderArgs.vertexColors = new[]
                    {
                        // TODO: Change this (cache it, use only alpha)
                        new PackedLinearColorA(dword_10BE68A4 | 0xFFFFFF),
                        new PackedLinearColorA(dword_10BE68A4 | 0xFFFFFF),
                        new PackedLinearColorA(dword_10BE68A4 | 0xFFFFFF),
                        new PackedLinearColorA(dword_10BE68A4 | 0xFFFFFF)
                    };
                    Tig.ShapeRenderer2d.DrawRectangle(ref renderArgs);
                }

                ++visibleChildIndex;
            }

            if (nodeOnPathToCurrentNode != -1)
            {
                if ((dword_10BE6D74 & 8) == 0)
                {
                    dword_10BE6D74 &= ~0x20;
                    dword_10BE6D74 |= 0x18;
                    dword_10BE6B44 = TimePoint.Now;
                    dword_10BE6D44 = -64;
                    dword_10BE6CCC = 0;
                    dword_10BE67C0 = 0;
                    dword_10BE6D34 = 0;
                    dword_10BE67A4 = 0;
                }

                RenderNodeChildren(nodeOnPathToCurrentNode, menuPos.X, menuPos.Y, 1, angleOnPathToCurrentNode,
                    57, 0);
            }
        }

        [TempleDllLocation(0x1013c230)]
        private void RenderNodeChildren(int nodeIdx, int menuPosX, int menuPosY, int depth, float angle, float a6,
            float a7)
        {
            float widthout1 = 0;
            float widthout2 = 0;

            var showChildrenOfNodeIdx = -1;
            var showChildrenAtAngle = angle;

            var visibleChildIdx = 0;
            var parent = nodeIdx;
            var currentNodeIdx = RadialMenus.GetCurrentNode();
            var visibleChildren = RadialMenuCountChildren(nodeIdx);
            int centerVisibleChildNodeIdx = GetCenterVisibleChildNodeIdx(nodeIdx);
            float wfactor = 16.0f / a6;

            var halfNodeWidth = visibleChildren * wfactor * 0.5f;
            var sliceStart = angle - halfNodeWidth;
            var sliceEnd = angle + halfNodeWidth;
            RadialMenuAnglesSthg_10139FE0(ref angle, ref sliceEnd, ref sliceStart);

            var childrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(nodeIdx);
            for (var i = 0; i < childrenCount; i++)
            {
                var childNodeIdx = RadialMenus.RadialMenuGetChild(parent, i);
                if (!RadialMenus.ActiveRadialHasChildrenWithCallback(childNodeIdx))
                {
                    // Skip invisible child nodes
                    continue;
                }

                var activeNode = RadialMenus.GetActiveRadMenuNodeRegardMorph(childNodeIdx);
                var vxx = visibleChildIdx * wfactor + angle - halfNodeWidth + wfactor * 0.5f;
                RadialMenuGetNodeWidth(parent, depth, a6, a7, out widthout1, out widthout2, wfactor);
                D20ActionType actionType = activeNode.entry.d20ActionType;
                var v14 = a6;
                var v37 = a7;

                var hourglassStateNew = HourglassState.MOVE;
                if (actionType != D20ActionType.NONE)
                {
                    int actualArgVal;
                    if (activeNode.entry.type != RadialMenuEntryType.Action)
                    {
                        actualArgVal = activeNode.entry.ArgumentGetter();
                    }
                    else
                    {
                        actualArgVal = 0;
                    }

                    int d20data1 = activeNode.entry.d20ActionData1;
                    var actType = actionType;
                    var leader = GameSystems.Party.GetConsciousLeader();
                    hourglassStateNew = GameSystems.D20.Actions.GetNewHourglassState(
                        leader,
                        actType,
                        d20data1,
                        actualArgVal,
                        activeNode.entry.d20SpellData);
                }

                if (childNodeIdx == currentNodeIdx ||
                    RadialMenus.RadialMenuNodeContainsChildQuery(childNodeIdx, currentNodeIdx))
                {
                    showChildrenOfNodeIdx = childNodeIdx;
                    showChildrenAtAngle = vxx;
                }
                else if (nodeIdx == currentNodeIdx)
                {
                    sub_1013AE90(nodeIdx);
                    if (childNodeIdx == centerVisibleChildNodeIdx)
                    {
                        v37 = a7 - dword_10BE6CCC;
                        v14 = a6 - dword_10BE6D44;
                        widthout1 -= dword_10BE6D44;
                        widthout2 -= dword_10BE6CCC;
                    }
                }

                var isActiveNode = childNodeIdx == _mouseOverNodeIdx
                                   || childNodeIdx == showChildrenOfNodeIdx;

                var outerColor = GetColor(hourglassStateNew, radialMenuIsCombat, isActiveNode, false);
                var innerColor = GetColor(hourglassStateNew, radialMenuIsCombat, isActiveNode, true);

                byte alpha;
                if (isActiveNode || nodeIdx != dword_102F9550)
                {
                    alpha = 208;
                }
                else if (childNodeIdx == centerVisibleChildNodeIdx)
                {
                    alpha = new PackedLinearColorA(dword_10BE67C0).A;
                }
                else
                {
                    var centerVisibleIndex = visibleChildren / 2;
                    // TODO Bit unclear what this does...
                    var visibleIndexOf = FindVisibleIndexOf(nodeIdx, childNodeIdx, centerVisibleChildNodeIdx);
                    float fadeInAmount;
                    if (centerVisibleIndex != 0)
                    {
                        var v48 = 208 + 208 * (centerVisibleIndex - visibleIndexOf) / centerVisibleIndex;
                        fadeInAmount = dword_10BE67A4 * v48;
                    }
                    else
                    {
                        fadeInAmount = dword_10BE67A4 * 208.0f; // 208 is the normal alpha value
                    }

                    alpha = (byte) Math.Min(208, (int) fadeInAmount);
                }

                var innerColorWithAlpha = new PackedLinearColorA(innerColor.R, innerColor.G, innerColor.B, alpha);
                var outerColorWithAlpha = new PackedLinearColorA(outerColor.R, outerColor.G, outerColor.B, alpha);

                Tig.ShapeRenderer2d.DrawPieSegment(
                    9, menuPosX, menuPosY, vxx, wfactor, v14, v37, widthout1, widthout2, innerColorWithAlpha,
                    outerColorWithAlpha
                );
                RenderNodeContent(widthout2, childNodeIdx, menuPosX, menuPosY, vxx, wfactor, v14, v37,
                    widthout1, 17, alpha);
                parent = nodeIdx;
                ++visibleChildIdx;
            }

            if (showChildrenOfNodeIdx != -1)
            {
                RenderNodeChildren(showChildrenOfNodeIdx, menuPosX, menuPosY, depth + 1, showChildrenAtAngle,
                    widthout1, widthout2);
            }
        }

        /// <summary>
        /// This renders the node text and a potential checkbox.
        /// </summary>
        [TempleDllLocation(0x1013a580)]
        public void RenderNodeContent(float outerOffset, int nodeIdx, int menuPosX, int menuPosY, float angleCenter,
            float angleWidth, float innerRadius, float innerOffset, float outerRadius, int seventeen, int alpha)
        {
            var v11 = outerOffset;
            var v47 = false;
            var node = RadialMenus.GetActiveRadMenuNodeRegardMorph(nodeIdx);
            var a7a = innerRadius + 2;

            // TODO Does this mean it's rendering upside down...?
            // Center is between 90° and 270°
            if (angleCenter is > PI_HALF and < MathF.PI + PI_HALF)
            {
                var v13 = -a7a;
                angleCenter -= MathF.PI;
                a7a = -outerRadius;
                innerOffset = -v11;
                outerRadius = v13;
                v47 = true;
            }

            using var displayText = GetNodeDisplayText(node);

            float x;
            var y = innerOffset + menuPosY - displayText.OverallHeight / 2;
            if (v47)
            {
                x = menuPosX + outerRadius - displayText.OverallWidth;
            }
            else
            {
                x = menuPosX + a7a;
            }

            var options = TextRenderOptions.Default
                .WithRotation(angleCenter, new Vector2(menuPosX, menuPosY));
            if (alpha < 255)
            {
                options = options.WithOpacity(alpha / 255f);
            }

            _textEngine.RenderTextLayout(
                x,
                y,
                displayText,
                options
            );

            var nodeType = node.entry.type;
            if (nodeType == RadialMenuEntryType.Toggle || nodeType == RadialMenuEntryType.Choice)
            {
                var a7b = a7a - 2;
                Span<Vertex2d> corners = stackalloc Vertex2d[4];
                // Set common vertex properties
                var color = new PackedLinearColorA(255, 255, 255, (byte) alpha);
                corners[0].diffuse = color;
                corners[0].uv = Vector2.Zero;
                corners[1].diffuse = color;
                corners[1].uv = Vector2.UnitX;
                corners[2].diffuse = color;
                corners[2].uv = Vector2.One;
                corners[3].diffuse = color;
                corners[3].uv = Vector2.UnitY;

                var v22 = angleWidth * 0.25f;
                var a7c = a7b - (a7b * angleWidth / 2.0f);
                if (v47)
                {
                    var v35 = a7c;
                    float v36 = (angleWidth * 0.5f + 1.0f) * v35;
                    var v39 = angleCenter - v22;
                    var v34 = angleCenter + v22;

                    corners[0].pos = new Vector4(
                        menuPosX + MathF.Cos(v34) * v36,
                        menuPosY + MathF.Sin(v34) * v36,
                        0,
                        1
                    );
                    corners[1].pos = new Vector4(
                        menuPosX + MathF.Cos(v34) * v35,
                        menuPosY + MathF.Sin(v34) * v35,
                        0,
                        1
                    );
                    corners[2].pos = new Vector4(
                        menuPosX + MathF.Cos(v39) * v35,
                        menuPosY + MathF.Sin(v39) * v35,
                        0,
                        1
                    );
                    corners[3].pos = new Vector4(
                        menuPosX + MathF.Cos(v39) * v36,
                        menuPosY + MathF.Sin(v39) * v36,
                        0,
                        1
                    );
                }
                else
                {
                    var v24 = angleCenter - v22;
                    float v25 = outerRadius;
                    float v26 = v25;
                    float v27 = v26 - v25 * angleWidth * 0.5f;
                    var v30 = v22 + angleCenter;

                    corners[0].pos = new Vector4(
                        menuPosX + MathF.Cos(v24) * v27,
                        menuPosY + MathF.Sin(v24) * v27,
                        0,
                        1
                    );
                    corners[1].pos = new Vector4(
                        menuPosX + MathF.Cos(v24) * v26,
                        menuPosY + MathF.Sin(v24) * v26,
                        0,
                        1
                    );
                    corners[2].pos = new Vector4(
                        menuPosX + MathF.Cos(v30) * v26,
                        menuPosY + MathF.Sin(v30) * v26,
                        0,
                        1
                    );
                    corners[3].pos = new Vector4(
                        menuPosX + MathF.Cos(v30) * v27,
                        menuPosY + MathF.Sin(v30) * v27,
                        0,
                        1
                    );
                }

                var v41 = node.entry.ArgumentGetter();
                if (v41 != 0)
                {
                    Tig.ShapeRenderer2d.DrawRectangle(corners, _checkboxChecked.Resource);
                }
                else
                {
                    Tig.ShapeRenderer2d.DrawRectangle(corners, _checkboxUnchecked.Resource);
                }
            }
        }

        [TempleDllLocation(0x10139e70)]
        private int FindVisibleIndexOf(int parentNodeIdx, int currentChildNodeIdx, int centerNodeIdx)
        {
            var childrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(parentNodeIdx);

            var visibleChildIdx = 0;
            for (var idx = 0; idx < childrenCount; idx++)
            {
                var childNodeIdx = RadialMenus.RadialMenuGetChild(parentNodeIdx, idx);
                if (childNodeIdx == currentChildNodeIdx)
                {
                    // Find the other node, starting with the current node
                    for (; idx < childrenCount; idx++)
                    {
                        var nodeIdx = RadialMenus.RadialMenuGetChild(parentNodeIdx, idx);
                        if (nodeIdx == centerNodeIdx)
                        {
                            return visibleChildIdx;
                        }

                        if (RadialMenus.ActiveRadialHasChildrenWithCallback(nodeIdx))
                        {
                            visibleChildIdx += 1;
                        }
                    }

                    return -1;
                }
                else if (childNodeIdx == centerNodeIdx)
                {
                    // Find the other node, starting with the current node
                    for (; idx < childrenCount; idx++)
                    {
                        var nodeIdx = RadialMenus.RadialMenuGetChild(parentNodeIdx, idx);
                        if (nodeIdx == currentChildNodeIdx)
                        {
                            return visibleChildIdx;
                        }

                        if (RadialMenus.ActiveRadialHasChildrenWithCallback(nodeIdx))
                        {
                            visibleChildIdx += 1;
                        }
                    }

                    return -1;
                }
            }

            return -1;
        }

        [TempleDllLocation(0x1013ae90)]
        private void sub_1013AE90(int nodeIdx)
        {
            if (dword_102F9550 != nodeIdx)
            {
                dword_102F9550 = nodeIdx;
                dword_10BE6D74 &= ~0x20;
                dword_10BE6D74 |= 0x18;
                dword_10BE6B44 = TimePoint.Now;
                dword_10BE6D44 = -64;
                dword_10BE6CCC = 0;
                dword_10BE67C0 = 0;
                dword_10BE6D34 = 0;
                dword_10BE67A4 = 0;
            }
        }

        /// <summary>
        /// This gets the node index of the child node that is positioned at the center of the given node.
        /// </summary>
        [TempleDllLocation(0x10139f80)]
        private int GetCenterVisibleChildNodeIdx(int parentNodeIdx)
        {
            // This is the index within the visible child nodes that we'd like to retrieve
            var centerVisibleIdx = RadialMenuCountChildren(parentNodeIdx) / 2;
            var childrenCount = RadialMenus.GetRadialActiveMenuNodeChildrenCount(parentNodeIdx);
            var visibleChildIdx = 0;
            for (var i = 0; i < childrenCount; i++)
            {
                var childNodeIdx = RadialMenus.RadialMenuGetChild(parentNodeIdx, i);
                if (RadialMenus.ActiveRadialHasChildrenWithCallback(childNodeIdx))
                {
                    if (visibleChildIdx == centerVisibleIdx)
                    {
                        return childNodeIdx;
                    }

                    visibleChildIdx++;
                }
            }

            return -1;
        }

        [TempleDllLocation(0x1013a2f0)]
        private void DrawLineFromMenuToCritter(Point screenPos, GameObjectBody leader)
        {
            if (leader == null)
            {
                return;
            }

            // TODO If this is the case, the radial menu should actually be bound to the game view it was opened in
            var viewport = GameViews.Primary;
            var mousePosWorld = viewport.ScreenToWorld(screenPos.X, screenPos.Y);
            var leaderPosWorld = leader.GetLocationFull().ToInches3D();

            var color = new PackedLinearColorA(0xFF80FFD2);
            Tig.ShapeRenderer3d.DrawLine(viewport, mousePosWorld, leaderPosWorld, color);
        }

        private static readonly TimeSpan HundredFiftyMs = TimeSpan.FromMilliseconds(150);

        [TempleDllLocation(0x1013c600)]
        public void sub_1013C600()
        {
            if ((dword_10BE6D74 & 1) != 0)
            {
                var elapsed = TimePoint.Now - dword_10BE689C;
                var fadeIn = elapsed / HundredFiftyMs;
                if (fadeIn > 1.0)
                {
                    dword_10BE68A4 = 0xD0000000;
                    dword_10BE6D74 &= ~1;
                }
                else
                {
                    dword_10BE68A4 = (uint) (208 * fadeIn) << 24;
                }
            }
            else if ((dword_10BE6D74 & 2) != 0)
            {
                var elapsed = TimePoint.Now - dword_10BE689C;
                var fadeIn = elapsed / HundredFiftyMs;
                if (fadeIn > 1.0)
                {
                    // TODO: Shouldn't it also set the maximum value here...?
                    dword_10BE6D74 &= ~0x06;
                }
                else
                {
                    dword_10BE68A4 = ((uint) (208 * fadeIn) << 24);
                }
            }

            if ((dword_10BE6D74 & 0x10) != 0)
            {
                var elapsed = TimePoint.Now - dword_10BE6B44;
                if (elapsed <= HundredFiftyMs)
                {
                    dword_10BE67C0 = (uint) (208 * (elapsed / HundredFiftyMs)) << 24;
                    dword_10BE6D44 = (int) (64 - 64 * elapsed / HundredFiftyMs);
                    dword_10BE6CCC = 0;
                }
                else
                {
                    dword_10BE6D74 &= ~0x10;
                    dword_10BE6D74 |= 0x20;
                    dword_10BE6228 = TimePoint.Now;
                    dword_10BE6D34 = 0;
                    dword_10BE67A4 = 0;
                    dword_10BE67C0 = 0xD0000000;
                    dword_10BE6D44 = 0;
                    dword_10BE6CCC = 0;
                }
            }
            else if ((dword_10BE6D74 & 0x20) != 0)
            {
                var elapsed = (TimePoint.Now - dword_10BE6228);
                if (elapsed <= HundredFiftyMs)
                {
                    dword_10BE6D34 = (uint) (208 * elapsed / HundredFiftyMs) << 24;
                    dword_10BE67A4 = (float) (elapsed / HundredFiftyMs);
                }
                else
                {
                    dword_10BE6D74 &= ~0x20;
                    dword_10BE6D34 = 0xD0000000;
                    dword_10BE67A4 = 1.0f;
                }
            }
        }

        [TempleDllLocation(0x10139d60)]
        public void ResizeViewport(Size size)
        {
            Stub.TODO();
        }

        public void Dispose()
        {
            _portraitAlphaMask.Dispose();
            _checkboxUnchecked.Dispose();
            _checkboxChecked.Dispose();
            _standardNodeIcons.Values.DisposeAll();
            _standardNodeIcons.Clear();
        }
    }
}