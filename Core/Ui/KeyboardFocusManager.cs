using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public class KeyboardFocusManager
{
    /// <summary>
    /// The widget that is the current keyboard focus.
    /// </summary>
    public WidgetBase? KeyboardFocus { get; private set; }

    private readonly WidgetBase _treeRoot;

    public KeyboardFocusManager(WidgetBase treeRoot)
    {
        _treeRoot = treeRoot;
    }

    public void FocusFirstFocusableChild(WidgetBase widget)
    {
        KeyboardFocus = FindFocusCandidate(widget, false, widget);
    }

    public void MoveFocusByKeyboard(bool backwards)
    {
        WidgetBase? candidate;
        if (_treeRoot.FirstChild == null)
        {
            KeyboardFocus = null;
            return;
        }

        if (!backwards)
        {
            // The first focusable element when navigating forward is simply the first
            // Otherwise just start with the first one following the current keyboard focus
            candidate = KeyboardFocus == null
                ? _treeRoot.FirstChild
                : KeyboardFocus.Following(predicate: CanContainFocus);
        }
        else
        {
            // 
            candidate = KeyboardFocus == null
                ? _treeRoot.LastInclusiveDescendant()
                : KeyboardFocus.Preceding();
        }

        KeyboardFocus = FindFocusCandidate(candidate, backwards, _treeRoot);
    }

    /// <summary>
    /// Move focus by holding down the primary mouse button on the given widget.
    /// This may also focus the closest focusable ancestor instead.
    /// </summary>
    public void MoveFocusByMouseDown(WidgetBase widget)
    {
        foreach (var candidate in widget.EnumerateSelfAndAncestors())
        {
            if (widget.FocusMode == FocusMode.User && CanContainFocus(widget))
            {
                KeyboardFocus = candidate;
                return;
            }
        }

        // Nothing focusable
        KeyboardFocus = null;
    }
    
    /// <summary>
    /// Moves keyboard focus to the given element programatically.
    /// If the element cannot be focused, the method fails silently.
    /// </summary>
    public void MoveFocus(WidgetBase widget)
    {
        if (widget.FocusMode == FocusMode.None || !CanContainFocus(widget))
        {
            return;
        }
        
        KeyboardFocus = widget;
    }
    
    /// <summary>
    /// Removes the current keyboard focus.
    /// </summary>
    public void Blur()
    {
        KeyboardFocus = null;
    }
    
    private static WidgetBase? FindFocusCandidate(WidgetBase? candidate, bool backwards, WidgetBase treeRoot)
    {
        while (candidate != null)
        {
            var skipChildren = false;

            // Skip invisible and disabled elements and their children
            if (!CanContainFocus(candidate))
            {
                skipChildren = true;
            }
            else if (candidate.FocusMode == FocusMode.User)
            {
                return candidate;
            }

            candidate = !backwards ? candidate.Following(treeRoot, skipChildren) : candidate.Preceding();
        }

        // We've reached the end of the focus list
        return null;
    }

    private static bool CanContainFocus(WidgetBase widget) => widget is {Visible: true, Disabled: false};

}