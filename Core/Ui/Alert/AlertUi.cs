using System;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Alert
{
    public class AlertUi
    {
        private WidgetContainer _mainWindow;

        private ScrollBox _scrollBox;

        private WidgetLegacyText _titleLabel;

        private Action<int> _okCallback;

        private static readonly TigTextStyle TitleStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_CENTER,
            kerning = 1,
            tracking = 3
        };

        private WidgetButton _okButton;

        [TempleDllLocation(0x1019da60)]
        public AlertUi()
        {
            var doc = WidgetDoc.Load("ui/alert_ui.json");

            _mainWindow = doc.TakeRootContainer();
            _mainWindow.ZIndex = 99800;
            _mainWindow.Name = "alert_main_window";
            _mainWindow.SetVisible(false);

            _titleLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_12, TitleStyle);
            _titleLabel.SetX(28);
            _titleLabel.SetY(14);
            _titleLabel.FixedSize = new Size(295, 18);
            _mainWindow.AddContent(_titleLabel);

            _okButton = doc.GetButton("alert_ok_button");
            _okButton.Name = "alert_ok_button";
            _okButton.SetClickHandler(OkButtonClicked);
            _mainWindow.Add(_okButton);

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
        public void Show(HelpRequest helpRequest, Action<int> callback, string buttonText)
        {
            Hide();

            _mainWindow.SetVisible(true);
            HelpUi.SetContent(_scrollBox, helpRequest, out var windowTitle);
            _titleLabel.Text = windowTitle;
            _mainWindow.BringToFront();
            _mainWindow.CenterOnScreen();
            GameSystems.TimeEvent.PushDisableFidget();

            _okCallback = callback;
            _okButton.SetText(buttonText);
        }

        [TempleDllLocation(0x1019d480)]
        public void Hide()
        {
            if (_mainWindow.IsVisible())
            {
                GameSystems.TimeEvent.PopDisableFidget();
                _mainWindow.SetVisible(false);
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
}