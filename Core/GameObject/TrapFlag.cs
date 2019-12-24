using System;

namespace OpenTemple.Core.GameObject
{
    [Flags]
    public enum TrapFlag : uint
    {
        UNUSED_01 = 0x1,
        BUSTED = 0x2
    }
}