using System;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.TigSubsystems
{
    /// <summary>
    /// Maintains a few default textures for scrollbars.
    /// </summary>
    public class TigWftScrollbar : IDisposable
    {
        public readonly ResourceRef<ITexture> arrow_up;
        public readonly ResourceRef<ITexture> arrow_up_click;
        public readonly ResourceRef<ITexture> arrow_down;
        public readonly ResourceRef<ITexture> arrow_down_click;
        public readonly ResourceRef<ITexture> top;
        public readonly ResourceRef<ITexture> top_click;
        public readonly ResourceRef<ITexture> fill;
        public readonly ResourceRef<ITexture> fill_click;
        public readonly ResourceRef<ITexture> bottom;
        public readonly ResourceRef<ITexture> bottom_click;
        public readonly ResourceRef<ITexture> empty;

        public TigWftScrollbar() {
            var textures = Tig.Textures;

            arrow_up = textures.Resolve("art\\scrollbar\\up.tga", false);
            arrow_up_click = textures.Resolve("art\\scrollbar\\up_click.tga", false);
            arrow_down = textures.Resolve("art\\scrollbar\\down.tga", false);
            arrow_down_click = textures.Resolve("art\\scrollbar\\down_click.tga", false);
            top = textures.Resolve("art\\scrollbar\\top.tga", false);
            top_click = textures.Resolve("art\\scrollbar\\top_click.tga", false);
            fill = textures.Resolve("art\\scrollbar\\fill.tga", false);
            fill_click = textures.Resolve("art\\scrollbar\\fill_click.tga", false);
            bottom = textures.Resolve("art\\scrollbar\\bottom.tga", false);
            bottom_click = textures.Resolve("art\\scrollbar\\bottom_click.tga", false);
            empty = textures.Resolve("art\\scrollbar\\empty.tga", false);
        }

        public void Dispose()
        {
            arrow_up.Dispose();
            arrow_up_click.Dispose();
            arrow_down.Dispose();
            arrow_down_click.Dispose();
            top.Dispose();
            top_click.Dispose();
            fill.Dispose();
            fill_click.Dispose();
            bottom.Dispose();
            bottom_click.Dispose();
            empty.Dispose();
        }
    }
}