using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Angle;
using Avalonia.OpenGL.Egl;
using OpenTemple.Core.Logging;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using D3D11Device = SharpDX.Direct3D11.Device;
using D2D1Device = SharpDX.Direct2D1.Device;
using D2D1Factory1 = SharpDX.Direct2D1.Factory1;
using D2D1Bitmap1 = SharpDX.Direct2D1.Bitmap1;
using DXGIFactory1 = SharpDX.DXGI.Factory1;
using DXGIDevice1 = SharpDX.DXGI.Device1;
using DXGIAdapter = SharpDX.DXGI.Adapter1;
using DWriteFactory = SharpDX.DirectWrite.Factory1;
using DWriteFontCollection = SharpDX.DirectWrite.FontCollection;
using DWriteTextFormat = SharpDX.DirectWrite.TextFormat;

namespace OpenTemple.Core.GFX
{
    public sealed class DirectXDevices : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public DXGIFactory1 DxgiFactory { get; } = new();
        public DXGIDevice1 DxgiDevice { get; }
        private DXGIAdapter DxgiAdapter { get; }

        public SharpDX.Direct3D11.Device Direct3D11Device { get; }
        public SharpDX.DirectWrite.Factory1 DirectWriteFactory { get; }
        public SharpDX.WIC.ImagingFactory ImagingFactory { get; } = new();
        public DeviceDebug DeviceDebug { get; }

        public bool IsDebug => DeviceDebug != null;

        private List<DisplayDevice> _displayDevices;

        public DirectXDevices(D3D11Device device)
        {
            Logger.Info("Using D3D11 device with feature level {0}", device.FeatureLevel);

            DeviceDebug = device.QueryInterfaceOrNull<DeviceDebug>();
            Direct3D11Device = device;

            DeviceDebug = Direct3D11Device.QueryInterfaceOrNull<DeviceDebug>();

            // Retrieve DXGI device
            DxgiDevice = Direct3D11Device.QueryInterfaceOrNull<DXGIDevice1>();
            if (DxgiDevice == null)
            {
                throw new GfxException("Couldn't retrieve DXGI device from D3D11 device.");
            }

            DxgiAdapter = DxgiDevice.GetParent<DXGIAdapter>();
            DxgiFactory = DxgiAdapter.GetParent<DXGIFactory1>(); // Hang on to the DXGI factory used here

            // Create the D2D factory
            DebugLevel debugLevel;
            if (IsDebug)
            {
                debugLevel = DebugLevel.Information;
                Logger.Info("Creating Direct2D Factory (debug=true).");
            }
            else
            {
                debugLevel = DebugLevel.None;
                Logger.Info("Creating Direct2D Factory (debug=false).");
            }

            // DirectWrite factory
            DirectWriteFactory = new DWriteFactory(SharpDX.DirectWrite.FactoryType.Shared);
        }

        public DirectXDevices(int adapterIdx = 0, bool debugDevice = false) : this(
            CreateDevice(adapterIdx, debugDevice))
        {
        }

        private static D3D11Device CreateDevice(int adapterIdx, bool debugDevice)
        {
            var displayDevices = new DXGIFactory1();

            // Find the adapter selected by the user, although we might fall back to the
            // default one if the user didn't select one or the adapter selection changed
            var dxgiAdapter = displayDevices.GetAdapter1(adapterIdx);
            if (dxgiAdapter == null)
            {
                // Fall back to default
                Logger.Error("Couldn't retrieve adapter #{0}. Falling back to default", 0);
                dxgiAdapter = displayDevices.GetAdapter1(0);
                if (dxgiAdapter == null)
                {
                    throw new GfxException(
                        "Couldn't retrieve your configured graphics adapter, but also couldn't fall back to the default adapter.");
                }
            }

            // Required for the Direct2D support
            var deviceFlags = DeviceCreationFlags.BgraSupport;
            if (debugDevice)
            {
                deviceFlags |=
                    DeviceCreationFlags.Debug | DeviceCreationFlags.DisableGpuTimeout;
            }

            FeatureLevel[] requestedLevels =
            {
                FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0, FeatureLevel.Level_9_3, FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1
            };

            try
            {
                return new SharpDX.Direct3D11.Device(dxgiAdapter, deviceFlags, requestedLevels);
            }
            catch (SharpDXException e)
            {
                // DXGI_ERROR_SDK_COMPONENT_MISSING
                if (debugDevice && e.ResultCode.Code == 0x887A002D)
                {
                    throw new GfxException("To use the D3D debugging feature, you need to " +
                                           "install the corresponding Windows SDK component.");
                }

                throw new GfxException("Unable to create a Direct3D 11 device.", e);
            }
        }

        ~DirectXDevices()
        {
            Dispose();
        }

        private SharpDX.DXGI.Adapter1 GetAdapter(int index) => DxgiFactory.GetAdapter1(index);

        public List<DisplayDevice> DisplayDevices
        {
            get
            {
                // Recreate the DXGI factory if we want to enumerate a new list of devices
                if (_displayDevices != null && DxgiFactory.IsCurrent)
                {
                    return _displayDevices;
                }

                // Enumerate devices
                Logger.Info("Enumerating DXGI display devices...");

                _displayDevices = new List<DisplayDevice>();

                int adapterCount = DxgiFactory.GetAdapterCount1();
                for (int adapterIdx = 0; adapterIdx < adapterCount; adapterIdx++)
                {
                    using var adapter = DxgiFactory.GetAdapter1(adapterIdx);

                    // Get an adapter descriptor
                    var adapterDesc = adapter.Description;

                    var displayDevice = new DisplayDevice();

                    displayDevice.name = adapterDesc.Description;
                    Logger.Info("Adapter #{0} '{1}'", adapterIdx, displayDevice.name);

                    // Enumerate all outputs of the adapter
                    var outputCount = adapter.GetOutputCount();

                    for (int outputIdx = 0; outputIdx < outputCount; outputIdx++)
                    {
                        using var output = adapter.GetOutput(outputIdx);

                        var outputDesc = output.Description;

                        var deviceName = outputDesc.DeviceName;

                        Span<char> monitorName = stackalloc char[128];
                        int monitorNameSize = monitorName.Length;
                        unsafe
                        {
                            fixed (char* monitorNamePtr = monitorName)
                            {
                                if (!Win32_GetMonitorName(outputDesc.MonitorHandle, monitorNamePtr,
                                    ref monitorNameSize))
                                {
                                    Logger.Warn("Failed to determine monitor name.");
                                }
                            }
                        }

                        monitorName = monitorName.Slice(0, monitorNameSize);

                        DisplayDeviceOutput displayOutput = new DisplayDeviceOutput();
                        displayOutput.id = deviceName;
                        displayOutput.name = new string(monitorName);
                        Logger.Info("  Output #{0} Device '{1}' Monitor '{2}'", outputIdx,
                            deviceName, displayOutput.name);
                        displayDevice.outputs.Add(displayOutput);
                    }

                    if (displayDevice.outputs.Count > 0)
                    {
                        _displayDevices.Add(displayDevice);
                    }
                    else
                    {
                        Logger.Info("Skipping device because it has no outputs.");
                    }
                }

                return _displayDevices;
            }
        }

        [DllImport("OpenTemple.Native")]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe bool Win32_GetMonitorName(IntPtr monitorHandle, char* name, ref int nameSize);

        public void Dispose()
        {
            DxgiFactory?.Dispose();
            DxgiDevice?.Dispose();
            DxgiAdapter?.Dispose();
            Direct3D11Device?.Dispose();
            DirectWriteFactory?.Dispose();
            ImagingFactory?.Dispose();
            GC.SuppressFinalize(this);
        }

        public static DirectXDevices FromDirect3D11Device(D3D11Device device) => new(device);
    }
}