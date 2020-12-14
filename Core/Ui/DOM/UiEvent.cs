using System.Diagnostics.CodeAnalysis;

namespace OpenTemple.Core.Ui.DOM
{
    public class UiEvent : EventImpl
    {
        public UiEvent(SystemEventType type, UiEventInit eventInit) : this(type, null, eventInit)
        {
        }

        public UiEvent(string type, UiEventInit eventInit) : this(SystemEventType.Custom, type, eventInit)
        {
        }

        protected UiEvent(SystemEventType systemType, string type, UiEventInit eventInit) : base(systemType, type, eventInit)
        {
            if (eventInit != null)
            {
                View = eventInit.View;
                Detail = eventInit.Detail;
            }
        }

        [MaybeNull]
        public Window View { get; internal set; }

        public long Detail { get; internal set; }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is UiEventInit uiEventInit)
            {
                uiEventInit.View = View;
                uiEventInit.Detail = Detail;
            }
        }
    }


    public class UiEventInit : EventInit
    {
        [MaybeNull]
        public Window View { get; set; }

        public long Detail { get; set; }
    }

}