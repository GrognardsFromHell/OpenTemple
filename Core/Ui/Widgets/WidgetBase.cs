#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Styles;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetBase : Styleable, IDisposable
{
    public string Name { get; set; }

    // Horizontal position relative to parent
    public int X { get; set; }

    // Vertical position relative to parent
    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool Visible
    {
        get => _visible;
        set
        {
            if (value != _visible)
            {
                _visible = value;
                Globals.UiManager.SetVisible(this, value);
            }
        }
    }

    public UiManager UiManager => Globals.UiManager;

    /// <summary>
    /// Content is shifted by this offset within the viewport of the widget.
    /// </summary>
    protected Point ContentOffset { get; set; }

    public Margins Margins
    {
        get => _margins;
        set => _margins = value;
    }

    public WidgetBase([CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1)
    {
        if (filePath != null)
        {
            mSourceURI = $"{Path.GetFileName(filePath)}:{lineNumber}";
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _parent?.Remove(this);
            Globals.UiManager.RemoveWidget(this);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event Action? OnBeforeRender;

    public event Func<Message, bool>? OnHandleMessage;

    /// <summary>
    /// Hit test the content of this widget instead of just checking against the content rectangle.
    /// </summary>
    public bool PreciseHitTest { get; set; } = false;

    public virtual bool HitTest(int x, int y)
    {
        var contentArea = GetContentArea();
        x += contentArea.X - _margins.Left;
        y += contentArea.Y - _margins.Top;

        if (!PreciseHitTest)
        {
            return contentArea.Contains(x, y);
        }

        UpdateLayout();

        foreach (var content in _content)
        {
            if (!content.Visible)
            {
                continue;
            }

            var contentRect = content.GetBounds();
            contentRect.Intersect(contentArea);

            if (contentRect.Contains(x, y))
            {
                return true;
            }
        }


        return false;
    }

    public virtual void Render()
    {
        if (!Visible)
        {
            return;
        }

        OnBeforeRender?.Invoke();

        UpdateLayout();

        var contentArea = GetContentArea();

        foreach (var content in _content)
        {
            if (!content.Visible || !content.GetBounds().IntersectsWith(contentArea))
            {
                continue;
            }

            content.Render();
        }
    }

    protected virtual void UpdateLayout()
    {
        ApplyAutomaticSizing();

        var contentArea = GetContentArea();

        // Size to content
        if (contentArea.Width == 0 && contentArea.Height == 0)
        {
            foreach (var content in _content)
            {
                var preferred = content.GetPreferredSize();
                contentArea.Width = Math.Max(contentArea.Width, preferred.Width);
                contentArea.Height = Math.Max(contentArea.Height, preferred.Height);
            }

            // set widget size (adding up the margins in addition to the content dimensions, since the overall size should include the margins)
            if (contentArea.Width != 0 && contentArea.Height != 0)
            {
                Width = contentArea.Width + _margins.Left + _margins.Right;
                Height = contentArea.Height + _margins.Top + _margins.Bottom;

                ApplyAutomaticSizing();
                contentArea = GetContentArea();
            }
        }

        foreach (var content in _content)
        {
            if (!content.Visible)
            {
                continue;
            }

            var contentBounds = contentArea;
            // Shift according to the content item positioning
            if (content.X != 0)
            {
                contentBounds.X += content.X;
                contentBounds.Width -= content.X;
                if (contentBounds.Width < 0)
                {
                    contentBounds.Width = 0;
                }
            }

            if (content.Y != 0)
            {
                contentBounds.Y += content.Y;
                contentBounds.Height -= content.Y;
                if (contentBounds.Height < 0)
                {
                    contentBounds.Height = 0;
                }
            }

            // If fixed width and height are used, the content area's width/height are overridden
            if (content.FixedWidth != 0)
            {
                contentBounds.Width = content.FixedWidth;
            }

            if (content.FixedHeight != 0)
            {
                contentBounds.Height = content.FixedHeight;
            }

            // Shift according to scroll offset for content
            if (ContentOffset != Point.Empty)
            {
                contentBounds.Offset(-ContentOffset.X, -ContentOffset.Y);
                // Cull the item when it's no longer visible at all
                if (!contentBounds.IntersectsWith(contentArea))
                {
                    contentBounds = Rectangle.Empty;
                }
            }

            if (content.GetBounds() != contentBounds)
            {
                content.SetBounds(contentBounds);
            }
        }
    }

    protected void ApplyAutomaticSizing()
    {
        if (mSizeToParent)
        {
            int containerWidth = _parent != null
                ? _parent.Width
                : Globals.UiManager.CanvasSize.Width;
            int containerHeight = _parent != null
                ? _parent.Height
                : Globals.UiManager.CanvasSize.Height;
            SetSize(new Size(containerWidth, containerHeight));
        }

        if (mCenterHorizontally)
        {
            int containerWidth = _parent != null
                ? _parent.Width
                : Globals.UiManager.CanvasSize.Width;
            int x = (containerWidth - Width) / 2;
            if (x != X)
            {
                X = x;
            }
        }

        if (mCenterVertically)
        {
            int containerHeight = _parent != null
                ? _parent.Height
                : Globals.UiManager.CanvasSize.Height;
            int y = (containerHeight - Height) / 2;
            if (y != Y)
            {
                Y = y;
            }
        }
    }

    public virtual bool HandleMessage(Message msg)
    {
        if (OnHandleMessage != null)
        {
            if (OnHandleMessage(msg))
            {
                return true;
            }
        }

        if (msg.type == MessageType.WIDGET && mWidgetMsgHandler != null)
        {
            return mWidgetMsgHandler(msg.WidgetArgs);
        }
        else if (msg.type == MessageType.KEYSTATECHANGE && mKeyStateChangeHandler != null)
        {
            return mKeyStateChangeHandler(msg.KeyStateChangeArgs);
        }
        else if (msg.type == MessageType.CHAR && mCharHandler != null)
        {
            return mCharHandler(msg.CharArgs);
        }
        else if (msg.type == MessageType.MOUSE)
        {
            return HandleMouseMessage(msg.MouseArgs);
        }

        return false;
    }

    public virtual bool IsContainer()
    {
        return false;
    }

    public virtual bool IsButton()
    {
        return false;
    }

    public virtual bool IsScrollView()
    {
        return false;
    }

    /**
         * Picks the widget a the x,y coordinate local to this widget.
         * Null if the coordinates are outside of this widget. If no
         * other widget inside is at the given coordinate, will just return this.
         */
    public virtual WidgetBase? PickWidget(int x, int y)
    {
        if (!Visible)
        {
            return null;
        }

        if (x >= _margins.Left &
            y >= _margins.Bottom &&
            x < Width - _margins.Right
            && y < Height - _margins.Top
            && HitTest(x, y))
        {
            return this;
        }

        return null;
    }

    public void AddContent(WidgetContent content)
    {
        content.Parent = this;
        _content.Add(content);
    }

    public void RemoveContent(WidgetContent content)
    {
        if (_content.Remove(content))
        {
            content.Parent = null;
        }
    }

    public void ClearContent()
    {
        foreach (var content in _content)
        {
            content.Parent = null;
        }

        _content.Clear();
    }

    public void Show()
    {
        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
    }

    public virtual void BringToFront()
    {
        var parent = _parent;
        if (parent != null)
        {
            parent.Remove(this);
            parent.Add(this);
        }
    }

    public void SetParent(WidgetContainer parent)
    {
        Trace.Assert(_parent == null || _parent == parent || parent == null);
        _parent = parent;
    }
    
    public WidgetContainer? GetParent()
    {
        return _parent;
    }
    
    public T? Closest<T>() where T : WidgetBase
    {
        if (this is T t)
        {
            return t;
        }

        return _parent?.Closest<T>();
    }

    /// <summary>
    /// Returns true if this widget is either the given widget, or is one of its descendants.
    /// </summary>
    public bool IsOrIsDescendantOf(WidgetBase widget)
    {
        if (this == widget)
        {
            return true;
        }
        else if (_parent != null)
        {
            return _parent.IsOrIsDescendantOf(widget);
        }
        else
        {
            return false;
        }
    }

    public Rectangle Rectangle
    {
        get => new(GetPos(), GetSize());
        set
        {
            SetPos(value.Location);
            SetSize(value.Size);
        }
    }

    public void SetPos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetPos(Point point) => SetPos(point.X, point.Y);

    public Point GetPos()
    {
        return new Point(X, Y);
    }

    public void SetSize(Size size)
    {
        if (size.Width != Width || size.Height != Height)
        {
            Width = size.Width;
            Height = size.Height;
            OnSizeChanged();
        }
    }

    public Size GetSize()
    {
        return new Size(Width, Height);
    }

    /**
         * A unique id for this widget within the source URI (see below).
         */
    public string GetId()
    {
        return mId;
    }

    public void SetId(string id)
    {
        mId = id;
    }

    /**
         * If this widget was loaded from a file, indicates the URI to that file to more easily identify it.
         */
    public string GetSourceURI()
    {
        return mSourceURI;
    }

    public void SetSourceURI(string sourceUri)
    {
        mSourceURI = sourceUri;
    }

    public void SetCenterHorizontally(bool enable)
    {
        mCenterHorizontally = enable;
    }

    public void SetCenterVertically(bool enable)
    {
        mCenterVertically = enable;
    }

    public void SetSizeToParent(bool enable)
    {
        mSizeToParent = enable;
        if (enable)
        {
            SetAutoSizeHeight(false);
            SetAutoSizeWidth(false);
        }
    }

    /**
         *	Basically gets a Rectangle of x,y,w,h.
         *	Can modify based on parent.
         */
    private static Rectangle GetContentArea(WidgetBase widget)
    {
        var bounds = new Rectangle(widget.GetPos(), widget.GetSize());

        // The content of an advanced widget container may be moved
        int scrollOffsetY = 0;
        if (widget.GetParent() != null)
        {
            var container = widget.GetParent();
            scrollOffsetY = container.GetScrollOffsetY();
        }

        if (widget.GetParent() != null)
        {
            var parentBounds = GetContentArea(widget.GetParent());
            bounds.X += parentBounds.X;
            bounds.Y += parentBounds.Y - scrollOffsetY;

            // Clamp width/height if necessary
            int parentRight = parentBounds.X + parentBounds.Width;
            int parentBottom = parentBounds.Y + parentBounds.Height;
            if (bounds.X >= parentRight)
            {
                bounds.Width = 0;
            }

            if (bounds.Y >= parentBottom)
            {
                bounds.Height = 0;
            }

            if (bounds.X + bounds.Width > parentRight)
            {
                bounds.Width = Math.Max(0, parentRight - bounds.X);
            }

            if (bounds.Y + bounds.Height > parentBottom)
            {
                bounds.Height = Math.Max(0, parentBottom - bounds.Y);
            }
        }

        return bounds;
    }

    /*
     Returns the {x,y,w,h} rect, but regards modification from parent and subtracts the margins.
     Content area controls:
     - Mouse handling active area
     - Rendering area
     */
    public Rectangle GetContentArea(bool includingMargins = false)
    {
        var res = GetContentArea(this);

        // if margins not included, subtract them
        if (!includingMargins)
        {
            if (res.Width != 0 & res.Height != 0)
            {
                res.X += _margins.Left;
                res.Width -= _margins.Left + _margins.Right;
                res.Y += _margins.Top;
                res.Height -= _margins.Bottom + _margins.Top;
                if (res.Width < 0) res.Width = 0;
                if (res.Height < 0) res.Height = 0;
            }
        }


        return res;
    }

    public Rectangle GetVisibleArea()
    {
        if (_parent != null)
        {
            Rectangle parentArea = _parent.GetVisibleArea();
            int parentLeft = parentArea.X;
            int parentTop = parentArea.Y;
            int parentRight = parentLeft + parentArea.Width;
            int parentBottom = parentTop + parentArea.Height;

            int clientLeft = parentArea.X + X;
            int clientTop = parentArea.Y + Y - _parent.GetScrollOffsetY();
            int clientRight = clientLeft + Width;
            int clientBottom = clientTop + Height;

            clientLeft = Math.Max(parentLeft, clientLeft);
            clientTop = Math.Max(parentTop, clientTop);

            clientRight = Math.Min(parentRight, clientRight);
            clientBottom = Math.Min(parentBottom, clientBottom);

            if (clientRight <= clientLeft)
            {
                clientRight = clientLeft;
            }

            if (clientBottom <= clientTop)
            {
                clientBottom = clientTop;
            }

            return new Rectangle(
                clientLeft,
                clientTop,
                clientRight - clientLeft,
                clientBottom - clientTop
            );
        }
        else
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }

    public void SetMouseMsgHandler(Func<MessageMouseArgs, bool> handler)
    {
        mMouseMsgHandler = handler;
    }

    public void SetWidgetMsgHandler(Func<MessageWidgetArgs, bool> handler)
    {
        mWidgetMsgHandler = handler;
    }

    public void SetKeyStateChangeHandler(Func<MessageKeyStateChangeArgs, bool> handler)
    {
        mKeyStateChangeHandler = handler;
    }

    public void SetCharHandler(Func<MessageCharArgs, bool> handler)
    {
        mCharHandler = handler;
    }

    public virtual bool HandleMouseMessage(MessageMouseArgs msg)
    {
        if (mMouseMsgHandler != null)
        {
            return mMouseMsgHandler(msg);
        }

        return false;
    }

    public virtual void OnUpdateTime(TimePoint timeMs)
    {
    }

    protected virtual void OnSizeChanged()
    {
    }

    public void SetAutoSizeWidth(bool enable)
    {
        mAutoSizeWidth = enable;
    }

    public void SetAutoSizeHeight(bool enable)
    {
        mAutoSizeHeight = enable;
    }
    
    /// <summary>
    /// Returns the top-most parent of this widget. That is the parent that has no further
    /// parents.
    /// </summary>
    public WidgetContainer? TopMostParent {
        get
        {
            if (_parent == null)
            {
                return null;
            }

            return _parent._parent == null ? _parent : _parent.TopMostParent;
        } 
    }

    protected WidgetContainer? _parent = null;
    protected string mSourceURI;
    protected string mId;
    protected bool mCenterHorizontally = false;
    protected bool mCenterVertically = false;
    protected bool mSizeToParent = false;
    protected bool mAutoSizeWidth = true;
    protected bool mAutoSizeHeight = true;
    protected Margins _margins;
    protected Func<MessageMouseArgs, bool> mMouseMsgHandler;
    protected Func<MessageWidgetArgs, bool> mWidgetMsgHandler;
    protected Func<MessageKeyStateChangeArgs, bool> mKeyStateChangeHandler;
    protected Func<MessageCharArgs, bool> mCharHandler;

    protected List<WidgetContent> _content = new();
    private bool _visible = true;

    public virtual void RenderTooltip(int x, int y)
    {
    }

    protected virtual void InvokeOnBeforeRender()
    {
        OnBeforeRender?.Invoke();
    }

    public override IStyleable? StyleParent => _parent;
    
    public virtual bool HasPseudoClass(StylingState stylingState) => false;

    protected override void OnStylesInvalidated()
    {
        base.OnStylesInvalidated();

        foreach (var content in _content)
        {
            content.InvalidateStyles();
        }
    }

    public event Action? OnMouseCaptureLost;

    public void NotifyMouseCaptureLost()
    {
        OnMouseCaptureLost?.Invoke();
    }
};