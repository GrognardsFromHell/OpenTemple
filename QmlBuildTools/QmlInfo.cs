using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace QmlBuildTools
{
    [SuppressUnmanagedCodeSecurity]
    public sealed class QmlInfo : IDisposable
    {
        private IntPtr _handle;

        public QmlInfo(string basePath)
        {
            _handle = QmlInfo_new(
                NativeDelegate.Create<LoggerDelegate>(Log),
                basePath
            );
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                QmlInfo_delete(_handle);
            }

            _handle = IntPtr.Zero;
        }

        public void LoadFile(string path)
        {
            if (!QmlInfo_addFile(_handle, path))
            {
                throw new ArgumentException(QmlInfo_error(_handle));
            }
        }

        public string Process()
        {
            var generatedCode = new StringWriter();

            void VisitTypeInfo(IntPtr typeLibraryHandle)
            {
                var typeLibrary = new TypeLibrary(typeLibraryHandle);
                try
                {
                    generatedCode.Write(ProxiesGenerator.Generate(typeLibrary.Types));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to generate code: " + e);
                }
            }

            QmlInfo_visitTypeLibrary(_handle, VisitTypeInfo);

            return generatedCode.ToString();
        }

        private void Log(LogLevel level, string message)
        {
            Console.WriteLine(message);
        }

        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern IntPtr QmlInfo_new(NativeDelegate logger, string basePath);

        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string QmlInfo_error(IntPtr qmlInfo);

        [DllImport(NativeLibrary.Path)]
        private static extern void QmlInfo_delete(IntPtr qmlInfo);

        [DllImport(NativeLibrary.Path)]
        private static extern IntPtr QmlInfo_propType(IntPtr qmlInfo, IntPtr propHandle);

        private delegate void TypeInfoVisitor(IntPtr handle);

        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern bool QmlInfo_addFile(
            IntPtr qmlInfo,
            string path);

        [DllImport(NativeLibrary.Path)]
        private static extern bool QmlInfo_visitTypeLibrary(
            IntPtr qmlInfo,
            TypeInfoVisitor visitor);

        [DllImport(NativeLibrary.Path, CharSet = CharSet.Unicode)]
        private static extern string QmlInfo_sourceUrl(IntPtr qmlInfo, int typeId);

        private enum LogLevel
        {
            Warning = 0,
            Error = 1
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private delegate void LoggerDelegate(LogLevel level, string message);
    }
}