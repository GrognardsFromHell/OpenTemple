using System.Drawing;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.Ui.Events;

public class MouseEvent : UiEvent
{
    public float X { get; init; }
    public float Y { get; init; }

    public PointF Pos => new PointF(X, Y);

    public MouseButton Button { get; init; }

    public MouseButtons Buttons { get; init; } = default;

    public bool IsLeftButtonHeld => (Buttons & MouseButtons.Left) != default;
    public bool IsRightButtonHeld => (Buttons & MouseButtons.Right) != default;
    public bool IsMiddleButtonHeld => (Buttons & MouseButtons.Middle) != default;

    public bool IsCtrlHeld { get; init; }
    public bool IsAltHeld { get; init; }
    public bool IsShiftHeld { get; init; }
    public bool IsMetaHeld { get; init; }
}
