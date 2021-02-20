using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Scenes
{
    public class LoadingScene : Canvas, IScene
    {
        public LoadingScene() : this(new LoadingScreenViewModel())
        {
        }

        public LoadingScene(LoadingScreenViewModel model)
        {
            DataContext = model;
            AvaloniaXamlLoader.Load(this);
        }

        public IControl UiContent => this;
    }

    public class LoadingScreenViewModel : ReactiveObject
    {
        private double _progress;

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref this._progress, value);
        }

        private IBitmap _image;

        public IBitmap Image
        {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }
    }
}
