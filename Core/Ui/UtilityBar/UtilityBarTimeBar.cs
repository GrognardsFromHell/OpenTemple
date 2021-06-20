using System.Drawing;
using System.Reflection.Metadata;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.UtilityBar
{
    public class UtilityBarTimeBar : WidgetContainer
    {

        private ResourceRef<ITexture> _timeBarTexture;

        private WidgetTooltipRenderer _tooltipRenderer = new WidgetTooltipRenderer();

        public UtilityBarTimeBar(Rectangle rect) : base(rect)
        {
            _timeBarTexture = Tig.Textures.Resolve("art/interface/utility_bar_ui/timebar.tga", false);

            var arrow = new WidgetImage("art/interface/utility_bar_ui/timebar_arrow.tga");
            arrow.X = 48;
            arrow.Y = 12;
            arrow.SourceRect = new Rectangle(1, 1, 12, 11);
            arrow.FixedSize = new Size(12, 11);
            AddContent(arrow);

            _tooltipRenderer.AlignLeft = true;

            // We draw the background in a custom render, so there's no content to hit test
            PreciseHitTest = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timeBarTexture.Dispose();
            }

            base.Dispose(disposing);
        }

        [TempleDllLocation(0x10110400)]
        public override void Render()
        {
            // Render the time bar texture under the other content
            var sourceRect = new Rectangle(0, 0, 117, 21);

            // The entire texture is 256 pixels wide, and midnight is at x=192
            var currentTime = GameSystems.TimeEvent.GameTime;
            const long secondsPerDay = 60 * 60 * 24;
            sourceRect.X = (int) ((long) currentTime.Seconds % secondsPerDay * 256 / secondsPerDay - 64);

            // TODO: This is still just wrong...
            // Have to shift it right by 53 pixels so it aligns with the middle of the little arrow

            var a1 = new Render2dArgs();
            a1.customTexture = _timeBarTexture.Resource;
            a1.flags = Render2dFlag.WRAP|Render2dFlag.BUFFERTEXTURE;
            a1.srcRect = sourceRect;
            a1.destRect = GetContentArea();
            Tig.ShapeRenderer2d.DrawRectangle(ref a1);

            base.Render();
        }

        [TempleDllLocation(0x101104a0)]
        public override void RenderTooltip(int x, int y)
        {
            var builder = new StringBuilder();
            GameSystems.TimeEvent.FormatGameTime(GameSystems.TimeEvent.GameTime, builder);

            _tooltipRenderer.TooltipText = builder.ToString();
            _tooltipRenderer.Render(x, y);
        }
    }
}