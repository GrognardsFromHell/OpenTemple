using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Styles;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.Ui.Widgets;

[DebuggerDisplay("{ToString()}")]
public partial class WidgetBase : Styleable, IDisposable
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private WidgetContainer? _parent;

    /// <summary>
    /// If this widget was loaded from a file, indicates the URI to that file to more easily identify it.
    /// </summary>
    public string? SourceURI { get; set; }

    /// <summary>
    /// A unique id for this widget within the source URI (see below).
    /// </summary>
    public string? Id { get; set; }

    public bool CenterHorizontally { get; set; }
    public bool CenterVertically { get; set; }
    protected bool _sizeToParent;
    public bool AutoSizeWidth { get; set; } = true;
    public bool AutoSizeHeight { get; set; } = true;
    protected readonly List<WidgetContent> Content = new();
    private readonly List<ActionHotkeyRegistration> _actionHotkeys = new();
    private readonly Dictionary<Hotkey, HeldHotkeyState> _heldHotkeys = new();
    public IReadOnlyList<ActionHotkeyRegistration> ActionHotkeys => _actionHotkeys;
    public IReadOnlyCollection<HeldHotkeyState> HeldHotkeys => _heldHotkeys.Values;
    private bool _visible = true;
    private bool _containsMouse;
    private bool _pressed;
    private bool _disabled;
    private FocusMode _focusMode = FocusMode.None;

    /// <summary>
    /// Controls whether this widget can receive keyboard focus.
    /// </summary>
    public FocusMode FocusMode
    {
        get => _focusMode;
        set
        {
            if (_focusMode != value)
            {
                _focusMode = value;
                // Ensure we relinquish focus if focusing is disabled
                if (value == FocusMode.None)
                {
                    Blur();
                }
            }
        }
    }

    /// <summary>
    /// True if this widget currently has keyboard focus.
    /// </summary>
    public bool HasFocus => UiManager?.KeyboardFocus == this;

    /// <summary>
    /// If this widget is currently focused, release the keyboard focus.
    /// </summary>
    private void Blur()
    {
        if (HasFocus)
        {
            Stub.TODO();
        }
    }

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
                UiManager?.VisibilityChanged(this);
            }
        }
    }

    /// <summary>
    /// Checks if a widget is really on screen by checking all of it's parents as well for visibility.
    /// </summary>
    public bool IsVisibleIncludingParents
    {
        get
        {
            if (!IsInTree)
            {
                return false; // can't be visible when we're not in the UI tree
            }

            var c = this;
            while (c != null)
            {
                if (!c.Visible)
                {
                    return false;
                }

                c = c.Parent;
            }

            return true;
        }
    }

    /// <summary>
    /// Content is shifted by this offset within the viewport of the widget.
    /// </summary>
    protected Point ContentOffset { get; set; }

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

    /// <summary>
    /// Hit test the widget at the given x and y position relative to its border area.
    /// </summary>
    public virtual bool HitTest(float x, float y)
    {
        if (HitTesting == HitTestingMode.Ignore)
        {
            return false;
        }

        var contentArea = BorderArea;
        x += contentArea.X;
        y += contentArea.Y;

        if (HitTesting == HitTestingMode.Area)
        {
            return BorderArea.Contains(x, y);
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

        // Draw background
        RenderDecorations(ComputedStyles);

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

    /// <summary>
    /// Renders background and borders under the widget content or child widgets.
    /// </summary>
    protected virtual void RenderDecorations(ComputedStyles style)
    {
        var area = BorderArea;

        if (style.BackgroundColor.A > 0)
        {
            Tig.ShapeRenderer2d.DrawRectangle(
                area,
                null,
                style.BackgroundColor
            );
        }
    }

    /// <summary>
    /// As per CSS Box Model the margin area includes margins, border and padding.
    /// </summary>
    public RectangleF MarginArea
    {
        get
        {
            var area = GetContentArea(true);
            return new RectangleF(
                area.X,
                area.Y,
                area.Width,
                area.Height
            );
        }
    }

    /// <summary>
    /// As per the CSS Box Model the border area includes border, padding and content.
    /// </summary>
    public RectangleF BorderArea
    {
        get
        {
            var area = MarginArea;
            var style = ComputedStyles;
            return new RectangleF(
                area.X + style.MarginLeft,
                area.Y + style.MarginTop,
                Math.Max(0, area.Width - style.MarginLeft - style.MarginRight),
                Math.Max(0, area.Height - style.MarginTop - style.MarginBottom)
            );
        }
    }

    /// <summary>
    /// As per the CSS Box Model the padding area includes padding and content.
    /// </summary>
    public RectangleF PaddingArea
    {
        get
        {
            var area = BorderArea;
            var style = ComputedStyles;
            return new RectangleF(
                area.X + style.BorderWidth,
                area.Y + style.BorderWidth,
                Math.Max(0, area.Width - style.BorderWidth - style.BorderWidth),
                Math.Max(0, area.Height - style.BorderWidth - style.BorderWidth)
            );
        }
    }

    /// <summary>
    /// As per CSS Box Model the content area does not include padding, border or margin.
    /// </summary>
    public RectangleF ContentArea
    {
        get
        {
            var area = PaddingArea;
            var style = ComputedStyles;
            return new RectangleF(
                area.X + style.PaddingLeft,
                area.Y + style.PaddingTop,
                Math.Max(0, area.Width - style.PaddingLeft - style.PaddingRight),
                Math.Max(0, area.Height - style.PaddingTop - style.PaddingBottom)
            );
        }
    }

    protected internal virtual void UpdateLayout()
    {
        ApplyAutomaticSizing();

        var contentArea = GetContentArea();

        // Size to content
        if (contentArea is {Width: 0, Height: 0})
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
                var style = ComputedStyles;
                Width = LegacyRound(contentArea.Width + style.MarginLeft + style.MarginRight + 2 * style.BorderWidth + style.PaddingLeft + style.PaddingRight);
                Height = LegacyRound(contentArea.Height + style.MarginTop + style.MarginBottom + 2 * style.BorderWidth + style.PaddingTop + style.PaddingBottom);

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

    // This function only exists to make it easier to track places that haven't been converted to use float for coordinates yet
    [Obsolete]
    protected internal static int LegacyRound(float v)
    {
        return (int) v;
    }

    protected virtual void ApplyAutomaticSizing()
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

    /// <summary>
    /// Same as <see cref="PickWidget"/>, but the x and y coordinates
    /// are global UI coordinates,and not local to this widget. 
    /// </summary>
    public WidgetBase? PickWidgetGlobal(float x, float y)
    {
        var contentBounds = BorderArea;
        if (x < contentBounds.X || x >= contentBounds.Right || y < contentBounds.Y || y >= contentBounds.Bottom)
        {
            return null;
        }

        return PickWidget(x - contentBounds.X, y - contentBounds.Y);
    }

    /// <summary>
    /// Picks the widget a the x,y coordinate local to this widgets content area.
    /// Null if the coordinates are outside of this widget. If no
    /// other widget inside is at the given coordinate, will just return this.
    /// </summary>
    public virtual WidgetBase? PickWidget(float x, float y)
    {
        if (!Visible)
        {
            return null;
        }

        return HitTest(x, y) ? this : null;
    }

    // Coordinates are absolute
    public WidgetContent? PickContent(float x, float y)
    {
        UpdateLayout();

        foreach (var content in Content)
        {
            var bounds = content.GetBounds();
            if (x >= bounds.Left && y >= bounds.Top && x < bounds.Right && y < bounds.Bottom)
            {
                return content;
            }
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

    public void BringToFront()
    {
        _parent?.BringToFront(this);
    }

    public void MoveToBack()
    {
        _parent?.MoveToBack(this);
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
        get => new(X, Y);
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
            AutoSizeHeight = false;
            AutoSizeWidth = false;
        }
    }

    /// <summary>
    /// Get the border area of the widget, accounting for its position in the parent.
    /// </summary>
    private static Rectangle GetBorderArea(WidgetBase widget)
    {
        // We use the border-sizing model, which means the widgets outer size includes border+padding
        var bounds = new RectangleF(widget.Pos, widget.Size);
        
        // The content of an advanced widget container may be moved
        var parent = widget.Parent;
        if (parent != null)
        {
            // Children are positioned relative to their parent content area
            var parentContentArea = parent.ContentArea;

            var scrollOffsetY = parent.GetScrollOffsetY();

            bounds.X += parentContentArea.X;
            bounds.Y += parentContentArea.Y - scrollOffsetY;

            // Clamp width/height if necessary
            var parentRight = parentContentArea.Right;
            var parentBottom = parentContentArea.Bottom;
            
            if (bounds.Right > parentRight)
            {
                bounds.Width = Math.Max(0, parentRight - bounds.X);
            }

            if (bounds.Bottom > parentBottom)
            {
                bounds.Height = Math.Max(0, parentBottom - bounds.Y);
            }
        }

        return new Rectangle(
            LegacyRound(bounds.X),
            LegacyRound(bounds.Y),
            LegacyRound(bounds.Width),
            LegacyRound(bounds.Height)
        );
    }

    /// <summary>
    /// Returns the {x,y,w,h} rect, but regards modification from parent and subtracts the margins.
    /// Content area controls:
    /// - Mouse handling active area
    /// - Rendering area
    /// </summary>
    public Rectangle GetContentArea(bool includingMargins = false)
    {
        return GetBorderArea(this);
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

    protected virtual void InvokeOnBeforeRender()
    {
        OnBeforeRender?.Invoke();
    }

    public override IStyleable? StyleParent => _parent;

    public bool HasPseudoClass(StylingState stylingState)
    {
        return stylingState switch
        {
            StylingState.Hover => ContainsMouse,
            StylingState.Pressed => Pressed,
            StylingState.Disabled => Disabled,
            _ => false
        };
    }

    public override StylingState PseudoClassState
    {
        get
        {
            StylingState result = default;
            if (ContainsMouse)
            {
                result |= StylingState.Hover;
            }

            if (Pressed)
            {
                result |= StylingState.Pressed;
            }

            if (Disabled)
            {
                result |= StylingState.Disabled;
            }

            return result;
        }
    }

    protected override void OnStylesInvalidated()
    {
        base.OnStylesInvalidated();

        foreach (var content in Content)
        {
            content.InvalidateStyles();
        }
    }

    /// <summary>
    /// Associates a hotkey with an action to be triggered when the hotkey is triggered.
    /// An additional condition may be specified that needs to be true in order for the action to be triggered.
    /// </summary>
    public void AddActionHotkey(Hotkey hotkey, Action callback, Func<bool>? condition = null)
    {
        if (hotkey.Trigger == HotkeyTrigger.Held)
        {
            throw new ArgumentException("Cannot register a hotkey that is triggered by holding as an action hotkey.");
        }

        condition ??= () => true;
        _actionHotkeys.Add(new ActionHotkeyRegistration(hotkey, callback, condition));
    }

    /// <summary>
    /// Declares that this widget reacts to a hotkey being held. The UI system ensures that the hotkey is reported as not being
    /// held, if the user input is redirect to another widget.
    /// An additional condition may be specified that needs to be true in order for the button to be held.
    /// </summary>
    /// <param name="callback">Called when the hold-state of the hotkey changes.</param>
    /// <param name="condition">If given, the hotkey will only be reported as being held, when this function returns true. The state change function will NOT be called when this function changes its return value.</param>
    public void AddHeldHotkey(Hotkey hotkey, Action<bool>? callback = null, Func<bool>? condition = null)
    {
        if (hotkey.Trigger != HotkeyTrigger.Held)
        {
            throw new ArgumentException("Cannot register an action hotkey as a held hotkey. Use AddActionHotkey instead. Trigger type was: " + hotkey.Trigger);
        }

        callback ??= _ => { };
        condition ??= () => true;
        if (!_heldHotkeys.TryAdd(hotkey, new HeldHotkeyState(hotkey, callback, condition)))
        {
            throw new ArgumentException("Held hotkey " + hotkey + " is already registered");
        }
    }

    /// <summary>
    /// Checks if a hotkey previously registered on this widget using <see cref="AddHeldHotkey"/> is currently held given the rules
    /// outlined in that method. 
    /// </summary>
    public bool IsHeldHotkeyPressed(Hotkey hotkey)
    {
        if (!_heldHotkeys.TryGetValue(hotkey, out var state))
        {
            throw new ArgumentException("Cannot get the hold-state of a hotkey that was not registered with AddHeldHotkey first: " + hotkey);
        }

        return state.Held && state.Condition();
    }

    public record ActionHotkeyRegistration(Hotkey Hotkey, Action Callback, Func<bool> Condition);

    /// <summary>
    /// Registration for a hotkey that causes the widget to be notified when the hotkey is being held.
    /// </summary>
    public record HeldHotkeyState(Hotkey Hotkey, Action<bool> Callback, Func<bool> Condition)
    {
        public bool Held { get; set; }
    }

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
    public DeclaredInterval AddInterval(Action callback, TimeSpan interval = default)
    {
        var declaredInterval = new DeclaredInterval(callback, interval);
        Intervals = Intervals.Add(declaredInterval);
        return declaredInterval;
    }

    public void StopInterval(DeclaredInterval interval)
    {
        Intervals = Intervals.Remove(interval);
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
            UiManager.OnRemovedFromTree(this);
        }

        UiManager = manager;

        UiManager?.OnAddedToTree(this);
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

    public void CenterInParent()
    {
        if (Parent != null)
        {
            var parentSize = Parent.Size;
            X = (parentSize.Width - Width) / 2;
            Y = (parentSize.Height - Height) / 2;
        }
    }

    public bool HasMouseCapture => UiManager?.MouseCaptureWidget == this;

    #region Tree Navigation

    public virtual WidgetBase? FirstChild => null;

    public virtual WidgetBase? LastChild => null;

    public WidgetBase? PreviousSibling
    {
        get
        {
            var siblings = (IReadOnlyList<WidgetBase>?) Parent?.Children;
            if (siblings == null)
            {
                return null;
            }

            // Search for us in the list of children of our parent and return the previous element
            for (var i = 0; i < siblings.Count; i++)
            {
                if (siblings[i] == this)
                {
                    return i > 0 ? siblings[i - 1] : null;
                }
            }

            throw new InvalidOperationException("Could not find this widget among the children of its parent.");
        }
    }

    public WidgetBase? NextSibling
    {
        get
        {
            var siblings = (IReadOnlyList<WidgetBase>?) Parent?.Children;
            if (siblings == null)
            {
                return null;
            }

            // Search for us in the list of children of our parent and return the next element
            for (var i = 0; i < siblings.Count; i++)
            {
                if (siblings[i] == this)
                {
                    return i + 1 < siblings.Count ? siblings[i + 1] : null;
                }
            }

            throw new InvalidOperationException("Could not find this widget among the children of its parent.");
        }
    }

    /// <summary>
    /// Find the preceding object (A) of the given object (B).
    /// An object A is preceding an object B if A and B are in the same tree
    ///     and A comes before B in tree order.
    /// * `O(n)` (worst case)
    ///     * `O(1)` (amortized when walking the entire tree)
    /// </summary>
    /// <param name="root">If set, `root` must be an inclusive ancestor
    ///      of the return value (or else null is returned). This check _assumes_
    ///        that `root` is also an inclusive ancestor of the given `object`</param>
    public WidgetBase? Preceding(WidgetBase? root = null)
    {
        if (this == root)
        {
            return null;
        }

        if (PreviousSibling != null)
        {
            return PreviousSibling.LastInclusiveDescendant();
        }

        // if there is no previous sibling return the parent (might be null)
        return Parent;
    }

    /// <summary>
    /// Find the following object (A) of the given object (B).
    /// An object A is following an object B if A and B are in the same tree
    /// and A comes after B in tree order.
    /// 
    /// `O(n)` (worst case) where `n` is the amount of objects in the entire tree
    /// `O(1)` (amortized when walking the entire tree)
    /// </summary>
    /// <param name="treeRoot">If not null, iteration will stop and return null, when this element is reached.</param>
    /// <param name="skipChildren">If true, iteration will skip past children of this node.</param>
    /// <param name="predicate">If not null, skip nodes that do not satisfy this predicate.</param>
    internal WidgetBase? Following(WidgetBase? treeRoot = null, bool skipChildren = false, Predicate<WidgetBase>? predicate = null)
    {
        if (!skipChildren && FirstChild != null && (predicate == null || predicate(FirstChild)))
        {
            return FirstChild;
        }

        var current = this;
        do
        {
            if (current == treeRoot)
            {
                return null;
            }

            for (var nextSibling = current.NextSibling; nextSibling != null; nextSibling = nextSibling.NextSibling)
            {
                if (predicate == null || predicate(nextSibling))
                {
                    return nextSibling;
                }
            }

            current = current.Parent;
        } while (current != null);

        return null;
    }

    /// <summary>
    /// Find the inclusive descendant that is last in tree order of the given object.
    /// `O(n)` (worst case) where `n` is the depth of the subtree of `object`
    /// </summary>
    public WidgetBase LastInclusiveDescendant()
    {
        var current = this;

        while (current.LastChild is { } lastChild)
        {
            current = lastChild;
        }

        return current;
    }

    #endregion

    public override string ToString()
    {
        var result = new StringBuilder();
        if (SourceURI != null)
        {
            result.Append(SourceURI);
            if (Id != null)
            {
                result.Append('#').Append(Id);
            }
        }
        else if (Id != null)
        {
            result.Append(Id);
        }
        else
        {
            return GetType().Name;
        }

        result.Append(" (").Append(GetType().Name).Append(')');
        return result.ToString();
    }

    public void Focus()
    {
        if (FocusMode == FocusMode.None || HasFocus || UiManager == null)
        {
            return;
        }

        UiManager.Focus(this);
    }
}