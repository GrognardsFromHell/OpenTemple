namespace SpicyTemple.Core.Systems.Pathfinding
{
    public class PathQueryResult : Path
    {
        // Sometimes, a pointer to the following two values is passed as "pPauseTime" (see 100131F0)
        public bool occupiedFlag; // TODO: This may be a TimePoint
        public int someDelay;
    }
}