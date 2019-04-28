using System;
using System.Runtime.InteropServices;

namespace SpicyTemple.Core.GFX
{
    public static class Cursor
    {
        public static void HideCursor(IntPtr windowHandle)
        {
            SetCursor(windowHandle, IntPtr.Zero);
        }

        public static void SetCursor(IntPtr windowHandle, IntPtr handle)
        {
            SetClassLongPtr(windowHandle, GCL_HCURSOR, handle);
            SetCursor(handle);
        }

        private const int GCL_HCURSOR = -12;

        [DllImport("user32.dll")]
        private static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr SetCursor(IntPtr handle);
    }
}