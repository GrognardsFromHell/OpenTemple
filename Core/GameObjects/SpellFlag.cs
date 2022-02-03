using System;

namespace OpenTemple.Core.GameObjects
{
    [Flags]
    public enum SpellFlag : uint
    {
        INVISIBLE = 1,
        FLOATING = 2,
        BODY_OF_AIR = 4,
        BODY_OF_EARTH = 8,
        BODY_OF_FIRE = 0x10,
        BODY_OF_WATER = 0x20,
        DETECTING_MAGIC = 0x40,
        DETECTING_ALIGNMENT = 0x80,
        DETECTING_TRAPS = 0x100,
        DETECTING_INVISIBLE = 0x200,
        SHIELDED = 0x400,
        ANTI_MAGIC_SHELL = 0x800,
        BONDS_OF_MAGIC = 0x1000,
        FULL_REFLECTION = 0x2000,
        SUMMONED = 0x4000,
        ILLUSION = 0x8000,
        STONED = 0x10000,
        POLYMORPHED = 0x20000,
        MIRRORED = 0x40000,
        SHRUNK = 0x80000,
        PASSWALLED = 0x100000,
        WATER_WALKING = 0x200000,
        MAGNETIC_INVERSION = 0x400000,
        CHARMED = 0x800000,
        ENTANGLED = 0x1000000,
        SPOKEN_WITH_DEAD = 0x2000000,
        TEMPUS_FUGIT = 0x4000000,
        MIND_CONTROLLED = 0x8000000,
        DRUNK = 0x10000000,
        ENSHROUDED = 0x20000000,
        FAMILIAR = 0x40000000,
        HARDENED_HANDS = 0x80000000
    }
}