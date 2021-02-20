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

        void Close();

        WindowConfig WindowConfig { get; set; }

        void RenderContent();

        void AddMainContent(IControl control);

        void RemoveMainContent(IControl control);

        void AddOverlay(Control control);

        void RemoveOverlay(Control control);

        Device Direct3D11Device { get; }

        void TakeScreenshot(string path, int width, int height);
    }
}
