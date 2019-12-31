using System;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.WorldMap
{
    public class YouAreHereWidget : WidgetBase
    {
        private readonly WidgetImage _image;

        public YouAreHereWidget()
        {
            _image = new WidgetImage("art/interface/WORLDMAP_UI/Worldmap_You_are_here.tga");
            AddContent(_image);
        }

        [TempleDllLocation(0x1015a5e0)]
        public override void Render()
        {
            // 5px pulse over 250ms
            var pulse = (int) (MathF.Sin((float) TimePoint.Now.Milliseconds / 250.0f) * 5.0f) / 2;

            _image.SetX(pulse);
            _image.SetY(pulse);
            _image.SetFixedWidth(Width - 2 * pulse);
            _image.SetFixedHeight(Height - 2 * pulse);

            base.Render();
        }

        // The icon is purely decorative and should not iteract with the mouse
        public override bool HitTest(int x, int y) => false;
    }
}