using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.Party;

public class PartyUi : IResetAwareSystem, IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10BE33F8)]
    public GameObject? ForceHovered { get; set; }

    [TempleDllLocation(0x10BE3400)]
    public GameObject? ForcePressed { get; set; }

    [TempleDllLocation(0x10BE2E98)]
    private List<PartyUiPortrait> _portraits = new();

    [TempleDllLocation(0x10BE33E0)]
    private bool ui_party_widgets_need_refresh;

    private WidgetContainer _container;

    private PartyUiParams _uiParams;

    [TempleDllLocation(0x10BE33E8)]
    private bool dword_10BE33E8;

    [TempleDllLocation(0x10BE3414)]
    private bool _canSwapPartyMembers;

    [TempleDllLocation(0x10BE33D4)]
    private Func<GameObject, bool> _targetClickCallback;

    [TempleDllLocation(0x10BE33D8)]
    private Action<GameObject> _targetOnEnterCallback;

    [TempleDllLocation(0x10BE33DC)]
    private Action<GameObject> _targetOnExitCallback;

    [TempleDllLocation(0x102F8EB4)]
    private int dword_102F8EB4;

    [TempleDllLocation(0x102F8EB0)]
    private int dword_102F8EB0;

    [TempleDllLocation(0x10BE33EC)]
    private bool _isDraggingPartyMember;

    [TempleDllLocation(0x10134bf0)]
    public PartyUi()
    {
        _uiParams = new PartyUiParams();
        CreateWidgets();
        ui_party_widgets_need_refresh = true;

        UiSystems.InGameSelect.LoadSelectionShaders();

        Globals.UiManager.OnCanvasSizeChanged += ResizeViewport;
        ResizeViewport(Globals.UiManager.CanvasSize);
        Globals.UiManager.AddWindow(_container);
    }

    // Related to target picking
    [TempleDllLocation(0x10131950)]
    public void SetTargetCallbacks(Func<GameObject, bool> onClick, Action<GameObject> onEnter,
        Action<GameObject> onExit)
    {
        _targetClickCallback = onClick;
        _targetOnEnterCallback = onEnter;
        _targetOnExitCallback = onExit;
    }

    [TempleDllLocation(0x101318D0)]
    private bool InvokeTargetClickCallback(GameObject obj)
    {
        var result = false;
        if (_targetClickCallback != null)
        {
            result = _targetClickCallback(obj);
            if (result)
            {
                _targetClickCallback = null;
                _targetOnEnterCallback = null;
                _targetOnExitCallback = null;
            }
        }

        return result;
    }

    /// <summary>
    /// Used for dropping things on party member portrait.
    /// </summary>
    [TempleDllLocation(0x10131a00)]
    public bool TryGetPartyMemberByWidget(WidgetBase widget, out GameObject partyMember)
    {
        if (widget is PortraitButton button)
        {
            partyMember = button.PartyMember;
            return true;
        }

        partyMember = null;
        return false;
    }

    [TempleDllLocation(0x10131910)]
    private void InvokeTargetOnEnterCallback(GameObject obj)
    {
        _targetOnEnterCallback?.Invoke(obj);
    }

    [TempleDllLocation(0x10131930)]
    private void InvokeTargetOnExitCallback()
    {
        _targetOnExitCallback?.Invoke(null);
    }

    [TempleDllLocation(0x10133800)]
    private void CreateWidgets()
    {
        var doc = WidgetDoc.Load("ui/party_ui.json");

        _container = doc.GetRootContainer();
    }

    [TempleDllLocation(0x10134cb0)]
    public void Update()
    {
        var availablePortraits = _portraits;
        _portraits = new List<PartyUiPortrait>(GameSystems.Party.PartySize);

        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (GameSystems.Party.IsAiFollower(partyMember))
            {
                continue;
            }

            var existingIdx = availablePortraits.FindIndex(p => p.PartyMember == partyMember);
            if (existingIdx != -1)
            {
                var portrait = availablePortraits[existingIdx];
                portrait.Update();
                _portraits.Add(portrait);
                availablePortraits.RemoveAt(existingIdx);
            }
            else
            {
                var portrait = new PartyUiPortrait(partyMember, _uiParams);
                var widget = portrait.Widget;
                widget.OnMouseDown += e => PortraitMouseDown(portrait, e);
                widget.OnMouseUp += e => PortraitMouseUp(portrait, e);
                widget.OnMouseMove += PortraitMouseMove;
                widget.OnMouseEnter += e => PortraitMouseEnter(portrait, e);
                widget.OnMouseLeave += PortraitMouseLeave;
                widget.AddOtherClickListener(() => PortraitRightClicked(portrait));
                _portraits.Add(portrait);
                _container.Add(portrait.Widget);
            }
        }

        // Position portraits according to their position in the party list
        for (var i = 0; i < _portraits.Count; i++)
        {
            _portraits[i].Widget.X = i * _portraits[i].Widget.ComputePreferredBorderAreaSize().Width;
        }

        // Free any remaining unused portraits
        availablePortraits.DisposeAll();
    }

    [TempleDllLocation(0x10135000)]
    public void UpdateAndShowMaybe()
    {
        Update();
    }

    [TempleDllLocation(0x101350b0)]
    private void ResizeViewport(Size screenSize)
    {
        _container.Width = Dimension.Pixels(screenSize.Width - 2 * _uiParams.party_ui_main_window.X);
        _container.Y = screenSize.Height + _uiParams.party_ui_main_window.Y;
    }

    [TempleDllLocation(0x101331e0)]
    private void PortraitRightClicked(PartyUiPortrait portrait)
    {
        var partyMember = portrait.PartyMember;
        if (UiSystems.CharSheet.State != CharInventoryState.LevelUp)
        {
            if (!GameSystems.Combat.IsCombatActive() || GameSystems.D20.Initiative.CurrentActor == partyMember)
            {
                if (UiSystems.CharSheet.HasCurrentCritter
                    && UiSystems.CharSheet.State != CharInventoryState.LevelUp)
                {
                    if (UiSystems.CharSheet.CurrentCritter == partyMember)
                    {
                        UiSystems.CharSheet.CurrentPage = 0;
                        UiSystems.CharSheet.Hide(CharInventoryState.Closed);
                    }
                    else
                    {
                        UiSystems.CharSheet.ShowInState(0, partyMember);
                    }
                }
                else
                {
                    UiSystems.HideOpenedWindows(true);
                    UiSystems.CharSheet.State = 0;
                    UiSystems.CharSheet.Show(partyMember);
                }
            }
        }
    }
    
    [TempleDllLocation(0x101331e0)]
    private void PortraitMouseDown(PartyUiPortrait portrait, MouseEvent e)
    {
        if (e.Button == MouseButton.Left && !dword_10BE33E8)
        {
            var partyMember = portrait.PartyMember;
            var partyIdx = GameSystems.Party.PartyMembers.ToList().IndexOf(partyMember);
            
            Logger.Info("pressed");
            dword_102F8EB0 = partyIdx;
            _isDraggingPartyMember = false;
            _canSwapPartyMembers = false;
            dword_102F8EB4 = partyIdx;
            dword_10BE33E8 = true;
            portrait.Widget.SetMouseCapture();
        }
    }

    [TempleDllLocation(0x101331e0)]
    private void PortraitMouseUp(PartyUiPortrait portrait, MouseEvent e)
    {
        if (e.Button != MouseButton.Left)
        {
            return;
        }
        
        var partyMember = portrait.PartyMember;

        if (!portrait.Widget.ContainsMouse)
        {
            goto LABEL_14;
        }
    
        if (UiSystems.HelpManager.IsSelectingHelpTarget)
        {
            dword_10BE33E8 = false;
            _isDraggingPartyMember = false;
            UiSystems.HelpManager.ShowPredefinedTopic(40);
            return;
        }

        if (InvokeTargetClickCallback(partyMember))
        {
            dword_10BE33E8 = false;
            _isDraggingPartyMember = false;
            _canSwapPartyMembers = false;
            return;
        }

        if (GameSystems.Combat.IsCombatActive())
        {
            UiSystems.TurnBased.sub_101749D0();
        }

        LABEL_14:
        Logger.Info("released");
        if (dword_10BE33E8)
        {
            _canSwapPartyMembers = false;
            dword_10BE33E8 = false;
        }

        if (_isDraggingPartyMember)
        {
            _isDraggingPartyMember = false;
            GameSystems.Party.Swap(dword_102F8EB0, dword_102F8EB4);
            return;
        }

        if (GameSystems.Combat.IsCombatActive() ||
            UiSystems.CharSheet.State == CharInventoryState.LevelUp)
            return;

        if (!UiSystems.CharSheet.Inventory.IsVisible)
        {
            var v10 = partyMember.GetLocation();
            GameSystems.Location.GetTranslation(v10.locx, v10.locy, out var xTranslationOut,
                out var yTranslationOut);
            if (xTranslationOut < 20
                || yTranslationOut < 20
                || xTranslationOut > Globals.UiManager.CanvasSize.Width - 20
                || yTranslationOut > Globals.UiManager.CanvasSize.Height - 20)
            {
                GameSystems.Scroll.CenterOnSmooth(v10.locx, v10.locy);
            }
        }

        var v11 = UiSystems.CharSheet.CurrentCritter;
        if (v11 == null)
        {
            if (UiSystems.RadialMenu.IsOpen)
                return;

            if (e.IsShiftHeld)
            {
                // Multi-select
                if (GameSystems.Party.IsSelected(partyMember))
                {
                    GameSystems.Party.RemoveFromSelection(partyMember);
                }
                else
                {
                    GameSystems.Party.AddToSelection(partyMember);
                }
            }
            else
            {
                GameSystems.Party.ClearSelection();
                GameSystems.Party.AddToSelection(partyMember);
            }

            return;
        }

        if (partyMember == v11 || UiSystems.CharSheet.State == CharInventoryState.LevelUp)
        {
            Logger.Info("[popup-window]: YOU CANNOT PRESS THIS BUTTON WHILE LEVELLING UP");
        }
        else
        {
            var state = (CharInventoryState) UiSystems.CharSheet.Looting.GetLootingState();
            UiSystems.CharSheet.ShowInState(state, partyMember);
        }
    }

    private void PortraitMouseMove(MouseEvent e)
    {
        if (e.IsLeftButtonHeld && dword_10BE33E8 && !_canSwapPartyMembers)
        {
            _canSwapPartyMembers = true;
        }
    }

    [TempleDllLocation(0x101331e0)]
    private void PortraitMouseEnter(PartyUiPortrait portrait, MouseEvent e)
    {
        var partyMember = portrait.PartyMember;
        var partyIdx = GameSystems.Party.PartyMembers.ToList().IndexOf(partyMember);
    
        InvokeTargetOnEnterCallback(partyMember);

        if (dword_10BE33E8)
        {
            dword_102F8EB4 = partyIdx;
            if (dword_102F8EB4 != dword_102F8EB0)
            {
                _isDraggingPartyMember = true;
            }
        }
        else if (GameSystems.Combat.IsCombatActive())
        {
            UiSystems.TurnBased.TargetFromPortrait(partyMember);
        }
    }
    
    [TempleDllLocation(0x101331e0)]
    private void PortraitMouseLeave(MouseEvent e)
    {
        InvokeTargetOnExitCallback();
        if (dword_10BE33E8)
        {
            dword_102F8EB4 = dword_102F8EB0;
            _isDraggingPartyMember = false;
        }

        UiSystems.TurnBased.TargetFromPortrait(null);
    }

    [TempleDllLocation(0x10132760)]
    public void Dispose()
    {
        Stub.TODO();
        _uiParams.Dispose();
    }

    [TempleDllLocation(0x10132500)]
    public ActionCursor? GetCursor()
    {
        if (_canSwapPartyMembers)
        {
            return ActionCursor.SlidePortraits;
        }

        return null;
    }

    [TempleDllLocation(0x10131a50)]
    public bool TryGetPartyMemberRect(GameObject caster, out RectangleF rectangle)
    {
        var portrait = _portraits.Find(p => p.PartyMember == caster);
        if (portrait != null)
        {
            rectangle = portrait.Widget.GetViewportBorderArea();
            return true;
        }

        rectangle = default;
        return false;
    }

    [TempleDllLocation(0x101350a0)]
    public void Reset()
    {
        Update();
    }

    [TempleDllLocation(0x10135080)]
    public void Clear()
    {
        _portraits.DisposeAndClear();
        _container.Clear();
        ui_party_widgets_need_refresh = true;
    }

    public void Show()
    {
        Globals.UiManager.AddWindow(_container);
    }

    public void Hide()
    {
        Globals.UiManager.RemoveWindow(_container);
    }
}