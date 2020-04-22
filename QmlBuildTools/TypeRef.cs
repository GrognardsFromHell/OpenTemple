using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace QmlBuildTools
{
    public readonly struct TypeRef
    {
        private readonly IntPtr _handle;

        public TypeRef(IntPtr handle)
        {
            _handle = handle;
        }

        public TypeRefKind Kind => TypeRef_kind(_handle);

        public BuiltInType BuiltIn
        {
            get
            {
                Trace.Assert(Kind == TypeRefKind.BuiltIn);
                return TypeRef_builtIn(_handle);
            }
        }

        public TypeInfo TypeInfo
        {
            get
            {
                Trace.Assert(Kind == TypeRefKind.TypeInfo);
                var handle = TypeRef_typeInfo(_handle);
                if (handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException();
                }

                return new TypeInfo(handle);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern TypeRefKind TypeRef_kind(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern BuiltInType TypeRef_builtIn(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr TypeRef_typeInfo(IntPtr handle);
    }

    public enum TypeRefKind
    {
        Void = 0,
        TypeInfo,
        BuiltIn
    }

    public enum BuiltInType
    {
        Bool = 0,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Double,
        Char,
        String
    }
}