using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.Images;

namespace OpenTemple.Core.Platform
{
    public interface IMainWindow
    {
        IntPtr NativeHandle { get; }

        IntPtr D3D11Device { get; }

        void SetWindowMsgFilter(WindowMsgFilter filter);

        void SetMouseMoveHandler(MouseMoveHandler handler);

        /// <summary>
        /// Invoked when the window changes size. Given size will be the new client area of the window.
        /// </summary>
        event Action<Size> Resized;

        WindowConfig WindowConfig { get; set; }

        NativeCursor Cursor { set; }

        void HideCursor();

        Task<dynamic> LoadView(string path);
    }
}