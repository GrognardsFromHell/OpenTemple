using System;
using System.Runtime.InteropServices;
using System.Security;

namespace QmlBuildTools
{
    public readonly struct PropInfo
    {
        private readonly IntPtr _handle;

        public PropInfo(IntPtr handle)
        {
            _handle = handle;
        }

        public string Name => PropInfo_name(_handle);

        public bool IsReadable => PropInfo_readable(_handle);

        public bool IsWritable => PropInfo_writable(_handle);

        public bool IsResettable => PropInfo_resettable(_handle);

        public TypeRef Type => new TypeRef(PropInfo_type(_handle));

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string PropInfo_name(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern bool PropInfo_readable(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern bool PropInfo_writable(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern bool PropInfo_resettable(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr PropInfo_type(IntPtr handle);

    }
}