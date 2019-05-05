using System;

namespace SpicyTemple.Core.Systems.D20
{
    [Flags]
    public enum D20AttackPower : uint
    {
        NORMAL = 0x1,
        UNSPECIFIED = 0x2,
        SILVER = 0x4,
        MAGIC = 0x8,
        HOLY = 0x10,
        UNHOLY = 0x20,
        CHAOS = 0x40,
        LAW = 0x80,
        ADAMANTIUM = 0x100,
        BLUDGEONING = 0x200,
        PIERCING = 0x400,
        SLASHING = 0x800,
        MITHRIL = 0x1000,
        COLD = 0x2000
    }
}