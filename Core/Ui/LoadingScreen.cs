using System;
using System.Threading.Tasks;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using Qml.Net;

namespace OpenTemple.Core.Ui
{
    public class LoadingScreen : IDisposable, ILoadingProgress
    {
        private readonly IMainWindow _mainWindow;

        private readonly QmlFiles.Loading _view;

        private LoadingScreen(IMainWindow mainWindow, QmlFiles.Loading view)
        {
            _view = view;
            _view.OnDestroyed += _ => {
                Console.WriteLine("VIEW GOT DESTROOOOOOOYED");
            };
            _mainWindow = mainWindow;
        }

        public string Message
        {
            set => _mainWindow.PostTask(() => { _view.message = value; });
        }

        public float Progress
        {
            set => _mainWindow.PostTask(() => { _view.progress = value; });
        }

        public void Dispose()
        {
            // TODO _view.Dispose();
        }

        public static async Task<ILoadingProgress> Create(IMainWindow mainWindow)
        {
            var view = new QmlFiles.Loading(await mainWindow.LoadViewNative("Loading.qml"));
            return new LoadingScreen(mainWindow, view);
        }
    }
}