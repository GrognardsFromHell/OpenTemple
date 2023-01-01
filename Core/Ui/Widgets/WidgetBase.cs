using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.Widgets
{
    [DebuggerDisplay("{ToString()}")]
    public partial class WidgetBase : Styleable, IDisposable
    {
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

        private WidgetContainer? _parent;

        private readonly Anchors _anchors;

        public Anchors Anchors => _anchors;

        /// <summary>
        /// If this widget was loaded from a file, indicates the URI to that file to more easily identify it.
        /// </summary>
        public string? SourceURI { get; set; }

        /// <summary>
        /// A unique id for this widget within the source URI (see below).
        /// </summary>
        public string? Id { get; set; }


        private readonly List<WidgetContent> _content = new();
        public IReadOnlyList<WidgetContent> Content => _content;

        private readonly List<HotkeyAction> _actionHotkeys = new();
        private readonly Dictionary<Hotkey, HeldHotkeyState> _heldHotkeys = new();
        public IReadOnlyList<HotkeyAction> ActionHotkeys => _actionHotkeys;
        public IReadOnlyCollection<HeldHotkeyState> HeldHotkeys => _heldHotkeys.Values;
        private bool _visible = true;
        private bool _containsMouse;
        private bool _pressed;
        private bool _disabled;
        private FocusMode _focusMode = FocusMode.None;

        /// <summary>
        /// Indicates that <see cref="LayoutBox"/> has been set by the layout engine and is valid.
        /// </summary>
        public bool HasValidLayout { get; private set; } = true;

        /// <summary>
        /// This is equivalent to the border box. Accessing it will automatically update the layout of the entire widget tree.
        /// It can also be accessed while layout is in progress, but only if it has already been set.
        /// </summary>
        public RectangleF LayoutBox
        {
            get
            {
                if (UiManager is {LayoutInProgress: true})
                {
                    if (!HasValidLayout)
                    {
                        throw new ArgumentException($"Accessing layout box of {this} before it has been updated.");
                    }
                }
                else
                {
                    EnsureLayoutIsUpToDate();
                    Debug.Assert(HasValidLayout);
                }

                return _layoutBox;
            }
        }

        private RectangleF _layoutBox;

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
        public float X
        {
            get => _x;
            set
            {
                if (Math.Abs(_x - value) > 0.1f)
                {
                    _x = value;
                    NotifyLayoutChange(LayoutChangeFlag.OwnPosition);
                }
            }
        }

        // Vertical position relative to parent
        public float Y
        {
            get => _y;
            set
            {
                if (Math.Abs(_y - value) > 0.1f)
                {
                    _y = value;
                    NotifyLayoutChange(LayoutChangeFlag.OwnPosition);
                }
            }
        }

        private Dimension _width;

        public Dimension Width
        {
            get => _width;
            set
            {
                if (value.Value < 0 || !float.IsFinite(value.Value))
                {
                    throw new ArgumentException("Must be positive/finite: " + value);
                }

                if (_width != value)
                {
                    _width = value;
                    NotifyLayoutChange(LayoutChangeFlag.OwnSize);
                }
            }
        }

        private Dimension _height;
        private float _x;
        private float _y;

        public Dimension Height
        {
            get => _height;
            set
            {
                if (value.Value < 0 || !float.IsFinite(value.Value))
                {
                    throw new ArgumentException("Must be positive/finite: " + value);
                }

                if (_height != value)
                {
                    _height = value;
                    NotifyLayoutChange(LayoutChangeFlag.OwnSize);
                }
            }
        }

        /// <summary>
        /// Convenient size to set <see cref="Width"/> and <see cref="Height"/> in UI pixels.
        /// </summary>
        public SizeF PixelSize
        {
            set
            {
                var width = Dimension.Pixels(value.Width);
                var height = Dimension.Pixels(value.Height);
                if (Width != width || Height != height)
                {
                    Width = width;
                    Height = height;
                }
            }
        }

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

            _anchors = new Anchors(this);
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

            EnsureLayoutIsUpToDate();

            if (HitTesting == HitTestingMode.Area)
            {
                return x >= 0 && y >= 0 && x < BorderArea.Width && y < BorderArea.Height;
            }

            foreach (var content in Content)
            {
                if (!content.Visible)
                {
                    continue;
                }

                var contentRect = content.GetBounds();
                // TODO: Should be clipped

                if (contentRect.Contains(x, y))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void Render(UiRenderContext context)
        {
            if (!Visible)
            {
                return;
            }

            OnBeforeRender?.Invoke();

            // Draw background
            RenderDecorations(ComputedStyles);

            var contentOrigin = GetViewportPaddingArea(true).Location;
            contentOrigin.X -= ContentOffset.X;
            contentOrigin.Y -= ContentOffset.Y;

            foreach (var content in Content)
            {
                if (!content.Visible)
                {
                    continue;
                }

                var viewportContentBounds = content.GetBounds();
                viewportContentBounds.Offset(contentOrigin);
                if (viewportContentBounds.IntersectsWith(GetViewportPaddingArea(true)))
                {
                    content.Render(contentOrigin);
                }
            }
        }

        /// <summary>
        /// Renders background and borders under the widget content or child widgets.
        /// </summary>
        protected virtual void RenderDecorations(ComputedStyles style)
        {
            var area = GetViewportBorderArea();

            if (style.BackgroundColor.A > 0)
            {
                Tig.ShapeRenderer2d.DrawRectangle(
                    area,
                    null,
                    style.BackgroundColor
                );
            }

            if (style.BorderWidth > 0 && style.BorderColor.A > 0)
            {
                Tig.ShapeRenderer2d.DrawRectangleOutline(
                    area,
                    style.BorderColor
                );
            }
        }

        /// <summary>
        /// As per CSS Box Model the margin area includes margins, border and padding.
        /// </summary>
        public RectangleF MarginArea =>
            new(
                BorderArea.X - ComputedStyles.MarginLeft,
                BorderArea.Y - ComputedStyles.MarginTop,
                BorderArea.Width + ComputedStyles.MarginLeft + ComputedStyles.MarginRight,
                BorderArea.Height + ComputedStyles.MarginTop + ComputedStyles.MarginBottom
            );

        /// <summary>
        /// As per the CSS Box Model the border area includes border, padding and content.
        /// </summary>
        public RectangleF BorderArea => LayoutBox;

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

        public void EnsureLayoutIsUpToDate()
        {
            // We cannot be laid out if we're not attached to the UI
            UiManager?.EnsureLayoutUpdated(this);
        }

        protected internal void UpdateLayout()
        {
            // Assumes that our own layout rect is valid
            if (!HasValidLayout)
            {
                throw new InvalidOperationException("Cannot run layout if the layout box is invalid");
            }

            // Layout cannot be run if not attached to the tree
            if (UiManager == null)
            {
                throw new InvalidOperationException("Cannot run layout without being attached to the tree");
            }

            // Clear validity of layout rects first
            foreach (var child in Children)
            {
                child.ClearLayout();
            }

            LayoutChildren();

            // Give children a chance to update layout of their own children
            foreach (var child in Children)
            {
                child.UpdateLayout();
            }

            OnAfterLayout();
        }

        protected virtual void LayoutChildren()
        {
            Debug.Assert(UiManager != null);

            var availableWidth = _layoutBox.Width;
            var availableHeight = _layoutBox.Height;

            // We use pooled storage for this set - only if its needed
            (WidgetBase? Child, RectangleF LayoutBox)[]? anchorPassOpenSet = null;
            var anchorPassOpenSetCount = 0;

            try
            {
                foreach (var child in Children)
                {
                    // TODO: Would need to lay out X/Y first and subtract (?)
                    var preferredSize = child.ComputePreferredBorderAreaSize(availableWidth, availableHeight);

                    var childX = child.X;
                    var childY = child.Y;
                    var childWidth = child.Width.Evaluate(availableWidth, UiManager.DevicePixelsPerUiPixel, preferredSize.Width);
                    var childHeight = child.Height.Evaluate(availableHeight, UiManager.DevicePixelsPerUiPixel, preferredSize.Height);
                    var childLayoutBox = new RectangleF(
                        childX,
                        childY,
                        childWidth,
                        childHeight
                    );

                    if (!child.Anchors.Apply(ref childLayoutBox))
                    {
                        // Anchors could not be applied because they rely on another sibling that has not been laid out yet
                        if (anchorPassOpenSet == null)
                        {
                            anchorPassOpenSet = ArrayPool<(WidgetBase, RectangleF)>.Shared.Rent(Children.Count);
                            anchorPassOpenSetCount = 0;
                        }

                        anchorPassOpenSet[anchorPassOpenSetCount++] = (child, childLayoutBox);
                        continue;
                    }

                    child.SetLayout(childLayoutBox);
                }

                if (anchorPassOpenSet != null)
                {
                    // We work on the open set until no more progress is being made
                    bool progressMade;
                    bool hasRemainingItems;
                    do
                    {
                        progressMade = false;
                        hasRemainingItems = false;
                        for (var i = anchorPassOpenSetCount - 1; i >= 0; i--)
                        {
                            var (child, layoutBox) = anchorPassOpenSet[i];
                            if (child != null)
                            {
                                hasRemainingItems = true;
                            }

                            // Try to apply the anchors
                            if (child != null && child.Anchors.Apply(ref layoutBox))
                            {
                                child.SetLayout(layoutBox);
                                anchorPassOpenSet[i] = (null, RectangleF.Empty);
                                progressMade = true;
                            }
                        }
                    } while (progressMade && hasRemainingItems);
                }
            }
            finally
            {
                if (anchorPassOpenSet != null)
                {
                    ArrayPool<(WidgetBase, RectangleF)>.Shared.Return(anchorPassOpenSet);
                }
            }
        }

        // This function only exists to make it easier to track places that haven't been converted to use float for coordinates yet
        [Obsolete]
        protected internal static int LegacyRound(float v)
        {
            return (int) v;
        }

        /// <summary>
        /// Same as <see cref="PickWidget"/>, but the x and y coordinates
        /// are global UI coordinates,and not local to this widget. 
        /// </summary>
        public WidgetBase? PickWidgetGlobal(float x, float y)
        {
            var contentBounds = GetViewportBorderArea(true);
            if (x < contentBounds.X || x >= contentBounds.Right || y < contentBounds.Y || y >= contentBounds.Bottom)
            {
                return null;
            }

            return PickWidget(x - contentBounds.X, y - contentBounds.Y);
        }

        /// <summary>
        /// Picks the widget a the x,y coordinate local to this widgets layout box (border area).
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
            ViewportToLocal(ref x, ref y);

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

        private void ViewportToLocal(ref float x, ref float y)
        {
            var borderArea = GetViewportBorderArea();
            x -= borderArea.X;
            y -= borderArea.Y;
        }

        public void AddContent(WidgetContent content)
        {
            content.Parent = this;
            _content.Add(content);

            NotifyLayoutChange(LayoutChangeFlag.Content);
        }

        public void RemoveContent(WidgetContent content)
        {
            if (_content.Remove(content))
            {
                content.Parent = null;
                NotifyLayoutChange(LayoutChangeFlag.Content);
            }
        }

        public void ClearContent()
        {
            foreach (var content in Content)
            {
                content.Parent = null;
            }

            _content.Clear();
            NotifyLayoutChange(LayoutChangeFlag.Content);
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

        public RectangleF Rectangle
        {
            get => new(Pos, GetSize());
            set
            {
                Pos = value.Location;
                PixelSize = value.Size;
            }
        }

        public PointF Pos
        {
            get => new(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public SizeF GetSize() => BorderArea.Size;

        /// <summary>
        /// Gets the <see cref="PaddingArea"/> of this widget relative to the current viewport and optionally clipped
        /// to all parents padding areas.
        /// </summary>
        public RectangleF GetViewportPaddingArea(bool clip = false)
        {
            var bounds = PaddingArea;
            Parent?.TransformClientToViewport(ref bounds, clip);
            return bounds;
        }

        public RectangleF GetViewportContentArea(bool clip = false)
        {
            var bounds = ContentArea;
            Parent?.TransformClientToViewport(ref bounds, clip);
            return bounds;
        }

        /// <summary>
        /// Gets the <see cref="BorderArea"/> transformed to the current viewport, but not clipped.
        /// </summary>
        public RectangleF GetViewportBorderArea(bool clip = false)
        {
            // We use the border-sizing model, which means the widgets outer size includes border+padding
            var bounds = BorderArea;
            Parent?.TransformClientToViewport(ref bounds, clip);
            return bounds;
        }

        /// <summary>
        /// Returns the {x,y,w,h} rect, but regards modification from parent and subtracts the margins.
        /// </summary>
        public RectangleF GetContentArea(bool includingMargins = false)
        {
            return GetViewportPaddingArea();
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

        protected virtual void OnAfterLayout()
        {
            // Content Area *relative to our own layout box*
            var contentArea = ContentArea;
            contentArea.Offset(-LayoutBox.Location.X, -LayoutBox.Location.Y);

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
                        contentBounds = RectangleF.Empty;
                    }
                }

                if (content.GetBounds() != contentBounds)
                {
                    content.SetBounds(contentBounds);
                }
            }
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
            AddActionHotkey(hotkey, _ => callback(), condition);
        }

        /// <summary>
        /// Associates a hotkey with an action to be triggered when the hotkey is triggered.
        /// An additional condition may be specified that needs to be true in order for the action to be triggered.
        /// </summary>
        public void AddActionHotkey(Hotkey hotkey, Action<KeyboardEvent> callback, Func<bool>? condition = null)
        {
            if (hotkey.Trigger == HotkeyTrigger.Held)
            {
                throw new ArgumentException("Cannot register a hotkey that is triggered by holding as an action hotkey.");
            }

            condition ??= () => true;
            _actionHotkeys.Add(new HotkeyAction(hotkey, callback, condition));
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

            UiManager?.InvalidateHotkeys(this);
        }

        /// <summary>
        /// Removes all registered hotkeys from this widget.
        /// </summary>
        public void ClearActionHotkeys()
        {
            _actionHotkeys.Clear();
            UiManager?.InvalidateHotkeys(this);
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

        /// <summary>
        /// Removes all registered held hotkeys from this widget.
        /// </summary>
        public void ClearHeldHotkeys()
        {
            _heldHotkeys.Clear();
            UiManager?.InvalidateHotkeys(this);
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

        [Obsolete]
        public void CenterInParent()
        {
            if (Parent != null)
            {
                var parentSize = Parent.GetSize();
                X = (parentSize.Width - BorderArea.Width) / 2;
                Y = (parentSize.Height - BorderArea.Height) / 2;
            }
        }

        public bool HasMouseCapture => UiManager?.MouseCaptureWidget == this;

        #region Tree Navigation

        public virtual WidgetBase? FirstChild => null;

        public virtual WidgetBase? LastChild => null;

        public virtual IReadOnlyList<WidgetBase> Children => Array.Empty<WidgetBase>();

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

        public IEnumerable<WidgetBase> EnumerateSelfAndAncestors()
        {
            var current = this;

            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        public IEnumerable<WidgetBase> EnumerateDescendantsInTreeOrder()
        {
            var current = FirstChild;

            while (current != null)
            {
                // We do this now since the child could be detached outside the enumerator
                var next = current.NextSibling;

                yield return current;
                foreach (var grandchild in current.EnumerateDescendantsInTreeOrder())
                {
                    yield return grandchild;
                }

                current = next;
            }
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

        /// <summary>
        /// Calculates the preferred size for this widgets <see cref="PaddingArea"/> using the given available horizontal and vertical space,
        /// which can be <see cref="float.PositiveInfinity"/>.
        /// </summary>
        protected virtual SizeF ComputePreferredPaddingAreaSize(float availableWidth, float availableHeight)
        {
            // Content is laid out relative to the content area
            var area = new RectangleF(
                ComputedStyles.PaddingLeft,
                ComputedStyles.PaddingTop,
                0,
                0
            );
            foreach (var content in Content)
            {
                var contentRect = new RectangleF
                {
                    Location = new PointF(content.X, content.Y),
                    Size = !content.FixedSize.IsEmpty ? content.FixedSize : content.GetPreferredSize()
                };
                area = RectangleF.Union(area, contentRect);
            }

            return new SizeF(
                Math.Min(availableWidth, area.Right + ComputedStyles.PaddingRight),
                Math.Min(availableHeight, area.Bottom + ComputedStyles.PaddingBottom)
            );
        }

        /// <summary>
        /// Calculates the preferred size for this widgets <see cref="BorderArea"/> using the given available horizontal and vertical space,
        /// which can be <see cref="float.PositiveInfinity"/>.
        /// </summary>
        public SizeF ComputePreferredBorderAreaSize(float availableWidth = float.PositiveInfinity, float availableHeight = float.PositiveInfinity)
        {
            if (availableWidth is < 0 or float.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(availableWidth));
            }

            if (availableHeight is < 0 or float.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(availableHeight));
            }

            var devicePixelsPerUiPixel = UiManager?.DevicePixelsPerUiPixel ?? 1;

            // If either width or height is in auto-mode, we need to measure content
            float preferredWidth = 0;
            float preferredHeight = 0;
            if (Width.Type == DimensionUnit.Auto || Height.Type == DimensionUnit.Auto)
            {
                var horizontalBorder = 2 * ComputedStyles.BorderWidth;
                var verticalBorder = 2 * ComputedStyles.BorderWidth;

                var innerWidth = availableWidth;
                var innerHeight = availableHeight;
                // When measuring content and either dimension is not auto, we use it for content measurement
                // Example: Set width to 100px and height to auto. Content should be given a chance to know
                // it should wrap at 100px.
                if (Width.Type != DimensionUnit.Auto)
                {
                    innerWidth = Width.Evaluate(availableWidth, devicePixelsPerUiPixel, 0);
                }

                if (Height.Type != DimensionUnit.Auto)
                {
                    innerHeight = Height.Evaluate(availableHeight, devicePixelsPerUiPixel, 0);
                }

                var contentAreaSize = ComputePreferredPaddingAreaSize(
                    innerWidth - horizontalBorder,
                    innerHeight - verticalBorder
                );

                preferredWidth = contentAreaSize.Width + horizontalBorder;
                preferredHeight = contentAreaSize.Height + verticalBorder;
            }

            return new SizeF(
                Width.Evaluate(availableWidth, devicePixelsPerUiPixel, preferredWidth),
                Height.Evaluate(availableHeight, devicePixelsPerUiPixel, preferredHeight)
            );
        }

        public void SetLayout(RectangleF box)
        {
            if (box.Width < 0 || box.Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(box), box, "Layout box size is negative");
            }

            // Apply pixel snapping
            if (UiManager != null)
            {
                var rootBox = box;
                if (Parent != null)
                {
                    rootBox.Offset(Parent.GetViewportBorderArea().Location);
                }

                var pos = rootBox.Location;
                UiManager?.SnapToPhysicalPixelGrid(ref pos);
                var deltaX = rootBox.X - pos.X;
                var deltaY = rootBox.Y - pos.Y;
                box.Offset(deltaX, deltaY);
            }

            _layoutBox = box;
            HasValidLayout = true;
        }

        private void ClearLayout()
        {
            HasValidLayout = false;

            // Recursively clear layout box
            foreach (var child in Children)
            {
                child.ClearLayout();
            }
        }

        protected void NotifyLayoutChange(LayoutChangeFlag flags)
        {
            UiManager?.InvalidateLayout();
        }
    }
}