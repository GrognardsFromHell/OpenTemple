using System;
using System.Drawing;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Platform
{
    public interface IMainWindow
    {
        IntPtr NativeHandle { get; }

        void SetWindowMsgFilter(WindowMsgFilter filter);

        void SetMouseMoveHandler(MouseMoveHandler handler);

        /// <summary>
        /// Invoked when the window changes size. Given size will be the new client area of the window.
        /// </summary>
        event Action<Size> Resized;

    }
}