using System;
using System.Threading;

namespace SpicyTemple.Core.Startup
{
    /// <summary>
    /// Checks that the game is only running once at a time.
    /// </summary>
    public sealed class SingleInstanceCheck : IDisposable
    {
        private readonly Mutex _mutex;

        public SingleInstanceCheck()
        {
            _mutex = new Mutex(false, "SpicyTempleMutex", out var newMutex);
            if (!newMutex)
            {
                _mutex.Dispose();
                throw new AlreadyRunningException();
            }
        }

        public void Dispose()
        {
            _mutex.Dispose();
        }
    }
}