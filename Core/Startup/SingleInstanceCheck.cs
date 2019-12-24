using System;
using System.Threading;

namespace OpenTemple.Core.Startup
{
    /// <summary>
    /// Checks that the game is only running once at a time.
    /// </summary>
    public sealed class SingleInstanceCheck : IDisposable
    {
        private readonly Mutex _mutex;

        public SingleInstanceCheck()
        {
            _mutex = new Mutex(false, "OpenTempleMutex", out var newMutex);
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