using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using Avalonia.Utilities;

namespace OpenTemple.Widgets
{
    /// <summary>
    /// Enhances a standard Avalonia TextBlock with the ability to draw a simple drop shadow.
    /// </summary>
    public class TextBlock : Avalonia.Controls.TextBlock
    {
        public static readonly AvaloniaProperty DropShadowBrushProperty =
            AvaloniaProperty.Register<TextBlock, IBrush>(nameof(DropShadowBrush));

        public IBrush DropShadowBrush
        {
            get => (IBrush) GetValue(DropShadowBrushProperty);
            set => SetValue(DropShadowBrushProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            if (DropShadowBrush != null)
            {
                using (var p = context.PushPreTransform(Matrix.CreateTranslation(1, 1)))
                {
                    var oldForeground = Foreground;
                    Foreground = DropShadowBrush;
                    base.Render(context);
                    Foreground = oldForeground;
                }
            }

            base.Render(context);
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            base.ArrangeCore(finalRect);

            if (IsVisible)
            {
                // Extend bounds by shadow size
                Bounds = Bounds.WithWidth(Bounds.Width + 1).WithHeight(Bounds.Height + 1);
            }
        }
    }
}
