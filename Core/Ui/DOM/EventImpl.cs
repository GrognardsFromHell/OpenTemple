using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui
{
    /// <summary>
    /// Modeled after the DOM event model.
    /// </summary>
    public class EventImpl : IEvent
    {
        internal bool _dispatchFlag;
        internal bool _initializedFlag;
        internal List<PathItem> _path = new List<PathItem>();
        internal bool _stopPropagationFlag;
        internal bool _stopImmediatePropagationFlag;
        internal bool _canceledFlag;
        internal bool _inPassiveListenerFlag;

        public SystemEventType SystemType { get; }
        public string Type { get; }
        public EventTarget Target { get; set; }
        public EventTarget CurrentTarget { get; set; }
        public EventPhase EventPhase { get; set; }

        [MaybeNull]
        public EventTarget RelatedTarget { get; set; }

        public bool Bubbles { get; set; }
        public bool Cancelable { get; set; }

        public bool DefaultPrevented { get; set; }

        // TODO: This might be unneeded because we dont support shadow DOM
        public bool Composed { get; set; }
        public UiTimestamp TimeStamp { get; set; }

        internal readonly struct PathItem
        {
            internal readonly EventTargetImpl item;
            internal readonly EventTargetImpl target;
            internal readonly EventTargetImpl relatedTarget;
            internal readonly List<EventTargetImpl> touchTargets;

            public PathItem(EventTargetImpl item, EventTargetImpl target,
                EventTargetImpl relatedTarget,
                List<EventTargetImpl> touchTargets)
            {
                this.item = item;
                this.target = target;
                this.relatedTarget = relatedTarget;
                this.touchTargets = touchTargets;
            }
        }

        // See https://dom.spec.whatwg.org/#constructing-events
        public EventImpl(SystemEventType type, EventInit eventInit = null) : this(type, SystemEventTypes.GetName(type),
            eventInit)
        {
        }

        // See https://dom.spec.whatwg.org/#constructing-events
        public EventImpl(string type, EventInit eventInit = null) : this(SystemEventTypes.FromName(type),
            type.ToLowerInvariant(), eventInit)
        {
        }

        protected EventImpl(SystemEventType systemType, string type, EventInit eventInit)
        {
            if (systemType != SystemEventType.Custom)
            {
                type = SystemEventTypes.GetName(systemType);
            }
            else
            {
                Debug.Assert(SystemEventTypes.FromName(type) == systemType);
            }

            SystemType = systemType;
            Type = type;
            if (eventInit != null)
            {
                Bubbles = eventInit.Bubbles;
                Cancelable = eventInit.Cancelable;
                Composed = eventInit.Composed;
            }

            _initializedFlag = true;
            TimeStamp = UiTimestamp.Now;
        }

        public void PreventDefault()
        {
            throw new NotImplementedException();
        }

        public void StopPropagation()
        {
            throw new NotImplementedException();
        }

        public void StopImmediatePropagation()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<EventTarget> ComposedPath()
        {
            throw new NotImplementedException();
        }

        public void AddToPath(EventTargetImpl item, EventTargetImpl target,
            EventTargetImpl relatedTarget,
            List<EventTargetImpl> touchTargets)
        {
            _path.Add(new PathItem(
                item,
                target,
                relatedTarget,
                touchTargets
            ));
        }

        public virtual void CopyTo(EventInit init)
        {
            init.Bubbles = Bubbles;
            init.Cancelable = Cancelable;
            init.Composed = Composed;
        }

        public virtual EventImpl Copy()
        {
            var init = new EventInit();
            CopyTo(init);
            return new EventImpl(SystemType, Type, init);
        }
    }
}