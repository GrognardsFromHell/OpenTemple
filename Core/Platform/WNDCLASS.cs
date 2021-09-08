using System;
using System.Runtime.InteropServices;

namespace OpenTemple.Core.Platform
{

    [StructLayout(LayoutKind.Sequential)]
    struct WNDCLASSEX
    {
        public int cbSize;
        public ClassStyles style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

}