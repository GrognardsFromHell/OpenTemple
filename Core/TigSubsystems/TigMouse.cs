using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.TigSubsystems;

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
    private readonly Stack<string> _cursorStash = new();

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

    // Millisecond interval in which mouse button held messages are sent
    private static readonly TimeSpan HeldMessageInterval = TimeSpan.FromMilliseconds(250);

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
        Held
    }

    public TigMouse()
    {
        // Move the mouse so it's initially outside the window
        mouseState.x = -1;
        mouseState.y = -1;
    }

    public void GetState(out TigMouseState state)
    {
        state = mouseState;
    }

    public void SetButtonState(MouseButton button, bool pressed)
    {
        var buttonIndex = (int)button;
        var newFlags = 0;
        var now = TimePoint.Now;

        if (pressed)
        {
            if (!IsPressed(button))
            {
                newFlags = buttonStatePressed1[buttonIndex];
                tig_mouse_button_time[buttonIndex] = now;
                QueueMouseButtonMessage(button, MouseEventAction.Pressed);
            }
        }
        else
        {
            if (IsPressed(button))
            {
                newFlags = buttonStateReleased[buttonIndex];
                tig_mouse_button_time[buttonIndex] = TimePoint.Now;

                QueueMouseButtonMessage(button, MouseEventAction.Released);
            }
        }

        if ((mouseState.flags & FlagHideCursor) != 0)
        {
            newFlags |= FlagHideCursor;
        }

        mouseState.flags = newFlags;
    }

    private bool IsPressed(MouseButton button)
    {
        var buttonIndex = (int)button;
        return (mouseState.flags & (buttonStatePressed1[buttonIndex] | buttonStatePressed2[buttonIndex])) != 0;
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
                    case MouseEventAction.Held:
                        return MouseEventFlag.LeftHeld;
                }

                break;
            case MouseButton.RIGHT:
                switch (action)
                {
                    case MouseEventAction.Released:
                        return MouseEventFlag.RightReleased;
                    case MouseEventAction.Pressed:
                        return MouseEventFlag.RightClick;
                    case MouseEventAction.Held:
                        return MouseEventFlag.RightHeld;
                }

                break;
            case MouseButton.MIDDLE:
                switch (action)
                {
                    case MouseEventAction.Released:
                        return MouseEventFlag.MiddleReleased;
                    case MouseEventAction.Pressed:
                        return MouseEventFlag.MiddleClick;
                    case MouseEventAction.Held:
                        return MouseEventFlag.MiddleHeld;
                }

                break;
        }

        throw new ArgumentOutOfRangeException();
    }

    public void ShowCursor()
    {
        mouseState.flags &= ~FlagHideCursor;
        ShowCursor(true);
    }

    public void HideCursor()
    {
        mouseState.flags |= FlagHideCursor;
        ShowCursor(false);
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

        // TODO: Introduce special "cursor" file type that includes hotspot info to remove these hardcoded values
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
        else if (cursorPath.Contains("ZoomCursor.tga"))
        {
            // This was previously set from the townmap UI via function @ 0x101dd4a0
            hotspotX = 10;
            hotspotY = 11;
        }

        Tig.MainWindow.SetCursor(hotspotX, hotspotY, cursorPath);
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

    public Point GetPos() => new(mouseState.x, mouseState.y);

    public bool IsMouseOutsideWindow { get; set; } = true;

    /// <summary>
    /// we no longer call SetPos if nothing has changed, so we need this function to trigger the PosChangeSlow
    /// event.
    /// </summary>
    [TempleDllLocation(0x101dd7c0)]
    public void AdvanceTime()
    {
        var now = TimePoint.Now;

        if (!mouseStoppedMoving && now - lastMousePosChange > DelayUntilMousePosStable)
        {
            var eventFlags = MouseEventFlag.PosChangeSlow | GetEventFlagsFromButtonState();
            mouseStoppedMoving = true;
            var args = new MessageMouseArgs(
                mouseState.x, mouseState.y, 0, eventFlags
            );
            Tig.MessageQueue.Enqueue(new Message(args));
        }

        // This sends messages if buttons are held for 250ms or longer.
        // This was previously handled by the DirectInput polling code in 0x101dd7c0
        void TrySendHeldMessage(MouseButton button)
        {
            if (IsPressed(button))
            {
                var buttonIndex = (int)button;
                mouseState.flags = buttonStatePressed2[buttonIndex];
                // Send a mouse button held message every 250ms after it was initially pressed
                if (now - tig_mouse_button_time[buttonIndex] > HeldMessageInterval)
                {
                    mouseState.flags |= buttonStatePressed1[buttonIndex];
                    tig_mouse_button_time[buttonIndex] = now;

                    QueueMouseButtonMessage(button, MouseEventAction.Held);
                }
            }
        }

        TrySendHeldMessage(MouseButton.LEFT);
        TrySendHeldMessage(MouseButton.RIGHT);
        TrySendHeldMessage(MouseButton.MIDDLE);
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

        if (_cursorLocked)
        {
            SetCursorPos(_cursorLockPos.X, _cursorLockPos.Y);
        }
    }

    private MouseEventFlag GetEventFlagsFromButtonState()
    {
        MouseEventFlag eventFlags = default;

        if (((buttonStatePressed1[0] | buttonStatePressed2[0]) & mouseState.flags) != 0)
        {
            eventFlags |= GetMouseEventFlag(MouseButton.LEFT, MouseEventAction.Held);
        }

        if (((buttonStatePressed1[1] | buttonStatePressed2[1]) & mouseState.flags) != 0)
        {
            eventFlags |= GetMouseEventFlag(MouseButton.RIGHT, MouseEventAction.Held);
        }

        if (((buttonStatePressed1[2] | buttonStatePressed2[2]) & mouseState.flags) != 0)
        {
            eventFlags |= GetMouseEventFlag(MouseButton.MIDDLE, MouseEventAction.Held);
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

    private bool _cursorLocked;

    [TempleDllLocation(0x10d251c0)] [TempleDllLocation(0x10d25580)]
    private Point _cursorLockPos;

    [TempleDllLocation(0x101ddee0)]
    public void PushCursorLock()
    {
        GetCursorPos(out _cursorLockPos);
        _cursorLocked = true;
        HideCursor();
    }

    [TempleDllLocation(0x101dd470)]
    public void PopCursorLock()
    {
        if (_cursorLocked)
        {
            _cursorLocked = false;
            SetCursorPos(_cursorLockPos.X, _cursorLockPos.Y);
            ShowCursor();
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point point);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int ShowCursor(bool show);
}