using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace QmlBuildTools
{
    public readonly struct MethodParamInfo
    {
        public readonly string Name;

        public readonly TypeRef Type;

        public MethodParamInfo(string name, TypeRef type)
        {
            Name = name;
            Type = type;
        }
    }

    public readonly struct MethodInfo
    {
        private readonly IntPtr _handle;

        public MethodInfo(IntPtr handle)
        {
            _handle = handle;
        }

        public string Name => MethodInfo_name(_handle);

        public string Signature => MethodInfo_signature(_handle);

        public int OverloadIndex => MethodInfo_overloadIndex(_handle);

        public TypeRef ReturnType => new TypeRef(MethodInfo_returnType(_handle));

        public IEnumerable<MethodParamInfo> Params
        {
            get
            {
                var count = MethodInfo_params_size(_handle);
                for (var i = 0; i < count; i++)
                {
                    var name = MethodInfo_params_name(_handle, i);
                    var typeHandle = MethodInfo_params_type(_handle, i);
                    yield return new MethodParamInfo(name, new TypeRef(typeHandle));
                }
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Ansi)]
        private static extern string MethodInfo_name(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Ansi)]
        private static extern string MethodInfo_signature(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr MethodInfo_returnType(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern int MethodInfo_overloadIndex(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern int MethodInfo_params_size(IntPtr handle);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, CharSet = CharSet.Ansi)]
        private static extern string MethodInfo_params_name(IntPtr handle, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr MethodInfo_params_type(IntPtr handle, int index);
    }
}