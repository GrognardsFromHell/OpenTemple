using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Options
{
    public class OptionsUi
    {
        // Height in pixel of a single options entry
        private const int OptionHeight = 34;

        // Border between options
        private const int OptionPadding = 1;

        // How many options fit on screen simultaneously
        private const int OptionRows = 11;

        [TempleDllLocation(0x101177d0)]
        public bool IsVisible => _container.IsInTree();

        [TempleDllLocation(0x10bda724)]
        private bool _fromMainMenu; // Prolly means shown from utility bar

        private readonly WidgetContainer _container;

        private WidgetScrollBar _scrollbar;

        [TempleDllLocation(0x10bda720)]
        private bool _confirmingVideoSettings;

        private readonly WidgetTabBar _tabBar;

        private readonly WidgetContainer _optionsContainer;

        private readonly List<OptionsPage> _pages = new List<OptionsPage>();

        [TempleDllLocation(0x1011b640)]
        public OptionsUi()
        {
            var doc = WidgetDoc.Load("ui/options_ui.json");
            _container = doc.TakeRootContainer();
            _optionsContainer = doc.GetContainer("options");

            _scrollbar = doc.GetScrollBar("scrollbar");

            var accept = doc.GetButton("accept");
            accept.SetClickHandler(ApplySettings);

            var cancel = doc.GetButton("cancel");
            cancel.SetClickHandler(Cancel);

            CreatePages();

            _tabBar = doc.GetTabBar("tabs");
            _tabBar.OnActiveTabIndexChanged += ShowPage;
            _tabBar.SetTabs(_pages.Select(page => page.Name));
            _tabBar.ActiveTabIndex = 0;
        }

        private void CreatePages()
        {
            // VIDEO
            _pages.Add(new OptionsPage(
                "#{options:0}",
                new CheckboxOption(
                    "Windowed",
                    () => Globals.Config.Window.Windowed,
                    value => Globals.Config.Window.Windowed = value
                ),
                new CheckboxOption(
                    "#{options:104}",
                    () => Globals.Config.Rendering.IsAntiAliasing,
                    value => Globals.Config.Rendering.IsAntiAliasing = value
                ),
                new SliderOption(
                    "#{options:110}",
                    () => Globals.Config.LineOfSightChecksPerFrame,
                    value => Globals.Config.LineOfSightChecksPerFrame = value,
                    1,
                    8
                )
            ));

            // AUDIO
            _pages.Add(new OptionsPage(
                "#{options:1}",
                new AudioSliderOption(VolumeType.Master),
                new AudioSliderOption(VolumeType.SoundEffects),
                new AudioSliderOption(VolumeType.Dialog),
                new AudioSliderOption(VolumeType.Music),
                new AudioSliderOption(VolumeType.Positional)
            ));

            // PREFS
            _pages.Add(new OptionsPage(
                "#{options:2}",
                new CheckboxOption(
                    "#{options:300}",
                    () => Globals.Config.ConcurrentTurnsEnabled,
                    value => Globals.Config.ConcurrentTurnsEnabled = value
                ),
                new CheckboxOption(
                    "#{options:301}",
                    () => Globals.Config.PartyVoiceConfirmations,
                    value => Globals.Config.PartyVoiceConfirmations = value
                ),
                new CheckboxOption(
                    "#{options:302}",
                    () => Globals.Config.PartyTextConfirmations,
                    value => Globals.Config.PartyTextConfirmations = value
                ),
                new SliderOption(
                    "#{options:303}",
                    () => Globals.Config.ScrollSpeed,
                    value => Globals.Config.ScrollSpeed = value,
                    0,
                    4
                ),
                new CheckboxOption(
                    "#{options:304}",
                    () => Globals.Config.ScrollAcceleration,
                    value => Globals.Config.ScrollAcceleration = value
                ),
                new CheckboxOption(
                    "#{options:305}",
                    () => Globals.Config.AutoSaveBetweenMaps,
                    value => Globals.Config.AutoSaveBetweenMaps = value
                ),
                new SliderOption(
                    "#{options:306}",
                    () => Globals.Config.TextFloatSpeed,
                    value => Globals.Config.TextFloatSpeed = value,
                    0,
                    3
                ),
                // TODO: End turn with time remaining
                new CheckboxOption(
                    "#{options:308}",
                    () => Globals.Config.EndTurnDefault,
                    value => Globals.Config.EndTurnDefault = value
                ),
                new CheckboxOption(
                    "#{options:309}",
                    () => Globals.Config.ShowPartyHitPoints,
                    value => Globals.Config.ShowPartyHitPoints = value
                ),
                new CheckboxOption(
                    "#{options:310}",
                    () => Globals.Config.StartupTip != -1,
                    value => Globals.Config.StartupTip = value ? 0 : -1
                ),
                new CheckboxOption(
                    "New Races",
                    () => Globals.Config.newRaces,
                    value => Globals.Config.newRaces = value
                ),
                new CheckboxOption(
                    "Forgotten Realm Races",
                    () => Globals.Config.forgottenRealmsRaces,
                    value => Globals.Config.forgottenRealmsRaces = value
                ),
                new CheckboxOption(
                    "Monstrous Races",
                    () => Globals.Config.monstrousRaces,
                    value => Globals.Config.monstrousRaces = value
                )
            ));

            // There's also "CONTROLS", but it wasn't used in ToEE
        }

        [TempleDllLocation(0x10119d20)]
        public void Show(bool fromMainMenu)
        {
            _tabBar.ActiveTabIndex = 0; // Select first tab
            _fromMainMenu = fromMainMenu;

            UiSystems.HideOpenedWindows(true);
            GameSystems.TimeEvent.PauseGameTime();
            Globals.UiManager.RootElement.Append(_container);

            if (fromMainMenu)
            {
                UiSystems.UtilityBar.Hide();
            }

            foreach (var page in _pages)
            {
                foreach (var option in page.Options)
                {
                    option.Reset();
                }
            }

            _tabBar.ActiveTabIndex = 0;
            _scrollbar.SetValue(0);
        }

        [TempleDllLocation(0x10117780)]
        public void Hide()
        {
            if (_container.IsInTree())
            {
                GameSystems.TimeEvent.ResumeGameTime();
                _container.Remove();
            }

            if (_fromMainMenu)
            {
                UiSystems.UtilityBar.Show();
            }
        }

        private void ShowPage(int pageIndex)
        {
            _optionsContainer.Clear();

            if (pageIndex == -1)
            {
                return;
            }

            var page = _pages[pageIndex];

            _scrollbar.SetValueChangeHandler(null);
            _scrollbar.SetMax(Math.Max(0, page.Options.Count - OptionRows));
            _scrollbar.SetValue(0);
            _scrollbar.SetValueChangeHandler(newOffset => ShowPage(pageIndex));

            var y = 0;
            var rowCount = 0;
            for (var index = _scrollbar.GetValue(); index < page.Options.Count; index++)
            {
                if (rowCount++ > OptionRows)
                {
                    break;
                }

                var option = page.Options[index];
                // Create a separate container for each option so that layouting inside of that is easier
                var optionContainer = new WidgetContainer(0, y, 448, OptionHeight);
                y += OptionHeight + OptionPadding;

                var text = new WidgetText(option.Label, "options-label");
                text.SetX(29);
                text.SetCenterVertically(true);
                optionContainer.AddContent(text);

                option.AddTo(optionContainer);

                _optionsContainer.Add(optionContainer);
            }
        }

        [TempleDllLocation(0x101186d0)]
        private void ApplySettings()
        {
            if (_confirmingVideoSettings)
            {
                return;
            }

            // TODO: If video settings changed, apply but show confirm with countdown

            foreach (var page in _pages)
            {
                foreach (var option in page.Options)
                {
                    option.Apply();
                }
            }
            Globals.ConfigManager.Save();
            Globals.ConfigManager.NotifyConfigChanged();

            if (!_confirmingVideoSettings)
            {
                UiSystems.Options.Hide();
                if (_fromMainMenu)
                {
                    UiSystems.MainMenu.Show(0);
                }
            }
        }

        [TempleDllLocation(0x10118bf0)]
        private void Cancel()
        {
            foreach (var page in _pages)
            {
                foreach (var option in page.Options)
                {
                    option.Cancel();
                }
            }

            Hide();
            if (_fromMainMenu)
            {
                UiSystems.MainMenu.Show(0);
            }
        }
    }
}