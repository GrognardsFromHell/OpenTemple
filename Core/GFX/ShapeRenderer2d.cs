using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.GFX.RenderMaterials;

namespace OpenTemple.Core.GFX;

public enum SamplerType2d
{
    CLAMP,
    WRAP,
    POINT
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
public struct Vertex2d
{
    [FieldOffset(0)]
    public Vector4 pos;

    [FieldOffset(16)]
    public Vector4 normal;

    [FieldOffset(32)]
    public PackedLinearColorA diffuse;

    [FieldOffset(36)]
    public Vector2 uv;

    public static readonly int Size = Marshal.SizeOf<Vertex2d>();
}

public sealed class ShapeRenderer2d : IDisposable
{
    private readonly RenderingDevice _device;
    private readonly Textures _textures;
    private Material untexturedMaterial;
    private Material texturedMaterial;
    private Material texturedWithoutBlendingMaterial;
    private Material texturedWithMaskMaterial;
    private Material lineMaterial;
    private BufferBinding mdfBufferBinding;
    private Material outlineMaterial;
    private Material pieFillMaterial;
    private ResourceRef<BufferBinding> bufferBinding;
    private ResourceRef<BufferBinding> lineBufferBinding;
    private ResourceRef<SamplerState> samplerWrapState;
    private ResourceRef<SamplerState> samplerClampPointState;
    private ResourceRef<SamplerState> samplerClampState;
    private ResourceRef<DepthStencilState> noDepthState;
    private ResourceRef<VertexBuffer> vertexBuffer;
    private ResourceRef<IndexBuffer> indexBuffer;

    public ShapeRenderer2d(RenderingDevice device)
    {
        _device = device;
        _textures = device.GetTextures();

        untexturedMaterial = CreateMaterial(device, "diffuse_only_ps");
        texturedMaterial = CreateMaterial(device, "textured_simple_ps");
        texturedWithoutBlendingMaterial = CreateMaterial(device, "textured_simple_ps", false, false);
        texturedWithMaskMaterial = CreateMaterial(device, "textured_two_ps");
        lineMaterial = CreateMaterial(device, "diffuse_only_ps", true);
        outlineMaterial = CreateOutlineMaterial(device);
        pieFillMaterial = CreatePieFillMaterial(device);
        bufferBinding = new BufferBinding(device, texturedMaterial.VertexShader).Ref();
        mdfBufferBinding = _device.CreateMdfBufferBinding();
        lineBufferBinding = new BufferBinding(device, lineMaterial.VertexShader).Ref();


        SamplerSpec samplerWrapSpec = new SamplerSpec();
        samplerWrapSpec.addressU = TextureAddress.Wrap;
        samplerWrapSpec.addressV = TextureAddress.Wrap;
        samplerWrapSpec.minFilter = TextureFilterType.Linear;
        samplerWrapSpec.magFilter = TextureFilterType.Linear;
        samplerWrapSpec.mipFilter = TextureFilterType.Linear;
        samplerWrapState = _device.CreateSamplerState(samplerWrapSpec);

        SamplerSpec samplerClampPointSpec = new SamplerSpec();
        samplerClampPointSpec.addressU = TextureAddress.Clamp;
        samplerClampPointSpec.addressV = TextureAddress.Clamp;
        samplerClampPointSpec.minFilter = TextureFilterType.NearestNeighbor;
        samplerClampPointSpec.magFilter = TextureFilterType.NearestNeighbor;
        samplerClampPointSpec.mipFilter = TextureFilterType.NearestNeighbor;
        samplerClampPointState = _device.CreateSamplerState(samplerClampPointSpec);

        SamplerSpec samplerClampSpec = new SamplerSpec();
        samplerClampSpec.addressU = TextureAddress.Clamp;
        samplerClampSpec.addressV = TextureAddress.Clamp;
        samplerClampSpec.minFilter = TextureFilterType.Linear;
        samplerClampSpec.magFilter = TextureFilterType.Linear;
        samplerClampSpec.mipFilter = TextureFilterType.Linear;
        samplerClampState = _device.CreateSamplerState(samplerClampSpec);

        DepthStencilSpec noDepthSpec = new DepthStencilSpec();
        noDepthSpec.depthEnable = false;
        noDepthState = _device.CreateDepthStencilState(noDepthSpec);

        vertexBuffer = _device.CreateEmptyVertexBuffer(Vertex2d.Size * 8, debugName:"ShapeRenderer2d");

        var indexData = new ushort[]
        {
            0, 1, 2,
            2, 3, 0
        };
        indexBuffer = _device.CreateIndexBuffer(indexData);

        bufferBinding.Resource.AddBuffer<Vertex2d>(vertexBuffer, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

        mdfBufferBinding.AddBuffer<Vertex2d>(vertexBuffer, 0)
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

    public void DrawRectangle(float x, float y, float width, float height, ITexture texture)
    {
        DrawRectangle(x, y, width, height, texture, PackedLinearColorA.White);
    }

    public void DrawRectangle(
        Rectangle rectangle,
        ITexture texture,
        PackedLinearColorA color,
        SamplerType2d samplerType = SamplerType2d.CLAMP
    )
    {
        DrawRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, texture, color, samplerType);
    }

    public void DrawRectangle(
        float x, float y, float width, float height,
        ITexture texture,
        PackedLinearColorA color,
        SamplerType2d samplerType = SamplerType2d.CLAMP
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

    public void DrawRectangle(Span<Vertex2d> corners,
        ITexture texture,
        ITexture? mask = null,
        SamplerType2d samplerType = SamplerType2d.CLAMP,
        bool blending = true)
    {
        var samplerState = getSamplerState(samplerType);

        if (texture != null && mask != null)
        {
            Trace.Assert(blending);
            _device.SetMaterial(texturedWithMaskMaterial);
            _device.SetSamplerState(0, samplerState);
            _device.SetSamplerState(1, samplerState);
            _device.SetTexture(0, texture);
            _device.SetTexture(1, mask);
        }
        else if (texture != null)
        {
            if (blending)
            {
                _device.SetMaterial(texturedMaterial);
            }
            else
            {
                _device.SetMaterial(texturedWithoutBlendingMaterial);
            }

            _device.SetSamplerState(0, samplerState);
            _device.SetTexture(0, texture);
        }
        else
        {
            _device.SetMaterial(untexturedMaterial);
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
        overrides.ignoreLighting = true;
        overrides.uiProjection = true;
        material?.Bind((WorldCamera) null, _device, Array.Empty<Light3d>(), overrides);

        _device.SetDepthStencilState(noDepthState);

        foreach (ref var vertex in corners)
        {
            vertex.normal = new Vector4(0, 0, -1, 0);
        }

        // Copy the vertices
        _device.UpdateBuffer<Vertex2d>(vertexBuffer, corners);

        mdfBufferBinding.Bind();

        _device.SetIndexBuffer(indexBuffer);

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
        _device.UpdateBuffer<Vertex2d>(vertexBuffer, corners);

        _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);

        bufferBinding.Resource.Bind();

        _device.SetIndexBuffer(indexBuffer);

        _device.DrawIndexed(PrimitiveType.TriangleList, 4, 6);
    }

    public void DrawLines(ReadOnlySpan<Line2d> lines)
    {
        _device.SetMaterial(lineMaterial);

        _device.SetVertexShaderConstant(0, StandardSlotSemantic.UiProjMatrix);

        bufferBinding.Resource.Bind();

        // Render in batches of 4
        foreach (ref readonly var line in lines)
        {
            // Generate the vertex data
            using var locked = _device.Map<Vertex2d>(vertexBuffer);

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

    [TempleDllLocation(0x101d8b70)]
    public void DrawRectangleOutline(Rectangle rectangle, PackedLinearColorA color)
    {
        var topLeft = new Vector2(rectangle.Left + 0.5f, rectangle.Top + 0.5f);
        var topRight = new Vector2(rectangle.Right - 0.5f, rectangle.Top + 0.5f);
        var bottomRight = new Vector2(rectangle.Right - 0.5f, rectangle.Bottom - 0.5f );
        var bottomLeft = new Vector2(rectangle.Left + 0.5f, rectangle.Bottom - 0.5f);

        Span<Line2d> lines = stackalloc Line2d[4];
        lines[0] = new Line2d(topLeft, topRight, color);
        lines[1] = new Line2d(topRight, bottomRight, color);
        lines[2] = new Line2d(bottomRight, bottomLeft, color);
        lines[3] = new Line2d(bottomLeft, topLeft, color);
        DrawLines(lines);
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
        const int MaxPositions = MaxSegments * 2 + 2;
        Span<Vertex2d> vertices = stackalloc Vertex2d[MaxPositions];

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

        _device.SetMaterial(pieFillMaterial);

        using var bufferBindingRef = new BufferBinding(_device, pieFillMaterial.VertexShader).Ref();
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

        _device.SetMaterial(pieFillMaterial);

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
        Span<ushort> outlineIndices = stackalloc ushort[MaxPositions + 1];
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
        float srcX;
        float srcY;
        float srcWidth;
        float srcHeight;
        Span<Vertex2d> vertices = stackalloc Vertex2d[4];

        // The townmap UI uses floating point coordinates for the srcrect
        // for whatever reason. They are passed in place of the integer coordinates
        // And need to be reinterpreted
        if ((args.flags & Render2dFlag.FLOATSRCRECT) != 0)
        {
            srcX = args.srcRectFloat.X;
            srcY = args.srcRectFloat.Y;
            srcWidth = args.srcRectFloat.Width;
            srcHeight = args.srcRectFloat.Height;
        }
        else
        {
            srcX = args.srcRect.X;
            srcY = args.srcRect.Y;
            srcWidth = args.srcRect.Width;
            srcHeight = args.srcRect.Height;
        }

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

        SamplerType2d samplerType = SamplerType2d.CLAMP;
        if ((args.flags & Render2dFlag.WRAP) != 0)
        {
            samplerType = SamplerType2d.WRAP;
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

    private SamplerState getSamplerState(SamplerType2d type)
    {
        switch (type)
        {
            default:
            case SamplerType2d.CLAMP:
                return samplerClampState;
            case SamplerType2d.POINT:
                return samplerClampPointState;
            case SamplerType2d.WRAP:
                return samplerWrapState;
        }
    }

    public void Dispose()
    {
        bufferBinding.Dispose();
        lineBufferBinding.Dispose();
        samplerWrapState.Dispose();
        samplerClampPointState.Dispose();
        samplerClampState.Dispose();
        noDepthState.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
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
    FLOATSRCRECT = 0x100,
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
    public Rectangle srcRect;
    public RectangleF srcRectFloat;
    public Rectangle destRect;
    public PackedLinearColorA[] vertexColors;
    public float vertexZ;
    public float rotation;
    public float rotationX;
    public float rotationY;
}