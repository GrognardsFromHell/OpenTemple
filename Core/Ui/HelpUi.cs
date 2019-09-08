using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.Systems.RollHistory;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui
{
    public class HelpUi
    {
        [TempleDllLocation(0x10be2e84)]
        private bool uiHelpIsVisible;

        [TempleDllLocation(0x10130300)]
        public bool IsVisible => uiHelpIsVisible;

        [TempleDllLocation(0x10be2c20)]
        private HelpRequest _currentHelpRequest;

        [TempleDllLocation(0x10be2e74)]
        private WidgetContainer uiHelpWnd;

        [TempleDllLocation(0x10be2ce0)]
        private WidgetButton uiHelpBackButton;

        [TempleDllLocation(0x10be2e14)]
        private WidgetButton uiHelpForwardButton;

        [TempleDllLocation(0x10be2ce4)]
        private WidgetButton uiHelpHomeButton;

        [TempleDllLocation(0x10be2e70)]
        private WidgetButton uiHelpPrevButton;

        [TempleDllLocation(0x10be2e28)]
        private WidgetButton uiHelpUpButton;

        [TempleDllLocation(0x10be2ce8)]
        private WidgetButton uiHelpNextButton;

        [TempleDllLocation(0x10be2ccc)]
        private WidgetButton uiHelpExitButton;

        [TempleDllLocation(0x10be2e88)]
        private string helpWindowTitle;

        [TempleDllLocation(0x10be2e78)]
        private ScrollBox _bodyScrollBox;

        [TempleDllLocation(0x101317f0)]
        public HelpUi()
        {
            uiHelpWnd = new WidgetContainer(new Rectangle(92, 7, 460, 505));
            // uiHelpWnd.OnHandleMessage += 0x10131030;
            // uiHelpWnd.OnBeforeRender += 0x101310c0;
            uiHelpWnd.ZIndex = 99800;
            uiHelpWnd.Name = "help_main_window";
            uiHelpWnd.SetVisible(false);

            uiHelpBackButton = new WidgetButton(new Rectangle(56, 443, 52, 19));
            // help_back_button1.OnHandleMessage += 0x10130ca0;
            // help_back_button1.OnBeforeRender += 0x101311f0;
            uiHelpBackButton.Name = "help_back_button";
            uiHelpWnd.Add(uiHelpBackButton);

            // Created @ 0x101f96fe
            uiHelpForwardButton = new WidgetButton(new Rectangle(113, 443, 52, 19));
            // uiHelpForwardButton.OnHandleMessage += 0x10130cd0;
            // uiHelpForwardButton.OnBeforeRender += 0x10131250;
            uiHelpForwardButton.Name = "help_forward_button";
            uiHelpWnd.Add(uiHelpForwardButton);

            // Created @ 0x101f96fe
            uiHelpHomeButton = new WidgetButton(new Rectangle(170, 443, 52, 19));
            // uiHelpHomeButton.OnHandleMessage += 0x10130d00;
            // uiHelpHomeButton.OnBeforeRender += 0x101312b0;
            uiHelpHomeButton.Name = "help_home_button";
            uiHelpWnd.Add(uiHelpHomeButton);

            // Created @ 0x101f96fe
            uiHelpPrevButton = new WidgetButton(new Rectangle(227, 443, 52, 19));
            // uiHelpPrevButton.OnHandleMessage += 0x10130d40;
            // uiHelpPrevButton.OnBeforeRender += 0x101312d0;
            uiHelpPrevButton.Name = "help_prev_button";
            uiHelpWnd.Add(uiHelpPrevButton);

            // Created @ 0x101f96fe
            var uiHelpUpButton = new WidgetButton(new Rectangle(284, 443, 52, 19));
            // uiHelpUpButton.OnHandleMessage += 0x10130d70;
            // uiHelpUpButton.OnBeforeRender += 0x10131330;
            uiHelpUpButton.Name = "help_up_button";
            uiHelpWnd.Add(uiHelpUpButton);

            // Created @ 0x101f96fe
            uiHelpNextButton = new WidgetButton(new Rectangle(341, 443, 52, 19));
            // uiHelpNextButton.OnHandleMessage += 0x10130da0;
            // uiHelpNextButton.OnBeforeRender += 0x10131390;
            uiHelpNextButton.Name = "help_next_button";
            uiHelpWnd.Add(uiHelpNextButton);

            // Created @ 0x101f96fe
            uiHelpExitButton = new WidgetButton(new Rectangle(399, 445, 55, 52));
            // uiHelpExitButton.OnHandleMessage += 0x10130dd0;
            // uiHelpExitButton.OnBeforeRender += 0x101313f0;
            uiHelpExitButton.Name = "help_exit_button";
            uiHelpWnd.Add(uiHelpExitButton);

            var settings = new ScrollBoxSettings();
            settings.TextArea = new Rectangle(0, 16, 380, 374);
            settings.ScrollBarPos = new Point(513, 44);
            settings.ScrollBarHeight = 398;
            settings.Indent = 100;
            settings.Font = PredefinedFont.ARIAL_10;
            _bodyScrollBox = new ScrollBox(new Rectangle(35, 37, 400, 398), settings);

/*
            // Begin top level window
            // Created @ 0x1019d9d4
            var alert_main_window1 = new WidgetContainer(new Rectangle(0, 0, 1024, 768));
            // alert_main_window1.OnHandleMessage += 0x1019d710;
            // alert_main_window1.OnBeforeRender += 0x1019d7a0;
            alert_main_window1.ZIndex = 99800;
            alert_main_window1.Name = "alert_main_window";
            alert_main_window1.SetVisible(false);
            // Created @ 0x101f96fe
            var alert_ok_button1 = new WidgetButton(new Rectangle(345, 370, 112, 22));
            // alert_ok_button1.OnHandleMessage += 0x1019d270;
            // alert_ok_button1.OnBeforeRender += 0x1019d8e0;
            alert_ok_button1.Name = "alert_ok_button";
            alert_main_window1.Add(alert_ok_button1);
            // End top level window

            // Begin top level window
            // Created @ 0x1018d96c
            var scrollbox_main_window2 = new WidgetContainer(new Rectangle(245, 136, 310, 226));
            // scrollbox_main_window2.OnHandleMessage += 0x1018d720;
            // scrollbox_main_window2.OnBeforeRender += 0x1018d840;
            scrollbox_main_window2.ZIndex = 99950;
            scrollbox_main_window2.Name = "scrollbox_main_window";
            scrollbox_main_window2.SetVisible(false);
            // Created @ 0x1018d9da
            // var @ [TempleDllLocation(0x16b8396c)]
            var 2 = new WidgetScrollbar(new Rectangle(297, 1, 13, 224));
            // 2.OnHandleMessage += 0x101fa410;
            // 2.OnBeforeRender += 0x101fa1b0;
            scrollbox_main_window2.Add(2);
            // End top level window
*/
        }

        [TempleDllLocation(0x10130670)]
        public void Show(HelpRequest request, bool hideUis)
        {
            _currentHelpRequest = request;
            Show(hideUis);
        }

        [TempleDllLocation(0x10130310)]
        public void Show(bool hideUis)
        {
            if (hideUis)
            {
                UiSystems.UtilityBar.HideOpenedWindows(true);
            }

            uiHelpIsVisible = true;
            uiHelpWnd.SetVisible(false);
            uiHelpWnd.BringToFront();

            if (_currentHelpRequest == null)
            {
                return;
            }

            switch (_currentHelpRequest.Type)
            {
                case HelpRequestType.HelpTopic:
                    var topic = _currentHelpRequest.Topic ?? GameSystems.Help.RootTopic;

                    _bodyScrollBox.Clear();
                    _bodyScrollBox.DontAutoScroll = true;
                    _bodyScrollBox.Indent = 15;
                    helpWindowTitle = topic.Title;

                    _bodyScrollBox.SetEntries(new List<D20RollHistoryLine>
                    {
                        new D20RollHistoryLine("\n\n", new List<D20HelpLink>()),
                        new D20RollHistoryLine(topic.Text, topic.Links)
                    });
                    break;

                case HelpRequestType.RollHistoryEntry:
                    var entry = _currentHelpRequest.RollHistoryEntry;
                    _bodyScrollBox.Clear();
                    _bodyScrollBox.DontAutoScroll = true;
                    _bodyScrollBox.Indent = 100;
                    helpWindowTitle = entry.Title;

                    var builder = new StringBuilder();
                    entry.FormatLong(builder);
                    _bodyScrollBox.SetEntries(new List<D20RollHistoryLine>
                    {
                        D20RollHistoryLine.Create(builder.ToString())
                    });

                    break;

                case HelpRequestType.Custom:
                    _bodyScrollBox.Clear();
                    _bodyScrollBox.DontAutoScroll = true;
                    _bodyScrollBox.Indent = 15;
                    helpWindowTitle = _currentHelpRequest.CustomHeader;
                    _bodyScrollBox.SetEntries(new List<D20RollHistoryLine>
                    {
                        D20RollHistoryLine.Create(_currentHelpRequest.CustomBody)
                    });
                    break;

                default:
                    return;
            }

            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            if (GameSystems.Help.CanNavigateBackward)
            {
                uiHelpBackButton.SetDisabled(false);
                // TODO uiHelpBackButton.field98 &= ~0x20;
            }
            else
            {
                uiHelpBackButton.SetDisabled(true);
                // TODO uiHelpBackButton.field98 |= 0x20;
            }

            if (GameSystems.Help.CanNavigateForward)
            {
                uiHelpForwardButton.SetDisabled(false);
                // TODO uiHelpForwardButton.field98 &= ~0x20;
            }
            else
            {
                uiHelpForwardButton.SetDisabled(true);
                // TODO uiHelpForwardButton.field98 |= 0x20;
            }

            if (GameSystems.Help.CanNavigateUp)
            {
                uiHelpUpButton.SetDisabled(false);
                // TODO uiHelpUpButton.field98 &= ~0x20;
            }
            else
            {
                uiHelpUpButton.SetDisabled(true);
                // TODO uiHelpUpButton.field98 |= 0x20;
            }

            if (GameSystems.Help.CanNavigateToPreviousSibling)
            {
                uiHelpPrevButton.SetDisabled(false);
                // TODO uiHelpPrevButton.field98 &= ~0x20;
            }
            else
            {
                uiHelpPrevButton.SetDisabled(true);
                // TODO uiHelpPrevButton.field98 |= 0x20;
            }

            if (GameSystems.Help.CanNavigateToNextSibling)
            {
                uiHelpNextButton.SetDisabled(false);
                // TODO uiHelpNextButton.field98 &= ~0x20;
            }
            else
            {
                uiHelpNextButton.SetDisabled(true);
                // TODO uiHelpNextButton.field98 |= 0x20;
            }
        }

        [TempleDllLocation(0x10130640)]
        public void Hide()
        {
        }
    }
}