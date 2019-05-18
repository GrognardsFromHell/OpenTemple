using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetText : WidgetContent
    {
        public WidgetText()
        {
            mText.defaultStyle = Globals.WidgetTextStyles.GetDefaultStyle();
        }

        public WidgetText(string text, string styleId)
        {
            mText.defaultStyle = Globals.WidgetTextStyles.GetTextStyle(styleId);
            SetText(text);
        }

        public WidgetText(string text, TextStyle style)
        {
            mText.defaultStyle = style;
            SetText(text);
        }

        public void SetText(string text)
        {
            // TODO: Process mes file placeholders
            mText.text = Globals.UiAssets.ApplyTranslation(text);
            UpdateBounds();
        }

        public void SetStyleId(string id)
        {
            mStyleId = id;
            if (id != null)
            {
                mText.defaultStyle = Globals.WidgetTextStyles.GetTextStyle(id);
            }
            else
            {
                mText.defaultStyle = Globals.WidgetTextStyles.GetDefaultStyle();
            }

            UpdateBounds();
        }

        public string GetStyleId()
        {
            return mStyleId;
        }

        public void SetStyle(TextStyle style)
        {
            mText.defaultStyle = style;
            UpdateBounds();
        }

        public TextStyle GetStyle()
        {
            return mText.defaultStyle;
        }

        public void SetCenterVertically(bool isCentered)
        {
            mCenterVertically = isCentered;
        }

        public override void Render()
        {
            if (mText.defaultStyle.fontFace == sScurlockFont)
            {
                RenderWithPredefinedFont(PredefinedFont.SCURLOCK_48, GetScurlockStyle(mText.defaultStyle.foreground));
            }
            else if (mText.defaultStyle.fontFace == sPriory12)
            {
                RenderWithPredefinedFont(PredefinedFont.PRIORY_12, GetPrioryStyle(mText.defaultStyle.foreground));
            }
            else if (mText.defaultStyle.fontFace == sArialBold10)
            {
                RenderWithPredefinedFont(PredefinedFont.ARIAL_BOLD_10,
                    GetArialBoldStyle(mText.defaultStyle.foreground));
            }
            else
            {
                var area = mContentArea; // Will be modified below

                if (mCenterVertically)
                {
                    Tig.RenderingDevice.GetTextEngine().MeasureText(mText, out var metrics);
                    area = new Rectangle(area.X,
                        area.Y + (area.Height - metrics.height) / 2,
                        area.Width, metrics.height);
                }

                Tig.RenderingDevice.GetTextEngine().RenderText(area, mText);
            }
        }

        private FormattedText mText;
        private string mStyleId;
        private bool mWordWrap = false;
        private bool mCenterVertically = false;

        private void UpdateBounds()
        {
            if (mText.defaultStyle.fontFace == sScurlockFont)
            {
                UpdateBoundsWithPredefinedFont(PredefinedFont.SCURLOCK_48,
                    GetScurlockStyle(mText.defaultStyle.foreground));
            }
            else if (mText.defaultStyle.fontFace == sPriory12)
            {
                UpdateBoundsWithPredefinedFont(PredefinedFont.PRIORY_12, GetPrioryStyle(mText.defaultStyle.foreground));
            }
            else if (mText.defaultStyle.fontFace == sArialBold10)
            {
                UpdateBoundsWithPredefinedFont(PredefinedFont.ARIAL_BOLD_10,
                    GetArialBoldStyle(mText.defaultStyle.foreground));
            }
            else
            {
                Tig.RenderingDevice.GetTextEngine().MeasureText(mText, out var textMetrics);
                mPreferredSize.Width = textMetrics.width;
                mPreferredSize.Height = textMetrics.height;
            }
        }

        private void RenderWithPredefinedFont(PredefinedFont font, TigTextStyle textStyle)
        {
            var area = mContentArea; // Will be modified below

            Tig.Fonts.PushFont(font);

            var text = mText.text;

            if (mText.defaultStyle.align == TextAlign.Center)
            {
                textStyle.flags |= TigTextStyleFlag.TTSF_CENTER;
                if (mCenterVertically)
                {
                    var textMeas = Tig.Fonts.MeasureTextSize(text, textStyle);
                    area = new Rectangle(area.X + (area.Width - textMeas.Width) / 2,
                        area.Y + (area.Height - textMeas.Height) / 2,
                        textMeas.Width, textMeas.Height);
                }
            }

            Tig.Fonts.RenderText(text, area, textStyle);

            Tig.Fonts.PopFont();
        }

        private void UpdateBoundsWithPredefinedFont(PredefinedFont font, TigTextStyle textStyle)
        {
            if (mText.defaultStyle.align == TextAlign.Center)
            {
                textStyle.flags |= TigTextStyleFlag.TTSF_CENTER;
            }

            Tig.Fonts.PushFont(font);
            var rect = Tig.Fonts.MeasureTextSize(mText.text, textStyle, 0, 0);
            Tig.Fonts.PopFont();
            if (mText.defaultStyle.align == TextAlign.Center)
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

        // Render this font using the old engine
        private const string sScurlockFont = "Scurlock";
        private const string sPriory12 = "Priory-12";
        private const string sArialBold10 = "Arial-Bold-ToEE";

        private static TigTextStyle GetScurlockStyle(Brush brush)
        {
            ColorRect sColorRect;
            sColorRect.topLeft = brush.primaryColor;
            sColorRect.topRight = brush.primaryColor;
            if (brush.gradient)
            {
                sColorRect.bottomLeft = brush.secondaryColor;
                sColorRect.bottomRight = brush.secondaryColor;
            }
            else
            {
                sColorRect.bottomLeft = brush.primaryColor;
                sColorRect.bottomRight = brush.primaryColor;
            }

            ColorRect sShadowColor = new ColorRect(new PackedLinearColorA(0, 0, 0, 0.5f));

            TigTextStyle textStyle = new TigTextStyle(sColorRect);
            textStyle.leading = 1;
            textStyle.kerning = 0;
            textStyle.tracking = 10;
            textStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW;
            textStyle.shadowColor = sShadowColor;
            return textStyle;
        }

        private static TigTextStyle GetPrioryStyle(Brush brush)
        {
            ColorRect sColorRect;
            sColorRect.topLeft = brush.primaryColor;
            sColorRect.topRight = brush.primaryColor;
            if (brush.gradient)
            {
                sColorRect.bottomLeft = brush.secondaryColor;
                sColorRect.bottomRight = brush.secondaryColor;
            }
            else
            {
                sColorRect.bottomLeft = brush.primaryColor;
                sColorRect.bottomRight = brush.primaryColor;
            }

            ColorRect sShadowColor = new ColorRect(new PackedLinearColorA(0, 0, 0, 0.5f));

            TigTextStyle textStyle = new TigTextStyle(sColorRect);
            textStyle.leading = 0;
            textStyle.kerning = 1;
            textStyle.tracking = 3;
            textStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW;
            textStyle.shadowColor = sShadowColor;

            return textStyle;
        }

        private static TigTextStyle GetArialBoldStyle(Brush brush)
        {
            ColorRect sColorRect;
            sColorRect.topLeft = brush.primaryColor;
            sColorRect.topRight = brush.primaryColor;
            if (brush.gradient)
            {
                sColorRect.bottomLeft = brush.secondaryColor;
                sColorRect.bottomRight = brush.secondaryColor;
            }
            else
            {
                sColorRect.bottomLeft = brush.primaryColor;
                sColorRect.bottomRight = brush.primaryColor;
            }

            ColorRect sShadowColor = new ColorRect(new PackedLinearColorA(0, 0, 0, 0.5f));

            TigTextStyle textStyle = new TigTextStyle(sColorRect);
            textStyle.leading = 0;
            textStyle.kerning = 1;
            textStyle.tracking = 3;
            textStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW;
            textStyle.shadowColor = sShadowColor;

            return textStyle;
        }

    }
}