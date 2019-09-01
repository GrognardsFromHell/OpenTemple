using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
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
        }

        [TempleDllLocation(0x10171510)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10171e70)]
        public void Reset()
        {
            Stub.TODO();
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

        [TempleDllLocation(0x10C03BD8)]
        private UiPromptListEntry[] uiPopups = new UiPromptListEntry[5];

        private struct UiPromptListEntry
        {
            public int flags;
            public bool isActive;
            public WidgetContainer wnd;
            public WidgetButton btn1;
            public WidgetButton btn2;
            public WidgetButton btn3;
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

        [TempleDllLocation(0x10C03BB8)]
        private Rectangle stru_10C03BB8;

        [TempleDllLocation(0x10C03BC8)]
        private Rectangle stru_10C03BC8;

        [TempleDllLocation(0x10C04018)]
        private Rectangle stru_10C04018;

        [TempleDllLocation(0x10C03FE4)]
        private Rectangle stru_10C03FE4;

        [TempleDllLocation(0x10C03FF4)]
        private Rectangle stru_10C03FF4;

        [TempleDllLocation(0x10c04008)]
        private Rectangle uiPromptRect;

        [TempleDllLocation(0x10171580)]
        public void UiPopupShow_Impl(UiPromptPacket uiPrompt, int uiPromptIdx, int flags)
        {
            uiPopups[uiPromptIdx].prompt = uiPrompt;
            uiPopupCurrent = uiPromptIdx;
            uiPopups[uiPromptIdx].isActive = true;
            UiPopupAddToList(uiPromptIdx, flags, uiPrompt);
            var styleIndex = uiPopups[uiPromptIdx].prompt.styleIdx;
            var popupStyle = uipopupstyles[styleIndex];

            if (uiPrompt.wndTitle != null)
            {
                Tig.Fonts.PushFont(popupStyle.Font);
                var metrics = new TigFontMetrics();
                Tig.Fonts.Measure(popupStyle.TextStyle, uiPrompt.wndTitle, ref metrics);
                stru_10C03FD4.X = (230 - metrics.width) / 2 + 30;
                stru_10C03FD4.Y = (26 - metrics.height) / 2 + 13;
                stru_10C03FD4.Width = metrics.width;
                stru_10C03FD4.Height = metrics.height;
                Tig.Fonts.PopFont();
            }

            if (uiPromptIdx == 0)
            {
                uiPopups[uiPromptIdx].btn1.SetVisible(true);
                uiPopups[uiPromptIdx].btn2.SetVisible(false);
                uiPopups[uiPromptIdx].btn3.SetVisible(false);
            }
            else if (uiPromptIdx == 1)
            {
                stru_10C03BB8 = new Rectangle(new Point(1, 1), uiPopups[uiPromptIdx].prompt.okRect.Size);
                stru_10C03BC8 = uiPopups[uiPromptIdx].prompt.okRect;

                if (uiPopups[uiPromptIdx].prompt.okButtonText != null)
                {
                    Tig.Fonts.PushFont(popupStyle.Font);
                    var metrics = new TigFontMetrics();
                    Tig.Fonts.Measure(popupStyle.TextStyle, uiPopups[uiPromptIdx].prompt.okButtonText, ref metrics);
                    stru_10C04018.X = stru_10C03BC8.X + (stru_10C03BC8.Width - metrics.width) / 2 -
                                      uiPopups[uiPromptIdx].prompt.wndRect.X;
                    stru_10C04018.Y = stru_10C03BC8.Y + (stru_10C03BC8.Height - metrics.height) / 2 -
                                      uiPopups[uiPromptIdx].prompt.wndRect.Y;
                    stru_10C04018.Width = metrics.width;
                    stru_10C04018.Height = metrics.height;
                    Tig.Fonts.PopFont();
                }

                stru_10C03FF4 = new Rectangle(new Point(1, 1), uiPopups[uiPromptIdx].prompt.cancelRect.Size);
                stru_10C03FE4 = uiPopups[uiPromptIdx].prompt.cancelRect;

                if (uiPopups[uiPromptIdx].prompt.cancelButtonText != null)
                {
                    Tig.Fonts.PushFont(popupStyle.Font);
                    var metrics = new TigFontMetrics();
                    Tig.Fonts.Measure(popupStyle.TextStyle, uiPopups[uiPromptIdx].prompt.cancelButtonText, ref metrics);
                    uiPromptRect.X = stru_10C03FE4.X + (stru_10C03FE4.Width - metrics.width) / 2 -
                                     uiPopups[uiPromptIdx].prompt.wndRect.X;
                    uiPromptRect.Y = stru_10C03FE4.Y + (stru_10C03FE4.Height - metrics.height) / 2 -
                                     uiPopups[uiPromptIdx].prompt.wndRect.Y;
                    uiPromptRect.Width = metrics.width;
                    uiPromptRect.Height = metrics.height;
                    Tig.Fonts.PopFont();
                }

                uiPopups[uiPromptIdx].btn1.SetVisible(true);
                uiPopups[uiPromptIdx].btn2.SetVisible(true);
                uiPopups[uiPromptIdx].btn3.SetVisible(false);
            }
            else if (uiPromptIdx == 2)
            {
                uiPopups[uiPromptIdx].btn1.SetVisible(true);
                uiPopups[uiPromptIdx].btn2.SetVisible(true);
                uiPopups[uiPromptIdx].btn3.SetVisible(true);
            }

            uiPopups[uiPromptIdx].wnd.SetVisible(true);
            uiPopups[uiPromptIdx].wnd.BringToFront();

            uiPopups[uiPromptIdx].prompt.onPopupShow?.Invoke();
        }

        #region vanilla_ui

        [TempleDllLocation(0x10c0c4f8)]
        private Rectangle _vanillaWindow = new Rectangle(254, 195, 291, 210);

        [TempleDllLocation(0x10c0c508)]
        private Rectangle _vanillaOk = new Rectangle(283, 366, 110, 21);

        [TempleDllLocation(0x10c0c518)]
        private Rectangle _vanillaCancel = new Rectangle(407, 366, 110, 21);

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
            uiPrompt.texture0 = GameSystems.UiArtManager.GetGenericPath(1);
            uiPrompt.btnHoverTexture = GameSystems.UiArtManager.GetGenericPath(0);
            uiPrompt.btnPressedTexture = GameSystems.UiArtManager.GetGenericPath(2);
            uiPrompt.btnDisabledTexture = GameSystems.UiArtManager.GetGenericPath(4);
            uiPrompt.texture1 = GameSystems.UiArtManager.GetGenericPath(4);
            uiPrompt.texture4 = GameSystems.UiArtManager.GetGenericPath(3);
            uiPrompt.texture7 = GameSystems.UiArtManager.GetGenericPath(5);
            uiPrompt.texture10 = GameSystems.UiArtManager.GetGenericPath(4);

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
        public string texture0;
        public string texture1;
        public string texture2;
        public string btnHoverTexture;
        public string texture4;
        public string texture5;
        public string btnPressedTexture;
        public string texture7;
        public string texture8;
        public string btnDisabledTexture;
        public string texture10;
        public string field50;
        public Action onPopupShow;
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