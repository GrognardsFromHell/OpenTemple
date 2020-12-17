using System;

namespace OpenTemple.Core.Ui.DOM
{
    public class KeyboardEvent : KeyboardModifierEvent
    {
        public KeyboardEvent(string type, KeyboardEventInit eventInit = null) : this(SystemEventType.Custom, type,
            eventInit)
        {
        }

        public KeyboardEvent(SystemEventType type, KeyboardEventInit eventInit = null) : this(type, null, eventInit)
        {
        }

        public KeyboardEvent(SystemEventType systemType, string type, KeyboardEventInit eventInit = null) : base(
            systemType, type, eventInit)
        {
            if (eventInit != null)
            {
                Repeat = eventInit.Repeat;
                Code = eventInit.Code;
                Key = eventInit.Key;
                Text = eventInit.Text;
                IsComposing = eventInit.IsComposing;
            }
        }

        /// <summary>
        /// Indicates that the event is for an automated repeat (the user is holding down the key).
        /// </summary>
        public bool Repeat { get; }

        /// <summary>
        /// This is equivalent to a name for the scan code of the pressed key.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A constant value for the key.
        /// </summary>
        public KeyboardKey Key { get; }

        /// <summary>
        /// When pressing a key that represents a printable character,
        /// this will contain that character. This is a string since
        /// some characters (i.e. Emojis) cannot be represented using
        /// a single char value in C# since that is UTF-16.
        /// </summary>
        public string Text { get; }
        
        public bool IsComposing { get; }
        
        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is KeyboardEventInit kbInit)
            {
                kbInit.Repeat = Repeat;
                kbInit.Code = Code;
                kbInit.Text = Text;
                kbInit.Key = Key;
                kbInit.IsComposing = IsComposing;
            }
        }

        public override EventImpl Copy()
        {
            var init = new KeyboardEventInit();
            CopyTo(init);
            return new KeyboardEvent(SystemType, Type, init);
        }
    }

    public class KeyboardEventInit : EventModifierInit
    {
        public bool Repeat { get; set; }

        public string Code { get; set; }

        public KeyboardKey Key { get; set; }
        
        public string Text { get; set; }
        
        public bool IsComposing { get; set; }
    }
}