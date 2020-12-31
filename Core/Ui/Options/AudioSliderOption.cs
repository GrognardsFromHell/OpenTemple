using System;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Widgets;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Options
{
    public enum VolumeType
    {
        Master,
        SoundEffects,
        Dialog,
        Music,
        Positional
    }

    /// <summary>
    /// Percentage slider intended for Audio Volume. Will instantly apply changes only to revert them if the
    /// options dialog is canceled.
    /// </summary>
    public sealed class AudioSliderOption : SliderOption
    {
        private readonly VolumeType _type;

        private int _initialVolume;

        public AudioSliderOption(VolumeType type)
            : base(GetLabel(type), () => GetVolume(type), value => SetVolume(type, value), 0, 100)
        {
            _type = type;

            // Immediately apply the volume change
            this.WhenAnyValue(x => x.Value)
                .Subscribe(UpdateVolume);
        }

        private void UpdateVolume(int volume)
        {
            SetVolume(_type, volume);
        }

        private static int GetVolume(VolumeType type)
        {
            return type switch
            {
                VolumeType.Master => Globals.Config.MasterVolume,
                VolumeType.SoundEffects => Globals.Config.EffectsVolume,
                VolumeType.Dialog => Globals.Config.VoiceVolume,
                VolumeType.Music => Globals.Config.MusicVolume,
                VolumeType.Positional => Globals.Config.ThreeDVolume,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static void SetVolume(VolumeType type, int newVolume)
        {
            switch (type)
            {
                case VolumeType.Master:
                    Globals.Config.MasterVolume = newVolume;
                    break;
                case VolumeType.SoundEffects:
                    Globals.Config.EffectsVolume = newVolume;
                    break;
                case VolumeType.Dialog:
                    Globals.Config.VoiceVolume = newVolume;
                    break;
                case VolumeType.Music:
                    Globals.Config.MusicVolume = newVolume;
                    break;
                case VolumeType.Positional:
                    Globals.Config.ThreeDVolume = newVolume;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GameSystems.SoundGame.UpdateVolume();
        }

        private static string GetLabel(VolumeType type)
        {
            return type switch
            {
                VolumeType.Master => "Master Volume", // TODO Localize
                VolumeType.SoundEffects => "#{options:200}",
                VolumeType.Dialog => "#{options:201}",
                VolumeType.Music => "#{options:202}",
                VolumeType.Positional => "#{options:203}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void Reset()
        {
            base.Reset();
            _initialVolume = Value;
        }

        public override void Cancel()
        {
            base.Cancel();

            // Reset the actual volume as well
            SetVolume(_type, _initialVolume);
        }
    }
}