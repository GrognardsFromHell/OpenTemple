using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Alert;

public class AlertUi
{
    private readonly WidgetContainer _mainWindow;

    private readonly ScrollBox _scrollBox;

    private readonly WidgetText _titleLabel;

    private readonly WidgetButton _okButton;

    private Action<int> _okCallback;

    [TempleDllLocation(0x1019da60)]
    public AlertUi()
    {
        var doc = WidgetDoc.Load("ui/alert_ui.json");

        _mainWindow = doc.GetRootContainer();
        _mainWindow.ZIndex = 99800;
        _mainWindow.Name = "alert_main_window";

        _titleLabel = doc.GetTextContent("title");

        _okButton = doc.GetButton("alert_ok_button");
        _okButton.Name = "alert_ok_button";
        _okButton.AddClickListener(OkButtonClicked);
        
        ScrollBoxSettings settings = new ScrollBoxSettings();
        settings.TextArea = new Rectangle(6, 16, 287, 208);
        settings.ScrollBarPos = new Point(297, 1);
        settings.ScrollBarHeight = 224;
        settings.Indent = 7;
        settings.Font = PredefinedFont.ARIAL_10;

        _scrollBox = new ScrollBox(new Rectangle(20, 36, 310, 226), settings);
        _scrollBox.ZIndex = 99950;
        _scrollBox.Name = "scrollbox_main_window";
        _mainWindow.Add(_scrollBox);
    }

    [TempleDllLocation(0x1019d6d0)]
    [TempleDllLocation(0x1019D500)]
    public void Show(HelpRequest helpRequest, Action<int> callback, string buttonText)
    {
        Hide();

        HelpUi.SetContent(_scrollBox, helpRequest, out var windowTitle);
        _titleLabel.Text = windowTitle;
        GameSystems.TimeEvent.PauseGameTime();
        Globals.UiManager.ShowModal(_mainWindow);

        _okCallback = callback;
        _okButton.Text = buttonText;
    }

    [TempleDllLocation(0x1019d480)]
    public void Hide()
    {
        if (_mainWindow.IsInTree)
        {
            GameSystems.TimeEvent.ResumeGameTime();
            Globals.UiManager.RemoveWindow(_mainWindow);
        }
    }

    [TempleDllLocation(0x1019d270)]
    private void OkButtonClicked()
    {
        Hide();

        var callback = _okCallback;
        _okCallback = null;
        callback?.Invoke(0);
    }

}