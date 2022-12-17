using System;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.Events;

/// <summary>
/// This event is dispatched when the UI has the opportunity to show a tooltip for an element of the UI.
/// This is often caused by the mouse hovering over an element. 
/// </summary>
public class TooltipEvent : MouseEvent
{
    public bool AlignLeft { get; set; }
    
    public InlineElement? Content { get; set; }

    public string? TextContent
    {
        set => Content = value == null ? null : new SimpleInlineElement(value);
    }

    public string? StyleId { get; set; } = null;
}