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