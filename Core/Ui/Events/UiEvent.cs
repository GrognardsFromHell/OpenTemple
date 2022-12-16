using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Events;

public class UiEvent
{
    public UiEventType Type { get; init; }
    
    public WidgetBase InitialTarget { get; init; }

    public bool IsDefaultPrevented { get; private set; }

    public bool IsImmediatePropagationStopped { get; private set; }

    public bool IsPropagationStopped { get; private set; }

    /// <summary>
    /// Call this to prevent default behavior associated with this event.
    /// </summary>
    public void PreventDefault() => IsDefaultPrevented = true;

    /// <summary>
    /// Call this to prevent other registered listeners on the current target from being called for this event.
    /// </summary>
    public void StopImmediatePropagation() => IsImmediatePropagationStopped = true;

    /// <summary>
    /// Call this to prevent this event from propagating (bubbling) up to all ancestors of the current target.
    /// </summary>
    public void StopPropagation() => IsPropagationStopped = true;
}