using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;
using SDL2;

namespace OpenTemple.Core.Platform;

public enum MessageType : uint
{
    CHAR = 4, // arg1 is the character, in Virtual Key terms
    KEYSTATECHANGE = 5,

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
