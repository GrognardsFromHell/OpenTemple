using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Ui.Widgets;

public sealed class Anchors
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly WidgetBase _owner;
    private AnchorReference _left;
    private AnchorReference _horizontalCenter;
    private AnchorReference _right;
    private AnchorReference _top;
    private AnchorReference _verticalCenter;
    private AnchorReference _bottom;

    public Anchors(WidgetBase owner)
    {
        _owner = owner;
    }

    public ref AnchorReference Left => ref _left;
    public ref AnchorReference HorizontalCenter => ref _horizontalCenter;
    public ref AnchorReference Right => ref _right;
    public ref AnchorReference Top => ref _top;
    public ref AnchorReference VerticalCenter => ref _verticalCenter;
    public ref AnchorReference Bottom => ref _bottom;

    /// <summary>
    /// True if any edges are anchored. 
    /// </summary>
    public bool HasAnchors => Left != default || Top != default || Right != default || Bottom != default || HorizontalCenter != default || VerticalCenter != default;

    /// <summary>
    /// If both the left and right edge are anchored, the width of the element is defined by the anchors. 
    /// </summary>
    public bool DefinesWidth => Left != default && Right != default;

    /// <summary>
    /// If both the top and bottom edge are anchored, the width of the element is defined by the anchors. 
    /// </summary>
    public bool DefinesHeight => Top != default && Bottom != default;

    public void FillParent()
    {
        _left.ToParent(AnchorEdge.Left);
        _right.ToParent(AnchorEdge.Right);
        _top.ToParent(AnchorEdge.Top);
        _bottom.ToParent(AnchorEdge.Bottom);
        _horizontalCenter = default;
        _verticalCenter = default;
    }

    /// <summary>
    /// Tries to apply configured anchors to the owning widget.
    /// </summary>
    /// <returns>True if anchors were successfully applied. False if there's an error condition.</returns>
    public bool Apply(ref RectangleF layoutBox)
    {
        return ApplyHorizontal(ref layoutBox) && ApplyVertical(ref layoutBox);
    }

    private bool ApplyHorizontal(ref RectangleF layoutBox)
    {
        WidgetBase? leftWidget = null;
        WidgetBase? rightWidget = null;
        WidgetBase? hCenterWidget = null;
        if (Left.IsValid && !ResolveWithValidLayout(out leftWidget))
        {
            return false;
        }

        if (Right.IsValid && !ResolveWithValidLayout(out rightWidget))
        {
            return false;
        }

        if (HorizontalCenter.IsValid && !ResolveWithValidLayout(out hCenterWidget))
        {
            return false;
        }

        // Prioritize left edge
        if (leftWidget != null)
        {
            layoutBox.X = GetEdgePosition(leftWidget, Left.Edge);
            // Should it be stretched?
            if (rightWidget != null)
            {
                var rightEdge = GetEdgePosition(rightWidget, Right.Edge);
                layoutBox.Width = rightEdge - layoutBox.X;
            }

            if (hCenterWidget != null)
            {
                Logger.Warn("Cannot anchor left and horizontal center of widget {0} simultaneously.", _owner);
            }
        }
        else if (rightWidget != null)
        {
            // We're sure at this point that left is null
            layoutBox.X = GetEdgePosition(rightWidget, Right.Edge) - layoutBox.Width;

            if (hCenterWidget != null)
            {
                Logger.Warn("Cannot anchor right and horizontal center of widget {0} simultaneously.", _owner);
            }
        }
        else if (hCenterWidget != null)
        {
            // Center horizontally
            var hCenter = GetEdgePosition(hCenterWidget, HorizontalCenter.Edge);
            layoutBox.X = hCenter - layoutBox.Width / 2;
        }

        return true;
    }

    private bool ApplyVertical(ref RectangleF layoutBox)
    {
        WidgetBase? topWidget = null;
        WidgetBase? bottomWidget = null;
        WidgetBase? vCenterWidget = null;
        if (Top.IsValid && !ResolveWithValidLayout(out topWidget))
        {
            return false;
        }

        if (Bottom.IsValid && !ResolveWithValidLayout(out bottomWidget))
        {
            return false;
        }

        if (VerticalCenter.IsValid && !ResolveWithValidLayout(out vCenterWidget))
        {
            return false;
        }

        // Prioritize top edge
        if (topWidget != null)
        {
            layoutBox.Y = GetEdgePosition(topWidget, Top.Edge);
            // Should it be stretched?
            if (bottomWidget != null)
            {
                var bottomEdge = GetEdgePosition(bottomWidget, Bottom.Edge);
                layoutBox.Height = bottomEdge - layoutBox.Y;
            }

            if (vCenterWidget != null)
            {
                Logger.Warn("Cannot anchor top and vertical center of widget {0} simultaneously.", _owner);
            }
        }
        else if (bottomWidget != null)
        {
            // We're sure at this point that top is null
            layoutBox.Y = GetEdgePosition(bottomWidget, Bottom.Edge) - layoutBox.Height;

            if (vCenterWidget != null)
            {
                Logger.Warn("Cannot anchor bottom and vertical center of widget {0} simultaneously.", _owner);
            }
        }
        else if (vCenterWidget != null)
        {
            // Move the widget's vertical center to the referenced edge position
            var vCenter = GetEdgePosition(vCenterWidget, VerticalCenter.Edge);
            layoutBox.Y = vCenter - layoutBox.Height / 2;
        }
        
        return true;
    }

    private static float GetEdgePosition(WidgetBase widget, AnchorEdge edge)
    {
        Debug.Assert(widget.HasValidLayout);
        return edge switch
        {
            AnchorEdge.Left => widget.LayoutBox.X,
            AnchorEdge.HorizontalCenter => widget.LayoutBox.X + widget.LayoutBox.Width / 2,
            AnchorEdge.Right => widget.LayoutBox.Right,
            AnchorEdge.Top => widget.LayoutBox.Y,
            AnchorEdge.VerticalCenter => widget.LayoutBox.Y + widget.LayoutBox.Height / 2,
            AnchorEdge.Bottom => widget.LayoutBox.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
        };
    }

    private bool ResolveWithValidLayout([MaybeNullWhen(false)] out WidgetBase widget)
    {
        return Resolve(in Left, out widget) && widget.HasValidLayout;
    }

    private bool Resolve(in AnchorReference reference, [MaybeNullWhen(false)] out WidgetBase widget)
    {
        // Parent-less control can have no references
        var parent = _owner.Parent;
        if (parent == null)
        {
            widget = null;
            return false;
        }

        if (reference.Sibling != null)
        {
            // Check if the referenced sibling has the same parent as us, otherwise it's stale!
            if (reference.Sibling.Parent != parent)
            {
                Logger.Warn("Anchor reference for {0} has stale reference to {1}, which is no longer a sibling", _owner, reference.Sibling);
                widget = null;
                return false;
            }

            widget = reference.Sibling;
            return true;
        }

        if (reference.SiblingId != null)
        {
            // Try to find the appropriate control among the parents children
            foreach (var sibling in parent.Children)
            {
                if (sibling.Id == reference.SiblingId)
                {
                    if (sibling == _owner)
                    {
                        Logger.Warn("Anchor reference from widget {0} to id '{1}' is self-referential", _owner, reference.SiblingId);
                        widget = null;
                        return false;
                    }

                    widget = sibling;
                    return true;
                }
            }
        }

        widget = parent;
        return true;
    }
}

public enum AnchorEdge
{
    Undefined,
    Left,
    HorizontalCenter,
    Right,
    Top,
    VerticalCenter,
    Bottom
}

/// <param name="Sibling">If not null, specifies the widget being referenced. If both this and <see cref="SiblingId"/> are null, the parent is being referenced.</param>
/// <param name="SiblingId">If not null, specifies the <see cref="WidgetBase.Id"/> of the referenced sibling. If both this and <see cref="Sibling"/> are null, the parent is being referenced.</param>
/// <param name="Edge">The edge of the widget that is being referenced.</param>
public readonly record struct AnchorReference(WidgetBase? Sibling, string? SiblingId, AnchorEdge Edge)
{
    public static AnchorReference Parent(AnchorEdge edge) => new AnchorReference(null, null, edge);

    public bool IsSiblingReference => IsValid && (Sibling != null || SiblingId != null);

    public bool IsParentReference => IsValid && Sibling == null && SiblingId == null;

    public bool IsValid => Edge != AnchorEdge.Undefined;
}

public static class AnchorReferenceExtensions
{
    public static void ToParent(this ref AnchorReference reference, AnchorEdge toEdge)
    {
        reference = AnchorReference.Parent(toEdge);
    }

    public static void Unbind(this ref AnchorReference reference)
    {
        reference = default;
    }
}