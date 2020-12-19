using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.DOM;

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

        public WindowConfig WindowConfig { get; set; }

        public event Action<IEvent> OnEvent;

        public event Action<string> OnTextInput;
    }
}