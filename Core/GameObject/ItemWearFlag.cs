using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum ItemWearFlag
    {
        HELMET = 1,
        NECKLACE = 2,
        GLOVES = 4,
        UNUSED_1 = 8,
        UNUSED_2 = 0x10,
        ARMOR = 0x20,
        RING = 0x40,
        UNUSED_3 = 0x80,
        BOOTS = 0x100,
        AMMO = 0x200,
        CLOAK = 0x400,
        BUCKLER = 0x800,
        ROBES = 0x1000,
        BRACERS = 0x2000,
        BARDIC_ITEM = 0x4000,
        LOCKPICKS = 0x8000,
        TWOHANDED_REQUIRED = 0x10000
    }
}