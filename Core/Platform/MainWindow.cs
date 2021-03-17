using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;
using Avalonia.Rendering;
using OpenTemple.Core.Config;
using OpenTemple.Core.Scenes;
using OpenTemple.Core.Ui;
using ReactiveUI;
using SharpDX.Direct3D11;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.Platform
{
    public delegate void MouseMoveHandler(int x, int y, int wheelDelta);

    public delegate bool WindowMsgFilter(uint msg, ulong wparam, long lparam);

    internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, ulong wParam, long lParam);

    public class MainWindow : IMainWindow, IDisposable
    {
        private readonly App _app;

        private readonly Ui.MainWindow _window;

        private readonly Panel _mainContentContainer;

        private readonly Panel _overlayContainer;

        private WindowConfig _config;

        public IntPtr NativeHandle { get; }

        public event Action<Size> Resized;

        public event Action Closed;

        public event Action BeforeRenderContent;

        public void AddMainContent(IControl control)
        {
            _mainContentContainer.Children.Add(control);
        }

        public void RemoveMainContent(IControl control)
        {
            _mainContentContainer.Children.Remove(control);
        }

        public MainWindow(WindowConfig config)
        {
            PlatformRegistrationManager.SetRegistrationNamespaces((RegistrationNamespace) 999);

            _app = (App) AppBuilder.Configure<App>()
                .With(() => new Win32PlatformOptions()
                {
                    UseWgl = false,
                    AllowEglInitialization = true,
                    UseWindowsUIComposition = false,
                    UseDeferredRendering = true
                })
                .With(() => new AngleOptions()
                {
                    AllowedPlatformApis = new List<AngleOptions.PlatformApi>() {AngleOptions.PlatformApi.DirectX11}
                })
                .UseWin32()
                .UseSkia()
                .SetupWithoutStarting()
                .UseReactiveUI()
                .Instance;

            _config = config.Copy();
            if (_config.Width < _config.MinWidth)
            {
                _config.Width = _config.MinWidth;
            }

            if (_config.Height < _config.MinHeight)
            {
                _config.Height = _config.MinHeight;
            }

            _window = new Ui.MainWindow();
            _mainContentContainer = _window.FindControl<Panel>("MainScene");
            _overlayContainer = _window.FindControl<Panel>("Overlays");

            _window.Closed += (_, _) => Closed?.Invoke();

            WindowConfig = _config;
            _window.Show();

            _window.GotFocus += (_, _) =>
            {
                if (!_config.Windowed)
                {
                    _window.Topmost = true;
                }
            };
            _window.LostFocus += (_, _) =>
            {
                if (!_config.Windowed)
                {
                    _window.Topmost = false;
                }
            };
            _window.GetObservable(TopLevel.ClientSizeProperty)
                .Subscribe(size => Resized?.Invoke(new Size((int) size.Width, (int) size.Height)));

            CenterOnScreen();

            NativeHandle = _window.PlatformImpl.Handle.Handle;
        }

        public Task PushScene(IScene scene)
        {
            throw new NotImplementedException();
        }

        private void CenterOnScreen()
        {
            var bounds = _window.Screens.Primary.Bounds;
            // TODO: Potentially monitor scaling will break this (needs testing)
            _window.Position = bounds.Center - new PixelPoint((int) _window.Width / 2, (int) _window.Height / 2);
        }

        public void Close()
        {
            _window.Close();
        }

        public void Dispose()
        {
            _window.Close();
        }

        private bool unsetClip = false;

        public WindowConfig WindowConfig
        {
            get => _config;
            set
            {
                _config = value.Copy();

                if (!_config.Windowed)
                {
                    _window.Topmost = true;
                    _window.SystemDecorations = SystemDecorations.None;
                    _window.WindowState = WindowState.FullScreen;
                }
                else
                {
                    _window.Topmost = false;
                    _window.SystemDecorations = SystemDecorations.Full;
                    _window.WindowState = WindowState.Normal;
                }

                _window.Width = _config.Width;
                _window.Height = _config.Height;
            }
        }

        public void RenderContent()
        {
            if (_window.IsVisible)
            {
                BeforeRenderContent?.Invoke();
                ((DeferredRenderer) _window.Renderer).RenderOnlyOnRenderThread = true;
                _window.Renderer.Paint(_window.Bounds);
            }
        }

        public void AddOverlay(Control control)
        {
            _overlayContainer.Children.Add(control);
        }

        public void RemoveOverlay(Control control)
        {
            _overlayContainer.Children.Remove(control);
        }

        // Locks the mouse cursor to this window
        // if we're in the foreground
        public void ConfineCursor(int x, int y, int w, int h)
        {
            var isForeground = GetForegroundWindow() == NativeHandle;

            if (isForeground)
            {
                var rect = new RECT {X = x, Y = y, Right = x + w, Bottom = y + h};

                ClipCursor(ref rect);
                unsetClip = false;
            }
            else // Unset the CursorClip  (was supposed to happen naturally when alt-tabbing, but sometimes it requires hitting a keystroke before it takes effect so adding it here too)
            {
                // Check if already unset the CursorClip so it doesn't cause interference in case some other app is trying to clip the cursor...
                if (unsetClip)
                    return;
                unsetClip = true;

                ClipCursor(IntPtr.Zero);
            }
        }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
        }

        // Sets a filter that receives a chance at intercepting all window messages
        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
        }

        public void TakeScreenshot(string path, int width, int height)
        {
            throw new NotImplementedException();
        }

//        private int mousePosX = 0; // Replaces memory @ 10D25CEC
//        private int mousePosY = 0; // Replaces memory @ 10D25CF0
//
//        private IntPtr WndProc(IntPtr hWnd, uint msg, ulong wParam, long lParam)
//        {
//            if (hWnd != _windowHandle)
//            {
//                return DefWindowProc(hWnd, msg, wParam, lParam);
//            }
//
//            if (mWindowMsgFilter != null && mWindowMsgFilter(msg, wParam, lParam))
//            {
//                return DefWindowProc(hWnd, msg, wParam, lParam);
//            }
//
//            switch (msg)
//            {
//                case WM_SETFOCUS:
//                    // Make our window topmost unless a debugger is attached
//                    if ((IntPtr) wParam == _windowHandle && !IsDebuggerPresent())
//                    {
//                        SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
//                    }
//
//                    break;
//                case WM_KILLFOCUS:
//                    // Make our window topmost unless a debugger is attached
//                    if ((IntPtr) wParam == _windowHandle && !IsDebuggerPresent())
//                    {
//                        SetWindowPos(_windowHandle, HWND_NOTOPMOST, 0, 0, 0, 0,
//                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
//                    }
//
//                    break;
//                case WM_SYSCOMMAND:
//                    if (wParam == SC_KEYMENU || wParam == SC_SCREENSAVE || wParam == SC_MONITORPOWER)
//                    {
//                        return IntPtr.Zero;
//                    }
//
//                    break;
//                case WM_GETMINMAXINFO:
//                    unsafe
//                    {
//                        var minMaxInfo = (MinMaxInfo*) lParam;
//                        minMaxInfo->ptMinTrackSize.X = _config.MinWidth;
//                        minMaxInfo->ptMinTrackSize.Y = _config.MinHeight;
//                    }
//
//                    break;
//                case WM_SIZE:
//                    Resized?.Invoke(new Size(
//                        WindowsMessageUtils.GetXParam(lParam),
//                        WindowsMessageUtils.GetYParam(lParam)
//                    ));
//                    break;
//                case WM_ACTIVATEAPP:
//                    IsInForeground = wParam == 1;
//                    IsInForegroundChanged?.Invoke(IsInForeground);
//                    break;
//                case WM_ERASEBKGND:
//                    return IntPtr.Zero;
//                case WM_CLOSE:
//                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs(1)));
//                    break;
//                case WM_QUIT:
//                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs((int) wParam)));
//                    break;
//                case WM_LBUTTONDOWN:
//                    Tig.Mouse.SetButtonState(MouseButton.LEFT, true);
//                    break;
//                case WM_LBUTTONUP:
//                    Tig.Mouse.SetButtonState(MouseButton.LEFT, false);
//                    break;
//                case WM_RBUTTONDOWN:
//                    Tig.Mouse.SetButtonState(MouseButton.RIGHT, true);
//                    break;
//                case WM_RBUTTONUP:
//                    Tig.Mouse.SetButtonState(MouseButton.RIGHT, false);
//                    break;
//                case WM_MBUTTONDOWN:
//                    Tig.Mouse.SetMmbReference();
//                    Tig.Mouse.SetButtonState(MouseButton.MIDDLE, true);
//                    break;
//                case WM_MBUTTONUP:
//                    Tig.Mouse.ResetMmbReference();
//                    Tig.Mouse.SetButtonState(MouseButton.MIDDLE, false);
//                    break;
//                case WM_SYSKEYDOWN:
//                case WM_KEYDOWN:
//                {
//                    var key = (DIK) ToDirectInputKey((VirtualKey) wParam);
//                    if (key != 0)
//                    {
//                        Tig.MessageQueue.Enqueue(new Message(
//                            new MessageKeyStateChangeArgs
//                            {
//                                key = key,
//                                // Means it has changed to pressed
//                                down = true
//                            }
//                        ));
//                    }
//                }
//                    break;
//                case WM_KEYUP:
//                case WM_SYSKEYUP:
//                {
//                    var key = (DIK) ToDirectInputKey((VirtualKey) wParam);
//                    if (key != 0)
//                    {
//                        Tig.MessageQueue.Enqueue(new Message(
//                            new MessageKeyStateChangeArgs
//                            {
//                                key = key,
//                                // Means it has changed to up
//                                down = false
//                            }
//                        ));
//                    }
//                }
//                    break;
//                case WM_CHAR:
//                    Tig.MessageQueue.Enqueue(new Message(new MessageCharArgs((char) wParam)));
//                    break;
//                case WM_MOUSEWHEEL:
//                    UpdateMousePos(
//                        mousePosX,
//                        mousePosY,
//                        WindowsMessageUtils.GetWheelDelta(wParam)
//                    );
//                    break;
//                case WM_MOUSEMOVE:
//                    mousePosX = WindowsMessageUtils.GetXParam(lParam);
//                    mousePosY = WindowsMessageUtils.GetYParam(lParam);
//                    UpdateMousePos(mousePosX, mousePosY, 0);
//                    break;
//            }
//
//            if (msg != WM_KEYDOWN)
//            {
//                UpdateMousePos(mousePosX, mousePosY, 0);
//            }
//
//            // Previously, ToEE called a global window proc here but it did nothing useful.
//            return DefWindowProc(hWnd, msg, wParam, lParam);
//        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

        private static int IDC_ARROW = 32512;

    }
}
