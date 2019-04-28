using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum ItemFlag : uint
    {
        IDENTIFIED = 0x1,
        WONT_SELL = 0x2,
        IS_MAGICAL = 0x4,
        NO_PICKPOCKET = 0x8,
        NO_DISPLAY = 0x10,
        NO_DROP = 0x20,
        NEEDS_SPELL = 0x40,
        CAN_USE_BOX = 0x80,
        NEEDS_TARGET = 0x100,
        LIGHT_SMALL = 0x200,
        LIGHT_MEDIUM = 0x400,
        LIGHT_LARGE = 0x800,
        LIGHT_XLARGE = 0x1000,
        PERSISTENT = 0x2000,
        MT_TRIGGERED = 0x4000,
        STOLEN = 0x8000,
        USE_IS_THROW = 0x10000,
        NO_DECAY = 0x20000,
        UBER = 0x40000,
        NO_NPC_PICKUP = 0x80000,
        NO_RANGED_USE = 0x100000,
        VALID_AI_ACTION = 0x200000,
        DRAW_WHEN_PARENTED = 0x400000,
        EXPIRES_AFTER_USE = 0x800000,
        NO_LOOT = 0x1000000,
        USES_WAND_ANIM = 0x2000000,
        NO_TRANSFER = 0x4000000,
        NO_TRANSFER_SPECIAL = 0x8000000
    }
}