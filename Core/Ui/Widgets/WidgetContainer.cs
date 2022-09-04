using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetContainer : WidgetBase
{
    private readonly List<WidgetBase> _children = new();

    private int _scrollOffsetY;
    
    /// <summary>
    /// Previously this was implemented by always returning true from the mouse event handler.
    /// TODO: Actually implement this, and check if it's not more like a modal backdrop
    /// </summary>
    public bool PreventsInGameInteraction { get; set; }

    public bool ClipChildren { get; set; } = true;

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

        // Containers are usually empty and should be click through where there is no content
        HitTesting = HitTestingMode.Content;
    }
    
    public virtual void Add(WidgetBase childWidget)
    {
        if (childWidget.Parent == this)
        {
            Trace.Assert(_children.Contains(childWidget));
            return;
        }
        if (childWidget.Parent != null)
        {
            childWidget.Parent.Remove(childWidget);
        }

        childWidget.Parent = this;
        // If the child widget was a top-level window before, remove it
        if (childWidget is WidgetContainer otherContainer)
        {
            UiManager?.RemoveWindow(otherContainer);
        }

        childWidget.AttachToTree(UiManager);
        _children.Add(childWidget);
        UiManager?.RefreshMouseOverState();
    }

    public void Remove(WidgetBase childWidget)
    {
        Trace.Assert(childWidget.Parent == this);

        childWidget.Parent = null;
        _children.Remove(childWidget);
        childWidget.AttachToTree(null);
        UiManager?.RefreshMouseOverState();
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

    public override void BringToFront()
    {
        if (Parent == null)
        {
            UiManager?.BringToFront(this);
        }
        else
        {
            base.BringToFront();
        }
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

    public override void HandleHotkeyAction(HotkeyActionMessage msg)
    {
        // Iterate in reverse order since this list is ordered in ascending z-order
        for (var i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            if (child.Visible)
            {
                child.HandleHotkeyAction(msg);
                if (msg.IsHandled)
                {
                    return;
                }
            }
        }

        base.HandleHotkeyAction(msg);
    }

    public override void OnUpdateTime(TimePoint now)
    {
        base.OnUpdateTime(now);

        foreach (var widget in _children)
        {
            widget.OnUpdateTime(now);
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

};