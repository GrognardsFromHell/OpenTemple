using System;
using System.Runtime.InteropServices;
using OpenTemple.Interop;

namespace OpenTemple.Core.Platform;

/// <summary>
/// SDL Functions specific to the Windows Platform.
/// </summary>
public static class WindowsPlatform
{
    public static void RegisterWindowClass(string name)
    {
        // On Windows we'd like a constant window class name for automation targeting
        // This needs to be called before SDL_Init
        int err;
        try
        {
            err = SDL_RegisterApp(name, 0x1000 /*CS_BYTEALIGNCLIENT*/ | 0x0020 /* CS_OWNDC */, IntPtr.Zero);
        }
        catch (EntryPointNotFoundException)
        {
            return;
        }

        if (err < 0)
        {
            throw new SDLException("Couldn't initialize window class");
        }
    }

    public static void UnregisterWindowClass()
    {
        SDL_UnregisterApp();
    }

    [DllImport(OpenTempleLib.Path, SetLastError = false)]
    private static extern int SDL_RegisterApp([MarshalAs(UnmanagedType.LPUTF8Str)] string name, uint style, IntPtr hInstance);

    [DllImport(OpenTempleLib.Path, SetLastError = false)]
    private static extern void SDL_UnregisterApp();
}
