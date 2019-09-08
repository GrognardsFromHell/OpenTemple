using System.Drawing;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.UtilityBar
{
    public class UtilityBarUi
    {
        private readonly UtilityBarHistoryUi _historyUi = new UtilityBarHistoryUi();

        [TempleDllLocation(0x10bd3120)] [TempleDllLocation(0x10bd2ee8)]
        private readonly WidgetContainer _container;

        public UtilityBarUi()
        {
            // Begin top level window
            // Created @ 0x10110f7f
            _container = new WidgetContainer(new Rectangle(846, 685, 179, 81));
            // _container.OnBeforeRender += 0x10110ea0;
            _container.SetWidgetMsgHandler(OnUtilityBarClick);
            _container.ZIndex = 100000;
            _container.SetVisible(false);
            var background = new WidgetImage("art/interface/utility_bar_ui/background.tga");
            // Vanilla used a srcrect with x=1,y=1
            background.SetY(-1);
            background.SetX(-1);
            _container.AddContent(background);

            // Created @ 0x101105c9
            var selectAllButton = new WidgetButton(new Rectangle(9, 5, 41, 65));
            selectAllButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/selectparty.tga",
                hoverImagePath = "art/interface/utility_bar_ui/selectparty_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/selectparty_click.tga"
            });
            // selectAllButton.OnBeforeRender += 0x1010f860;
            // selectAllButton.OnRenderTooltip += 0x1010f990;
            selectAllButton.SetClickHandler(OnSelectAllButtonClick);
            _container.Add(selectAllButton);

            // Created @ 0x101106f9
            var formationButton = new WidgetButton(new Rectangle(51, 5, 19, 41));
            formationButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/formation.tga",
                hoverImagePath = "art/interface/utility_bar_ui/formation_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/formation_click.tga"
            });
            // formationButton.OnBeforeRender += 0x1010fa00;
            // formationButton.OnRenderTooltip += 0x1010fae0;
            formationButton.SetClickHandler(OnFormationButtonClick);
            _container.Add(formationButton);

            // Created @ 0x10110829
            var logbookButton = new WidgetButton(new Rectangle(71, 5, 19, 41));
            logbookButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/logbook.tga",
                hoverImagePath = "art/interface/utility_bar_ui/logbook_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/logbook_click.tga"
            });
            // logbookButton.OnBeforeRender += 0x1010fb50;
            // logbookButton.OnRenderTooltip += 0x1010fd00;
            logbookButton.SetClickHandler(OnLogbookButtonClick);
            _container.Add(logbookButton);

            // Created @ 0x10110959
            var mapButton = new WidgetButton(new Rectangle(91, 5, 19, 41));
            mapButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/townmap.tga",
                hoverImagePath = "art/interface/utility_bar_ui/townmap_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/townmap_click.tga"
            });
            // mapButton.OnBeforeRender += 0x1010fd70;
            // mapButton.OnRenderTooltip += 0x1010ff60;
            mapButton.SetClickHandler(OnMapButtonClick);
            _container.Add(mapButton);

            // Created @ 0x10110a89
            var restButton = new WidgetButton(new Rectangle(111, 5, 19, 41));
            restButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/camp.tga",
                hoverImagePath = "art/interface/utility_bar_ui/camp_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/camp_click.tga"
            });
            // restButton.OnBeforeRender += 0x1010ffd0;
            // restButton.OnRenderTooltip += 0x101100f0;
            restButton.SetClickHandler(OnRestButtonClick);
            _container.Add(restButton);

            // Created @ 0x10110cec
            var helpButton = new WidgetButton(new Rectangle(131, 5, 19, 41));
            helpButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/help.tga",
                hoverImagePath = "art/interface/utility_bar_ui/help_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/help_click.tga"
            });
            // 7.OnBeforeRender += 0x10110170;
            // 7.OnRenderTooltip += 0x10110250;
            helpButton.SetClickHandler(OnHelpButtonClick);
            _container.Add(helpButton);

            // Created @ 0x10110bbc
            var optionsButton = new WidgetButton(new Rectangle(151, 5, 19, 41));
            optionsButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/options.tga",
                hoverImagePath = "art/interface/utility_bar_ui/options_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/options_click.tga"
            });
            // optionsButton.OnBeforeRender += 0x101102c0;
            // optionsButton.OnRenderTooltip += 0x10110390;
            optionsButton.SetClickHandler(OnOptionsButtonClick);
            _container.Add(optionsButton);

            // Created @ 0x10110e15
            var timeWidget = new UtilityBarClock(new Rectangle(52, 48, 117, 21));
            _container.Add(timeWidget);
        }

        [TempleDllLocation(0x1010f820)]
        private bool OnUtilityBarClick(MessageWidgetArgs msg)
        {
            if (msg.widgetEventType == TigMsgWidgetEvent.MouseReleased)
            {
                if (UiSystems.HelpManager.IsSelectingHelpTarget)
                {
                    UiSystems.HelpManager.ShowPredefinedTopic(63);
                }
            }

            return true;
        }

        [TempleDllLocation(0x1010f8e0)]
        private void OnSelectAllButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(63);
                return;
            }

            if (GameSystems.Combat.IsCombatActive())
            {
                if (GameSystems.D20.Actions.curSeqGetTurnBasedStatus() != null)
                {
                    var actor = GameSystems.D20.Initiative.CurrentActor;
                    if (actor != null)
                    {
                        var location = actor.GetLocation();
                        GameSystems.Location.CenterOn(location.locx, location.locy);
                    }
                }
            }
            else
            {
                GameSystems.Party.ClearSelection();
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    GameSystems.Party.AddToSelection(partyMember);
                }
            }
        }

        [TempleDllLocation(0x1010fa80)]
        private void OnFormationButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(20);
                return;
            }

            if (UiSystems.Formation.IsVisible)
            {
                UiSystems.Formation.Hide();
            }
            else
            {
                UiSystems.Formation.Show();
            }
        }

        [TempleDllLocation(0x1010fca0)]
        private void OnLogbookButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(26);
                return;
            }

            if (UiSystems.Logbook.IsVisible)
            {
                UiSystems.Logbook.Hide();
            }
            else
            {
                UiSystems.Logbook.Show();
            }
        }

        [TempleDllLocation(0x1010ff00)]
        private void OnMapButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(61);
                return;
            }

            if (UiSystems.TownMap.IsVisible)
            {
                UiSystems.TownMap.Hide();
            }
            else
            {
                UiSystems.TownMap.Show();
            }
        }

        [TempleDllLocation(0x10110080)]
        private void OnRestButtonClick()
        {
            GameSystems.RandomEncounter.UpdateSleepStatus();
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(1);
                return;
            }

            if (GameSystems.RandomEncounter.SleepStatus != 2)
            {
                if (UiSystems.Camping.IsHidden)
                {
                    UiSystems.Camping.Show();
                }
                else
                {
                    UiSystems.Camping.Hide();
                }
            }
        }

        [TempleDllLocation(0x101101f0)]
        private void OnHelpButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(21);
                return;
            }

            if (UiSystems.Help.IsVisible)
            {
                UiSystems.Help.Hide();
            }
            else
            {
                GameSystems.Help.ShowTopic(GameSystems.Help.RootTopic.Id);
            }
        }

        [TempleDllLocation(0x10110340)]
        private void OnOptionsButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(38);
                return;
            }

            UiSystems.Options.Show(false);
        }

        [TempleDllLocation(0x1010eec0)]
        [TemplePlusLocation("ui_utility_bar.cpp:18")]
        public void Hide()
        {
            _container.SetVisible(false);
            _historyUi.Hide();
        }

        [TempleDllLocation(0x101156b0)]
        public void HideOpenedWindows(bool b)
        {
            // TODO  throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10bd33f8)]
        public bool IsVisible() => _container.IsVisible();

        [TempleDllLocation(0x1010ee80)]
        [TemplePlusLocation("ui_utility_bar.cpp:12")]
        public void Show()
        {
            _container.SetVisible(true);
            _container.BringToFront(); // TODO: Fishy
            _historyUi.Show();
        }
    }
}