using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX.Materials;

namespace OpenTemple.Core.GFX;

/// <summary>
/// A simple utility to render a full-screen quad. Used mostly for post-processing render-targets. 
/// </summary>
public class FullQuadRenderer : IDisposable
{
    private readonly RenderingDevice _device;
    private ResourceRef<Material> _simpleTextured;
    private readonly SimpleMesh _mesh;

    public FullQuadRenderer(RenderingDevice device)
    {
        _device = device;
        _simpleTextured = CreateMaterial(device).Ref();

        _mesh = BuildMesh();
    }

    /// <summary>
    /// Builds a mesh that fills the entire screen-space, assuming an identity project matrix.
    /// </summary>
    private SimpleMesh BuildMesh()
    {
        var builder = SimpleMeshBuilder<Vertex>.Quads(1);
        // Clockwise, assuming LH coordinate system and identity projection matrix
        ref var ul = ref builder.Vertex();
        ul.pos = new Vector4(-1, 1, 0, 1);
        ul.diffuse = PackedLinearColorA.White;
        ul.uv = new Vector2(0, 0);
        ref var ur = ref builder.Vertex();
        ur.pos = new Vector4(1, 1, 0, 1);
        ur.diffuse = PackedLinearColorA.White;
        ur.uv = new Vector2(1, 0);
        ref var br = ref builder.Vertex();
        br.pos = new Vector4(1, -1, 0, 1);
        br.diffuse = PackedLinearColorA.White;
        br.uv = new Vector2(1, 1);
        ref var bl = ref builder.Vertex();
        bl.pos = new Vector4(-1, -1, 0, 1);
        bl.diffuse = PackedLinearColorA.White;
        bl.uv = new Vector2(0, 1);

        return builder.Build(_device, _simpleTextured.Resource.VertexShader);
    }

    private static Material CreateMaterial(RenderingDevice device)
    {
        var depthStencilSpec = new DepthStencilSpec();
        depthStencilSpec.depthEnable = false;

        using var vertexShader = device.GetShaders().LoadVertexShader("gui_vs");
        using var pixelShader = device.GetShaders().LoadPixelShader("textured_simple_ps");

        return device.CreateMaterial(null, depthStencilSpec, null,
            null, vertexShader.Resource, pixelShader.Resource);
    }

    public void Dispose()
    {
        _simpleTextured.Dispose();
    }

    public void Render(ITexture texture)
    {
        _device.SetMaterial(_simpleTextured);
        _device.SetTexture(0, texture);
        var projection = Matrix4x4.Identity;
        _device.SetVertexShaderConstants(0, ref projection);
        _mesh.Render(_device);
    }

    [StructLayout(LayoutKind.Explicit, Size = 36, Pack = 1)]
    public struct Vertex : IVertexFormat
    {
        [FieldOffset(0)]
        public Vector4 pos;

        [FieldOffset(16)]
        public PackedLinearColorA diffuse;

        [FieldOffset(20)]
        public Vector2 uv;

        public static void Describe(ref BufferBindingBuilder builder)
        {
            Debug.Assert(Size == 44);
            builder.AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        public static readonly int Size = Marshal.SizeOf<Vertex2d>();
    }
}