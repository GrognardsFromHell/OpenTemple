namespace OpenTemple.Core.Ui.Widgets;

/// <summary>
/// Defines how a widget participates in hit-testing to find the target
/// of mouse events at a specific x,y coordinate on screen.
/// </summary>
public enum HitTestingMode
{
  
    /// <summary>
    /// Any point within the <see cref="WidgetBase.BorderArea">Border Area</see>
    /// </summary>
    Area,

    /// <summary>
    /// Hit test against the content of this widget, causing areas not covered by content to be transparent to hit-testing.
    /// </summary>
    Content,
    
    /// <summary>
    /// Causes this widget to always fail hit-testing. This means it becomes invisible to mouse events. Children in the
    /// widget can still participate in hit testing normally.
    /// </summary>
    Ignore
}
