using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Schema;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Startup;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using OpenTemple.Interop;
using Qml.Net.Internal.Types;
using QmlFiles;

namespace OpenTemple.Core
{
    /// <summary>
    /// Manages currently active views of the game world. This has to be centralized to allow
    /// game logic to know whether something is currently on screen, and to allow for intelligent
    /// pre-loading and keeping assets cached.
    /// </summary>
    public class GameViews : IDisposable
    {
        static GameViews()
        {
            GameViewsCallbacks.Install();
        }

        private NativeMainWindow _mainWindow;

        private RenderingConfig _config;

        public GameViews(NativeMainWindow mainWindow, RenderingConfig config)
        {
            _mainWindow = mainWindow;
            _config = config.Copy();
            using var netRef = NetReference.CreateForObject(this);
            gameviews_install(_mainWindow.UiHandle, netRef.Handle);
        }

        public List<GameView> Views { get; } = new List<GameView>();

        // When the anti-aliasing mode changes, we have to re-create the buffers
        public void UpdateConfig(RenderingConfig config)
        {
            if (_config.IsAntiAliasing != config.IsAntiAliasing
                || _config.MSAAQuality != config.MSAAQuality
                || _config.MSAASamples != config.MSAASamples)
            {
                _config = config.Copy();
                foreach (var gameView in Views)
                {
                    gameView.UpdateConfig(config);
                }
            }
        }

        public void Render()
        {
            foreach (var view in Views)
            {
                view.Render();
            }
        }

        public GameView CreateView()
        {
            var view = new GameView(Tig.RenderingDevice, _config);
            Views.Add(view);
            return view;
        }

        public void Dispose()
        {
            gameviews_uninstall(_mainWindow.NativeHandle);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OpenTempleLib.Path)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool gameviews_install(IntPtr ui, IntPtr gameViews);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OpenTempleLib.Path)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool gameviews_uninstall(IntPtr ui);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GameViewsCallbacks
    {
        private delegate IntPtr CreateGameViewDelegate(IntPtr gameViewsHandle);

        private delegate void DestroyGameViewDelegate(IntPtr gameView);

        private delegate void SetSizeDelegate(IntPtr handle, int width, int height);

        private delegate bool GetTextureDelegate(IntPtr handle, out IntPtr textureHandle, out int width,
            out int height);

        private NativeDelegate CreateGameView;
        private NativeDelegate DisposeGameView;
        private NativeDelegate GetTexture;
        private NativeDelegate SetSize;

        public static void Install()
        {
            var callbacks = new GameViewsCallbacks
            {
                CreateGameView = NativeDelegate.Create<CreateGameViewDelegate>(
                    (gameViewsRefPtr) =>
                    {
                        var gameViewsRef = new NetReference(gameViewsRefPtr, false);
                        var gameViews = (GameViews) gameViewsRef.Instance;
                        var gameView = gameViews?.CreateView();
                        return NetReference.CreateForObject(gameView, true, false).Handle;
                    }
                ),
                DisposeGameView = NativeDelegate.Create<DestroyGameViewDelegate>(
                    gameViewNetRef =>
                    {
                        var netRef = new NetReference(gameViewNetRef, false);
                        var gameView = (GameView) netRef.Instance;
                        gameView?.Dispose();
                    }
                ),
                GetTexture = NativeDelegate.Create<GetTextureDelegate>(
                    (IntPtr handle, out IntPtr textureHandle, out int width, out int height) =>
                    {
                        var view = (GameView) new NetReference(handle, false).Instance;
                        var renderTarget = view?.ColorTarget;
                        if (renderTarget != null)
                        {
                            var texture = renderTarget.IsMultiSampled
                                ? renderTarget.ResolvedTexture
                                : renderTarget.Texture;
                            textureHandle = texture.NativePointer;
                            width = renderTarget.GetSize().Width;
                            height = renderTarget.GetSize().Height;
                            return true;
                        }
                        else
                        {
                            textureHandle = default;
                            width = default;
                            height = default;
                            return false;
                        }
                    }),
                SetSize = NativeDelegate.Create<SetSizeDelegate>((handle, width, height) =>
                {
                    var view = (GameView) new NetReference(handle, false).Instance;
                    if (view != null)
                    {
                        view.Size = new Size(width, height);
                    }
                })
            };
            gameviews_set_callbacks(callbacks);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OpenTempleLib.Path)]
        private static extern void gameviews_set_callbacks(GameViewsCallbacks callbacks);
    }
}