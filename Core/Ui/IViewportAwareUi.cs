using System.Drawing;

namespace SpicyTemple.Core.Ui
{
    public interface IViewportAwareUi
    {
        void ResizeViewport(Size size);
    }
}