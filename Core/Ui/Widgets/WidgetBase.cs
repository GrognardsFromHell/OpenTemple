#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Styles;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.Ui.Widgets;

public partial class WidgetBase : Styleable, IDisposable
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);
    
    private WidgetContainer? _parent;
    /// <summary>
    /// If this widget was loaded from a file, indicates the URI to that file to more easily identify it.
    /// </summary>
    public string SourceURI { get; set; }
    /// <summary>
    /// A unique id for this widget within the source URI (see below).
    /// </summary>
    public string Id { get; set; }
    public bool CenterHorizontally { get; set; }
    public bool CenterVertically { get; set; }
    protected bool _sizeToParent = false;
    protected bool _autoSizeWidth = true;
    protected bool _autoSizeHeight = true;
    protected Margins _margins;
    protected Func<MessageKeyStateChangeArgs, bool>? mKeyStateChangeHandler;
    protected Func<MessageCharArgs, bool>? mCharHandler;
    protected readonly List<WidgetContent> Content = new();
    private readonly List<AvailableHotkey> _hotkeys = new();
    private bool _visible = true;
    private bool _containsMouse;
    private bool _pressed;
    private bool _disabled;
    
    /// <summary>
    /// Is the mouse currently over this widget?
    /// </summary>
    public bool ContainsMouse
    {
        get => _containsMouse;
        set
        {
            if (value != _containsMouse)
            {
                _containsMouse = value;
                InvalidateStyles(); // Pseudo-class has changed
            }
        }
    }

    /// <summary>
    /// Indicates that the primary mouse button has been pressed on this element
    /// or one of its descendants, and has not been released yet.
    /// </summary>
    public bool Pressed
    {
        get => _pressed;
        set
        {
            if (value != _pressed)
            {
                _pressed = value;
                InvalidateStyles(); // Pseudo-class has changed
            }
        }
    }

    /// <summary>
    /// Convenience property that is true if the widget is both pressed and contains the mouse.
    /// </summary>
    public bool ContainsPress => ContainsMouse && Pressed;
    
    /// <summary>
    /// Disables the interactivity of this element.
    /// </summary>
    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (value != _disabled)
            {
                _disabled = value;
                InvalidateStyles(); // Pseudo-class has changed
            }
        }
    }

    public int ZIndex { get; set; }

    /// <summary>
    /// Indicates whether this widget is currently part of the widget tree.
    /// </summary>
    public bool IsInTree => UiManager != null;

    /// <summary>
    /// Gets the UI Manager that this widget is attached to.
    /// </summary>
    public UiManager? UiManager { get; private set; }

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
                UiManager?.RefreshMouseOverState();
            }
        }
    }

    /// <summary>
    /// Content is shifted by this offset within the viewport of the widget.
    /// </summary>
    protected Point ContentOffset { get; set; }

    public Margins Margins
    {
        get => _margins;
        set => _margins = value;
    }

    public WidgetBase([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1)
    {
        if (filePath != null)
        {
            SourceURI = $"{Path.GetFileName(filePath)}:{lineNumber}";
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

    /// <summary>
    /// Hit test the content of this widget instead of just checking against the content rectangle.
    /// </summary>
    public HitTestingMode HitTesting { get; set; }

    public virtual bool HitTest(float x, float y)
    {
        if (HitTesting == HitTestingMode.Ignore)
        {
            return false;
        }
        
        var contentArea = GetContentArea();
        x += contentArea.X - _margins.Left;
        y += contentArea.Y - _margins.Top;

        if (HitTesting == HitTestingMode.ContentArea)
        {
            return contentArea.Contains((int) x, (int) y);
        }

        UpdateLayout();

        foreach (var content in Content)
        {
            if (!content.Visible)
            {
                continue;
            }

            var contentRect = content.GetBounds();
            contentRect.Intersect(contentArea);

            if (contentRect.Contains((int) x, (int) y))
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

        foreach (var content in Content)
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
            foreach (var content in Content)
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

        foreach (var content in Content)
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
        if (_sizeToParent)
        {
            var containerWidth = _parent?.Width ?? UiManager?.CanvasSize.Width ?? 1;
            var containerHeight = _parent?.Height ?? UiManager?.CanvasSize.Height ?? 1;
            Size = new Size(containerWidth, containerHeight);
        }

        if (CenterHorizontally)
        {
            var containerWidth = _parent?.Width ?? UiManager?.CanvasSize.Width ?? 0;
            var x = (containerWidth - Width) / 2;
            if (x != X)
            {
                X = x;
            }
        }

        if (CenterVertically)
        {
            var containerHeight = _parent?.Height ?? UiManager?.CanvasSize.Height ?? 0;
            var y = (containerHeight - Height) / 2;
            if (y != Y)
            {
                Y = y;
            }
        }
    }

    public virtual bool HandleMessage(Message msg)
    {
        if (msg.type == MessageType.KEYSTATECHANGE && mKeyStateChangeHandler != null)
        {
            return mKeyStateChangeHandler(msg.KeyStateChangeArgs);
        }
        else if (msg.type == MessageType.CHAR && mCharHandler != null)
        {
            return mCharHandler(msg.CharArgs);
        }

        return false;
    }

    /// <summary>
    /// Picks the widget a the x,y coordinate local to this widget.
    /// Null if the coordinates are outside of this widget. If no
    /// other widget inside is at the given coordinate, will just return this.
    /// </summary>
    public virtual WidgetBase? PickWidget(float x, float y)
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
        Content.Add(content);
    }

    public void RemoveContent(WidgetContent content)
    {
        if (Content.Remove(content))
        {
            content.Parent = null;
        }
    }

    public void ClearContent()
    {
        foreach (var content in Content)
        {
            content.Parent = null;
        }

        Content.Clear();
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

    public WidgetContainer? Parent
    {
        get => _parent;
        set
        {
            Trace.Assert(_parent == null || _parent == value || value == null);
            _parent = value;
        }
    }

    public Rectangle Rectangle
    {
        get => new(Pos, Size);
        set
        {
            Pos = value.Location;
            Size = value.Size;
        }
    }

    public void SetPos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point Pos
    {
        get => new (X, Y);
        set => SetPos(value.X, value.Y);
    }

    public Size Size
    {
        get => new(Width, Height);
        set
        {
            if (value.Width != Width || value.Height != Height)
            {
                Width = value.Width;
                Height = value.Height;
                OnSizeChanged();
            }
        }
    }

    public void SetSizeToParent(bool enable)
    {
        _sizeToParent = enable;
        if (enable)
        {
            SetAutoSizeHeight(false);
            SetAutoSizeWidth(false);
        }
    }

    /// <summary>
    /// Basically gets a Rectangle of x,y,w,h.
    /// Can modify based on parent.
    /// </summary>
    private static Rectangle GetContentArea(WidgetBase widget)
    {
        var bounds = new Rectangle(widget.Pos, widget.Size);

        // The content of an advanced widget container may be moved
        var container = widget.Parent;
        if (container != null)
        {
            var scrollOffsetY = container.GetScrollOffsetY();

            var parentBounds = GetContentArea(container);
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

    /// <summary>
    /// Returns the {x,y,w,h} rect, but regards modification from parent and subtracts the margins.
    /// Content area controls:
    /// - Mouse handling active area
    /// - Rendering area
    /// </summary>
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

    public event Action<HotkeyActionMessage>? OnHotkeyAction;

    [Obsolete]
    public void SetKeyStateChangeHandler(Func<MessageKeyStateChangeArgs, bool> handler)
    {
        mKeyStateChangeHandler = handler;
    }

    [Obsolete]
    public void SetCharHandler(Func<MessageCharArgs, bool> handler)
    {
        mCharHandler = handler;
    }

    public virtual void HandleHotkeyAction(HotkeyActionMessage msg)
    {
        OnHotkeyAction?.Invoke(msg);

        if (!msg.IsHandled)
        {
            foreach (var (hotkey, callback, condition) in _hotkeys)
            {
                if (condition() && hotkey == msg.Hotkey)
                {
                    callback();
                    msg.SetHandled();
                }
            }
        }
    }

    [Obsolete]
    public virtual bool HandleMouseMessage(MessageMouseArgs msg)
    {
        return false;
    }

    public virtual void OnUpdateTime(TimePoint now)
    {
        foreach (var interval in Intervals)
        {
            if (interval.NextTrigger <= now)
            {
                interval.Trigger();
            } 
        }
    }

    protected virtual void OnSizeChanged()
    {
    }

    public void SetAutoSizeWidth(bool enable)
    {
        _autoSizeWidth = enable;
    }

    public void SetAutoSizeHeight(bool enable)
    {
        _autoSizeHeight = enable;
    }

    public virtual void RenderTooltip(int x, int y)
    {
    }

    protected virtual void InvokeOnBeforeRender()
    {
        OnBeforeRender?.Invoke();
    }

    public override IStyleable? StyleParent => _parent;

    public virtual bool HasPseudoClass(StylingState stylingState)
    {
        return stylingState switch
        {
            StylingState.Hover => ContainsMouse,
            StylingState.Pressed => Pressed,
            StylingState.Disabled => Disabled,
            _ => false
        };
    }

    protected override void OnStylesInvalidated()
    {
        base.OnStylesInvalidated();

        foreach (var content in Content)
        {
            content.InvalidateStyles();
        }
    }

    public void AddHotkey(Hotkey hotkey, Action callback, Func<bool>? condition = null)
    {
        condition ??= () => true;
        _hotkeys.Add(new AvailableHotkey(hotkey, callback, condition));
    }

    private record AvailableHotkey(Hotkey Hotkey, Action Callback, Func<bool> Condition);

    public IEnumerable<WidgetBase> EnumerateSelfAndAncestors()
    {
        var current = this;

        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public ImmutableList<DeclaredInterval> Intervals { get; private set; } = ImmutableList<DeclaredInterval>.Empty; 
    
    /// <summary>
    /// Adds a callback that will be called in regular intervals as long as this widget is part of the UI tree. 
    /// </summary>
    public void AddInterval(Action callback, TimeSpan interval = default)
    {
        Intervals = Intervals.Add(new DeclaredInterval(callback, interval));
    }

    public record DeclaredInterval(Action Callback, TimeSpan Interval)
    {
        public TimePoint NextTrigger { get; set; } = GetNextIntervalTrigger(Interval);

        public void Trigger()
        {
            NextTrigger = GetNextIntervalTrigger(Interval); 
            Callback();
        }
        
        private static TimePoint GetNextIntervalTrigger(TimeSpan interval)
        {
            if (interval == default)
            {
                interval = DefaultInterval;
            }
            return TimePoint.Now + interval;
        }
        
    }

    public virtual void AttachToTree(UiManager? manager)
    {
        if (UiManager != null && UiManager != manager)
        {
            UiManager.RemoveWidget(this);
        }
        UiManager = manager;
    }

    public void DetachFromTree() => AttachToTree(null);

    public bool SetMouseCapture()
    {
        return UiManager?.CaptureMouse(this) ?? false;
    }

    public void ReleaseMouseCapture()
    {
        UiManager?.ReleaseMouseCapture(this);
    }

    public void CenterOnScreen()
    {
        Trace.Assert(Parent == null);
        if (UiManager != null)
        {
            var screenSize = UiManager.CanvasSize;
            X = (screenSize.Width - Width) / 2;
            Y = (screenSize.Height - Height) / 2;
        }
    }    

    public bool HasMouseCapture => UiManager?.MouseCaptureWidget == this;
}
