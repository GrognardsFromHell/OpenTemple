using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.Materials;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.TigSubsystems
{
    public class FontRenderer : IDisposable
    {
        private const int MaxGlyphFiles = 4;

        private readonly GlyphFileState[] _fileState = new GlyphFileState[MaxGlyphFiles];

        public FontRenderer()
        {
            for (var i = 0; i < _fileState.Length; i++)
            {
                _fileState[i] = new GlyphFileState();
            }
        }

        public void Dispose()
        {
        }

        public void RenderRun(ReadOnlySpan<char> text,
            int x,
            int y,
            Rectangle bounds,
            TigTextStyle style,
            TigFont font,
            DrawingContext context)
        {
            foreach (var state in _fileState)
            {
                state.GlyphCount = 0;
            }

            var fontFace = font.FontFace;
            for (var it = 0; it < text.Length; ++it)
            {
                var ch = text[it];
                var nextCh = '\0';
                if (it + 1 < text.Length)
                {
                    nextCh = text[it + 1];
                }

                // @0 to @9 select one of the text colors
                if (ch == '@' && char.IsDigit(nextCh))
                {
                    ++it; // Skip the digit
                    style.colorSlot = nextCh - '0';
                    continue;
                }

                // Handle @t tabstop movement
                if (ch == '@' && nextCh == 't')
                {
                    var tabWidth = style.tabStop - bounds.X;

                    if (tabWidth > 0)
                    {
                        ++it; // Skip the t

                        var tabCount = 1 + (x - bounds.X) / tabWidth;
                        x = bounds.X + tabCount * tabWidth;
                    }

                    continue;
                }

                if (!font.GetGlyphIdx(text[it], out var glyphIdx))
                {
                    continue;
                }

                if (text[it] == ' ')
                {
                    glyphIdx = '-' - '!';
                }

                if (glyphIdx >= fontFace.Glyphs.Length)
                {
                    return; // Trying to render invalid character
                }

                if (ch >= 0 && ch < 128 && char.IsWhiteSpace(ch))
                {
                    x += style.tracking;
                    continue;
                }

                var glyph = fontFace.Glyphs[glyphIdx];

                // For some mysterious reason ToEE actually uses one pixel more to the left of the
                // Glyph than is specified in the font file. That area should be transparent, but
                // it means all rendered text is shifted one pixel more to the right than it should be.
                float u1 = glyph.Rectangle.X - 1;
                float v1 = glyph.Rectangle.Y - 1;
                var u2 = u1 + glyph.Rectangle.Width + 1;
                var v2 = v1 + glyph.Rectangle.Height + 1;

                var state = _fileState[glyph.FontArtIndex];

                var destRect = new Rectangle(
                    x,
                    y + fontFace.BaseLine - glyph.BaseLineYOffset,
                    glyph.Rectangle.Width + 1,
                    glyph.Rectangle.Height + 1
                );

                x += style.kerning + glyph.WidthLine;

                Trace.Assert(!style.flags.HasFlag((TigTextStyleFlag) 0x1000));
                Trace.Assert(!style.flags.HasFlag((TigTextStyleFlag) 0x2000));

                // Drop Shadow
                if (style.flags.HasFlag(TigTextStyleFlag.TTSF_DROP_SHADOW))
                {
                    Trace.Assert(style.shadowColor.HasValue);
                    var shadowVertexIdx = state.GlyphCount * 4;
                    var shadowColor = style.shadowColor.Value.topLeft;
                    shadowColor.A = 255;

                    // Top Left
                    ref var sVertexTL = ref state.Vertices[shadowVertexIdx];
                    sVertexTL.X = destRect.X + 1.0f;
                    sVertexTL.Y = destRect.Y + 1.0f;
                    sVertexTL.U = u1;
                    sVertexTL.V = v1;
                    sVertexTL.diffuse = shadowColor;

                    // Top Right
                    ref var sVertexTR = ref state.Vertices[shadowVertexIdx + 1];
                    sVertexTR.X = destRect.X + destRect.Width + 1.0f;
                    sVertexTR.Y = destRect.Y + 1.0f;
                    sVertexTR.U = u2;
                    sVertexTR.V = v1;
                    sVertexTR.diffuse = shadowColor;

                    // Bottom Right
                    ref var sVertexBR = ref state.Vertices[shadowVertexIdx + 2];
                    sVertexBR.X = destRect.X + destRect.Width + 1.0f;
                    sVertexBR.Y = destRect.Y + destRect.Height + 1.0f;
                    sVertexBR.U = u2;
                    sVertexBR.V = v2;
                    sVertexBR.diffuse = shadowColor;

                    // Bottom Left
                    ref var sVertexBL = ref state.Vertices[shadowVertexIdx + 3];
                    sVertexBL.X = destRect.X + 1.0f;
                    sVertexBL.Y = destRect.Y + destRect.Height + 1.0f;
                    sVertexBL.U = u1;
                    sVertexBL.V = v2;
                    sVertexBL.diffuse = shadowColor;

                    state.GlyphCount++;

                    if (state.GlyphCount >= GlyphFileState.MaxGlyphs)
                    {
                        RenderGlyphs(state.Vertices, font, glyph.FontArtIndex, state.GlyphCount, context);
                        state.GlyphCount = 0;
                    }
                }

                var vertexIdx = state.GlyphCount * 4;
                var colorRect = style.GetTextColor(style.colorSlot);

                // Top Left
                ref var vertexTL = ref state.Vertices[vertexIdx];
                vertexTL.X = destRect.X;
                vertexTL.Y = destRect.Y;
                vertexTL.U = u1;
                vertexTL.V = v1;
                vertexTL.diffuse = colorRect.topLeft;

                // Top Right
                ref var vertexTR = ref state.Vertices[vertexIdx + 1];
                vertexTR.X = (float) destRect.X + destRect.Width;
                vertexTR.Y = destRect.Y;
                vertexTR.U = u2;
                vertexTR.V = v1;
                vertexTR.diffuse = colorRect.topRight;

                // Bottom Right
                ref var vertexBR = ref state.Vertices[vertexIdx + 2];
                vertexBR.X = (float) destRect.X + destRect.Width;
                vertexBR.Y = (float) destRect.Y + destRect.Height;
                vertexBR.U = u2;
                vertexBR.V = v2;
                vertexBR.diffuse = colorRect.bottomRight;

                // Bottom Left
                ref var vertexBL = ref state.Vertices[vertexIdx + 3];
                vertexBL.X = destRect.X;
                vertexBL.Y = (float) destRect.Y + destRect.Height;
                vertexBL.U = u1;
                vertexBL.V = v2;
                vertexBL.diffuse = colorRect.bottomLeft;

                // Support rotations (i.e. for the radial menu)
                if (style.flags.HasFlag(TigTextStyleFlag.TTSF_ROTATE))
                {
                    float rotCenterX, rotCenterY;
                    if (style.flags.HasFlag(TigTextStyleFlag.TTSF_ROTATE_OFF_CENTER))
                    {
                        rotCenterX = style.rotationCenterX;
                        rotCenterY = style.rotationCenterY;
                    }
                    else
                    {
                        rotCenterX = bounds.X;
                        rotCenterY = (float) bounds.Y + fontFace.BaseLine;
                    }

                    var rotCos = MathF.Cos(style.rotation);
                    var rotSin = MathF.Sin(style.rotation);
                    Rotate2d(ref vertexTL.X, ref vertexTL.Y, rotCos, rotSin, rotCenterX, rotCenterY);
                    Rotate2d(ref vertexTR.X, ref vertexTR.Y, rotCos, rotSin, rotCenterX, rotCenterY);
                    Rotate2d(ref vertexBR.X, ref vertexBR.Y, rotCos, rotSin, rotCenterX, rotCenterY);
                    Rotate2d(ref vertexBL.X, ref vertexBL.Y, rotCos, rotSin, rotCenterX, rotCenterY);
                }

                state.GlyphCount++;

                if (state.GlyphCount >= GlyphFileState.MaxGlyphs)
                {
                    RenderGlyphs(state.Vertices, font, glyph.FontArtIndex, state.GlyphCount, context);
                    state.GlyphCount = 0;
                }
            }


            // Flush the remaining state
            for (var i = 0; i < MaxGlyphFiles; ++i)
            {
                var state = _fileState[i];
                if (state.GlyphCount > 0)
                {
                    RenderGlyphs(state.Vertices, font, i, state.GlyphCount, context);
                }
            }
        }

        private void RenderGlyphs(Span<GlyphVertex2d> vertices2d, TigFont font, int fontArtIndex, int glyphCount,
            DrawingContext context)
        {
            var image = font.GetAlphaMask(fontArtIndex);

            for (var i = 0; i < glyphCount; i++)
            {
                // Order is TL, TR, BR, BL
                ref var topLeft = ref vertices2d[i * 4];
                ref var bottomRight = ref vertices2d[i * 4 + 2];

                var destRect = new Rect(
                    new Avalonia.Point(topLeft.X, topLeft.Y),
                    new Avalonia.Point(bottomRight.X, bottomRight.Y)
                );
                var srcRect = new Rect(
                    new Avalonia.Point(topLeft.U, topLeft.V),
                    new Avalonia.Point(bottomRight.U, bottomRight.V)
                );

                context.Custom(new GlyphRenderOp(image, srcRect, destRect, topLeft.diffuse,
                    bottomRight.diffuse));
            }
        }

        private static void Rotate2d(ref float x, ref float y,
            float rotCos, float rotSin,
            float centerX, float centerY)
        {
            var newX = centerX + rotCos * (x - centerX) - rotSin * (y - centerY);
            var newY = centerY + rotSin * (x - centerX) + rotCos * (y - centerY);
            x = newX;
            y = newY;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct GlyphVertex2d
        {
            public float U;
            public float V;
            public float X;
            public float Y;
            public PackedLinearColorA diffuse;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct GlyphVertex3d
        {
            public Vector3 pos;
            public PackedLinearColorA diffuse;
            public Vector2 uv;

            public static readonly int Size = Marshal.SizeOf<GlyphVertex3d>();
        }

        private class GlyphFileState
        {
            public const int MaxGlyphs = 800;
            public int GlyphCount;
            public readonly GlyphVertex2d[] Vertices = new GlyphVertex2d[MaxGlyphs * 4];
        }
    }
}
