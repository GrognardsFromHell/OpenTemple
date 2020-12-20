using System.Drawing;
using Vortice.Direct3D11;

namespace OpenTemple.Core.GFX
{
    public class RenderTargetDepthStencil : GpuResource<RenderTargetDepthStencil>
    {
        public Size Size { get; }

        internal ID3D11DepthStencilView DsView { get; private set; }

        internal ID3D11Texture2D TextureNew { get; private set; }

        public RenderTargetDepthStencil(RenderingDevice device,
            ID3D11Texture2D textureNew,
            ID3D11DepthStencilView dsView,
            Size size) : base()
        {
            Size = size;
            DsView = dsView;
            TextureNew = textureNew;
        }

        protected override void FreeResource()
        {
            DsView?.Dispose();
            DsView = null;

            TextureNew?.Dispose();
            TextureNew = null;
        }
    }
}