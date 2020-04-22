using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Security;
using OpenTemple.Core;
using OpenTemple.Core.Utils;

namespace QtInterop
{
    [SuppressUnmanagedCodeSecurity]
    public abstract class QObjectBase : QGadgetBase
    {
        private static readonly DelegateSlotFree _releaseDelegate;

        static QObjectBase()
        {
            _releaseDelegate = HandleSlotRelease;
            DelegateSlotObject_setCallbacks(_releaseDelegate);
        }

        private static void HandleSlotRelease(GCHandle delegateHandle)
        {
            delegateHandle.Free();
        }

        protected QObjectBase(IntPtr handle) : base(handle)
        {
            Console.WriteLine();
        }

        protected static IntPtr GetMetaObjectByQmlSourceUrl(IntPtr exampleInstance, string sourceUrl)
        {
            if (!QMetaType_resolveQmlType(exampleInstance, sourceUrl, out var metaTypeId, out var metaObject))
            {
                throw new ArgumentException("Failed to resolve QML type from source url " + sourceUrl);
            }

            return metaObject;
        }

        protected void AddSignalHandler(int signalIndex, Delegate @delegate, DelegateSlotCallback dispatcher)
        {
            var delegateHandle = GCHandle.Alloc(@delegate);
            if (!QObject_connect(_handle, signalIndex, delegateHandle, dispatcher))
            {
                delegateHandle.Free();
                throw new InvalidOperationException();
            }
        }

        protected void RemoveSignalHandler(int signalIndex, Delegate @delegate)
        {
            throw new NotSupportedException();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected unsafe delegate void DelegateSlotCallback(GCHandle delegateHandle, void** args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelegateSlotFree(GCHandle delegateHandle);

        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Unicode)]
        private static extern bool DelegateSlotObject_setCallbacks(
            DelegateSlotFree release
        );

        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool
            QObject_connect(IntPtr instance, int signalIndex, GCHandle delegateHandle, DelegateSlotCallback dispatcher);

        // Reads the UTF-16 string from a pointer to a QString instance
        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Unicode)]
        protected static extern unsafe string QString_read(void* instance);

        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Unicode)]
        private static extern bool
            QMetaType_resolveQmlType(IntPtr exampleInstance, string sourceUrl, out int metaTypeId,
                out IntPtr metaObject);
    }

    [SuppressUnmanagedCodeSecurity]
    public abstract class QGadgetBase
    {
        protected IntPtr _handle;

        protected QGadgetBase(IntPtr handle)
        {
            _handle = handle;
        }

        protected unsafe void SetPropertyQString(int index, string value)
        {
            ReadOnlySpan<char> strval = value;
            fixed (char* strptr = strval)
            {
                if (!QObject_setPropertyQString(_handle, index, strptr, strval.Length))
                {
                    throw new InvalidOperationException("Couldn't set property " + index);
                }
            }
        }

        protected unsafe string GetPropertyQString(int index)
        {
            if (!QObject_getPropertyQString(_handle, index, out var strptr, out var strlen))
            {
                throw new InvalidOperationException("Couldn't get property " + index);
            }

            if (strptr == null)
            {
                return null;
            }

            return new string(strptr, 0, strlen);
        }

        protected static IntPtr GetMetaObjectByClassName(string className)
        {
            if (!QMetaType_resolveCppType(className + "*", out var metaTypeId, out var metaObject))
            {
                throw new ArgumentException("Failed to resolve Qt type " + className);
            }

            return metaObject;
        }

        protected static void FindMetaObjectProperty(IntPtr metaObject, string name, out int propertyIndex)
        {
            propertyIndex = QMetaObject_indexOfProperty(metaObject, name);

            if (propertyIndex < 0)
            {
                throw new ArgumentException("Failed to find property " + name);
            }
        }

        protected static void FindMetaObjectMethod(IntPtr metaObject, string name, out int methodIndex)
        {
            methodIndex = QMetaObject_indexOfMethod(metaObject, name);

            if (methodIndex < 0)
            {
                throw new ArgumentException("Failed to find method " + name);
            }
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(NativeMainWindow.DllName)]
        private static extern unsafe bool QObject_setPropertyQString(IntPtr target, int idx, char* value, int len);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(NativeMainWindow.DllName)]
        private static extern unsafe bool QObject_getPropertyQString(IntPtr target, int idx, out char* value,
            out int len);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(NativeMainWindow.DllName)]
        private static extern unsafe bool QObject_setProperty(IntPtr target, int idx, void* value);

        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Ansi)]
        private static extern bool
            QMetaType_resolveCppType(string className, out int metaTypeId, out IntPtr metaObject);

        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Ansi)]
        private static extern int
            QMetaObject_indexOfProperty(IntPtr metaObject, string name);

        [DllImport(NativeMainWindow.DllName, CharSet = CharSet.Ansi)]
        private static extern int
            QMetaObject_indexOfMethod(IntPtr metaObject, string name);
    }
}