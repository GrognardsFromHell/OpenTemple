using System;
using System.Drawing;
using System.Threading;
using ImGuiNET;
using OpenTemple.Core.Config;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core
{
    public sealed class GameLoop : IDisposable
    {
        private RenderingConfig _config;

        private readonly RenderingDevice _device;

        private readonly ShapeRenderer2d _shapeRenderer2d;

        private readonly GameRenderer _gameRenderer;

        private readonly DebugUiSystem _debugUiSystem;

        private MessageQueue _messageQueue;

        private bool _quit;

        private GameView _gameView;

        public readonly object _globalMonitor = new object();

        private ResourceRef<RenderTargetTexture> mSceneColor;

        private ResourceRef<RenderTargetDepthStencil> mSceneDepth;

        private Size _renderingSize;

        private readonly int _resizeListener;

        public void AcquireGlobalLock()
        {
            Monitor.Enter(_globalMonitor);
        }

        public void ReleaseGlobalLock()
        {
            Monitor.Exit(_globalMonitor);
        }

        public GameRenderer GameRenderer => _gameRenderer;

        public GameLoop(
            MessageQueue messageQueue,
            RenderingDevice device,
            ShapeRenderer2d shapeRenderer2d,
            RenderingConfig config,
            DebugUiSystem debugUiSystem)
        {
            _messageQueue = messageQueue;
            _config = config.Copy();
            _device = device;
            _shapeRenderer2d = shapeRenderer2d;
            _debugUiSystem = debugUiSystem;

            CreateGpuResources();

            // TODO: We need a different solution for this
            var size = mSceneColor.Resource.GetSize();
            _gameView = new GameView(Tig.MainWindow, size, size);
            Globals.MainGameView = _gameView;

            _gameRenderer = new GameRenderer(Tig.RenderingDevice, _gameView);

            Globals.GameLoop = this;

            _resizeListener = device.AddResizeListener(Resize);
        }

        private void Resize(int w, int h)
        {
            CreateGpuResources();
            // Currently this is the same, later, the render resolution would have to update via percentage of the
            // actual size.
            _gameView.SetRenderResolution(w, h);
            _gameView.SetSize(w, h);
        }

        private void CreateGpuResources()
        {
            mSceneColor.Dispose();
            mSceneDepth.Dispose();

            // Create the buffers for the scaled game view
            var renderSize = _device.GetCamera().ScreenSize;
            mSceneColor = _device.CreateRenderTargetTexture(
                BufferFormat.A8R8G8B8, renderSize.Width, renderSize.Height, _config.IsAntiAliasing
            );
            mSceneDepth = _device.CreateRenderTargetDepthStencil(
                renderSize.Width, renderSize.Height, _config.IsAntiAliasing
            );
            _renderingSize = renderSize;
        }

        public void TakeScreenshot(string filename, int width, int height, int quality = 90)
        {
            _device.TakeScaledScreenshot(mSceneColor.Resource, filename,
                width, height, quality);
        }

        public void Run()
        {
            // Run console commands from "startup.txt" (working dir)
            Tig.DynamicScripting.RunStartupScripts();

            while (!_quit)
            {
                AcquireGlobalLock();
                try
                {
                    Tig.SystemEventPump.PumpSystemEvents();

                    try
                    {
                        ProcessMessages();
                    }
                    catch (Exception e)
                    {
                        ErrorReporting.ReportException(e);
                    }

                    RenderFrame();

                    GameSystems.AdvanceTime();

                    UiSystems.AdvanceTime();
                }
                finally
                {
                    ReleaseGlobalLock();
                }

                Thread.Sleep(1);
            }
        }

        private void ProcessMessages()
        {
            // Why does it process msgs AFTER rendering???
            while (_messageQueue.Process(out var msg))
            {
                if (msg.type == MessageType.EXIT)
                {
                    _quit = true;
                    return;
                }

                // Pressing the F10 key toggles the diag screen
                if (msg.type == MessageType.KEYSTATECHANGE)
                {
                    var keyArgs = msg.KeyStateChangeArgs;
                    if (keyArgs.key == DIK.DIK_F10 && keyArgs.down)
                    {
                        // TODO mDiagScreen->Toggle();
                        // TODO UIShowDebug();
                        Stub.TODO();
                    }
                }

                // I have not found any place where message type 7 is queued,
                // so i removed the out of place re-rendering of the game frame

                if (!UiSystems.MainMenu.IsVisible())
                {
                    UiSystems.InGame.HandleMessage(msg);
                }

                var sceneIdx = UiSystems.InGame.GetActiveSceneIdx();
                if (UiSystems.InGame.IsMouseScrollingEnabled(sceneIdx))
                {
                    DoMouseScrolling();
                }
            }
        }

        public void RenderFrame()
        {
            // Recreate the render targets if AA changed
            if (_config.IsAntiAliasing != mSceneColor.Resource.IsMultiSampled)
            {
                CreateGpuResources();
            }

            using var _ = _device.CreatePerfGroup("Game Loop Rendering");

            _debugUiSystem.NewFrame();

            _device.BeginFrame();

            // Clear the backbuffer
            _device.PushRenderTarget(mSceneColor, mSceneDepth);

            _device.ClearCurrentColorTarget(new LinearColorA(0f, 0f, 0f, 1));
            _device.ClearCurrentDepthTarget();

            try
            {
                _gameRenderer.Render();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            // Reset the render target
            _device.PopRenderTarget();

            _device.BeginPerfGroup("Draw Scaled Scene");

            // Copy from the actual render target to the back buffer and scale / position accordingly
            var destRect = new Rectangle(Point.Empty, _device.GetCamera().ScreenSize);
            var srcRect = new Rectangle(Point.Empty, _renderingSize);
            srcRect.FitInto(destRect);

            SamplerType2d samplerType = SamplerType2d.CLAMP;
            if (!_config.IsUpscaleLinearFiltering)
            {
                samplerType = SamplerType2d.POINT;
            }

            _shapeRenderer2d.DrawRectangle(
                srcRect,
                mSceneColor.Resource,
                PackedLinearColorA.White,
                samplerType
            );

            _device.BeginPerfGroup("UI");
            try
            {
                Globals.UiManager.Render();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            _device.EndPerfGroup();

            // TODO mDiagScreen.Render();

            try
            {
                Tig.Mouse.DrawTooltip();
                Tig.Mouse.DrawItemUnderCursor();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            // Render "GFade" overlay
            if (GameSystems.GFade.IsOverlayEnabled)
            {
                var w = _device.GetCamera().GetScreenWidth();
                var h = _device.GetCamera().GetScreenHeight();
                var color = GameSystems.GFade.OverlayColor;
                Tig.ShapeRenderer2d.DrawRectangle(0, 0, w, h, null, color);
            }

            // Render the Debug UI
            _debugUiSystem.Render();

            _device.EndPerfGroup();

            _device.Present();

            _device.EndPerfGroup();
        }

        private TimePoint _lastScrolling;

        private static readonly TimeSpan ScrollButterDelay = TimeSpan.FromMilliseconds(16);

        [TempleDllLocation(0x10001010)]
        private void DoMouseScrolling()
        {
            var config = Globals.Config.Window;
            if (config.Windowed && Tig.Mouse.MouseOutsideWndGet())
            {
                return;
            }

            var now = TimePoint.Now;
            if (_lastScrolling.Time != 0 && now - _lastScrolling < ScrollButterDelay)
            {
                if (!Globals.Config.ScrollAcceleration)
                {
                    return;
                }
            }

            _lastScrolling = now;

            Point mousePt = Tig.Mouse.GetPos();
            Point mmbRef = Tig.Mouse.GetMmbReference();
            ScrollDirection? scrollDir = null;

            if (mmbRef.X != -1 && mmbRef.Y != -1)
            {
                int dx = mousePt.X - mmbRef.X;
                int dy = mousePt.Y - mmbRef.Y;
                if (dx * dx + dy * dy >= 60)
                {
                    if (Math.Abs(dy) > 1.70 * Math.Abs(dx)) // vertical
                    {
                        if (dy > 0)
                            scrollDir = ScrollDirection.DOWN;
                        else
                            scrollDir = ScrollDirection.UP;
                    }
                    else if (Math.Abs(dx) > 1.70 * Math.Abs(dy)) // horizontal
                    {
                        if (dx > 0)
                            scrollDir = ScrollDirection.RIGHT;
                        else
                            scrollDir = ScrollDirection.LEFT;
                    }
                    else // diagonal
                    {
                        if (dx > 0)
                        {
                            if (dy > 0)
                                scrollDir = ScrollDirection.DOWN_RIGHT;
                            else
                                scrollDir = ScrollDirection.UP_RIGHT;
                        }
                        else
                        {
                            if (dy > 0)
                                scrollDir = ScrollDirection.DOWN_LEFT;
                            else
                                scrollDir = ScrollDirection.UP_LEFT;
                        }
                    }
                }
            }

            if (scrollDir.HasValue)
            {
                GameSystems.Scroll.SetScrollDirection(scrollDir.Value);
                return;
            }

            int scrollMarginV = 2;
            int scrollMarginH = 3;
            if (config.Windowed)
            {
                scrollMarginV = 7;
                scrollMarginH = 7;
            }

            // TODO This should be the size of the game view
            var size = mSceneColor.Resource.GetSize();
            var renderWidth = size.Width;
            var renderHeight = size.Height;

            if (mousePt.X <= scrollMarginH) // scroll left
            {
                if (mousePt.Y <= scrollMarginV) // scroll upper left
                    scrollDir = ScrollDirection.UP_LEFT;
                else if (mousePt.Y >= renderHeight - scrollMarginV) // scroll bottom left
                    scrollDir = ScrollDirection.DOWN_LEFT;
                else
                    scrollDir = ScrollDirection.LEFT;
            }
            else if (mousePt.X >= renderWidth - scrollMarginH) // scroll right
            {
                if (mousePt.Y <= scrollMarginV) // scroll top right
                    scrollDir = ScrollDirection.UP_RIGHT;
                else if (mousePt.Y >= renderHeight - scrollMarginV) // scroll bottom right
                    scrollDir = ScrollDirection.DOWN_RIGHT;
                else
                    scrollDir = ScrollDirection.RIGHT;
            }
            else // scroll vertical only
            {
                if (mousePt.Y <= scrollMarginV) // scroll up
                    scrollDir = ScrollDirection.UP;
                else if (mousePt.Y >= renderHeight - scrollMarginV) // scroll down
                    scrollDir = ScrollDirection.DOWN;
            }

            if (scrollDir.HasValue)
            {
                GameSystems.Scroll.SetScrollDirection(scrollDir.Value);
            }
        }

        public void Dispose()
        {
            mSceneColor.Dispose();
            mSceneDepth.Dispose();
            _gameRenderer.Dispose();
            _gameView.Dispose();
            _device.RemoveResizeListener(_resizeListener);
        }

        // When the anti-aliasing mode changes, we have to re-create the buffers
        public void UpdateConfig(RenderingConfig configRendering)
        {
            if (_config.IsAntiAliasing != configRendering.IsAntiAliasing
                || _config.MSAAQuality != configRendering.MSAAQuality
                || _config.MSAASamples != configRendering.MSAASamples)
            {
                _config = configRendering.Copy();
                CreateGpuResources();
            }
        }
    }
}