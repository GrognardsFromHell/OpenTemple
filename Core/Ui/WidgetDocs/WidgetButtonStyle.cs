namespace SpicyTemple.Core.Ui.WidgetDocs
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
    };
}