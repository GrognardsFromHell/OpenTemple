using System;
using System.Drawing;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Events;

public class MouseEvent : UiEvent
{
    public float X { get; init; }
    public float Y { get; init; }

    /// <summary>
    /// This may be different from initial target if the mouse is captured.
    /// </summary>
    public WidgetBase? MouseOverWidget { get; init; }

    public PointF Pos => new(X, Y);

    public MouseButton Button { get; init; }

    public MouseButtons Buttons { get; init; }

    public bool IsLeftButtonHeld => (Buttons & MouseButtons.Left) != default;
    public bool IsRightButtonHeld => (Buttons & MouseButtons.Right) != default;
    public bool IsMiddleButtonHeld => (Buttons & MouseButtons.Middle) != default;

    public bool IsCtrlHeld { get; init; }
    public bool IsAltHeld { get; init; }
    public bool IsShiftHeld { get; init; }
    public bool IsMetaHeld { get; init; }

    /// <summary>
    /// Returns <see cref="Pos"/> relative to the widgets content area.
    /// </summary>
    public PointF GetLocalPos(WidgetBase widget)
    {
        var contentRect = widget.GetViewportBorderArea();
        return new PointF(Pos.X - contentRect.X, Pos.Y - contentRect.Y);
    }
}

public enum MouseButton : int
{
    Unchanged = 0,
    Left = 1,
    Right = 2,
    Middle = 3,
    Extra1 = 4,
    Extra2 = 5,
}

[Flags]
public enum MouseButtons : byte
{
    Left = 0x01,
    Right = 0x02,
    Middle = 0x04,
    Extra1 = 0x08,
    Extra2 = 0x10
}
