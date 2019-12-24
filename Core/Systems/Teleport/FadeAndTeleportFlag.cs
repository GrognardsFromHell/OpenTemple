using System;

namespace OpenTemple.Core.Systems.Teleport
{
    [Flags]
    public enum FadeAndTeleportFlags : uint
    {
        play_movie = 1,
        FadeOut = 2,
        FadeIn = 4,
        advance_time = 8,
        play_sound = 0x10,
        unk20 = 0x20,
        play_movie2 = 0x40,
        CenterOnPartyLeader = 0x200,
        EnableCallback = 0x400,
        EnableCallbackArg = 0x800,
        unk80000000 = 0x80000000
    };
}