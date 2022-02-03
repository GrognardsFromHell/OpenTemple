namespace OpenTemple.Core.Platform;

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

    internal static ushort GetWidthParam(long lParam)
    {
        return unchecked((ushort) (lParam & 0xFFFF));
    }

    internal static ushort GetHeightParam(long lParam)
    {
        return unchecked((ushort) ((lParam >> 16) & 0xFFFF));
    }

    public static ushort GetXButton(ulong wParam)
    {
        return unchecked((ushort) ((wParam >> 16) & 0xFFFF));
    }
}