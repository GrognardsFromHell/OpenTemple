using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenTemple.Core.Ui.DOM
{
    /// <summary>
    /// A potential event target is null or an EventTarget object.
    /// An event has an associated target (a potential event target). Unless stated otherwise it is null.
    /// An event has an associated relatedTarget (a potential event target). Unless stated otherwise it is null.
    /// <a href="https://dom.spec.whatwg.org/#interface-event">W3C Specification</a>
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// If this event is a known system-event type, this will return that type. For custom  events,
        /// this will return Custom.
        /// </summary>
        public SystemEventType SystemType { get; }

        /// <summary>
        /// Returns the type of event, e.g. "click", "hashchange", or "submit".
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Returns the object to which event is dispatched (its target).
        /// </summary>
        [MaybeNull]
        public EventTarget Target { get; }

        [MaybeNull]
        public EventTarget RelatedTarget { get; set; }

        /// <summary>
        /// Returns the object whose event listener’s callback is currently being invoked.
        /// </summary>
        [MaybeNull]
        public EventTarget CurrentTarget { get; }

        /// <summary>
        /// Returns the invocation target objects of event’s path (objects on which listeners will be invoked), except
        /// for any nodes in shadow trees of which the shadow root’s mode is "closed" that are not reachable from
        /// event’s currentTarget.
        /// </summary>
        public IEnumerable<EventTarget> ComposedPath();

        /// <summary>
        /// Returns the event’s phase, which is one of NONE, CAPTURING_PHASE, AT_TARGET, and BUBBLING_PHASE.
        /// </summary>
        public EventPhase EventPhase { get; }

        /// <summary>
        /// When dispatched in a tree, invoking this method prevents event from reaching
        /// any objects other than the current object.
        /// </summary>
        void StopPropagation();

        /// <summary>
        /// Invoking this method prevents event from reaching any registered event listeners after the current one
        /// finishes running and, when dispatched in a tree, also prevents event from reaching any other objects.
        /// </summary>
        void StopImmediatePropagation();

        /// <summary>
        /// Returns true or false depending on how event was initialized. True if event goes through its target’s
        /// ancestors in reverse tree order, and false otherwise.
        /// </summary>
        public bool Bubbles { get; }

        /// <summary>
        /// Returns true or false depending on how event was initialized. Its return value does not always carry
        /// meaning, but true can indicate that part of the operation during which event was dispatched, can be
        /// canceled by invoking the preventDefault() method.
        /// </summary>
        public bool Cancelable { get; }

        /// <summary>
        /// If invoked when the cancelable attribute value is true, and while executing a listener for the event with
        /// passive set to false, signals to the operation that caused event to be dispatched that it needs to be canceled.
        /// </summary>
        void PreventDefault();

        /// <summary>
        /// Returns true if preventDefault() was invoked successfully to indicate cancelation, and false otherwise.
        /// </summary>
        public bool DefaultPrevented { get; }

        /// <summary>
        /// Returns true or false depending on how event was initialized. True if event invokes listeners past a
        /// ShadowRoot node that is the root of its target, and false otherwise.
        /// </summary>
        public bool Composed { get; }

        /// <summary>
        /// Returns the event’s timestamp as the number of milliseconds measured relative to the time origin.
        /// </summary>
        public UiTimestamp TimeStamp { get; }
    }

}