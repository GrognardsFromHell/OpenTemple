namespace OpenTemple.Core.Systems.Anim;

public struct AnimStateTransition {
    public uint newState;
    // Delay in milliseconds or one of the constants below
    public int delay;

    // Delay is read from runSlot->animDelay
    public const int DelaySlot = -2;
    // Delay is read from 0x10307534 (written by some goal states)
    public const int DelayCustom = -3;
    // Specifies a random 0-300 ms delay
    public const int DelayRandom = -4;
}