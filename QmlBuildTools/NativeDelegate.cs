using System;
using System.Runtime.InteropServices;
using System.Security;

namespace QmlBuildTools
{
    /// <summary>
    /// Counterpart to managed_delegate.h, which allows us to store a handle to a managed delegate
    /// in native code, without having to hold onto the delegate object in manged code too.
    /// The managed object will be released to the GC as soon as the native reference is gone.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NativeDelegate
    {
        private static readonly NativeDelegateCallbacks Callbacks = new NativeDelegateCallbacks(ReleaseHandle);

        private static void ReleaseHandle(GCHandle handle)
        {
            handle.Free();
        }

        static NativeDelegate()
        {
            SetCallbacks(Callbacks);
        }

        public readonly GCHandle Handle;

        public readonly IntPtr FunctionPointer;

        private NativeDelegate(GCHandle handle, IntPtr functionPointer)
        {
            Handle = handle;
            FunctionPointer = functionPointer;
        }

        public static NativeDelegate Create<TDelegate>(TDelegate dlgt) where TDelegate : notnull
        {
            // Generic delegates will crash
            if (typeof(TDelegate).IsGenericType)
            {
                throw new ArgumentException("Cannot create native delegates for generic delegate types.");
            }

            var handle = GCHandle.Alloc((Delegate) (object) dlgt);
            var functionPointer = Marshal.GetFunctionPointerForDelegate(dlgt);
            return new NativeDelegate(handle, functionPointer);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary.Path, EntryPoint = "managed_delegate_set_callbacks")]
        private static extern void SetCallbacks(in NativeDelegateCallbacks callbacks);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct NativeDelegateCallbacks
    {
        public delegate void ReleaseHandleDelegate(GCHandle handle);

        public readonly ReleaseHandleDelegate ReleaseHandle;

        public NativeDelegateCallbacks(ReleaseHandleDelegate releaseHandle)
        {
            ReleaseHandle = releaseHandle;
        }
    }
}