using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.GFX;

[StructLayout(LayoutKind.Sequential)]
public struct ShapeVertex3d
{
    public Vector4 pos;
    public Vector4 normal;
    public Vector2 uv;

    public static readonly int Size = Marshal.SizeOf<ShapeVertex3d>();
}

[StructLayout(LayoutKind.Sequential)]
public struct Shape3dGlobals
{
    public Matrix4x4 viewProj;
    public Vector4 colors;
}

public class ShapeRenderer3d : IDisposable
{
    // When drawing circles, how many segments do we use?
    private const int CircleSegments = 74;

    private readonly RenderingDevice _device;

    private readonly ResourceLifecycleCallbacks _resourceListener;

    private ResourceRef<Material> _lineMaterial;

    private ResourceRef<Material> _quadMaterial;

    private ResourceRef<Material> _lineOccludedMaterial;

    private ResourceRef<BufferBinding> _discBufferBinding;

    private ResourceRef<BufferBinding> _lineBinding;

    private ResourceRef<BufferBinding> _circleBinding;

    private readonly ShapeVertex3d[] _discVerticesTpl = new ShapeVertex3d[16];

    private ResourceRef<IndexBuffer> _discIndexBuffer;
    private ResourceRef<VertexBuffer> _discVertexBuffer;

    private ResourceRef<VertexBuffer> _circleVertexBuffer;

    private ResourceRef<IndexBuffer> _circleIndexBuffer;

    private ResourceRef<VertexBuffer> _lineVertexBuffer;

    public ShapeRenderer3d(RenderingDevice device)
    {
        _device = device;

        _lineMaterial = CreateLineMaterial(false).Ref();
        _quadMaterial = CreateQuadMaterial().Ref();
        _lineOccludedMaterial = CreateLineMaterial(true).Ref();
        _discBufferBinding = _device.CreateMdfBufferBinding().Ref();
        _lineBinding = new BufferBinding(_device, _lineMaterial.Resource.VertexShader).Ref();
        _circleBinding = new BufferBinding(_device, _lineMaterial.Resource.VertexShader).Ref();

        _discVerticesTpl[0].pos = new Vector4(-1.0f, 0.0f, -1.0f, 1);
        _discVerticesTpl[1].pos = new Vector4(0.0f, 0.0f, -1.0f, 1);
        _discVerticesTpl[2].pos = new Vector4(0.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[3].pos = new Vector4(-1.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[4].pos = new Vector4(0.0f, 0.0f, -1.0f, 1);
        _discVerticesTpl[5].pos = new Vector4(1.0f, 0.0f, -1.0f, 1);
        _discVerticesTpl[6].pos = new Vector4(1.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[7].pos = new Vector4(0.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[8].pos = new Vector4(0.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[9].pos = new Vector4(1.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[10].pos = new Vector4(1.0f, 0.0f, 1.0f, 1);
        _discVerticesTpl[11].pos = new Vector4(0.0f, 0.0f, 1.0f, 1);
        _discVerticesTpl[12].pos = new Vector4(-1.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[13].pos = new Vector4(0.0f, 0.0f, 0.0f, 1);
        _discVerticesTpl[14].pos = new Vector4(0.0f, 0.0f, 1.0f, 1);
        _discVerticesTpl[15].pos = new Vector4(-1.0f, 0.0f, 1.0f, 1);

        for (var i = 0; i < _discVerticesTpl.Length; i++)
        {
            _discVerticesTpl[i].normal = new Vector4(0, 1, 0, 0);
        }

        _discVerticesTpl[0].uv = new Vector2(0, 0);
        _discVerticesTpl[1].uv = new Vector2(1, 0);
        _discVerticesTpl[2].uv = new Vector2(1, 1);
        _discVerticesTpl[3].uv = new Vector2(0, 1);
        _discVerticesTpl[4].uv = new Vector2(0, 1);
        _discVerticesTpl[5].uv = new Vector2(0, 0);
        _discVerticesTpl[6].uv = new Vector2(1, 0);
        _discVerticesTpl[7].uv = new Vector2(1, 1);
        _discVerticesTpl[8].uv = new Vector2(1, 1);
        _discVerticesTpl[9].uv = new Vector2(0, 1);
        _discVerticesTpl[10].uv = new Vector2(0, 0);
        _discVerticesTpl[11].uv = new Vector2(1, 0);
        _discVerticesTpl[12].uv = new Vector2(1, 0);
        _discVerticesTpl[13].uv = new Vector2(1, 1);
        _discVerticesTpl[14].uv = new Vector2(0, 1);
        _discVerticesTpl[15].uv = new Vector2(0, 0);

        _resourceListener = new ResourceLifecycleCallbacks(_device, CreateResources, FreeResources);
    }

    public void Dispose()
    {
        _resourceListener.Dispose();
        _lineMaterial.Dispose();
        _quadMaterial.Dispose();
        _lineOccludedMaterial.Dispose();
        _discBufferBinding.Dispose();
        _lineBinding.Dispose();
        _circleBinding.Dispose();
    }

    private Material CreateLineMaterial(bool occludedOnly)
    {
        var blendState = new BlendSpec();
        blendState.blendEnable = true;
        blendState.srcBlend = BlendOperand.SrcAlpha;
        blendState.destBlend = BlendOperand.InvSrcAlpha;
        var depthState = new DepthStencilSpec();
        depthState.depthEnable = true;
        depthState.depthWrite = false;
        if (occludedOnly)
        {
            depthState.depthFunc = ComparisonFunc.GreaterEqual;
        }

        using var vs = _device.GetShaders().LoadVertexShader("line_vs");
        using var ps = _device.GetShaders().LoadPixelShader("diffuse_only_ps");

        return _device.CreateMaterial(
            blendState,
            depthState,
            null,
            null,
            vs.Resource,
            ps.Resource
        );
    }

    private Material CreateQuadMaterial()
    {
        var blendState = new BlendSpec
        {
            blendEnable = true, srcBlend = BlendOperand.SrcAlpha, destBlend = BlendOperand.InvSrcAlpha
        };
        var depthState = new DepthStencilSpec();
        depthState.depthEnable = true;
        depthState.depthWrite = false;
        var rasterizerState = new RasterizerSpec();
        rasterizerState.cullMode = CullMode.None;
        var vsDefines = new Dictionary<string, string>
        {
            {"TEXTURE_STAGES", "1"}
        };
        var vs = _device.GetShaders().LoadVertexShader("mdf_vs", vsDefines);
        var ps = _device.GetShaders().LoadPixelShader("textured_simple_ps");
        var samplerState = new SamplerSpec();
        samplerState.addressU = TextureAddress.Clamp;
        samplerState.addressV = TextureAddress.Clamp;
        samplerState.minFilter = TextureFilterType.Linear;
        samplerState.magFilter = TextureFilterType.Linear;
        samplerState.mipFilter = TextureFilterType.Linear;

        var samplers = new[] {new MaterialSamplerSpec(new ResourceRef<ITexture>(null), samplerState)};

        return _device.CreateMaterial(
            blendState,
            depthState,
            rasterizerState,
            samplers,
            vs,
            ps);
    }

    private void CreateResources(RenderingDevice obj)
    {
        Span<ushort> indices = stackalloc ushort[]
        {
            0, 2, 1,
            0, 3, 2,
            4, 6, 5,
            4, 7, 6,
            8, 10, 9,
            8, 11, 10,
            12, 14, 13,
            12, 15, 14
        };
        _discIndexBuffer = _device.CreateIndexBuffer(indices);

        _discVertexBuffer =
            _device.CreateEmptyVertexBuffer(ShapeVertex3d.Size * 16, debugName: "ShapeRenderer3dDisc");

        _discBufferBinding.Resource.AddBuffer<ShapeVertex3d>(_discVertexBuffer, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

        // +3 because for n lines, you need n+1 points and in this case, we have to repeat
        // the first point again to close the loop (so +2) and also include the center point
        // to draw a circle at the end (+3)
        _circleVertexBuffer = _device.CreateEmptyVertexBuffer(Marshal.SizeOf<Vector3>() * (CircleSegments + 3),
            debugName: "ShapeRenderer3dCircle");

        // Pre-generate the circle indexbuffer.
        // One triangle per circle segment
        Span<ushort> circleIndices = stackalloc ushort[CircleSegments * 3];
        for (ushort i = 0; i < CircleSegments; i++)
        {
            circleIndices[i * 3] = (ushort) (i + 2);
            circleIndices[i * 3 + 1] = 0; // The center point has to be in the triangle
            circleIndices[i * 3 + 2] = (ushort) (i + 1);
        }

        _circleIndexBuffer = _device.CreateIndexBuffer(circleIndices, true);

        _circleBinding.Resource.AddBuffer<Vector3>(_circleVertexBuffer, 0)
            .AddElement(VertexElementType.Float3, VertexElementSemantic.Position);

        // Just the two end points of a line
        _lineVertexBuffer =
            _device.CreateEmptyVertexBuffer(Marshal.SizeOf<Vector3>() * 2, debugName: "ShapeRenderer3dLine");
        _lineBinding.Resource.AddBuffer<Vector3>(_lineVertexBuffer, 0)
            .AddElement(VertexElementType.Float3, VertexElementSemantic.Position);
    }

    private void FreeResources(RenderingDevice obj)
    {
        _discVertexBuffer.Dispose();
        _discIndexBuffer.Dispose();
    }


    private void BindLineMaterial(IGameViewport viewport, PackedLinearColorA color, bool occludedOnly = false)
    {
        if (occludedOnly)
        {
            _device.SetMaterial(_lineOccludedMaterial);
        }
        else
        {
            _device.SetMaterial(_lineMaterial);
        }

        Shape3dGlobals globals;
        globals.viewProj = viewport.Camera.GetViewProj();
        globals.colors = color.ToRGBA();

        _device.SetVertexShaderConstants(0, ref globals);
    }

    private void BindQuadMaterial(IGameViewport viewport, PackedLinearColorA color, ITexture texture)
    {
        _device.SetMaterial(_quadMaterial);

        Shape3dGlobals globals;
        globals.viewProj = viewport.Camera.GetViewProj();
        globals.colors = color.ToRGBA();

        _device.SetVertexShaderConstants(0, ref globals);

        _device.SetTexture(0, texture);
    }


    public void DrawQuad(IGameViewport viewport,
        ReadOnlySpan<ShapeVertex3d> corners,
        PackedLinearColorA color,
        ITexture texture)
    {
        _discVertexBuffer.Resource.Update(corners);
        _discBufferBinding.Resource.Bind();

        BindQuadMaterial(viewport, color, texture);

        _device.SetIndexBuffer(_discIndexBuffer);
        _device.DrawIndexed(PrimitiveType.TriangleList, 4, 2 * 3);
    }

    public void DrawQuad(IGameViewport viewport, ReadOnlySpan<ShapeVertex3d> corners,
        IMdfRenderMaterial material,
        PackedLinearColorA color)
    {
        _discVertexBuffer.Resource.Update(corners);
        _discBufferBinding.Resource.Bind();

        MdfRenderOverrides? overrides = new MdfRenderOverrides();
        overrides.overrideDiffuse = true;
        overrides.overrideColor = color;
        material.Bind(viewport, _device, null, overrides);

        _device.SetIndexBuffer(_discIndexBuffer);
        _device.DrawIndexed(PrimitiveType.TriangleList, 4, 2 * 3);
    }

    [TempleDllLocation(0x10107050)]
    public void DrawDisc(IGameViewport viewport, Vector3 center,
        float rotation,
        float radius,
        IMdfRenderMaterial material)
    {
        Span<ShapeVertex3d> vertices = stackalloc ShapeVertex3d[_discVerticesTpl.Length];
        _discVerticesTpl.CopyTo(vertices);

        // There is some sort of rotation going on here that is related to
        // the view transformation
        var v8 = MathF.Cos(-0.77539754f) * -1.5f;
        var v9 = MathF.Sin(-0.77539754f) * -1.5f;

        for (var i = 0; i < _discVerticesTpl.Length; ++i)
        {
            var orgx = _discVerticesTpl[i].pos.X;
            var orgz = _discVerticesTpl[i].pos.Z;

            // The cos/sin magic rotates around the Y axis
            vertices[i].pos.X = MathF.Sin(rotation) * orgz + MathF.Cos(rotation) * orgx;
            vertices[i].pos.Y = v9;
            vertices[i].pos.Z = MathF.Cos(rotation) * orgz - MathF.Sin(rotation) * orgx;

            // Scale is being applied here
            vertices[i].pos.X = radius * vertices[i].pos.X;
            vertices[i].pos.Z = radius * vertices[i].pos.Z;

            vertices[i].pos.X = center.X + MathF.Cos(2.3561945f) * v8 + vertices[i].pos.X;
            vertices[i].pos.Z = center.Z + MathF.Cos(2.3561945f) * v8 + vertices[i].pos.Z;
        }

        _discVertexBuffer.Resource.Update<ShapeVertex3d>(vertices);
        _discBufferBinding.Resource.Bind();
        material.Bind(viewport, _device, null);

        _device.SetIndexBuffer(_discIndexBuffer);
        _device.DrawIndexed(PrimitiveType.TriangleList, 16, 8 * 3);
    }

    public void DrawLine(IGameViewport viewport,
        Vector3 from,
        Vector3 to,
        PackedLinearColorA color)
    {
        Span<Vector3> positions = stackalloc Vector3[]
        {
            from,
            to
        };

        _lineVertexBuffer.Resource.Update<Vector3>(positions);
        _lineBinding.Resource.Bind();
        BindLineMaterial(viewport, color);

        _device.Draw(PrimitiveType.LineList, 2);
    }

    public void DrawLineWithoutDepth(IGameViewport viewport,
        Vector3 from,
        Vector3 to,
        PackedLinearColorA color)
    {
        Span<Vector3> positions = stackalloc Vector3[]
        {
            from,
            to
        };

        _lineVertexBuffer.Resource.Update<Vector3>(positions);
        _lineBinding.Resource.Bind();
        BindLineMaterial(viewport, color);
        _device.Draw(PrimitiveType.LineList, 2);

        BindLineMaterial(viewport, color, true);
        _device.Draw(PrimitiveType.LineList, 2);
    }

    private const float cos45 = 0.70709997f;

    // Draw the 3d bounding cylinder of the object
    private static readonly PackedLinearColorA CylinderDiffuse = PackedLinearColorA.OfFloats(0, 1.0f, 0, 0.5f);

    public void DrawCylinder(IGameViewport viewport, Vector3 pos, float radius, float height)
    {
        float x = pos.X;
        float y = pos.Y;
        float z = pos.Z;

        var scaledRadius = radius * cos45;
        Vector3 from, to;

        from.X = x + scaledRadius;
        from.Y = y;
        from.Z = z - scaledRadius;
        to.X = from.X;
        to.Y = y + height;
        to.Z = from.Z;

        DrawLine(viewport, from, to, CylinderDiffuse);

        from.X = x - scaledRadius;
        from.Z = z + scaledRadius;
        to.X = from.X;
        to.Z = from.Z;
        DrawLine(viewport, from, to, CylinderDiffuse);

        /*
        Draw the circle on top and on the bottom
        of the cylinder.
        */
        for (var i = 0; i < 24; ++i)
        {
            // We rotate 360° in 24 steps of 15° each
            var rot = i * Angles.ToRadians(15);
            var nextRot = rot + Angles.ToRadians(15);

            // This is the bottom cap
            from.X = x + MathF.Cos(rot) * radius;
            from.Y = y;
            from.Z = z - MathF.Sin(rot) * radius;
            to.X = x + MathF.Cos(nextRot) * radius;
            to.Y = y;
            to.Z = z - MathF.Sin(nextRot) * radius;
            DrawLine(viewport, from, to, CylinderDiffuse);

            // This is the top cap
            from.X = x + MathF.Cos(rot) * radius;
            from.Y = y + height;
            from.Z = z - MathF.Sin(rot) * radius;
            to.X = x + MathF.Cos(nextRot) * radius;
            to.Y = y + height;
            to.Z = z - MathF.Sin(nextRot) * radius;
            DrawLine(viewport, from, to, CylinderDiffuse);
        }
    }

    /*
        occludedOnly means that the circle will only draw
        in already occluded areas (based on depth buffer)
    */
    public void DrawFilledCircle(IGameViewport viewport,
        Vector3 center,
        float radius,
        PackedLinearColorA borderColor,
        PackedLinearColorA fillColor,
        bool occludedOnly = false)
    {
        // The positions array contains the following:
        // 0 . The center of the circle
        // 1 - (sCircleSegments + 1) . Positions on the diameter of the circle
        // sCircleSegments + 2 . The first position again to close the circle
        Span<Vector3> positions = stackalloc Vector3[CircleSegments + 3];

        // A full rotation divided by the number of segments
        var rotPerSegment = 2 * MathF.PI / CircleSegments;

        positions[0] = center;
        for (var i = 1; i < CircleSegments + 3; ++i)
        {
            var rot = (CircleSegments - i) * rotPerSegment;
            positions[i].X = center.X + MathF.Cos(rot) * radius - MathF.Sin(rot) * 0.0f;
            positions[i].Y = center.Y;
            positions[i].Z = center.Z + MathF.Cos(rot) * 0.0f + MathF.Sin(rot) * radius;
        }

        positions[^1] = positions[1];

        _circleVertexBuffer.Resource.Update<Vector3>(positions);
        _circleBinding.Resource.Bind();

        BindLineMaterial(viewport, borderColor, occludedOnly);

        // The first vertex is the center vertex, so we skip it for line drawing
        _device.Draw(PrimitiveType.LineStrip, CircleSegments + 1, 1);

        BindLineMaterial(viewport, fillColor, occludedOnly);

        _device.SetIndexBuffer(_circleIndexBuffer);

        _device.DrawIndexed(PrimitiveType.TriangleList, 0, CircleSegments * 3);
    }
}