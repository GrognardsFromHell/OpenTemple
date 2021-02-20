using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using OpenTemple.Core.Scenes;

namespace OpenTemple.Core.Platform
{
    public class StageManager : IStage
    {
        private readonly IMainWindow _mainWindow;

        private readonly Stack<SceneStackEntry> _sceneStack = new();

        public StageManager(IMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            PushScene(EmptyScene.Instance);
        }

        public Task PushScene(IScene scene)
        {
            if (_sceneStack.TryPeek(out var currentScene))
            {
                _mainWindow.RemoveMainContent(currentScene.Control);
            }

            var stackEntry = new SceneStackEntry(scene);
            _sceneStack.Push(stackEntry);
            _mainWindow.AddMainContent(stackEntry.Control);
            return stackEntry.CompletionSource.Task;
        }

        public bool TryPopScene(IScene scene)
        {
            if (_sceneStack.TryPeek(out var currentScene) && currentScene.Scene == scene)
            {
                var entry = _sceneStack.Pop();
                _mainWindow.RemoveMainContent(entry.Control);
                entry.CompletionSource.SetResult();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    readonly struct SceneStackEntry
    {
        public TaskCompletionSource CompletionSource { get; }

        public IScene Scene { get; }

        public IControl Control { get; }

        public SceneStackEntry(IScene scene)
        {
            CompletionSource = new TaskCompletionSource();
            Scene = scene;
            Control = scene.UiContent;
        }
    }
}
