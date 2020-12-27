using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui.Widgets
{

    /// <summary>
    /// A virtual scroll view will only render the items that are currently visible, but only supports
    /// items with a fixed height.
    /// </summary>
    public class VirtualScrollView : WidgetContainer
    {
        /// <summary>
        /// How many items that are outside of the visible range are kept alive to slightly speed up
        /// scrolling back and forth.
        /// </summary>
        private const int Overhang = 2;

        private readonly WidgetScrollBar _scrollBar;

        /// <summary>
        /// Gap between items.
        /// </summary>
        public int Gap { get; set; }

        // Keep track of items and their index
        private readonly List<(int, WidgetBase)> _items = new List<(int, WidgetBase)>();

        public VirtualScrollView(IVirtualListContent content,
            [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(0, 0, 0, 0, filePath, lineNumber)
        {
            Content = content;
            _scrollBar = new WidgetScrollBar();
            Add(_scrollBar);

            AddEventListener(SystemEventType.Wheel, msg =>
            {
                var evt = (WheelEvent) msg;
                if (evt.Target is Node targetNode && !_scrollBar.IsInclusiveAncestor(targetNode))
                {
                    _scrollBar.DispatchEvent(evt.Copy());
                }
            });
        }

        public IVirtualListContent Content { get; set; }

        private int Count => Content?.Count ?? 0;

        private int ItemHeight => Math.Max(1, Content?.ItemHeight ?? 1);

        private int ItemStride => ItemHeight + Gap;

        private (int, int) VisibleRange
        {
            get
            {
                var offset = _scrollBar.GetValue() / ItemStride;
                // Not only do we need to round up, but additionally, the partially shown
                // item could be split across the top and bottom, which results in two
                // additional items shown (one from rounding up, one added unconditionally)
                var count = (int) MathF.Ceiling(Height / ItemStride) + 1;
                count = Math.Clamp(count, 0, Count - offset);
                return (offset, count);
            }
        }

        public override void Render()
        {
            // Position scrollbar
            _scrollBar.Height = Height;
            _scrollBar.X = Width - _scrollBar.Width;
            _scrollBar.SetMax(Math.Max(0, ItemHeight + (Count - 1) * ItemStride) - (int) Height);
            // The scrollbar arrow buttons should at most scroll 1/5th the viewport
            _scrollBar.SmallChange = (int) Math.Min(Height / 5, ItemStride);
            // A mouse-wheel event should scroll at most a full view
            _scrollBar.LargeChange = (int) Math.Min(Height, VisibleRange.Item2 * ItemStride);

            UpdateItemList();
            PositionItems();

            base.Render();
        }

        private void UpdateItemList()
        {
            // Calculate range of visible items
            var (offset, count) = VisibleRange;

            // First pass on existing items: Find any items from within the range that are already created
            Span<bool> presentItems = stackalloc bool[count];
            foreach (var (index, _) in _items)
            {
                if (index >= offset && index < offset + count)
                {
                    presentItems[index - offset] = true;
                }
            }

            // Second pass on existing items: Delete any item that is outside of the visible range
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var index = _items[i].Item1;
                if (index < offset - Overhang || index >= offset + count + Overhang)
                {
                    Remove(_items[i].Item2);
                    _items.RemoveAt(i);
                }
            }

            // Create any items that are missing
            for (var i = 0; i < presentItems.Length; i++)
            {
                // Only create the item if it's not already present
                if (!presentItems[i])
                {
                    var index = offset + i;
                    var item = Content.CreateItem(index);
                    _items.Add((index, item));
                    Add(item);
                }
            }
        }

        private RectangleF GetRectangle(int index)
        {
            var y = index * ItemStride;
            return new RectangleF(0, y - _scrollBar.GetValue(), Width - _scrollBar.Width, ItemHeight);
        }

        private void PositionItems()
        {
            foreach (var (index, item) in _items)
            {
                item.Rectangle = GetRectangle(index);
            }
        }
    }

    public interface IVirtualListContent
    {
        int Count { get; }

        /// <summary>
        ///     Height of a single item in this list. This is used to compute the range
        ///     of items that is currently visible, together with ItemCount.
        /// </summary>
        int ItemHeight { get; }

        /// <summary>
        /// Creates the item to display for the given position in the list.
        /// Position and size will be set by the list.
        /// </summary>
        WidgetBase CreateItem(int index);
    }
}