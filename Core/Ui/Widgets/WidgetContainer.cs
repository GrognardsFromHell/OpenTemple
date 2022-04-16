using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetContainer : WidgetBase
{
    public WidgetContainer(Size size, [CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1)
        : this(0, 0, size.Width, size.Height, filePath, lineNumber)
    {
    }

    public WidgetContainer(Rectangle rectangle, [CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1) : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, filePath,
        lineNumber)
    {
    }

    public WidgetContainer(int width, int height, [CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1) : this(0, 0, width, height, filePath, lineNumber)
    {
    }

    public WidgetContainer(int x, int y, int width, int height, [CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1) : base(filePath, lineNumber)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;

        Globals.UiManager.AddWindow(this);

        // Containers are usually empty and should be click through where there is no content
        // but this only applies to top-level windows, which is tricky to model here.
        PreciseHitTest = GetType() == typeof(WidgetContainer);
    }

    public bool ClipChildren { get; set; } = true;

    public virtual void Add(WidgetBase childWidget)
    {
        if (childWidget.GetParent() != null && childWidget.GetParent() != this)
        {
            childWidget.GetParent().Remove(childWidget);
        }

        childWidget.SetParent(this);
        // If the child widget was a top-level window before, remove it
        if (childWidget is WidgetContainer otherContainer)
        {
            Globals.UiManager.RemoveWindow(otherContainer);
        }

        _children.Add(childWidget);
        Globals.UiManager.RefreshMouseOverState();
    }

    public LgcyWindowMouseState MouseState { get; internal set; }

    public int ZIndex { get; set; }

    public void Remove(WidgetBase childWidget)
    {
        Trace.Assert(childWidget.GetParent() == this);

        childWidget.SetParent(null);
        _children.Remove(childWidget);
        Globals.UiManager.RefreshMouseOverState();
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

    public override WidgetBase PickWidget(int x, int y)
    {
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

    public override void BringToFront()
    {
        if (_parent == null)
        {
            Globals.UiManager.BringToFront(this);
        }
        else
        {
            base.BringToFront();
        }
    }

    public override bool IsContainer()
    {
        return true;
    }

    public List<WidgetBase> GetChildren()
    {
        return _children;
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

    public override bool HandleMouseMessage(MessageMouseArgs msg)
    {
        var area = GetContentArea();

        // Iterate in reverse order since this list is ordered in ascending z-order
        for (var i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];

            int x = msg.X - area.X;
            int y = msg.Y - area.Y + GetScrollOffsetY();

            if (child.Visible & x >= child.X && y >= child.Y && x < child.X + child.Width &&
                y < child.Y + child.Height)
            {
                if (child.HandleMouseMessage(msg))
                {
                    return true;
                }
            }
        }

        return base.HandleMouseMessage(msg);
    }

    public override void OnUpdateTime(TimePoint timeMs)
    {
        base.OnUpdateTime(timeMs);

        foreach (var widget in _children)
        {
            widget.OnUpdateTime(timeMs);
        }
    }

    public void SetScrollOffsetY(int scrollY)
    {
        _scrollOffsetY = scrollY;
        Globals.UiManager.RefreshMouseOverState();
    }

    [TempleDllLocation(0x101fa150)]
    public int GetScrollOffsetY()
    {
        return _scrollOffsetY;
    }

    private List<WidgetBase> _children = new();

    private int _scrollOffsetY = 0;

    public void CenterOnScreen()
    {
        Trace.Assert(GetParent() == null);
        var screenSize = Globals.UiManager.CanvasSize;
        X = (screenSize.Width - Width) / 2;
        Y = (screenSize.Height - Height) / 2;
    }
};