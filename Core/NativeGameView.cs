using System;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core
{
    public class NativeGameView : IDisposable
    {

        private readonly RenderingDevice _device;

        private readonly GameRenderer _gameRenderer;

        private ResourceRef<RenderTargetTexture> _sceneColor;

        private ResourceRef<RenderTargetDepthStencil> _sceneDepth;

        private Size _size;

        private RenderingConfig _config;

        public NativeGameView(RenderingDevice device, RenderingConfig config)
        {
            _device = device;
            _gameRenderer = new GameRenderer(device, null);
        }

        public Size Size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    _sceneColor.Dispose();
                    _size = value;
                }
            }
        }

        public RenderTargetTexture ColorTarget
        {
            get
            {
                if (!_sceneColor.IsValid)
                {
                    CreateResources();
                }

                return _sceneColor.Resource;
            }
        }

        private void CreateResources()
        {
            _sceneColor.Dispose();
            _sceneDepth.Dispose();

            // Create the buffers for the scaled game view
            if (_size.IsEmpty)
            {
                // Cannot create textures with no pixels
                return;
            }

            _sceneColor = _device.CreateRenderTargetTexture(
                BufferFormat.A8R8G8B8, _size.Width, _size.Height, _config.IsAntiAliasing
            );
            _sceneDepth = _device.CreateRenderTargetDepthStencil(
                _size.Width, _size.Height, _config.IsAntiAliasing
            );
        }

        public void Dispose()
        {
            _sceneColor.Dispose();
            _sceneDepth.Dispose();
        }

        public void Render()
        {
            if (!_sceneColor.IsValid)
            {
                return;
            }

            _device.ResizeBuffers(_size);

            _device.BeginFrame();

            // Clear the backbuffer
            _device.PushRenderTarget(_sceneColor, _sceneDepth);

            _device.ClearCurrentColorTarget(new LinearColorA(0f, 0f, 0f, 1));
            _device.ClearCurrentDepthTarget();

            try
            {
                _gameRenderer.VisibleSize = _size;
                _gameRenderer.Render();
            }
            catch (Exception e)
            {
                ErrorReporting.ReportException(e);
            }

            // Reset the render target
            _device.PopRenderTarget();

            _device.Present();
        }

        public void UpdateConfig(RenderingConfig config)
        {
            _config = config;
            _sceneColor.Dispose();
            _sceneDepth.Dispose();
        }
    }
}