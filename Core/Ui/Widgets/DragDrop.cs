#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Widgets;

public static class DragDrop
{
    public static DragController MakeDraggable(WidgetBase? widget)
    {
        var controller = new DragController(widget);
        widget.SetMouseMsgHandler(controller.HandleMouseMessage);
        return controller;
    }

    public record DragController(WidgetBase? Widget)
    {
        private bool IsDragging => Widget.UiManager.GetMouseCaptureWidget() == Widget;

        // The point on the widget that was clicked when dragging was initiated (relative to upper left corner of widget)
        private Point _mouseDownAt;

        // True if the button is being held
        private bool _mouseHeld;

        public event Action<Rectangle>? OnRenderDragging;

        public event Action<WidgetBase?>? OnDrop;

        // Track which widgets we are highlighting to properly remove the styles again
        private List<(WidgetBase widget, string styleId)> _activeHighlights = new();

        /// <summary>
        /// Register a handler for being dropped on a specific type of widget.
        /// </summary>
        public void WhenDroppedOn<T>(Action<T> handler) where T : WidgetBase
        {
            OnDrop += droppedOn =>
            {
                var target = droppedOn?.Closest<T>();
                if (target != null)
                {
                    handler(target);
                }
            };
        }

        /// <summary>
        /// Register a handler for being dropped on a specific widget instance.
        /// </summary>
        public void WhenDroppedOn(WidgetBase dropTarget, Action handler)
        {
            OnDrop += droppedOn =>
            {
                if (droppedOn != null && droppedOn.IsOrIsDescendantOf(dropTarget))
                {
                    handler();
                }
            };
        }

        public void WithDraggedContent(params WidgetContent[] contents)
        {
            OnRenderDragging += contentArea =>
            {
                foreach (var content in contents)
                {
                    content.SetBounds(contentArea);
                    content.Render();                    
                }
            };
        }

        /// <summary>
        /// Adds the given styleId to the given widget whenever something is dragged over it by this
        /// controller. The style will be removed automatically when dragging ends or the cursor is
        /// moved out of the widget. 
        /// </summary>
        public void HighlightDropTarget(WidgetBase dropTarget, string styleId)
        {
            
        }

        internal bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if (IsDragging)
            {
                if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
                {
                    EndDrag();                    

                    var widgetUnderCursor = Widget.UiManager.GetWidgetAt(msg.X, msg.Y);
                    if (widgetUnderCursor != null)
                    {
                        OnDrop?.Invoke(widgetUnderCursor);
                    }
                }
                else if ((msg.flags & MouseEventFlag.PosChange) != 0)
                {
                    var widgetUnderCursor = Widget.UiManager.GetWidgetAt(msg.X, msg.Y);
                    if (widgetUnderCursor != null)
                    {
                        
                    }
                }

                return true;
            }
            else
            {
                // Figure out where in the widget we got clicked so we can draw the dragged text with the proper offset
                var globalContentArea = Widget.GetContentArea(true);
                var localX = msg.X - globalContentArea.X;
                var localY = msg.Y - globalContentArea.Y;

                if ((msg.flags & MouseEventFlag.LeftClick) != 0)
                {
                    // Dragging is initiated by moving the mouse a minimum amount after holding the button on the widget
                    if (!_mouseHeld)
                    {
                        _mouseDownAt = new Point(localX, localY);
                        _mouseHeld = true;
                    }
                }
                else if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
                {
                    _mouseHeld = false;
                }
                else if (_mouseHeld && (msg.flags & MouseEventFlag.PosChange) != 0)
                {
                    var deltaX = Math.Abs(localX - _mouseDownAt.X);
                    var deltaY = Math.Abs(localY - _mouseDownAt.Y);
                    var dragSize = Widget.UiManager.DragStartDistance;
                    if (deltaX >= dragSize.Width || deltaY >= dragSize.Height)
                    {
                        _mouseHeld = false;
                        BeginDrag();
                    }
                }
            }

            return true;
        }

        private void EndDrag()
        {
            Widget.UiManager.SetCursorDrawCallback(null);
            Widget.UiManager.ReleaseMouseCapture(Widget);
            
            foreach (var (widget, styleId) in _activeHighlights)
            {
                widget.RemoveStyle(styleId);
            }
            _activeHighlights.Clear();
        }

        private void BeginDrag()
        {
            // Something else can have the mouse capture right now (how are we getting this message then...?)
            if (!Widget.UiManager.TryCaptureMouse(Widget))
            {
                return;
            }

            // This will allow the drag controller to draw a representation of the dragged widget
            Widget.UiManager.SetCursorDrawCallback(DrawDraggedWidget);
        }

        private void DrawDraggedWidget(int x, int y, object _)
        {
            var point = new Point(x, y);
            point.Offset(-_mouseDownAt.X, -_mouseDownAt.Y);
            var contentArea = new Rectangle(point, Widget.GetSize());

            OnRenderDragging?.Invoke(contentArea);
        }
    }
}