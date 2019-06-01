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
                if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Clicked)
                {
                    if (mClickHandler != null && !mDisabled)
                    {
                        var contentArea = GetContentArea();
                        int x = widgetMsg.x - contentArea.X;
                        int y = widgetMsg.y - contentArea.Y;
                        mClickHandler(x, y);
                        mLastClickTriggered = TimePoint.Now;
                    }

                    return true;
                }
                else if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Entered)
                {
                    OnMouseEnter?.Invoke(widgetMsg);
                }
                else if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Exited)
                {
                    OnMouseExit?.Invoke(widgetMsg);
                }
            }

            return base.HandleMessage(msg);
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
            if (mRepeat & mButton.buttonState == LgcyButtonState.Down)
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

        protected LgcyButton mButton;
        protected bool mDisabled = false;

        protected bool
            mActive = false; // is the state associated with the button active? Note: this is separate from mDisabled, which determines if the button itself is disabled or not

        protected bool mRepeat = false;
        protected TimeSpan mRepeatInterval = TimeSpan.FromMilliseconds(200);
        protected TimePoint mLastClickTriggered;

        public delegate void ClickHandler(int x, int y);

        private ClickHandler mClickHandler;
    };
}