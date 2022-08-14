using System;
using System.Runtime.InteropServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.TigSubsystems;

public class SystemEventPump
{
    [TempleDllLocation(0x11E74578)] public TimePoint system_events_processed_time { get; private set; }

    [TempleDllLocation(0x101DF440)]
    public void PumpSystemEvents()
    {
        system_events_processed_time = TimePoint.Now;

        Tig.MessageQueue.Enqueue(new Message(MessageType.UPDATE_TIME));

        try
        {
            Tig.Keyboard.Update();
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
            Tig.Mouse.AdvanceTime();
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
            ProcessWindowMessages();
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
            Tig.Sound.ProcessEvents();
        }
        catch (Exception e)
        {
            if (!ErrorReporting.ReportException(e))
            {
                throw;
            }
        }
    }

    [TempleDllLocation(0x101de880)]
    private void ProcessWindowMessages()
    {
        Tig.MainWindow.ProcessEvents();
    }
}