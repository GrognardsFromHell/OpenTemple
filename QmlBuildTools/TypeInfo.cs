using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace QmlBuildTools
{
    public enum TypeInfoKind
    {
        CppQObject = 0,
        CppGadget = 1,
        QmlQObject = 2
    }

    public readonly struct TypeInfo
    {
        private readonly IntPtr _handle;

        public TypeInfo(IntPtr handle)
        {
            _handle = handle;
        }

        public TypeInfoKind Kind => TypeInfo_kind(_handle);

        public string MetaClassName => TypeInfo_metaClassName(_handle);

        public string Name => TypeInfo_name(_handle);

        public string QmlModule => TypeInfo_qmlModule(_handle);

        public string QmlSourceUrl => TypeInfo_qmlSourceUrl(_handle);

        public TypeInfo? Parent
        {
            get
            {
                var parentHandle = TypeInfo_parent(_handle);
                TypeInfo? result = null;
                if (parentHandle != IntPtr.Zero)
                {
                    result = new TypeInfo(parentHandle);
                }

                return result;
            }
        }

        public IEnumerable<PropInfo> Props
        {
            get
            {
                var count = TypeInfo_props_size(_handle);
                for (var i = 0; i < count; i++)
                {
                    yield return new PropInfo(TypeInfo_props(_handle, i));
                }
            }
        }

        public IEnumerable<MethodInfo> Signals
        {
            get
            {
                var count = TypeInfo_signals_size(_handle);
                for (var i = 0; i < count; i++)
                {
                    yield return new MethodInfo(TypeInfo_signals(_handle, i));
                }
            }
        }

        public IEnumerable<MethodInfo> Methods
        {
            get
            {
                var count = TypeInfo_methods_size(_handle);
                for (var i = 0; i < count; i++)
                {
                    yield return new MethodInfo(TypeInfo_methods(_handle, i));
                }
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern TypeInfoKind TypeInfo_kind(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string TypeInfo_metaClassName(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string TypeInfo_name(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string TypeInfo_qmlModule(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string TypeInfo_qmlSourceUrl(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr TypeInfo_parent(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern int TypeInfo_props_size(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr TypeInfo_props(IntPtr handle, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern int TypeInfo_signals_size(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr TypeInfo_signals(IntPtr handle, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern int TypeInfo_methods_size(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr TypeInfo_methods(IntPtr handle, int index);
    }
}