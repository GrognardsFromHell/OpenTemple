using System;
using System.Drawing;
using Avalonia.Controls;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Scenes;
using OpenTemple.Core.Ui.Options;
using SharpDX.Direct3D11;

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

        /// <summary>
        /// Invoked when the window closes.
        /// </summary>
        event Action Closed;

        /// <summary>
        /// Invoked directly before a new frame is going to be rendered (on the main thread).
        /// </summary>
        event Action BeforeRenderContent;

        void Close();

        WindowConfig WindowConfig { get; set; }

        void RenderContent();

        void AddMainContent(IControl control);

        void RemoveMainContent(IControl control);

        void AddOverlay(Control control);

        void RemoveOverlay(Control control);

        void TakeScreenshot(string path, int width, int height);
    }
}
