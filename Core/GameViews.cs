using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Xml.Schema;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Startup;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using OpenTemple.Interop;
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
            gameviews_install(_mainWindow.UiHandle, GCHandle.Alloc(this));
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
        private static extern bool gameviews_install(IntPtr ui, GCHandle gameViews);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(OpenTempleLib.Path)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool gameviews_uninstall(IntPtr ui);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GameViewsCallbacks
    {
        private delegate void DestroyGameViewsDelegate(GCHandle gameViewsHandle);

        private delegate GCHandle CreateGameViewDelegate(GCHandle gameViewsHandle);

        private delegate void DestroyGameViewDelegate(GCHandle gameView);

        private delegate void SetSizeDelegate(GCHandle handle, int width, int height);

        private delegate bool GetTextureDelegate(GCHandle handle, out IntPtr textureHandle, out int width,
            out int height);

        private NativeDelegate DestroyGameViews;
        private NativeDelegate CreateGameView;
        private NativeDelegate DestroyGameView;
        private NativeDelegate GetTexture;
        private NativeDelegate SetSize;

        public static void Install()
        {
            var callbacks = new GameViewsCallbacks
            {
                DestroyGameViews = NativeDelegate.Create<DestroyGameViewsDelegate>(
                    gameViewsHandle => { gameViewsHandle.Free(); }
                ),
                CreateGameView = NativeDelegate.Create<CreateGameViewDelegate>(
                    gameViewsHandle =>
                    {
                        var gameViews = (GameViews) gameViewsHandle.Target;
                        var gameView = gameViews?.CreateView();
                        return gameView != null ? GCHandle.Alloc(gameView) : default;
                    }
                ),
                DestroyGameView = NativeDelegate.Create<DestroyGameViewDelegate>(
                    gameViewHandle =>
                    {
                        var gameView = (OldGameView) gameViewHandle.Target;
                        gameViewHandle.Free();
                        gameView?.Dispose();
                    }
                ),
                GetTexture = NativeDelegate.Create<GetTextureDelegate>(
                    (GCHandle handle, out IntPtr textureHandle, out int width, out int height) =>
                    {
                        var view = (GameView) handle.Target;
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
                    var view = (GameView) handle.Target;
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