using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
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
        public bool IsVisible => _dialog.IsVisible;

        [TempleDllLocation(0x10bda724)]
        private bool _fromMainMenu; // Prolly means shown from utility bar

        private readonly OptionsDialog _dialog;

        private readonly OptionsDialogViewModel _model = new ();

        private readonly List<OptionsPage> _pages = new ();

        [TempleDllLocation(0x10bda720)]
        private bool _confirmingVideoSettings;

        [TempleDllLocation(0x1011b640)]
        public OptionsUi()
        {
            CreatePages();

            _dialog = new OptionsDialog {DataContext = _model, IsVisible = false};
            _model.OnAccept += ApplySettings;
            _model.OnCancel += Cancel;
            _model.Pages = _pages;
            Tig.MainWindow.AddOverlay(_dialog);
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
            _fromMainMenu = fromMainMenu;

            UiSystems.HideOpenedWindows(true);
            GameSystems.TimeEvent.PauseGameTime();
            _dialog.IsVisible = true;

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
        }

        [TempleDllLocation(0x10117780)]
        public void Hide()
        {
            if (_dialog.IsVisible)
            {
                GameSystems.TimeEvent.ResumeGameTime();
            }

            _dialog.IsVisible = false;

            if (_fromMainMenu)
            {
                UiSystems.UtilityBar.Show();
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