using Avalonia.Controls;

namespace OpenTemple.Core.Scenes
{
    public class EmptyScene : IScene
    {
        public static EmptyScene Instance { get; } = new EmptyScene();

        private EmptyScene()
        {
        }

        public IControl UiContent => null;
    }
}
