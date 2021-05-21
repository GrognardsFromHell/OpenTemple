using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.UtilityBar
{
    public class UtilityBarUi : ITimeAwareSystem, IResetAwareSystem
    {
        private readonly UtilityBarHistoryUi _historyUi = new UtilityBarHistoryUi();

        public UtilityBarHistoryUi HistoryUi => _historyUi;

        [TempleDllLocation(0x10bd3120)] [TempleDllLocation(0x10bd2ee8)]
        private readonly WidgetContainer _container;

        [TempleDllLocation(0x10bd347c)]
        private static readonly WidgetButtonStyle PassTimeButtonStyle = new WidgetButtonStyle
        {
            normalImagePath = "art/interface/utility_bar_ui/camp.tga",
            hoverImagePath = "art/interface/utility_bar_ui/camp_hover.tga",
            pressedImagePath = "art/interface/utility_bar_ui/camp_click.tga",
            disabledImagePath = "art/interface/utility_bar_ui/camp_clock_grey.tga"
        };

        [TempleDllLocation(0x10bd34d0)]
        private static readonly WidgetButtonStyle RestForbiddenButtonStyle = new WidgetButtonStyle
        {
            normalImagePath = "art/interface/utility_bar_ui/camp_red.tga",
            disabledImagePath = "art/interface/utility_bar_ui/camp_grey.tga"
        };

        [TempleDllLocation(0x10bd33d0)]
        private static readonly WidgetButtonStyle RestUnsafeButtonStyle = new WidgetButtonStyle
        {
            normalImagePath = "art/interface/utility_bar_ui/camp_yellow.tga",
            hoverImagePath = "art/interface/utility_bar_ui/camp_yellow_hover.tga",
            pressedImagePath = "art/interface/utility_bar_ui/camp_yellow_click.tga",
            disabledImagePath = "art/interface/utility_bar_ui/camp_grey.tga",
        };

        [TempleDllLocation(0x10bd2de4)]
        private static readonly WidgetButtonStyle RestSafeButtonStyle = new WidgetButtonStyle
        {
            normalImagePath = "art/interface/utility_bar_ui/camp_green.tga",
            hoverImagePath = "art/interface/utility_bar_ui/camp_green_hover.tga",
            pressedImagePath = "art/interface/utility_bar_ui/camp_green_click.tga",
            disabledImagePath = "art/interface/utility_bar_ui/camp_grey.tga",
        };

        [TempleDllLocation(0x10bd33f0)]
        private readonly Dictionary<int, string> _translations;

        private WidgetButton _restButton;

        private PulsingButton _mapButton;

        private PulsingButton _logbookButton;

        public UtilityBarUi()
        {
            _translations = Tig.FS.ReadMesFile("mes/utility_bar.mes");

            // Begin top level window
            // Created @ 0x10110f7f
            _container = new WidgetContainer(new Rectangle(0, 0, 179, 81));
            _container.SetWidgetMsgHandler(OnUtilityBarClick);
            _container.ZIndex = 100000;
            _container.Visible = false;
            var background = new WidgetImage("art/interface/utility_bar_ui/background.tga");
            background.SourceRect = new Rectangle(1, 1, 179, 81);
            _container.AddContent(background);

            // Created @ 0x101105c9
            var selectAllButton = new WidgetButton(new Rectangle(8, 4, 41, 65));
            selectAllButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/selectparty.tga",
                hoverImagePath = "art/interface/utility_bar_ui/selectparty_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/selectparty_click.tga"
            });
            selectAllButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            selectAllButton.TooltipText = _translations[10];
            selectAllButton.SetClickHandler(OnSelectAllButtonClick);
            _container.Add(selectAllButton);

            // Created @ 0x101106f9
            var formationButton = new WidgetButton(new Rectangle(50, 4, 19, 41));
            formationButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/formation.tga",
                hoverImagePath = "art/interface/utility_bar_ui/formation_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/formation_click.tga"
            });
            formationButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            formationButton.TooltipText = _translations[0];
            formationButton.SetClickHandler(OnFormationButtonClick);
            _container.Add(formationButton);

            // Created @ 0x10110829
            _logbookButton = new PulsingButton("art/interface/utility_bar_ui/logbook_bling.tga",
                new Rectangle(70, 4, 19, 41));
            _logbookButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/logbook.tga",
                hoverImagePath = "art/interface/utility_bar_ui/logbook_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/logbook_click.tga"
            });
            _logbookButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            _logbookButton.TooltipText = _translations[1];
            _logbookButton.SetClickHandler(OnLogbookButtonClick);
            _container.Add(_logbookButton);

            // Created @ 0x10110959
            _mapButton = new PulsingButton("art/interface/utility_bar_ui/townmap_bling.tga",
                new Rectangle(90, 4, 19, 41));
            _mapButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/townmap.tga",
                hoverImagePath = "art/interface/utility_bar_ui/townmap_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/townmap_click.tga"
            });
            _mapButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            _mapButton.TooltipText = _translations[2];
            _mapButton.SetClickHandler(OnMapButtonClick);
            _container.Add(_mapButton);

            // Created @ 0x10110a89
            _restButton = new WidgetButton(new Rectangle(110, 4, 19, 41));
            _restButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/camp.tga",
                hoverImagePath = "art/interface/utility_bar_ui/camp_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/camp_click.tga"
            });
            _restButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            _restButton.SetClickHandler(OnRestButtonClick);
            UpdateRestButton();
            _container.Add(_restButton);

            // Created @ 0x10110cec
            var helpButton = new WidgetButton(new Rectangle(130, 4, 19, 41));
            helpButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/help.tga",
                hoverImagePath = "art/interface/utility_bar_ui/help_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/help_click.tga"
            });
            helpButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            helpButton.TooltipText = _translations[4];
            helpButton.SetClickHandler(OnHelpButtonClick);
            _container.Add(helpButton);

            // Created @ 0x10110bbc
            var optionsButton = new WidgetButton(new Rectangle(150, 4, 19, 41));
            optionsButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/utility_bar_ui/options.tga",
                hoverImagePath = "art/interface/utility_bar_ui/options_hover.tga",
                pressedImagePath = "art/interface/utility_bar_ui/options_click.tga"
            });
            optionsButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            optionsButton.TooltipText = _translations[5];
            optionsButton.SetClickHandler(OnOptionsButtonClick);
            _container.Add(optionsButton);

            // Created @ 0x10110e15
            var timeWidget = new UtilityBarTimeBar(new Rectangle(52, 48, 117, 21));
            _container.Add(timeWidget);

            Globals.UiManager.OnCanvasSizeChanged += UpdateSize;
            UpdateSize(Globals.UiManager.CanvasSize);
        }

        private void UpdateSize(Size size)
        {
            _container.X = size.Width - _container.Width + 1;
            _container.Y = size.Height - _container.Height - 2;
        }

        [TempleDllLocation(0x1010ffd0)]
        [TempleDllLocation(0x101100f0)]
        private void UpdateRestButton()
        {
            WidgetButtonStyle style;
            string tooltip;
            switch (GameSystems.RandomEncounter.SleepStatus)
            {
                case SleepStatus.PassTimeOnly:
                    style = PassTimeButtonStyle;
                    tooltip = _translations[6];
                    break;
                case SleepStatus.Impossible:
                    style = RestForbiddenButtonStyle;
                    tooltip = _translations[3];
                    break;
                case SleepStatus.Dangerous:
                    style = RestUnsafeButtonStyle;
                    tooltip = _translations[3];
                    break;
                case SleepStatus.Safe:
                    style = RestSafeButtonStyle;
                    tooltip = _translations[3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _restButton.SetStyle(style);
            _restButton.TooltipText = tooltip;
            _restButton.SetDisabled(GameSystems.Combat.IsCombatActive());
        }

        /// <summary>
        /// For how long will the map and logbook button pulse when triggered.
        /// </summary>
        private static readonly TimeSpan PulsingDuration = TimeSpan.FromSeconds(6);

        /// <summary>
        /// Cycle time in milliseconds of the logbook and map button pulsing. (From fully transparent to fully opaque).
        /// </summary>
        private const int PulsingCycle = 500;

        /// <summary>
        /// True if the map buttion is currently "pulsing" to draw the players attention.
        /// </summary>
        [TempleDllLocation(0x10bd34e4)]
        private bool _logbookButtonPulsing;

        [TempleDllLocation(0x10bd3454)]
        private TimePoint _logbookButtonPulsingStart;

        [TempleDllLocation(0x1010f100)]
        public void PulseLogbookButton()
        {
            // TODO: Imho the duration should refresh even when it's already blinging,
            // and the sound should re-play when it's 1+ s in the past

            if ( !_logbookButtonPulsing )
            {
                _logbookButtonPulsing = true;
                _logbookButtonPulsingStart = TimePoint.Now;
                Tig.Sound.PlaySoundEffect(3100);
            }
        }

        [TempleDllLocation(0x1010fb50)]
        private void UpdateLogbookButton()
        {
            _logbookButton.PulseColor = GetPulsingColor(ref _logbookButtonPulsing, _logbookButtonPulsingStart);
        }

        /// <summary>
        /// The button is initially disabled until the player visits any map that allows the map button to be used.
        /// From that point onwards the button is permanently enabled until the game is reset.
        /// </summary>
        [TempleDllLocation(0x10bd34e0)]
        private bool _enableMapButton;

        /// <summary>
        /// True if the map buttion is currently "pulsing" to draw the players attention.
        /// </summary>
        [TempleDllLocation(0x10bd34e8)]
        private bool _mapButtonPulsing;

        [TempleDllLocation(0x10bd2ed0)]
        private TimePoint _mapButtonPulsingStart;

        [TempleDllLocation(0x1010f130)]
        public void PulseMapButton()
        {
            // TODO: Imho the duration should refresh even when it's already blinging,
            // and the sound should re-play when it's 1+ s in the past
            if (!_mapButtonPulsing)
            {
                _mapButtonPulsing = true;
                _mapButtonPulsingStart = TimePoint.Now;
                Tig.Sound.PlaySoundEffect(3100);
            }
        }

        [TempleDllLocation(0x1010fd70)]
        private void UpdateTownmapButton()
        {
            if (!_enableMapButton && UiSystems.TownMap.IsTownMapAvailable)
            {
                _enableMapButton = true;
            }

            if (!_enableMapButton || GameSystems.Combat.IsCombatActive())
            {
                _mapButton.SetDisabled(true);
            }
            else
            {
                _mapButton.SetDisabled(false);
            }

            _mapButton.PulseColor = GetPulsingColor(ref _mapButtonPulsing, _mapButtonPulsingStart);
        }

        private static PackedLinearColorA GetPulsingColor(ref bool enabled, TimePoint started)
        {
            byte alpha;
            if (enabled)
            {
                var elapsed = TimePoint.Now - started;
                if (elapsed >= PulsingDuration)
                {
                    enabled = false;
                    alpha = 0;
                }
                else
                {
                    var elapsedMs = (int) elapsed.TotalMilliseconds;
                    var fadingIn = (elapsedMs / PulsingCycle % 2) == 0;
                    var msInCycle = elapsedMs % PulsingCycle;

                    alpha = (byte) (msInCycle / (float) PulsingCycle * 255.0f);
                    if (!fadingIn)
                    {
                        alpha = (byte) (255 - alpha);
                    }
                }
            }
            else
            {
                alpha = 0;
            }

            return new PackedLinearColorA(255, 255, 255, alpha);
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

            if (GameSystems.RandomEncounter.SleepStatus != SleepStatus.Impossible)
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
            _container.Visible = false;
            _historyUi.Hide();
        }

        [TempleDllLocation(0x10bd33f8)]
        public bool IsVisible() => _container.Visible;

        [TempleDllLocation(0x1010ee80)]
        [TemplePlusLocation("ui_utility_bar.cpp:12")]
        public void Show()
        {
            _container.Visible = true;
            _container.BringToFront(); // TODO: Fishy
            _historyUi.Show();
        }

        public void AdvanceTime(TimePoint time)
        {
            UpdateRestButton();
            UpdateLogbookButton();
            UpdateTownmapButton();
        }

        [TempleDllLocation(0x1010ee70)]
        public void Reset()
        {
            _enableMapButton = false;
        }
    }
}