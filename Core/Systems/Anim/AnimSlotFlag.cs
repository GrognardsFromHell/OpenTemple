using System;

namespace OpenTemple.Core.Systems.Anim;

[Flags]
public enum AnimSlotFlag {
    ACTIVE = 1,
    STOP_PROCESSING = 2, // Used in context with "killing the animation slot"
    UNK3 = 4, // Seen in goalstatefunc_82, goalstatefunc_83, set with 0x8 in
    // goalstatefunc_42
    UNK4 = 8, // Seen in goalstatefunc_82, goalstatefunc_83, set with 0x8 in
    // goalstatefunc_42
    // Might mean the slot is currently animating (?)
    UNK5 = 0x10, // Seen in goalstatefunc_84_animated_forever, set in
    // goalstatefunc_87
    UNK7 = 0x20, // Seen as 0x30 is cleared in goalstatefunc_7 and goalstatefunc_8
    RUNNING = 0x40,
    SPEED_RECALC = 0x80,
    UNK_100 = 0x100,
    UNK_200 = 0x200,
    UNK8 = 0x400,   // Seen in goal_calc_path_to_loc, goalstatefunc_13_rotate,
    // set in goalstatefunc_18
    UNK10 = 0x800,  // Seen in goalstatefunc_37
    UNK9 = 0x4000,  // Seen in goalstatefunc_19
    UNK11 = 0x8000, // Test in goalstatefunc_43
    UNK1 = 0x10000,
    UNK6 = 0x20000, // Probably sound related (seen in anim_goal_free_sound_id)
    UNK12 = 0x40000 // set goalstatefunc_48, checked in goalstatefunc_50
}