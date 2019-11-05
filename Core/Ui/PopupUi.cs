using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.CharSheet.Portrait;
using SpicyTemple.Core.Ui.Styles;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui
{
    public class PopupUi : IDisposable, IResetAwareSystem
    {
        private readonly Dictionary<int, string> _vanillaTranslations;

        [TempleDllLocation(0x10C04028)]
        private UiPopupStyle[] uipopupstyles;

        [TempleDllLocation(0x102fc218)]
        private int uiPopupCurrent;

        [TempleDllLocation(0x10C03BD8)]
        private UiPromptListEntry[] uiPopups = new UiPromptListEntry[5];

        [TempleDllLocation(0x10171df0)]
        public PopupUi()
        {
            Stub.TODO();
            _vanillaTranslations = Tig.FS.ReadMesFile("mes/vanilla_ui.mes");

            // Previously loaded from MES file (art/interface/popup_ui/popup_ui_styles.mes)
            uipopupstyles = new[]
            {
                new UiPopupStyle("default", PredefinedFont.ARIAL_10, PackedLinearColorA.White),
                new UiPopupStyle("small", PredefinedFont.ARIAL_10, new PackedLinearColorA(153, 255, 153, 255)),
                new UiPopupStyle("green", PredefinedFont.ARIAL_10, new PackedLinearColorA(153, 255, 153, 255)),
            };

            for (var i = 0; i < uiPopups.Length; i++)
            {
                uiPopups[i] = new UiPromptListEntry();
                CreatePopupWidget(uiPopups[i]);
            }
        }

        [TempleDllLocation(0x10171ba0)]
        private void CreatePopupWidget(UiPromptListEntry uiPopup)
        {
            var window = new WidgetContainer(new Rectangle(0, 0, 0, 0));
// popup_ui_main_window1.OnBeforeRender += 0x10170a90;
            window.Name = "popup_ui_main_window";
            window.SetVisible(false);
            window.SetMouseMsgHandler(msg => true); // Swallow mouse clicks and such
            uiPopup.wnd = window;

            uiPopup.background = new WidgetImage();
            window.AddContent(uiPopup.background);

            uiPopup.titleLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_10, TigTextStyle.standardWhite);
            window.AddContent(uiPopup.titleLabel);

            uiPopup.bodyLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_10, TigTextStyle.standardWhite);
            window.AddContent(uiPopup.bodyLabel);

            var okButton = new WidgetButton(new Rectangle(0, 0, 0, 0));
// popup_ui_button1.OnHandleMessage += 0x10171b50;
// popup_ui_button1.OnBeforeRender += 0x10170c30;
// popup_ui_button1.OnRenderTooltip += 0x100027f0;
            okButton.Name = "popup_ok_button";
            okButton.SetVisible(false);
            window.Add(okButton);
            uiPopup.btn1 = okButton;
            okButton.SetClickHandler(() => OnClickButton(uiPopup, 0));

            var cancelButton = new WidgetButton(new Rectangle(0, 0, 0, 0));
// popup_ui_button2.OnHandleMessage += 0x10171b50;
// popup_ui_button2.OnBeforeRender += 0x10170e40;
// popup_ui_button2.OnRenderTooltip += 0x100027f0;
            cancelButton.Name = "popup_cancel_button";
            cancelButton.SetVisible(false);
            window.Add(cancelButton);
            uiPopup.btn2 = cancelButton;
            cancelButton.SetClickHandler(() => OnClickButton(uiPopup, 1));

            var popup_ui_button3 = new WidgetButton(new Rectangle(0, 0, 0, 0));
// popup_ui_button3.OnHandleMessage += 0x10171b50;
// popup_ui_button3.OnBeforeRender += 0x10171a90;
// popup_ui_button3.OnRenderTooltip += 0x100027f0;
            popup_ui_button3.Name = "popup_ui_button";
            popup_ui_button3.SetVisible(false);
            window.Add(popup_ui_button3);
            uiPopup.btn3 = popup_ui_button3;
        }

        [TempleDllLocation(0x101719d0)]
        private void OnClickButton(UiPromptListEntry popup, int buttonIdx)
        {
            var callback = popup.prompt.callback;
            var flag1_on = (popup.flags & 1) != 0;
            if (!flag1_on)
            {
                callback?.Invoke(buttonIdx);
            }

            popup.isActive = false;
            uiPopupCurrent = -1;
            popup.wnd.SetVisible(false);

            popup.prompt.onPopupHide?.Invoke();
            popup.wnd.Rectangle = Rectangle.Empty;
            popup.btn1.Rectangle = Rectangle.Empty;
            popup.btn2.Rectangle = Rectangle.Empty;
            popup.btn3.Rectangle = Rectangle.Empty;
            popup.prompt = null;

            if (flag1_on)
            {
                callback?.Invoke(buttonIdx);
            }
        }


        [TempleDllLocation(0x10171510)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10171e70)]
        public void Reset()
        {
            // Cancel all
            foreach (var popup in uiPopups)
            {
                if (popup.prompt != null)
                {
                    UiSystems.Popup.OnClickButton(popup, 1);
                }
            }
            uiPopupCurrent = -1;
        }

        [TempleDllLocation(0x10171a70)]
        public bool IsAnyOpen()
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x10171e40)]
        public void CloseAll()
        {
            Stub.TODO();
        }

        private class UiPromptListEntry
        {
            public int flags;
            public bool isActive;
            public WidgetContainer wnd;
            public WidgetButton btn1;
            public WidgetButton btn2;
            public WidgetButton btn3;
            public WidgetImage background;
            public WidgetLegacyText titleLabel;
            public WidgetLegacyText bodyLabel;
            public UiPromptPacket prompt;
        }

        private class UiPopupStyle
        {
            public readonly string StyleName;
            public readonly PredefinedFont Font;
            public readonly PackedLinearColorA FontColor;
            public readonly TigTextStyle TextStyle;

            public UiPopupStyle(string styleName, PredefinedFont font, PackedLinearColorA fontColor)
            {
                StyleName = styleName;
                Font = font;
                FontColor = fontColor;

                TextStyle = new TigTextStyle
                {
                    textColor = new ColorRect(FontColor),
                    kerning = 1,
                    tracking = 3
                };
            }
        }

        [TempleDllLocation(0x10171380)]
        public void UiPopupAddToList(int popupIdx, int flags, UiPromptPacket uiPrompt)
        {
            uiPopups[popupIdx].flags = flags;
            uiPopups[popupIdx].wnd.Rectangle = uiPrompt.wndRect;
            uiPopups[popupIdx].btn1.Rectangle = uiPrompt.okRect;
            uiPopups[popupIdx].btn2.Rectangle = uiPrompt.cancelRect;
            uiPopups[popupIdx].btn3.Rectangle = uiPrompt.textEntryRect;
        }

        [TempleDllLocation(0x10c03fd4)]
        private Rectangle stru_10C03FD4;

        [TempleDllLocation(0x10C04018)]
        private Rectangle stru_10C04018;

        [TempleDllLocation(0x10c04008)]
        private Rectangle uiPromptRect;

        [TempleDllLocation(0x10171580)]
        public void UiPopupShow_Impl(UiPromptPacket uiPrompt, int uiPromptIdx, int flags)
        {
            ref var popup = ref uiPopups[uiPromptIdx];
            popup.prompt = uiPrompt;
            uiPopupCurrent = uiPromptIdx;
            popup.isActive = true;
            UiPopupAddToList(uiPromptIdx, flags, uiPrompt);
            var styleIndex = popup.prompt.styleIdx;
            var popupStyle = uipopupstyles[styleIndex];

            popup.background.SetTexture(uiPrompt.image);

            if (uiPrompt.wndTitle != null)
            {
                Tig.Fonts.PushFont(popupStyle.Font);
                var metrics = new TigFontMetrics();
                Tig.Fonts.Measure(popupStyle.TextStyle, uiPrompt.wndTitle, ref metrics);

                popup.titleLabel.TextStyle = popupStyle.TextStyle;
                popup.titleLabel.Font = popupStyle.Font;
                popup.titleLabel.Visible = true;
                popup.titleLabel.SetX((230 - metrics.width) / 2 + 30);
                popup.titleLabel.SetY((26 - metrics.height) / 2 + 13);
                popup.titleLabel.FixedSize = new Size(metrics.width, metrics.height);
                popup.titleLabel.Text = uiPrompt.wndTitle;

                Tig.Fonts.PopFont();
            }
            else
            {
                popup.titleLabel.Visible = false;
            }

            if (uiPrompt.bodyText != null)
            {
                popup.bodyLabel.Visible = true;
                popup.bodyLabel.SetX(popup.prompt.textRect.X);
                popup.bodyLabel.SetY(popup.prompt.textRect.Y);
                popup.bodyLabel.FixedSize = popup.prompt.textRect.Size;
                popup.bodyLabel.Text = uiPrompt.bodyText;
            }
            else
            {
                popup.bodyLabel.Visible = true;
            }

            if (uiPromptIdx == 0)
            {
                popup.btn1.SetText(popup.prompt.okButtonText);
                popup.btn1.Rectangle = popup.prompt.okRect;
                popup.btn1.SetStyle(uiPrompt.OkayButtonStyle);
                popup.btn1.SetVisible(true);
                popup.btn2.SetVisible(false);
                popup.btn3.SetVisible(false);
            }
            else if (uiPromptIdx == 1)
            {
                popup.btn1.SetStyle(uiPrompt.OkayButtonStyle);
                popup.btn1.SetVisible(true);
                popup.btn1.SetText(popup.prompt.okButtonText);
                popup.btn1.Rectangle = popup.prompt.okRect;

                popup.btn2.SetStyle(uiPrompt.CancelButtonStyle);
                popup.btn2.SetVisible(true);
                popup.btn2.SetText(popup.prompt.cancelButtonText);
                popup.btn2.Rectangle = popup.prompt.cancelRect;

                popup.btn3.SetVisible(false);
            }
            else if (uiPromptIdx == 2)
            {
                popup.btn1.SetStyle(uiPrompt.OkayButtonStyle);
                popup.btn1.SetVisible(true);
                popup.btn2.SetStyle(uiPrompt.CancelButtonStyle);
                popup.btn2.SetVisible(true);
                popup.btn3.SetVisible(true);
                throw new NotImplementedException();
            }

            popup.wnd.SetVisible(true);
            popup.wnd.BringToFront();
            popup.wnd.CenterOnScreen();

            popup.prompt.onPopupShow?.Invoke();
        }

        #region vanilla_ui

        [TempleDllLocation(0x10c0c4f8)]
        private Rectangle _vanillaWindow = new Rectangle(254, 195, 291, 210);

        [TempleDllLocation(0x10c0c508)]
        private Rectangle _vanillaOk = new Rectangle(28, 170, 112, 22);

        [TempleDllLocation(0x10c0c518)]
        private Rectangle _vanillaCancel = new Rectangle(152, 170, 112, 22);

        [TempleDllLocation(0x10c0c528)]
        private Rectangle _vanillaText = new Rectangle(23, 42, 245, 121);

        [TempleDllLocation(0x10c0c538)]
        private Rectangle _vanillaTextEntry = new Rectangle(254, 195, 291, 210);

        private int _vanillaTextEntryMaxWidth = 242;

        [TempleDllLocation(0x1017cf20)]
        public void ConfirmBox(string bodyText, string title, bool yesNoButtons, Action<int> callback, int flags = 0)
        {
            UiPromptPacket uiPrompt = new UiPromptPacket();
            uiPrompt.idx = 1;

            uiPrompt.image = GameSystems.UiArtManager.GetGenericTiledImagePath(1);

            uiPrompt.OkayButtonStyle = Globals.WidgetButtonStyles.GetStyle("accept-button");
            uiPrompt.CancelButtonStyle = Globals.WidgetButtonStyles.GetStyle("cancel-button");

            uiPrompt.callback = callback;

            uiPrompt.wndRect = _vanillaWindow;

            uiPrompt.okRect = _vanillaOk;

            if (yesNoButtons)
            {
                uiPrompt.okButtonText = _vanillaTranslations[0];
            }
            else
            {
                uiPrompt.okButtonText = _vanillaTranslations[2];
            }

            uiPrompt.cancelRect = _vanillaCancel;
            if (yesNoButtons)
            {
                uiPrompt.cancelButtonText = _vanillaTranslations[1];
            }
            else
            {
                uiPrompt.cancelButtonText = _vanillaTranslations[3];
            }

            uiPrompt.wndTitle = title;
            uiPrompt.textRect = _vanillaText;
            uiPrompt.bodyText = bodyText;
            UiPopupShow_Impl(uiPrompt, 1, flags);
        }

        [TempleDllLocation(0x1017d110)]
        public void RequestTextEntry(string body, string title, Action<string, bool> callback)
        {
            var crNamePkt = new UiCreateNamePacket();
            crNamePkt.okBtnText = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.ok);
            crNamePkt.cancelBtnText = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.cancel);
            crNamePkt.bodyText = body;
            crNamePkt.title = title;
            crNamePkt.type_or_flags = 2;
            crNamePkt.wndX = _vanillaTextEntry.X;
            crNamePkt.wndY = _vanillaTextEntry.Y;
            crNamePkt.callback = callback;
            UiSystems.TextDialog.UiTextDialogInit(crNamePkt);
            UiSystems.TextDialog.UiTextDialogShow();
        }

        #endregion
    }

    public class UiPromptPacket
    {
        public int idx = -1;
        public int styleIdx;
        public string bodyText;
        public Rectangle textRect;
        public string wndTitle;
        public string image; // Path to .img file
        public WidgetButtonStyle OkayButtonStyle;
        public WidgetButtonStyle CancelButtonStyle;
        public string texture2;
        public string texture5;
        public string texture8;
        public string field50;
        public Action onPopupShow;
        public Action onPopupHide;
        public Action<int> callback;
        public Rectangle wndRect;
        public Rectangle okRect;
        public string okButtonText;
        public Rectangle cancelRect;
        public string cancelButtonText;
        public Rectangle textEntryRect;
        public int unkA8;
        public int unkAC;
        public int unkB0;

        [TempleDllLocation(0x101709e0)]
        public UiPromptPacket()
        {
        }
    }
}