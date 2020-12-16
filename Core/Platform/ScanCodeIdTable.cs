using System.Collections.Generic;

namespace OpenTemple.Core.Platform
{
    /// <summary>
    /// Maps Windows ScanCodes to their DOM names.
    /// </summary>
    internal static class ScanCodeIdTable
    {
        /// <summary>
        /// 0xe000 is the bitmask used for extended scan codes.
        /// </summary>
        private static readonly (ushort, string)[] TableEntries =
        {
            (0xe05f, "Sleep"),
            (0xe063, "WakeUp"),
            (0x1e, "KeyA"),
            (0x30, "KeyB"),
            (0x2e, "KeyC"),
            (0x20, "KeyD"),
            (0x12, "KeyE"),
            (0x21, "KeyF"),
            (0x22, "KeyG"),
            (0x23, "KeyH"),
            (0x17, "KeyI"),
            (0x24, "KeyJ"),
            (0x25, "KeyK"),
            (0x26, "KeyL"),
            (0x32, "KeyM"),
            (0x31, "KeyN"),
            (0x18, "KeyO"),
            (0x19, "KeyP"),
            (0x10, "KeyQ"),
            (0x13, "KeyR"),
            (0x1f, "KeyS"),
            (0x14, "KeyT"),
            (0x16, "KeyU"),
            (0x2f, "KeyV"),
            (0x11, "KeyW"),
            (0x2d, "KeyX"),
            (0x15, "KeyY"),
            (0x2c, "KeyZ"),
            (0x2, "Digit1"),
            (0x3, "Digit2"),
            (0x4, "Digit3"),
            (0x5, "Digit4"),
            (0x6, "Digit5"),
            (0x7, "Digit6"),
            (0x8, "Digit7"),
            (0x9, "Digit8"),
            (0xa, "Digit9"),
            (0xb, "Digit0"),
            (0x1c, "Enter"),
            (0x1, "Escape"),
            (0xe, "Backspace"),
            (0xf, "Tab"),
            (0x39, "Space"),
            (0xc, "Minus"),
            (0xd, "Equal"),
            (0x1a, "BracketLeft"),
            (0x1b, "BracketRight"),
            (0x2b, "Backslash"),
            (0x27, "Semicolon"),
            (0x28, "Quote"),
            (0x29, "Backquote"),
            (0x33, "Comma"),
            (0x34, "Period"),
            (0x35, "Slash"),
            (0x3a, "CapsLock"),
            (0x3b, "F1"),
            (0x3c, "F2"),
            (0x3d, "F3"),
            (0x3e, "F4"),
            (0x3f, "F5"),
            (0x40, "F6"),
            (0x41, "F7"),
            (0x42, "F8"),
            (0x43, "F9"),
            (0x44, "F10"),
            (0x57, "F11"),
            (0x58, "F12"),
            (0xe037, "PrintScreen"),
            (0x46, "ScrollLock"),
            (0x45, "Pause"),
            (0xe052, "Insert"),
            (0xe047, "Home"),
            (0xe049, "PageUp"),
            (0xe053, "Delete"),
            (0xe04f, "End"),
            (0xe051, "PageDown"),
            (0xe04d, "ArrowRight"),
            (0xe04b, "ArrowLeft"),
            (0xe050, "ArrowDown"),
            (0xe048, "ArrowUp"),
            (0xe045, "NumLock"),
            (0xe035, "NumpadDivide"),
            (0x37, "NumpadMultiply"),
            (0x4a, "NumpadSubtract"),
            (0x4e, "NumpadAdd"),
            (0xe01c, "NumpadEnter"),
            (0x4f, "Numpad1"),
            (0x50, "Numpad2"),
            (0x51, "Numpad3"),
            (0x4b, "Numpad4"),
            (0x4c, "Numpad5"),
            (0x4d, "Numpad6"),
            (0x47, "Numpad7"),
            (0x48, "Numpad8"),
            (0x49, "Numpad9"),
            (0x52, "Numpad0"),
            (0x53, "NumpadDecimal"),
            (0x56, "IntlBackslash"),
            (0xe05d, "ContextMenu"),
            (0xe05e, "Power"),
            (0x59, "NumpadEqual"),
            (0x64, "F13"),
            (0x65, "F14"),
            (0x66, "F15"),
            (0x67, "F16"),
            (0x68, "F17"),
            (0x69, "F18"),
            (0x6a, "F19"),
            (0x6b, "F20"),
            (0x6c, "F21"),
            (0x6d, "F22"),
            (0x6e, "F23"),
            (0x76, "F24"),
            (0xe03b, "Help"),
            (0xe008, "Undo"),
            (0xe017, "Cut"),
            (0xe018, "Copy"),
            (0xe00a, "Paste"),
            (0xe020, "AudioVolumeMute"),
            (0xe030, "AudioVolumeUp"),
            (0xe02e, "AudioVolumeDown"),
            (0x7e, "NumpadComma"),
            (0x73, "IntlRo"),
            (0x70, "KanaMode"),
            (0x7d, "IntlYen"),
            (0x79, "Convert"),
            (0x7b, "NonConvert"),
            (0x72, "Lang1"),
            (0x71, "Lang2"),
            (0x78, "Lang3"),
            (0x77, "Lang4"),
            (0x1d, "ControlLeft"),
            (0x2a, "ShiftLeft"),
            (0x38, "AltLeft"),
            (0xe05b, "MetaLeft"),
            (0xe01d, "ControlRight"),
            (0x36, "ShiftRight"),
            (0xe038, "AltRight"),
            (0xe05c, "MetaRight"),
            (0xe019, "MediaTrackNext"),
            (0xe010, "MediaTrackPrevious"),
            (0xe024, "MediaStop"),
            (0xe02c, "Eject"),
            (0xe022, "MediaPlayPause"),
            (0xe06d, "MediaSelect"),
            (0xe06c, "LaunchMail"),
            (0xe021, "LaunchApp2"),
            (0xe06b, "LaunchApp1"),
            (0xe065, "BrowserSearch"),
            (0xe032, "BrowserHome"),
            (0xe06a, "BrowserBack"),
            (0xe069, "BrowserForward"),
            (0xe068, "BrowserStop"),
            (0xe067, "BrowserRefresh"),
            (0xe066, "BrowserFavorites"),
        };

        private static readonly Dictionary<ushort, string> ScanCodeIds;

        static ScanCodeIdTable()
        {
            ScanCodeIds = new Dictionary<ushort, string>(TableEntries.Length);
            foreach (var (code, id) in TableEntries)
            {
                ScanCodeIds[code] = id;
            }
        }

        public static string GetId(bool extended, byte scanCode)
        {
            ushort entryKey = scanCode;
            if (extended)
            {
                entryKey |= 0xe000;
            }

            return ScanCodeIds.GetValueOrDefault(entryKey, "");
        }
    }
}