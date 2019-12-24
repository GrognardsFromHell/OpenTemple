using System;
using System.Drawing;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace OpenTemple.Core.GFX
{
    public class RenderTargetTexture : GpuResource<RenderTargetTexture>, ITexture
    {
        private RenderTargetView mRtView;
        private Texture2D mTexture;
        private Texture2D mResolvedTexture;
        private ShaderResourceView mResourceView;
        private Size mSize;
        private Rectangle mContentRect;
        private bool mMultiSampled;

        internal Texture2D Texture => mTexture;

        internal RenderTargetView RenderTargetView => mRtView;

        public bool IsMultiSampled => mMultiSampled;

        /**
         * Only valid for MSAA targets. Will return the texture used to resolve the multisampling
         * so it can be used as a shader resource.
         */
        public Texture2D ResolvedTexture => mResolvedTexture;

        /**
         * For MSAA targets, this will return the resolvedTexture, while for normal targets,
         * this will just be the texture itself.
         */
        public ShaderResourceView ResourceView => mResourceView;

        public BufferFormat Format { get; }

        public RenderTargetTexture(RenderingDevice device,
            Texture2D texture,
            RenderTargetView rtView,
            Texture2D resolvedTexture,
            ShaderResourceView resourceView,
            Size size,
            bool multisampled) : base()
        {
            mTexture = texture.QueryInterface<Texture2D>(); // Creates our own reference
            mRtView = rtView;
            mResolvedTexture = resolvedTexture;
            mResourceView = resourceView;
            mSize = size;
            mMultiSampled = multisampled;
            mContentRect = new Rectangle(Point.Empty, size);

            var desc = mTexture.Description;
            switch (desc.Format)
            {
                case SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    Format = BufferFormat.A8R8G8B8;
                    break;
                case SharpDX.DXGI.Format.B8G8R8X8_UNorm:
                    Format = BufferFormat.X8R8G8B8;
                    break;
                default:
                    throw new GfxException("Unsupported buffer format: " + desc.Format);
            }
        }

        protected override void FreeResource()
        {
            mRtView?.Dispose();
            mRtView = null;
            mTexture?.Dispose();
            mTexture = null;
            mResolvedTexture?.Dispose();
            mResolvedTexture = null;
            mResourceView?.Dispose();
            mResourceView = null;
        }

        public int GetId()
        {
            throw new System.NotImplementedException("Unsupported operation for render target textures.");
        }

        public string GetName() => "<rt>";

        public Rectangle GetContentRect() => mContentRect;

        public Size GetSize() => mSize;

        public void FreeDeviceTexture()
        {
            mTexture?.Dispose();
            mTexture = null;
            mRtView?.Dispose();
            mRtView = null;
            mResourceView?.Dispose();
            mResourceView = null;
            mResolvedTexture?.Dispose();
            mResolvedTexture = null;
        }

        public ShaderResourceView GetResourceView()
        {
            if (mResourceView == null)
            {
                throw new InvalidOperationException("This render target texture is not suitable" +
                                                    " for use in a shader.");
            }

            return mResourceView;
        }

        public TextureType Type => TextureType.RenderTarget;

        public bool IsValid() => true;
    }
}