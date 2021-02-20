using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Scenes
{
    public class InGameScene : Canvas, IScene
    {
        public InGameScene()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IControl UiContent => this;
    }
}
