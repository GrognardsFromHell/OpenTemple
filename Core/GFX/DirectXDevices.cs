using System;
using OpenTemple.Core.Logging;
using SharpDX;
using SharpDX.Direct3D;
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
        // ReSharper disable once InconsistentNaming
        private const int DXGI_ERROR_SDK_COMPONENT_MISSING = unchecked((int) 0x887A002D);

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public DXGIFactory1 DxgiFactory { get; } = new();
        public DXGIDevice1 DxgiDevice { get; }
        private DXGIAdapter DxgiAdapter { get; }

        public SharpDX.Direct3D11.Device Direct3D11Device { get; }
        public SharpDX.DirectWrite.Factory1 DirectWriteFactory { get; }
        public SharpDX.WIC.ImagingFactory ImagingFactory { get; } = new();
        public DeviceDebug DeviceDebug { get; }

        public bool IsDebug => DeviceDebug != null;

        public DirectXDevices(bool debugDevice)
        {
            // Required for the Direct2D support
            DeviceCreationFlags deviceFlags = DeviceCreationFlags.BgraSupport;
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
                Direct3D11Device = new D3D11Device(DriverType.Hardware, deviceFlags, requestedLevels);
            }
            catch (SharpDXException e)
            {
                if (debugDevice && e.ResultCode.Code == DXGI_ERROR_SDK_COMPONENT_MISSING)
                {
                    throw new GfxException("To use the D3D debugging feature, you need to " +
                                           "install the corresponding Windows SDK component.");
                }

                throw new GfxException("Unable to create a Direct3D 11 device.", e);
            }

            Logger.Info("Using D3D11 device with feature level {0}", Direct3D11Device.FeatureLevel);

            DeviceDebug = Direct3D11Device.QueryInterfaceOrNull<DeviceDebug>();

            // Retrieve DXGI device
            DxgiDevice = Direct3D11Device.QueryInterfaceOrNull<DXGIDevice1>();
            if (DxgiDevice == null)
            {
                throw new GfxException("Couldn't retrieve DXGI device from D3D11 device.");
            }

            DxgiAdapter = DxgiDevice.GetParent<DXGIAdapter>();
            DxgiFactory = DxgiAdapter.GetParent<DXGIFactory1>(); // Hang on to the DXGI factory used here

            // DirectWrite factory
            DirectWriteFactory = new DWriteFactory(SharpDX.DirectWrite.FactoryType.Shared);
        }

        private static D3D11Device CreateDevice(bool debugDevice)
        {
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
                return new SharpDX.Direct3D11.Device(DriverType.Hardware, deviceFlags, requestedLevels);
            }
            catch (SharpDXException e)
            {
                // DXGI_ERROR_SDK_COMPONENT_MISSING
                if (debugDevice && e.ResultCode.Code == unchecked((int) 0x887A002D))
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
    }
}
