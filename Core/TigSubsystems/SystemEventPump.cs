using System;
using System.Runtime.InteropServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.TigSubsystems
{
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
                ErrorReporting.ReportException(e);
            }

            try
            {
                Tig.Mouse.AdvanceTime();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            try
            {
                ProcessWindowMessages();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            try
            {
                Tig.Sound.ProcessEvents();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }
        }

        [TempleDllLocation(0x101de880)]
        private void ProcessWindowMessages()
        {
            while (PeekMessage(out var msg, IntPtr.Zero, 0, 0, 1))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
            uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }
    }
}