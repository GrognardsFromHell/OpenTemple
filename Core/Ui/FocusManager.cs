using System.Diagnostics;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public class FocusManager
    {
        private readonly Document _document;

        private Element _pendingFocusChange;

        public Element Focused { get; private set; }

        public FocusManager(Document document)
        {
            _document = document;
            Focused = document.DocumentElement;
        }

        public void NotifyElementRemoval(Node node)
        {
            // https://html.spec.whatwg.org/#focus-fixup-rule
            if (node == Focused)
            {
                _pendingFocusChange = _document.DocumentElement;
            }

            // Something tried to .focus() an element, and then it got removed
            // before that focus took effect
            if (node == _pendingFocusChange)
            {
                _pendingFocusChange = null;
            }
        }

        public void RequestFocus(Element element, bool immediate = false)
        {
            _pendingFocusChange = element;
            if (immediate)
            {
                PerformPendingFocusChange();
            }
        }

        public void PerformPendingFocusChange()
        {
            var aContentToFocus = _pendingFocusChange;
            _pendingFocusChange = null;

            if (aContentToFocus == Focused)
            {
                return;
            }

            var previouslyFocused = Focused;
            if (previouslyFocused != null)
            {
                NotifyFocusStateChange(Focused, aContentToFocus, false);
            }

            if (aContentToFocus != null)
            {
                NotifyFocusStateChange(aContentToFocus, null, true);
            }

            Focused = aContentToFocus;

            if (previouslyFocused != null)
            {
                previouslyFocused.Dispatch(new UiEvent(SystemEventType.FocusOut, new FocusEventInit()
                {
                    Bubbles = true,
                    Composed = true,
                    RelatedTarget = Focused
                }));
                previouslyFocused.Dispatch(new UiEvent(SystemEventType.Blur, new FocusEventInit()
                {
                    Composed = true,
                    RelatedTarget = Focused
                }));
            }

            if (Focused != null)
            {
                Focused.Dispatch(new UiEvent(SystemEventType.FocusIn, new FocusEventInit()
                {
                    Bubbles = true,
                    Composed = true,
                    RelatedTarget = previouslyFocused
                }));
                Focused.Dispatch(new UiEvent(SystemEventType.Focus, new FocusEventInit()
                {
                    Bubbles = true,
                    Composed = true,
                    RelatedTarget = previouslyFocused
                }));
            }
        }

        private void NotifyFocusStateChange(Element element,
            Element aContentToFocus,
            bool gettingFocus)
        {
            Debug.Assert(aContentToFocus == null || !gettingFocus);

            Element commonAncestor = null;
            if (aContentToFocus != null)
            {
                commonAncestor = element.CommonAncestor(aContentToFocus);
            }

            if (gettingFocus)
            {
                element.AddStates(EventState.FOCUS);
            }
            else
            {
                element.RemoveStates(EventState.FOCUS);
            }

            for (var content = element;
                content != null && content != commonAncestor;
                content = content.ParentElement)
            {
                if (gettingFocus)
                {
                    if (content.GetState(EventState.FOCUS_WITHIN))
                    {
                        break;
                    }

                    content.AddStates(EventState.FOCUS_WITHIN);
                }
                else
                {
                    content.RemoveStates(EventState.FOCUS_WITHIN);
                }
            }
        }

        public void MoveFocus(bool forward)
        {
            var element = GetNextFocusableElement(forward);

            RequestFocus(element, true);
        }

        private bool CanBeFocusedNow(Node node)
        {
            if (node is WidgetBase {IsFocusable: true} widget)
            {
                return widget.IsEffectivelyVisible();
            }

            return false;
        }

        private Element GetNextFocusableElement(bool forward)
        {
            var focusable = _document.TreeToArray(null, CanBeFocusedNow);
            if (focusable.Count == 0)
            {
                return null;
            }

            if (Focused != null)
            {
                var index = focusable.IndexOf(Focused);
                if (index != -1)
                {
                    index += forward ? 1 : -1;
                    if (index < 0)
                    {
                        index += focusable.Count;
                    }
                    else if (index >= focusable.Count)
                    {
                        index -= focusable.Count;
                    }

                    return (Element) focusable[index];
                }
            }

            return (Element) (forward ? focusable[0] : focusable[^1]);
        }
    }
}