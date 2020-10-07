using System;
using System.Collections.Generic;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.Ui.Options
{
    public class OptionsUi
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x101177d0)]
        public bool IsVisible => false; // TODO _container.Visible;

        [TempleDllLocation(0x10bda724)]
        private bool _fromMainMenu; // Prolly means shown from utility bar

        private readonly List<OptionsPage> _pages = new List<OptionsPage>();

        public IList<OptionsPage> Pages => _pages;

        [TempleDllLocation(0x1011b640)]
        public OptionsUi(IUserInterfaceInterop uiInterop)
        {
            uiInterop.CreateModule("OpenTemple.Options", module => {
                module.RegisterType<Option>();
                module.RegisterType<CheckboxOption>();
                module.RegisterType<SliderOption>();
                module.RegisterType<OptionsPage>();
                module.RegisterSingleton(this, "Options");
            });

            CreatePages();
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

        public void Save()
        {
            Logger.Info("Saving configuration");
            Globals.ConfigManager.Save();
            Globals.ConfigManager.NotifyConfigChanged();
        }

        [TempleDllLocation(0x10119d20)]
        public void Show(bool fromMainMenu)
        {
            Stub.DEPRECATED();
            // _fromMainMenu = fromMainMenu;
            //
            // UiSystems.HideOpenedWindows(true);
            // GameSystems.TimeEvent.PushDisableFidget();
            //
            // if (fromMainMenu)
            // {
            //     UiSystems.UtilityBar.Hide();
            // }
            //
            // foreach (var page in _pages)
            // {
            //     // TODO
            //     // foreach (var option in page.Options)
            //     // {
            //     // option.Reset();
            //     // }
            // }
        }

        [TempleDllLocation(0x10117780)]
        public void Hide()
        {
            Stub.DEPRECATED();
            // if (_container.Visible)
            // {
            //     GameSystems.TimeEvent.PopDisableFidget();
            // }
            //
            // _container.Visible = false;
            //
            // if (_fromMainMenu)
            // {
            //     UiSystems.UtilityBar.Show();
            // }
        }

    }
}