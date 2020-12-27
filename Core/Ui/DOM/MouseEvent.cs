using System.Diagnostics.CodeAnalysis;

namespace OpenTemple.Core.Ui.DOM
{
    public class MouseEvent : KeyboardModifierEvent
    {
        public MouseEvent(string type, MouseEventInit eventInit = null) : this(SystemEventType.Custom, type, eventInit)
        {
        }

        public MouseEvent(SystemEventType type, MouseEventInit eventInit = null) : this(type, null, eventInit)
        {
        }

        public MouseEvent(SystemEventType systemType, string type, MouseEventInit eventInit = null) : base(systemType, type, eventInit)
        {
            if (eventInit != null)
            {
                ScreenX = eventInit.ScreenX;
                ScreenY = eventInit.ScreenY;
                ClientX = eventInit.ClientX;
                ClientY = eventInit.ClientY;
                Button = eventInit.Button;
                Buttons = eventInit.Buttons;
                RelatedTarget = eventInit.RelatedTarget;
            }
        }

        public float ScreenX { get; }
        public float ScreenY { get; }
        public float ClientX { get; }
        public float ClientY { get; }

        public short Button { get; }
        public ushort Buttons { get; }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is MouseEventInit mouseInit)
            {
                mouseInit.ScreenX = ScreenX;
                mouseInit.ScreenY = ScreenY;
                mouseInit.ClientX = ClientX;
                mouseInit.ClientY = ClientY;
                mouseInit.Button = Button;
                mouseInit.Buttons = Buttons;
                mouseInit.RelatedTarget = RelatedTarget;
                mouseInit.AltKey = AltKey;
                mouseInit.CtrlKey = CtrlKey;
                mouseInit.ShiftKey = ShiftKey;
                mouseInit.MetaKey = MetaKey;
            }
        }

        public override EventImpl Copy()
        {
            var init = new MouseEventInit();
            CopyTo(init);
            return new MouseEvent(SystemType, Type, init);
        }
    }

    public class MouseEventInit : EventModifierInit
    {
        public long ScreenX { get; set; }
        public long ScreenY { get; set; }
        public long ClientX { get; set; }
        public long ClientY { get; set; }

        public short Button { get; set; }
        public ushort Buttons { get; set; }

        [MaybeNull]
        public EventTarget RelatedTarget { get; set; }
    }

}