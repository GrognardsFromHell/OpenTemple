namespace OpenTemple.Core.Ui.DOM
{
    public class KeyboardEvent : UiEvent
    {
        public KeyboardEvent(string type, KeyboardEventInit eventInit = null) : this(SystemEventType.Custom, type, eventInit)
        {
        }

        public KeyboardEvent(SystemEventType type, KeyboardEventInit eventInit = null) : this(type, null, eventInit)
        {
        }

        public KeyboardEvent(SystemEventType systemType, string type, KeyboardEventInit eventInit = null) : base(systemType, type, eventInit)
        {
            if (eventInit != null)
            {
                ScreenX = eventInit.ScreenX;
                ScreenY = eventInit.ScreenY;
                ClientX = eventInit.ClientX;
                ClientY = eventInit.ClientY;
                CtrlKey = eventInit.CtrlKey;
                ShiftKey = eventInit.ShiftKey;
                AltKey = eventInit.AltKey;
                MetaKey = eventInit.MetaKey;
                Button = eventInit.Button;
                Buttons = eventInit.Buttons;
                RelatedTarget = eventInit.RelatedTarget;
            }
        }

        public long ScreenX { get; }
        public long ScreenY { get; }
        public long ClientX { get; }
        public long ClientY { get; }

        public bool CtrlKey { get; }
        public bool ShiftKey { get; }
        public bool AltKey { get; }
        public bool MetaKey { get; }

        public short Button { get; }
        public ushort Buttons { get; }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is KeyboardEventInit mouseInit)
            {
                mouseInit.ScreenX = ScreenX;
                mouseInit.ScreenY = ScreenY;
                mouseInit.ClientX = ClientX;
                mouseInit.ClientY = ClientY;
                mouseInit.Button = Button;
                mouseInit.Buttons = Buttons;
                mouseInit.RelatedTarget = RelatedTarget;
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
    }

}