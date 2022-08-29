using System.Diagnostics;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public static class WidgetExtensions
{
    /// <summary>
    /// For this widget, find the first ancestor that it shares with another given widget.
    /// Inspired by node.cc in Blinks source code.
    /// </summary>
    /// <returns>Null, if the other widget is null or if there are no common ancestors.</returns>
    public static WidgetBase? GetCommonAncestor(this WidgetBase self, WidgetBase other)
    {
        if (self.UiManager != other.UiManager)
        {
            return null;
        }

        // Determine the depth of this node in the UI tree, and fast-exit if we
        // find the other node along the way to the root.
        var selfDepth = 0;
        for (var node = self; node != null; node = node.Parent)
        {
            if (node == other)
            {
                return node;
            }

            selfDepth++;
        }

        // Same as above, but for the other node.
        var otherDepth = 0;
        for (var node = other; node != null; node = node.Parent)
        {
            if (node == self)
            {
                return node;
            }

            otherDepth++;
        }

        // The common ancestor MUST have a depth that is less than min(selfDepth, otherDepth)
        // So first, we move the deeper iterator up to be on the same level of the other one
        var selfIt = self;
        var otherIt = other;
        if (selfDepth > otherDepth)
        {
            for (var i = selfDepth; i > otherDepth; --i)
            {
                selfIt = selfIt!.Parent;
            }
        }
        else if (otherDepth > selfDepth)
        {
            for (var i = otherDepth; i > selfDepth; --i)
            {
                otherIt = otherIt!.Parent;
            }
        }

        // Since both iterators are now on the same level, we check if they refer to the same node,
        // which means we found the first common ancestor.
        while (selfIt != null)
        {
            if (selfIt == otherIt)
            {
                return selfIt;
            }

            // Otherwise, we move them both up one level and continue
            selfIt = selfIt.Parent;
            otherIt = otherIt!.Parent;
        }

        Debug.Assert(selfIt == null && otherIt == null, "Since otherIt and selfIt were both at the same level, both should be null if no common ancestor was found");
        return null;
    }

    public static bool IsInclusiveAncestorOf(this WidgetBase self, WidgetBase other)
    {
        return self == other || IsAncestorOf(self, other);
    }

    public static bool IsAncestorOf(this WidgetBase self, WidgetBase other)
    {
        return self != other && other.IsDescendantOf(self);
    }

    /// <summary>
    /// Checks if a widget contains another widget.
    /// </summary>
    public static bool Contains(this WidgetBase self, WidgetBase? other)
    {
        return self == other || other.IsDescendantOf(self);
    }

    /// <summary>
    /// Checks if a widget is a descendant of another node.
    /// </summary>
    public static bool IsDescendantOf(this WidgetBase self, WidgetBase? other)
    {
        // Return true if other is an ancestor of this, otherwise false
        if (other == null || self.UiManager != other.UiManager)
        {
            return false;
        }

        for (var parent = self.Parent; parent != null; parent = parent.Parent)
        {
            if (parent == other)
            {
                return true;
            }
        }

        return false;
    }
}