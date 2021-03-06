using System;
using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.MainMenu
{
    public class LegacyText : UserControl
    {
        public static readonly AvaloniaProperty FontProperty = AvaloniaProperty.Register<LegacyText, PredefinedFont>(
            nameof(Font), default, false, BindingMode.TwoWay);

        public PredefinedFont Font
        {
            get => (PredefinedFont) GetValue(FontProperty);
            set => SetValue(FontProperty, value);
        }

        public static readonly AvaloniaProperty TextStyleProperty = AvaloniaProperty.Register<LegacyText, TigTextStyle>(
            nameof(TextStyle), default, false, BindingMode.TwoWay);

        public TigTextStyle TextStyle
        {
            get => (TigTextStyle) GetValue(TextStyleProperty);
            set => SetValue(TextStyleProperty, value);
        }

        public LegacyText()
        {
            this.GetObservable(ContentProperty).Subscribe(_ =>
            {
                UpdateBounds();
                InvalidateVisual();
            });
            this.GetObservable(TextStyleProperty).Subscribe(_ =>
            {
                UpdateBounds();
                InvalidateVisual();
            });
            this.GetObservable(FontProperty).Subscribe(_ =>
            {
                UpdateBounds();
                InvalidateVisual();
            });
            Template = null;
        }

        private void UpdateBounds()
        {
            if (!(Content is string text) || TextStyle == null)
            {
                Width = 0;
                Height = 0;
                return;
            }

            Tig.Fonts.PushFont(Font);
            var bounds = Tig.Fonts.MeasureTextSize(text, TextStyle);
            Width = bounds.Width;
            Height = bounds.Height;
            Tig.Fonts.PopFont();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (!(Content is string text) || TextStyle == null)
            {
                return;
            }

            var extents = new Rectangle();

            TextStyle.flags &= ~TigTextStyleFlag.TTSF_DROP_SHADOW;
            Tig.Fonts.PushFont(Font);
            Tig.Fonts.RenderText(text, extents, TextStyle, context);
            Tig.Fonts.PopFont();
        }
    }
}
