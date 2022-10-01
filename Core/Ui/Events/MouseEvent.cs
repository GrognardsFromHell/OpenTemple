using System;
using System.Drawing;
using OpenTemple.Core.Platform;
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
}

public enum MouseButton : int
{
    LEFT = 0,
    RIGHT = 1,
    MIDDLE = 2,
    EXTRA1 = 3,
    EXTRA2 = 4,
    Unchanged = 0
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
