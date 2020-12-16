using System.Collections.Generic;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Platform
{
    /// <summary>
    /// Maps from Windows virtual keys (and others...) to <see cref="KeyboardKey"/>.
    /// The mapping is derived from Firefox's mapping found in NativeKeyToDOMKeyName.h
    /// </summary>
    public static class WindowsKeyMapping
    {
        /// <summary>
        /// Direct mappings that always work from VirtualKey to KeyboardKey.
        /// </summary>
        private static readonly Dictionary<VirtualKey, KeyboardKey> VirtualKeyMapping = new()
        {
            {VirtualKey.VK_MENU, KeyboardKey.Alt},
            {VirtualKey.VK_LMENU, KeyboardKey.Alt},
            {VirtualKey.VK_RMENU, KeyboardKey.Alt},
            {VirtualKey.VK_CAPITAL, KeyboardKey.CapsLock},
            {VirtualKey.VK_CONTROL, KeyboardKey.Control},
            {VirtualKey.VK_LCONTROL, KeyboardKey.Control},
            {VirtualKey.VK_RCONTROL, KeyboardKey.Control},
            {VirtualKey.VK_NUMLOCK, KeyboardKey.NumLock},
            {VirtualKey.VK_LWIN, KeyboardKey.Meta},
            {VirtualKey.VK_RWIN, KeyboardKey.Meta},
            {VirtualKey.VK_SCROLL, KeyboardKey.ScrollLock},
            {VirtualKey.VK_SHIFT, KeyboardKey.Shift},
            {VirtualKey.VK_LSHIFT, KeyboardKey.Shift},
            {VirtualKey.VK_RSHIFT, KeyboardKey.Shift},
            {VirtualKey.VK_RETURN, KeyboardKey.Enter},
            {VirtualKey.VK_TAB, KeyboardKey.Tab},
            {VirtualKey.VK_DOWN, KeyboardKey.ArrowDown},
            {VirtualKey.VK_LEFT, KeyboardKey.ArrowLeft},
            {VirtualKey.VK_RIGHT, KeyboardKey.ArrowRight},
            {VirtualKey.VK_UP, KeyboardKey.ArrowUp},
            {VirtualKey.VK_END, KeyboardKey.End},
            {VirtualKey.VK_HOME, KeyboardKey.Home},
            {VirtualKey.VK_NEXT, KeyboardKey.PageDown},
            {VirtualKey.VK_PRIOR, KeyboardKey.PageUp},
            {VirtualKey.VK_BACK, KeyboardKey.Backspace},
            {VirtualKey.VK_CLEAR, KeyboardKey.Clear},
            {VirtualKey.VK_OEM_CLEAR, KeyboardKey.Clear},
            {VirtualKey.VK_CRSEL, KeyboardKey.CrSel},
            {VirtualKey.VK_DELETE, KeyboardKey.Delete},
            {VirtualKey.VK_EREOF, KeyboardKey.EraseEof},
            {VirtualKey.VK_EXSEL, KeyboardKey.ExSel},
            {VirtualKey.VK_INSERT, KeyboardKey.Insert},
            {VirtualKey.VK_ACCEPT, KeyboardKey.Accept},
            {VirtualKey.VK_CANCEL, KeyboardKey.Cancel},
            {VirtualKey.VK_APPS, KeyboardKey.ContextMenu},
            {VirtualKey.VK_ESCAPE, KeyboardKey.Escape},
            {VirtualKey.VK_EXECUTE, KeyboardKey.Execute},
            {VirtualKey.VK_HELP, KeyboardKey.Help},
            {VirtualKey.VK_PAUSE, KeyboardKey.Pause},
            {VirtualKey.VK_PLAY, KeyboardKey.Play},
            {VirtualKey.VK_SELECT, KeyboardKey.Select},
            {VirtualKey.VK_SNAPSHOT, KeyboardKey.PrintScreen},
            {VirtualKey.VK_SLEEP, KeyboardKey.Standby},
            {VirtualKey.VK_CONVERT, KeyboardKey.Convert},
            {VirtualKey.VK_FINAL, KeyboardKey.FinalMode},
            {VirtualKey.VK_MODECHANGE, KeyboardKey.ModeChange},
            {VirtualKey.VK_NONCONVERT, KeyboardKey.NonConvert},
            {VirtualKey.VK_PROCESSKEY, KeyboardKey.Process},
            {VirtualKey.VK_JUNJA, KeyboardKey.JunjaMode},
            {VirtualKey.VK_F1, KeyboardKey.F1},
            {VirtualKey.VK_F2, KeyboardKey.F2},
            {VirtualKey.VK_F3, KeyboardKey.F3},
            {VirtualKey.VK_F4, KeyboardKey.F4},
            {VirtualKey.VK_F5, KeyboardKey.F5},
            {VirtualKey.VK_F6, KeyboardKey.F6},
            {VirtualKey.VK_F7, KeyboardKey.F7},
            {VirtualKey.VK_F8, KeyboardKey.F8},
            {VirtualKey.VK_F9, KeyboardKey.F9},
            {VirtualKey.VK_F10, KeyboardKey.F10},
            {VirtualKey.VK_F11, KeyboardKey.F11},
            {VirtualKey.VK_F12, KeyboardKey.F12},
            {VirtualKey.VK_F13, KeyboardKey.F13},
            {VirtualKey.VK_F14, KeyboardKey.F14},
            {VirtualKey.VK_F15, KeyboardKey.F15},
            {VirtualKey.VK_F16, KeyboardKey.F16},
            {VirtualKey.VK_F17, KeyboardKey.F17},
            {VirtualKey.VK_F18, KeyboardKey.F18},
            {VirtualKey.VK_F19, KeyboardKey.F19},
            {VirtualKey.VK_F20, KeyboardKey.F20},
            {VirtualKey.VK_F21, KeyboardKey.F21},
            {VirtualKey.VK_F22, KeyboardKey.F22},
            {VirtualKey.VK_F23, KeyboardKey.F23},
            {VirtualKey.VK_F24, KeyboardKey.F24},
            {VirtualKey.VK_MEDIA_PLAY_PAUSE, KeyboardKey.MediaPlayPause},
            {VirtualKey.VK_MEDIA_STOP, KeyboardKey.MediaStop},
            {VirtualKey.VK_MEDIA_NEXT_TRACK, KeyboardKey.MediaTrackNext},
            {VirtualKey.VK_MEDIA_PREV_TRACK, KeyboardKey.MediaTrackPrevious},
            {VirtualKey.VK_VOLUME_DOWN, KeyboardKey.AudioVolumeDown},
            {VirtualKey.VK_VOLUME_UP, KeyboardKey.AudioVolumeUp},
            {VirtualKey.VK_VOLUME_MUTE, KeyboardKey.AudioVolumeMute},
            {VirtualKey.VK_LAUNCH_MAIL, KeyboardKey.LaunchMail},
            {VirtualKey.VK_LAUNCH_MEDIA_SELECT, KeyboardKey.LaunchMediaPlayer},
            {VirtualKey.VK_LAUNCH_APP1, KeyboardKey.LaunchApplication1},
            {VirtualKey.VK_LAUNCH_APP2, KeyboardKey.LaunchApplication2},
            {VirtualKey.VK_BROWSER_BACK, KeyboardKey.BrowserBack},
            {VirtualKey.VK_BROWSER_FAVORITES, KeyboardKey.BrowserFavorites},
            {VirtualKey.VK_BROWSER_FORWARD, KeyboardKey.BrowserForward},
            {VirtualKey.VK_BROWSER_HOME, KeyboardKey.BrowserHome},
            {VirtualKey.VK_BROWSER_REFRESH, KeyboardKey.BrowserRefresh},
            {VirtualKey.VK_BROWSER_SEARCH, KeyboardKey.BrowserSearch},
            {VirtualKey.VK_BROWSER_STOP, KeyboardKey.BrowserStop},
            {VirtualKey.VK_ZOOM, KeyboardKey.ZoomToggle},
        };

        public static KeyboardKey FromVirtualKey(VirtualKey virtualKey)
        {
            return VirtualKeyMapping.GetValueOrDefault(virtualKey, KeyboardKey.Unidentified);
        }
    }
}