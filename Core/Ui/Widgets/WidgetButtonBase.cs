#nullable enable
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetButtonBase : WidgetBase
{
    public string? TooltipStyle { get; set; }

    public string? TooltipText
    {
        get => _tooltipText;
        set
        {
            _tooltipContent = null;
            _tooltipText = value;
        }
    }

    public InlineElement? TooltipContent
    {
        get => _tooltipContent;
        set
        {
            _tooltipText = null;
            _tooltipContent = value;
        }
    }

    public int SoundMouseLeave { get; set; } = -1;

    public int SoundMouseEnter { get; set; } = -1;

    public int SoundPressed { get; set; } = -1;

    public int SoundClicked { get; set; } = -1;

    /// <summary>
    /// A repeating button triggers a click event immediately when pressed using the primary mouse button,
    /// and continuously at regular intervals while the mouse button remains held. Triggering will pause
    /// while the mouse cursor is not over the button, but will resume when it is moved back onto the button.
    /// </summary>
    public bool IsRepeat { get; set; }

    /// <summary>
    /// The interval in which repeat events trigger while the mouse is being held.
    /// </summary>
    public TimeSpan RepeatInterval { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// The initial mouse-down event that causes the button to repeatedly trigger.
    /// </summary>
    private MouseEvent? _repeatingEvent;

    private TimePoint _repeatingEventTime;

    // Suppresses sound events. Used to suppress sound events when the button is triggered via repeated events.
    private bool _suppressSound;
    private InlineElement? _tooltipContent;
    private string? _tooltipText;

    public WidgetButtonBase([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : base(filePath, lineNumber)
    {
        FocusMode = FocusMode.User;
    }

    public WidgetButtonBase(Rectangle rect, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : this(filePath, lineNumber)
    {
        Pos = rect.Location;
        Size = rect.Size;
    }

    protected override void DefaultMouseDownAction(MouseEvent e)
    {
        if (Disabled)
        {
            e.PreventDefault();
            return;
        }

        if (SoundPressed != -1)
        {
            Tig.Sound.PlaySoundEffect(SoundPressed);
        }

        if (IsRepeat && e.Button == MouseButton.Left && SetMouseCapture())
        {
            _repeatingEvent = e;
            TriggerAction(e);
            e.PreventDefault(); // Prevent normal click handling                
        }
    }

    protected override void DefaultMouseUpAction(MouseEvent e)
    {
        if (IsRepeat && e.Button == MouseButton.Left)
        {
            ReleaseMouseCapture();
            _repeatingEvent = null;
            e.PreventDefault();
        }
    }

    private void TriggerAction(MouseEvent e)
    {
        _suppressSound = true;
        try
        {
            DispatchClick(e); // TODO: should translate mouse event here
        }
        finally
        {
            _suppressSound = false;
        }

        _repeatingEventTime = TimePoint.Now;
    }

    protected override void HandleClick(MouseEvent e)
    {
        base.HandleClick(e);
        if (!_suppressSound && SoundClicked != -1)
        {
            Tig.Sound.PlaySoundEffect(SoundClicked);
        }
    }

    protected override void HandleMouseEnter(MouseEvent e)
    {
        base.HandleMouseEnter(e);

        if (SoundMouseEnter != -1)
        {
            Tig.Sound.PlaySoundEffect(SoundMouseEnter);
        }
    }

    protected override void HandleMouseLeave(MouseEvent e)
    {
        base.HandleMouseLeave(e);

        if (SoundMouseLeave != -1)
        {
            Tig.Sound.PlaySoundEffect(SoundMouseLeave);
        }
    }

    public override void OnUpdateTime(TimePoint now)
    {
        if (IsRepeat && _repeatingEvent != null && !Disabled)
        {
            if (_repeatingEventTime + RepeatInterval < now && ContainsMouse)
            {
                TriggerAction(_repeatingEvent);
            }
        }
    }

    protected override void HandleTooltip(TooltipEvent e)
    {
        if (_tooltipContent != null)
        {
            e.Content = _tooltipContent;
            if (TooltipStyle != null)
            {
                e.StyleId = TooltipStyle;
            }
        }
        else if (_tooltipText != null)
        {
            e.TextContent = _tooltipText;
            if (TooltipStyle != null)
            {
                e.StyleId = TooltipStyle;
            }
        }
    }
}