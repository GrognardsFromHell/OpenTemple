using System.Drawing;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace OpenTemple.Core.Platform
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MinMaxInfo
    {
        public Point ptReserved;
        public Point ptMaxSize;
        public Point ptMaxPosition;
        public Point ptMinTrackSize;
        public Point ptMaxTrackSize;
    }
}