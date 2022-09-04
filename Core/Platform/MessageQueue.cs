using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;
using SDL2;

namespace OpenTemple.Core.Platform;

public enum MouseButton : int
{
    LEFT = 0,
    RIGHT = 1,
    MIDDLE = 2,
    EXTRA1 = 3,
    EXTRA2 = 4,
    Unchanged = 0
}

[Flags]
public enum MouseButtons : byte
{
    Left = 0x01,
    Right = 0x02,
    Middle = 0x04,
    Extra1 = 0x08,
    Extra2 = 0x10
}

public enum MessageType : uint
{
    EXIT = 3, // may be exit game, queued on WM_CLOSE and WM_QUIT
    CHAR = 4, // arg1 is the character, in Virtual Key terms
    KEYSTATECHANGE = 5,

    /*
        Send once whenever system message are being processed.
        No arguments, just the create time is set to the time
        the messages were processed.
    */
    UPDATE_TIME = 6,
    KEYDOWN = 8,
    HOTKEY_ACTION = 9
}

public struct ExitMessageArgs
{
    public readonly int Code;

    public ExitMessageArgs(int code)
    {
        Code = code;
    }
}

public struct MessageKeyStateChangeArgs
{
    public SDL.SDL_Keycode key;
    public SDL.SDL_Scancode scancode;
    public bool down;
    public bool modAlt;
    public bool modCtrl;
    public bool modShift;
}

public readonly struct MessageCharArgs
{
    public readonly string Text;

    public MessageCharArgs(string text)
    {
        Text = text;
    }
}

public class Message
{
    public readonly TimePoint created;
    public readonly MessageType type;
    private object args;

    public Message(MessageType type)
    {
        this.type = type;
        created = TimePoint.Now;
    }

    public Message(ExitMessageArgs messageArgs) : this(MessageType.EXIT)
    {
        args = messageArgs;
    }

    public Message(MessageCharArgs messageArgs) : this(MessageType.CHAR)
    {
        CharArgs = messageArgs;
    }

    public Message(MessageKeyStateChangeArgs keyArgs) : this(MessageType.KEYSTATECHANGE)
    {
        KeyStateChangeArgs = keyArgs;
    }

    public MessageKeyStateChangeArgs KeyStateChangeArgs
    {
        get
        {
            Trace.Assert(type == MessageType.KEYSTATECHANGE);
            return (MessageKeyStateChangeArgs) args;
        }
        private init
        {
            Trace.Assert(type == MessageType.KEYSTATECHANGE);
            args = value;
        }
    }

    public MessageCharArgs CharArgs
    {
        get
        {
            Trace.Assert(type == MessageType.CHAR);
            return (MessageCharArgs) args;
        }
        private init
        {
            Trace.Assert(type == MessageType.CHAR);
            args = value;
        }
    }
}

public class HotkeyActionMessage : Message
{
    public HotkeyActionMessage(Hotkey hotkey) : base(MessageType.HOTKEY_ACTION)
    {
        Hotkey = hotkey;
    }
    
    public Hotkey Hotkey { get; }
    
    public bool IsHandled { get; private set; }

    public void SetHandled()
    {
        IsHandled = true;
    }
}
