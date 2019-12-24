using System;

namespace OpenTemple.Core.GameObject
{
    [Flags]
    public enum CritterFlag : uint
    {
        IS_CONCEALED = 0x1,
        MOVING_SILENTLY = 0x2,
        EXPERIENCE_AWARDED = 0x4,
        UNUSED_00000008 = 0x8,
        FLEEING = 0x10,
        STUNNED = 0x20,
        PARALYZED = 0x40,
        BLINDED = 0x80,
        HAS_ARCANE_ABILITY = 0x100,
        UNUSED_00000200 = 0x200,
        UNUSED_00000400 = 0x400,
        UNUSED_00000800 = 0x800,
        SLEEPING = 0x1000,
        MUTE = 0x2000,
        SURRENDERED = 0x4000,
        MONSTER = 0x8000,
        SPELL_FLEE = 0x10000,
        ENCOUNTER = 0x20000,
        COMBAT_MODE_ACTIVE = 0x40000,
        LIGHT_SMALL = 0x80000,
        LIGHT_MEDIUM = 0x100000,
        LIGHT_LARGE = 0x200000,
        LIGHT_XLARGE = 0x400000,
        UNREVIVIFIABLE = 0x800000,
        UNRESURRECTABLE = 0x1000000,
        UNUSED_02000000 = 0x2000000,
        UNUSED_04000000 = 0x4000000,
        NO_FLEE = 0x8000000,
        NON_LETHAL_COMBAT = 0x10000000,
        MECHANICAL = 0x20000000,
        UNUSED_40000000 = 0x40000000,
        FATIGUE_LIMITING = 0x80000000
    }
}