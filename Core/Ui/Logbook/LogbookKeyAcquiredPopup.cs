using System;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Logbook
{
    internal class LogbookKeyAcquiredPopup
    {

        private static readonly TigTextStyle TitleStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_CENTER|TigTextStyleFlag.TTSF_DROP_SHADOW,
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            kerning = 1,
            tracking = 3
        };

        private static readonly TigTextStyle BodyStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_CENTER,
            kerning = 2,
            tracking = 2
        };

        private readonly WidgetContainer _window;

        public event Action<bool> OnChangeNotificationSetting;

        public LogbookKeyAcquiredPopup(LogbookKeyTranslations translations)
        {
            var doc = WidgetDoc.Load("ui/key_acquired_popup.json");

            _window = doc.TakeRootContainer();

            var title = new WidgetLegacyText(translations.NotificationPopupTitle, PredefinedFont.PRIORY_12,
                TitleStyle);
            title.SetY(18);
            _window.AddContent(title);

            var textContainer = doc.GetWindow("textContainer");

            var text = new WidgetLegacyText("", PredefinedFont.ARIAL_10,
                BodyStyle);
            text.MaxWidth = 237;
            text.Text = translations.NotificationPopupText;
            textContainer.AddContent(text);

            var prompt = new WidgetLegacyText(translations.NotificationPopupPrompt, PredefinedFont.ARIAL_10,
                BodyStyle);
            prompt.SetY(text.GetPreferredSize().Height + 13);
            textContainer.AddContent(prompt);

            // Created @ 0x1019727c
            // _window.OnHandleMessage += 0x101f5850;
            // _window.OnBeforeRender += 0x10196a10;
            _window.ZIndex = 100051;
            _window.Name = "logbook_ui_keys_key_entry_window";
            _window.SetVisible(false);

            // Created @ 0x10197385
            var acceptButton = doc.GetButton("accept");
            // logbook_ui_key_entry_accept_butn1.OnHandleMessage += 0x10197070;
            // logbook_ui_key_entry_accept_butn1.OnBeforeRender += 0x10196d70;
            acceptButton.SetText(translations.NotificationPopupYes);
            acceptButton.Name = "logbook_ui_key_entry_accept_butn";
            acceptButton.SetClickHandler(() =>
            {
                OnChangeNotificationSetting?.Invoke(false);
                Hide();
            });

            // Created @ 0x101974c2
            var declineButton = doc.GetButton("decline");
            // logbook_ui_key_entry_decline_butn1.OnHandleMessage += 0x10197070;
            // logbook_ui_key_entry_decline_butn1.OnBeforeRender += 0x10196d70;
            declineButton.SetText(translations.NotificationPopupNo);
            declineButton.Name = "logbook_ui_key_entry_decline_butn";
            declineButton.SetClickHandler(Hide);
        }

        [TempleDllLocation(0x101954C0)]
        public void Show()
        {
            _window.CenterOnScreen();
            _window.SetVisible(true);
            _window.BringToFront();
        }

        public void Hide()
        {
            _window.SetVisible(false);
        }
    }
}