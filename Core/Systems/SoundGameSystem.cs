using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class SoundGameSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem,
        ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly PositionalAudioConfig _positionalAudioConfig;

        [TempleDllLocation(0x108f2744)]
        private int musicVolume;

        [TempleDllLocation(0x108F273C)]
        private int effectsVolume;

        [TempleDllLocation(0x108EE828)]
        private int voiceVolume;

        [TempleDllLocation(0x108F2868)]
        private int threeDVolume;

        [TempleDllLocation(0x108f28c8)]
        private int dword_108F28C8 = 0;

        [TempleDllLocation(0x108f28cc)]
        private int dword_108F28CC = 0;

        [TempleDllLocation(0x108f28c0)]
        private int soundBaseX = 0;

        [TempleDllLocation(0x108ee830)]
        private int soundBaseY = 0;

        [TempleDllLocation(0x108f2748)]
        private int musicOn = 0;

        [TempleDllLocation(0x108f274c)]
        private int soundscheme_stashed = 0;

        [TempleDllLocation(0x108f2738)]
        private readonly Dictionary<int, string> _soundIndex = new Dictionary<int, string>();

        [TempleDllLocation(0x1003c9f0)]
        public int MusicVolume => musicVolume;

        [TempleDllLocation(0x1003d4a0)]
        public SoundGameSystem()
        {
            // TODO SOUND
            var soundParams = Tig.FS.ReadMesFile("sound/soundparams.mes");
            _positionalAudioConfig = new PositionalAudioConfig(soundParams);

            LoadSoundIndex();
            
            effectsVolume = 127 * Globals.Config.EffectsVolume / 10;
            Tig.Sound.SetVolume(tig_sound_type.TIG_ST_EFFECTS, 80 * effectsVolume / 100);
            
            voiceVolume = 127 * Globals.Config.VoiceVolume / 10;
            Tig.Sound.SetVolume(tig_sound_type.TIG_ST_VOICE, 80 * voiceVolume / 100);
            // TODO: Set movie volume
            
            musicVolume = 127 * Globals.Config.MusicVolume / 10;

            threeDVolume = 127 * Globals.Config.VoiceVolume / 10;
            Tig.Sound.SetVolume(tig_sound_type.TIG_ST_THREE_D, threeDVolume);

            /* TODO
              if ( soundgame_inited )
    {
      sub_10028EC0((location2d)line, &mesId, (int64_t *)&pYOut);
      if ( soundgame_inited )
      {
        qword_108EE838 = mesId;
        qword_108F2888 = pYOut;
        sub_101E3EA0((int (__cdecl *)(_DWORD))sub_1003CEF0);
      }
    }*/
            
        }

        private void LoadSoundIndex()
        {
            var soundLists = Tig.FS.ReadMesFile("sound/snd_00index.mes");
            foreach (var soundListFilename in soundLists.Values)
            {
                var soundListPath = $"sound/{soundListFilename}";
                Logger.Info("Loading sound list {0}", soundListPath);

                var soundList = Tig.FS.ReadMesFile(soundListPath);
                foreach (var (soundId, soundFilename) in soundList)
                {
                    if (soundFilename.Length > 0)
                    {
                        var soundPath = "sound/" + soundFilename.Replace('\\', '/');
                        _soundIndex[soundId] = soundPath;
                    }
                }
            }

            Logger.Info("Loaded sound index with {0} sounds.", _soundIndex.Count);
        }

        [TempleDllLocation(0x1003bb10)]
        public void Dispose()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003bb80)]
        public void LoadModule()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003bbc0)]
        public void UnloadModule()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003cb30)]
        public void Reset()
        {
            dword_108F28C8 = 0;
            dword_108F28CC = 0;
            soundBaseX = 0;
            soundBaseY = 0;
            musicOn = 0;
            soundscheme_stashed = 0;
            StopAll(false);
        }

        [TempleDllLocation(0x1003bbd0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1003cb70)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1003dc50)]
        public void AdvanceTime(TimePoint time)
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003bdb0)]
        public void Sound(int soundId, int loopCount = 1)
        {
            Stub.TODO();
        }

        public int PositionalSound(int soundId, GameObjectBody source)
        {
            return PositionalSound(soundId, 1, source);
        }

        public int PositionalSound(int soundId, locXY location)
        {
            return PositionalSound(soundId, 1, location);
        }

        [TempleDllLocation(0x1003d090)]
        public int PositionalSound(int soundId, int loopCount, GameObjectBody source)
        {
            Stub.TODO();
            return -1;
        }

        [TempleDllLocation(0x1003dcb0)]
        public int PositionalSound(int soundId, int loopCount, locXY location)
        {
            Stub.TODO();
            return -1;
        }

        [TempleDllLocation(0x1003c5b0)]
        public void StopAll(bool b)
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003c4d0)]
        public void SetScheme(int s1, int s2)
        {
            // TODO SOUND
        }

        /// <summary>
        /// Sets the tile coordinates the view is currently centered on.
        /// </summary>
        [TempleDllLocation(0x1003D3C0)]
        public void SetViewCenterTile(locXY location)
        {
            // TODO
        }

        [TempleDllLocation(0x1003c770)]
        public void StartCombatMusic(GameObjectBody handle)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1003C8B0)]
        public void StopCombatMusic(GameObjectBody handle)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1003b9e0)]
        public string FindSoundFilename(int soundId)
        {
            // weapon hit sounds (encoded data)
            if ((soundId & 0xC0000000) == 0xC0000000)
            {
                return GameSystems.SoundMap.GetWeaponHitSoundPath(soundId);
            }

            return _soundIndex.GetValueOrDefault(soundId, null);
        }

        public bool IsValidSoundId(int soundId) => FindSoundFilename(soundId) != null;

        [TempleDllLocation(0x1003bdd0)]
        public SoundSourceSize GetSoundSourceSize(GameObjectBody obj)
        {
            if (obj.type == ObjectType.scenery)
            {
                var sceneryFlags = obj.GetSceneryFlags();
                if (sceneryFlags.HasFlag(SceneryFlag.SOUND_SMALL))
                {
                    return SoundSourceSize.Small;
                }

                if (sceneryFlags.HasFlag(SceneryFlag.SOUND_MEDIUM))
                {
                    return SoundSourceSize.Medium;
                }

                if (sceneryFlags.HasFlag(SceneryFlag.SOUND_EXTRA_LARGE))
                {
                    return SoundSourceSize.ExtraLarge;
                }
            }

            return SoundSourceSize.Large;
        }

        [TempleDllLocation(0x1003BE30)]
        public int GetSoundOutOfRangeRange(GameObjectBody obj)
        {
            var sourceSize = GetSoundSourceSize(obj);
            return _positionalAudioConfig.AttenuationRangeEnd[sourceSize] / 28;
        }

        [TempleDllLocation(0x1003c5f0)]
        public int PlaySpeechFile(string soundPath, int i)
        {
            if (soundscheme_stashed != 0)
            {
                return -1;
            }

            if (Tig.Sound.tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_VOICE) != 0)
            {
                return -1;
            }

            Tig.Sound.SetStreamVolume(streamId, voiceVolume);
            // TODO tig_sound_load_stream /*0x101e3b00*/(a1, soundPath, i, 1, -1);
            // TODO tig_sound_is_stream_active /*0x101e3dc0*/(a1);
            throw new NotImplementedException();
        }
    }


    public enum PortalSoundEffect
    {
        Open = 0,
        Close,
        Locked = 2
    }

    /**
     * TODO: This might correspond to the scs_ python enum
     * scs_critically_hit = 0
     * scs_dying = 1
     * scs_dying_gruesome = 2
     * scs_fidgeting = 3
     * scs_attacking = 4
     * scs_alerted = 5
     * scs_agitated = 6
     * scs_footsteps = 7
     */
    public enum CritterSoundEffect
    {
        Attack = 0,
        Death = 1,
        Unk5 = 5,
        Footsteps = 7
    }

    public enum SoundSourceSize
    {
        Small = 0,
        Medium = 1,
        Large = 2, // This is the default
        ExtraLarge = 3
    }

    public class PositionalAudioConfig
    {
        /// <summary>
        /// Sounds closer than this (in screen coordinates) are unattenuated.
        /// This seems to be relative to the center of the screen.
        /// </summary>
        public Dictionary<SoundSourceSize, int> AttenuationRangeStart { get; } =
            new Dictionary<SoundSourceSize, int>
            {
                {SoundSourceSize.Small, 50},
                {SoundSourceSize.Medium, 50},
                {SoundSourceSize.Large, 150},
                {SoundSourceSize.ExtraLarge, 50}
            };

        /// <summary>
        /// Sound sources further away than this (in screen coordinates) from the
        /// center of the screen play at zero volume.
        /// TODO: This should calculate based on the screen edge.
        /// </summary>
        public Dictionary<SoundSourceSize, int> AttenuationRangeEnd { get; } =
            new Dictionary<SoundSourceSize, int>
            {
                {SoundSourceSize.Small, 150},
                {SoundSourceSize.Medium, 400},
                {SoundSourceSize.Large, 800},
                {SoundSourceSize.ExtraLarge, 1500}
            };

        /// <summary>
        /// The volume for sound sources of a given size at minimum attenuation.
        /// </summary>
        public Dictionary<SoundSourceSize, int> AttenuationMaxVolume { get; } =
            new Dictionary<SoundSourceSize, int>
            {
                {SoundSourceSize.Small, 40},
                {SoundSourceSize.Medium, 70},
                {SoundSourceSize.Large, 100},
                {SoundSourceSize.ExtraLarge, 100}
            };

        /// <summary>
        /// Sounds within this range of the screen center (in screen coordinates) play
        /// dead center.
        /// </summary>
        public int PanningMinRange { get; } = 150;

        /// <summary>
        /// Sounds further away than this range relative to the screen center (in screen coordinates) play
        /// fully on that side.
        /// </summary>
        public int PanningMaxRange { get; } = 400;

        public PositionalAudioConfig()
        {
        }

        public PositionalAudioConfig(Dictionary<int, string> parameters)
        {
            AttenuationRangeStart[SoundSourceSize.Large] = int.Parse(parameters[1]);
            AttenuationRangeEnd[SoundSourceSize.Large] = int.Parse(parameters[2]);
            PanningMinRange = int.Parse(parameters[3]);
            PanningMaxRange = int.Parse(parameters[4]);

            AttenuationRangeStart[SoundSourceSize.Small] = int.Parse(parameters[10]);
            AttenuationRangeEnd[SoundSourceSize.Small] = int.Parse(parameters[11]);
            AttenuationMaxVolume[SoundSourceSize.Small] = int.Parse(parameters[12]);

            AttenuationRangeStart[SoundSourceSize.Medium] = int.Parse(parameters[20]);
            AttenuationRangeEnd[SoundSourceSize.Medium] = int.Parse(parameters[21]);
            AttenuationMaxVolume[SoundSourceSize.Medium] = int.Parse(parameters[22]);

            AttenuationRangeStart[SoundSourceSize.ExtraLarge] = int.Parse(parameters[30]);
            AttenuationRangeEnd[SoundSourceSize.ExtraLarge] = int.Parse(parameters[31]);
            AttenuationMaxVolume[SoundSourceSize.ExtraLarge] = int.Parse(parameters[32]);
        }
    }
}