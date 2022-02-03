using System;

namespace OpenTemple.Core.Systems.Anim;

[Flags]
public enum AnimStateTransitionFlags : uint
{
    UNK_1000000 = 0x1000000,
    GOAL_INVALIDATE_PATH = 0x2000000,
    UNK_4000000 = 0x4000000,
    REWIND = 0x10000000, // will transition back to state 0
    POP_GOAL = 0x30000000,
    POP_GOAL_TWICE = 0x38000000,
    PUSH_GOAL = 0x40000000,
    UNK_70000000 = 0x70000000,
    POP_ALL = 0x90000000,
    MASK = 0xFF000000 // the general mask for the special state transition flags
}