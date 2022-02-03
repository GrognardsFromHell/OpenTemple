using System;

namespace OpenTemple.Core.GameObjects
{
    [Flags]
    public enum NpcFlag : uint
    {
        EX_FOLLOWER = 0x1,
        WAYPOINTS_DAY = 0x2,
        WAYPOINTS_NIGHT = 0x4,
        AI_WAIT_HERE = 0x8,
        AI_SPREAD_OUT = 0x10,
        JILTED = 0x20,
        LOGBOOK_IGNORES = 0x40,
        UNUSED_00000080 = 0x80,
        KOS = 0x100,
        USE_ALERTPOINTS = 0x200, // 0x200
        FORCED_FOLLOWER = 0x400,
        KOS_OVERRIDE = 0x800,
        WANDERS = 0x1000,
        WANDERS_IN_DARK = 0x2000,
        FENCE = 0x4000,
        FAMILIAR = 0x8000,
        CHECK_LEADER = 0x10000,
        NO_EQUIP = 0x20000,
        CAST_HIGHEST = 0x40000,
        GENERATOR = 0x80000,
        GENERATED = 0x100000,
        GENERATOR_RATE1 = 0x200000,
        GENERATOR_RATE2 = 0x400000,
        GENERATOR_RATE3 = 0x800000,
        DEMAINTAIN_SPELLS = 16777216,
        UNUSED_02000000 = 33554432,
        UNUSED_04000000 = 67108864,
        UNUSED_08000000 = 134217728,
        BACKING_OFF = 0x10000000,
        NO_ATTACK = 0x20000000,
        BOSS_MONSTER = 0x40000000,
        EXTRAPLANAR = 0x80000000
    }

}