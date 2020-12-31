using System;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using Brush = OpenTemple.Core.GFX.TextRendering.Brush;

namespace OpenTemple.Core.Ui.Widgets
{
    /// <summary>
    /// Draws a rectangle using DrawRectangle or DrawRectangle Outline.
    /// </summary>
    class WidgetRectangle : WidgetContent, IDisposable
    {
        public Brush? Brush { get; set; }

        public PackedLinearColorA Pen { get; set; }

        public override void Render()
        {
            var renderer = Tig.ShapeRenderer2d;

            if (Brush.HasValue)
            {
                var brush = Brush.Value;
                PackedLinearColorA topColor, bottomColor;
                if (brush.gradient)
                {
                    topColor = brush.primaryColor;
                    bottomColor = brush.secondaryColor;
                }
                else
                {
                    topColor = brush.primaryColor;
                    bottomColor = brush.primaryColor;
                }

                Span<Vertex2d> vertices = stackalloc Vertex2d[4]
                {
                    new Vertex2d
                    {
                        diffuse = topColor,
                        pos = new Vector4(mContentArea.Right, mContentArea.Top, 0, 1)
                    },
                    new Vertex2d
                    {
                        diffuse = bottomColor,
                        pos = new Vector4(mContentArea.Right, mContentArea.Bottom, 0, 1)
                    },
                    new Vertex2d
                    {
                        diffuse = bottomColor,
                        pos = new Vector4(mContentArea.Left, mContentArea.Bottom, 0, 1)
                    },
                    new Vertex2d
                    {
                        diffuse = topColor,
                        pos = new Vector4(mContentArea.Left, mContentArea.Top, 0, 1)
                    },
                };

                renderer.DrawRectangle(vertices, null);
            }

            if (Pen.A != 0)
            {
                renderer.DrawRectangleOutline(
                    mContentArea,
                    Pen
                );
            }
        }

        public void Dispose()
        {
        }
    };
}