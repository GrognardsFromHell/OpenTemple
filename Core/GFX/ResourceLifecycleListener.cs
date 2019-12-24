using System;

namespace OpenTemple.Core.GFX
{
    public interface IResourceLifecycleListener
    {
        void CreateResources(RenderingDevice device);
        void FreeResources(RenderingDevice device);
    }

    public sealed class ResourceLifecycleCallbacks : IResourceLifecycleListener, IDisposable
    {
        private RenderingDevice _device;
        private Action<RenderingDevice> _createCallback;
        private Action<RenderingDevice> _freeCallback;

        public ResourceLifecycleCallbacks(
            RenderingDevice device,
            Action<RenderingDevice> createCallback,
            Action<RenderingDevice> freeCallback)
        {
            _device = device;
            _createCallback = createCallback;
            _freeCallback = freeCallback;
            _device.AddResourceListener(this);
        }

        public void CreateResources(RenderingDevice device)
        {
            _createCallback(device);
        }

        public void FreeResources(RenderingDevice device)
        {
            _freeCallback(device);
        }

        public void Dispose()
        {
            if (_device != null)
            {
                _device.RemoveResourceListener(this);
                _device = null;
                _createCallback = null;
                _freeCallback = null;
            }
        }
    }
}