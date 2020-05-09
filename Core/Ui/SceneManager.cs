using System;
using System.Threading.Tasks;
using OpenTemple.Core.Platform;
using Qml.Net;
using QtQuick;

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

        public Task<T> LoadScene<T>(string path) where T : Item
        {
            return _mainWindow.LoadView<T>(path);
        }
    }
}