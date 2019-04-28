using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum NpcFlag : uint
    {
        EX_FOLLOWER = 1,
        WAYPOINTS_DAY = 2,
        WAYPOINTS_NIGHT = 4,
        AI_WAIT_HERE = 8,
        AI_SPREAD_OUT = 16,
        JILTED = 32,
        LOGBOOK_IGNORES = 64,
        UNUSED_00000080 = 128,
        KOS = 256,
        USE_ALERTPOINTS = 512, // 0x200
        FORCED_FOLLOWER = 1024,
        KOS_OVERRIDE = 2048,
        WANDERS = 4096,
        WANDERS_IN_DARK = 8192,
        FENCE = 16384,
        FAMILIAR = 32768,
        CHECK_LEADER = 65536,
        NO_EQUIP = 131072,
        CAST_HIGHEST = 262144,
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