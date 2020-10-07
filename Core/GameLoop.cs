using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core
{
    public class GameLoop : IGameLoop, ITaskQueue
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        // The thread that tasks will be processed on
        private readonly Thread _thread = Thread.CurrentThread;

        private readonly Queue<Action> _taskQueue = new Queue<Action>();

        public event Action OnFrame;

        public bool IsInThread => Thread.CurrentThread == _thread;

        public Task<T> PostTask<T>(Func<T> work)
        {
            if (IsInThread)
            {
                return Task.FromResult(work());
            }

            var completionSource = new TaskCompletionSource<T>();

            void WorkOnUiThread()
            {
                try
                {
                    completionSource.SetResult(work());
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            }

            lock (_taskQueue)
            {
                _taskQueue.Enqueue(WorkOnUiThread);
            }

            return completionSource.Task;
        }

        public Task<T> PostTask<T>(Func<Task<T>> work)
        {
            var completionSource = new TaskCompletionSource<T>();

            async void WorkOnUiThread()
            {
                try
                {
                    completionSource.SetResult(await work());
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            }

            lock (_taskQueue)
            {
                _taskQueue.Enqueue(WorkOnUiThread);
            }

            return completionSource.Task;
        }


        private bool DequeueWork(out Action work)
        {
            lock (_taskQueue)
            {
                if (_taskQueue.TryDequeue(out work))
                {
                    return true;
                }
            }

            return false;
        }

        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.Assert(IsInThread);

                while (DequeueWork(out var work))
                {
                    try
                    {
                        work();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Work on main thread failed: {0}", e);
                    }
                }

                OnFrame?.Invoke();
                Thread.Yield();
            }
        }
    }
}