using System;
using System.Drawing;
using System.Threading;
using ImGuiNET;
using OpenTemple.Core.Config;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core
{
    public sealed class GameLoop
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly MessageQueue _messageQueue;

        private readonly RenderingDevice _device;

        private readonly DebugUiSystem _debugUiSystem;

        private volatile bool _quit;

        private readonly object _globalMonitor = new();

        public void AcquireGlobalLock()
        {
            Monitor.Enter(_globalMonitor);
        }

        public void ReleaseGlobalLock()
        {
            Monitor.Exit(_globalMonitor);
        }

        public GameLoop(
            MessageQueue messageQueue,
            RenderingDevice device,
            DebugUiSystem debugUiSystem)
        {
            _messageQueue = messageQueue;
            _device = device;
            _debugUiSystem = debugUiSystem;

            Globals.GameLoop = this;
        }

        public void Run()
        {
            // Run console commands from "startup.txt" (working dir)
            Tig.DynamicScripting.RunStartupScripts();

            while (!_quit)
            {
                RunOneIteration();

                Thread.Sleep(1);
            }
        }

        public void RunOneIteration(bool advanceTime = true)
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

                if (advanceTime)
                {
                    GameSystems.AdvanceTime();

                    UiSystems.AdvanceTime();
                }
            }
            finally
            {
                ReleaseGlobalLock();
            }
        }

        public void Stop()
        {
            _quit = true;
            Logger.Info("Stopping game loop");
        }

        private void ProcessMessages()
        {
            // Why does it process msgs AFTER rendering???
            while (!_quit && _messageQueue.Process(out var msg))
            {
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
            }
        }

        public void RenderFrame()
        {
            using var _ = _device.CreatePerfGroup("Game Loop Rendering");

            _debugUiSystem.NewFrame();

            _device.BeginFrame();

            // Update all game views
            foreach (var gameView in GameViews.AllVisible)
            {
                if (gameView is GameView renderableGameView)
                {
                    renderableGameView.RenderScene();
                }
            }

            _device.BeginPerfGroup("UI");
            _device.BeginDraw();
            try
            {
                Globals.UiManager.Render();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            _device.EndDraw();
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
                var w = Globals.UiManager.CanvasSize.Width;
                var h = Globals.UiManager.CanvasSize.Height;
                var color = GameSystems.GFade.OverlayColor;
                Tig.ShapeRenderer2d.DrawRectangle(0, 0, w, h, null, color);
            }

            // Render the Debug UI
            _debugUiSystem.Render();

            _device.EndPerfGroup();

            _device.Present();

            _device.EndPerfGroup();
        }

    }
}