namespace SpicyTemple.Core.Systems.AI
{
    public enum LockStatus
    {
        PLS_OPEN = 0,
        PLS_LOCKED = 1,
        PLS_JAMMED = 2,
        PLS_MAGICALLY_HELD = 3,
        PLS_DENIED_BY_SCRIPT = 4,
        PLS_INVALID_OPENER = 5,
        PLS_SECRET_UNDISCOVERED = 6,
    }
}