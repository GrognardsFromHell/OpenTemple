using System.Runtime.InteropServices;

namespace OpenTemple.Core.Platform
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X, Y;
    }
}