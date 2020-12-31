using System;
using System.Drawing;
using Avalonia.Controls;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
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

        public void Close()
        {
            Closed?.Invoke();
        }

        public WindowConfig WindowConfig { get; set; }

        public IControl MainContent { get; set; }

        public void RenderContent()
        {
        }

        public void AddOverlay(Control control)
        {
        }

        public void RemoveOverlay(Control control)
        {
        }

        // TODO: Create offscreen WARP device
        public Device Direct3D11Device { get; }
    }
}