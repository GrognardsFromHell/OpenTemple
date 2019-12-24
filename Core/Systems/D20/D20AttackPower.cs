using System;

namespace OpenTemple.Core.Systems.D20
{
    [Flags]
    public enum D20AttackPower : uint
    {
        NORMAL = 0x0,
        UNSPECIFIED = 0x1,
        SILVER = 0x2,
        MAGIC = 0x4,
        HOLY = 0x8,
        UNHOLY = 0x10,
        CHAOS = 0x20,
        LAW = 0x40,
        ADAMANTIUM = 0x80,
        BLUDGEONING = 0x100,
        PIERCING = 0x200,
        SLASHING = 0x400,
        MITHRIL = 0x800,
        COLD = 0x1000
    }
}