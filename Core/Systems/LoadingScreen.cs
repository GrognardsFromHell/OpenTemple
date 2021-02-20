using System;
using Avalonia.Media.Imaging;
using OpenTemple.Core.Scenes;

namespace OpenTemple.Core.Systems
{
    public class LoadingScreen : IDisposable, ILoadingProgress
    {
        private readonly LoadingScreenViewModel _model;
        private readonly LoadingScene _scene;
        private readonly IStage _stage;

        public LoadingScreen(IStage stage)
        {
            _stage = stage;
            _model = new LoadingScreenViewModel();
            _scene = new LoadingScene(_model);
            _stage.PushScene(_scene);

            _model.Image = new Bitmap("art/splash/legal0322.img");

            Update();
        }

        public void Dispose()
        {
            _stage.TryPopScene(_scene);
        }

        public string Message
        {
            get => _model.StatusText;
            set => _model.StatusText = value;
        }

        public double Progress
        {
            get => _model.Progress;
            set => _model.Progress = value;
        }

        public void Update()
        {
            Globals.GameLoop.RunOneIteration(false);
        }
    }
}
