using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Platform;
using Qml.Net;
using QtQuick;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.TigSubsystems
{
    public class HeadlessMainWindow : IMainWindow
    {
        public IntPtr NativeHandle => IntPtr.Zero;
        public Device D3D11Device { get; }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
        }

        public event Action<Size> Resized;

        public Size RenderTargetSize { get; }

        public WindowConfig WindowConfig { get; set; }

        public NativeCursor Cursor { get; set; }

        public void Show()
        {
        }

        public void HideCursor()
        {
        }

        public void QueueUpdate()
        {
        }

        public void ProcessEvents()
        {
        }

        public void Quit()
        {
        }

        public string BaseUrl { get; set; }

        public Task<T> LoadView<T>(string path) where T : QQuickItem
        {
            return Task.FromException<T>(new NotSupportedException());
        }

        public Task<IntPtr> LoadViewNative(string path)
        {
            return Task.FromException<IntPtr>(new NotSupportedException());
        }

        public event Action OnBeforeRendering;
        public event Action OnBeforeRenderPassRecording;
        public event Action OnAfterRenderPassRecording;
        public event Action OnAfterRendering;
        public event Action OnClose;
        public event Action<Device> OnDeviceCreated;
        public event Action<Device> OnDeviceDestroyed;

        public void BeginExternalCommands()
        {
        }

        public void EndExternalCommands()
        {
        }

        public NativeMouseEventFilter MouseEventFilter { get; set; }

        public NativeWheelEventFilter WheelEventFilter { get; set; }

        public NativeKeyEventFilter KeyEventFilter { get; set; }

        public bool IsInThread => true;

        public Task PostTask(Action work)
        {
            work();
            return Task.CompletedTask;
        }

        public Task<T> PostTask<T>(Func<T> work)
        {
            return Task.FromResult(work());
        }

        public Task<T> PostTask<T>(Func<Task<T>> work)
        {
            return work();
        }

        public QQuickItem RootItem { get; set; }

        public Task CreateModule(string uri, Action<IModuleBuilder> moduleFactory)
        {
            throw new NotImplementedException();
        }
    }
}