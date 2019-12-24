namespace OpenTemple.Core.Ui.Widgets
{
    public sealed class WidgetButtonStyle
    {
        public string id;
        public string normalImagePath;
        public string activatedImagePath;
        public string hoverImagePath;
        public string pressedImagePath;
        public string disabledImagePath;
        public string frameImagePath;

        public string textStyleId;
        public string hoverTextStyleId;
        public string pressedTextStyleId;
        public string disabledTextStyleId;
        public int soundEnter = -1;
        public int soundLeave = -1;
        public int soundDown = -1;
        public int soundClick = -1;

        public WidgetButtonStyle Copy()
        {
            return (WidgetButtonStyle) MemberwiseClone();
        }

        // TODO: Trace all uses of this function in Vanilla and make the same call in the proper places in C#
        [TempleDllLocation(0x101f9660)]
        public WidgetButtonStyle UseDefaultSounds()
        {
            soundEnter = 3010;
            soundLeave = 3011;
            soundClick = 3013;
            soundDown = 3012;
            return this;
        }
    }

}