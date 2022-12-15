using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.InGameSelect.Pickers;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;
using Rectangle = System.Drawing.Rectangle;

namespace OpenTemple.Core.Ui.InGameSelect;

public class InGameSelectUi : IResetAwareSystem, IDisposable
{
    public ComputedStyles PickerTooltipStyle { get; }
    public ComputedStyles PickerTargetLabelStyle { get; }

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

    [TempleDllLocation(0x10BD2C50)]
    private ResourceRef<IMdfRenderMaterial> _spellPointerMaterial;

    public IFlowContentHost TextHost { get; } = new FlowContentHost();

    [TempleDllLocation(0x10BD2CD8)]
    private readonly Dictionary<GameObject, string> intgameselTexts = new();

    [TempleDllLocation(0x102F920C)]
    private int _activePickerIndex = -1; // TODO: Just replace with _activePickers

    private PickerState? ActivePicker
    {
        get
        {
            if (_activePickerIndex >= 0 && _activePickerIndex < _activePickers.Count)
            {
                return _activePickers[_activePickerIndex];
            }
            else
            {
                return null;
            }
        }
    }

    private readonly List<PickerState> _activePickers = new();

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
        new()
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
        _playerSpellPointerRenderer = new PlayerSpellPointerRenderer(Tig.RenderingDevice);
        _spellPointerRenderer = new SpellPointerRenderer(Tig.RenderingDevice);
        _pickerCircleRenderer = new PickerCircleRenderer(Tig.RenderingDevice);
        _pickerAreaRenderer = new PickerAreaRenderer(Tig.RenderingDevice);
        _coneRenderer = new ConeRenderer(Tig.RenderingDevice);
        _rectangleRenderer = new RectangleRenderer(Tig.RenderingDevice);
        GameSystems.PathXRender.LoadShaders();

        PickerTooltipStyle = Globals.UiStyles.GetComputed("picker-tooltip");
        PickerTargetLabelStyle = Globals.UiStyles.GetComputed("picker-target-label");

        quarterArcShaderId = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/quarter-arc.mdf");
        invalidSelectionShaderId = Tig.MdfFactory.LoadMaterial("art/interface/cursors/InvalidSelection.mdf");
        _spellPointerMaterial =
            Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/spell_player-pointer.mdf");

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

        InitConfirmSelectionButton();
    }

    [TempleDllLocation(0x10138cb0)]
    [TempleDllLocation(0x10BE60D8)]
    public GameObject Focus { get; set; }

    [TempleDllLocation(0x10135970)]
    public bool IsPicking => _activePickerIndex >= 0;

    [TempleDllLocation(0x101387c0)]
    private void InitConfirmSelectionButton()
    {
        _confirmSelectionButtonContainer = new WidgetContainer(0, 0, 30, 30);
        _confirmSelectionButtonContainer.ZIndex = 999999;

        var confirmSelectionButton = new WidgetButton(new Rectangle(0, 0, 30, 30));
        confirmSelectionButton.SetStyle(new WidgetButtonStyle
        {
            NormalImagePath = "art/interface/radial_menu/Cast_Spell_Icon.tga",
            HoverImagePath = "art/interface/radial_menu/Cast_Spell_Icon_Hovered.tga",
            PressedImagePath = "art/interface/radial_menu/Cast_Spell_Icon_Clicked.tga"
        }.UseDefaultSounds());
        _confirmSelectionButtonContainer.Add(confirmSelectionButton);
    }

    [TempleDllLocation(0x10137560)]
    public void Dispose()
    {
        _pickerCircleRenderer.Dispose();
    }

    [TempleDllLocation(0x10137640)]
    public void Reset()
    {
        intgameselTexts.Clear();

        while (_activePickerIndex > -1)
        {
            // TODO: Vanilla just cleared the result list and never called the cancel callback here. Does this cause issues when resetting?
            _activePickers[_activePickerIndex].Behavior.CancelPicker();
            _activePickerIndex--;
        }

        _activePickers.Clear();
    }

    [TempleDllLocation(0x10BE60E0)]
    private readonly List<GameObject> _selection = new();

    private readonly PlayerSpellPointerRenderer _playerSpellPointerRenderer;
    private readonly SpellPointerRenderer _spellPointerRenderer;
    private readonly PickerCircleRenderer _pickerCircleRenderer;
    private readonly PickerAreaRenderer _pickerAreaRenderer;
    private readonly ConeRenderer _coneRenderer;
    private readonly RectangleRenderer _rectangleRenderer;

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

    /// <summary>
    /// This button is shown if a caster can select multiple targets, and the spell allows to choose fewer
    /// targets than the theoretical maximum.
    /// </summary>
    private WidgetContainer _confirmSelectionButtonContainer;

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
    public void AddToFocusGroup(GameObject obj)
    {
        if (!_selection.Contains(obj))
        {
            _selection.Add(obj);
        }
    }

    [TempleDllLocation(0x10138cf0)]
    private bool IsInScreenRect(IGameViewport viewport, GameObject obj, RectangleF screenRect)
    {
        var screenPos = GameSystems.MapObject.GetScreenPosOfObject(viewport, obj);
        return screenPos.X - 10.0f <= screenRect.Right
               && screenPos.X + 10.0f >= screenRect.Left
               && screenPos.Y - 10.0f <= screenRect.Bottom
               && screenPos.Y + 10.0f >= screenRect.Top;
    }

    [TempleDllLocation(0x10139b60)]
    private List<GameObject> FindPartyMembersInRect(IGameViewport viewport, RectangleF screenRect)
    {
        var result = new List<GameObject>();
        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (IsInScreenRect(viewport, partyMember, screenRect))
            {
                result.Add(partyMember);
            }
        }

        return result;
    }

    [TempleDllLocation(0x10139CE0)]
    public void SelectInRectangle(IGameViewport viewport, RectangleF rectangle)
    {
        var partyMembers = FindPartyMembersInRect(viewport, rectangle);
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
    public void SetFocusToRect(IGameViewport viewport, float x1, float y1, float x2, float y2)
    {
        if (x1 > x2)
        {
            (x2, x1) = (x1, x2);
        }

        if (y1 > y2)
        {
            (y2, y1) = (y1, y2);
        }

        var screenRect = RectangleF.FromLTRB(x1, y1, x2, y2);

        uiIntgameBoxSelectUL.X = x1;
        uiIntgameBoxSelectUL.Y = y1;
        uiIntgameBoxSelectBR.X = x2;
        uiIntgameBoxSelectBR.Y = y2;
        uiIntgameBoxSelectOn = true;

        foreach (var partyMember in FindPartyMembersInRect(viewport, screenRect))
        {
            if (!_selection.Contains(partyMember))
            {
                _selection.Add(partyMember);
            }
        }
    }

    [TempleDllLocation(0x101357e0)]
    public bool ShowPicker(PickerArgs picker, object callbackArgs)
    {
        // hardcoded tutorials. NICE!
        if (GameSystems.Map.GetCurrentMapId() == 5118)
        {
            if (GameSystems.Script.GetGlobalFlag(6) && picker.spellEnum == WellKnownSpells.MagicMissile)
            {
                GameUiBridge.EnableTutorial();
                GameUiBridge.ShowTutorialTopic(TutorialTopic.CastSpells);
                GameSystems.Script.SetGlobalFlag(6, false);
                GameSystems.Script.SetGlobalFlag(7, true);
            }
            else if (GameSystems.Script.GetGlobalFlag(9))
            {
                if (picker.spellEnum == WellKnownSpells.Fireball)
                {
                    GameUiBridge.EnableTutorial();
                    GameUiBridge.ShowTutorialTopic(TutorialTopic.WandFire);
                    GameSystems.Script.SetGlobalFlag(9, false);
                }
            }
        }

        var mousePos = Globals.UiManager.MousePos;
        var activePicker = new PickerState
        {
            Picker = picker,
            MouseX = mousePos.X,
            MouseY = mousePos.Y,
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
    private GameObject PreviousRenderFocus;

    [TempleDllLocation(0x10BE6204)]
    private TimePoint PreviousRenderFocusSet;

    [TempleDllLocation(0x10139420)]
    public void RenderMouseoverOrSth(IGameViewport viewport)
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
                DrawDiscAtObj(viewport, selected, selectionOcShaderId.Resource, 45.0f);
            }
            else
            {
                // TODO 45 is total junk, since it's radians..
                DrawDiscAtObj(viewport, selected, selectionShaderId.Resource, 45.0f);
            }
        }

        RenderFocus(viewport);
        RenderFocusList(viewport);
    }

    [TempleDllLocation(0x10139420)]
    private void RenderFocus(IGameViewport viewport)
    {
        if (Focus == null || Focus.HasFlag(ObjectFlag.DESTROYED))
            return;

        if (!Focus.type.IsCritter()
            && !Focus.type.IsEquipment()
            && Focus.type != ObjectType.container
            && Focus.type != ObjectType.portal
            && Focus.ProtoId != WellKnownProtos.GuestBook)
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
                    DrawDiscAtObj(viewport, Focus, mouseOverOcShaderId.Resource, flt_10BE621C);
                }
                else if (GameSystems.Party.IsInParty(Focus))
                {
                    DrawDiscAtObj(viewport, Focus, mouseOverPartyOcShaderId.Resource, flt_10BE621C);
                }
                else if (Focus.type.IsCritter() && GameSystems.Combat.IsCombatModeActive(Focus))
                {
                    DrawDiscAtObj(viewport, Focus, mouseOverEnemyOcShaderId.Resource, flt_10BE621C);
                }
                else if (Focus.type != ObjectType.portal &&
                         (!Focus.type.IsCritter() || !GameSystems.Critter.IsDeadNullDestroyed(Focus)))
                {
                    DrawDiscAtObj(viewport, Focus, mouseOverFriendlyOcShaderId.Resource, flt_10BE621C);
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
                    || Focus.ProtoId == WellKnownProtos.GuestBook)
                {
                    RenderOutline(viewport, Focus, mouseOverShaderId);
                }
                else if (GameSystems.Party.IsInParty(Focus))
                {
                    DrawDiscAtObj(viewport, Focus, mouseOverPartyShaderId.Resource, flt_10BE621C);
                }
                else if (Focus.type.IsCritter() && GameSystems.Combat.IsCombatModeActive(Focus))
                {
                    DrawDiscAtObj(viewport, Focus, mouseOverEnemyShaderId.Resource, flt_10BE621C);
                }
                else if (Focus.type != ObjectType.portal &&
                         (!Focus.type.IsCritter() || !GameSystems.Critter.IsDeadNullDestroyed(Focus)))
                {
                    DrawDiscAtObj(viewport, Focus, mouseOverFriendlyShaderId.Resource, flt_10BE621C);
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
                RenderTooltip(viewport, Focus);
            }
        }
    }

    [TempleDllLocation(0x10023ec0)]
    private void RenderOutline(IGameViewport viewport, GameObject obj, ResourceRef<IMdfRenderMaterial> resourceRef)
    {
        UiSystems.GameView.GetMapObjectRenderer().RenderObjectHighlight(viewport, obj, resourceRef);
    }

    [TempleDllLocation(0x10138e20)]
    private void RenderTooltip(IGameViewport viewport, GameObject obj)
    {
        var leader = GameSystems.Party.GetConsciousLeader();
        UiSystems.Tooltip.RenderObjectTooltip(viewport, obj, leader);
    }

    private void RenderFocusList(IGameViewport viewport)
    {
        foreach (var obj in _selection)
        {
            var loc = obj.GetLocationFull();
            if ((GameSystems.MapFogging.GetFogStatus(loc) & 0xB0) != 0)
            {
                DrawDiscAtObj(viewport, obj, mouseDownOcShaderId.Resource, flt_10BE621C);
            }
            else
            {
                DrawDiscAtObj(viewport, obj, mouseDownShaderId.Resource, flt_10BE621C);
            }
        }
    }

    [TempleDllLocation(0x10138c00)]
    private void DrawDiscAtObj(IGameViewport viewport, GameObject obj, IMdfRenderMaterial material, float rotation)
    {
        var location = obj.GetLocationFull().ToInches3D();
        var radius = obj.GetRadius();
        Tig.ShapeRenderer3d.DrawDisc(viewport, location, rotation, radius, material);
    }

    [TempleDllLocation(0x10112f30)]
    public void RenderMovementTargets(IGameViewport viewport)
    {
        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (GameSystems.Anim.IsRunningGoal(partyMember, AnimGoalType.move_to_tile, out var slotId,
                    out var goalIndex))
            {
                var slot = GameSystems.Anim.GetSlot(slotId);
                Trace.Assert(slot != null);

                GameSystems.PathXRender.RenderMovementTarget(viewport, slot.goals[goalIndex].targetTile.location,
                    partyMember);
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
    public void ShowConfirmSelectionButton(GameObject caster)
    {
        if (caster != null && GameSystems.Party.IsInParty(caster))
        {
            if (!UiSystems.Party.TryGetPartyMemberRect(caster, out var rectangle))
            {
                Globals.UiManager.RemoveWindow(_confirmSelectionButtonContainer);
                return;
            }

            // Position the confirm button on top of the casting party member's portrait
            var buttonWidth = _confirmSelectionButtonContainer.Width;
            // Center the button horizontally with respect to the portrait
            var x = rectangle.X + (rectangle.Width - buttonWidth) / 2;
            var y = rectangle.Y - 12; // TODO: I believe this rectangle might differ from vanilla and this height adjustment needs to be changed
            _confirmSelectionButtonContainer.SetPos(x, y);
            Globals.UiManager.AddWindow(_confirmSelectionButtonContainer);
            return;
        }

        Globals.UiManager.RemoveWindow(_confirmSelectionButtonContainer);
    }

    public void HideConfirmSelectionButton()
    {
        Globals.UiManager.RemoveWindow(_confirmSelectionButtonContainer);
    }

    [TempleDllLocation(0x101350f0)]
    public void RenderPickers(IGameViewport viewport)
    {
        using var perfGroup = Tig.RenderingDevice.CreatePerfGroup("Pickers");

        var drawSpellPlayerPointer = true;
        var tgtCount = 0;

        var pick = ActivePicker;

        // Get the picker and the originator
        if (pick == null)
        {
            RenderTargetNumberLabels(viewport);
            return;
        }

        var pickerStatusFlags = pick.Behavior.PickerStatusFlags;

        var originator = pick.Picker.caster;
        if (originator == null)
        {
            originator = GameSystems.Party.GetConsciousLeader();
            if (originator == null)
                return;
        }

        var tgt = pick.Target;
        if (tgt != null)
        {
            // renders the circle for the current hovered target (using an appropriate shader based on ok/not ok selection)
            if ((pickerStatusFlags & PickerStatusFlags.Invalid) != 0)
                DrawCircleInvalidTarget(viewport, tgt, originator, pick.Picker.spellEnum);
            else
                DrawCircleValidTarget(viewport, tgt, originator, pick.Picker.spellEnum);
        }

        // Draw rotating circles for selected targets
        if (pick.Picker.result.HasSingleResult)
        {
            var handle = pick.Picker.result.handle;
            if (pick.Picker.result.handle == originator)
            {
                drawSpellPlayerPointer = false;
            }

            if (pick.Picker.IsBaseModeTarget(UiPickerType.Multi))
            {
                DrawCircleValidTarget(viewport, handle, originator, pick.Picker.spellEnum);
                AddTargetNumberLabel(handle, 1);
            }
        }

        if (pick.Picker.result.HasMultipleResults)
        {
            foreach (var handle in pick.Picker.result.objList)
            {
                if (handle == originator)
                    drawSpellPlayerPointer = false;

                var handleObj = handle;
                var fogFlags = GameSystems.MapFogging.GetFogStatus(handleObj.GetLocationFull());
                var isExplored = (fogFlags & 1) != 0;

                if (Globals.Config.laxRules && Globals.Config.ShowTargetingCirclesInFogOfWar ||
                    !GameSystems.Critter.IsConcealed(handle) && isExplored)
                {
                    // fixed rendering for hidden critters
                    DrawCircleValidTarget(viewport, handle, originator, pick.Picker.spellEnum);
                    AddTargetNumberLabel(handle, ++tgtCount);
                }
            }
        }

        // Draw the Spell Player Pointer
        if (drawSpellPlayerPointer)
        {
            if (tgt != null)
            {
                if (tgt != originator)
                {
                    var tgtLoc = tgt.GetLocationFull();
                    DrawPlayerSpellPointer(viewport, originator, tgtLoc);
                }
            }
            else // draw the picker arrow from the originator to the mouse position
            {
                var tgtLoc = viewport.ScreenToTile(pick.MouseX, pick.MouseY);
                DrawPlayerSpellPointer(viewport, originator, tgtLoc);
            }
        }

        var origObj = originator;
        var originLoc = origObj.GetLocationFull();
        var originRadius = originator.GetRadius();

        tgt = pick.Target; //just in case it got updated
        var tgtObj = tgt;

        // Area targeting
        if (pick.Picker.IsBaseModeTarget(UiPickerType.Area))
        {
            LocAndOffsets tgtLoc;
            if (tgt != null)
            {
                tgtLoc = tgtObj.GetLocationFull();
            }
            else
            {
                tgtLoc = viewport.ScreenToTile(pick.MouseX, pick.MouseY);
            }

            var orgAbs = originLoc.ToInches2D();
            var orgAbsX = orgAbs.X;
            var orgAbsY = orgAbs.Y;

            var tgtAbs = tgtLoc.ToInches2D();
            var tgtAbsX = tgtAbs.X;
            var tgtAbsY = tgtAbs.Y;

            var areaRadiusInch = locXY.INCH_PER_FEET * pick.Picker.radiusTarget;

            // Draw the big AoE circle
            DrawCircleAoE(viewport, tgtLoc, 1.0f, areaRadiusInch, pick.Picker.spellEnum);


            // Draw Spell Effect pointer (points from AoE to caster)
            var spellEffectPointerSize = areaRadiusInch / 80.0f * 38.885002f;
            if (spellEffectPointerSize <= 135.744f)
            {
                if (spellEffectPointerSize < 11.312f)
                    spellEffectPointerSize = 11.312f;
            }
            else
            {
                spellEffectPointerSize = 135.744f;
            }

            if (originRadius * 1.5f + areaRadiusInch + spellEffectPointerSize < tgtLoc.DistanceTo(originLoc))
            {
                DrawSpellEffectPointer(viewport, tgtLoc, originLoc, areaRadiusInch);
            }
        }

        else if (pick.Picker.IsBaseModeTarget(UiPickerType.Personal))
        {
            if (tgt != null && (pick.Picker.flagsTarget & UiPickerFlagsTarget.Radius) != 0 && tgt == originator)
            {
                DrawCircleAoE(viewport, originLoc, 1.0f, locXY.INCH_PER_FEET * pick.Picker.radiusTarget,
                    pick.Picker.spellEnum);
            }
        }

        else if (pick.Picker.IsBaseModeTarget(UiPickerType.Cone))
        {
            LocAndOffsets tgtLoc;
            var degreesTarget = pick.Picker.degreesTarget;
            if ((pick.Picker.flagsTarget & UiPickerFlagsTarget.Degrees) == 0)
            {
                degreesTarget = 60.0f;
            }


            var coneOrigin = originLoc;
            if (pick.Picker.IsModeTargetFlagSet(UiPickerType.PickOrigin))
            {
                coneOrigin = viewport.ScreenToTile(pick.MouseX, pick.MouseY);
                var dir = Vector2.Normalize(coneOrigin.ToInches2D() - originLoc.ToInches2D());

                LocAndOffsets newTgtLoc = coneOrigin;
                newTgtLoc.off_x += dir.X * 4000;
                newTgtLoc.off_y += dir.Y * 4000;
                newTgtLoc.Regularize();
                tgtLoc = newTgtLoc;
            }
            else
            {
                // normal cone emanating from caster
                if (tgt != null)
                {
                    tgtLoc = tgtObj.GetLocationFull();
                }
                else
                {
                    tgtLoc = viewport.ScreenToTile(pick.MouseX, pick.MouseY);
                }
            }

            if ((pick.Picker.flagsTarget & UiPickerFlagsTarget.FixedRadius) != 0)
            {
                tgtLoc = LocAndOffsets.FromInches(coneOrigin.ToInches3D()
                    .AtFixedDistanceTo(tgtLoc.ToInches3D(), pick.Picker.radiusTarget * locXY.INCH_PER_FEET));
            }

            DrawConeAoE(viewport, coneOrigin, tgtLoc, degreesTarget, pick.Picker.spellEnum);
        }

        else if (pick.Picker.IsBaseModeTarget(UiPickerType.Ray))
        {
            if ((pick.Picker.flagsTarget & UiPickerFlagsTarget.Range) != 0)
            {
                var tgtLoc = viewport.ScreenToTile(pick.MouseX, pick.MouseY);

                var rayWidth = pick.Picker.radiusTarget * locXY.INCH_PER_FEET / 2.0f;
                var rayLength = originRadius + pick.Picker.trimmedRangeInches;

                DrawRectangleAoE(viewport, originLoc, tgtLoc, rayWidth, rayLength, rayLength, pick.Picker.spellEnum);
            }
        }

        else if (pick.Picker.IsBaseModeTarget(UiPickerType.Wall) &&
                 pick.Behavior is WallTargetBehavior wallBehavior)
        {
            if ((pick.Picker.flagsTarget & UiPickerFlagsTarget.Range) != 0)
            {
                var tgtLoc = viewport.ScreenToTile(pick.MouseX, pick.MouseY);

                if (wallBehavior.WallState == WallState.EndPoint)
                {
                    var rayWidth = pick.Picker.radiusTarget * locXY.INCH_PER_FEET / 2.0f;
                    var rayLength = originRadius + pick.Picker.trimmedRangeInches;

                    var wallStart = pick.Picker.result.location;

                    DrawRectangleAoE(viewport, wallStart, tgtLoc, rayWidth, rayLength, rayLength, pick.Picker.spellEnum);
                }
            }
        }

        RenderTargetNumberLabels(viewport);
    }

    private void DrawRectangleAoE(IGameViewport viewport, LocAndOffsets originLoc, LocAndOffsets tgtLoc, float rayWidth, float minRange,
        float maxRange, int spellEnum)
    {
        using var materialInside = GetPickerMaterial(spellEnum, 0, false);
        using var materialOutside = GetPickerMaterial(spellEnum, 1, false);

        _rectangleRenderer.Render(
            viewport,
            originLoc.ToInches3D(),
            tgtLoc.ToInches3D(),
            rayWidth,
            minRange,
            maxRange,
            materialInside.Resource,
            materialOutside.Resource
        );
    }

    private void DrawConeAoE(IGameViewport viewport, LocAndOffsets originLoc, LocAndOffsets tgtLoc, float angularWidthDegrees,
        int spellEnum)
    {
        using var materialInside = GetPickerMaterial(spellEnum, 0, false);
        using var materialOutside = GetPickerMaterial(spellEnum, 1, false);

        _coneRenderer.Render(viewport, originLoc, tgtLoc, angularWidthDegrees, materialInside.Resource,
            materialOutside.Resource);
    }

    private void DrawSpellEffectPointer(IGameViewport viewport, LocAndOffsets spellAoECenter, LocAndOffsets pointedToLoc,
        float aoeRadiusInch)
    {
        _spellPointerRenderer.Render(
            viewport,
            spellAoECenter.ToInches3D(),
            pointedToLoc.ToInches3D(),
            aoeRadiusInch
        );
    }

    private void DrawCircleAoE(IGameViewport viewport, LocAndOffsets originLoc, float elevation, float radius, int spellEnum)
    {
        var centerPos = originLoc.ToInches3D();

        using var innerMaterial = GetPickerMaterial(spellEnum, 0, false);
        using var outerMaterial = GetPickerMaterial(spellEnum, 1, false);

        _pickerAreaRenderer.Render(viewport, centerPos, elevation, radius, innerMaterial.Resource, outerMaterial.Resource);
    }

    [TempleDllLocation(0x10109980)]
    private void DrawCircleInvalidTarget(IGameViewport viewport, GameObject target, GameObject caster, int spellEnum)
    {
        var friendly = GameSystems.Critter.IsFriendly(target, caster);
        var outcome = friendly ? 6 : 7;
        IntgameSpellTargetCircleRender(viewport, target, spellEnum, outcome);
    }

    [TempleDllLocation(0x10109940)]
    public void DrawCircleValidTarget(IGameViewport viewport, GameObject target, GameObject originator, int spellEnum)
    {
        var friendly = GameSystems.Critter.IsFriendly(target, originator);
        var outcome = friendly ? 3 : 4;
        IntgameSpellTargetCircleRender(viewport, target, spellEnum, outcome);
    }

    [TempleDllLocation(0x10108c50)]
    private void IntgameSpellTargetCircleRender(IGameViewport viewport, GameObject target, int spellEnum, int outcome)
    {
        var radius = target.GetRadius();
        var centerLoc = target.GetLocationFull();
        var center = centerLoc.ToInches3D();

        var fogStatus = GameSystems.MapFogging.GetFogStatus(centerLoc);
        var occluded = (fogStatus & 0xB0) != 0;

        using var material = GetPickerMaterial(spellEnum, outcome, occluded);
        _pickerCircleRenderer.Render(viewport, center, radius, material.Resource);
    }

    [TempleDllLocation(0x10106d10)]
    private void DrawPlayerSpellPointer(IGameViewport viewport, GameObject originator, LocAndOffsets tgtLoc)
    {
        var radius = originator.GetRadius() * 1.5f;
        var centerLoc = originator.GetLocationFull();
        var center = centerLoc.ToInches3D();

        var direction = centerLoc.RotationTo(tgtLoc);
        _playerSpellPointerRenderer.Render(viewport, center, radius, direction, _spellPointerMaterial.Resource);
    }

    private static readonly string[] OutcomeNames =
    {
        "inner",
        "outer",
        "target",
        "target_friendly",
        "target_hostile",
        "invalid",
        "invalid_friendly",
        "invalid_hostile",
    };

    [TempleDllLocation(0x10107180)]
    private ResourceRef<IMdfRenderMaterial> GetPickerMaterial(int spellEnum, int outcome, bool occluded)
    {
        var occludedSuffix = occluded ? "_oc" : "";
        var outcomeName = OutcomeNames[outcome];

        // Try loading a spell specific target texture
        var spellEnumName = GameSystems.Spell.GetSpellEnumName(spellEnum);
        if (spellEnumName != null)
        {
            var filename = $"art/interface/intgame_select/{spellEnumName}-{outcomeName}{occludedSuffix}.mdf";
            if (Tig.FS.FileExists(filename))
            {
                return Tig.MdfFactory.LoadMaterial(filename);
            }
        }

        // Fall back to a spell school specific targetting circle
        var spellSchool = GameSystems.Spell.GetSpellSchoolEnum(spellEnum);
        var spellSchoolName = GameSystems.Spell.GetSpellSchoolEnumName(spellSchool);
        if (spellSchoolName != null)
        {
            var filename = $"art/interface/intgame_select/{spellSchoolName}-{outcomeName}{occludedSuffix}.mdf";
            if (Tig.FS.FileExists(filename))
            {
                return Tig.MdfFactory.LoadMaterial(filename);
            }
        }

        // Previously it fell back to the name of spell 0, but this translates just to 'spell_none'
        var fallbackFilename = $"art/interface/intgame_select/spell_none-{outcomeName}{occludedSuffix}.mdf";
        return Tig.MdfFactory.LoadMaterial(fallbackFilename);
    }

    [TempleDllLocation(0x10108fa0)]
    private void RenderTargetNumberLabels(IGameViewport viewport)
    {
        foreach (var kvp in intgameselTexts)
        {
            var obj = kvp.Key;
            var text = kvp.Value;

            var worldPos = obj.GetLocationFull().ToInches3D();

            var screenPos = viewport.WorldToScreen(worldPos);

            var extents = new RectangleF
            {
                X = screenPos.X - 100,
                Y = screenPos.Y,
                Width = 200,
                Height = 20
            };
            Tig.RenderingDevice.TextEngine.RenderText(
                extents,
                PickerTargetLabelStyle,
                text
            );
        }

        intgameselTexts.Clear();
    }

    [TempleDllLocation(0x10108ed0)]
    private void AddTargetNumberLabel(GameObject obj, int tgtNumber)
    {
        // Append to the same object's text if it exists
        if (intgameselTexts.TryGetValue(obj, out var existingText))
        {
            intgameselTexts[obj] = existingText + $", {tgtNumber}";
        }
        else
        {
            intgameselTexts[obj] = $"{tgtNumber}";
        }
    }

    public void CancelPicker()
    {
        ActivePicker?.Behavior.CancelPicker();
    }

    // TODO
    public bool HandleMessage(IGameViewport viewport, Message msg)
    {
        if (!IsPicking)
        {
            return false;
        }

        var pickerSpec = ActivePicker.Behavior;

        if (msg.type == MessageType.KEYSTATECHANGE)
        {
            return pickerSpec.KeyStateChanged(msg.KeyStateChangeArgs);
        }
        else if (msg.type == MessageType.CHAR)
        {
            return pickerSpec.CharacterTyped(msg.CharArgs);
        }

        return false;
    }

    
    [TempleDllLocation(0x101375e0)]
    [TemplePlusLocation("ui_picker.cpp:144")]
    [TempleDllLocation(0x10135f00)]
    public void AddEventListeners<T>(T viewport) where T : WidgetBase, IGameViewport
    {
        WidgetBase.EventHandler<MouseEvent> MakeHandler(Action<PickerBehavior, MouseEvent> pickerHandler)
        {
            return e =>
            {
                var picker = ActivePicker;
                if (picker != null)
                {
                    e.StopImmediatePropagation();
                
                    // update x/y position
                    picker.MouseX = (int) e.X;
                    picker.MouseY = (int) e.Y;
                
                    var pickerSpec = picker.Behavior;
                    pickerHandler(pickerSpec, e);
                }
            };
        }

        viewport.OnMouseDown += MakeHandler((behavior, e) =>
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    behavior.LeftMouseButtonClicked(viewport, e);
                    break;
                case MouseButton.Right:
                    behavior.RightMouseButtonClicked(viewport, e);
                    break;
                case MouseButton.Middle:
                    behavior.MiddleMouseButtonClicked(viewport, e);
                    break;
            }
        });
        viewport.OnMouseUp += MakeHandler((behavior, e) =>
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    behavior.LeftMouseButtonReleased(viewport, e);
                    break;
                case MouseButton.Right:
                    behavior.RightMouseButtonReleased(viewport, e);
                    break;
                case MouseButton.Middle:
                    behavior.MiddleMouseButtonReleased(viewport, e);
                    break;
            }
        });
        viewport.OnMouseMove += MakeHandler((behavior, e) =>
        {
            behavior.MouseMoved(viewport, e);
        });
        viewport.OnMouseWheel += MakeHandler((behavior, e) =>
        {
            behavior.MouseWheelScrolled(viewport, e);
        });
    }
}

internal class PickerState
{
    public PickerArgs Picker;

    public float MouseX;

    public float MouseY;

    public object CallbackArgs;

    public int tgtIdx;

    public bool cursorStackCount_Maybe;

    public PickerBehavior Behavior { get; set; }

    public GameObject Target { get; set; }

    public GameRaycastFlags GetFlagsFromExclusions(MouseEvent e)
    {
        GameRaycastFlags result = GameRaycastFlags.HITTEST_3D;
        if (e.IsAltHeld)
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