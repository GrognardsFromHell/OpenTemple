using System;
using Avalonia;
using Avalonia.Controls;

namespace OpenTemple.Widgets
{
    public class ZStackPanel : Panel
    {
        /// <summary>
        /// Initializes static members of the <see cref="ZStackPanel"/> class.
        /// </summary>
        static ZStackPanel()
        {
            ClipToBoundsProperty.OverrideDefaultValue<ZStackPanel>(false);
        }

        /// <summary>
        /// Arranges the control's children.
        /// </summary>
        /// <param name="finalSize">The size allocated to the control.</param>
        /// <returns>The space taken.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in Children)
            {
                child.Arrange(new Rect(new Point(0, 0), finalSize));
            }

            return finalSize;
        }
    }
}
