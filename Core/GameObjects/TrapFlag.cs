using System;

namespace OpenTemple.Core.GameObjects;

[Flags]
public enum TrapFlag : uint
{
    UNUSED_01 = 0x1,
    BUSTED = 0x2
}