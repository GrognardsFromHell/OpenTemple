using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.TigSubsystems
{
    public struct TigMouseState
    {
        public int flags;
        public int mouseCenterX;
        public int mouseCenterY;
        public int cursorTexWidth;
        public int cursorTexHeight;
        public int cursorHotspotX;
        public int cursorHotspotY;
        public int x;
        public int y;
        public int field24;
    }

    public delegate void CursorDrawCallback(int x, int y, object userArg);

    public class TigMouse
    {
        private readonly Stack<string> _cursorStash = new Stack<string>();

        private TigMouseState mouseState;

        private TimePoint[] tig_mouse_button_time = new TimePoint[3];

        private bool mouseStoppedMoving = false;

        private TimePoint lastMousePosChange = TimePoint.Now;

        /// <summary>
        /// The time span in which the mouse must not move after being last moved until we send
        /// a message with the <see cref="MouseEventFlag.PosChangeSlow"/> flag.
        /// </summary>
        private static readonly TimeSpan DelayUntilMousePosStable = TimeSpan.FromMilliseconds(35);

        private const int FlagHideCursor = 1;

        private bool mMouseOutsideWnd = false;

        private Point mMmbReference = new Point(-1, -1);

        private static readonly int[] buttonStatePressed1 =
        {
            0x02, // Left button
            0x10, // Right button
            0x80 // Middle button
        };

        private static readonly int[] buttonStatePressed2 =
        {
            0x004, // Left button
            0x020, // Right button
            0x100 // Middle button
        };

        private static readonly int[] buttonStateReleased =
        {
            0x008, // Left button
            0x040, // Right button
            0x200 // Middle button
        };

        private enum MouseEventAction
        {
            Released,
            Pressed,
            Unknown
        }

        public void GetState(out TigMouseState state)
        {
            state = this.mouseState;
        }

        public void SetButtonState(MouseButton button, bool pressed)
        {
            int buttonIndex = (int) button;

            var currentFlags = mouseState.flags;
            mouseState.flags = 0;

            var now = TimePoint.Now;

            if (pressed)
            {
                if ((currentFlags & (buttonStatePressed2[buttonIndex] | buttonStatePressed1[buttonIndex])) != 0)
                {
                    mouseState.flags = buttonStatePressed2[buttonIndex];
                    // Clicked less than 250ms after the first click?
                    if ((now - tig_mouse_button_time[buttonIndex]).TotalMilliseconds <= 250)
                    {
                        if ((currentFlags & FlagHideCursor) != 0)
                            mouseState.flags |= FlagHideCursor;
                        return;
                    }

                    mouseState.flags |= buttonStatePressed1[buttonIndex];
                    if ((currentFlags & 1) != 0)
                        mouseState.flags |= 1;
                    tig_mouse_button_time[buttonIndex] = now;

                    QueueMouseButtonMessage(button, MouseEventAction.Unknown);
                    return;
                }

                mouseState.flags = buttonStatePressed1[buttonIndex];
                if ((currentFlags & 1) != 0)
                    mouseState.flags |= 1;
                tig_mouse_button_time[buttonIndex] = now;

                QueueMouseButtonMessage(button, MouseEventAction.Pressed);
            }
            else
            {
                if (((buttonStatePressed1[buttonIndex] | buttonStatePressed2[buttonIndex]) & currentFlags) == 0)
                {
                    if ((currentFlags & 1) != 0)
                        mouseState.flags |= 1;
                    return;
                }

                mouseState.flags = buttonStateReleased[buttonIndex];
                if ((currentFlags & 1) != 0)
                    mouseState.flags |= 1;
                tig_mouse_button_time[buttonIndex] = TimePoint.Now;

                QueueMouseButtonMessage(button, MouseEventAction.Released);
            }
        }

        private void QueueMouseButtonMessage(MouseButton button, MouseEventAction action)
        {
            var msg = new Message(MessageType.MOUSE);
            msg.arg1 = mouseState.x;
            msg.arg2 = mouseState.y;
            msg.arg3 = 0;
            msg.arg4 = (int) GetMouseEventFlag(button, action);
            Tig.MessageQueue.Enqueue(msg);
        }

        private static MouseEventFlag GetMouseEventFlag(MouseButton button, MouseEventAction action)
        {
            switch (button)
            {
                case MouseButton.LEFT:
                    switch (action)
                    {
                        case MouseEventAction.Released:
                            return MouseEventFlag.LeftReleased;
                        case MouseEventAction.Pressed:
                            return MouseEventFlag.LeftClick;
                        case MouseEventAction.Unknown:
                            return MouseEventFlag.LeftDown;
                    }

                    break;
                case MouseButton.RIGHT:
                    switch (action)
                    {
                        case MouseEventAction.Released:
                            return MouseEventFlag.RightReleased;
                        case MouseEventAction.Pressed:
                            return MouseEventFlag.RightClick;
                        case MouseEventAction.Unknown:
                            return MouseEventFlag.RightDown;
                    }

                    break;
                case MouseButton.MIDDLE:
                    switch (action)
                    {
                        case MouseEventAction.Released:
                            return MouseEventFlag.MiddleReleased;
                        case MouseEventAction.Pressed:
                            return MouseEventFlag.MiddleClick;
                        case MouseEventAction.Unknown:
                            return MouseEventFlag.MiddleDown;
                    }

                    break;
            }

            throw new ArgumentOutOfRangeException();
        }

        public void SetMmbReference()
        {
            if (mMmbReference.X == -1 && mMmbReference.Y == -1)
            {
                mMmbReference = GetPos();
            }
        }

        public Point GetMmbReference()
        {
            return mMmbReference;
        }

        public void ResetMmbReference()
        {
            mMmbReference = new Point(-1, -1);
        }

        public void ShowCursor()
        {
            mouseState.flags &= ~FlagHideCursor;
        }

        public void HideCursor()
        {
            mouseState.flags |= FlagHideCursor;
        }

        public void SetCursor(string cursorPath)
        {
            _cursorStash.Push(cursorPath);

            int hotspotX = 0;
            int hotspotY = 0;

            // Special handling for cursors that don't have their hotspot on 0,0
            if (cursorPath.Contains("Map_GrabHand_Closed.tga")
                || cursorPath.Contains("Map_GrabHand_Open.tga")
                || cursorPath.Contains("SlidePortraits.tga"))
            {
                var data = Tig.FS.ReadBinaryFile(cursorPath);
                var info = IO.Images.ImageIO.DetectImageFormat(data);

                hotspotX = info.width / 2;
                hotspotY = info.height / 2;
            }

            Tig.RenderingDevice.SetCursor(hotspotX, hotspotY, cursorPath);
        }

        // This is sometimes queried by ToEE to check which callback is active
        // It contains the callback function's address
        public CursorDrawCallback CursorDrawCallback { get; private set; }

        // Extra argument passed to cursor draw callback.
        public object CursorDrawCallbackArg { get; private set; }

        [TempleDllLocation(0x101DD5C0)]
        public void SetCursorDrawCallback(CursorDrawCallback callback, object arg = null)
        {
            CursorDrawCallback = callback;
            CursorDrawCallbackArg = arg;
        }

        public Point GetPos() => new Point(mouseState.x, mouseState.y);

        [TempleDllLocation(0x101DD010)]
        public void SetBounds(Size screenSize)
        {
            // TODO: Only used during DInput mouse polling, which we dont use
        }

        public void MouseOutsideWndSet(bool outside)
        {
            mMouseOutsideWnd = outside;
        }

        public bool MouseOutsideWndGet()
        {
            return mMouseOutsideWnd;
        }

        [TempleDllLocation(0x101DD070)]
        public void SetPos(int x, int y, int wheelDelta)
        {
            MouseEventFlag eventFlags = 0;

            if (x != mouseState.x || y != mouseState.y)
            {
                mouseStoppedMoving = false;
                lastMousePosChange = TimePoint.Now;
                eventFlags = MouseEventFlag.PosChange;
            }
            else if (!mouseStoppedMoving && TimePoint.Now - lastMousePosChange > DelayUntilMousePosStable)
            {
                mouseStoppedMoving = true;
                eventFlags |= MouseEventFlag.PosChangeSlow;
            }

            if (wheelDelta != 0)
            {
                eventFlags |= MouseEventFlag.ScrollWheelChange;
            }

            mouseState.x = x;
            mouseState.y = y;

            if (((buttonStatePressed1[0] | buttonStatePressed2[0]) & mouseState.flags) != 0)
            {
                eventFlags |= GetMouseEventFlag(MouseButton.LEFT, MouseEventAction.Unknown);
            }

            if (((buttonStatePressed1[1] | buttonStatePressed2[1]) & mouseState.flags) != 0)
            {
                eventFlags |= GetMouseEventFlag(MouseButton.RIGHT, MouseEventAction.Unknown);
            }

            if (((buttonStatePressed1[2] | buttonStatePressed2[2]) & mouseState.flags) != 0)
            {
                eventFlags |= GetMouseEventFlag(MouseButton.MIDDLE, MouseEventAction.Unknown);
            }

            if (eventFlags != 0)
            {
                var args = new MessageMouseArgs(
                    x, y, wheelDelta, eventFlags
                );
                Tig.MessageQueue.Enqueue(new Message(args));
            }
        }
    }
}