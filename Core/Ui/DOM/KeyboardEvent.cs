using System;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.Ui.DOM
{
    public class KeyboardEvent : UiEvent
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
                CtrlKey = eventInit.CtrlKey;
                ShiftKey = eventInit.ShiftKey;
                AltKey = eventInit.AltKey;
                MetaKey = eventInit.MetaKey;
                Repeat = eventInit.Repeat;
                Code = eventInit.Code;
                VirtualKey = eventInit.VirtualKey;
                Key = eventInit.Key;
            }
        }

        public bool CtrlKey { get; }
        public bool ShiftKey { get; }
        public bool AltKey { get; }
        public bool MetaKey { get; }

        /// <summary>
        /// Indicates that the event is for an automated repeat (the user is holding down the key).
        /// </summary>
        public bool Repeat { get; }

        /// <summary>
        /// This is equivalent to a name for the scan code of the pressed key.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The Windows virtual key code for the pressed key.
        /// </summary>
        [Obsolete]
        public VirtualKey VirtualKey { get; }

        /// <summary>
        /// A constant value for the key.
        /// </summary>
        public string Key { get; }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is KeyboardEventInit kbInit)
            {
                kbInit.CtrlKey = CtrlKey;
                kbInit.ShiftKey = ShiftKey;
                kbInit.AltKey = AltKey;
                kbInit.MetaKey = MetaKey;
                kbInit.Repeat = Repeat;
                kbInit.Code = Code;
                kbInit.VirtualKey = VirtualKey;
                kbInit.Key = Key;
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

        public VirtualKey VirtualKey { get; set; }

        public string Key { get; set; }
    }
}