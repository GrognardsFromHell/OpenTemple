using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenTemple.Core.Ui.DOM
{
    /// <summary>
    /// <a href="https://dom.spec.whatwg.org/#eventtarget">W3C Specification</a>
    /// </summary>
    public class EventTargetImpl : EventTarget
    {
        private Dictionary<string, EventListenerRegistration[]> _listeners = null;

        private bool _hasActivationBehavior;
        private Action _legacyPreActivationBehavior;
        private Action<IEvent> _activationBehavior;
        private Action _legacyCanceledActivationBehavior;

        protected virtual EventTargetImpl _getTheParent(EventImpl evt) => null;

        private readonly struct EventListenerRegistration
        {
            public EventListener Callback { get; }
            public bool Capture { get; }
            public bool Once { get; }
            public bool Passive { get; }

            public EventListenerRegistration(EventListener callback, bool capture, bool once, bool passive)
            {
                Callback = callback;
                Capture = capture;
                Once = once;
                Passive = passive;
            }
        }

        public void AddEventListener(SystemEventType type, [AllowNull]
            EventListener callback,
            bool capture = false,
            bool once = false,
            bool passive = false)
        {
            Debug.Assert(type != SystemEventType.Custom);
            AddEventListener(SystemEventTypes.GetName(type), callback, capture, once, passive);
        }

        public EventListener AddEventListener<T>(SystemEventType type,
            EventListener<T> callback,
            bool capture = false,
            bool once = false,
            bool passive = false) where T : class, IEvent
        {
            void Listener(IEvent e)
            {
                if (e is T typedEvent)
                {
                    callback(typedEvent);
                }
            }
            AddEventListener(SystemEventTypes.GetName(type), Listener, capture, once, passive);
            return Listener;
        }

        public void AddEventListener(string type, [AllowNull]
            EventListener callback,
            bool capture = false,
            bool once = false,
            bool passive = false)
        {
            if (callback == null)
            {
                return;
            }

            if (_listeners == null)
            {
                _listeners = new Dictionary<string, EventListenerRegistration[]>();
            }

            if (!_listeners.TryGetValue(type, out var listeners))
            {
                listeners = Array.Empty<EventListenerRegistration>();
            }

            for (var i = 0; i < listeners.Length; i++)
            {
                ref readonly var listener = ref listeners[i];
                // The same callback is already registered for the same phase
                if (listener.Callback == callback && listener.Capture == capture)
                {
                    return;
                }
            }

            Array.Resize(ref listeners, listeners.Length + 1);
            listeners[^1] = new EventListenerRegistration(callback, capture, once, passive);
            _listeners[type] = listeners;
        }

        public void RemoveEventListener(string type, [AllowNull]
            EventListener callback,
            bool capture = false)
        {
            if (_listeners == null || !_listeners.TryGetValue(type, out var listeners))
            {
                return;
            }

            for (var i = 0; i < listeners.Length; i++)
            {
                ref readonly var listener = ref listeners[i];
                if (listener.Callback == callback && listener.Capture == capture)
                {
                    if (listeners.Length <= 1)
                    {
                        _listeners.Remove(type);
                    }
                    else
                    {
                        var newListeners = new EventListenerRegistration[listeners.Length - 1];
                        Array.Copy(listeners, 0, newListeners, 0, i);
                        Array.Copy(listeners, i + 1, newListeners, i, listeners.Length - i - 1);
                        _listeners[type] = listeners;
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// <a href="https://dom.spec.whatwg.org/#concept-event-dispatch">W3C Spec</a>
        /// </summary>
        public bool DispatchEvent(IEvent evt)
        {
            if (!(evt is EventImpl eventImpl))
            {
                throw new DOMException();
            }

            if (eventImpl._dispatchFlag || !eventImpl._initializedFlag)
            {
                throw new DOMException(
                    "Tried to dispatch an uninitialized event",
                    "InvalidStateError"
                );
            }

            if (eventImpl.EventPhase != EventPhase.NONE)
            {
                throw new DOMException(
                    "Tried to dispatch a dispatching event",
                    "InvalidStateError"
                );
            }

            return Dispatch(eventImpl);
        }

        // https://dom.spec.whatwg.org/#concept-event-path-append
        private void AppendToEventPath(EventImpl eventImpl, EventTargetImpl target, EventTargetImpl targetOverride,
            EventTargetImpl relatedTarget, List<EventTargetImpl> touchTargets)
        {
            eventImpl.AddToPath(
                target,
                targetOverride,
                relatedTarget,
                touchTargets
            );
        }

        internal bool Dispatch(EventImpl eventImpl, EventTarget targetOverride = null)
        {
            var targetImpl = this;

            eventImpl._dispatchFlag = true;

            targetOverride ??= targetImpl;

            EventTargetImpl activationTarget = null;

            var relatedTarget = (EventTargetImpl) eventImpl.RelatedTarget;

            if (targetImpl != relatedTarget || targetImpl == eventImpl.RelatedTarget)
            {
                var touchTargets = new List<EventTargetImpl>();

                AppendToEventPath(eventImpl, targetImpl, (EventTargetImpl) targetOverride, relatedTarget, touchTargets);

                var isActivationEvent = eventImpl is MouseEvent && eventImpl.Type == "click";

                if (isActivationEvent && targetImpl._hasActivationBehavior)
                {
                    activationTarget = targetImpl;
                }


                var parent = targetImpl._getTheParent(eventImpl);

                // Populate event path
                // https://dom.spec.whatwg.org/#event-path
                while (parent != null)
                {
                    if (
                        targetImpl is Node &&
                        (parent is Node parentNode &&
                         targetImpl.GetRoot().IsShadowInclusiveAncestor(parentNode)) ||
                        parent is Window
                    )
                    {
                        if (isActivationEvent && eventImpl.Bubbles && activationTarget == null &&
                            parent._hasActivationBehavior)
                        {
                            activationTarget = parent;
                        }

                        AppendToEventPath(eventImpl, parent, null, relatedTarget, touchTargets);
                    }
                    else if (parent == relatedTarget)
                    {
                        parent = null;
                    }
                    else
                    {
                        targetImpl = parent;

                        if (isActivationEvent && activationTarget == null && targetImpl._hasActivationBehavior)
                        {
                            activationTarget = targetImpl;
                        }

                        AppendToEventPath(eventImpl, parent, targetImpl, relatedTarget, touchTargets);
                    }

                    parent = parent?._getTheParent(eventImpl);
                }

                var clearTargetsStructIndex = -1;
                for (var i = eventImpl._path.Count - 1; i >= 0 && clearTargetsStructIndex == -1; i--)
                {
                    if (eventImpl._path[i].target != null)
                    {
                        clearTargetsStructIndex = i;
                    }
                }

                if (activationTarget != null && activationTarget._legacyPreActivationBehavior != null)
                {
                    activationTarget._legacyPreActivationBehavior();
                }

                for (var i = eventImpl._path.Count - 1; i >= 0; --i)
                {
                    var pathItem = eventImpl._path[i];

                    if (pathItem.target != null)
                    {
                        eventImpl.EventPhase = EventPhase.AT_TARGET;
                    }
                    else
                    {
                        eventImpl.EventPhase = EventPhase.CAPTURING_PHASE;
                    }

                    InvokeEventListeners(pathItem, eventImpl, InvokePhase.CAPTURING);
                }

                for (var i = 0; i < eventImpl._path.Count; i++)
                {
                    var pathItem = eventImpl._path[i];

                    if (pathItem.target != null)
                    {
                        eventImpl.EventPhase = EventPhase.AT_TARGET;
                    }
                    else
                    {
                        if (!eventImpl.Bubbles)
                        {
                            continue;
                        }

                        eventImpl.EventPhase = EventPhase.BUBBLING_PHASE;
                    }

                    InvokeEventListeners(pathItem, eventImpl, InvokePhase.BUBBLING);
                }
            }

            eventImpl.EventPhase = EventPhase.NONE;

            eventImpl.CurrentTarget = null;
            eventImpl._path.Clear();
            eventImpl._dispatchFlag = false;
            eventImpl._stopPropagationFlag = false;
            eventImpl._stopImmediatePropagationFlag = false;

            if (activationTarget != null)
            {
                if (!eventImpl.DefaultPrevented)
                {
                    activationTarget._activationBehavior(eventImpl);
                }
                else if (activationTarget._legacyCanceledActivationBehavior != null)
                {
                    activationTarget._legacyCanceledActivationBehavior();
                }
            }

            return !eventImpl.DefaultPrevented;
        }

        enum InvokePhase
        {
            CAPTURING,
            BUBBLING
        }

        // https://dom.spec.whatwg.org/#concept-event-listener-invoke
        private void InvokeEventListeners(in EventImpl.PathItem pathItem, EventImpl eventImpl, InvokePhase phase)
        {
            var pathItemIndex = eventImpl._path.IndexOf(pathItem);
            for (var i = pathItemIndex; i >= 0; i--)
            {
                var t = eventImpl._path[i]; // TODO performance
                if (t.target != null)
                {
                    eventImpl.Target = t.target;
                    break;
                }
            }

            eventImpl.RelatedTarget = pathItem.relatedTarget;

            if (eventImpl._stopPropagationFlag)
            {
                return;
            }

            eventImpl.CurrentTarget = pathItem.item;

            var listeners = pathItem.item._listeners;
            InnerInvokeEventListeners(eventImpl, listeners, phase);
        }

        // https://dom.spec.whatwg.org/#concept-event-listener-inner-invoke
        private bool InnerInvokeEventListeners(EventImpl eventImpl,
            Dictionary<string, EventListenerRegistration[]> listeners, InvokePhase phase)
        {
            var found = false;

            var type = eventImpl.Type;

            if (listeners == null || !listeners.TryGetValue(type, out var handlers))
            {
                return false;
            }

            foreach (var listener in handlers)
            {
                var capture = listener.Capture;
                var once = listener.Once;
                var passive = listener.Passive;

                // Check if the event listener has been removed since the listeners has been cloned.
                if (!listeners.TryGetValue(type, out var currentHandlers)
                    || currentHandlers != handlers && !currentHandlers.Contains(listener))
                {
                    continue;
                }

                found = true;

                if (
                    (phase == InvokePhase.CAPTURING && !capture) ||
                    (phase == InvokePhase.BUBBLING && capture)
                )
                {
                    continue;
                }

                if (once)
                {
                    RemoveEventListener(type, listener.Callback, capture);
                }

                if (passive)
                {
                    eventImpl._inPassiveListenerFlag = true;
                }

                try
                {
                    listener.Callback(eventImpl);
                }
                catch (Exception e)
                {
                    ReportException(e);
                }

                eventImpl._inPassiveListenerFlag = false;

                if (eventImpl._stopImmediatePropagationFlag)
                {
                    break;
                }
            }

            return found;
        }

        internal void ReportException(Exception exception)
        {
            Utils.ErrorReporting.ReportException(exception);
        }
    }
}