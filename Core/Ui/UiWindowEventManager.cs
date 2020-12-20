using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui
{
    /// <summary>
    /// Handles and dispatches events that come from the system window,
    /// and relates mostly to mouse and keyboard input events.
    /// </summary>
    public class UiWindowEventManager
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private const int ButtonCount = 5;

        private readonly Document _document;

        private bool sIsPointerLocked;
        private Element sPointerLockedElement;

        /// <summary>
        /// Currently hovered element.
        /// </summary>
        private Node _currentMouseOver;

        /// <summary>
        /// The target we're currently handling an event for.
        /// Set in pre-handle, unset in post-handle.
        /// </summary>
        private Element mCurrentTarget;

        /// <summary>
        /// The target that is currently :active
        /// </summary>
        private Element mActiveContent;

        /// <summary>
        /// The target that is currently :hover
        /// </summary>
        private Element mHoverContent;

        private Element sDragOverContent;

        private Element mURLTargetContent;

        private readonly ButtonTracking[] _buttons = new ButtonTracking[ButtonCount];

        private readonly OverOutElementsWrapper mMouseEnterLeaveHelper = new OverOutElementsWrapper();

        public UiWindowEventManager(Document document)
        {
            _document = document;
        }

        public void PreHandleEvent(MouseEvent evt)
        {
            var target = (EventTargetImpl) evt.Target;

            mCurrentTarget = target as Element;

            switch (evt.SystemType)
            {
                case SystemEventType.MouseDown:
                    _buttons[evt.Button].IsDown = true;
                    _buttons[evt.Button].MouseDownOn = target;
                    _buttons[evt.Button].MouseDownPos = new PointF(evt.ClientX, evt.ClientY);
                    _buttons[evt.Button].MouseDownTimestamp = evt.TimeStamp;

                    _document.FocusManager.RequestFocus(mCurrentTarget);
                    break;
                case SystemEventType.MouseMove:
                    if (_currentMouseOver != target)
                    {
                        _currentMouseOver?.Dispatch(new MouseEvent(SystemEventType.MouseOut, new MouseEventInit()
                        {
                            Bubbles = true,
                            Cancelable = true,
                            Composed = true,
                            Button = evt.Button,
                            Buttons = evt.Buttons,
                            ClientX = evt.ClientX,
                            ClientY = evt.ClientY,
                            ScreenX = evt.ScreenX,
                            ScreenY = evt.ScreenY,
                            RelatedTarget = target
                        }));

                        target?.Dispatch(new MouseEvent(SystemEventType.MouseOver, new MouseEventInit()
                        {
                            Bubbles = true,
                            Cancelable = true,
                            Composed = true,
                            Button = evt.Button,
                            Buttons = evt.Buttons,
                            ClientX = evt.ClientX,
                            ClientY = evt.ClientY,
                            ScreenX = evt.ScreenX,
                            ScreenY = evt.ScreenY,
                            RelatedTarget = target
                        }));
                        _currentMouseOver = mCurrentTarget;

                        if (mCurrentTarget != null)
                        {
                            NotifyMouseOver(evt, mCurrentTarget);
                        }
                    }

                    break;
            }
        }

        public void PostHandleEvent(MouseEvent evt)
        {
            var target = (Node) evt.Target;

            switch (evt.SystemType)
            {
                case SystemEventType.MouseDown:
                    if (evt.Button == 0)
                    {
                        SetContentState(target.ClosestElement, EventState.ACTIVE);
                    }

                    break;
                case SystemEventType.MouseUp:
                    if (evt.Button == 0)
                    {
                        ClearGlobalActiveContent();
                    }

                    ref var down = ref _buttons[evt.Button];
                    if (down.IsDown)
                    {
                        var mouseDownOn = down.MouseDownOn;
                        down = default;
                        if (evt.Target == mouseDownOn)
                        {
                            // Dispatch a click event
                            var clickType = evt.Button == 0 ? SystemEventType.Click : SystemEventType.AuxClick;
                            target?.Dispatch(new MouseEvent(clickType, new MouseEventInit()
                            {
                                Bubbles = true,
                                Cancelable = true,
                                Composed = true,
                                Button = evt.Button,
                                Buttons = evt.Buttons,
                                ClientX = evt.ClientX,
                                ClientY = evt.ClientY,
                                ScreenX = evt.ScreenX,
                                ScreenY = evt.ScreenY
                            }));
                        }
                    }

                    _document.FocusManager.PerformPendingFocusChange();
                    break;
            }

            mCurrentTarget = null;
        }

        private void ClearGlobalActiveContent()
        {
            SetContentState(null, EventState.ACTIVE);
            if (sDragOverContent != null)
            {
                SetContentState(null, EventState.DRAGOVER);
            }
        }

        private OverOutElementsWrapper GetWrapperByEventID(MouseEvent aEvent)
        {
            return mMouseEnterLeaveHelper;
        }

        private void NotifyMouseOver(MouseEvent aMouseEvent, Element aContent)
        {
            var wrapper = GetWrapperByEventID(aMouseEvent);

            if (wrapper == null || wrapper.mLastOverElement == aContent)
            {
                return;
            }

            // Before firing mouseover, check for recursion
            if (aContent == wrapper.mFirstOverEventElement)
            {
                return;
            }

            // Firing the DOM event in the parent document could cause all kinds
            // of havoc.  Reverify and take care.
            if (wrapper.mLastOverElement == aContent)
            {
                return;
            }

            // Remember mLastOverElement as the related content for the
            // DispatchMouseOrPointerEvent() call below, since NotifyMouseOut() resets it,
            // bug 298477.
            var lastOverElement = wrapper.mLastOverElement;

            var enterDispatcher = new EnterLeaveDispatcher(this, aContent, lastOverElement,
                aMouseEvent,
                SystemEventType.MouseEnter);

            SetContentState(aContent, EventState.HOVER);

            NotifyMouseOut(aMouseEvent, aContent);

            // Store the first mouseOver event we fire and don't refire mouseOver
            // to that element while the first mouseOver is still ongoing.
            wrapper.mFirstOverEventElement = aContent;

            // Fire mouseover
            wrapper.mLastOverFrame = DispatchMouseOrPointerEvent(
                aMouseEvent, SystemEventType.MouseOver, aContent,
                lastOverElement);
            enterDispatcher.Dispatch();
            wrapper.mLastOverElement = aContent;

            // Turn recursion protection back off
            wrapper.mFirstOverEventElement = null;
        }

        private void NotifyMouseOut(MouseEvent aMouseEvent, Element aMovingInto) {
          var wrapper = GetWrapperByEventID(aMouseEvent);

          if (wrapper?.mLastOverElement == null)
          {
              return;
          }

          // Before firing mouseout, check for recursion
          if (wrapper.mLastOverElement == wrapper.mFirstOutEventElement)
          {
              return;
          }

          // That could have caused DOM events which could wreak havoc. Reverify
          // things and be careful.
          if (wrapper.mLastOverElement == null)
          {
              return;
          }

          // Store the first mouseOut event we fire and don't refire mouseOut
          // to that element while the first mouseOut is still ongoing.
          wrapper.mFirstOutEventElement = wrapper.mLastOverElement;

          // Don't touch hover state if aMovingInto is non-null.  Caller will update
          // hover state itself, and we have optimizations for hover switching between
          // two nearby elements both deep in the DOM tree that would be defeated by
          // switching the hover state to null here.
          // Unset :hover
          if (aMovingInto == null)
          {
              SetContentState(null, EventState.HOVER);
          }

          var leaveDispatcher = new EnterLeaveDispatcher(this, wrapper.mLastOverElement,
                                               aMovingInto, aMouseEvent,
                                               SystemEventType.MouseLeave);

          // Fire mouseout
          DispatchMouseOrPointerEvent(aMouseEvent, SystemEventType.MouseOut,
                                      wrapper.mLastOverElement, aMovingInto);
          leaveDispatcher.Dispatch();

          wrapper.mLastOverFrame = null;
          wrapper.mLastOverElement = null;

          // Turn recursion protection back off
          wrapper.mFirstOutEventElement = null;
        }

        private bool SetContentState(Element aContent, EventState aState)
        {
            Element notifyContent1 = null;
            Element notifyContent2 = null;
            bool updateAncestors;

            if (aState == EventState.HOVER || aState == EventState.ACTIVE)
            {
                // Hover and active are hierarchical
                updateAncestors = true;

                // check to see that this state is allowed by style. Check dragover too?
                // XXX Is this even what we want?
                if (mCurrentTarget != null)
                {
                    var ui = mCurrentTarget.StyleUI;
                    if (ui.DisableUserInput)
                    {
                        return false;
                    }
                }

                if (aState == EventState.ACTIVE)
                {
                    if (aContent != mActiveContent)
                    {
                        notifyContent1 = aContent;
                        notifyContent2 = mActiveContent;
                        mActiveContent = aContent;
                    }
                }
                else
                {
                    if (aContent != mHoverContent)
                    {
                        notifyContent1 = aContent;
                        notifyContent2 = mHoverContent;
                        mHoverContent = aContent;
                    }
                }
            }
            else
            {
                updateAncestors = false;
                if (aState == EventState.DRAGOVER)
                {
                    if (aContent != sDragOverContent)
                    {
                        notifyContent1 = aContent;
                        notifyContent2 = sDragOverContent;
                        sDragOverContent = aContent;
                    }
                }
                else if (aState == EventState.URLTARGET)
                {
                    if (aContent != mURLTargetContent)
                    {
                        notifyContent1 = aContent;
                        notifyContent2 = mURLTargetContent;
                        mURLTargetContent = aContent;
                    }
                }
            }

            // We need to keep track of which of notifyContent1 and notifyContent2 is
            // getting the state set and which is getting it unset.  If both are
            // non-null, then notifyContent1 is having the state set and notifyContent2
            // is having it unset.  But if one of them is null, we need to keep track of
            // the right thing for notifyContent1 explicitly.
            bool content1StateSet = true;
            if (notifyContent1 == null)
            {
                // This is ok because FindCommonAncestor wouldn't find anything
                // anyway if notifyContent1 is null.
                notifyContent1 = notifyContent2;
                notifyContent2 = null;
                content1StateSet = false;
            }

            if (notifyContent1 != null)
            {
                if (updateAncestors)
                {
                    var commonAncestor = FindCommonAncestor(notifyContent1, notifyContent2);
                    if (notifyContent2 != null)
                    {
                        // It's very important to first notify the state removal and
                        // then the state addition, because due to labels it's
                        // possible that we're removing state from some element but
                        // then adding it again (say because mHoverContent changed
                        // from a control to its label).
                        UpdateAncestorState(notifyContent2, commonAncestor, aState, false);
                    }

                    UpdateAncestorState(notifyContent1, commonAncestor, aState,
                        content1StateSet);
                }
                else
                {
                    if (notifyContent2 != null)
                    {
                        DoStateChange(notifyContent2, aState, false);
                    }

                    DoStateChange(notifyContent1, aState, content1StateSet);
                }
            }

            return true;
        }

        private static void UpdateAncestorState(Element startNode,
            Element stopBefore,
            EventState state,
            bool addState)
        {
            for (;
                startNode != null && startNode != stopBefore;
                startNode = startNode.ParentElement)
            {
                DoStateChange(startNode, state, addState);
                var labelTarget = GetLabelTarget(startNode);
                if (labelTarget != null)
                {
                    DoStateChange(labelTarget, state, addState);
                }
            }

            if (addState)
            {
                // We might be in a situation where a node was in hover both
                // because it was hovered and because the label for it was
                // hovered, and while we stopped hovering the node the label is
                // still hovered.  Or we might have had two nested labels for the
                // same node, and while one is no longer hovered the other still
                // is.  In that situation, the label that's still hovered will be
                // aStopBefore or some ancestor of it, and the call we just made
                // to UpdateAncestorState with aAddState = false would have
                // removed the hover state from the node.  But the node should
                // still be in hover state.  To handle this situation we need to
                // keep walking up the tree and any time we find a label mark its
                // corresponding node as still in our state.
                for (; startNode != null; startNode = startNode.ParentElement)
                {
                    var labelTarget = GetLabelTarget(startNode);
                    if (labelTarget != null && !labelTarget.GetState(state))
                    {
                        DoStateChange(labelTarget, state, true);
                    }
                }
            }
        }

        private static void DoStateChange(Element element, EventState state, bool addState)
        {
            if (addState)
            {
                element.AddStates(state);
            }
            else
            {
                element.RemoveStates(state);
            }
        }

        private static Element GetLabelTarget(Element aStartNode)
        {
            // TODO: We don't have labels yet
            return null;
        }

        private static Element FindCommonAncestor(Element elementA, Element elementB)
        {
            if (elementA == null || elementB == null)
            {
                return null;
            }

            return elementA.CommonAncestor(elementB);
        }

        private struct ButtonTracking
        {
            public bool IsDown;
            public EventTargetImpl MouseDownOn;
            public PointF MouseDownPos;
            public UiTimestamp MouseDownTimestamp;
            public UiTimestamp ClickTimestamp;
        }

        private struct EnterLeaveDispatcher
        {
            public EnterLeaveDispatcher(UiWindowEventManager aESM, Element aTarget,
                Element aRelatedTarget,
                MouseEvent aMouseEvent,
                SystemEventType aEventMessage)
            {
                mESM = aESM;
                mMouseEvent = aMouseEvent;
                mEventMessage = aEventMessage;
                mTargets = new List<Element>();

                mRelatedTarget = aRelatedTarget;
                var commonParent = FindCommonAncestor(aTarget, aRelatedTarget);
                var current = aTarget;
                // Note, it is ok if commonParent is null!
                while (current != null && current != commonParent)
                {
                    mTargets.Add(current);
                    // mouseenter/leave is fired only on elements.
                    current = current.ParentElement;
                }
            }

            public void Dispatch()
            {
                if (mEventMessage == SystemEventType.MouseEnter)
                {
                    for (var i = mTargets.Count - 1; i >= 0; --i)
                    {
                        mESM.DispatchMouseOrPointerEvent(mMouseEvent, mEventMessage,
                            mTargets[i], mRelatedTarget);
                    }
                }
                else
                {
                    for (var i = 0; i < mTargets.Count; ++i)
                    {
                        mESM.DispatchMouseOrPointerEvent(mMouseEvent, mEventMessage,
                            mTargets[i], mRelatedTarget);
                    }
                }
            }

            UiWindowEventManager mESM;
            List<Element> mTargets;
            Element mRelatedTarget;
            MouseEvent mMouseEvent;
            SystemEventType mEventMessage;
        }

        private Element DispatchMouseOrPointerEvent(
            MouseEvent aMouseEvent, SystemEventType aMessage,
            Element aTargetContent, Element aRelatedContent)
        {
            // http://dvcs.w3.org/hg/webevents/raw-file/default/mouse-lock.html#methods
            // "[When the mouse is locked on an element...e]vents that require the concept
            // of a mouse cursor must not be dispatched (for example: mouseover,
            // mouseout).
            if (sIsPointerLocked && (aMessage == SystemEventType.MouseLeave || aMessage == SystemEventType.MouseEnter ||
                                     aMessage == SystemEventType.MouseOver || aMessage == SystemEventType.MouseOut))
            {
                Element pointerLockedElement = sPointerLockedElement;
                if (pointerLockedElement == null)
                {
                    Logger.Warn("Should have pointer locked element, but didn't.");
                    return null;
                }

                return pointerLockedElement;
            }

            if (aTargetContent == null)
            {
                return null;
            }

            var targetContent = aTargetContent;
            var relatedContent = aRelatedContent;

            var dispatchEvent = new MouseEvent(aMessage, new MouseEventInit()
            {
                View = aMouseEvent.View,
                Detail = aMouseEvent.Detail,
                ScreenX = aMouseEvent.ScreenX,
                ScreenY = aMouseEvent.ScreenY,
                ClientX = aMouseEvent.ClientX,
                ClientY = aMouseEvent.ClientY,
                CtrlKey = aMouseEvent.CtrlKey,
                ShiftKey = aMouseEvent.ShiftKey,
                AltKey = aMouseEvent.AltKey,
                MetaKey = aMouseEvent.MetaKey,
                Button = aMouseEvent.Button,
                Buttons = aMouseEvent.Buttons,
                RelatedTarget = relatedContent,
            });

            var previousTarget = mCurrentTarget;
            mCurrentTarget = targetContent;

            targetContent.Dispatch(dispatchEvent);

            mCurrentTarget = previousTarget;

            return targetContent;
        }

        private class OverOutElementsWrapper
        {
            public Element mLastOverFrame;

            public Element mLastOverElement;

            // The last element on which we fired a over event, or null if
            // the last over event we fired has finished processing.
            public Element mFirstOverEventElement;

            // The last element on which we fired a out event, or null if
            // the last out event we fired has finished processing.
            public Element mFirstOutEventElement;
        };
    }
}