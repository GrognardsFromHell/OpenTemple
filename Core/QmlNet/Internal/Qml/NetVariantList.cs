﻿using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Qml.Net.Internal.Qml
{
    internal class NetVariantList : BaseDisposable
    {
        public NetVariantList()
            : this(Interop.NetVariantList.Create())
        {
        }

        public NetVariantList(IntPtr handle, bool ownsHandle = true)
            : base(handle, ownsHandle)
        {
        }

        public int Count => Interop.NetVariantList.Count(Handle);

        public void Add(NetVariant variant)
        {
            Interop.NetVariantList.Add(Handle, variant.Handle);
        }

        public NetVariant Get(int index)
        {
            var result = Interop.NetVariantList.Get(Handle, index);
            if (result == IntPtr.Zero) return null;
            return new NetVariant(result);
        }

        public void Remove(int index)
        {
            Interop.NetVariantList.Remove(Handle, index);
        }

        public void Clear()
        {
            Interop.NetVariantList.Clear(Handle);
        }

        protected override void DisposeUnmanaged(IntPtr ptr)
        {
            Interop.NetVariantList.Destroy(ptr);
        }

        public static NetVariantList From(params NetVariant[] variants)
        {
            var list = new NetVariantList();

            foreach (var variant in variants)
            {
                list.Add(variant);
            }

            return list;
        }
    }

    internal class NetVariantListInterop
    {
        [NativeSymbol(Entrypoint = "net_variant_list_create")]
        public CreateDel Create { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr CreateDel();

        [NativeSymbol(Entrypoint = "net_variant_list_destroy")]
        public DestroyDel Destroy { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DestroyDel(IntPtr list);

        [NativeSymbol(Entrypoint = "net_variant_list_count")]
        public CountDel Count { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CountDel(IntPtr list);

        [NativeSymbol(Entrypoint = "net_variant_list_add")]
        public AddDel Add { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AddDel(IntPtr list, IntPtr variant);

        [NativeSymbol(Entrypoint = "net_variant_list_get")]
        public GetDel Get { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetDel(IntPtr list, int index);

        [NativeSymbol(Entrypoint = "net_variant_list_remove")]
        public RemoveDel Remove { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RemoveDel(IntPtr list, int index);

        [NativeSymbol(Entrypoint = "net_variant_list_clear")]
        public ClearDel Clear { get; set; }

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ClearDel(IntPtr list);
    }
}