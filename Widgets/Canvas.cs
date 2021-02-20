using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace OpenTemple.Widgets
{
    public class Canvas : Avalonia.Controls.Canvas
    {
        /// <summary>
        /// Arranges the control's children.
        /// </summary>
        /// <param name="finalSize">The size allocated to the control.</param>
        /// <returns>The space taken.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (Control child in Children)
            {
                double x = 0.0;
                double y = 0.0;
                double elementLeft = GetLeft(child);
                double elementRight = GetRight(child);
                var size = child.DesiredSize;

                if (!double.IsNaN(elementLeft))
                {
                    x = elementLeft;
                    // When both left and right are specified, set the child's width
                    if (!double.IsNaN(elementRight))
                    {
                        // Arrange with right.
                        if (child.HorizontalAlignment == HorizontalAlignment.Center)
                        {
                            x = (finalSize.Width - size.Width) / 2;
                        }
                        else
                        {
                            size = size.WithWidth(finalSize.Width - elementLeft - elementRight);
                        }
                    }
                }
                else if (!double.IsNaN(elementRight))
                {
                    x = finalSize.Width - size.Width - elementRight;
                }

                double elementTop = GetTop(child);
                double elementBottom = GetBottom(child);
                if (!double.IsNaN(elementTop))
                {
                    y = elementTop;
                    // When both top and bottom are specified, set the child's height
                    if (!double.IsNaN(elementBottom))
                    {
                        if (child.VerticalAlignment == VerticalAlignment.Center)
                        {
                            y = (finalSize.Height - size.Height) / 2;
                        }
                        else
                        {
                            size = size.WithHeight(finalSize.Height - elementTop - elementBottom);
                        }
                    }
                }
                else if (!double.IsNaN(elementBottom))
                {
                    y = finalSize.Height - size.Height - elementBottom;
                }

                child.Arrange(new Rect(new Point(x, y), size));
            }

            return finalSize;
        }
    }
}
