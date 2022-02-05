using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.InGameSelect;

public class PickerCircleRenderer : IDisposable
{
    private readonly RenderingDevice _device;

    private readonly ResourceRef<VertexBuffer> _vertexBuffer;
    private ResourceRef<IndexBuffer> _indexBuffer;
    private ResourceRef<BufferBinding> _bufferBinding;

    [TempleDllLocation(0x10bd2bb8)]
    private TimePoint _time;

    [TempleDllLocation(0x10bd2cdc)]
    private float _currentRotation;

    private static readonly IntgameVertex[] Vertices =
    {
        new()
        {
            pos = new Vector4(1.0f, 0.0f, -1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, -1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(-1.0f, 0.0f, -1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(-1.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(-1.0f, 0.0f, 1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(1.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 1.0f),
            diffuse = PackedLinearColorA.White
        },
        new()
        {
            pos = new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
            normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            uv = new Vector2(0.0f, 0.0f),
            diffuse = PackedLinearColorA.White
        }
    };

    private static readonly ushort[] Indices =
    {
        0, 2, 1,
        0, 3, 2,
        4, 6, 5,
        4, 7, 6,
        8, 10, 9,
        8, 11, 10,
        12, 14, 13,
        12, 15, 14,
    };

    public PickerCircleRenderer(RenderingDevice device)
    {
        _device = device;
        _indexBuffer = device.CreateIndexBuffer(Indices);
        _vertexBuffer = device.CreateVertexBuffer<IntgameVertex>(Vertices, false, debugName:"PickerCircleRenderer");

        _bufferBinding = device.CreateMdfBufferBinding().Ref();
        _bufferBinding.Resource.AddBuffer<IntgameVertex>(_vertexBuffer.Resource, 0)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
    }

    [TempleDllLocation(0x10108c50)]
    public void Render(IGameViewport viewport, Vector3 targetCenter,
        float targetRadius,
        IMdfRenderMaterial material)
    {
        var tilt = MathF.Cos(2.3561945f) * (MathF.Cos(-0.77539754f) * -1.5f);
        var elevation = MathF.Sin(-0.77539754f) * -1.5f;

        var elapsedSecs = (TimePoint.Now - _time).TotalSeconds;
        _time = TimePoint.Now;
        _currentRotation += (float) (elapsedSecs / 2.0 * Math.PI); // Full rotation every 4 seconds
        // Normalize the angle
        _currentRotation = Angles.NormalizeRadians(_currentRotation);
        var cosCurrentRot = MathF.Cos(_currentRotation);
        var sinCurrentRot = MathF.Sin(_currentRotation);

        Span<IntgameVertex> vertices = stackalloc IntgameVertex[16];
        Vertices.CopyTo(vertices);

        for (var i = 0; i < vertices.Length; i++)
        {
            ref var pos = ref vertices[i].pos;
            var originalX = pos.X;
            var originalZ = pos.Z;

            // Rotate, Scale, Translate
            pos.X = sinCurrentRot * originalZ + cosCurrentRot * originalX;
            pos.Y = targetCenter.Y + elevation;
            pos.X = targetRadius * pos.X;
            pos.Z = (cosCurrentRot * originalZ - sinCurrentRot * originalX) * targetRadius;
            pos.X = targetCenter.X + pos.X + tilt;
            pos.Z = targetCenter.Z + pos.Z + tilt;
        }

        _vertexBuffer.Resource.Update<IntgameVertex>(vertices);

        _bufferBinding.Resource.Bind();
        _device.SetIndexBuffer(_indexBuffer);

        material.Bind(viewport, _device, new List<Light3d>());

        _device.DrawIndexed(
            PrimitiveType.TriangleList,
            vertices.Length,
            Indices.Length);
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _bufferBinding.Dispose();
    }
}