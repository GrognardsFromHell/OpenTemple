using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OpenTemple.Core.Utils
{
    /// <summary>
    /// Used for native interop for handling string results from native code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public ref struct StringResult
    {
        private IntPtr _data;

        private int _length;

        private IntPtr _privateData;

        public ReadOnlySpan<byte> Data
        {
            get
            {
                if (_data == IntPtr.Zero || _length == 0)
                {
                    return ReadOnlySpan<byte>.Empty;
                }

                unsafe
                {
                    return new ReadOnlySpan<byte>((void*) _data, _length);
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

                return Encoding.UTF8.GetString(Data);
            }
        }

        public void Dispose()
        {
            if (_privateData != IntPtr.Zero)
            {
                StringResult_Delete(_privateData);
                _privateData = IntPtr.Zero;
                _data = IntPtr.Zero;
                _length = 0;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("OpenTemple.Native")]
        private static extern void StringResult_Delete(IntPtr privateData);
    }
}