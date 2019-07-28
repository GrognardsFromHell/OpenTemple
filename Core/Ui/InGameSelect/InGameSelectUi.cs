using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui.InGameSelect.Pickers;
using SpicyTemple.Core.Ui.WidgetDocs;
using Rectangle = System.Drawing.Rectangle;

namespace SpicyTemple.Core.Ui.InGameSelect
{
    public class InGameSelectUi : AbstractUi, IDisposable
    {
        private ResourceRef<IMdfRenderMaterial> quarterArcShaderId;
        private ResourceRef<IMdfRenderMaterial> invalidSelectionShaderId;

        private PackedLinearColorA validConeOutlineRGBA;
        private PackedLinearColorA validConeInsideRGBA;
        private PackedLinearColorA invalidArcOutlineRGBA;
        private PackedLinearColorA invalidArcInsideRGBA;
        private PackedLinearColorA validAreaOutlineRGBA;
        private PackedLinearColorA validAreaInsideRGBA;
        private PackedLinearColorA invalidAreaOutlineRGBA;
        private PackedLinearColorA invalidAreaInsideRGBA;

        [TempleDllLocation(0x102F920C)]
        private int _activePickerIndex = -1; // TODO: Just replace with _activePickers

        private readonly List<PickerState> _activePickers = new List<PickerState>();

        [TempleDllLocation(0x10135A80)]
        public bool IsCurrentPickerTargetInvalid
        {
            get
            {
                if (_activePickers.Count == 0)
                {
                    return false;
                }

                return _activePickers[^1].Behavior.PickerStatusFlags.HasFlag(PickerStatusFlags.Invalid);
            }
        }

        private static readonly Dictionary<UiPickerType, Func<PickerState, PickerBehavior>> PickerFactories =
            new Dictionary<UiPickerType, Func<PickerState, PickerBehavior>>
            {
                {UiPickerType.Single, state => new SingleTargetBehavior(state)},
                {UiPickerType.Multi, state => new MultiTargetBehavior(state)},
                {UiPickerType.Cone, state => new ConeTargetBehavior(state)},
                {UiPickerType.Area, state => new AreaTargetBehavior(state)},
                {UiPickerType.Location, state => new LocationTargetBehavior(state)},
                {UiPickerType.Personal, state => new PersonalTargetBehavior(state)},
                {UiPickerType.InventoryItem, state => new InventoryItemTargetBehavior(state)},
                {UiPickerType.Ray, state => new RayTargetBehavior(state)},
                {UiPickerType.Wall, state => new WallTargetBehavior(state)},
            };

        [TempleDllLocation(0x10138a40)]
        public InGameSelectUi()
        {
            GameSystems.PathXRender.LoadShaders();

            quarterArcShaderId = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/quarter-arc.mdf");
            invalidSelectionShaderId = Tig.MdfFactory.LoadMaterial("art/interface/cursors/InvalidSelection.mdf");

            var intgameRules = Tig.FS.ReadMesFile("rules/intgame_select.mes");

            PackedLinearColorA IntgameGetRGBA(int index)
            {
                if (intgameRules.TryGetValue(index, out var line))
                {
                    var tokenizer = new Tokenizer(line);

                    if (tokenizer.NextToken() && tokenizer.IsNumber)
                    {
                        var red = (byte) tokenizer.TokenInt;
                        if (tokenizer.NextToken() && tokenizer.IsNumber)
                        {
                            var green = (byte) tokenizer.TokenInt;
                            if (tokenizer.NextToken() && tokenizer.IsNumber)
                            {
                                var blue = (byte) tokenizer.TokenInt;
                                if (tokenizer.NextToken() && tokenizer.IsNumber)
                                {
                                    var alpha = (byte) tokenizer.TokenInt;
                                    return new PackedLinearColorA(red, green, blue, alpha);
                                }
                            }
                        }
                    }
                }

                return PackedLinearColorA.White;
            }

            validConeOutlineRGBA = IntgameGetRGBA(0);
            validConeInsideRGBA = IntgameGetRGBA(1);
            invalidArcOutlineRGBA = IntgameGetRGBA(2);
            invalidArcInsideRGBA = IntgameGetRGBA(3);
            validAreaOutlineRGBA = IntgameGetRGBA(10);
            validAreaInsideRGBA = IntgameGetRGBA(11);
            invalidAreaOutlineRGBA = IntgameGetRGBA(12);
            invalidAreaInsideRGBA = IntgameGetRGBA(13);

            _window = new WidgetContainer(Tig.RenderingDevice.GetCamera().ScreenSize.Width, 20);

            InitCastSpellButton();
        }

        [TempleDllLocation(0x10138cb0)]
        [TempleDllLocation(0x10BE60D8)]
        public GameObjectBody Focus { get; set; }

        [TempleDllLocation(0x10135970)]
        public bool IsPicking => _activePickerIndex >= 0;

        [TempleDllLocation(0x101387c0)]
        private void InitCastSpellButton()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10137560)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10137640)]
        public override void Reset()
        {
            base.Reset();
        }

        [TempleDllLocation(0x101375e0)]
        public bool HandleMessage(Message msg)
        {
            // TODO
            return false;
        }


        private WidgetContainer _window;

        [TempleDllLocation(0x10BE60E0)]
        private SortedSet<GameObjectBody> _selection = new SortedSet<GameObjectBody>();

        private ResourceRef<IMdfRenderMaterial> selectionShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverPartyShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverEnemyShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverFriendlyShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseDownShaderId;
        private ResourceRef<IMdfRenderMaterial> selectionOcShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverPartyOcShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverEnemyOcShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverFriendlyOcShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseOverOcShaderId;
        private ResourceRef<IMdfRenderMaterial> mouseDownOcShaderId;

        [TempleDllLocation(0x10139290)]
        public void LoadSelectionShaders()
        {
            FocusClear();

            selectionShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/selection.mdf");
            mouseOverPartyShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseoverParty.mdf");
            mouseOverEnemyShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseoverEnemy.mdf");
            mouseOverFriendlyShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseoverFriend.mdf");
            mouseOverShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseover.mdf");
            mouseDownShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mousedown.mdf");
            selectionOcShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/selection_oc.mdf");
            mouseOverPartyOcShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseoverParty_oc.mdf");
            mouseOverEnemyOcShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseoverEnemy_oc.mdf");
            mouseOverFriendlyOcShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseoverFriend_oc.mdf");
            mouseOverOcShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mouseover_oc.mdf");
            mouseDownOcShaderId = Tig.MdfFactory.LoadMaterial("art/meshes/mousedown_oc.mdf");

            var uiRules = Tig.FS.ReadMesFile("rules/ui.mes");

            var boxSelectColorStr = uiRules[100];
            var parts = boxSelectColorStr.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            _boxSelectColor = new PackedLinearColorA(
                byte.Parse(parts[0]),
                byte.Parse(parts[1]),
                byte.Parse(parts[2]),
                byte.Parse(parts[3])
            );
        }

        [TempleDllLocation(0x10138c70)]
        public void ClearFocusGroup()
        {
            _selection.Clear();
        }

        [TempleDllLocation(0x10138c90)]
        public void AddToFocusGroup(GameObjectBody obj)
        {
            _selection.Add(obj);
        }

        [TempleDllLocation(0x10138cf0)]
        private bool IsInScreenRect(GameObjectBody obj, float minX, float minY, float maxX, float maxY)
        {
            var screenPos = GameSystems.MapObject.GetScreenPosOfObject(obj);
            return screenPos.X - 10.0 <= maxX
                   && screenPos.X + 10.0 >= minX
                   && screenPos.Y - 10.0 <= maxY
                   && screenPos.Y + 10.0 >= minY;
        }

        [TempleDllLocation(0x10139b60)]
        private List<GameObjectBody> FindPartyMembersInRect(float maxX, float maxY, float minX, float minY)
        {
            var tmp = minX;
            if (maxX < minX)
            {
                minX = maxX;
                maxX = tmp;
            }

            tmp = minY;
            if (maxY < minY)
            {
                minY = maxY;
                maxY = tmp;
            }

            var result = new List<GameObjectBody>();
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (IsInScreenRect(partyMember, minX, minY, maxX, maxY))
                {
                    result.Add(partyMember);
                }
            }

            return result;
        }

        [TempleDllLocation(0x10139CE0)]
        public void SelectInRectangle(Rectangle rectangle)
        {
            var partyMembers = FindPartyMembersInRect(
                rectangle.Left, rectangle.Top,
                rectangle.Bottom, rectangle.Right
            );
            foreach (var obj in partyMembers)
            {
                GameSystems.Party.AddToSelection(obj);
            }
        }

        [TempleDllLocation(0x10139400)]
        public void FocusClear()
        {
            _selection.Clear();
            Focus = null;
            uiIntgameBoxSelectOn = false;
        }

        [TempleDllLocation(0x10BE6200)]
        private bool uiIntgameBoxSelectOn;

        [TempleDllLocation(0x11E72E60)]
        private Vector2 uiIntgameBoxSelectBR;

        [TempleDllLocation(0x11E72E70)]
        private Vector2 uiIntgameBoxSelectUL;

        [TempleDllLocation(0x10139c20)]
        public void SetFocusToRect(int x1, int y1, int x2, int y2)
        {
            uiIntgameBoxSelectUL.X = x1;
            uiIntgameBoxSelectUL.Y = y1;
            uiIntgameBoxSelectBR.X = x2;
            uiIntgameBoxSelectBR.Y = y2;
            uiIntgameBoxSelectOn = true;

            foreach (var partyMember in FindPartyMembersInRect(x1, y1, x2, y2))
            {
                _selection.Add(partyMember);
            }
        }

        [TempleDllLocation(0x101357e0)]
        public bool ShowPicker(PickerArgs picker, object callbackArgs)
        {
            // hardcoded tutorials. NICE!
            if (GameSystems.Map.GetCurrentMapId() == 5118)
            {
                if (GameSystems.Script.GetGlobalFlag(6) && picker.spellEnum == 288)
                {
                    GameUiBridge.EnableTutorial();
                    GameUiBridge.ShowTutorialTopic(30);
                    GameSystems.Script.SetGlobalFlag(6, false);
                    GameSystems.Script.SetGlobalFlag(7, true);
                }
                else if (GameSystems.Script.GetGlobalFlag(9))
                {
                    if (picker.spellEnum == 171)
                    {
                        GameUiBridge.EnableTutorial();
                        GameUiBridge.ShowTutorialTopic(36);
                        GameSystems.Script.SetGlobalFlag(9, false);
                    }
                }
            }

            Tig.Mouse.GetState(out var mouseState);
            var activePicker = new PickerState
            {
                Picker = picker,
                MouseX = mouseState.x,
                MouseY = mouseState.y,
                CallbackArgs = callbackArgs
            };
            _activePickers.Add(activePicker);
            _activePickerIndex = _activePickers.Count - 1;

            var pickerType = picker.GetBaseModeTarget();

            activePicker.Behavior = PickerFactories[pickerType](activePicker);

            Tig.Mouse.SetCursorDrawCallback((x, y, _) => activePicker.Behavior.DrawTextAtCursor(x, y));

            return true;
        }

        [TempleDllLocation(0x10BE6220)]
        private TimePoint dword_10BE6220;

        [TempleDllLocation(0x10BE621C)]
        private float flt_10BE621C;

        [TempleDllLocation(0x11E72E7C)]
        private PackedLinearColorA _boxSelectColor;

        [TempleDllLocation(0x10BE6210)]
        private GameObjectBody PreviousRenderFocus;

        [TempleDllLocation(0x10BE6204)]
        private TimePoint PreviousRenderFocusSet;

        [TempleDllLocation(0x10139420)]
        public void RenderMouseoverOrSth()
        {
            if (IsPicking)
            {
                return;
            }

            var now = TimePoint.Now;
            var rotation = now - dword_10BE6220;
            dword_10BE6220 = now;
            var radius = (float) (rotation.TotalMilliseconds * 0.0015707964);
            if (radius > 0 && radius < 2 * MathF.PI)
            {
                flt_10BE621C += radius;
                while (flt_10BE621C > 2 * MathF.PI)
                {
                    flt_10BE621C -= 2 * MathF.PI;
                }
            }

            // Draw the rectangle for mouse-based group selection
            if (uiIntgameBoxSelectOn)
            {
                Tig.ShapeRenderer2d.DrawRectangleOutline(
                    uiIntgameBoxSelectUL,
                    uiIntgameBoxSelectBR,
                    _boxSelectColor
                );
            }

            // Draw selection circles for party members
            foreach (var selected in GameSystems.Party.Selected)
            {
                var location = selected.GetLocationFull();

                // TODO: 0xB0 ends up being a *static* check against sector visibility (extend, end, archway)
                if ((GameSystems.MapFogging.GetFogStatus(location) & 0xB0) != 0)
                {
                    // TODO 45 is total junk, since it's radians..
                    DrawDiscAtObj(selected, selectionOcShaderId.Resource, 45.0f);
                }
                else
                {
                    // TODO 45 is total junk, since it's radians..
                    DrawDiscAtObj(selected, selectionShaderId.Resource, 45.0f);
                }
            }

            RenderFocus();
            RenderFocusList();
        }

        [TempleDllLocation(0x10139420)]
        private void RenderFocus()
        {
            if (Focus == null || Focus.HasFlag(ObjectFlag.DESTROYED))
                return;

            if (!Focus.type.IsCritter()
                && !Focus.type.IsEquipment()
                && Focus.type != ObjectType.container
                && Focus.type != ObjectType.portal
                && Focus.ProtoId != 2064 /* Guestbook */)
            {
                // TODO: What about scenery with teleport target???
                return;
            }

            var loc = Focus.GetLocationFull();
            if ((GameSystems.MapFogging.GetFogStatus(loc) & 1) == 0)
            {
                // Skip unexplored tiles (?)
                return;
            }

            if (!_selection.Contains(Focus))
            {
                if ((GameSystems.MapFogging.GetFogStatus(loc) & 0xB0) != 0)
                {
                    if (Focus.type.IsCritter()
                        && GameSystems.Critter.IsDeadNullDestroyed(Focus)
                        && Focus.GetInt32(obj_f.critter_inventory_num) !=
                        0 // TODO The unoccluded version uses different logic here
                        || Focus.type.IsEquipment()
                        || Focus.type == ObjectType.container)
                    {
                        DrawDiscAtObj(Focus, mouseOverOcShaderId.Resource, flt_10BE621C);
                    }
                    else if (GameSystems.Party.IsInParty(Focus))
                    {
                        DrawDiscAtObj(Focus, mouseOverPartyOcShaderId.Resource, flt_10BE621C);
                    }
                    else if (Focus.type.IsCritter() && GameSystems.Combat.IsCombatModeActive(Focus))
                    {
                        DrawDiscAtObj(Focus, mouseOverEnemyOcShaderId.Resource, flt_10BE621C);
                    }
                    else if (Focus.type != ObjectType.portal &&
                             (!Focus.type.IsCritter() || !GameSystems.Critter.IsDeadNullDestroyed(Focus)))
                    {
                        DrawDiscAtObj(Focus, mouseOverFriendlyOcShaderId.Resource, flt_10BE621C);
                    }
                }
                else
                {
                    // The proto num is for the Guest Book (PartyPool in inn)
                    if (Focus.type.IsCritter()
                        && GameSystems.Critter.IsDeadNullDestroyed(Focus)
                        && GameSystems.Critter.IsLootableCorpse(Focus)
                        || Focus.type.IsEquipment()
                        || Focus.type == ObjectType.container
                        || Focus.ProtoId == 2064 /* Guestbook */)
                    {
                        RenderOutline(Focus, mouseOverShaderId);
                    }
                    else if (GameSystems.Party.IsInParty(Focus))
                    {
                        DrawDiscAtObj(Focus, mouseOverPartyShaderId.Resource, flt_10BE621C);
                    }
                    else if (Focus.type.IsCritter() && GameSystems.Combat.IsCombatModeActive(Focus))
                    {
                        DrawDiscAtObj(Focus, mouseOverEnemyShaderId.Resource, flt_10BE621C);
                    }
                    else if (Focus.type != ObjectType.portal &&
                             (!Focus.type.IsCritter() || !GameSystems.Critter.IsDeadNullDestroyed(Focus)))
                    {
                        DrawDiscAtObj(Focus, mouseOverFriendlyShaderId.Resource, flt_10BE621C);
                    }
                }
            }

            if (Focus != PreviousRenderFocus || PreviousRenderFocusSet == default)
            {
                PreviousRenderFocus = Focus;
                PreviousRenderFocusSet = TimePoint.Now;
            }

            // Render an object tooltip when the mouse hovered long enough over the focus, or
            // immediately when we are in combat.
            if (TimePoint.Now - PreviousRenderFocusSet > UiSystems.Tooltip.TooltipDelay
                || GameSystems.Combat.IsCombatModeActive(GameSystems.Party.GetLeader()))
            {
                if (UiSystems.Tooltip.TooltipsEnabled)
                {
                    RenderTooltip(Focus);
                }
            }
        }

        [TempleDllLocation(0x10023ec0)]
        private void RenderOutline(GameObjectBody obj, ResourceRef<IMdfRenderMaterial> resourceRef)
        {
            Globals.GameLoop.GameRenderer.GetMapObjectRenderer().RenderObjectHighlight(obj, resourceRef);
        }

        [TempleDllLocation(0x10138e20)]
        private void RenderTooltip(GameObjectBody obj)
        {
            var tooltipStyle = UiSystems.Tooltip.GetStyle(0);

            var style = tooltipStyle.TextStyle.Copy();
            style.additionalTextColors = new[]
            {
                new ColorRect(PackedLinearColorA.White),
                new ColorRect(new PackedLinearColorA(0xFF3333FF))
            };

            Tig.Fonts.PushFont(tooltipStyle.Font);
            if (obj.IsCritter())
            {
                var currentHp = GameSystems.Stat.StatLevelGet(obj, Stat.hp_current);
                var maxHp = GameSystems.Stat.StatLevelGet(obj, Stat.hp_max);

                if (obj.IsPC())
                {
                    if (currentHp < maxHp)
                    {
                        style.additionalTextColors[0] = new ColorRect(new PackedLinearColorA(0xFFFF0000));
                    }
                }
                else if (GameSystems.Critter.IsDeadNullDestroyed(obj) || currentHp <= 0)
                {
                    style.additionalTextColors[0] = new ColorRect(new PackedLinearColorA(0xFF7F7F7F));
                }
                else
                {
                    var injuryLevel = UiSystems.Tooltip.GetInjuryLevel(obj);
                    style.additionalTextColors[0] = new ColorRect(UiSystems.Tooltip.GetInjuryLevelColor(injuryLevel));
                }
            }

            var leader = GameSystems.Party.GetConsciousLeader();
            var tooltipText = UiSystems.Tooltip.GetObjectDescription(obj, leader);

            if (tooltipText.Length > 0)
            {
                var metrics = Tig.Fonts.MeasureTextSize(tooltipText, style);

                var objRect = GameSystems.MapObject.GetObjectRect(obj, 0);
                var extents = new Rectangle(
                    objRect.X + (objRect.Width - metrics.Width) / 2,
                    objRect.Y - metrics.Height,
                    metrics.Width,
                    metrics.Height
                );
                UiSystems.Tooltip.ClampTooltipToScreen(ref extents);

                Tig.Fonts.RenderText(tooltipText, extents, style);
            }

            Tig.Fonts.PopFont();
        }

        private void RenderFocusList()
        {
            foreach (var obj in _selection)
            {
                var loc = obj.GetLocationFull();
                if ((GameSystems.MapFogging.GetFogStatus(loc) & 0xB0) != 0)
                {
                    DrawDiscAtObj(obj, mouseDownOcShaderId.Resource, flt_10BE621C);
                }
                else
                {
                    DrawDiscAtObj(obj, mouseDownShaderId.Resource, flt_10BE621C);
                }
            }
        }

        [TempleDllLocation(0x10138c00)]
        private void DrawDiscAtObj(GameObjectBody obj, IMdfRenderMaterial material, float rotation)
        {
            var location = obj.GetLocationFull().ToInches3D();
            var radius = obj.GetRadius();
            Tig.ShapeRenderer3d.DrawDisc(location, rotation, radius, material);
        }

        [TempleDllLocation(0x10112f30)]
        public void RenderMovementTargets()
        {
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (GameSystems.Anim.IsRunningGoal(partyMember, AnimGoalType.move_to_tile, out var slotId))
                {
                    var slot = GameSystems.Anim.GetSlot(slotId);
                    Trace.Assert(slot != null);

                    GameSystems.PathXRender.RenderMovementTarget(slot.goals[1].targetTile.location, partyMember);
                }
            }
        }

        [TempleDllLocation(0x10BE61E8)]
        private ObjectId _savedFocusId;

        [TempleDllLocation(0x10138d80)]
        public void SaveFocus()
        {
            _savedFocusId = Focus?.id ?? ObjectId.CreateNull();
            _selection.Clear();
            Focus = null;
        }

        [TempleDllLocation(0x10138de0)]
        public void RestoreFocus()
        {
            Focus = GameSystems.Object.GetObject(_savedFocusId);
            _savedFocusId = ObjectId.CreateNull();
        }

        [TempleDllLocation(0x10137680)]
        public void FreeCurrentPicker()
        {
            if (_activePickers.Count == 0)
            {
                return;
            }

            _activePickers.RemoveAt(_activePickerIndex);
            _activePickerIndex--;

            HideConfirmSelectionButton();

            Tig.Mouse.SetCursorDrawCallback(null);
        }

        /// <summary>
        /// Used to show the button that confirms the selection without selecting all possible targets.
        /// </summary>
        [TempleDllLocation(0x10135b30)]
        public void ShowConfirmSelectionButton(GameObjectBody caster)
        {
            throw new NotImplementedException();
        }

        public void HideConfirmSelectionButton()
        {
            // v7 = uiIntgameSelectMainId;
            // WidgetSetHidden(uiIntgameSelectMainId, 1);
            throw new NotImplementedException();
        }

    }

    internal class PickerState
    {
        public PickerArgs Picker;

        public int MouseX;

        public int MouseY;

        public object CallbackArgs;

        public int tgtIdx;

        public bool cursorStackCount_Maybe;

        public PickerBehavior Behavior { get; set; }

        public GameObjectBody Target { get; set; }

        public GameRaycastFlags GetFlagsFromExclusions()
        {
            GameRaycastFlags result = GameRaycastFlags.HITTEST_3D;
            if (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LMENU) || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RMENU))
            {
                result = GameRaycastFlags.HITTEST_SEL_CIRCLE;
            }

            if (Picker.excFlags.HasFlag(UiPickerIncFlags.UIPI_NonCritter))
            {
                result |= GameRaycastFlags.ExcludeContainers | GameRaycastFlags.ExcludePortals |
                          GameRaycastFlags.ExcludeScenery;
            }

            if (Picker.excFlags.HasFlag(UiPickerIncFlags.UIPI_Dead))
            {
                result |= GameRaycastFlags.ExcludeDead;
            }

            if (Picker.excFlags.HasFlag(UiPickerIncFlags.UIPI_Unconscious))
            {
                result |= GameRaycastFlags.ExcludeUnconscious;
            }

            return result;
        }
    }
}