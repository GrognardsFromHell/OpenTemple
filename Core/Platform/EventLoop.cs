using System;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Platform;

public class EventLoop
{
    private readonly IMainWindow _mainWindow;
    private readonly TigKeyboard _keyboard;
    private readonly TigSound _sound;

    public event Action? OnQuit;
    
    public EventLoop(IMainWindow mainWindow, TigKeyboard keyboard, TigSound sound)
    {
        _mainWindow = mainWindow;
        _keyboard = keyboard;
        _sound = sound;
        _mainWindow.Closed += InvokeOnQuit;
    }

    public void Tick()
    {
        try
        {
            _keyboard.Update();
        }
        catch (Exception e)
        {
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }

        try
        {
            _mainWindow.ProcessEvents();
        }
        catch (Exception e)
        {
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }

        try
        {
            _sound.ProcessEvents();
        }
        catch (Exception e)
        {
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }
    }

    private void InvokeOnQuit()
    {
        OnQuit?.Invoke();
    }

    public void Stop()
    {
        InvokeOnQuit();
    }
}