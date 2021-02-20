using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Scenes
{
    public class MainMenuScene : Canvas, IScene
    {
        public MainMenuScene()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IControl UiContent => this;
    }
}
