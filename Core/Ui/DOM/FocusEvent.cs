using System.Diagnostics.CodeAnalysis;

namespace OpenTemple.Core.Ui.DOM
{
    public class FocusEvent : UiEvent
    {
        public FocusEvent(SystemEventType type, FocusEventInit eventInit) : this(type, null, eventInit)
        {
        }

        public FocusEvent(string type, FocusEventInit eventInit) : this(SystemEventType.Custom, type, eventInit)
        {
        }

        protected FocusEvent(SystemEventType systemType, string type, FocusEventInit eventInit) : base(systemType, type,
            eventInit)
        {
            if (eventInit != null)
            {
                RelatedTarget = eventInit.RelatedTarget;
            }
        }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is FocusEventInit focusEventInit)
            {
                focusEventInit.RelatedTarget = RelatedTarget;
            }
        }
    }


    public class FocusEventInit : UiEventInit
    {
        [MaybeNull] public EventTarget RelatedTarget { get; set; }
    }
}