using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace OpenTemple.Core.Ui.DOM
{
    public interface INonDocumentTypeChildNode
    {
        [MaybeNull]
        Element PreviousElementSibling { get; }

        [MaybeNull]
        Element NextElementSibling { get; }
    }

    /// <summary>
    /// Equivalent to Firefox's EventState.*
    /// </summary>
    [Flags]
    public enum EventState : long
    {
        // Mouse is down on content.
        ACTIVE = 1L <<0,
        // Content has focus.
        FOCUS = 1L <<1,
        // Mouse is hovering over content.
        HOVER = 1L <<2,
        // Content is enabled (and can be disabled).
        ENABLED = 1L <<3,
        // Content is disabled.
        DISABLED = 1L <<4,
        // Content is checked.
        CHECKED = 1L <<5,
        // Content is in the indeterminate state.
        INDETERMINATE = 1L <<6,
        // Content shows its placeholder
        PLACEHOLDERSHOWN = 1L <<7,
        // Content is URL's target (ref).
        URLTARGET = 1L <<8,
        // Content is the full screen element, or a frame containing the
        // current fullscreen element.
        FULLSCREEN = 1L <<9,
        // Content is valid (and can be invalid).
        VALID = 1L <<10,
        // Content is invalid.
        INVALID = 1L <<11,
        // UI friendly version of :valid pseudo-class.
        MOZ_UI_VALID = 1L <<12,
        // UI friendly version of :invalid pseudo-class.
        MOZ_UI_INVALID = 1L <<13,
        // Content could not be rendered (image/object/etc).
        BROKEN = 1L <<14,

        // There are two free bits here.

        // Content is still loading such that there is nothing to show the
        // user (eg an image which hasn't started coming in yet).
        LOADING = 1L <<17,
        // Handler for the content has been blocked.
        HANDLER_BLOCKED = 1L <<18,
        // Handler for the content has been disabled.
        HANDLER_DISABLED = 1L <<19,
        // Handler for the content has crashed
        HANDLER_CRASHED = 1L <<20,
        // Content is required.
        REQUIRED = 1L <<21,
        // Content is optional (and can be required).
        OPTIONAL = 1L <<22,
        // Element is either a defined custom element or uncustomized element.
        DEFINED = 1L <<23,
        // Link has been visited.
        VISITED = 1L <<24,
        // Link hasn't been visited.
        UNVISITED = 1L <<25,
        // Drag is hovering over content.
        DRAGOVER = 1L <<26,
        // Content value is in-range (and can be out-of-range).
        INRANGE = 1L <<27,
        // Content value is out-of-range.
        OUTOFRANGE = 1L <<28,
        // Content is read-only.
        // TODO(emilio): This is always the inverse of READWRITE. With some style system
        // work we could remove one of the two bits.
        READONLY = 1L <<29,
        // Content is editable.
        READWRITE = 1L <<30,
        // Content is the default one (meaning depends of the context).
        DEFAULT = 1L <<31,
        // Content is a submit control and the form isn't valid.
        MOZ_SUBMITINVALID = 1L <<32,
        // Content is in the optimum region.
        OPTIMUM = 1L <<33,
        // Content is in the suboptimal region.
        SUB_OPTIMUM = 1L <<34,
        // Content is in the sub-suboptimal region.
        SUB_SUB_OPTIMUM = 1L <<35,
        // Element is highlighted (devtools inspector)
        DEVTOOLS_HIGHLIGHTED = 1L <<36,
        // Element is transitioning for rules changed by style editor
        STYLEEDITOR_TRANSITIONING = 1L <<37,
        INCREMENT_SCRIPT_LEVEL = 1L <<38,
        TYPE_CLICK_TO_PLAY = 1L <<40,
        // Handler for click to play plugin (vulnerable w/update)
        VULNERABLE_UPDATABLE = 1L <<41,
        // Handler for click to play plugin (vulnerable w/no update)
        VULNERABLE_NO_UPDATE = 1L <<42,
        // Element has focus-within.
        FOCUS_WITHIN = 1L <<43,
        // Element is ltr (for :dir pseudo-class)
        LTR = 1L <<44,
        // Element is rtl (for :dir pseudo-class)
        RTL = 1L <<45,
        // States for tracking the state of the "dir" attribute for HTML elements.  We
        // use these to avoid having to get "dir" attributes all the time during
        // selector matching for some parts of the UA stylesheet.
        //
        // These states are externally managed, because we also don't want to keep
        // getting "dir" attributes in IntrinsicState.
        //
        // Element is HTML and has a "dir" attibute.  This state might go away depending
        // on how https://github.com/whatwg/html/issues/2769 gets resolved.  The value
        // could be anything.
        HAS_DIR_ATTR = 1L <<46,
        // Element is HTML, has a "dir" attribute, and the attribute's value is
        // case-insensitively equal to "ltr".
        DIR_ATTR_LTR = 1L <<47,
        // Element is HTML, has a "dir" attribute, and the attribute's value is
        // case-insensitively equal to "rtl".
        DIR_ATTR_RTL = 1L <<48,
        // Element is HTML, and is either a <bdi> element with no valid-valued "dir"
        // attribute or any HTML element which has a "dir" attribute whose value is
        // "auto".
        DIR_ATTR_LIKE_AUTO = 1L <<49,
        // Element is filled by Autofill feature.
        AUTOFILL = 1L <<50,
        // Element is filled with preview data by Autofill feature.
        AUTOFILL_PREVIEW = 1L <<51,
        // Modal <dialog> element
        MODAL_DIALOG = 1L <<53,
        // Inert subtrees
        MOZINERT = 1L <<54,
        // Topmost Modal <dialog> element in top layer
        TOPMOST_MODAL_DIALOG = 1L <<55,
    }

    public class StyleUI
    {
        public bool DisableUserInput { get; set; }
    }

    public partial class Element : ParentNode, INonDocumentTypeChildNode, IChildNode
    {
        public Element(Document ownerDocument, string tagName = "#element") : base(ownerDocument)
        {
            TagName = tagName;
        }

        public override string NodeName => TagName;

        public override Element ClosestElement => this;

        public string TagName { get; protected set; }

        public override NodeType NodeType => NodeType.ELEMENT_NODE;

        private const EventState DIRECTION_STATES = (EventState.LTR | EventState.RTL);

        private const EventState DIR_ATTR_STATES =
            (EventState.HAS_DIR_ATTR | EventState.DIR_ATTR_LTR |
             EventState.DIR_ATTR_RTL | EventState.DIR_ATTR_LIKE_AUTO);

        private const EventState DISABLED_STATES = (EventState.DISABLED | EventState.ENABLED);

        private const EventState REQUIRED_STATES = (EventState.REQUIRED | EventState.OPTIONAL);

        // Event states that can be added and removed through
        // Element::{Add,Remove}ManuallyManagedStates.
        //
        // Take care when manually managing state bits.  You are responsible for
        // setting or clearing the bit when an Element is added or removed from a
        // document (e.g. in BindToTree and UnbindFromTree), if that is an
        // appropriate thing to do for your state bit.
        private const EventState MANUALLY_MANAGED_STATES =
            (EventState.AUTOFILL | EventState.AUTOFILL_PREVIEW);

        // Event states that are managed externally to an element (by the
        // EventStateManager, or by other code).  As opposed to those in
        // INTRINSIC_STATES, which are are computed by the element itself
        // and returned from Element::IntrinsicState.
        private const EventState EXTERNALLY_MANAGED_STATES =
            (MANUALLY_MANAGED_STATES | DIR_ATTR_STATES | DISABLED_STATES |
             REQUIRED_STATES | EventState.ACTIVE | EventState.DEFINED |
             EventState.DRAGOVER | EventState.FOCUS |
             EventState.FOCUS_WITHIN | EventState.FULLSCREEN |
             EventState.HOVER | EventState.URLTARGET | EventState.MODAL_DIALOG |
             EventState.MOZINERT | EventState.TOPMOST_MODAL_DIALOG);

        private const EventState INTRINSIC_STATES = ~EXTERNALLY_MANAGED_STATES;

        internal EventState State { get; private set; }

        public StyleUI StyleUI { get; } = new StyleUI();

        public Element PreviousElementSibling
        {
            get
            {
                var node = PreviousSibling;
                while (node != null && !(node is Element))
                {
                    node = node.PreviousSibling;
                }

                return (Element) node;
            }
        }

        public Element NextElementSibling
        {
            get
            {
                var node = NextSibling;
                while (node != null && !(node is Element))
                {
                    node = node.NextSibling;
                }

                return (Element) node;
            }
        }

        public Element CommonAncestor(Element other)
        {
            return CommonAncestor(this, other, e => e.ParentElement);
        }

        private static T CommonAncestor<T>(T node1, T node2, Func<T, T> parentAccessor) where T : class
        {
            Debug.Assert(node1 != node2);

            // Build the chain of parents
            // TODO: Try to somehow pool these lists
            var parents1 = new List<T>(30);
            var parents2 = new List<T>(30);
            do {
                parents1.Add(node1);
                node1 = parentAccessor(node1);
            } while (node1 != null);
            do {
                parents2.Add(node2);
                node2 = parentAccessor(node2);
            } while (node2 != null);

            // Find where the parent chain differs
            var pos1 = parents1.Count;
            var pos2 = parents2.Count;
            T parent = null;
            int len;
            for (len = Math.Min(pos1, pos2); len > 0; --len)
            {
                var child1 = parents1[--pos1];
                var child2 = parents2[--pos2];
                if (child1 != child2) {
                    break;
                }
                parent = child1;
            }

            return parent;
        }

        public bool GetState(EventState state) => (State & state) != 0;

        // Methods for the ESM, nsGlobalWindow, focus manager and Document to
        // manage state bits.
        // These will handle setting up script blockers when they notify, so no need
        // to do it in the callers unless desired.  States passed here must only be
        // those in EXTERNALLY_MANAGED_STATES.
        internal virtual void AddStates(EventState states)
        {
            Debug.Assert((states & INTRINSIC_STATES) == 0);
            AddStatesSilently(states);
            NotifyStateChange(states);
        }
        internal virtual void RemoveStates(EventState states) {
            Debug.Assert((states & INTRINSIC_STATES) == 0);
            RemoveStatesSilently(states);
            NotifyStateChange(states);
        }
        internal virtual void ToggleStates(EventState states, bool notify) {
            Debug.Assert((states & INTRINSIC_STATES) == 0);
            State ^= states;
            if (notify) {
                NotifyStateChange(states);
            }
        }

        /// <summary>
        /// Method to get the _intrinsic_ content state of this element.  This is the
        /// state that is independent of the element's presentation.  To get the full
        /// content state, use State().  See mozilla/EventStates.h for
        /// the possible bits that could be set here.
        /// </summary>
        protected virtual EventState IntrinsicState()
        {
            return EventState.READONLY;
        }

        /// <summary>
        /// Method to add state bits.  This should be called from subclass
        /// constructors to set up our event state correctly at construction
        /// time and other places where we don't want to notify a state change.
        /// </summary>
        protected void AddStatesSilently(EventState states) { State |= states; }

        /// <summary>
        /// Method to remove state bits.  This should be called from subclass
        /// constructors to set up our event state correctly at construction
        /// time and other places where we don't want to notify a state
        ///     change.
        /// </summary>
        protected void RemoveStatesSilently(EventState states) { State &= ~states; }

        protected void NotifyStateChange(EventState states)
        {
            OwnerDocument?.ContentStateChanged(this, states);
        }

        public bool HasPointerCapture()
        {
            return Globals.UiManager.GetCapturingContent() == this;
        }

        public void SetPointerCapture()
        {
            SetCapture(true);
        }

        public void ReleasePointerCapture()
        {
            ReleaseCapture();
        }

        /// <summary>
        /// Set this during a mousedown event to grab and retarget all mouse events
        /// to this element until the mouse button is released or releaseCapture is
        /// called. If retargetToElement is true, then all events are targetted at
        /// this element. If false, events can also fire at descendants of this
        /// element.
        /// </summary>
        private void SetCapture(bool retargetToElement = true)
        {
            var manager = Globals.UiManager;
            // If there is already an active capture, ignore this request. This would
            // occur if a splitter, frame resizer, etc had already captured and we don't
            // want to override those.
            if (manager.GetCapturingContent() == null) {
                manager.SetCapturingContent(
                    this, CaptureFlags.PreventDragStart |
                          (retargetToElement ? CaptureFlags.RetargetToElement
                              : CaptureFlags.None));
            }
        }

        /// <summary>
        /// If this element has captured the mouse, release the capture. If another
        /// element has captured the mouse, this method has no effect.
        /// </summary>
        private void ReleaseCapture() {
            var manager = Globals.UiManager;
            if (manager.GetCapturingContent() == this) {
                manager.ReleaseCapturingContent();
            }
        }

        private bool _focusable;
        private float _scrollLeft;
        private float _scrollTop;

        public bool IsFocusable
        {
            get => _focusable;
            set
            {
                if (!_focusable)
                {
                    OwnerDocument.FocusManager.NotifyElementRemoval(this);
                }
                _focusable = value;
            }
        }

        // https://html.spec.whatwg.org/multipage/#dom-focus
        public void Focus() {
            // https://html.spec.whatwg.org/multipage/#focusing-steps
            OwnerDocument?.FocusManager.RequestFocus(this);
        }

        public bool IsFocused => (State & EventState.FOCUS) != 0;

        // https://html.spec.whatwg.org/multipage/#dom-blur
        public void Blur() {
            // TODO: Run the unfocusing steps.
            if (!IsFocused)
            {
                return;
            }
            // https://html.spec.whatwg.org/multipage/#unfocusing-steps
            OwnerDocument?.FocusManager.RequestFocus(null);
        }

        public void FireSyntheticMouseEvent(SystemEventType eventType, MouseEventInit init = null)
        {
            var actualInit = new MouseEventInit();
            if (init != null)
            {
                new MouseEvent(eventType, init).CopyTo(actualInit);
            }

            actualInit.Bubbles = true;
            actualInit.Cancelable = true;
            actualInit.Composed = true;

            var evt = new MouseEvent(eventType, actualInit);
            evt.Target = this;
            Dispatch(evt);
        }

        public float ScrollLeft
        {
            get => _scrollLeft;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value != _scrollTop)
                {
                    _scrollLeft = value;
                    OwnerDocument?.Host.NotifyVisualTreeChange(this);
                }
            }
        }

        public float ScrollTop
        {
            get => _scrollTop;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value != _scrollTop)
                {
                    _scrollTop = value;
                    OwnerDocument?.Host.NotifyVisualTreeChange(this);
                }
            }
        }

        public virtual float OffsetLeft => 0;
        public virtual float OffsetTop => 0;
        public virtual float OffsetWidth => 0;
        public virtual float OffsetHeight => 0;
        
        public RectangleF GetBoundingClientRect()
        {
            if (ParentElement != null)
            {
                var parentArea = ParentElement.GetBoundingClientRect();
                var parentLeft = parentArea.X;
                var parentTop = parentArea.Y;
                var parentRight = parentLeft + parentArea.Width;
                var parentBottom = parentTop + parentArea.Height;

                var clientLeft = parentArea.X + OffsetLeft - ParentElement.ScrollLeft;
                var clientTop = parentArea.Y + OffsetTop - ParentElement.ScrollTop;
                var clientRight = clientLeft + OffsetWidth;
                var clientBottom = clientTop + OffsetHeight;

                clientLeft = Math.Max(parentLeft, clientLeft);
                clientTop = Math.Max(parentTop, clientTop);

                clientRight = Math.Min(parentRight, clientRight);
                clientBottom = Math.Min(parentBottom, clientBottom);

                if (clientRight <= clientLeft)
                {
                    clientRight = clientLeft;
                }

                if (clientBottom <= clientTop)
                {
                    clientBottom = clientTop;
                }

                return new RectangleF(
                    clientLeft,
                    clientTop,
                    clientRight - clientLeft,
                    clientBottom - clientTop
                );
            }
            else
            {
                return new RectangleF(OffsetLeft, OffsetTop, OffsetWidth, OffsetHeight);
            }
        }
    }

}
