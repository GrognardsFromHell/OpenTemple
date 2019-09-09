using System;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    class WidgetImage : WidgetContent, IDisposable
    {
        public WidgetImage(string path)
        {
            SetTexture(path);
        }

        public WidgetImage()
        {
        }

        public Rectangle? SourceRect { get; set; }

        public PackedLinearColorA Color { get; set; } = PackedLinearColorA.White;

        public override void Render()
        {
            if (!mTexture.IsValid)
            {
                return;
            }

            var renderer = Tig.ShapeRenderer2d;
            if (SourceRect.HasValue)
            {
                var drawArgs = new Render2dArgs();
                drawArgs.srcRect = SourceRect.Value;
                drawArgs.destRect = mContentArea;
                drawArgs.customTexture = mTexture.Resource;
                drawArgs.flags = Render2dFlag.BUFFERTEXTURE;
                if (Color != PackedLinearColorA.White)
                {
                    drawArgs.flags |= Render2dFlag.VERTEXALPHA|Render2dFlag.VERTEXCOLORS;
                    drawArgs.vertexColors = new[]
                    {
                        Color,
                        Color,
                        Color,
                        Color
                    };
                }
                renderer.DrawRectangle(ref drawArgs);
            }
            else
            {
                renderer.DrawRectangle(
                    mContentArea.X,
                    mContentArea.Y,
                    mContentArea.Width,
                    mContentArea.Height,
                    mTexture.Resource,
                    Color
                );
            }
        }

        public void SetTexture(string path)
        {
            mPath = path;
            mTexture.Dispose();
            if (path != null)
            {
                mTexture = Tig.RenderingDevice.GetTextures().Resolve(path, false);
                if (mTexture.Resource.IsValid())
                {
                    mPreferredSize = mTexture.Resource.GetSize();
                }
            }
        }

        public void SetTexture(ITexture texture)
        {
            mPath = texture.GetName();
            mTexture.Dispose();
            mTexture = texture.Ref();
            if (mTexture.Resource.IsValid())
            {
                mPreferredSize = mTexture.Resource.GetSize();
            }
        }

        private string mPath;

        private ResourceRef<ITexture> mTexture;

        public void Dispose()
        {
            mTexture.Dispose();
        }
    };
}