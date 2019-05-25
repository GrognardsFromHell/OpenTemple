using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.Materials;

namespace SpicyTemple.Core.TigSubsystems
{
    public class FontRenderer : IDisposable
    {
        private const int MaxGlyphFiles = 4;

        private readonly ResourceLifecycleCallbacks _callbacks;
        private readonly RenderingDevice _device;

        private readonly GlyphFileState[] _fileState = new GlyphFileState[MaxGlyphFiles];

        private ResourceRef<BufferBinding> _bufferBinding;

        private ResourceRef<IndexBuffer> _indexBuffer;

        private ResourceRef<Material> _material;

        public FontRenderer(RenderingDevice device)
        {
            _device = device;
            _material = CreateMaterial(device).Ref();
            _callbacks = new ResourceLifecycleCallbacks(device, CreateResources, FreeResources);

            for (var i = 0; i < _fileState.Length; i++)
            {
                _fileState[i] = new GlyphFileState();
            }
        }

        public void Dispose()
        {
            _callbacks.Dispose();
            _material.Dispose();
            _bufferBinding.Dispose();
            _indexBuffer.Dispose();
        }

        private void CreateResources(RenderingDevice device)
        {
            var vertexIdx = 0;
            Span<ushort> indicesData = stackalloc ushort[GlyphFileState.MaxGlyphs * 6];

            int j = 0;
            for (var i = 0; i < GlyphFileState.MaxGlyphs; ++i)
            {
                // Counter clockwise quad rendering
                indicesData[j++] = (ushort) (vertexIdx + 0);
                indicesData[j++] = (ushort) (vertexIdx + 1);
                indicesData[j++] = (ushort) (vertexIdx + 2);
                indicesData[j++] = (ushort) (vertexIdx + 0);
                indicesData[j++] = (ushort) (vertexIdx + 2);
                indicesData[j++] = (ushort) (vertexIdx + 3);
                vertexIdx += 4;
            }

            _indexBuffer = device.CreateIndexBuffer(indicesData);

            _bufferBinding = new BufferBinding(device, _material.Resource.VertexShader).Ref();
            _bufferBinding.Resource.AddBuffer<GlyphVertex3d>(null, 0)
                .AddElement(VertexElementType.Float3, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        private void FreeResources(RenderingDevice device)
        {
            _bufferBinding.Dispose();
            _indexBuffer.Dispose();
        }

        private static Material CreateMaterial(RenderingDevice device)
        {
            var blendState = new BlendSpec
            {
                blendEnable = true,
                srcBlend = BlendOperand.SrcAlpha,
                destBlend = BlendOperand.InvSrcAlpha
            };

            var depthStencilState = new DepthStencilSpec();
            depthStencilState.depthEnable = false;

            var vertexShader = device.GetShaders().LoadVertexShader("font_vs");
            var pixelShader = device.GetShaders().LoadPixelShader("textured_simple_ps");

            return device.CreateMaterial(
                blendState,
                depthStencilState,
                null,
                null,
                vertexShader,
                pixelShader
            );
        }

        public void RenderRun(ReadOnlySpan<char> text,
            int x,
            int y,
            Rectangle bounds,
            TigTextStyle style,
            TigFont font)
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
                    var tabWidth = style.field4c - bounds.X;

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
                        RenderGlyphs(state.Vertices, font.GetFontArt(glyph.FontArtIndex), state.GlyphCount);
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
                    RenderGlyphs(state.Vertices, font.GetFontArt(glyph.FontArtIndex), state.GlyphCount);
                    state.GlyphCount = 0;
                }
            }


            // Flush the remaining state
            for (var i = 0; i < MaxGlyphFiles; ++i)
            {
                var state = _fileState[i];
                if (state.GlyphCount > 0)
                {
                    RenderGlyphs(state.Vertices, font.GetFontArt(i), state.GlyphCount);
                }
            }
        }

        private void RenderGlyphs(Span<GlyphVertex2d> vertices2d, ITexture texture, int glyphCount)
        {
            _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);
            _device.SetMaterial(_material);
            _device.SetTexture(0, texture);

            var textureSize = texture.GetSize();

            var vertexCount = glyphCount * 4;

            // TODO: Use pooling
            var vertices3d = new GlyphVertex3d[vertexCount];

            for (var i = 0; i < vertexCount; i++)
            {
                ConvertVertex(ref vertices2d[i], out vertices3d[i], textureSize);
            }

            using var buffer = _device.CreateVertexBuffer<GlyphVertex3d>(vertices3d);

            _bufferBinding
                .Resource
                .SetBuffer(0, buffer)
                .Bind();
            _device.SetIndexBuffer(_indexBuffer);

            _device.DrawIndexed(PrimitiveType.TriangleList, vertexCount, glyphCount * 2 * 3);
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

        private static void ConvertVertex(ref GlyphVertex2d vertex2d, out GlyphVertex3d vertex3d, Size textureSize)
        {
            vertex3d = new GlyphVertex3d();
            vertex3d.pos.X = vertex2d.X;
            vertex3d.pos.Y = vertex2d.Y;
            vertex3d.pos.Z = 0.5f;

            vertex3d.diffuse = vertex2d.diffuse;

            vertex3d.uv.X = vertex2d.U / textureSize.Width;
            vertex3d.uv.Y = vertex2d.V / textureSize.Height;
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