using System;
using System.Drawing;
using Avalonia.Controls;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Scenes;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.TigSubsystems
{
    public class HeadlessMainWindow : IMainWindow
    {
        public IntPtr NativeHandle => IntPtr.Zero;

        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
        }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
        }

        public event Action<Size> Resized;

        public event Action Closed;

        public event Action BeforeRenderContent;

        public void Close()
        {
            Closed?.Invoke();
        }

        public WindowConfig WindowConfig { get; set; }

        public void AddMainContent(IControl control)
        {
        }

        public void RemoveMainContent(IControl control)
        {
        }

        public void RenderContent()
        {
        }

        public void AddOverlay(Control control)
        {
        }

        public void RemoveOverlay(Control control)
        {
        }

        public void TakeScreenshot(string path, int width, int height)
        {
            throw new NotImplementedException();
        }

        // TODO: Create offscreen WARP device
        public Device Direct3D11Device { get; }

    }
}
