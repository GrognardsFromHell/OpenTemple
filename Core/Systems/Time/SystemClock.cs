using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Time;

public static class SystemClock
{

    /**
         * Emulates the timeGetTime windows function.
         */
    public static long timeGetTime()
    {
        return (long) TimePoint.Now.Milliseconds;
    }

}