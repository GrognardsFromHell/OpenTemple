using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetButtonBase : WidgetBase
    {
        private readonly WidgetTooltipRenderer _tooltipRenderer = new WidgetTooltipRenderer();

        public bool ClickOnMouseDown { get; set; } = false;

        public TooltipStyle TooltipStyle
        {
            get => _tooltipRenderer.TooltipStyle;
            set => _tooltipRenderer.TooltipStyle = value;
        }

        public string TooltipText
        {
            get => _tooltipRenderer.TooltipText;
            set => _tooltipRenderer.TooltipText = value;
        }

        public WidgetButtonBase([CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1)
            : base(filePath, lineNumber)
        {
            var button = new LgcyButton();

            // This is our special sauce...
            button.render = id => Render();
            button.handleMessage = (id, msg) => HandleMessage(msg);

            var widgetId = Globals.UiManager.AddButton(button);
            Globals.UiManager.SetAdvancedWidget(widgetId, this);
            mButton = Globals.UiManager.GetButton(widgetId);
            mWidget = mButton;
        }

        public WidgetButtonBase(Rectangle rect, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : this(filePath, lineNumber)
        {
            SetPos(rect.Location);
            SetSize(rect.Size);
        }

        public event Action<MessageWidgetArgs> OnMouseEnter;

        public event Action<MessageWidgetArgs> OnMouseExit;

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

            base.HandleMouseMessage(msg);
            return true; // Always swallow mouse messages by default to prevent buttons from being click-through
        }

        public LgcyButtonState ButtonState => mButton.buttonState;

        public void SetDisabled(bool disabled)
        {
            mDisabled = disabled;
        }

        public bool IsDisabled()
        {
            return mDisabled;
        }

        public void SetActive(bool isActive)
        {
            mActive = isActive;
        }

        public bool IsActive()
        {
            return mActive;
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
            if (mRepeat && mButton.buttonState == LgcyButtonState.Down)
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

        protected LgcyButton mButton;
        protected bool mDisabled = false;

        protected bool
            mActive = false; // is the state associated with the button active? Note: this is separate from mDisabled, which determines if the button itself is disabled or not

        protected bool mRepeat = false;
        protected TimeSpan mRepeatInterval = TimeSpan.FromMilliseconds(200);
        protected TimePoint mLastClickTriggered;

        public delegate void ClickHandler(int x, int y);

        private ClickHandler mClickHandler;

        public event ClickHandler OnRightClick;
    };
}