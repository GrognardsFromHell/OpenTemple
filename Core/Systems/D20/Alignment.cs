using System;

namespace SpicyTemple.Core.Systems.D20
{
    [Flags]
    public enum Alignment
    {
        NEUTRAL = 0,
        LAWFUL = 1,
        CHAOTIC = 2,
        GOOD = 4,
        EVIL = 8,

        TRUE_NEUTRAL = 0,
        LAWFUL_NEUTRAL = 1,
        CHAOTIC_NEUTRAL = 2,
        NEUTRAL_GOOD = 4,
        LAWFUL_GOOD = 5,
        CHAOTIC_GOOD = 6,
        NEUTRAL_EVIL = 8,
        LAWFUL_EVIL = 9,
        CHAOTIC_EVIL = 10,
    }

}