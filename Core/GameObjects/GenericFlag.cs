using System;

namespace OpenTemple.Core.GameObjects;

[Flags]
public enum GenericFlag
{
    UNUSED_00000001 = 0x1,
    IS_LOCKPICK = 0x2,
    IS_TRAP_DEVICE = 0x4,
    UNUSED_00000008 = 0x8,
    IS_GRENADE = 0x10
}