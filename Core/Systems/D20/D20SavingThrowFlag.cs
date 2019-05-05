using System;

namespace SpicyTemple.Core.Systems.D20
{
    [Flags]
    public enum D20SavingThrowFlag : uint
    {
        MAX = 0,
        NONE = 0,
        REROLL = 0x1,
        CHARM = 0x2,
        TRAP = 0x4,
        POISON = 0x8,
        SPELL_LIKE_EFFECT = 0x10,
        SPELL_SCHOOL_ABJURATION = 0x20,
        SPELL_SCHOOL_CONJURATION = 0x40,
        SPELL_SCHOOL_DIVINATION = 0x80,
        SPELL_SCHOOL_ENCHANTMENT = 0x100,
        SPELL_SCHOOL_EVOCATION = 0x200,
        SPELL_SCHOOL_ILLUSION = 0x400,
        SPELL_SCHOOL_NECROMANCY = 0x800,
        SPELL_SCHOOL_TRANSMUTATION = 0x1000,
        SPELL_DESCRIPTOR_ACID = 0x2000,
        SPELL_DESCRIPTOR_CHAOTIC = 0x4000,
        SPELL_DESCRIPTOR_COLD = 0x8000,
        SPELL_DESCRIPTOR_DARKNESS = 0x10000,
        SPELL_DESCRIPTOR_DEATH = 0x20000,
        SPELL_DESCRIPTOR_ELECTRICITY = 0x40000,
        SPELL_DESCRIPTOR_EVIL = 0x80000,
        SPELL_DESCRIPTOR_FEAR = 0x100000,
        SPELL_DESCRIPTOR_FIRE = 0x200000,
        SPELL_DESCRIPTOR_FORCE = 0x400000,
        SPELL_DESCRIPTOR_GOOD = 0x800000,
        SPELL_DESCRIPTOR_LANGUAGE_DEPENDENT = 0x1000000,
        SPELL_DESCRIPTOR_LAWFUL = 0x2000000,
        SPELL_DESCRIPTOR_LIGHT = 0x4000000,
        SPELL_DESCRIPTOR_MIND_AFFECTING = 0x8000000,
        SPELL_DESCRIPTOR_SONIC = 0x10000000, // might be an offset here
        SPELL_DESCRIPTOR_TELEPORTATION = 0x20000000,
        SPELL_DESCRIPTOR_AIR = 0x40000000,
        SPELL_DESCRIPTOR_EARTH = 0x80000000,
        SPELL_DESCRIPTOR_WATER = 33, // <- This one might not even work anymore...
        DISABLE_SLIPPERY_MIND = 34
    };
}