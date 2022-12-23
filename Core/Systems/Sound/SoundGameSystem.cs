using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Sound;

public class SoundGameSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem,
    ITimeAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(250);

    private static readonly bool IsEditor = false;

    [TempleDllLocation(0x108f270c)]
    private bool soundgameInited;

    private readonly PositionalAudioConfig _positionalAudioConfig;

    [TempleDllLocation(0x108f2710)]
    private TimePoint _lastUpdate = new(long.MaxValue);

    [TempleDllLocation(0x108f2740)]
    private readonly Dictionary<int, string> soundSchemeIndexMes;

    [TempleDllLocation(0x108f2750)]
    private readonly Dictionary<int, string> soundSchemeListMes;

    [TempleDllLocation(0x108ee848)]
    private SoundScheme[] soundScheme =
    {
        new(),
        new()
    };

    [TempleDllLocation(0x108f2744)]
    private int musicVolume;

    [TempleDllLocation(0x108F273C)]
    private int effectsVolume;

    [TempleDllLocation(0x108EE828)]
    private int _voiceVolume;

    [TempleDllLocation(0x108F2868)]
    private int threeDVolume;

    [TempleDllLocation(0x108f28c8)]
    private int dword_108F28C8 = 0;

    [TempleDllLocation(0x108f28cc)]
    private int dword_108F28CC = 0;

    [TempleDllLocation(0x108f2748)]
    private bool _combatMusicPlaying;

    [TempleDllLocation(0x108ee840)]
    private int _combatMusicIntroStreamId;

    [TempleDllLocation(0x108f28e4)]
    private int _combatMusicLoopStreamId;

    [TempleDllLocation(0x108f28b8)]
    private (int, int) _schemesBeforeCombatMusic;

    [TempleDllLocation(0x108f274c)]
    private int soundscheme_stashed = 0;

    [TempleDllLocation(0x108f286c)]
    private int[] _stashedSoundSchemes = new int[2];

    [TempleDllLocation(0x108f2738)]
    private readonly Dictionary<int, string> _soundIndex = new();

    [TempleDllLocation(0x1003c9f0)]
    public int MusicVolume => musicVolume;

    [TempleDllLocation(0x108ee838)]
    [TempleDllLocation(0x108f2888)]
    private Vector3 _currentListenerPos;

    [TempleDllLocation(0x108f2890)]
    private bool _playingOverlayScheme;

    [TempleDllLocation(0x108f2874)]
    private (int, int) _schemesBeforeOverlayScheme;

    [TempleDllLocation(0x1003d4a0)]
    public SoundGameSystem()
    {
        soundgameInited = true;

        // TODO SOUND
        var soundParams = Tig.FS.ReadMesFile("sound/soundparams.mes");
        _positionalAudioConfig = new PositionalAudioConfig(soundParams);

        LoadSoundIndex();

        soundSchemeIndexMes = Tig.FS.ReadMesFile("sound/SchemeIndex.mes");
        soundSchemeListMes = Tig.FS.ReadMesFile("sound/SchemeList.mes");

        UpdateVolume();
        Globals.ConfigManager.OnConfigChanged += UpdateVolume;

        if (soundgameInited)
        {
            Stub.TODO();
            // sub_10028EC0((location2d)line, &mesId, (int64_t*)&pYOut);
            // if (soundgameInited)
            // {
            //     qword_108EE838 = mesId;
            //     qword_108F2888 = pYOut;
            //     sub_101E3EA0((int(__cdecl *)(_DWORD))sub_1003CEF0);
            // }
        }
    }

    public void UpdateVolume()
    {
        Tig.Sound.MasterVolume = Globals.Config.MasterVolume / 100.0f;

        effectsVolume = 127 * Globals.Config.EffectsVolume / 100;
        Tig.Sound.SetVolume(tig_sound_type.TIG_ST_EFFECTS, 80 * effectsVolume / 100);

        _voiceVolume = 127 * Globals.Config.VoiceVolume / 100;
        Tig.Sound.SetVolume(tig_sound_type.TIG_ST_VOICE, 80 * _voiceVolume / 100);
        // TODO: Set movie volume

        musicVolume = 127 * Globals.Config.MusicVolume / 100;
        Tig.Sound.SetVolume(tig_sound_type.TIG_ST_MUSIC, musicVolume);

        threeDVolume = 127 * Globals.Config.ThreeDVolume / 100;
        Tig.Sound.SetVolume(tig_sound_type.TIG_ST_THREE_D, threeDVolume);
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
        soundscheme_unloadall(0);
        soundgameInited = false;
    }

    [TempleDllLocation(0x1003cb30)]
    public void Reset()
    {
        _combatMusicPlaying = false;
        soundscheme_stashed = 0;
        StopAll(0);
    }

    [TempleDllLocation(0x1003bbd0)]
    public void SaveGame(SavedGameState savedGameState)
    {
        savedGameState.SoundGameState = new SavedSoundGameState
        {
            IsOverlayScheme = _playingOverlayScheme,
            SchemesSuppressedByOverlay = _schemesBeforeOverlayScheme,
            IsCombatMusicPlaying = _combatMusicPlaying,
            SchemesSuppressedByCombatMusic = _schemesBeforeCombatMusic,
            CurrentSchemeIds = (
                soundScheme[0].SchemelistKey != 0 ? soundScheme[0].SchemeId : 0,
                soundScheme[1].SchemelistKey != 0 ? soundScheme[1].SchemeId : 0
            )
        };
    }

    [TempleDllLocation(0x1003cb70)]
    public void LoadGame(SavedGameState savedGameState)
    {
        var soundState = savedGameState.SoundGameState;

        _playingOverlayScheme = soundState.IsOverlayScheme;
        _schemesBeforeOverlayScheme = soundState.SchemesSuppressedByOverlay;
        _combatMusicPlaying = soundState.IsCombatMusicPlaying;
        _schemesBeforeCombatMusic = soundState.SchemesSuppressedByCombatMusic;

        if (_combatMusicPlaying)
        {
            soundscheme_unloadall(0);
            var scheme = new SoundScheme();
            LoadSoundScheme(_schemesBeforeCombatMusic.Item1, scheme);
            scheme.GetCombatMusicFiles(out _, out var loop);
            _combatMusicLoopStreamId = PlayCombatLoopMusic(loop, 0);
        }
        else
        {
            SetScheme(soundState.CurrentSchemeIds.Item1, soundState.CurrentSchemeIds.Item2);
        }
    }

    [TempleDllLocation(0x1003dc50)]
    public void AdvanceTime(TimePoint time)
    {
        // Update the position for attenuation purposes. This is different from behavior in Vanilla,
        // where it was only updated from the scroll-system.
        if (!IsEditor && GameViews.Primary != null)
        {
            SetViewCenterTile(GameViews.Primary.CenteredOn.ToInches3D());
        }

        // Update the schemes every 250ms
        if (time < _lastUpdate || time - _lastUpdate >= UpdateInterval)
        {
            _lastUpdate = time;
            TickSoundSchemes();
        }
    }

    [TempleDllLocation(0x1003d0f0)]
    private void TickSoundSchemes()
    {
        foreach (var scheme in soundScheme)
        {
            TickSoundScheme(scheme);
        }
    }

    private void TickSoundScheme(SoundScheme scheme)
    {
        var hourOfDay = GameSystems.TimeEvent.HourOfDay;
        foreach (var schemeElement in scheme.Lines)
        {
            if (schemeElement.Music)
            {
                ApplySoundSchemeImmediateElement(schemeElement, hourOfDay);
            }
            else
            {
                ApplySoundSchemeAmbientElement(schemeElement, hourOfDay);
            }
        }
    }

    private void ApplySoundSchemeImmediateElement(SoundSchemeElement schemeElement, int hourOfDay)
    {
        if (schemeElement.Over)
        {
            if (!Tig.Sound.IsStreamPlaying(schemeElement.StreamId))
            {
                if (_playingOverlayScheme)
                {
                    SetScheme(_schemesBeforeOverlayScheme.Item1, _schemesBeforeOverlayScheme.Item2);
                    _playingOverlayScheme = false;
                }
            }
        }
        else if (schemeElement.Loop)
        {
            if (hourOfDay < schemeElement.TimeFrom || hourOfDay > schemeElement.TimeTo)
            {
                if (schemeElement.StreamId != -1)
                {
                    Tig.Sound.FadeOutStream(schemeElement.StreamId, 80);
                    schemeElement.StreamId = -1;
                }
            }
            else
            {
                if (schemeElement.StreamId == -1)
                {
                    if (_combatMusicPlaying)
                    {
                        Tig.Sound.FadeOutStream(_combatMusicIntroStreamId, 25);
                        Tig.Sound.FadeOutStream(_combatMusicLoopStreamId, 25);
                        _combatMusicIntroStreamId = -1;
                        _combatMusicLoopStreamId = -1;
                        _combatMusicPlaying = false;
                    }

                    var path = ResolvePath(schemeElement.Filename);
                    Tig.Sound.tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_MUSIC);
                    Tig.Sound.SetStreamVolume(streamId,
                        musicVolume * 80 / 100 * schemeElement.VolFrom / 100);
                    Tig.Sound.StreamPlayMusicLoop(streamId, path, 80, false, -1);
                    schemeElement.StreamId = streamId;
                }
            }
        }
    }

    private void ApplySoundSchemeAmbientElement(SoundSchemeElement schemeElement, int hourOfDay)
    {
        if (hourOfDay < schemeElement.TimeFrom || hourOfDay > schemeElement.TimeTo)
        {
            return; // Element does not apply to this time of day
        }

        var roll = GameSystems.Random.GetInt(0, 999);
        if (roll >= schemeElement.Freq)
        {
            return; // Random occurrence roll not met 
        }

        var filename = ResolvePath(schemeElement.Filename);

        var maxVolume = schemeElement.VolTo;
        var volume = schemeElement.VolFrom;
        if (maxVolume > volume)
        {
            volume = schemeElement.VolFrom + GameSystems.Random.GetInt(0, maxVolume - volume - 1);
        }

        if (schemeElement.Scatter)
        {
            Logger.Debug("Playing ambient 3d sound effect {0}. Volume={1}", schemeElement.Filename, volume);
            // this is obviously wrong since the volume is 0-127, not 0-100, but it's what ToEE did
            Tig.Sound.PlayPositionalAmbientSample(filename, (volume * threeDVolume) / 100f / 127f);
        }
        else
        {
            var maxPanning = schemeElement.BalanceTo;
            int panning = schemeElement.BalanceFrom;
            if (maxPanning > panning)
            {
                panning = schemeElement.BalanceFrom + GameSystems.Random.GetInt(0, maxPanning - panning - 1);
            }

            Logger.Debug("Playing ambient sound effect {0}. Volume={1}, Panning={2}", schemeElement.Filename, volume, panning);
            CreateSoundStream(filename, 0, 1, volume, panning);
        }
    }

    private int PlayMusic(string path, float volume, bool once = false)
    {
        path = ResolvePath(path);
        Tig.Sound.tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_MUSIC);
        Tig.Sound.SetStreamVolume(streamId, (int) ((float) musicVolume * 80 / 100 * volume));
        if (once)
        {
            Tig.Sound.StreamPlayMusicOnce(streamId, path, 80, false, -1);
        }
        else
        {
            Tig.Sound.StreamPlayMusicLoop(streamId, path, 80, false, -1);
        }

        return streamId;
    }

    private string ResolvePath(string path)
    {
        if (path.StartsWith('#'))
        {
            return FindSoundFilename(int.Parse(path.Substring(1)));
        }
        else
        {
            return $"sound/{path}";
        }
    }

    [TempleDllLocation(0x1003bdb0)]
    public int Sound(int soundId, int loopCount = 1)
    {
        return CreateSoundStream(soundId, loopCount, 127, 64);
    }

    public int PositionalSound(int soundId, GameObject source)
    {
        return PositionalSound(soundId, 1, source);
    }

    public int PositionalSound(int soundId, locXY location)
    {
        return PositionalSound(soundId, 1, location);
    }

    /// <returns>The stream id or -1.</returns>
    [TempleDllLocation(0x1003d090)]
    public int PositionalSound(int soundId, int loopCount, GameObject source)
    {
        if (soundId == -1)
        {
            return -1;
        }

        var worldPos = source.GetLocationFull().ToInches3D();
        var sourceSize = GetSoundSourceSize(source);
        return PositionalSound(soundId, loopCount, worldPos, sourceSize);
    }

    [TempleDllLocation(0x1003dc80)]
    [TempleDllLocation(0x1003dcb0)]
    public int PositionalSound(int soundId, int loopCount, locXY location)
    {
        var worldPos = location.ToInches3D();
        return PositionalSound(soundId, loopCount, worldPos, SoundSourceSize.Large);
    }

    [TempleDllLocation(0x1003cff0)]
    [TempleDllLocation(0x1003cf60)]
    private int PositionalSound(int soundId, int loopCount, Vector3 worldPos, SoundSourceSize soundSize)
    {
        if (soundId == -1)
        {
            return -1;
        }

        SoundGameApplyAttenuation(worldPos, soundSize, out var volume, out var panning);
        var streamId = CreateSoundStream(soundId, loopCount, volume, panning);
        Tig.Sound.SetStreamWorldPos(streamId, worldPos);
        Tig.Sound.SetStreamSourceSize(streamId, soundSize);
        return streamId;
    }

    [TempleDllLocation(0x1003bd50)]
    private int CreateSoundStream(int soundId, int loopCount, int volume, float panning)
    {
        var soundPath = FindSoundFilename(soundId);
        if (soundPath == null)
        {
            Logger.Warn("Failed to find sound for id {0}", soundId);
            return -1;
        }

        return CreateSoundStream(soundPath, soundId, loopCount, volume, panning);
    }

    [TempleDllLocation(0x1003bc90)]
    private int CreateSoundStream(string soundPath, int soundId, int loopCount, int volume, float panning)
    {
        if (soundscheme_stashed != 0)
        {
            return -1;
        }

        if (Tig.Sound.tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_EFFECTS) != 0)
        {
            return -1;
        }

        Tig.Sound.SetStreamLoopCount(streamId, loopCount);
        var actualVolume = volume * (80 * effectsVolume / 100) / 127;
        Tig.Sound.SetStreamVolume(streamId, actualVolume);
        Tig.Sound.SetStreamPanning(streamId, panning);
        Tig.Sound.SetStreamSourceFromPath(streamId, soundPath, soundId);

        if (Tig.Sound.IsStreamActive(streamId))
        {
            return streamId;
        }
        else
        {
            return -1;
        }
    }

    [TempleDllLocation(0x1003cc50)]
    public void SoundGameApplyAttenuation(Vector3 sourcePos, SoundSourceSize soundSizeType, out int volumeOut,
        out float panningOut)
    {
        var distanceToSource = (sourcePos - _currentListenerPos).Length();

        var minRadius = _positionalAudioConfig.AttenuationRangeStart[soundSizeType];
        var maxRadius = _positionalAudioConfig.AttenuationRangeEnd[soundSizeType];

        var volume = 127;
        var maxVolume = _positionalAudioConfig.AttenuationMaxVolume[soundSizeType];
        if (distanceToSource < minRadius)
        {
            volume = 127;
        }
        else if (distanceToSource > maxRadius)
        {
            volume = 0;
        }
        else if (maxRadius > minRadius)
        {
            var v18 = (minRadius - distanceToSource) * 127;
            volume = (int) ((v18 / (maxRadius - minRadius)) + 127);
            volume = Math.Clamp(volume, 0, 127);
        }

        var panningMinRange = _positionalAudioConfig.PanningMinRange;
        var panningMaxRange = _positionalAudioConfig.PanningMaxRange;
        var distanceFromXAxis = Math.Abs(sourcePos.X - _currentListenerPos.X);

        var panning = 0.0f;
        if (sourcePos.X < _currentListenerPos.X)
        {
            if (distanceFromXAxis >= panningMaxRange)
            {
                panning = -1.0f; // Fully on the left
            }
            else if (distanceFromXAxis > panningMinRange)
            {
                panning = (panningMinRange - distanceFromXAxis) / (panningMaxRange - panningMinRange);
                panning = Math.Clamp(panning, -1.0f, 0.0f);
            }
        }

        if (sourcePos.X > _currentListenerPos.X)
        {
            if (distanceFromXAxis > panningMaxRange)
            {
                panning = 1.0f; // Fully on the right speaker
            }
            else if (distanceFromXAxis > panningMinRange)
            {
                panning = (distanceFromXAxis - panningMinRange) / (panningMaxRange - panningMinRange);
                panning = Math.Clamp(panning, 0.0f, 1.0f);
            }
        }

        volumeOut = volume * maxVolume / 100;
        panningOut = panning;
    }

    [TempleDllLocation(0x1003c5b0)]
    public void StopAll(int fadeOutTime)
    {
        if (soundgameInited)
        {
            soundscheme_unloadall(fadeOutTime);
            _playingOverlayScheme = false;
            Tig.Sound.FadeOutAll(fadeOutTime);
        }
    }

    [TempleDllLocation(0x1003c4d0)]
    public void SetScheme(int scheme1, int scheme2)
    {
        if (soundgameInited && soundscheme_stashed == 0)
        {
            if (scheme1 == scheme2)
            {
                scheme2 = 0;
            }

            Tig.Sound.SoundDisableReverb();
            if (_combatMusicPlaying)
            {
                _schemesBeforeCombatMusic = (scheme1, scheme2);
            }
            else
            {
                var scheme1Running = soundscheme_get(scheme1, out var currentScheme1Slot);
                var scheme2Running = soundscheme_get(scheme2, out var currentScheme2Slot);
                if (scheme1Running)
                {
                    if (!scheme2Running)
                    {
                        soundscheme_unload(80, 1 - currentScheme1Slot);
                        soundscheme_load(scheme2);
                    }
                }
                else if (scheme2Running)
                {
                    soundscheme_unload(80, 1 - currentScheme2Slot);
                    soundscheme_load(scheme1);
                }
                else
                {
                    soundscheme_unloadall(80);
                    soundscheme_load(scheme1);
                    soundscheme_load(scheme2);
                }
            }
        }
    }

    [TempleDllLocation(0x1003c490)]
    public bool soundscheme_get(int schemeId, out int index)
    {
        if (schemeId != 0)
        {
            for (var i = 0; i < soundScheme.Length; i++)
            {
                if (soundScheme[i].SchemelistKey != 0 && soundScheme[i].SchemeId == schemeId)
                {
                    index = i;
                    return true;
                }
            }
        }

        index = -1;
        return false;
    }

    [TempleDllLocation(0x1003bef0)]
    public void soundscheme_unloadall(int fadeOutTime)
    {
        if (soundgameInited)
        {
            for (var i = 0; i < soundScheme.Length; i++)
            {
                soundscheme_unload(fadeOutTime, i);
            }
        }
    }

    [TempleDllLocation(0x1003be60)]
    public void soundscheme_unload(int fadeOutTime, int idx)
    {
        var scheme = soundScheme[idx];
        var stashSchemeId = 0;
        if (scheme.SchemelistKey != 0)
        {
            stashSchemeId = scheme.SchemeId;
            foreach (var element in scheme.Lines)
            {
                if (element.Loop)
                {
                    if (element.StreamId != -1)
                    {
                        Tig.Sound.FadeOutStream(element.StreamId, fadeOutTime);
                        element.StreamId = -1;
                    }
                }
            }

            scheme.Reset();
        }

        // If it is later determined that the "new" scheme was a one-off, this allows us to restore this scheme
        if (!_playingOverlayScheme)
        {
            if (idx == 0)
            {
                _schemesBeforeOverlayScheme.Item1 = stashSchemeId;
            }
            else if (idx == 1)
            {
                _schemesBeforeOverlayScheme.Item2 = stashSchemeId;
            }
        }
    }

    [TempleDllLocation(0x1003c2f0)]
    [TempleDllLocation(0x1003c2f0)]
    public void soundscheme_load(int sndSchemeIdx)
    {
        if (sndSchemeIdx == 0)
        {
            return;
        }

        int freeSlot = -1;
        for (var i = 0; i < soundScheme.Length; i++)
        {
            if (soundScheme[i].SchemelistKey == 0)
            {
                freeSlot = i;
                break;
            }
        }

        if (freeSlot == -1)
        {
            return;
        }

        var scheme = soundScheme[freeSlot];
        LoadSoundScheme(sndSchemeIdx, scheme);
    }

    private void LoadSoundScheme(int id, SoundScheme scheme)
    {
        scheme.Reset();
        scheme.SchemeId = id;

        if (!soundSchemeIndexMes.TryGetValue(id, out var meslineValue))
        {
            Logger.Warn("Unknown sound scheme: {0}", id);
            return;
        }

        var indexLineParts = meslineValue.Split('#', 2);
        if (indexLineParts.Length != 2)
        {
            Logger.Warn("Invalid sound scheme {0}: {1}", id, meslineValue);
            return;
        }

        scheme.SchemeName = indexLineParts[0].Trim();
        scheme.SchemelistKey = int.Parse(indexLineParts[1]);

        for (var i = 0; i < 100; i++)
        {
            var lineKey = scheme.SchemelistKey + i;
            if (!soundSchemeListMes.TryGetValue(lineKey, out var line))
            {
                continue;
            }

            SoundSchemeElement element;
            try
            {
                element = SoundSchemeElement.Parse(line);
            }
            catch (Exception e)
            {
                Logger.Warn("Sound scheme line {0} is invalid: {1}", lineKey, e);
                continue;
            }

            if (element.Type == SoundSchemeElementType.CombatIntro)
            {
                scheme.CombatIntro = element.Filename;
                continue;
            }
            else if (element.Type == SoundSchemeElementType.CombatLoop)
            {
                scheme.CombatLoop = element.Filename;
                continue;
            }

            scheme.Lines.Add(element);

            if (element.Music)
            {
                if (!element.Loop)
                {
                    var path = ResolvePath(element.Filename);
                    Tig.Sound.tig_sound_alloc_stream(out element.StreamId, tig_sound_type.TIG_ST_MUSIC);
                    Tig.Sound.SetStreamVolume(element.StreamId,
                        80 * musicVolume * element.VolFrom / 100 / 100);
                    Tig.Sound.StreamPlayMusicOnce(element.StreamId, path, 80, false, -1);
                }

                if (element.Over)
                {
                    _playingOverlayScheme = true;
                }
            }
        }
    }

    /// <summary>
    /// Sets the tile coordinates the view is currently centered on.
    /// </summary>
    [TempleDllLocation(0x1003D3C0)]
    private void SetViewCenterTile(Vector3 worldPos)
    {
        var distSquared = (_currentListenerPos - worldPos).LengthSquared();
        if (distSquared > 1)
        {
            _currentListenerPos = worldPos;
            Tig.Sound.SoundStreamForEach3dSound(UpdateAttenuation);
        }
    }

    [TempleDllLocation(0x1003cef0)]
    private void UpdateAttenuation(int streamId)
    {
        if (Tig.Sound.TryGetStreamWorldPos(streamId, out var sourcePos))
        {
            var sourceSize = Tig.Sound.GetStreamSourceSize(streamId);
            SoundGameApplyAttenuation(sourcePos, sourceSize, out var volume, out var panning);
            var adjustedVolume2 = AdjustVolume(streamId, volume);
            Tig.Sound.SetStreamVolume(streamId, adjustedVolume2);
            Tig.Sound.SetStreamPanning(streamId, panning);
        }
    }

    [TempleDllLocation(0x1003ca90)]
    private int AdjustVolume(int streamId, int volume)
    {
        switch (Tig.Sound.SoundStreamGetType(streamId))
        {
            case tig_sound_type.TIG_ST_EFFECTS:
                return volume * (80 * effectsVolume / 100) / 127;
            case tig_sound_type.TIG_ST_MUSIC:
                return volume * (80 * musicVolume / 100) / 127;
            case tig_sound_type.TIG_ST_VOICE:
                return volume * _voiceVolume / 127;
            case tig_sound_type.TIG_ST_THREE_D:
                return volume * threeDVolume / 127;
            default:
                return volume;
        }
    }

    [TempleDllLocation(0x1003c770)]
    public void StartCombatMusic(GameObject obj)
    {
        if (!_combatMusicPlaying && obj.IsNPC())
        {
            _schemesBeforeCombatMusic = (
                soundScheme[0].SchemelistKey != 0 ? soundScheme[0].SchemeId : 0,
                soundScheme[1].SchemelistKey != 0 ? soundScheme[1].SchemeId : 0
            );

            soundScheme[0].GetCombatMusicFiles(out var combatIntro, out var combatLoop);

            soundscheme_unloadall(80);

            _combatMusicPlaying = true;
            _combatMusicIntroStreamId = PlayCombatIntroMusic(combatIntro, 0);
            _combatMusicLoopStreamId = PlayCombatLoopMusic(combatLoop, 0);
        }
    }

    [TempleDllLocation(0x1003C8B0)]
    public void StopCombatMusic(GameObject handle)
    {
        if (_combatMusicPlaying)
        {
            Tig.Sound.FadeOutStream(_combatMusicIntroStreamId, 25);
            Tig.Sound.FadeOutStream(_combatMusicLoopStreamId, 25);
            _combatMusicIntroStreamId = -1;
            _combatMusicLoopStreamId = -1;
            _combatMusicPlaying = false;
            GameSystems.SoundGame.SetScheme(_schemesBeforeCombatMusic.Item1, _schemesBeforeCombatMusic.Item2);
        }
    }

    [TempleDllLocation(0x1003c650)]
    private int PlayCombatIntroMusic(string path, int a4)
    {
        if (soundscheme_stashed == 0)
        {
            Tig.Sound.tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_MUSIC);
            Tig.Sound.SetStreamVolume(streamId, 100 * (80 * musicVolume / 100) / 100);
            Tig.Sound.StreamPlayMusicOnce(streamId, path, a4, false, -1);
            if (Tig.Sound.IsStreamActive(streamId))
            {
                return streamId;
            }
        }

        return -1;
    }

    [TempleDllLocation(0x1003c6e0)]
    private int PlayCombatLoopMusic(string path, int a3)
    {
        if (soundscheme_stashed == 0)
        {
            Tig.Sound.tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_MUSIC);
            Tig.Sound.SetStreamVolume(streamId, 100 * (80 * musicVolume / 100) / 100);
            Tig.Sound.StreamPlayMusicLoop(streamId, path, a3, false, -1);
            if (Tig.Sound.IsStreamActive(streamId))
            {
                return streamId;
            }
        }

        return -1;
    }

    [TempleDllLocation(0x1003b9e0)]
    public string? FindSoundFilename(int soundId)
    {
        // weapon hit sounds (encoded data)
        if (GameSystems.SoundMap.IsEncodedWeaponSound(soundId))
        {
            return GameSystems.SoundMap.GetWeaponHitSoundPath(soundId);
        }

        return _soundIndex.GetValueOrDefault(soundId);
    }

    public bool IsValidSoundId(int soundId) => FindSoundFilename(soundId) != null;

    [TempleDllLocation(0x1003bdd0)]
    public SoundSourceSize GetSoundSourceSize(GameObject obj)
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
    public int GetSoundOutOfRangeRange(GameObject obj)
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

        Tig.Sound.SetStreamVolume(streamId, _voiceVolume);
        Tig.Sound.StreamPlayMusicOnce(streamId, soundPath, i, true, -1);

        if (Tig.Sound.IsStreamActive(streamId))
        {
            return streamId;
        }

        return -1;
    }

    /// <summary>
    /// Saves the active sound-schemes and suspends them. Can be reactivated using <see cref="UnstashSchemes"/>.
    /// </summary>
    [TempleDllLocation(0x1003c910)]
    public void StashSchemes()
    {
        if (GameSystems.SoundGame.soundscheme_stashed == 0)
        {
            for (var i = 0; i < soundScheme.Length; i++)
            {
                if (soundScheme[i].SchemelistKey != 0)
                {
                    _stashedSoundSchemes[i] = soundScheme[i].SchemeId;
                }
                else
                {
                    _stashedSoundSchemes[i] = 0;
                }
            }

            soundscheme_unloadall(0);
        }

        ++GameSystems.SoundGame.soundscheme_stashed;
    }

    /// <summary>
    /// Reactivates sound-schemes previously stashed via <see cref="StashSchemes"/>
    /// </summary>
    [TempleDllLocation(0x1003C970)]
    public void UnstashSchemes()
    {
        if (soundscheme_stashed > 0 && --soundscheme_stashed == 0)
        {
            SetScheme(_stashedSoundSchemes[0], _stashedSoundSchemes[1]);
        }
    }
}