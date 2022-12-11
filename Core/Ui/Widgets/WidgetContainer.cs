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
    
    public IReadOnlyList<WidgetBase> Children => _children;

    /// <summary>
    /// Previously this was implemented by always returning true from the mouse event handler.
    /// TODO: Actually implement this, and check if it's not more like a modal backdrop
    /// </summary>
    public bool PreventsInGameInteraction { get; set; }

    public bool ClipChildren { get; set; } = true;

    public WidgetContainer([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1)
        : this(0, 0, 0, 0, filePath, lineNumber)
    {
    }

    public WidgetContainer(Size size, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1)
        : this(0, 0, size.Width, size.Height, filePath, lineNumber)
    {
    }

    public WidgetContainer(Rectangle rectangle, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
        filePath,
        lineNumber)
    {
    }

    public WidgetContainer(int width, int height, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : this(0, 0, width, height, filePath, lineNumber)
    {
    }

    public WidgetContainer(int x, int y, int width, int height, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : base(filePath, lineNumber)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;

        // Containers are usually empty and should be click through where there is no content
        HitTesting = HitTestingMode.Content;
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
        UiManager?.RefreshMouseOverState();
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
        childWidget.AttachToTree(null);
        UiManager?.RefreshMouseOverState();
        return true;
    }

    public virtual void Clear(bool disposeChildren = false)
    {
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

        for (var i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];

            if (!child.Visible)
            {
                continue;
            }

            var localX = x - child.X;
            var localY = y - child.Y + _scrollOffsetY;
            if (localY < 0 || localY >= child.Height)
            {
                continue;
            }

            if (localX < 0 || localX >= child.Width)
            {
                continue;
            }

            var result = child.PickWidget(localX, localY);
            if (result != null)
            {
                return result;
            }
        }

        return base.PickWidget(x, y);
    }

    protected override void Dispose(bool disposing)
    {
        for (var i = _children.Count - 1; i >= 0; i--)
        {
            _children[i].Dispose();
        }

        // Child widgets should have removed themselves from this list
        Trace.Assert(_children.Count == 0);

        base.Dispose(disposing);
    }

    public override void Render()
    {
        if (!Visible)
        {
            return;
        }

        ContentOffset = new Point(0, _scrollOffsetY);

        base.Render();

        var visArea = GetVisibleArea();

        var clipAreaSet = false;

        foreach (var child in _children)
        {
            if (child.Visible)
            {
                if (ClipChildren && !clipAreaSet)
                {
                    Tig.RenderingDevice.SetScissorRect(visArea.X, visArea.Y, visArea.Width, visArea.Height);
                    clipAreaSet = true;
                }

                child.Render();
            }
        }

        if (clipAreaSet)
        {
            Tig.RenderingDevice.ResetScissorRect();
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
        _children.Sort((windowA, windowB) => windowA.ZIndex.CompareTo(windowB.ZIndex));

        // Reassign a zindex in monotonous order to those windows that dont have one
        for (var i = 0; i < _children.Count; ++i)
        {
            var window = _children[i];
            if (window.ZIndex == 0)
            {
                window.ZIndex = i * 100;
            }
        }
    }

    public void BringToFront(WidgetBase widget)
    {
        widget.ZIndex = _children
            .Where(child => child != widget)
            .Select(child => child.ZIndex)
            .DefaultIfEmpty()
            .Max() + 1;
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

    public void SendToBack(WidgetBase widget)
    {
        widget.ZIndex = int.MinValue;
        SortChildren();
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
            foreach (var child in container.Children)
            {
                foreach (var widgetBase in ElementsFromPoint(child, x, y))
                {
                    yield return widgetBase;
                }
            }
        }

        yield return context;
    }
}
