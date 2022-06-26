using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.TigSubsystems;

public class HeadlessMainWindow : IMainWindow
{
    public void SetWindowMsgFilter(WindowMsgFilter filter)
    {
    }

    public event Action<WindowEvent> OnInput;

    public event Action<Size> Resized;

    public event Action Closed;

    public Size OffScreenSize { get; set; } = new(1024, 768);

    public WindowConfig WindowConfig { get; set; }

    public void InvokeClosed()
    {
        Closed?.Invoke();
    }

    public SizeF UiCanvasSize => new(OffScreenSize.Width, OffScreenSize.Height);

    public event Action UiCanvasSizeChanged;

    public float UiScale { get; set; } = 1.0f;

    public void SetCursor(int hotspotX, int hotspotY, string imagePath)
    {
    }

    public bool IsCursorVisible { get; set; }
    
    public Size DragStartDistance => new (1, 1);

    public void Dispose()
    {
    }

    public void SendInput(WindowEvent obj)
    {
        OnInput?.Invoke(obj);
    }
    
    public PointF ToUiCanvas(Point screenPoint) => new Point(
        (int)(screenPoint.X / UiScale),
        (int)(screenPoint.Y / UiScale)
    );

    public Point FromUiCanvas(PointF uiPoint) => new Point(
        (int)(uiPoint.X * UiScale),
        (int)(uiPoint.Y * UiScale)
    );
    
    public void SendMouseEvent(WindowEventType type, Point screenPoint, MouseButton button)
    {
        SendInput(new MouseWindowEvent(
            type,
            this,
            screenPoint,
            ToUiCanvas(screenPoint)
        )
        {
            Button = button
        });
    }

    public void Click(Point screenPoint, MouseButton button = MouseButton.LEFT)
    {
        SendMouseEvent(WindowEventType.MouseDown, screenPoint, button);
        SendMouseEvent(WindowEventType.MouseUp, screenPoint, button);
    }

    public void ClickUi(float x, float y, MouseButton button = MouseButton.LEFT)
    {
        Click(FromUiCanvas(new PointF(x, y)), button);
    }
}