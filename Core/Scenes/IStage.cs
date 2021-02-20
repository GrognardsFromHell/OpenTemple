using System;
using System.Threading.Tasks;

namespace OpenTemple.Core.Scenes
{
    public interface IStage
    {
        Task PushScene(IScene scene);

        bool TryPopScene(IScene scene);
    }
}
