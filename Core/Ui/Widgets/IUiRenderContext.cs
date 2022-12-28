using System.Drawing;

namespace OpenTemple.Core.Ui.Widgets;

public interface IUiRenderContext
{
    void PushScissorRect(RectangleF rect);
    void PopScissorRect();
}
