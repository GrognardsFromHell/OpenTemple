using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.GFX;

public enum SamplerType2d
{
    Clamp,
    Wrap,
    Point
}

public struct Line2d
{
    public Vector2 from;
    public Vector2 to;
    public PackedLinearColorA diffuse;

    public Line2d(Vector2 from, Vector2 to, PackedLinearColorA diffuse)
    {
        this.from = from;
        this.to = to;
        this.diffuse = diffuse;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 44, Pack = 1)]
public struct Vertex2d : IVertexFormat
{
    [FieldOffset(0)]
    public Vector4 pos;

    [FieldOffset(16)]
    public Vector4 normal;

    [FieldOffset(32)]
    public PackedLinearColorA diffuse;

    [FieldOffset(36)]
    public Vector2 uv;

    public static void Describe(ref BufferBindingBuilder builder)
    {
        Debug.Assert(Size == 44);
        builder.AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
    }

    public static readonly int Size = Marshal.SizeOf<Vertex2d>();
}

public sealed class ShapeRenderer2d : IDisposable
{
    private readonly RenderingDevice _device;
    private readonly Textures _textures;
    private readonly Material _untexturedMaterial;
    private readonly Material _texturedMaterial;
    private readonly Material _texturedWithoutBlendingMaterial;
    private readonly Material _texturedWithMaskMaterial;
    private readonly Material _lineMaterial;
    private readonly BufferBinding _mdfBufferBinding;
    private Material _outlineMaterial;
    private readonly Material _pieFillMaterial;
    private ResourceRef<BufferBinding> _bufferBinding;
    private ResourceRef<BufferBinding> _lineBufferBinding;
    private ResourceRef<SamplerState> _samplerWrapState;
    private ResourceRef<SamplerState> _samplerClampPointState;
    private ResourceRef<SamplerState> _samplerClampState;
    private ResourceRef<DepthStencilState> _noDepthState;
    private ResourceRef<VertexBuffer> _vertexBuffer;
    private ResourceRef<IndexBuffer> _indexBuffer;

    public ShapeRenderer2d(RenderingDevice device)
    {
        _device = device;
        _textures = device.GetTextures();

        _untexturedMaterial = CreateMaterial(device, "diffuse_only_ps");
        _texturedMaterial = CreateMaterial(device, "textured_simple_ps");
        _texturedWithoutBlendingMaterial = CreateMaterial(device, "textured_simple_ps", false, false);
        _texturedWithMaskMaterial = CreateMaterial(device, "textured_two_ps");
        _lineMaterial = CreateMaterial(device, "diffuse_only_ps", true);
        _outlineMaterial = CreateOutlineMaterial(device);
        _pieFillMaterial = CreatePieFillMaterial(device);
        _bufferBinding = new BufferBinding(device, _texturedMaterial.VertexShader).Ref();
        _mdfBufferBinding = _device.CreateMdfBufferBinding();
        _lineBufferBinding = new BufferBinding(device, _lineMaterial.VertexShader).Ref();


        SamplerSpec samplerWrapSpec = new SamplerSpec();
        samplerWrapSpec.addressU = TextureAddress.Wrap;
        samplerWrapSpec.addressV = TextureAddress.Wrap;
        samplerWrapSpec.minFilter = TextureFilterType.Linear;
        samplerWrapSpec.magFilter = TextureFilterType.Linear;
        samplerWrapSpec.mipFilter = TextureFilterType.Linear;
        _samplerWrapState = _device.CreateSamplerState(samplerWrapSpec);

        SamplerSpec samplerClampPointSpec = new SamplerSpec();
        samplerClampPointSpec.addressU = TextureAddress.Clamp;
        samplerClampPointSpec.addressV = TextureAddress.Clamp;
        samplerClampPointSpec.minFilter = TextureFilterType.NearestNeighbor;
        samplerClampPointSpec.magFilter = TextureFilterType.NearestNeighbor;
        samplerClampPointSpec.mipFilter = TextureFilterType.NearestNeighbor;
        _samplerClampPointState = _device.CreateSamplerState(samplerClampPointSpec);

        SamplerSpec samplerClampSpec = new SamplerSpec();
        samplerClampSpec.addressU = TextureAddress.Clamp;
        samplerClampSpec.addressV = TextureAddress.Clamp;
        samplerClampSpec.minFilter = TextureFilterType.Linear;
        samplerClampSpec.magFilter = TextureFilterType.Linear;
        samplerClampSpec.mipFilter = TextureFilterType.Linear;
        _samplerClampState = _device.CreateSamplerState(samplerClampSpec);

        DepthStencilSpec noDepthSpec = new DepthStencilSpec();
        noDepthSpec.depthEnable = false;
        _noDepthState = _device.CreateDepthStencilState(noDepthSpec);

        _vertexBuffer = _device.CreateEmptyVertexBuffer(Vertex2d.Size * 8, debugName: "ShapeRenderer2d");

        var indexData = new ushort[]
        {
            0, 1, 2,
            2, 3, 0
        };
        _indexBuffer = _device.CreateIndexBuffer(indexData);

        _bufferBinding.Resource.AddBuffer<Vertex2d>(_vertexBuffer, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

        _mdfBufferBinding.AddBuffer<Vertex2d>(_vertexBuffer, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
    }

    private static Material CreateMaterial(RenderingDevice device,
        string pixelShaderName,
        bool forLine = false,
        bool blending = true)
    {
        BlendSpec blendSpec = new BlendSpec();
        blendSpec.blendEnable = blending;
        blendSpec.srcBlend = BlendOperand.SrcAlpha;
        blendSpec.destBlend = BlendOperand.InvSrcAlpha;
        DepthStencilSpec depthStencilSpec = new DepthStencilSpec();
        depthStencilSpec.depthEnable = false;
        var vsDefines = new Dictionary<string, string>();
        if (forLine)
        {
            vsDefines["DRAW_LINES"] = "1";
        }

        using var vertexShader = device.GetShaders().LoadVertexShader("gui_vs", vsDefines);
        using var pixelShader = device.GetShaders().LoadPixelShader(pixelShaderName);

        return device.CreateMaterial(blendSpec, depthStencilSpec, null,
            null, vertexShader.Resource, pixelShader.Resource);
    }

    private static Material CreateOutlineMaterial(RenderingDevice device)
    {
        BlendSpec blendSpec = new BlendSpec();
        blendSpec.blendEnable = true;
        blendSpec.srcBlend = BlendOperand.SrcAlpha;
        blendSpec.destBlend = BlendOperand.InvSrcAlpha;
        DepthStencilSpec depthStencilSpec = new DepthStencilSpec();
        depthStencilSpec.depthEnable = false;
        using var vertexShader = device.GetShaders().LoadVertexShader("line_vs");
        using var pixelShader = device.GetShaders().LoadPixelShader("diffuse_only_ps");

        return device.CreateMaterial(blendSpec, depthStencilSpec, null, null, vertexShader, pixelShader);
    }

    private Material CreatePieFillMaterial(RenderingDevice device)
    {
        BlendSpec blendSpec = new BlendSpec();
        blendSpec.blendEnable = true;
        blendSpec.srcBlend = BlendOperand.SrcAlpha;
        blendSpec.destBlend = BlendOperand.InvSrcAlpha;
        DepthStencilSpec depthStencilSpec = new DepthStencilSpec();
        depthStencilSpec.depthEnable = false;
        RasterizerSpec rasterizerSpec = new RasterizerSpec();
        rasterizerSpec.cullMode = CullMode.None;

        using var vertexShader = _device.GetShaders().LoadVertexShader("gui_vs");
        using var pixelShader = _device.GetShaders().LoadPixelShader("diffuse_only_ps");

        return _device.CreateMaterial(blendSpec, depthStencilSpec, rasterizerSpec,
            null, vertexShader, pixelShader);
    }

    public void DrawDashedRectangle(Rectangle bounds, DashPattern pattern) => DrawDashedRectangle(new RectangleF(
        bounds.X,
        bounds.Y,
        bounds.Width,
        bounds.Height
    ), pattern);

    public void DrawDashedRectangle(RectangleF bounds, DashPattern pattern)
    {
        var nowMillis = (long) TimePoint.Now.Milliseconds;

        var t = 0f;
        if (pattern.AnimationCycleMs > 0)
        {
            t = (nowMillis % (int) pattern.AnimationCycleMs) / pattern.AnimationCycleMs;
        }

        var builder = SimpleMeshBuilder<Vertex2d>.Quads(100);
        using (builder)
        {
            float z = 0;

            BuildHorizontalDashedLine(ref builder, t, bounds.X, bounds.Right, bounds.Y, z, pattern, false);
            BuildHorizontalDashedLine(ref builder, t, bounds.X, bounds.Right, bounds.Bottom - pattern.Width, z,
                pattern, true);

            BuildVerticalDashedLine(ref builder, t, bounds.X, bounds.Y, bounds.Bottom, z, pattern, true);
            BuildVerticalDashedLine(ref builder, t, bounds.Right - pattern.Width, bounds.Y, bounds.Bottom, z, pattern, false);

            BuildAndDrawMesh(ref builder, _untexturedMaterial);
        }
    }

    private void BuildAndDrawMesh(ref SimpleMeshBuilder<Vertex2d> builder, Material material)
    {
        using var mesh = builder.Build(material.VertexShader);
        _device.SetMaterial(material);
        _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);
        mesh.Render(_device);
    }

    private static void BuildHorizontalDashedLine(ref SimpleMeshBuilder<Vertex2d> builder,
        float t, float x1, float x2, float y, float z,
        DashPattern pattern, bool reverse)
    {
        if (!reverse)
        {
            t = 1 - t;
        }

        var phase = t * pattern.Length;

        var color = pattern.Color;

        for (float x = x1 - phase; x < x2; x += pattern.Length)
        {
            // Clockwise, starting top-left
            builder.Vertex(Math.Clamp(x, x1, x2), y, z).Color(color);
            builder.Vertex(Math.Clamp(x + pattern.OnLength, x1, x2), y, z).Color(color);
            builder.Vertex(Math.Clamp(x + pattern.OnLength, x1, x2), y + pattern.Width, z).Color(color);
            builder.Vertex(Math.Clamp(x, x1, x2), y + pattern.Width, z).Color(color);
        }
    }

    private static void BuildVerticalDashedLine(ref SimpleMeshBuilder<Vertex2d> builder, float t, float x, float y1, float y2, float z, DashPattern pattern, bool reverse)
    {
        if (!reverse)
        {
            t = 1 - t;
        }

        var phase = t * pattern.Length;

        var color = pattern.Color;

        for (float y = y1 - phase; y < y2; y += pattern.Length)
        {
            // Clockwise, starting top-left
            builder.Vertex(x, Math.Clamp(y, y1, y2), z).Color(color);
            builder.Vertex(x + pattern.Width, Math.Clamp(y, y1, y2), z).Color(color);
            builder.Vertex(x + pattern.Width, Math.Clamp(y + pattern.OnLength, y1, y2), z).Color(color);
            builder.Vertex(x, Math.Clamp(y + pattern.OnLength, y1, y2), z).Color(color);
        }
    }

    public void DrawRectangle(float x, float y, float width, float height, ITexture texture)
    {
        DrawRectangle(x, y, width, height, texture, PackedLinearColorA.White);
    }

    public void DrawRectangle(
        Rectangle rectangle,
        ITexture? texture,
        PackedLinearColorA color,
        SamplerType2d samplerType = SamplerType2d.Clamp
    )
    {
        DrawRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, texture, color, samplerType);
    }

    public void DrawRectangle(
        RectangleF rect,
        ITexture? texture,
        PackedLinearColorA color,
        SamplerType2d samplerType = SamplerType2d.Clamp
    )
    {
        DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, texture, color, samplerType);
    }

    public void DrawRectangle(
        float x, float y, float width, float height,
        ITexture? texture,
        PackedLinearColorA color,
        SamplerType2d samplerType = SamplerType2d.Clamp
    )
    {
        // Generate the vertex data
        Span<Vertex2d> vertices = stackalloc Vertex2d[4];

        // Upper Left
        vertices[0].pos = new Vector4(x, y, 0.5f, 1);
        vertices[0].uv = new Vector2(0, 0);
        vertices[0].diffuse = color;

        // Upper Right
        vertices[1].pos = new Vector4(x + width, y, 0.5f, 1);
        vertices[1].uv = new Vector2(1, 0);
        vertices[1].diffuse = color;

        // Lower Right
        vertices[2].pos = new Vector4(x + width, y + height, 0.5f, 1);
        vertices[2].uv = new Vector2(1, 1);
        vertices[2].diffuse = color;

        // Lower Left
        vertices[3].pos = new Vector4(x, y + height, 0.5f, 1);
        vertices[3].uv = new Vector2(0, 1);
        vertices[3].diffuse = color;

        DrawRectangle(vertices, texture, null, samplerType);
    }

    public void DrawRectangle(RectangleF rectangle, PackedLinearColorA color)
    {
        DrawRectangle(rectangle, null, color);
    }

    public void DrawRectangle(Span<Vertex2d> corners,
        ITexture? texture,
        ITexture? mask = null,
        SamplerType2d samplerType = SamplerType2d.Clamp,
        bool blending = true)
    {
        var samplerState = GetSamplerState(samplerType);

        if (texture != null && mask != null)
        {
            Trace.Assert(blending);
            _device.SetMaterial(_texturedWithMaskMaterial);
            _device.SetSamplerState(0, samplerState);
            _device.SetSamplerState(1, samplerState);
            _device.SetTexture(0, texture);
            _device.SetTexture(1, mask);
        }
        else if (texture != null)
        {
            if (blending)
            {
                _device.SetMaterial(_texturedMaterial);
            }
            else
            {
                _device.SetMaterial(_texturedWithoutBlendingMaterial);
            }

            _device.SetSamplerState(0, samplerState);
            _device.SetTexture(0, texture);
        }
        else
        {
            _device.SetMaterial(_untexturedMaterial);
        }

        DrawRectangle(corners);

        if (texture != null)
        {
            _device.SetTexture(0, Textures.InvalidTexture);
        }

        if (mask != null)
        {
            _device.SetTexture(1, Textures.InvalidTexture);
        }
    }

    public void DrawRectangleWithMaterial(Span<Vertex2d> corners, IMdfRenderMaterial material)
    {
        MdfRenderOverrides? overrides = new MdfRenderOverrides();
        overrides.IgnoreLighting = true;
        overrides.UiProjection = true;
        material?.Bind((WorldCamera) null, _device, Array.Empty<Light3d>(), overrides);

        _device.SetDepthStencilState(_noDepthState);

        foreach (ref var vertex in corners)
        {
            vertex.normal = new Vector4(0, 0, -1, 0);
        }

        // Copy the vertices
        _device.UpdateBuffer<Vertex2d>(_vertexBuffer, corners);

        _mdfBufferBinding.Bind();

        _device.SetIndexBuffer(_indexBuffer);

        _device.DrawIndexed(PrimitiveType.TriangleList, 4, 6);
    }

    /// <param name="corners">Must be counter-clockwise.</param>
    public void DrawRectangle(Span<Vertex2d> corners)
    {
        foreach (ref var vertex in corners)
        {
            vertex.normal = new Vector4(0, 0, -1, 0);
        }

        // Copy the vertices
        _device.UpdateBuffer<Vertex2d>(_vertexBuffer, corners);

        _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);

        _bufferBinding.Resource.Bind();

        _device.SetIndexBuffer(_indexBuffer);

        _device.DrawIndexed(PrimitiveType.TriangleList, 4, 6);
    }

    public void DrawLines(ReadOnlySpan<Line2d> lines)
    {
        _device.SetMaterial(_lineMaterial);

        _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);

        _bufferBinding.Resource.Bind();

        // Render in batches of 4
        foreach (ref readonly var line in lines)
        {
            // Generate the vertex data
            using var locked = _device.Map<Vertex2d>(_vertexBuffer);

            var data = locked.Data;

            data[0].pos.X = line.from.X;
            data[0].pos.Y = line.from.Y;
            data[0].pos.Z = 0.5f;
            data[0].pos.W = 1;
            data[0].diffuse = line.diffuse;

            data[1].pos.X = line.to.X;
            data[1].pos.Y = line.to.Y;
            data[1].pos.Z = 0.5f;
            data[1].pos.W = 1;
            data[1].diffuse = line.diffuse;

            locked.Dispose();

            _device.Draw(PrimitiveType.LineList, 2);
        }
    }

    /// <summary>
    /// Draws a rectangle outline *inside* the given rectangle.
    /// </summary>
    [TempleDllLocation(0x101d8b70)]
    public void DrawRectangleOutline(RectangleF rectangle, PackedLinearColorA color, float strokeWidth = 1f)
    {
        var horHalfStroke = new Vector2(strokeWidth / 2, 0);
        var verHalfStroke = new Vector2(0, strokeWidth / 2);
        var topLeft = new Vector2(rectangle.Left, rectangle.Top);
        var topRight = new Vector2(rectangle.Right, rectangle.Top);
        var bottomRight = new Vector2(rectangle.Right, rectangle.Bottom);
        var bottomLeft = new Vector2(rectangle.Left, rectangle.Bottom);

        // Tessellate the lines
        var builder = SimpleMeshBuilder<Vertex2d>.Quads(4);
        using (builder)
        {
            // Top Edge
            builder.TessellateLine(topLeft + verHalfStroke, topRight + verHalfStroke, strokeWidth, color);
            // Bottom Edge
            builder.TessellateLine(bottomLeft - verHalfStroke, bottomRight - verHalfStroke, strokeWidth, color);
            // Left Edge (note the inset on top/bottom to not overlap with the top/bottom edges)
            builder.TessellateLine(topLeft + horHalfStroke + verHalfStroke, bottomLeft + horHalfStroke - verHalfStroke, strokeWidth, color);
            // Right Edge (note the inset on top/bottom to not overlap with the top/bottom edges)
            builder.TessellateLine(topRight - horHalfStroke + verHalfStroke, bottomRight - horHalfStroke - verHalfStroke, strokeWidth, color);

            BuildAndDrawMesh(ref builder, _untexturedMaterial);
        }
    }

    [TempleDllLocation(0x101d8b70)]
    public void DrawRectangleOutline(Vector2 topLeft, Vector2 bottomRight, PackedLinearColorA color)
    {
        topLeft.X += 0.5f;
        topLeft.Y += 0.5f;
        bottomRight.X -= 0.5f;
        bottomRight.Y -= 0.5f;

        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

        Span<Line2d> lines = stackalloc Line2d[4];
        lines[0] = new Line2d(topLeft, topRight, color);
        lines[1] = new Line2d(topRight, bottomRight, color);
        lines[2] = new Line2d(bottomRight, bottomLeft, color);
        lines[3] = new Line2d(bottomLeft, topLeft, color);
        DrawLines(lines);
    }

    public void DrawFullScreenQuad()
    {
        Span<Vertex2d> fullScreenCorners = stackalloc Vertex2d[4];
        fullScreenCorners[0].pos = new Vector4(-1, -1, 0, 1);
        fullScreenCorners[0].uv = new Vector2(0, 0);
        fullScreenCorners[1].pos = new Vector4(1, -1, 0, 1);
        fullScreenCorners[1].uv = new Vector2(1, 0);
        fullScreenCorners[2].pos = new Vector4(1, 1, 0, 1);
        fullScreenCorners[2].uv = new Vector2(1, 1);
        fullScreenCorners[3].pos = new Vector4(-1, 1, 0, 1);
        fullScreenCorners[3].uv = new Vector2(0, 1);
        DrawRectangle(fullScreenCorners);
    }

    private const int MaxSegments = 50;

    struct PieSegmentGlobals
    {
        public Matrix4x4 projMat;
        public Vector4 colors;
    };

    /// <summary>
    /// Renders a circle/pie segment for use with the radial menu.
    /// </summary>
    public void DrawPieSegment(int segments,
        int x, int y,
        float angleCenter, float angleWidth,
        float innerRadius, float innerOffset,
        float outerRadius, float outerOffset,
        PackedLinearColorA color1, PackedLinearColorA color2)
    {
        Trace.Assert(segments <= MaxSegments);

        var posCount = segments * 2 + 2;

        // There are two positions for the start and 2 more for each segment thereafter
        const int maxPositions = MaxSegments * 2 + 2;
        Span<Vertex2d> vertices = stackalloc Vertex2d[maxPositions];

        var angleStep = angleWidth / (posCount);
        var angleStart = angleCenter - angleWidth * 0.5f;

        // We generate one more position because of the starting points
        for (var i = 0; i < posCount; ++i)
        {
            var angle = angleStart + i * angleStep;
            ref var pos = ref vertices[i].pos;
            vertices[i].diffuse = color1;

            // The generated positions alternate between the outside
            // and inner circle
            if (i % 2 == 0)
            {
                pos.X = x + MathF.Cos(angle) * innerRadius - MathF.Sin(angle) * innerOffset;
                pos.Y = y + MathF.Sin(angle) * innerRadius + MathF.Cos(angle) * innerOffset;
            }
            else
            {
                pos.X = x + MathF.Cos(angle) * outerRadius - MathF.Sin(angle) * outerOffset;
                pos.Y = y + MathF.Sin(angle) * outerRadius + MathF.Cos(angle) * outerOffset;
            }

            pos.Z = 0;
            pos.W = 1;
        }

        _device.SetMaterial(_pieFillMaterial);

        using var bufferBindingRef = new BufferBinding(_device, _pieFillMaterial.VertexShader).Ref();
        var binding = bufferBindingRef.Resource;
        using var buffer = _device.CreateVertexBuffer<Vertex2d>(vertices, false, "ShapeRendererPieSegments");
        binding.AddBuffer<Vertex2d>(buffer, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        binding.Bind();

        PieSegmentGlobals globals;
        globals.projMat = _device.UiProjection;
        globals.colors = color1.ToRGBA();
        _device.SetVertexShaderConstants(0, ref globals);

        _device.Draw(PrimitiveType.TriangleStrip, posCount);

        _device.SetMaterial(_pieFillMaterial);

        // Change to the outline color
        globals.colors = color2.ToRGBA();
        _device.SetVertexShaderConstants(0, ref globals);

        // We generate one more position because of the starting points
        for (var i = 0; i < segments + 1; ++i)
        {
            vertices[i * 2].diffuse = color2;
            vertices[i * 2 + 1].diffuse = color2;
        }

        buffer.Resource.Update<Vertex2d>(vertices);

        /*
            Build an index buffer that draws an outline around the pie
            segment using the previously generated positions.
        */
        Span<ushort> outlineIndices = stackalloc ushort[maxPositions + 1];
        CreateOutlineIndices(posCount, outlineIndices);
        using var ib = _device.CreateIndexBuffer(outlineIndices);
        _device.SetIndexBuffer(ib);

        _device.DrawIndexed(PrimitiveType.LineStrip, posCount, posCount);
    }

    [TempleDllLocation(0x101D9300)]
    public bool DrawRectangle(ref Render2dArgs args)
    {
        float texwidth;
        float texheight;
        Span<Vertex2d> vertices = stackalloc Vertex2d[4];
        var srcX = args.srcRect.X;
        var srcY = args.srcRect.Y;
        var srcWidth = args.srcRect.Width;
        var srcHeight = args.srcRect.Height;

        // Has a special vertex z value been set? Otherwise we render all UI
        // on the same level
        var vertexZ = 0.5f;
        if ((args.flags & Render2dFlag.VERTEXZ) != 0)
        {
            vertexZ = args.vertexZ;
        }

        // Inherit vertex colors from the caller
        if ((args.flags & Render2dFlag.VERTEXCOLORS) != 0)
        {
            Debug.Assert(args.vertexColors.Length == 4);
            // Previously, ToEE tried to compute some gradient stuff here
            // which we removed because it was never actually utilized properly
            vertices[0].diffuse = args.vertexColors[0];
            vertices[0].diffuse.A = 0xFF;
            vertices[1].diffuse = args.vertexColors[1];
            vertices[2].diffuse.A = 0xFF;
            vertices[2].diffuse = args.vertexColors[2];
            vertices[2].diffuse.A = 0xFF;
            vertices[3].diffuse = args.vertexColors[3];
            vertices[3].diffuse.A = 0xFF;
        }
        else
        {
            vertices[0].diffuse = PackedLinearColorA.White;
            vertices[1].diffuse = PackedLinearColorA.White;
            vertices[2].diffuse = PackedLinearColorA.White;
            vertices[3].diffuse = PackedLinearColorA.White;
        }

        // Only if this flag is set, is the alpha value of
        // the vertex colors used
        if ((args.flags & Render2dFlag.VERTEXALPHA) != 0)
        {
            Debug.Assert(args.vertexColors.Length == 4);
            vertices[0].diffuse.A = args.vertexColors[0].A;
            vertices[1].diffuse.A = args.vertexColors[1].A;
            vertices[2].diffuse.A = args.vertexColors[2].A;
            vertices[3].diffuse.A = args.vertexColors[3].A;
        }

        // Load the associated texture
        ITexture deviceTexture = null;
        if ((args.flags & Render2dFlag.BUFFERTEXTURE) != 0)
        {
            // This is a custom flag we introduced for TP
            deviceTexture = args.customTexture;

            var size = deviceTexture.GetSize();
            texwidth = size.Width;
            texheight = size.Height;
        }
        else if ((args.flags & Render2dFlag.BUFFER) == 0)
        {
            if (args.textureId != 0)
            {
                var texture = _textures.GetById(args.textureId);
                if (texture == null || !texture.IsValid() || texture.GetResourceView() == null)
                {
                    return false;
                }

                deviceTexture = texture;

                var size = texture.GetSize();
                texwidth = size.Width;
                texheight = size.Height;
            }
            else
            {
                texwidth = args.destRect.Width;
                texheight = args.destRect.Height;
            }
        }
        else
        {
            throw new Exception("Unsupported operation mode for TextureRender2d");
        }

        var contentRectLeft = srcX;
        var contentRectTop = srcY;
        var contentRectRight = srcX + srcWidth;
        var contentRectBottom = srcY + srcHeight;

        // Create the UV coordinates to honor the contentRect based
        // on the real texture size
        var uvLeft = (contentRectLeft) / texwidth;
        var uvRight = (contentRectRight) / texwidth;
        var uvTop = (contentRectTop) / texheight;
        var uvBottom = (contentRectBottom) / texheight;
        vertices[0].uv.X = uvLeft;
        vertices[0].uv.Y = uvTop;
        vertices[1].uv.X = uvRight;
        vertices[1].uv.Y = uvTop;
        vertices[2].uv.X = uvRight;
        vertices[2].uv.Y = uvBottom;
        vertices[3].uv.X = uvLeft;
        vertices[3].uv.Y = uvBottom;

        // Flip the U coordinates horizontally
        if ((args.flags & Render2dFlag.FLIPH) != 0)
        {
            // Top Left with Top Right
            Swap(ref vertices[0].uv.X, ref vertices[1].uv.X);
            // Bottom Right with Bottom Left
            Swap(ref vertices[2].uv.X, ref vertices[3].uv.X);
        }

        // Flip the V coordinates horizontally
        if ((args.flags & Render2dFlag.FLIPV) != 0)
        {
            // Top Left with Bottom Left
            Swap(ref vertices[0].uv.Y, ref vertices[3].uv.Y);
            // Top Right with Bottom Right
            Swap(ref vertices[1].uv.Y, ref vertices[2].uv.Y);
        }

        float destX = args.destRect.X;
        float destY = args.destRect.Y;

        if ((args.flags & Render2dFlag.ROTATE) != 0)
        {
            // Rotation?
            var cosRot = MathF.Cos(args.rotation);
            var sinRot = MathF.Sin(args.rotation);
            var destRect = args.destRect;
            vertices[0].pos.X = args.rotationX
                                + (destX - args.rotationX) * cosRot
                                - (destY - args.rotationY) * sinRot;
            vertices[0].pos.Y = args.rotationY
                                + (destY - args.rotationY) * cosRot
                                + (destX - args.rotationX) * sinRot;
            vertices[0].pos.Z = vertexZ;

            vertices[1].pos.X = args.rotationX
                                + ((destX + destRect.Width) - args.rotationX) * cosRot
                                - (destY - args.rotationY) * sinRot;
            vertices[1].pos.Y = args.rotationY
                                + ((destX + destRect.Width) - args.rotationX) * sinRot
                                + (destY - args.rotationY) * cosRot;
            vertices[1].pos.Z = vertexZ;

            vertices[2].pos.X = args.rotationX
                                + ((destX + destRect.Width) - args.rotationX) * cosRot
                                - ((destY + destRect.Width) - args.rotationY) * sinRot;
            vertices[2].pos.Y = args.rotationY
                                + ((destY + destRect.Width) - args.rotationY) * cosRot
                                + (destX + destRect.Width - args.rotationX) * sinRot;
            vertices[2].pos.Z = vertexZ;

            vertices[3].pos.X = args.rotationX
                                + (destX - args.rotationX) * cosRot
                                - ((destY + destRect.Height) - args.rotationY) * sinRot;
            vertices[3].pos.Y = args.rotationY
                                + ((destY + destRect.Height) - args.rotationY) * cosRot
                                + (destX - args.rotationX) * sinRot;
            vertices[3].pos.Z = vertexZ;
        }
        else
        {
            var destRect = args.destRect;
            vertices[0].pos.X = destX;
            vertices[0].pos.Y = destY;
            vertices[0].pos.Z = vertexZ;
            vertices[1].pos.X = destX + destRect.Width;
            vertices[1].pos.Y = destY;
            vertices[1].pos.Z = vertexZ;
            vertices[2].pos.X = destX + destRect.Width;
            vertices[2].pos.Y = destY + destRect.Height;
            vertices[2].pos.Z = vertexZ;
            vertices[3].pos.X = destX;
            vertices[3].pos.Y = destY + destRect.Height;
            vertices[3].pos.Z = vertexZ;
        }

        vertices[0].pos.W = 1;
        vertices[1].pos.W = 1;
        vertices[2].pos.W = 1;
        vertices[3].pos.W = 1;

        ITexture maskTexture = null;
        // We have a secondary texture
        if ((args.flags & Render2dFlag.MASK) != 0)
        {
            var texture = args.maskTexture;
            if (texture == null || !texture.IsValid() || texture.GetResourceView() == null)
            {
                return false;
            }

            maskTexture = texture;
        }

        // This is used by the portrait UI to mask the equipment slot background when
        // rendering an icon
        var blending = ((args.flags & Render2dFlag.DISABLEBLENDING) == 0);

        SamplerType2d samplerType = SamplerType2d.Clamp;
        if ((args.flags & Render2dFlag.WRAP) != 0)
        {
            samplerType = SamplerType2d.Wrap;
        }

        DrawRectangle(vertices, deviceTexture, maskTexture, samplerType, blending);
        return true;
    }

    private static void Swap(ref float a, ref float b)
    {
        var tmp = a;
        a = b;
        b = tmp;
    }

    private static void CreateOutlineIndices(int posCount, Span<ushort> outlineIndices)
    {
        var i = 0;
        var j = 0;
        // The first run of indices is along the inner radius
        while (i < posCount / 2)
        {
            outlineIndices[i++] = (ushort) j;
            j += 2;
        }

        // Then backwards along the outer radius
        j = posCount - 1;
        while (i < posCount)
        {
            outlineIndices[i++] = (ushort) j;
            j -= 2;
        }

        // And finally it goes back to the starting point
        outlineIndices[posCount] = 0;
    }

    private SamplerState GetSamplerState(SamplerType2d type)
    {
        switch (type)
        {
            default:
            case SamplerType2d.Clamp:
                return _samplerClampState;
            case SamplerType2d.Point:
                return _samplerClampPointState;
            case SamplerType2d.Wrap:
                return _samplerWrapState;
        }
    }

    public void Dispose()
    {
        _bufferBinding.Dispose();
        _lineBufferBinding.Dispose();
        _samplerWrapState.Dispose();
        _samplerClampPointState.Dispose();
        _samplerClampState.Dispose();
        _noDepthState.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}

[Flags]
public enum Render2dFlag
{
    VERTEXCOLORS = 1,
    VERTEXZ = 2,
    VERTEXALPHA = 4,
    FLIPH = 0x10,
    FLIPV = 0x20,
    UNK = 0x40, // Does not seem to have an effect
    BUFFER = 0x80,
    ROTATE = 0x200,
    MASK = 0x400,

    // This is not 100% correct, but it should do the trick
    DISABLEBLENDING = 0x800,
    WRAP = 0x1000,
    BUFFERTEXTURE = 0x2000
}

public struct Render2dArgs
{
    public Render2dFlag flags;
    public int textureId; // Unused for shaders
    public ITexture maskTexture;
    public IntPtr texBuffer; // Unused for shaders
    public ITexture customTexture;
    public int shaderId;
    public RectangleF srcRect;
    public RectangleF destRect;
    public PackedLinearColorA[] vertexColors;
    public float vertexZ;
    public float rotation;
    public float rotationX;
    public float rotationY;
}

public readonly record struct DashPattern(float Width, float OnLength, float OffLength, PackedLinearColorA Color, float AnimationCycleMs)
{
    public float Length => OnLength + OffLength;

    public DashPattern Scale(float scale)
    {
        return new DashPattern(Width * scale, OnLength * scale, OffLength * scale, Color, AnimationCycleMs);
    }
}

public static class Vertex2dExtensions
{
    public static void TessellateLine(ref this SimpleMeshBuilder<Vertex2d> builder, Vector2 from, Vector2 to, float strokeWidth, PackedLinearColorA color)
    {
        Span<PackedLinearColorA> colors = stackalloc PackedLinearColorA[4]
        {
            color, color, color, color
        };
        TessellateLine(ref builder, from, to, strokeWidth, colors);
    }

    /// <summary>
    /// Tessellates a line using "butt" caps. The line starts and ends (in direction from->to) immediately on the given coordinate
    /// and extends 1/2 strokeWidth to the "left" and "right" (in relation to the from->to direction vector). 
    /// </summary>
    public static void TessellateLine(ref this SimpleMeshBuilder<Vertex2d> builder, Vector2 from, Vector2 to, float strokeWidth, scoped ReadOnlySpan<PackedLinearColorA> colors)
    {
        var dirNorm = Vector2.Normalize(to - from);
        var strokeRight = new Vector2(-dirNorm.Y, dirNorm.X) * strokeWidth / 2;
        var strokeLeft = -strokeRight;

        // Vertices clock-wise
        builder.Vertex(from + strokeLeft).Color(colors[0]);
        builder.Vertex(to + strokeLeft).Color(colors[1]);
        builder.Vertex(to + strokeRight).Color(colors[2]);
        builder.Vertex(from + strokeRight).Color(colors[3]);
    }

    public static ref Vertex2d Vertex(ref this SimpleMeshBuilder<Vertex2d> builder, float x, float y, float z = 0)
    {
        ref var vertex = ref builder.Vertex();
        vertex.pos = new Vector4(x, y, z, 1);
        return ref vertex;
    }

    public static ref Vertex2d Vertex(ref this SimpleMeshBuilder<Vertex2d> builder, Vector2 pos, float z = 0)
    {
        ref var vertex = ref builder.Vertex();
        vertex.pos = new Vector4(pos.X, pos.Y, z, 1);
        return ref vertex;
    }

    public static ref Vertex2d Color(ref this Vertex2d vertex, PackedLinearColorA color)
    {
        vertex.diffuse = color;
        return ref vertex;
    }
}