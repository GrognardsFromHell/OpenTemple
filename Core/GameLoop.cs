using System;
using System.Drawing;
using System.Threading;
using ImGuiNET;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.DebugUI;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;

namespace SpicyTemple.Core
{
    public sealed class GameLoop : IDisposable
    {
        private readonly RenderingConfig _config;

        private readonly RenderingDevice _device;

        private readonly ShapeRenderer2d _shapeRenderer2d;

        private readonly GameRenderer _gameRenderer;

        private readonly DebugUISystem _debugUiSystem;

        private MessageQueue _messageQueue;

        private bool _quit;

        private GameView _gameView;

        public GameLoop(
            MessageQueue messageQueue,
            RenderingDevice device,
            ShapeRenderer2d shapeRenderer2d,
            RenderingConfig config,
            DebugUISystem debugUiSystem)
        {
            _messageQueue = messageQueue;
            _config = config;
            _device = device;
            _shapeRenderer2d = shapeRenderer2d;
            _debugUiSystem = debugUiSystem;

            CreateGpuResources();

            // TODO: We need a different solution for this
            var size = mSceneColor.Resource.GetSize();
            _gameView = new GameView(Tig.RenderingDevice, Tig.MainWindow, size.Width, size.Height);

            _gameRenderer = new GameRenderer(Tig.RenderingDevice, _gameView);

        }

        private void CreateGpuResources()
        {
            mSceneColor.Dispose();
            mSceneDepth.Dispose();

            // Create the buffers for the scaled game view
            var renderSize = _config.GetRenderResolution();
            mSceneColor = _device.CreateRenderTargetTexture(
                BufferFormat.A8R8G8B8, renderSize.Width, renderSize.Height, _config.IsAntiAliasing
            );
            mSceneDepth = _device.CreateRenderTargetDepthStencil(
                renderSize.Width, renderSize.Height, _config.IsAntiAliasing
            );
        }

        public void Run()
        {
            while (!_quit)
            {
                Tig.SystemEventPump.PumpSystemEvents();

                ProcessMessages();

                RenderFrame();

                // TODO GameSystems.AdvanceTime();

                Thread.Sleep(10);
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
                if (msg.type == MessageType.KEYSTATECHANGE
                    && msg.arg1 == 0x44
                    && msg.arg2 == 1)
                {
                    // TODO mDiagScreen->Toggle();
                    // TODO UIShowDebug();
                }

                // I have not found any place where message type 7 is queued,
                // so i removed the out of place re-rendering of the game frame

                // TODO if (!uiSystems->GetMM().IsVisible()) {
                // mainLoop.InGameHandleMessage(msg);
                // }

                var unk = UiSystems.InGame.sub_10113CD0();
                if (UiSystems.InGame.sub_10113D40(unk) != 0)
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
            _device.ClearCurrentColorTarget(new LinearColorA(1, 0, 0, 1));
            _device.ClearCurrentDepthTarget();

            _device.PushRenderTarget(mSceneColor, mSceneDepth);

            _device.ClearCurrentColorTarget(new LinearColorA(0.3f, 0.3f, 0.3f, 1));
            _device.ClearCurrentDepthTarget();

            _gameRenderer.Render();

            _device.BeginPerfGroup("UI");
            Globals.UiManager.Render();
            _device.EndPerfGroup();

            // TODO mDiagScreen.Render();

            // TODO mouseFuncs.InvokeCursorDrawCallback();
            // TODO mouseFuncs.DrawItemUnderCursor(); // This draws dragged items

            // Reset the render target
            _device.PopRenderTarget();

            _device.BeginPerfGroup("Draw Scaled Scene");

            // Copy from the actual render target to the back buffer and scale / position accordingly
            var destRect = new ContentRect(
                0,
                0,
                (int) _device.GetCamera().GetScreenWidth(),
                (int) _device.GetCamera().GetScreenHeight()
            );

            var srcRect = new ContentRect(0, 0, _config.GetRenderResolution());
            srcRect.FitInto(destRect);

            SamplerType2d samplerType = SamplerType2d.CLAMP;
            if (!_config.IsUpscaleLinearFiltering)
            {
                samplerType = SamplerType2d.POINT;
            }

            _shapeRenderer2d.DrawRectangle(
                srcRect.x,
                srcRect.y,
                srcRect.width,
                srcRect.height,
                mSceneColor.Resource,
                0xFFFFFFFF,
                samplerType
            );

            // TODO tig.GetConsole().Render();

            // Render the Debug UI
            _debugUiSystem.Render();

            // Render "GFade" overlay
            /* TODO static auto& gfadeEnabled = temple.GetRef<BOOL>(0x10D25118);
            static auto& gfadeColor = temple.GetRef<XMCOLOR>(0x10D24A28);
            if (gfadeEnabled) {
                auto w = (float) _device.GetCamera().GetScreenWidth();
                auto h = (float)_device.GetCamera().GetScreenHeight();
                tig.GetShapeRenderer2d().DrawRectangle(0, 0, w, h, gfadeColor);
            }*/

            _device.EndPerfGroup();

            _device.Present();

            _device.EndPerfGroup();
        }

        private TimePoint _lastScrolling;
        private static readonly TimeSpan ScrollButterDelay = TimeSpan.FromMilliseconds(16);

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
                if (GameSystems.Scroll.ScrollButter == 0)
                {
                    return;
                }
            }

            _lastScrolling = now;

            Point mousePt = Tig.Mouse.GetPos();
            Point mmbRef = Tig.Mouse.GetMmbReference();
            int scrollDir = -1;

            if (mmbRef.X != -1 && mmbRef.Y != -1)
            {
                int dx = mousePt.X - mmbRef.X;
                int dy = mousePt.Y - mmbRef.Y;
                if (dx * dx + dy * dy >= 60)
                {
                    if (Math.Abs(dy) > 1.70 * Math.Abs(dx)) // vertical
                    {
                        if (dy > 0)
                            scrollDir = 4;
                        else
                            scrollDir = 0;
                    }
                    else if (Math.Abs(dx) > 1.70 * Math.Abs(dy)) // horizontal
                    {
                        if (dx > 0)
                            scrollDir = 2;
                        else
                            scrollDir = 6;
                    }
                    else // diagonal
                    {
                        if (dx > 0)
                        {
                            if (dy > 0)
                                scrollDir = 3;
                            else
                                scrollDir = 1;
                        }
                        else
                        {
                            if (dy > 0)
                                scrollDir = 5;
                            else
                                scrollDir = 7;
                        }
                    }
                }
            }

            if (scrollDir != -1)
            {
                GameSystems.Scroll.SetScrollDirection(scrollDir);
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
                    scrollDir = 7;
                else if (mousePt.Y >= renderHeight - scrollMarginV) // scroll bottom left
                    scrollDir = 5;
                else
                    scrollDir = 6;
            }
            else if (mousePt.X >= renderWidth - scrollMarginH) // scroll right
            {
                if (mousePt.Y <= scrollMarginV) // scroll top right
                    scrollDir = 1;
                else if (mousePt.Y >= renderHeight - scrollMarginV) // scroll bottom right
                    scrollDir = 3;
                else
                    scrollDir = 2;
            }
            else // scroll vertical only
            {
                if (mousePt.Y <= scrollMarginV) // scroll up
                    scrollDir = 0;
                else if (mousePt.Y >= renderHeight - scrollMarginV) // scroll down
                    scrollDir = 4;
            }

            if (scrollDir != -1)
            {
                GameSystems.Scroll.SetScrollDirection(scrollDir);
            }
        }

        private ResourceRef<RenderTargetTexture> mSceneColor;
        private ResourceRef<RenderTargetDepthStencil> mSceneDepth;

        public void Dispose()
        {
            mSceneColor.Dispose();
            mSceneDepth.Dispose();
            _gameRenderer.Dispose();
            _gameView.Dispose();
        }
    }
}