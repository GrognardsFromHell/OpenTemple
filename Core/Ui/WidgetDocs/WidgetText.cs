using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetText : WidgetContent
    {
        // Render these fonts using the old rendering engine
        private static readonly Dictionary<string, PredefinedFont> PredefinedFontMapping =
            new Dictionary<string, PredefinedFont>
            {
                {"scurlock-48", PredefinedFont.SCURLOCK_48},
                {"arial-10", PredefinedFont.ARIAL_10},
                {"arial-12", PredefinedFont.ARIAL_12},
                {"priory-12", PredefinedFont.PRIORY_12},
                {"arial-bold-10", PredefinedFont.ARIAL_BOLD_10},
                {"arial-bold-24", PredefinedFont.ARIAL_BOLD_24}
            };

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
            if (PredefinedFontMapping.TryGetValue(mText.defaultStyle.fontFace, out var predefinedFont))
            {
                RenderWithPredefinedFont(predefinedFont, GetLegacyStyle(mText.defaultStyle));
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
            if (PredefinedFontMapping.TryGetValue(mText.defaultStyle.fontFace, out var predefinedFont))
            {
                UpdateBoundsWithPredefinedFont(predefinedFont, GetLegacyStyle(mText.defaultStyle));
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

        private static TigTextStyle GetLegacyStyle(TextStyle style)
        {
            var sColorRect = new ColorRect(style.foreground.primaryColor);
            if (style.foreground.gradient)
            {
                sColorRect.bottomLeft = style.foreground.secondaryColor;
                sColorRect.bottomRight = style.foreground.secondaryColor;
            }

            TigTextStyle textStyle = new TigTextStyle(sColorRect);
            textStyle.leading = style.legacyLeading;
            textStyle.kerning = style.legacyKerning;
            textStyle.tracking = style.legacyTracking;

            if (style.dropShadow)
            {
                textStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW;
                var shadowColor = new ColorRect(style.dropShadowBrush.primaryColor);
                if (style.dropShadowBrush.gradient)
                {
                    shadowColor.bottomLeft = style.dropShadowBrush.secondaryColor;
                    shadowColor.bottomRight = style.dropShadowBrush.secondaryColor;
                }

                textStyle.shadowColor = shadowColor;
            }

            return textStyle;
        }
    }
}