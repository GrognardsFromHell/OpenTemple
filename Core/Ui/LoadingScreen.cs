using System;
using System.Threading.Tasks;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using QmlFiles;

namespace OpenTemple.Core.Ui
{
    public class LoadingScreen : ILoadingProgress
    {
        private readonly IMainWindow _mainWindow;

        private LoadingQml _view;

        private LoadingScreen(IMainWindow mainWindow, LoadingQml view)
        {
            _view = view;
            _view.OnDestroyed += _ => { Console.WriteLine("VIEW GOT DESTROOOOOOOYED"); };
            _mainWindow = mainWindow;
        }

        public string Message
        {
            set => _mainWindow.PostTask(() => { _view.Message = value; });
        }

        public float Progress
        {
            set => _mainWindow.PostTask(() => { _view.Progress = value; });
        }

        public void Dispose()
        {
            _view?.DeleteLater();
            _view = null;
        }

        public static async Task<ILoadingProgress> Create(IMainWindow mainWindow)
        {
            var view = await mainWindow.LoadView<LoadingQml>("Loading.qml");
            return new LoadingScreen(mainWindow, view);
        }
    }
}