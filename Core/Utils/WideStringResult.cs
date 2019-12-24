using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OpenTemple.Core.Utils
{
    /// <summary>
    /// Used for native interop for handling 16-bit string results from native code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public ref struct WideStringResult
    {
        private IntPtr _data;

        private int _length;

        private IntPtr _privateData;

        public ReadOnlySpan<char> Data
        {
            get
            {
                if (_data == IntPtr.Zero || _length == 0)
                {
                    return ReadOnlySpan<char>.Empty;
                }

                unsafe
                {
                    return new ReadOnlySpan<char>((void*) _data, _length);
                }
            }
        }

        public string String
        {
            get
            {
                if (_data == IntPtr.Zero)
                {
                    return null;
                }

                return new string(Data);
            }
        }

        public void Dispose()
        {
            if (_privateData != IntPtr.Zero)
            {
                WideStringResult_Delete(_privateData);
                _privateData = IntPtr.Zero;
                _data = IntPtr.Zero;
                _length = 0;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("OpenTemple.Native")]
        private static extern void WideStringResult_Delete(IntPtr privateData);
    }
}