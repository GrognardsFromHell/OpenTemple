using System.Drawing;

namespace OpenTemple.Core.Ui.DOM
{
    public interface CaretPosition
    {
        public Node OffsetNode { get; }
        public ulong Offset { get; }
        RectangleF? GetClientRect();
    }
}