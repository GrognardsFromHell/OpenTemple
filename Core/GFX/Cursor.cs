using System;
using System.Runtime.InteropServices;

namespace OpenTemple.Core.GFX;

public static class Cursor
{
    public static void HideCursor(IntPtr windowHandle)
    {
        SetCursor(windowHandle, IntPtr.Zero);
    }

    public static void SetCursor(IntPtr windowHandle, IntPtr handle)
    {
        if (IntPtr.Size == 8)
        {
            SetClassLongPtr64(windowHandle, GCL_HCURSOR, handle);
        }
        else
        {
            SetClassLongPtr32(windowHandle, GCL_HCURSOR, handle);
        }

        SetCursor(handle);
    }

    private const int GCL_HCURSOR = -12;

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
    private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetClassLong")]
    private static extern IntPtr SetClassLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr handle);
}