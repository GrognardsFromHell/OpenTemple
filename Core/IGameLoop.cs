using System;
using System.Threading.Tasks;

namespace OpenTemple.Core
{

    public interface ITaskQueue
    {
        bool IsInThread { get; }

        Task PostTask(Action work)
        {
            return PostTask(() =>
            {
                work();
                return true;
            });
        }

        Task<T> PostTask<T>(Func<T> work);

        Task<T> PostTask<T>(Func<Task<T>> work);

    }

    public interface IGameLoop
    {
        event Action OnFrame;
    }
}