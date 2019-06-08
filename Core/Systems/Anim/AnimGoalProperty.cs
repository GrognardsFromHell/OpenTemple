namespace SpicyTemple.Core.Systems.Anim
{
    public enum AnimGoalProperty {
        SELF_OBJ = 0,     // Type: 1 (Object)
        TARGET_OBJ,       // Type: 1
        BLOCK_OBJ,        // Type: 1
        SCRATCH_OBJ,      // Type: 1
        PARENT_OBJ,       // Type: 1
        TARGET_TILE,      // Type: 2 (Location)
        RANGE_DATA,       // Type: 2
        ANIM_ID,          // Type: 0 (just a 32-bit number it seems)
        ANIM_ID_PREV, // Type: 0
        ANIM_DATA,        // Type: 0
        SPELL_DATA,       // Type: 0
        SKILL_DATA,       // Type: 0
        FLAGS_DATA,       // Type: 0
        SCRATCH_VAL1,     // Type: 0
        SCRATCH_VAL2,     // Type: 0
        SCRATCH_VAL3,     // Type: 0
        SCRATCH_VAL4,     // Type: 0
        SCRATCH_VAL5,     // Type: 0
        SCRATCH_VAL6,     // Type: 0
        SOUND_HANDLE,      // Type: 0

        SELF_OBJ_PRECISE_LOC = 31,
        TARGET_OBJ_PRECISE_LOC = 32,
        NULL_HANDLE = 33,
        TARGET_LOC_PRECISE = 34
    }
}