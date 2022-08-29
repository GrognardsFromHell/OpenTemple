using System;
using System.Drawing;
using OpenTemple.Core.Config;

namespace OpenTemple.Core.Platform;

public interface IMainWindow : IDisposable
{
    
    void SetWindowMsgFilter(SDLEventFilter filter);

    /// <summary>
    /// Invoked when the window changes size. Given size will be the new client area of the window.
    /// </summary>
    event Action<Size> Resized;

    /// <summary>
    /// Invoked when the player closes the window.
    /// </summary>
    event Action Closed;

    public IUiRoot? UiRoot { get; set; }
    
    WindowConfig WindowConfig { get; set; }

    SizeF UiCanvasSize { get; }

    event Action UiCanvasSizeChanged;

    float UiScale { get; }

    void SetCursor(int hotspotX, int hotspotY, string imagePath);

    bool IsCursorVisible { set; }
    
    void ProcessEvents();
}