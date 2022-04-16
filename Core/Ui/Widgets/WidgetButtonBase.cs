#nullable enable
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetButtonBase : WidgetBase
{
    private readonly WidgetTooltipRenderer _tooltipRenderer = new ();

    public bool ClickOnMouseDown { get; set; } = false;

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

    protected bool mDisabled = false;

    protected bool mRepeat = false;
    protected TimeSpan mRepeatInterval = TimeSpan.FromMilliseconds(200);
    protected TimePoint mLastClickTriggered;

    public delegate void ClickHandler(int x, int y);

    private ClickHandler? mClickHandler;

    public event ClickHandler? OnRightClick;

    public event Action<MessageWidgetArgs>? OnMouseEnter;

    public event Action<MessageWidgetArgs>? OnMouseExit;

    public WidgetButtonBase([CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1)
        : base(filePath, lineNumber)
    {
    }

    public WidgetButtonBase(Rectangle rect, [CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1) : this(filePath, lineNumber)
    {
        SetPos(rect.Location);
        SetSize(rect.Size);
    }

    public override bool HandleMessage(Message msg)
    {
        if (msg.type == MessageType.WIDGET)
        {
            MessageWidgetArgs widgetMsg = msg.WidgetArgs;
            if (mClickHandler != null
                && (ClickOnMouseDown && widgetMsg.widgetEventType == TigMsgWidgetEvent.Clicked
                    || !ClickOnMouseDown && widgetMsg.widgetEventType == TigMsgWidgetEvent.MouseReleased))
            {
                if (mClickHandler != null && !mDisabled)
                {
                    var contentArea = GetContentArea();
                    int x = widgetMsg.x - contentArea.X;
                    int y = widgetMsg.y - contentArea.Y;
                    mClickHandler(x, y);
                    mLastClickTriggered = TimePoint.Now;
                }
            }
            else if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Entered)
            {
                OnMouseEnter?.Invoke(widgetMsg);
            }
            else if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Exited)
            {
                OnMouseExit?.Invoke(widgetMsg);
            }

            if (mWidgetMsgHandler != null)
            {
                return mWidgetMsgHandler(widgetMsg);
            }

            return true;
        }

        return base.HandleMessage(msg);
    }

    public override bool HandleMouseMessage(MessageMouseArgs msg)
    {
        if (ClickOnMouseDown && (msg.flags & MouseEventFlag.RightClick) != 0
            || !ClickOnMouseDown && (msg.flags & MouseEventFlag.RightReleased) != 0)
        {
            var clickHandler = OnRightClick;
            if (!mDisabled && clickHandler != null)
            {
                var contentArea = GetContentArea();
                var x = msg.X - contentArea.X;
                var y = msg.Y - contentArea.Y;
                clickHandler(x, y);
                return true;
            }
        }

        if (base.HandleMouseMessage(msg))
        {
            return true;
        }

        if ((msg.flags & MouseEventFlag.ScrollWheelChange) != default)
        {
            // Let mouse-wheel messages through to the closest scroll-view
            return false;
        }
        
        return true; // Always swallow mouse messages by default to prevent buttons from being click-through
    }

    public LgcyButtonState ButtonState { get; set; }

    public int sndHoverOff { get; set; } = -1;

    public int sndHoverOn { get; set; } = -1;

    public int sndDown { get; set; } = -1;

    public int sndClick { get; set; } = -1;

    public void SetDisabled(bool disabled)
    {
        mDisabled = disabled;
    }

    public bool IsDisabled()
    {
        return mDisabled;
    }

    public void SetClickHandler(Action handler)
    {
        mClickHandler = (x, y) => handler();
    }

    public void SetClickHandler(ClickHandler handler)
    {
        mClickHandler = handler;
    }

    public override bool IsButton()
    {
        return true;
    }

    public bool IsRepeat()
    {
        return mRepeat;
    }

    public void SetRepeat(bool enable)
    {
        mRepeat = enable;
        ClickOnMouseDown = true;
    }

    public TimeSpan GetRepeatInterval()
    {
        return mRepeatInterval;
    }

    public void SetRepeatInterval(TimeSpan interval)
    {
        mRepeatInterval = interval;
    }

    public override void OnUpdateTime(TimePoint timeMs)
    {
        if (mRepeat && ButtonState == LgcyButtonState.Down)
        {
            var pos = Tig.Mouse.GetPos();
            if (mClickHandler != null && !mDisabled && mLastClickTriggered + mRepeatInterval < timeMs)
            {
                var contentArea = GetContentArea();
                int x = pos.X - contentArea.X;
                int y = pos.Y - contentArea.Y;
                mClickHandler(x, y);
                mLastClickTriggered = TimePoint.Now;
            }
        }
    }

    public override void RenderTooltip(int x, int y)
    {
        _tooltipRenderer.Render(x, y);
    }

    public override bool HasPseudoClass(StylingState stylingState)
    {
        if (stylingState == StylingState.Hover)
        {
            return ButtonState == LgcyButtonState.Hovered;
        }

        return base.HasPseudoClass(stylingState);
    }
}