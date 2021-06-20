using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Logbook
{
    internal class LogbookKeyAcquiredPopup
    {

        private readonly WidgetContainer _window;

        public event Action<bool> OnChangeNotificationSetting;

        public LogbookKeyAcquiredPopup(LogbookKeyTranslations translations)
        {
            var doc = WidgetDoc.Load("ui/key_acquired_popup.json");

            _window = doc.GetRootContainer();

            doc.GetTextContent("title").Text = translations.NotificationPopupTitle;

            var textContainer = doc.GetContainer("textContainer");

            var text = new WidgetText();
            text.FixedWidth = 237;
            text.Text = translations.NotificationPopupText;
            textContainer.AddContent(text);

            var prompt = new WidgetText();
            prompt.FixedWidth = 237;
            prompt.Text = translations.NotificationPopupPrompt;
            prompt.Y = text.GetPreferredSize().Height + 13;
            textContainer.AddContent(prompt);

            // Created @ 0x1019727c
            // _window.OnHandleMessage += 0x101f5850;
            // _window.OnBeforeRender += 0x10196a10;
            _window.ZIndex = 100051;
            _window.Name = "logbook_ui_keys_key_entry_window";
            _window.Visible = false;

            // Created @ 0x10197385
            var acceptButton = doc.GetButton("accept");
            // logbook_ui_key_entry_accept_butn1.OnHandleMessage += 0x10197070;
            // logbook_ui_key_entry_accept_butn1.OnBeforeRender += 0x10196d70;
            acceptButton.Text = translations.NotificationPopupYes;
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
            declineButton.Text = translations.NotificationPopupNo;
            declineButton.Name = "logbook_ui_key_entry_decline_butn";
            declineButton.SetClickHandler(Hide);
        }

        [TempleDllLocation(0x101954C0)]
        public void Show()
        {
            _window.CenterOnScreen();
            _window.Visible = true;
            _window.BringToFront();
        }

        public void Hide()
        {
            _window.Visible = false;
        }
    }
}