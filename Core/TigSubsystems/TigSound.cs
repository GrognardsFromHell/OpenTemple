using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using SoLoud;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Sound;
using OpenTemple.Core.Time;
using OpenTemple.Interop;

namespace OpenTemple.Core.TigSubsystems;

public enum tig_sound_type
{
    TIG_ST_EFFECTS = 0,
    TIG_ST_MUSIC = 1,
    TIG_ST_VOICE = 2,
    TIG_ST_THREE_D = 3
}

public class TigSound : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private Soloud? _soloud;

    /// <summary>
    /// This is segmented as follows:
    /// Indices  Purpose
    /// 0-1      speech
    /// 2-5      music
    /// 6-10     3d positional
    /// 11-69    sound effects
    /// </summary>
    [TempleDllLocation(0x10ee7578)]
    private tig_sound_stream[] tig_sound_streams = new tig_sound_stream[70];

    [TempleDllLocation(0x10EED5A0)]
    private int nextFreeMusicStreamIdx = 2;

    [TempleDllLocation(0x10eed38c)]
    private readonly Func<int, string?> _soundLookup;

    [TempleDllLocation(0x10ee7568)]
    private bool mss_reverb_enabled = false;

    [TempleDllLocation(0x10eed388)]
    private float mss_reverb_dry; // Range: [0,1] (non-reverb level)

    [TempleDllLocation(0x10ee756c)]
    private float mss_reverb_wet; // Range: [0,1] (reverb level)

    private const int FirstMusicStreamIdx = 2;
    private const int LastMusicStreamIdx = 5;

    private const int FirstEffectStreamIdx = 11;
    private const int LastEffectStreamIdx = 69; // This is post

    [TempleDllLocation(0x10ee7574)]
    private int nextFreeEffectStreamIdx = FirstEffectStreamIdx;

    [TempleDllLocation(0x10EED398)]
    public int EffectVolume { get; set; }

    private float _masterVolume = 1.0f;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            _soloud?.setGlobalVolume(_masterVolume);
        }
    }

    [TempleDllLocation(0x101e3fa0)]
    public TigSound(Func<int, string> soundLookup, bool disableSound)
    {
        nextFreeEffectStreamIdx = 11;

        _soundLookup = soundLookup;

        _soloud = new Soloud();
        if (disableSound)
        {
            _soloud.init(Soloud.CLIP_ROUNDOFF | Soloud.LEFT_HANDED_3D, Soloud.NULLDRIVER);
        }
        else
        {
            var err = _soloud.init(
                Soloud.CLIP_ROUNDOFF | Soloud.LEFT_HANDED_3D
            );
            if (err != 0)
            {
                Logger.Error("Failed to initialize sound system: {0}", _soloud.getErrorString(err));
                _soloud.init(Soloud.CLIP_ROUNDOFF | Soloud.LEFT_HANDED_3D, Soloud.NULLDRIVER);
                return;
            }
        }

        Logger.Info(
            "Using Sound backend: {0} ({1} ch, {2} kHz)",
            _soloud.getBackendString(),
            _soloud.getBackendChannels(),
            _soloud.getBackendSamplerate()
        );
        
        // This is the same as DirectSound 8 which is a potential backend for MSS32 used by ToEE
        _soloud.set3dListenerPosition(0, 0, 0);
        _soloud.set3dListenerAt(0, 0, 1);
        _soloud.set3dListenerUp(0, 1, 0);

        // TODO: Possibly sample cache
        // TODO dword_10EED5A4/*0x10eed5a4*/ = sub_10200B20/*0x10200b20*/(0x14, 1000000);
    }

    [TempleDllLocation(0x101e46a0)]
    public void PlaySoundEffect(int soundId)
    {
        if (_soloud == null || soundId == -1)
        {
            return;
        }

        if (tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_EFFECTS) != 0)
        {
            return;
        }

        SetStreamSourceFromSoundId(streamId, soundId);
        SetStreamVolume(streamId, EffectVolume);
    }

    [TempleDllLocation(0x101e4700)]
    public void PlayPositionalAmbientSample(string path, float volume)
    {
        if (_soloud == null || tig_sound_alloc_stream(out var streamId, tig_sound_type.TIG_ST_THREE_D) != 0)
        {
            return;
        }

        ref var stream = ref tig_sound_streams[streamId];

        if (!LoadSample(ref stream, path))
        {
            FreeStream(streamId);
            return;
        }

        // TODO v5 = (int*) sub_10200C00 /*0x10200c00*/(dword_10EED5A4 /*0x10eed5a4*/, path);
        // TODO stream.field_134 = (int) v5;

        if (mss_reverb_enabled)
        {
            // TODO AIL_set_3D_sample_effects_level(stream.mss_3dsample, 0x3F800000); ???
        }

        // Just  generate a random point around the listener
        var distance = 10f + GameSystems.Random.GetFactor() * 40f;
        var theta = GameSystems.Random.GetFactor() * 2 * MathF.PI;
        var x = distance * MathF.Cos(theta);
        var z = distance * MathF.Sin(theta);
        
        stream.voiceHandle = _soloud.play3d(stream.wav, x, 0, z, aVolume: volume, aPaused: true);
        _soloud.set3dSourceMinMaxDistance(stream.voiceHandle, 2.0f, 50.0f);
        // model 1 is inverse distance clamped. seems to be the standard DirectSound model and also used by MSS
        _soloud.set3dSourceAttenuation(stream.voiceHandle, 2, 1);
        _soloud.update3dAudio();
        _soloud.setPause(stream.voiceHandle, false);

        stream.flags |= 0x400;
    }

    private bool LoadSample(ref tig_sound_stream stream, string path)
    {
        FreeResources(ref stream);
        stream.soundPath = null;

        stream.wav = new Wav();

        try
        {
            using var sampleData = Tig.FS.ReadFile(path);
            var err = stream.wav.loadMem(sampleData.Memory.Span);
            if (err != 0)
            {
                Logger.Warn("Failed to load sound: {0}: {1}", path, err);
                return false;
            }
        }
        catch (FileNotFoundException)
        {
            Logger.Warn("Failed to find sound: {0}", path);
            return false;
        }

        stream.soundPath = path;
        return true;
    }

    private void FreeResources(ref tig_sound_stream stream)
    {
        if (stream.wav != null)
        {
            stream.wav.Dispose();
            stream.wav = null;
        }
    }

    public void PlayDynamicSource(SoLoudDynamicSource source)
    {
        if (_soloud != null)
        {
            _soloud.play(source); // TODO: Movie volume, etc.
        }
    }

    [TempleDllLocation(0x101E38D0)]
    public void SetStreamSourceFromSoundId(int streamId, int soundId)
    {
        if (_soloud == null)
        {
            return;
        }

        var soundPath = _soundLookup(soundId);
        if (soundPath == null)
        {
            Logger.Debug("No sound path found for sound id {0}", soundId);
            return;
        }

        SetStreamSourceFromPath(streamId, soundPath, soundId);
    }

    [TempleDllLocation(0x101E3790)]
    public void SetStreamSourceFromPath(int streamId, string soundPath, int soundId)
    {
        if (_soloud == null || streamId == -1)
        {
            return;
        }

        // TODO: Inefficient and unsafe (not freeing old handle for instance)

        ref var stream = ref tig_sound_streams[streamId];

        LoadSample(ref stream, soundPath);

        stream.flags |= 2;
        stream.soundId = soundId;
        stream.voiceHandle = _soloud.play(stream.wav, stream.volume / 127.0f, stream.panning);
        _soloud.setInaudibleBehavior(stream.voiceHandle, false, true);
    }

    [TempleDllLocation(0x101e3920)]
    private void tig_sound_load_stream_0(int streamId, string path, int loopCount, int a4, bool allowReverb,
        int otherStreamId)
    {
        if (_soloud == null || streamId < 0 || streamId >= tig_sound_streams.Length)
        {
            return;
        }

        ref var stream = ref tig_sound_streams[streamId];

        stream.field8 = Math.Abs(a4);
        stream.fieldC = 0;

        stream.flags |= 1;

        LoadSample(ref stream, path);

        stream.voiceHandle = _soloud.play(stream.wav, stream.volume / 127.0f, stream.panning);

        if (!_soloud.isValidVoiceHandle(stream.voiceHandle))
        {
            // TODO: Free wav (also bad handling :[)
            stream.active = false;
            return;
        }

        if (allowReverb && mss_reverb_enabled)
        {
            // TODO REVERB AIL_set_stream_reverb_levels(v9, mss_reverb_dry /*0x10eed388*/,
            // TODO REVERB     mss_reverb_wet /*0x10ee756c*/);
        }

        stream.loopCount = loopCount;
        if (loopCount == 0)
        {
            // In MSS, looping = 0 -> loops forever
            _soloud.setLooping(stream.voiceHandle, true);
        }
        else if (loopCount != 1)
        {
            // TODO Only looping=true|false supported right now
        }

        if (otherStreamId >= 0 && a4 > 0)
        {
            stream.flags |= 0x08;
        }
        else
        {
            stream.flags |= 0x10;
        }

        if (otherStreamId >= 0)
        {
            ref var otherStream = ref tig_sound_streams[otherStreamId];
            if ((otherStream.flags & 0x10) != 0)
            {
                otherStream.flags &= ~0x10;
            }

            if ((otherStream.flags & 8) != 0)
            {
                otherStream.flags &= ~0x8;
            }

            otherStream.flags |= 4;
            otherStream.field8 = Math.Abs(a4);
            otherStream.fieldC = 0;
            otherStream.field20 = streamId;
        }
    }

    [TempleDllLocation(0x101e3ad0)]
    public void StreamPlayMusicLoop(int streamId, string soundPath, int a3, bool allowReverb, int otherStreamId)
    {
        tig_sound_load_stream_0(streamId, soundPath, 0, a3, allowReverb, otherStreamId);
    }

    [TempleDllLocation(0x101e3b00)]
    public void StreamPlayMusicOnce(int streamId, string soundPath, int a4, bool allowReverb, int otherStreamId)
    {
        tig_sound_load_stream_0(streamId, soundPath, 1, a4, allowReverb, otherStreamId);
    }

    [TempleDllLocation(0x11e74544)]
    private TimePoint _nextAudioTick;

    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan HundredMs = TimeSpan.FromMilliseconds(100);

    [TempleDllLocation(0x101E4360)]
    public void ProcessEvents()
    {
        if (_soloud == null)
        {
            return;
        }
        
        var now = TimePoint.Now;

        if (now < _nextAudioTick - OneSecond)
        {
            _nextAudioTick = now;
        }

        if (now <= _nextAudioTick + OneSecond)
        {
            if (now < _nextAudioTick)
            {
                return;
            }

            _nextAudioTick += HundredMs;
        }
        else
        {
            _nextAudioTick = now + HundredMs;
        }

        UpdateSoundStreams();
    }

    [TempleDllLocation(0x101e40c0)]
    private void UpdateSoundStreams()
    {
        if (_soloud == null)
        {
            return;
        }
        
        for (var i = 0; i < tig_sound_streams.Length; i++)
        {
            ref var stream = ref tig_sound_streams[i];
            if (!stream.active)
            {
                continue;
            }

            if ((stream.flags & 8) != 0)
            {
                // TODO: muted!?!?
                continue;
            }

            if ((stream.flags & 4) != 0)
            {
                // TODO: Probably fade out
                stream.fieldC++;
                int fadedVolume;
                if (stream.fieldC <= stream.field8)
                {
                    fadedVolume = stream.volume * (stream.field8 - stream.fieldC) / stream.field8;
                }
                else
                {
                    stream.flags &= ~4;
                    if (stream.field20 >= 0)
                    {
                        ref var otherStream = ref tig_sound_streams[stream.field20];
                        if ((otherStream.flags & 8) != 0)
                        {
                            otherStream.flags &= ~8;
                            otherStream.flags |= 0x10;
                        }
                    }

                    fadedVolume = 0;
                }

                if ((stream.flags & 1) != 0)
                {
                    var newVolume = fadedVolume / 127.0f;
                    _soloud.setVolume(stream.voiceHandle, newVolume);
                    stream.flags |= 0x20;
                }
                else if ((stream.flags & 2) != 0)
                {
                    var actualVol = (float)(fadedVolume * stream.volume / 128) / 127.0f;
                    _soloud.setVolume(stream.voiceHandle, actualVol);
                }
                else if ((stream.flags & 0x400) != 0)
                {
                    var newVolume = (fadedVolume * stream.volume / 128) / 127.0f;
                    _soloud.setVolume(stream.voiceHandle, newVolume);
                }
            }
            else if ((stream.flags & 0x10) != 0)
            {
                // TODO: Probably fade in
                if (stream.fieldC == 0)
                {
                    _soloud.setPause(stream.voiceHandle, false);
                }

                stream.fieldC++;
                int fadedVolume;
                if (stream.fieldC <= stream.field8)
                {
                    fadedVolume = stream.fieldC * stream.volume / stream.field8;
                }
                else
                {
                    fadedVolume = stream.volume;
                    stream.flags &= ~0x10;
                }

                var newVolume = fadedVolume / 127.0f;
                if ((stream.flags & 1) != 0)
                {
                    _soloud.setVolume(stream.voiceHandle, newVolume);
                }
                else if ((stream.flags & 2) != 0)
                {
                    _soloud.setVolume(stream.voiceHandle, newVolume);
                    _soloud.setPan(stream.voiceHandle, 0.0f);
                }
                else if ((stream.flags & 0x400) != 0)
                {
                    _soloud.setVolume(stream.voiceHandle, newVolume);
                }
            }
            else
            {
                if (!IsStreamPlaying(i))
                {
                    stream.flags |= 0x20;
                }

                if ((stream.flags & 0x20) != 0)
                {
                    FreeStream(i);
                }
            }
        }
    }

    [TempleDllLocation(0x10300bac)]
    private readonly Dictionary<tig_sound_type, int> tig_sound_type_stream_flags =
        new()
        {
            { tig_sound_type.TIG_ST_EFFECTS, 0x80 },
            { tig_sound_type.TIG_ST_MUSIC, 0x100 },
            { tig_sound_type.TIG_ST_VOICE, 0x200 },
            { tig_sound_type.TIG_ST_THREE_D, 0x400 },
        };

    [TempleDllLocation(0x101e45b0)]
    [TemplePlusLocation("sound.cpp:76")]
    public int tig_sound_alloc_stream(out int streamId, tig_sound_type streamType)
    {
        if (_soloud == null)
        {
            streamId = -1;
            return 1;
        }

        streamId = tig_sound_alloc_stream_real(streamType);
        if (streamId == -1)
        {
            return 3;
        }

        ref var stream = ref tig_sound_streams[streamId];
        stream = default;
        stream.active = true;
        stream.field20 = -1;
        stream.flags = tig_sound_type_stream_flags[streamType];
        stream.loopCount = 1;
        stream.volume = 127;
        stream.panning = 0.0f;
        return 0;
    }

    [TempleDllLocation(0x101e4440)]
    private int tig_sound_alloc_stream_real(tig_sound_type a1)
    {
        int streamIdx;

        switch (a1)
        {
            case tig_sound_type.TIG_ST_EFFECTS:
                streamIdx = nextFreeEffectStreamIdx;
                var triesMade = 0;
                while (tig_sound_streams[streamIdx].active)
                {
                    if (++streamIdx > LastEffectStreamIdx)
                        streamIdx = FirstEffectStreamIdx;
                    if (++triesMade > LastEffectStreamIdx)
                    {
                        nextFreeEffectStreamIdx = streamIdx;
                        return -1;
                    }
                }

                nextFreeEffectStreamIdx = streamIdx;
                break;
            case tig_sound_type.TIG_ST_VOICE:
                streamIdx = 0;
                var v5 = 0;
                while (tig_sound_streams[v5].active)
                {
                    ++v5;
                    ++streamIdx;
                    if (v5 > 1)
                        return -1;
                }

                break;
            case tig_sound_type.TIG_ST_MUSIC:
                // Fix for allocating music
                var newStreamId = -1;
                for (var i = FirstMusicStreamIdx; i <= LastMusicStreamIdx; i++)
                {
                    if (!tig_sound_streams[i].active)
                    {
                        newStreamId = i;
                        break;
                    }
                }

                if (newStreamId == -1)
                {
                    FreeStream(nextFreeMusicStreamIdx);
                    newStreamId = nextFreeMusicStreamIdx++;
                    if (nextFreeMusicStreamIdx > LastMusicStreamIdx)
                        nextFreeMusicStreamIdx = FirstMusicStreamIdx;
                }

                return newStreamId;
            case tig_sound_type.TIG_ST_THREE_D:
                streamIdx = 6;

                while (tig_sound_streams[streamIdx].active)
                {
                    ++streamIdx;
                    if (streamIdx > 10)
                        return -1;
                }

                break;
            default:
                return -1;
        }

        return streamIdx;
    }

    [TempleDllLocation(0x101e36d0)]
    public void FreeStream(int streamId)
    {
        if (_soloud == null || streamId < 0 || streamId >= tig_sound_streams.Length)
        {
            return;
        }

        ref var stream = ref tig_sound_streams[streamId];

        if (!stream.active)
        {
            return;
        }

        FadeOutStream(streamId, 0);

        if ((stream.flags & 1) != 0)
        {
            _soloud.stop(stream.voiceHandle);
            stream.active = false;
        }
        else if ((stream.flags & 2) != 0)
        {
            _soloud.stop(stream.voiceHandle);
            // TODO sub_10200B90/*0x10200b90*/(dword_10EED5A4/*0x10eed5a4*/, stream.field134);
        }
        else if ((stream.flags & 0x400) != 0)
        {
            _soloud.stop(stream.voiceHandle);
            // TODO sub_10200B90/*0x10200b90*/(dword_10EED5A4/*0x10eed5a4*/, stream.field134);
        }

        FreeResources(ref stream);
        stream.active = false;
    }

    [TempleDllLocation(0x101e3660)]
    [TempleDllLocation(0x101e36c0)]
    public void FadeOutStream(int streamId, int fadeOutTime)
    {
        if (_soloud == null || streamId < 0 || streamId >= 70)
        {
            return;
        }

        ref var stream = ref tig_sound_streams[streamId];

        stream.flags &= ~0x18;
        if ((stream.flags & 4) == 0)
        {
            stream.flags |= 4;
            stream.field8 = Math.Abs(fadeOutTime);
            stream.fieldC = 0;
        }
    }

    [TempleDllLocation(0x101e4640)]
    public void SetVolume(tig_sound_type soundType, int volume)
    {
        if (_soloud == null)
        {
            return;
        }

        var typeFlags = tig_sound_type_stream_flags[soundType];

        for (var i = 0; i < tig_sound_streams.Length; i++)
        {
            ref var stream = ref tig_sound_streams[i];

            if (stream.active && (stream.flags & typeFlags) != 0)
            {
                SetStreamVolume(i, volume);
            }
        }

        if (soundType == tig_sound_type.TIG_ST_EFFECTS)
        {
            EffectVolume = volume;
        }
    }

    // Volume is 0-127
    [TempleDllLocation(0x101E3B60)]
    public void SetStreamVolume(int streamId, int volume)
    {
        if (_soloud == null || streamId == -1)
        {
            return;
        }

        ref var stream = ref tig_sound_streams[streamId];
        if (stream.volume == volume)
        {
            return;
        }

        stream.volume = volume;

        var actualVolume = volume / 127.0f;
        if ((stream.flags & 1) != 0)
        {
            stream.wav?.setVolume(actualVolume);
        }
        else if ((stream.flags & 2) != 0)
        {
            stream.wav?.setVolume(actualVolume);
        }
        else if ((stream.flags & 0x400) != 0)
        {
            stream.wav?.setVolume(actualVolume);
        }

        if (stream.voiceHandle != 0)
        {
            _soloud?.setVolume(stream.voiceHandle, actualVolume);
        }
    }

    [TempleDllLocation(0x101e3c20)]
    public tig_sound_type SoundStreamGetType(int streamId)
    {
        // TODO: storing the "type" in a bitfield is stupid, let's just use a normal field (if at all)
        if (_soloud != null && streamId >= 0 && streamId < 70)
        {
            tig_sound_type type = tig_sound_type.TIG_ST_EFFECTS;
            while ((tig_sound_streams[streamId].flags & tig_sound_type_stream_flags[type]) == 0)
            {
                if (++type > tig_sound_type.TIG_ST_THREE_D)
                    return default;
            }

            return type;
        }
        else
        {
            return default;
        }
    }

    [TempleDllLocation(0x101e3c70)]
    public SoundSourceSize GetStreamSourceSize(int streamId)
    {
        if (_soloud != null && streamId >= 0 && streamId < 70)
        {
            return tig_sound_streams[streamId].sourceSize;
        }
        else
        {
            return SoundSourceSize.Large;
        }
    }

    [TempleDllLocation(0x101e3ca0)]
    public void SetStreamSourceSize(int streamId, SoundSourceSize value)
    {
        if (_soloud != null && streamId >= 0 && streamId < 70)
        {
            tig_sound_streams[streamId].sourceSize = value;
        }
    }

    [TempleDllLocation(0x101e3cd0)]
    public void SetStreamPanning(int streamId, float panning)
    {
        if (_soloud == null || streamId is < 0 or >= 70)
        {
            return;
        }

        ref var stream = ref tig_sound_streams[streamId];
        if (Math.Abs(stream.panning - panning) > 0.0001f)
        {
            stream.panning = panning;
            if ((stream.flags & 1) == 0 && (stream.flags & 2) != 0)
            {
                _soloud.setPan(stream.voiceHandle, stream.panning);
            }
        }
    }

    [TempleDllLocation(0x101e3d40)]
    public bool IsStreamPlaying(int streamId)
    {
        if (_soloud == null || streamId is < 0 or >= 70)
        {
            return false;
        }

        ref var stream = ref tig_sound_streams[streamId];
        if (stream.active)
        {
            return _soloud.isValidVoiceHandle(stream.voiceHandle)
                   && !_soloud.getPause(stream.voiceHandle);
        }

        return false;
    }

    [TempleDllLocation(0x101e3dc0)]
    public bool IsStreamActive(int streamId)
    {
        if (streamId < 0 || streamId >= 70)
            return false;
        else
            return tig_sound_streams[streamId].active;
    }

    [TempleDllLocation(0x101e3b30)]
    public void SetStreamLoopCount(int streamId, int loopCount)
    {
        if (_soloud != null)
        {
            if (streamId >= 0 && streamId < 70)
                tig_sound_streams[streamId].loopCount = loopCount;
        }
    }

    [TempleDllLocation(0x101e3df0)]
    public void SetStreamWorldPos(int streamId, Vector3 worldPos)
    {
        if (streamId >= 0 && streamId < 70)
        {
            ref var stream = ref tig_sound_streams[streamId];
            stream.worldPos = worldPos;
            stream.hasWorldPos = true;
        }
    }

    [TempleDllLocation(0x101e3e40)]
    public bool TryGetStreamWorldPos(int streamId, out Vector3 worldPos)
    {
        if (streamId >= 0 && streamId < 70)
        {
            ref var stream = ref tig_sound_streams[streamId];
            worldPos = stream.worldPos;
            return stream.hasWorldPos;
        }

        worldPos = default;
        return false;
    }

    [TempleDllLocation(0x101e3ea0)]
    public void SoundStreamForEach3dSound(Action<int> callback)
    {
        for (var i = 0; i < tig_sound_streams.Length; i++)
        {
            ref var stream = ref tig_sound_streams[i];
            if (stream.active && stream.hasWorldPos)
            {
                callback(i);
            }
        }
    }

    [TempleDllLocation(0x101e3f20)]
    public void SoundDisableReverb()
    {
        mss_reverb_enabled = false;
    }

    [TempleDllLocation(0x101e3f30)]
    public void SetReverb(ReverbRoomType roomType, int reverbDryPercent, int reverbWetPercent)
    {
        mss_reverb_enabled = true;
        // TODO: Reverb room type presets

        mss_reverb_dry = reverbDryPercent * 0.01f;
        mss_reverb_wet = reverbWetPercent * 0.01f;
    }

    [TempleDllLocation(0x101e43b0)]
    public void FadeOutAll(int a1)
    {
        for (var i = 0; i < tig_sound_streams.Length; i++)
        {
            ref var stream = ref tig_sound_streams[i];
            if (stream.active)
            {
                FadeOutStream(i, a1);
            }
        }

        nextFreeEffectStreamIdx = FirstEffectStreamIdx;
        nextFreeMusicStreamIdx = FirstMusicStreamIdx;
        UpdateSoundStreams();
    }

    private struct tig_sound_stream
    {
        public bool active;
        public int flags;
        public int field8;
        public int fieldC;
        public int loopCount; // TODO: may not be loop count, but just looping flag!
        public IntPtr mss_stream;
        public int mss_audiodata;
        public int mss_3dsample;
        public int field20; // ID of other stream (cross-fade ??!?!)
        public string? soundPath;
        public int soundId;
        public int volume; // 0-127
        public float panning; // -1 to 1 (where 0 is center, -1 is left, 1 is right)
        public int field134;
        public bool hasWorldPos;
        public int field13c;
        public Vector3 worldPos;
        public SoundSourceSize sourceSize;
        public int field154;
        public Wav? wav;

        // Soloud handle returned by play
        public uint voiceHandle;
    }

    [TempleDllLocation(0x101e48a0)]
    public void Dispose()
    {
        // We have to release all Wav objects we allocated before actually shutting down soloud itself
        for (var i = 0; i < tig_sound_streams.Length; i++)
        {
            FreeResources(ref tig_sound_streams[i]);
        }

        // TODO sub_10200B70((void *)dword_10EED5A4); ( sample cache ?)

        _soloud?.deinit();
        _soloud = null;
    }
}

// Same as EAX / RAD Game Tools
// See OpenAL-Soft for a replica of the EAX reverb algorithm and it's settings.
// I.e.: https://github.com/kcat/openal-soft/blob/dc3fa3e51f91096a22ba53faec5bd1146936bee3/include/AL/efx-presets.h
// NOTE: The only map to use reverb is the earth node. It uses type=8 (Cave)
public enum ReverbRoomType
{
    Generic = 0, // factory default
    Paddedcell = 1,
    Room = 2, // standard environments
    Bathroom = 3,
    Livingroom = 4,
    Stoneroom = 5,
    Auditorium = 6,
    ConcertHall = 7,
    Cave = 8,
    Arena = 9,
    Hangar = 10,
    CarpetedHallway = 11,
    Hallway = 12,
    StoneCorridor = 13,
    Alley = 14,
    Forest = 15,
    City = 16,
    Mountains = 17,
    Quarry = 18,
    Plain = 19,
    ParkingLot = 20,
    SewerPipe = 21,
    Underwater = 22,
    Drugged = 23,
    Dizzy = 24,
    Psychotic = 25
}