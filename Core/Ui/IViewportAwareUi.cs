using System.Drawing;

namespace OpenTemple.Core.Ui
{
    public interface IViewportAwareUi
    {
        void ResizeViewport(Size size);
    }
}