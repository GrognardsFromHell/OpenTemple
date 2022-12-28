using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.Widgets;

/// <summary>
/// A widget that displays a scrollable list of draggable items.
/// </summary>
public class DraggableItemList<T> : WidgetScrollView
{
    private readonly List<T> _items = new();

    public required Func<T, InlineElement> TextFactory { get; init; }

    private bool _childrenInvalid = true;

    public DraggableItemList()
    {
        ContainerPadding = 0;
    }

    public IReadOnlyList<T> Items
    {
        get => _items;
        set
        {
            _items.Clear();
            _items.AddRange(value);
            _childrenInvalid = true;
        }
    }

    protected internal override void UpdateLayout(LayoutContext context)
    {
        if (_childrenInvalid)
        {
            _childrenInvalid = false;
            Clear(true);

            foreach (var item in _items)
            {
                var widget = new DraggableListItem<T>(item);
                widget.Content = TextFactory(item);
                widget.LocalStyles.BackgroundColor = new PackedLinearColorA(255, 0, 0, 255);
                widget.LocalStyles.PaddingTop = 2;
                widget.LocalStyles.PaddingBottom = 2;
                Add(widget);
            }
        }

        base.UpdateLayout(context);
    }
}