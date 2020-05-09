using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTemple.Interop;

namespace OpenTemple.Core
{
    public static class NativeCompletionSource
    {
        private delegate void AsyncSuccessCallback(GCHandle handle, IntPtr result, int resultSize);

        private delegate void AsyncErrorCallback(GCHandle handle, [MarshalAs(UnmanagedType.LPWStr)]
            string errorMessage);

        private delegate void AsyncCancelCallback(GCHandle handle);

        public static (Task<T>, IntPtr) Create<T>() where T : unmanaged
        {
            var completionSource = new TaskCompletionSource<byte[]>();
            var nativeCompletionSource = completion_source_create(
                GCHandle.Alloc(completionSource),
                SuccessDelegate,
                ErrorDelegate,
                CancelDelegate
            );

            var task = completionSource.Task.ContinueWith(resultTask =>
            {
                var resultData = resultTask.Result;
                return MemoryMarshal.Cast<byte, T>(resultData)[0];
            });

            return (task, nativeCompletionSource);
        }

        private static readonly AsyncSuccessCallback SuccessDelegate = (handle, result, resultSize) =>
        {
            var task = (TaskCompletionSource<byte[]>) handle.Target;
            handle.Free();

            // Make a copy of the value pointed to by result
            byte[] resultData;
            unsafe
            {
                resultData = new ReadOnlySpan<byte>((void*) result, resultSize).ToArray();
            }

            task?.SetResult(resultData);
        };

        private static readonly AsyncErrorCallback ErrorDelegate = (handle, message) =>
        {
            var task = (TaskCompletionSource<byte[]>) handle.Target;
            handle.Free();

            task?.SetException(new Exception(message));
        };

        private static readonly AsyncCancelCallback CancelDelegate = handle =>
        {
            var task = (TaskCompletionSource<byte[]>) handle.Target;
            handle.Free();

            task?.SetCanceled();
        };

        [DllImport(OpenTempleLib.Path)]
        private static extern IntPtr completion_source_create(
            GCHandle handle,
            Delegate successCallback,
            Delegate errorCallback,
            Delegate cancelCallback
        );
    }
}