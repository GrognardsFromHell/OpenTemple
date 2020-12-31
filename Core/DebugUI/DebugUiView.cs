using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;

namespace OpenTemple.Core.DebugUI
{
    public class DebugUiView : Control
    {
        public PixelSize PixelSize
        {
            get
            {
                var scaling = VisualRoot.RenderScaling;
                return new PixelSize(Math.Max(1, (int) (Bounds.Width * scaling)),
                    Math.Max(1, (int) (Bounds.Height * scaling)));
            }
        }
    }
}