using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO.Images;
using QtQuick;
using D3D11Device = SharpDX.Direct3D11.Device;

namespace OpenTemple.Core.Platform
{

    public interface IModuleBuilder
    {
        IModuleBuilder RegisterType<T>();
        IModuleBuilder RegisterSingleton<T>(string name = null);
        IModuleBuilder RegisterSingleton<T>(T instance, string name = null);
    }

    public interface IUserInterfaceInterop
    {
        Task CreateModule(string uri, Action<IModuleBuilder> moduleFactory);
    }

    public interface IUserInterface : ITaskQueue
    {

        Task<T> LoadView<T>(string path) where T : QQuickItem;

        QtQuick.QQuickItem RootItem { get; }

    }

    public interface IMainWindow : IUserInterfaceInterop, IUserInterface
    {
        IntPtr NativeHandle { get; }

        D3D11Device D3D11Device { get; }

        void SetMouseMoveHandler(MouseMoveHandler handler);

        /// <summary>
        /// Invoked when the window changes size. Given size will be the new render target size
        /// </summary>
        event Action<Size> Resized;

        Size RenderTargetSize { get; }

        WindowConfig WindowConfig { get; set; }

        NativeCursor Cursor { set; }

        void Show();

        void HideCursor();

        void QueueUpdate();

        void ProcessEvents();

        void Quit();

        /// <summary>
        /// Base-URL when loading QML components via LoadView
        /// </summary>
        string BaseUrl { get; set; }

        event Action OnBeforeRendering;

        event Action OnBeforeRenderPassRecording;

        event Action OnAfterRenderPassRecording;

        event Action OnAfterRendering;

        event Action OnClose;

        event Action<D3D11Device> OnDeviceCreated;

        event Action<D3D11Device> OnDeviceDestroyed;

        void BeginExternalCommands();

        void EndExternalCommands();

        NativeMouseEventFilter MouseEventFilter { get; set; }

        NativeWheelEventFilter WheelEventFilter { get; set; }

        NativeKeyEventFilter KeyEventFilter { get; set; }

    }
}