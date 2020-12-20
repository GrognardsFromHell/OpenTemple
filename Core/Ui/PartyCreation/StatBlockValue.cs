using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation
{
    internal class StatBlockValue
    {
        private readonly WidgetText _caption;
        private readonly WidgetContent _captionBg;

        private readonly WidgetText _valueLabel;
        private readonly WidgetContent _valueBorder;

        public bool IsActive
        {
            set
            {
                _caption.Visible = value;
                _captionBg.Visible = value;
                _valueLabel.Visible = value;
                _valueBorder.Visible = value;
            }
        }

        public string Value
        {
            set => _valueLabel.SetText(value);
        }

        public StatBlockValue(WidgetDoc doc, string idPrefix)
        {
            _caption = doc.GetTextContent(idPrefix + "Caption");
            _captionBg = doc.GetContent(idPrefix + "CaptionBg");
            _valueLabel = doc.GetTextContent(idPrefix + "ValueLabel");
            _valueBorder = doc.GetContent(idPrefix + "ValueBorder");
        }
    }
}