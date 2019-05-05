using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum SceneryFlag : uint
    {
        NO_AUTO_ANIMATE = 0x1,
        BUSTED = 0x2,
        NOCTURNAL = 0x4,
        MARKS_TOWNMAP = 0x8,
        IS_FIRE = 0x10,
        RESPAWNABLE = 0x20,
        SOUND_SMALL = 0x40,
        SOUND_MEDIUM = 0x80,
        SOUND_EXTRA_LARGE = 0x100,
        UNDER_ALL = 0x200,
        RESPAWNING = 0x400,
        TAGGED_SCENERY = 0x800,
        USE_OPEN_WORLDMAP = 0x1000
    }
}