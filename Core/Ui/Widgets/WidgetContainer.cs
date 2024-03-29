using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetContainer : WidgetBase
{
    private int _scrollOffsetY;

    private readonly List<WidgetBase> _children = new();

    // Children ordered by ascending Z-Index
    private readonly List<WidgetBase> _zOrderChildren = new();

    public override IReadOnlyList<WidgetBase> Children => _children;

    public IReadOnlyList<WidgetBase> ZOrderChildren => _zOrderChildren;

    /// <summary>
    /// Previously this was implemented by always returning true from the mouse event handler.
    /// TODO: Actually implement this, and check if it's not more like a modal backdrop
    /// </summary>
    public bool PreventsInGameInteraction { get; set; }

    public bool ClipChildren { get; set; } = true;

    public WidgetContainer([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : base(filePath, lineNumber)
    {
        // Containers are usually empty and should be click through where there is no content
        HitTesting = HitTestingMode.Content;
    }
    
    public WidgetContainer(RectangleF rect, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : this(filePath, lineNumber)
    {
        Pos = rect.Location;
        PixelSize = rect.Size;
    }

    public virtual void Add(WidgetBase childWidget)
    {
        if (childWidget.Parent == this)
        {
            Trace.Assert(Children.Contains(childWidget));
            return;
        }

        childWidget.Parent?.Remove(childWidget);

        childWidget.Parent = this;

        childWidget.AttachToTree(UiManager);
        _children.Add(childWidget);
        SortChildren();
        
        NotifyLayoutChange(LayoutChangeFlag.Content);
    }

    public bool Remove(WidgetBase childWidget)
    {
        if (childWidget.Parent == null)
        {
            return false;
        }

        Trace.Assert(childWidget.Parent == this);

        childWidget.Parent = null;
        _children.Remove(childWidget);
        _zOrderChildren.Remove(childWidget);
        childWidget.AttachToTree(null);

        NotifyLayoutChange(LayoutChangeFlag.Content);
        
        return true;
    }

    public virtual void Clear(bool disposeChildren = false)
    {
        _zOrderChildren.Clear();
        for (var i = _children.Count - 1; i >= 0; i--)
        {
            if (disposeChildren)
            {
                // This will auto remove from the list
                _children[i].Dispose();
            }
            else
            {
                Remove(_children[i]);
            }
        }
    }

    public override WidgetBase? PickWidget(float x, float y)
    {
        if (!Visible)
        {
            return null;
        }

        for (var i = _zOrderChildren.Count - 1; i >= 0; i--)
        {
            var child = _zOrderChildren[i];

            if (child.Visible && child.BorderArea.Contains(x, y))
            {
                var localX = x - child.BorderArea.X;
                var localY = y - child.BorderArea.Y;
                var result = child.PickWidget(localX, localY);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return base.PickWidget(x, y);
    }

    protected override void Dispose(bool disposing)
    {
        _zOrderChildren.Clear();
        for (var i = _children.Count - 1; i >= 0; i--)
        {
            _children[i].Dispose();
        }

        // Child widgets should have removed themselves from this list
        Trace.Assert(_children.Count == 0);

        base.Dispose(disposing);
    }

    public override void Render(UiRenderContext context)
    {
        if (!Visible)
        {
            return;
        }

        ContentOffset = new Point(0, _scrollOffsetY);

        base.Render(context);

        var clipAreaSet = false;

        foreach (var child in _zOrderChildren)
        {
            if (child.Visible)
            {
                if (ClipChildren && !clipAreaSet)
                {
                    context.PushScissorRect(GetViewportPaddingArea(true));
                    clipAreaSet = true;
                }

                child.Render(context);
            }
        }

        if (clipAreaSet)
        {
            context.PopScissorRect();
        }
    }

    public override void OnUpdateTime(TimePoint now)
    {
        base.OnUpdateTime(now);

        // Make a local copy of the widget list, because it could be modified by the event handlers
        var tempWidgets = ArrayPool<WidgetBase>.Shared.Rent(_children.Count);
        try
        {
            _children.CopyTo(tempWidgets);
            var widgets = tempWidgets.AsSpan(0, _children.Count);

            // Dispatch time update messages continuously to all advanced widgets
            foreach (var entry in widgets)
            {
                // Only dispatch if the widget wasn't moved away
                if (entry.Parent == this)
                {
                    entry.OnUpdateTime(now);

                    // Stop updating if we've been detached from the tree
                    if (!IsInTree)
                    {
                        break;
                    }
                }
            }
        }
        finally
        {
            ArrayPool<WidgetBase>.Shared.Return(tempWidgets);
        }
    }

    public override void AttachToTree(UiManager? manager)
    {
        base.AttachToTree(manager);

        foreach (var child in _children)
        {
            child.AttachToTree(manager);
        }
    }

    public override WidgetBase? FirstChild => _children.Count > 0 ? _children[0] : null;

    public override WidgetBase? LastChild => _children.Count > 0 ? _children[^1] : null;

    protected override SizeF ComputePreferredPaddingAreaSize(float availableWidth, float availableHeight)
    {
        var area = base.ComputePreferredPaddingAreaSize(availableWidth, availableHeight);

        foreach (var child in Children)
        {
            var childSize = child.ComputePreferredBorderAreaSize(availableWidth, availableHeight);
            area.Width = Math.Max(area.Width, childSize.Width);
            area.Height = Math.Max(area.Height, childSize.Height);
        }

        return area;
    }

    public void SetScrollOffsetY(int scrollY)
    {
        _scrollOffsetY = scrollY;
        UiManager?.RefreshMouseOverState();
    }

    [TempleDllLocation(0x101fa150)]
    public int GetScrollOffsetY()
    {
        return _scrollOffsetY;
    }

    /// <summary>
    /// This will sort the windows using their z-order in the order in which
    /// they should be rendered.
    /// </summary>
    private void SortChildren()
    {
        // Sort Windows by Z-Index
        _zOrderChildren.Clear();
        _zOrderChildren.AddRange(_children.OrderBy(child => child.ZIndex));
    }

    public void BringToFront(WidgetBase widget)
    {
        if (!_zOrderChildren.Contains(widget))
        {
            return;
        }

        // If the widget is already at the top and has a unique Z-Index, we don't do anything
        if (_zOrderChildren.Count == 1
            || (_zOrderChildren.Count >= 2
                && _zOrderChildren[^1] == widget
                && _zOrderChildren[^2].ZIndex < widget.ZIndex))
        {
            return;
        }

        var highestZIndex = _zOrderChildren[^1].ZIndex;
        widget.ZIndex = highestZIndex + 1;
        SortChildren();
    }

    public void MoveToBack(WidgetBase widget)
    {
        widget.ZIndex = _children
            .Where(child => child != widget)
            .Select(child => child.ZIndex)
            .DefaultIfEmpty()
            .Min() - 1;
        SortChildren();
    }

    public void SendToBack()
    {
        Parent?.SendToBack(this);
    }

    public void SendToBack(WidgetBase child)
    {
        if (child.Parent == this)
        {
            child.ZIndex = int.MinValue;
            SortChildren();
        }
    }

    /// <summary>
    /// Works like Document.elementsFromPoint and enumerates all elements that intersect the given point on screen,
    /// from topmost to bottommost.
    /// </summary>
    public IEnumerable<WidgetBase> ElementsFromPoint(int x, int y)
    {
        return ElementsFromPoint(this, x, y);
    }

    private static IEnumerable<WidgetBase> ElementsFromPoint(WidgetBase context, int x, int y)
    {
        if (!context.Visible || !context.HitTest(x, y))
        {
            yield break;
        }

        if (context is WidgetContainer container)
        {
            foreach (var child in container._zOrderChildren)
            {
                foreach (var widgetBase in ElementsFromPoint(child, x, y))
                {
                    yield return widgetBase;
                }
            }
        }

        yield return context;
    }

    public void TransformClientToViewport(ref RectangleF bounds, bool clip)
    {
        // Take our own padding box and transform it to the viewport using our parent
        // All layout of children is relative to the padding area.
        var ourViewportBox = PaddingArea;
        Parent?.TransformClientToViewport(ref ourViewportBox, clip);

        // Layout is relative to the parent padding area
        var scrollOffsetY = GetScrollOffsetY();

        bounds.X += ourViewportBox.X;
        bounds.Y += ourViewportBox.Y - scrollOffsetY;

        if (clip)
        {
            bounds = RectangleF.Intersect(ourViewportBox, bounds);
        }
    }

    /// <summary>
    /// Returns whether the size of this element depends on its content.
    /// </summary>
    public bool SizeDependsOnContent =>
        (!Anchors.Left.IsValid || !Anchors.Right.IsValid) && Width == Dimension.Auto
        || (!Anchors.Top.IsValid || !Anchors.Bottom.IsValid) && Height == Dimension.Auto;
}