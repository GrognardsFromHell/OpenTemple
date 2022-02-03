using System;

namespace OpenTemple.Core.Systems.D20;

[Flags]
public enum MonsterSubtype
{
    air = 0x1,
    aquatic = 0x2,
    extraplanar = 0x4,
    cold = 0x8,
    chaotic = 0x10,
    demon = 0x20,
    devil = 0x40,
    dwarf = 0x80,
    earth = 0x100,
    electricity = 0x200,
    elf = 0x400,
    evil = 0x800,
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