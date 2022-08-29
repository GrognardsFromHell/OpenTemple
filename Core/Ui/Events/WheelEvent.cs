namespace OpenTemple.Core.Ui.Events;

public class WheelEvent : MouseEvent
{
    public float DeltaX { get; init; }
    public float DeltaY { get; init; }
    public float DeltaZ { get; init; }
}
