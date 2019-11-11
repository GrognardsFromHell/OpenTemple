using System;
using System.Collections.Generic;
using SoLoud;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.TigSubsystems
{
    public enum tig_sound_type
    {
        TIG_ST_EFFECTS = 0,
        TIG_ST_MUSIC = 1,
        TIG_ST_VOICE = 2,
        TIG_ST_THREE_D = 3
    }

    public class TigSound : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private Soloud _soloud;

        [TempleDllLocation(0x10ee7570)]
        private bool sound_initialized => _soloud != null;

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
        private int ringBufferStreamId = 2;

        [TempleDllLocation(0x10eed38c)]
        private Func<int, string> _soundLookup;

        [TempleDllLocation(0x10ee7568)]
        private bool mss_reverb_enabled = false;

        private const int FirstEffectStreamIdx = 11;
        private const int LastEffectStreamIdx = 70; // This is post

        [TempleDllLocation(0x10ee7574)]
        private int nextFreeEffectStreamIdx = FirstEffectStreamIdx;

        [TempleDllLocation(0x101e3fa0)]
        public TigSound(Func<int, string> soundLookup)
        {
            nextFreeEffectStreamIdx = 11;

            _soloud = new Soloud();
            var err = _soloud.init(
                Soloud.CLIP_ROUNDOFF | Soloud.LEFT_HANDED_3D
            );
            if (err != 0)
            {
                Logger.Error("Failed to initialize sound system: {0}", _soloud.getErrorString(err));
                _soloud = null;
            }

            // TODO: Possibly sample cache
            // TODO dword_10EED5A4/*0x10eed5a4*/ = sub_10200B20/*0x10200b20*/(0x14, 1000000);
        }

        [TempleDllLocation(0x101e46a0)]
        public void MssPlaySound(int soundId)
        {
            if (!sound_initialized || soundId == -1)
            {
                return;
            }



            // TODO
            Console.WriteLine("PLAY SOUND " + soundId);
        }

        [TempleDllLocation(0x101E4360)]
        public void ProcessEvents()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x101e3f30)]
        public void SetReverb(int roomType, int reverbDry, int reverbWet)
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x10300bac)]
        private readonly Dictionary<tig_sound_type, int> tig_sound_type_stream_flags =
            new Dictionary<tig_sound_type, int>
            {
                {tig_sound_type.TIG_ST_EFFECTS, 0x80},
                {tig_sound_type.TIG_ST_MUSIC, 0x100},
                {tig_sound_type.TIG_ST_VOICE, 0x200},
                {tig_sound_type.TIG_ST_THREE_D, 0x400},
            };

        [TempleDllLocation(0x101e45b0)]
        [TemplePlusLocation("sound.cpp:76")]
        public int tig_sound_alloc_stream(out int streamId, tig_sound_type streamType)
        {
            if (!sound_initialized)
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
            stream.field130 = 64;
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
                        if (++streamIdx >= LastEffectStreamIdx)
                            streamIdx = FirstEffectStreamIdx;
                        if (++triesMade >= LastEffectStreamIdx)
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
                    for (var i = 2; i <= 5; i++)
                    {
                        if (!tig_sound_streams[i].active)
                        {
                            newStreamId = i;
                            break;
                        }
                    }

                    if (newStreamId == -1)
                    {
                        FreeStream(ringBufferStreamId);
                        newStreamId = ringBufferStreamId++;
                        if (ringBufferStreamId > 5)
                            ringBufferStreamId = 2;
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
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101e4640)]
        public void SetVolume(int streamType, int volume)
        {
            throw new NotImplementedException();
        }

        private struct tig_sound_stream
        {
            public bool active;
            public int flags;
            public int field8;
            public int fieldC;
            public int loopCount;
            public IntPtr mss_stream;
            public int mss_audiodata;
            public int mss_3dsample;
            public int field20;
            public int[] field24; // TODO  = new int[65]
            public int field128;
            public int volume;
            public int field130;
            public int field134;
            public int field138;
            public int field13c;
            public long x;
            public long y;
            public int field150;
            public int field154;
        };

        public void Dispose()
        {
            _soloud?.deinit();
            _soloud = null;
        }
    }
}