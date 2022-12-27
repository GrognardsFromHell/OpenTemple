using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Ui.Widgets;

public class DraggableListItem<T> : WidgetLabel
{
    public T Item { get; }

    public DraggableListItem(T item, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : base(filePath, lineNumber)
    {
        Item = item;
    }
}
