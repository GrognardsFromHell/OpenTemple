using System.Drawing;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetLegacyText : WidgetContent
    {
        private PredefinedFont _font;

        private string _text;

        private TigTextStyle _textStyle;

        public int MaxWidth { get; set; }

        public WidgetLegacyText(string text, PredefinedFont font, TigTextStyle style)
        {
            _text = Globals.UiAssets.ApplyTranslation(text);
            _font = font;
            _textStyle = style;
            UpdateBounds();
        }

        public string Text
        {
            get => _text;
            set
            {
                if (value == null)
                {
                    _text = null;
                    mPreferredSize = Size.Empty;
                }
                else
                {
                    _text = Globals.UiAssets.ApplyTranslation(value);
                    UpdateBounds();
                }
            }
        }

        public PredefinedFont Font
        {
            get => _font;
            set
            {
                _font = value;
                UpdateBounds();
            }
        }

        public TigTextStyle TextStyle
        {
            get => _textStyle;
            set
            {
                _textStyle = value;
                UpdateBounds();
            }
        }

        public bool CenterVertically { get; set; }

        public override void Render()
        {
            var area = mContentArea; // Will be modified below

            Tig.Fonts.PushFont(_font);

            if (CenterVertically)
            {
                var textMeas = Tig.Fonts.MeasureTextSize(Text, _textStyle, MaxWidth);
                area = new Rectangle(area.X + (area.Width - textMeas.Width) / 2,
                    area.Y + (area.Height - textMeas.Height) / 2,
                    textMeas.Width, textMeas.Height);
            }

            Tig.Fonts.RenderText(Text, area, _textStyle);

            Tig.Fonts.PopFont();
        }

        private void UpdateBounds()
        {
            Tig.Fonts.PushFont(_font);
            var rect = Tig.Fonts.MeasureTextSize(Text, _textStyle, MaxWidth);
            Tig.Fonts.PopFont();
            if (_textStyle.flags.HasFlag(TigTextStyleFlag.TTSF_CENTER))
            {
                // Return 0 here to be in sync with the new renderer
                mPreferredSize.Width = 0;
            }
            else
            {
                mPreferredSize.Width = rect.Width;
            }

            mPreferredSize.Height = rect.Height;
        }
    }
}