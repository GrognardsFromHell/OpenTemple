using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum PortalFlag : uint
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
        OPEN = 0x200
    }
}