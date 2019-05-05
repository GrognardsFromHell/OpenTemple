using System;

namespace SpicyTemple.Core.Systems.D20
{
    [Flags]
    public enum MonsterSubtype
    {
        air = 1,
        aquatic = 2,
        extraplanar = 4,
        cold = 8,
        chaotic = 16,
        demon = 32,
        devil = 64,
        dwarf = 128,
        earth = 256,
        electricity = 512,
        elf = 1024,
        evil = 2048,
        fire = 0x1000,
        formian = 0x2000,
        gnoll = 0x4000,
        gnome = 0x8000,
        goblinoid = 0x10000,
        good = 0x20000,
        guardinal = 0x40000,
        half_orc = 0x80000,
        halfling = 0x100000,
        human = 0x200000,
        lawful = 0x400000,
        incorporeal = 0x800000,
        orc = 0x1000000,
        reptilian = 0x2000000,
        slaadi = 0x4000000,
        water = 0x8000000
    }
}