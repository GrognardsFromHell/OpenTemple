using System;
using System.Threading;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core;

public sealed class GameLoop
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly EventLoop _eventLoop;

    private readonly RenderingDevice _device;

    private readonly IDebugUI _debugUiSystem;

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
        EventLoop eventLoop,
        RenderingDevice device,
        IDebugUI debugUiSystem)
    {
        _eventLoop = eventLoop;
        _device = device;
        _debugUiSystem = debugUiSystem;

        Globals.GameLoop = this;
    }

    public void Run()
    {
        // Run console commands from "startup.txt" (working dir)
        Tig.DynamicScripting.RunStartupScripts();

        _eventLoop.OnQuit += Stop;
        
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
            _eventLoop.Tick();

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
        _eventLoop.OnQuit -= Stop;
        _quit = true;
        Logger.Info("Stopping game loop");
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
                // Update of game view must be up-to-date...
                renderableGameView.EnsureLayoutIsUpToDate();
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
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }

        _device.EndDraw();
        _device.EndPerfGroup();

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