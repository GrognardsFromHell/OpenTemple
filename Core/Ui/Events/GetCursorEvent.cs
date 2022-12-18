
namespace OpenTemple.Core.Ui.Events;

public class GetCursorEvent : MouseEvent
{
    /// <summary>
    /// Sets whether the mouse cursor should be visible or not.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Path to the cursor definition file that should be used for the mouse cursor.
    /// </summary>
    public string? Cursor { get; set; }
}
