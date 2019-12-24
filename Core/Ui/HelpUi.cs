using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public class HelpUi : IResetAwareSystem
    {
        [TempleDllLocation(0x10be2e84)]
        [TempleDllLocation(0x10130300)]
        public bool IsVisible => uiHelpWnd.Visible;

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
        private string helpWindowTitle
        {
            set => _titleLabel.Text = value;
        }

        [TempleDllLocation(0x10be2e78)]
        private ScrollBox _bodyScrollBox;

        private WidgetLegacyText _titleLabel;

        [TempleDllLocation(0x101317f0)]
        public HelpUi()
        {
            //  Original render @ 0x101310c0, message @ 0x10131030
            uiHelpWnd = new WidgetContainer(new Rectangle(92, 7, 462, 507));
            uiHelpWnd.ZIndex = 99800;
            uiHelpWnd.Name = "help_main_window";
            uiHelpWnd.SetKeyStateChangeHandler(args =>
            {
                if (!args.down && args.key == DIK.DIK_ESCAPE)
                {
                    Hide();
                }

                return true;
            });
            uiHelpWnd.Visible = false;
            uiHelpWnd.SetMouseMsgHandler(msg => true); // Dont allow click-through

            var background = new WidgetImage("art/interface/HELP_UI/helpmenu_background.img");
            uiHelpWnd.AddContent(background);

            // This is the topic title displayed in the window's header bar
            var titleStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
            {
                flags = TigTextStyleFlag.TTSF_CENTER,
                kerning = 1,
                tracking = 3
            };
            _titleLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_12, titleStyle);
            _titleLabel.SetX(25);
            _titleLabel.SetY(10);
            _titleLabel.FixedSize = new Size(410, 18);
            uiHelpWnd.AddContent(_titleLabel);

            uiHelpBackButton = new WidgetButton(new Rectangle(56, 443, 52, 19));
            uiHelpBackButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/HelpMenu_Button_Back_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/HelpMenu_Button_Back_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/HelpMenu_Button_Back_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/HelpMenu_Button_Back_Grey.tga"
            });
            uiHelpBackButton.SetClickHandler(() => GameSystems.Help.NavigateBackward());
            uiHelpBackButton.Name = "help_back_button";
            uiHelpWnd.Add(uiHelpBackButton);

            // Created @ 0x101f96fe
            uiHelpForwardButton = new WidgetButton(new Rectangle(113, 443, 52, 19));
            uiHelpForwardButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/HelpMenu_Button_Forward_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/HelpMenu_Button_Forward_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/HelpMenu_Button_Forward_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/HelpMenu_Button_Forward_Grey.tga"
            });
            uiHelpForwardButton.SetClickHandler(() => GameSystems.Help.NavigateForward());
            uiHelpForwardButton.Name = "help_forward_button";
            uiHelpWnd.Add(uiHelpForwardButton);

            // Created @ 0x101f96fe
            uiHelpHomeButton = new WidgetButton(new Rectangle(170, 443, 52, 19));
            uiHelpHomeButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/HelpMenu_Button_Home_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/HelpMenu_Button_Home_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/HelpMenu_Button_Home_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/HelpMenu_Button_Home_Grey.tga"
            });
            uiHelpHomeButton.SetClickHandler(() => GameSystems.Help.ShowTopic(GameSystems.Help.RootTopic.Id));
            uiHelpHomeButton.Name = "help_home_button";
            uiHelpWnd.Add(uiHelpHomeButton);

            // Created @ 0x101f96fe
            uiHelpPrevButton = new WidgetButton(new Rectangle(227, 443, 52, 19));
            uiHelpPrevButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/HelpMenu_Button_PreviousPage_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/HelpMenu_Button_PreviousPage_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/HelpMenu_Button_PreviousPage_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/HelpMenu_Button_PreviousPage_Grey.tga"
            });
            uiHelpPrevButton.SetClickHandler(() => GameSystems.Help.NavigateToPreviousSibling());
            uiHelpPrevButton.Name = "help_prev_button";
            uiHelpWnd.Add(uiHelpPrevButton);

            // Created @ 0x101f96fe
            uiHelpUpButton = new WidgetButton(new Rectangle(284, 443, 52, 19));
            uiHelpUpButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/HelpMenu_Button_UpPage_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/HelpMenu_Button_UpPage_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/HelpMenu_Button_UpPage_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/HelpMenu_Button_UpPage_Grey.tga"
            });
            uiHelpUpButton.SetClickHandler(() => GameSystems.Help.NavigateUp());
            uiHelpUpButton.Name = "help_up_button";
            uiHelpWnd.Add(uiHelpUpButton);

            // Created @ 0x101f96fe
            uiHelpNextButton = new WidgetButton(new Rectangle(341, 443, 52, 19));
            uiHelpNextButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/HelpMenu_Button_NextPage_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/HelpMenu_Button_NextPage_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/HelpMenu_Button_NextPage_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/HelpMenu_Button_NextPage_Grey.tga"
            });
            uiHelpNextButton.SetClickHandler(() => GameSystems.Help.NavigateToNextSibling());
            uiHelpNextButton.Name = "help_next_button";
            uiHelpWnd.Add(uiHelpNextButton);

            // Created @ 0x101f96fe
            uiHelpExitButton = new WidgetButton(new Rectangle(399, 445, 55, 52));
            uiHelpExitButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/HELP_UI/main_exit_button_Unselected.tga",
                hoverImagePath = "art/interface/HELP_UI/main_exit_button_Hover.tga",
                pressedImagePath = "art/interface/HELP_UI/main_exit_button_Click.tga",
                disabledImagePath = "art/interface/HELP_UI/main_exit_button_Grey.tga"
            });
            uiHelpExitButton.SetClickHandler(Hide);
            uiHelpExitButton.Name = "help_exit_button";
            uiHelpWnd.Add(uiHelpExitButton);

            var settings = new ScrollBoxSettings();
            settings.TextArea = new Rectangle(0, 16, 380, 374);
            settings.ScrollBarPos = new Point(386, 0);
            settings.ScrollBarHeight = 398;
            settings.Indent = 100;
            settings.Font = PredefinedFont.ARIAL_10;
            _bodyScrollBox = new ScrollBox(new Rectangle(35, 37, 400, 398), settings);
            uiHelpWnd.Add(_bodyScrollBox);
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
                UiSystems.HideOpenedWindows(true);
            }

            uiHelpWnd.Visible = true;
            uiHelpWnd.BringToFront();
            uiHelpWnd.CenterOnScreen();

            if (_currentHelpRequest == null)
            {
                return;
            }

            SetContent(_bodyScrollBox, _currentHelpRequest, out var windowTitle);
            helpWindowTitle = windowTitle;
            UpdateNavigationButtons();
        }

        internal static void SetContent(ScrollBox scrollBox, HelpRequest request, out string title)
        {
            switch (request.Type)
            {
                case HelpRequestType.HelpTopic:
                    var topic = request.Topic ?? GameSystems.Help.RootTopic;

                    title = topic.Title;
                    scrollBox.SetHelpContent(topic);
                    break;

                case HelpRequestType.RollHistoryEntry:
                    var entry = request.RollHistoryEntry;
                    scrollBox.ClearLines();
                    scrollBox.DontAutoScroll = true;
                    scrollBox.Indent = 100;
                    title = entry.Title;

                    var builder = new StringBuilder();
                    entry.FormatLong(builder);
                    scrollBox.SetEntries(new List<D20RollHistoryLine>
                    {
                        D20RollHistoryLine.Create(builder.ToString())
                    });

                    break;

                case HelpRequestType.Custom:
                    scrollBox.ClearLines();
                    scrollBox.DontAutoScroll = true;
                    scrollBox.Indent = 15;
                    title = request.CustomHeader;
                    scrollBox.SetEntries(new List<D20RollHistoryLine>
                    {
                        D20RollHistoryLine.Create(request.CustomBody)
                    });
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            uiHelpWnd.Visible = false;
        }

        [TempleDllLocation(0x10130f00)]
        public void Reset()
        {
            UiSystems.Alert.Hide();
            Hide();
        }
    }

    public static class ScrollBoxHelpExtensions
    {
        public static void SetHelpContent(this ScrollBox scrollBox, string topicId, bool includeTitle = false)
        {
            if (GameSystems.Help.TryGetTopic(topicId, out var topic))
            {
                scrollBox.SetHelpContent(topic, includeTitle);
            }
            else
            {
                scrollBox.ClearLines();
            }
        }

        public static void SetHelpContent(this ScrollBox scrollBox, D20HelpTopic topic, bool includeTitle = false)
        {
            scrollBox.ClearLines();
            scrollBox.DontAutoScroll = true;
            scrollBox.Indent = 15;
            var firstLine = includeTitle ? topic.Title + "\n\n" : "\n\n";

            scrollBox.SetEntries(new List<D20RollHistoryLine>
            {
                new D20RollHistoryLine(firstLine, new List<D20HelpLink>()),
                new D20RollHistoryLine(topic.Text, topic.Links)
            });
        }
    }
}