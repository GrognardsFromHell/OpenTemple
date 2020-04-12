using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.TigSubsystems
{
    public class HeadlessMainWindow : IMainWindow
    {
        public IntPtr NativeHandle => IntPtr.Zero;

        public IntPtr D3D11Device { get; }

        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
        }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
        }

        public event Action<Size> Resized;

        public WindowConfig WindowConfig { get; set; }

        public NativeCursor Cursor { get; set; }

        public void HideCursor()
        {
            throw new NotImplementedException();
        }

        public Task<object> LoadView(string path)
        {
            return Task.FromException<object>(new NotSupportedException());
        }
    }
}