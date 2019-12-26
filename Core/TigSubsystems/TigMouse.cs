using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.TigSubsystems
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

        [TempleDllLocation(0x10D2558C)]
        private ResourceRef<ITexture> _iconUnderCursor;

        private Point _iconUnderCursorCenter;

        private Size _iconUnderCursorSize;

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
            Tig.MessageQueue.Enqueue(new Message(
                new MessageMouseArgs(mouseState.x, mouseState.y, 0, GetMouseEventFlag(button, action)))
            );
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
            SetCursorInternal(cursorPath);
        }

        private void SetCursorInternal(string cursorPath)
        {
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

        [TempleDllLocation(0x101DD770)]
        [TemplePlusLocation("tig_mouse.cpp:180")]
        public void ResetCursor()
        {
            // The back is the one on screen
            if (_cursorStash.Count > 0)
            {
                _cursorStash.Pop();
            }

            // The back is the one on screen
            if (_cursorStash.TryPeek(out var texturePath))
            {
                SetCursorInternal(texturePath);
            }
        }

        [TempleDllLocation(0x101dd500)]
        public void SetDraggedIcon(string texturePath, Point center, Size size = default)
        {
            using var texture = Tig.Textures.Resolve(texturePath, false);
            SetDraggedIcon(texture.Resource, center, size);
        }

        public void SetDraggedIcon(ITexture texture, Point center, Size size = default)
        {
            if (texture != null)
            {
                if (size.IsEmpty)
                {
                    _iconUnderCursorSize = texture.GetSize();
                }
                else
                {
                    _iconUnderCursorSize = size;
                }

                _iconUnderCursorCenter = center;
                _iconUnderCursor.Dispose();
                _iconUnderCursor = texture.Ref();
            }
            else
            {
                _iconUnderCursor.Dispose();
            }
        }

        [TempleDllLocation(0x101dd500, true)]
        public void ClearDraggedIcon()
        {
            _iconUnderCursor.Dispose();
            _iconUnderCursorCenter = Point.Empty;
            _iconUnderCursorSize = Size.Empty;
        }

        // This is sometimes queried by ToEE to check which callback is active
        // It contains the callback function's address
        [TempleDllLocation(0x101dd5e0)]
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

        public void MouseOutsideWndSet(bool outside)
        {
            mMouseOutsideWnd = outside;
        }

        public bool MouseOutsideWndGet()
        {
            return mMouseOutsideWnd;
        }

        /// <summary>
        /// we no longer call SetPos if nothing has changed, so we need this function to trigger the PosChangeSlow
        /// event.
        /// </summary>
        [TempleDllLocation(0x101dd7c0)]
        public void AdvanceTime()
        {
            if (!mouseStoppedMoving && TimePoint.Now - lastMousePosChange > DelayUntilMousePosStable)
            {
                var eventFlags = MouseEventFlag.PosChangeSlow | GetEventFlagsFromButtonState();
                mouseStoppedMoving = true;
                var args = new MessageMouseArgs(
                    mouseState.x, mouseState.y, 0, eventFlags
                );
                Tig.MessageQueue.Enqueue(new Message(args));
            }
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

            eventFlags |= GetEventFlagsFromButtonState();

            if (eventFlags != 0)
            {
                var args = new MessageMouseArgs(
                    x, y, wheelDelta, eventFlags
                );
                Tig.MessageQueue.Enqueue(new Message(args));
            }
        }

        private MouseEventFlag GetEventFlagsFromButtonState()
        {
            MouseEventFlag eventFlags = default;

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

            return eventFlags;
        }

        [TempleDllLocation(0x101dd330, true)]
        public void DrawTooltip()
        {
            CursorDrawCallback?.Invoke(mouseState.x, mouseState.y, CursorDrawCallbackArg);
        }

        [TempleDllLocation(0x101dd330, true)]
        public void DrawItemUnderCursor()
        {
            if (!_iconUnderCursor.IsValid)
            {
                return;
            }

            Tig.ShapeRenderer2d.DrawRectangle(
                mouseState.x + _iconUnderCursorCenter.X,
                mouseState.y + _iconUnderCursorCenter.Y,
                _iconUnderCursorSize.Width,
                _iconUnderCursorSize.Height,
                _iconUnderCursor.Resource
            );
        }

    }
}