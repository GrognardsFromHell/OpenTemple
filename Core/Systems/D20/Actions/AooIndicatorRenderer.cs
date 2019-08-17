using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    /// <summary>
    /// This was previously part of the PathXRenderer.
    /// </summary>
    public class AooIndicatorRenderer
    {
        [TempleDllLocation(0x10106f30)]
        [TempleDllLocation(0x10106f30)]
        public void Render(LocAndOffsets location, ITexture texture)
        {
            var screenPos = Tig.RenderingDevice.GetCamera().WorldToScreenUi(location.ToInches3D());

            var contentRect = texture.GetContentRect();
            var renderArgs = new Render2dArgs
            {
                srcRect = contentRect,
                flags = Render2dFlag.BUFFERTEXTURE,
                destRect = new Rectangle(
                    (int) (screenPos.X - contentRect.Width / 2),
                    (int) (screenPos.Y - contentRect.Height / 2),
                    contentRect.Width,
                    contentRect.Height
                ),
                customTexture = texture
            };
            Tig.ShapeRenderer2d.DrawRectangle(ref renderArgs);
        }
    }
}