#nullable enable
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;

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
        SetPos(rect.Location);
        SetSize(rect.Size);
    }

    protected override void DefaultMouseDownAction(MouseEvent e)
    {
        if (Disabled)
        {
            e.PreventDefault();
            return;
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
        if (sndClick != -1)
        {
            Tig.Sound.PlaySoundEffect(sndClick);
        }
    }

    public int sndHoverOff { get; set; } = -1;

    public int sndHoverOn { get; set; } = -1;

    public int sndDown { get; set; } = -1;

    public int sndClick { get; set; } = -1;

    protected override void HandleMouseEnter(MouseEvent e)
    {
        base.HandleMouseEnter(e);

        if (sndHoverOn != -1)
        {
            Tig.Sound.PlaySoundEffect(sndHoverOn);
        }
    }

    protected override void HandleMouseLeave(MouseEvent e)
    {
        base.HandleMouseLeave(e);
        
        if (sndHoverOff != -1)
        {
            Tig.Sound.PlaySoundEffect(sndHoverOff);
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