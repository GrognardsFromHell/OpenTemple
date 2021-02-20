using Avalonia.Controls;

namespace OpenTemple.Core.Scenes
{
    public interface IScene
    {
        IControl UiContent { get; }
    }
}
