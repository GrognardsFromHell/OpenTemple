#nullable enable
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetButtonBase : WidgetBase
{
    private readonly WidgetTooltipRenderer _tooltipRenderer = new ();

    public string TooltipStyle
    {
        get => _tooltipRenderer.TooltipStyle;
        set => _tooltipRenderer.TooltipStyle = value;
    }

    public string? TooltipText
    {
        get => _tooltipRenderer.TooltipText;
        set => _tooltipRenderer.TooltipText = value;
    }

    public InlineElement? TooltipContent
    {
        get => _tooltipRenderer.TooltipContent;
        set => _tooltipRenderer.TooltipContent = value;
    }

    public bool IsRepeat { get; set; }
    
    public TimeSpan RepeatInterval { get; set; } = TimeSpan.FromMilliseconds(200);
    
    private MouseEvent? _repeatingEvent;
    private TimePoint _repeatingEventTime;

    public WidgetButtonBase([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : base(filePath, lineNumber)
    {
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
        
        if (IsRepeat && e.Button == MouseButton.LEFT)
        {
            if (SetMouseCapture())
            {
                _repeatingEvent = e;
                TriggerAction(e);
                e.PreventDefault(); // Prevent normal click handling                
            }
        }
    }

    protected override void DefaultMouseUpAction(MouseEvent e)
    {
        if (IsRepeat && e.Button == MouseButton.LEFT)
        {
            ReleaseMouseCapture();
            _repeatingEvent = null;
            e.PreventDefault();
        }
    }
    
    private void TriggerAction(MouseEvent e)
    {
        DispatchClick(e); // TODO: should translate mouse event here
        _repeatingEventTime = TimePoint.Now;
    }

    protected override void HandleClick(MouseEvent e)
    {
        base.HandleClick(e);
        if (SoundClicked != -1)
        {
            Tig.Sound.PlaySoundEffect(SoundClicked);
        }
    }

    public int SoundMouseLeave { get; set; } = -1;

    public int SoundMouseEnter { get; set; } = -1;

    public int SoundPressed { get; set; } = -1;

    public int SoundClicked { get; set; } = -1;

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
                DispatchClick(_repeatingEvent);
                _repeatingEventTime = TimePoint.Now;
            }
        }
    }

    public override void RenderTooltip(int x, int y)
    {
        _tooltipRenderer.Render(x, y);
    }
}