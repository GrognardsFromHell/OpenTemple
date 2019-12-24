using System.Drawing;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.GFX
{
    public class RenderTargetDepthStencil : GpuResource<RenderTargetDepthStencil>
    {
        public Size Size { get; }

        internal DepthStencilView DsView { get; private set; }

        internal Texture2D TextureNew { get; private set; }

        public RenderTargetDepthStencil(RenderingDevice device,
            Texture2D textureNew,
            DepthStencilView dsView,
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