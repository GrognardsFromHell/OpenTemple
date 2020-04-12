using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using Qml.Net.Internal.Qml;

namespace OpenTemple.Core
{
    /// <summary>
    /// Settings for the window on the native side.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct NativeWindowConfig
    {
        public int MinWidth;
        public int MinHeight;
        public int Width;
        public int Height;
        public bool IsFullScreen;
    }

    [SuppressUnmanagedCodeSecurity]
    public class NativeMainWindow : IMainWindow, IDisposable
    {
        public const string DllName = @"D:\OpenTemple.Native\cmake-build-debug\OpenTemple.Native.dll";

        public static NativeMainWindow Instance { get; set; }

        readonly GameViews views;

        readonly GameViewDelegates delegates;

        [SuppressUnmanagedCodeSecurity]
        private delegate void BeforeRenderingCallback();

        private readonly BeforeRenderingCallback _beforeRenderingCallback;

        private IntPtr _ui;

        public IntPtr NativeHandle { get; }

        public IntPtr D3D11Device => ui_getd3d11device(_ui);

        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
            // TODO
        }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
            // TODO
        }

        public event Action<Size> Resized;

        public WindowConfig WindowConfig { get; set; }

        public static void AddUiSearchPath(string path)
        {
            ui_add_search_path("ui", path);
        }

        public NativeMainWindow(WindowConfig windowConfig)
        {
            _beforeRenderingCallback = InvokeOnBeforeRendering;

            views = new GameViews();
            delegates = GameViewDelegates.Create(views);

            _ui = ui_create(ref delegates);

            ui_set_before_rendering_callback(_ui, _beforeRenderingCallback);
            ui_set_title(_ui, "OpenTemple");
            ui_set_icon(_ui, "ui:app.ico");

            // Configures and shows the window
            NativeWindowConfig nativeConfig = default;
            nativeConfig.Width = windowConfig.Width;
            nativeConfig.Height = windowConfig.Height;
            nativeConfig.MinWidth = windowConfig.MinWidth;
            nativeConfig.MinHeight = windowConfig.MinHeight;
            nativeConfig.IsFullScreen = !windowConfig.Windowed;
            ui_set_config(_ui, ref nativeConfig);
        }

        private void InvokeOnBeforeRendering()
        {
            // Tig.SystemEventPump.PumpSystemEvents();
            //ProcessMessages();
            GameSystems.AdvanceTime();
            UiSystems.AdvanceTime();

            views.Render();
        }

        public bool Render()
        {
            return ui_render(_ui);
        }

        public void Dispose()
        {
            ui_destroy(_ui);
            _ui = IntPtr.Zero;
        }

        public void HideCursor()
        {
            ui_hide_cursor(_ui);
        }

        public NativeCursor Cursor
        {
            set
            {
                Trace.Assert(value.Handle != null);
                ui_set_cursor(_ui, value.Handle);
            }
        }

        [DllImport(DllName)]
        private static extern IntPtr ui_create([In]
            ref GameViewDelegates delegates);

        [DllImport(DllName)]
        private static extern bool ui_render(IntPtr ui);

        [DllImport(DllName)]
        private static extern IntPtr ui_getd3d11device(IntPtr ui);

        [DllImport(DllName)]
        private static extern void ui_set_cursor(IntPtr ui, IntPtr cursor);

        [DllImport(DllName)]
        private static extern void ui_hide_cursor(IntPtr ui);

        [DllImport(DllName)]
        private static extern void ui_set_title(IntPtr ui, [MarshalAs(UnmanagedType.LPWStr)]
            string title);

        [DllImport(DllName)]
        private static extern void ui_set_icon(IntPtr ui, [MarshalAs(UnmanagedType.LPWStr)]
            string path);

        [DllImport(DllName)]
        private static extern void ui_set_config(IntPtr ui, ref NativeWindowConfig config);

        [DllImport(DllName)]
        private static extern void ui_destroy(IntPtr handle);

        [DllImport(DllName)]
        private static extern void ui_set_before_rendering_callback(IntPtr handle, BeforeRenderingCallback callback);

        [DllImport(DllName)]
        private static extern void ui_add_search_path([MarshalAs(UnmanagedType.LPWStr)]
            string prefix,
            [MarshalAs(UnmanagedType.LPWStr)]
            string path);

        [DllImport(DllName)]
        private static extern void ui_load_view(IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string path,
            IntPtr completionSource
        );

        public Task<dynamic> LoadView(string path)
        {
            var (nativeTask, completionSource) = NativeCompletionSource.Create<IntPtr>();
            ui_load_view(_ui, path, completionSource);

            return nativeTask.ContinueWith(task => new NetQObject(task.Result, false).AsDynamic());
        }
    }
}