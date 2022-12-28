using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.UtilityBar;

public class UtilityBarHistoryUi
{
    private static readonly WidgetButtonStyle ToggleRollButtonStyle = new()
    {
        NormalImagePath = "art/interface/history/Tab_Roll_Unselected.tga",
        HoverImagePath = "art/interface/history/Tab_Roll_Hover.tga",
        PressedImagePath = "art/interface/history/Tab_Roll_Click.tga"
    };

    private static readonly WidgetButtonStyle ToggleDialogButtonStyle = new()
    {
        NormalImagePath = "art/interface/history/Dialog_Unselected.tga",
        HoverImagePath = "art/interface/history/Dialog_Hover.tga",
        PressedImagePath = "art/interface/history/Dialog_Click.tga"
    };

    [TempleDllLocation(0x10bdddb0)] [TempleDllLocation(0x10bdde54)]
    private WidgetContainer _container;

    [TempleDllLocation(0x10bdde58)]
    private ScrollBox _rollHistory;

    [TempleDllLocation(0x10bdde5c)]
    private bool _maximized;

    [TempleDllLocation(0x10bdde30)]
    private WidgetButton uiHistoryMinimizeBtn;

    [TempleDllLocation(0x10bdde14)]
    private WidgetButton uiHistoryMaximizeBtn;

    [TempleDllLocation(0x10bdde50)]
    private WidgetButton uiHistoryMinimizeDialogBtn;

    [TempleDllLocation(0x10bdde48)]
    private WidgetButton uiHistoryMaximizeDialogBtn;

    [TempleDllLocation(0x10bdde34)]
    public bool IsVisible => _container.IsInTree;

    private List<D20RollHistoryLine> _lines = new();

    [TempleDllLocation(0x101226a0)]
    public UtilityBarHistoryUi()
    {
        var containerRect = new RectangleF(0, 0, 182, 292);
        _container = new WidgetContainer(containerRect);
        _container.ZIndex = 99900;
        UpdatePosition(Globals.UiManager.CanvasSize);

        uiHistoryMinimizeBtn = new WidgetButton(new RectangleF(containerRect.Width - 36, 0, 24, 20));
        uiHistoryMinimizeBtn.SetStyle(ToggleRollButtonStyle);
        uiHistoryMinimizeBtn.TooltipText = UiSystems.Tooltip.GetString(6029);
        uiHistoryMinimizeBtn.AddClickListener(() =>
        {
            // Previously @ 0x10121C20
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(18);
            }
            else
            {
                Minimize();
            }
        });
        _container.Add(uiHistoryMinimizeBtn);

        uiHistoryMaximizeBtn = new WidgetButton(new RectangleF(containerRect.Width - 36, 272, 24, 20));
        uiHistoryMaximizeBtn.SetStyle(ToggleRollButtonStyle);
        uiHistoryMaximizeBtn.TooltipText = UiSystems.Tooltip.GetString(6029);
        uiHistoryMaximizeBtn.AddClickListener(() =>
        {
            // Previously @ 0x10121bd0
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(18);
            }
            else
            {
                Maximize();
            }
        });
        _container.Add(uiHistoryMaximizeBtn);

        uiHistoryMaximizeDialogBtn = new WidgetButton(new RectangleF(containerRect.Width - 62, 272, 24, 20));
        uiHistoryMaximizeDialogBtn.SetStyle(ToggleDialogButtonStyle);
        uiHistoryMaximizeDialogBtn.AddClickListener(OnDialogButtonClicked);
        uiHistoryMaximizeDialogBtn.TooltipText = UiSystems.Tooltip.GetString(6028);
        _container.Add(uiHistoryMaximizeDialogBtn);

        uiHistoryMinimizeDialogBtn = new WidgetButton(new RectangleF(containerRect.Width - 62, 0, 24, 20));
        uiHistoryMinimizeDialogBtn.SetStyle(ToggleDialogButtonStyle);
        uiHistoryMinimizeDialogBtn.AddClickListener(OnDialogButtonClicked);
        uiHistoryMinimizeDialogBtn.TooltipText = UiSystems.Tooltip.GetString(6028);
        _container.Add(uiHistoryMinimizeDialogBtn);

        ScrollBoxSettings settings = new ScrollBoxSettings();
        settings.TextArea = new Rectangle(10, 18, 144, 250);
        settings.ScrollBarPos = new Point(155, 8);
        settings.ScrollBarHeight = 262;
        settings.Indent = 7;
        settings.Font = PredefinedFont.ARIAL_10;
        _rollHistory = new ScrollBox(new Rectangle(0, 18, 177, 275), settings);
        _rollHistory.BackgroundPath = "art/interface/SCROLLBOX/rollhistory_background.img";
        _container.Add(_rollHistory);
        GameSystems.RollHistory.OnHistoryLineAdded += line =>
        {
            _lines.Add(line);
            if (_lines.Count > 100)
            {
                _lines.RemoveAt(0);
            }

            _rollHistory.SetEntries(_lines);
        };
        GameSystems.RollHistory.OnHistoryCleared += ClearLines;

        UpdateWidgetVisibility();

        Globals.UiManager.OnCanvasSizeChanged += UpdatePosition;
        UpdatePosition(Globals.UiManager.CanvasSize);
    }

    private void UpdatePosition(Size screenSize)
    {
        _container.X = screenSize.Width - 182;
        _container.Y = screenSize.Height - 292 - 82;
    }

    [TempleDllLocation(0x100dff90)]
    private void ClearLines()
    {
        _lines.Clear();
        _rollHistory.ClearLines();
    }

    [TempleDllLocation(0x10121c70)]
    private void OnDialogButtonClicked()
    {
        if (UiSystems.HelpManager.IsSelectingHelpTarget)
        {
            UiSystems.HelpManager.ShowPredefinedTopic(19);
        }
        else
        {
            UiSystems.Dialog.ToggleHistory();
        }
    }

    [TempleDllLocation(0x10121ac0)]
    public void UpdateWidgetVisibility()
    {
        _rollHistory.Visible = _maximized;
        uiHistoryMinimizeBtn.Visible = _maximized;
        uiHistoryMaximizeBtn.Visible = !_maximized;
        uiHistoryMinimizeDialogBtn.Visible = _maximized;
        uiHistoryMaximizeDialogBtn.Visible = !_maximized;
    }

    [TempleDllLocation(0x10121b20)]
    public void HideDialogButton()
    {
        uiHistoryMinimizeDialogBtn.Visible = false;
        uiHistoryMaximizeDialogBtn.Visible = false;
    }

    [TempleDllLocation(0x10121c08)]
    [TempleDllLocation(0x10121a00)]
    private void Maximize()
    {
        _maximized = true;
        UpdateWidgetVisibility();
    }

    [TempleDllLocation(0x10121c58)]
    [TempleDllLocation(0x10121a60)]
    private void Minimize()
    {
        _maximized = false;
        UpdateWidgetVisibility();
    }

    [TempleDllLocation(0x101221c0)]
    public void Show()
    {
        Globals.UiManager.AddWindow(_container);
        if (_maximized)
        {
            Maximize();
        }
        else
        {
            Minimize();
        }
    }

    [TempleDllLocation(0x101219b0)]
    public void Hide()
    {
        Globals.UiManager.RemoveWindow(_container);
    }
}