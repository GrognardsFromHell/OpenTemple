namespace OpenTemple.Core.Platform
{
    internal static class WindowsMessageUtils
    {

        internal static short GetWheelDelta(ulong wParam)
        {
            return unchecked((short) ((wParam >> 16) & 0xFFFF));
        }

        internal static short GetXParam(long lParam)
        {
            return unchecked((short) (lParam & 0xFFFF));
        }
        internal static short GetYParam(long lParam)
        {
            return unchecked((short) ((lParam >> 16) & 0xFFFF));
        }

        // Equivalent to HIWORD macro
        internal static int HiWord(ulong val) => (int) ((val >> 16) & 0xFFFF);

        // Equivalent to LOWORD macro
        internal static int LoWord(ulong val) => (int) (val & 0xFFFF);

        // The left mouse button is down.
        private const int MK_LBUTTON = 0x0001;
        // The middle mouse button is down.
        private const int MK_MBUTTON = 0x0010;
        // The right mouse button is down.
        private const int MK_RBUTTON = 0x0002;
        // The first X button is down.
        private const int MK_XBUTTON1 = 0x0020;
        // The second X button is down.
        private const int MK_XBUTTON2 = 0x0040;

        internal static ushort GetMouseMessagePressedButtons(ulong wParam)
        {
            var modifierKeys = LoWord(wParam);

            ushort buttons = 0;
            if ((modifierKeys & MK_LBUTTON) != 0)
            {
                buttons |= 1;
            }
            if ((modifierKeys & MK_RBUTTON) != 0)
            {
                buttons |= 2;
            }
            if ((modifierKeys & MK_MBUTTON) != 0)
            {
                buttons |= 4;
            }
            if ((modifierKeys & MK_XBUTTON1) != 0)
            {
                buttons |= 8;
            }
            if ((modifierKeys & MK_XBUTTON2) != 0)
            {
                buttons |= 16;
            }

            return buttons;
        }

    }
}