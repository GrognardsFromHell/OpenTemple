using System;
using System.Threading.Tasks;
using OpenTemple.Core.Platform;
using Qml.Net;

namespace OpenTemple.Core.Ui
{
    public class Scene
    {
    }

    /// <summary>
    /// Manages the primary scene currently on screen.
    /// </summary>
    public class SceneManager
    {
        public static SceneManager Instance { get; set; }

        private readonly IMainWindow _mainWindow;

        public SceneManager(IMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public Task<IntPtr> LoadScene(string path)
        {
            return _mainWindow.LoadViewNative(path);
        }
    }
}