using System.Drawing;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.Events;
using static SDL2.SDL;

namespace OpenTemple.Core.Systems.Movies;

// Overrides normal UI and captures keypress and mouse events to skip the movies
public class MoviePlayerUi : IUiRoot
{
    public bool KeyPressed { get; private set; }

    public void KeyDown(SDL_Keycode virtualKey, SDL_Scancode physicalKey, KeyModifier modifiers, bool repeat)
    {
        KeyPressed = true;
    }

    public void MouseUp(Point windowPos, PointF uiPos, MouseButton button)
    {
        if (button == MouseButton.LEFT)
        {
            KeyPressed = true;
        }
    }
}