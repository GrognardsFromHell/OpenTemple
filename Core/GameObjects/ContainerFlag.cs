using System;

namespace OpenTemple.Core.GameObjects
{
    [Flags]
    public enum ContainerFlag : uint
    {
        LOCKED = 0x1,
        JAMMED = 0x2,
        MAGICALLY_HELD = 0x4,
        NEVER_LOCKED = 0x8,
        ALWAYS_LOCKED = 0x10,
        LOCKED_DAY = 0x20,
        LOCKED_NIGHT = 0x40,
        BUSTED = 0x80,
        NOT_STICKY = 0x100,
        INVEN_SPAWN_ONCE = 0x200,
        INVEN_SPAWN_INDEPENDENT = 0x400,
        OPEN = 0x800,
        HAS_BEEN_OPENED = 0x1000
    };
}