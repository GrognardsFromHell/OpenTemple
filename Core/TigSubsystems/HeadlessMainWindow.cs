using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.TigSubsystems
{
    public class HeadlessMainWindow : IMainWindow
    {
        public IntPtr NativeHandle => IntPtr.Zero;

        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
        }

        public event Action<WindowEvent> OnInput;

        public event Action<Size> Resized;

        public event Action Closed;

        public WindowConfig WindowConfig { get; set; }

        public void InvokeClosed()
        {
            Closed?.Invoke();
        }

        public Size UiCanvasTargetSize { get; set; }
        public SizeF UiCanvasSize { get; set; }
        public event Action UiCanvasSizeChanged;
        public float UiScale { get; set; }
    }
}