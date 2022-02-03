using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.InGameSelect;

/// <summary>
/// Renders a pointer on the edge of the AOE area, pointing from the location of the caster.
/// </summary>
public class SpellPointerRenderer : IDisposable
{
    private static readonly ushort[] Indices =
    {
        0, 1, 2, 1, 3, 2
    };

    private readonly RenderingDevice _device;

    private readonly ResourceRef<VertexBuffer> _vertexBuffer;
    private ResourceRef<BufferBinding> _bufferBinding;
    private ResourceRef<IndexBuffer> _indexBuffer;
    private ResourceRef<IMdfRenderMaterial> _material;

    public SpellPointerRenderer(RenderingDevice device)
    {
        _device = device;
        _indexBuffer = device.CreateIndexBuffer(Indices);
        _vertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 4, debugName:"SpellPointer");

        _bufferBinding = device.CreateMdfBufferBinding().Ref();
        _bufferBinding.Resource.AddBuffer<IntgameVertex>(_vertexBuffer.Resource, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

        _material = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/Spell_Effect-Pointer.mdf");
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _bufferBinding.Dispose();
    }

    [TempleDllLocation(0x101068a0)]
    public void Render(IGameViewport viewport, Vector3 aoeCenter, Vector3 casterPos, float aoeRadius)
    {
        Span<IntgameVertex> vertices = stackalloc IntgameVertex[4];

        var elevation = MathF.Sin(-0.77539754f) * -1.5f;
        var v3 = aoeRadius * 0.56560004f;
        float v14 = v3;
        if (v3 <= 135.744)
        {
            if (v14 < 11.312)
            {
                v14 = 11.312f;
            }
        }
        else
        {
            v14 = 135.744f;
        }


        var dirVector = Vector3.Normalize(casterPos - aoeCenter);

        float v10 = aoeRadius - v14;
        vertices[0] = new IntgameVertex
        {
            pos = new Vector4(
                aoeCenter.X + v10 * dirVector.X,
                elevation,
                aoeCenter.Z + v10 * dirVector.Z,
                1
            ),
            normal = Vector4.UnitY,
            uv = new Vector2(0, 1),
            diffuse = PackedLinearColorA.White
        };
        vertices[1] = new IntgameVertex
        {
            pos = new Vector4(
                aoeCenter.X + aoeRadius * dirVector.X + v14 * dirVector.Z,
                elevation,
                aoeCenter.Z + aoeRadius * dirVector.Z - v14 * dirVector.X,
                1
            ),
            normal = Vector4.UnitY,
            uv = new Vector2(0, 0),
            diffuse = PackedLinearColorA.White
        };
        vertices[2] = new IntgameVertex
        {
            pos = new Vector4(
                aoeCenter.X + aoeRadius * dirVector.X - v14 * dirVector.Z,
                elevation,
                aoeCenter.Z + v14 * dirVector.X + aoeRadius * dirVector.Z,
                1
            ),
            normal = Vector4.UnitY,
            uv = new Vector2(1, 1),
            diffuse = PackedLinearColorA.White
        };
        vertices[3] = new IntgameVertex
        {
            pos = new Vector4(
                aoeCenter.X + (v14 + aoeRadius) * dirVector.X,
                elevation,
                aoeCenter.Z + (v14 + aoeRadius) * dirVector.Z,
                1
            ),
            normal = Vector4.UnitY,
            uv = new Vector2(1, 0),
            diffuse = PackedLinearColorA.White
        };

        _vertexBuffer.Resource.Update<IntgameVertex>(vertices);

        _bufferBinding.Resource.Bind();
        _device.SetIndexBuffer(_indexBuffer);

        _material.Resource.Bind(viewport, _device, new List<Light3d>());

        _device.DrawIndexed(
            PrimitiveType.TriangleList,
            vertices.Length,
            Indices.Length);
    }
}