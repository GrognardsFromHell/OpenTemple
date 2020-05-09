using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTemple.Core;
using OpenTemple.Interop;
using Qml.Net.Internal.Qml;
using Qml.Net.Internal.Types;

namespace Qml.Net.Internal
{
    internal static class Interop
    {
        static readonly CallbacksImpl DefaultCallbacks = new CallbacksImpl(new DefaultCallbacks());

        static Interop()
        {
            Callbacks = LoadInteropType<CallbacksInterop>();
            NetTypeInfo = LoadInteropType<NetTypeInfoInterop>();
            NetJsValue = LoadInteropType<NetJsValueInterop>();
            NetMethodInfo = LoadInteropType<NetMethodInfoInterop>();
            NetPropertyInfo = LoadInteropType<NetPropertyInfoInterop>();
            NetTypeManager = LoadInteropType<NetTypeManagerInterop>();
            QCoreApplication = LoadInteropType<QCoreApplicationInterop>();
            QQmlApplicationEngine = LoadInteropType<QQmlApplicationEngineInterop>();
            NetVariant = LoadInteropType<NetVariantInterop>();
            NetReference = LoadInteropType<NetReferenceInterop>();
            NetVariantList = LoadInteropType<NetVariantListInterop>();
            NetTestHelper = LoadInteropType<NetTestHelperInterop>();
            NetSignalInfo = LoadInteropType<NetSignalInfoInterop>();
            QResource = LoadInteropType<QResourceInterop>();
            NetDelegate = LoadInteropType<NetDelegateInterop>();
            QQuickStyle = LoadInteropType<QQuickStyleInterop>();
            QtInterop = LoadInteropType<QtInterop>();
            Utilities = LoadInteropType<UtilitiesInterop>();
            QtWebEngine = LoadInteropType<QtWebEngineInterop>();
            QTest = LoadInteropType<QTestInterop>();
            NetQObject = LoadInteropType<NetQObjectInterop>();
            NetQObjectSignalConnection = LoadInteropType<NetQObjectSignalConnectionInterop>();
            QLocale = LoadInteropType<QLocaleInterop>();

            // RuntimeManager.ConfigureRuntimeDirectory may set these environment variables.
            // However, they only really work when called with Qt.PutEnv.
            Qt.PutEnv("QT_PLUGIN_PATH", Environment.GetEnvironmentVariable("QT_PLUGIN_PATH"));
            Qt.PutEnv("QML2_IMPORT_PATH", Environment.GetEnvironmentVariable("QML2_IMPORT_PATH"));

            var cb = DefaultCallbacks.Callbacks();
            Callbacks.RegisterCallbacks(ref cb);
        }

        public static CallbacksInterop Callbacks { get; }

        public static NetTypeInfoInterop NetTypeInfo { get; }

        public static NetMethodInfoInterop NetMethodInfo { get; }

        public static NetPropertyInfoInterop NetPropertyInfo { get; }

        public static NetTypeManagerInterop NetTypeManager { get; }

        public static QCoreApplicationInterop QCoreApplication { get; }

        public static QQmlApplicationEngineInterop QQmlApplicationEngine { get; }

        public static NetVariantInterop NetVariant { get; }

        public static NetReferenceInterop NetReference { get; }

        public static NetVariantListInterop NetVariantList { get; }

        public static NetTestHelperInterop NetTestHelper { get; }

        public static NetSignalInfoInterop NetSignalInfo { get; }

        public static QResourceInterop QResource { get; }

        public static NetDelegateInterop NetDelegate { get; }

        public static NetJsValueInterop NetJsValue { get; }

        public static QQuickStyleInterop QQuickStyle { get; }

        public static QtInterop QtInterop { get; }

        public static UtilitiesInterop Utilities { get; }

        public static QtWebEngineInterop QtWebEngine { get; }

        public static QTestInterop QTest { get; }

        public static NetQObjectInterop NetQObject { get; }

        public static NetQObjectSignalConnectionInterop NetQObjectSignalConnection { get; }

        public static QLocaleInterop QLocale { get; set; }

        private static T LoadInteropType<T>()
            where T : new()
        {
            var result = new T();
            LoadDelegates(result);
            return result;
        }

        private static void LoadDelegates(object o)
        {
            foreach (var property in o.GetType().GetProperties())
            {
                var entryName = property.GetCustomAttributes().OfType<NativeSymbolAttribute>().First().Entrypoint;
                var symbol = GetExportedFunction(entryName);
                property.SetValue(o, Marshal.GetDelegateForFunctionPointer(symbol, property.PropertyType));
            }
        }

        [DllImport(OpenTempleLib.Path, EntryPoint = "get_exported_function")]
        private static extern IntPtr GetExportedFunction([MarshalAs(UnmanagedType.LPStr)]
            string name);
    }
}