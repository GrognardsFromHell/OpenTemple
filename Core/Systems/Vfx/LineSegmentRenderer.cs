using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Vfx;

public abstract class LineSegmentRenderer : IDisposable
{

    private const float MaxJitterOffset = 80;
    private const float MainArcWidth = 24;
    private const float ForkArcWidth = 8;
    // How many vertices should be used to tesellate a "round" cap at the start of the lightning bolt,
    // in addition to the center vertex.
    private const int CapTriangleCount = 14;

    private readonly PerlinNoise _noise;
    private readonly RenderingDevice _device;
    private ResourceRef<IMdfRenderMaterial> _material;
    private ResourceRef<VertexBuffer> _vertexBuffer;
    private ResourceRef<IndexBuffer> _indexBuffer;
    private ResourceRef<IndexBuffer> _capIndexBuffer;
    private ResourceRef<BufferBinding> _bufferBinding;

    protected readonly Vertex[] Vertices;
    private readonly Vertex[] _capVertices = new Vertex[CapTriangleCount + 2];

    // Used to store the "jitter" of lightning lines, which is the deviation from the center at a given stop
    [TempleDllLocation(0x10B397C0)]
    protected readonly float[] Jitter;

    protected LineSegmentRenderer(RenderingDevice device, MdfMaterialFactory materialFactory, PerlinNoise noise,
        int maxLineSegments, string debugName)
    {
        _noise = noise;
        Vertices = new Vertex[maxLineSegments * 2];
        Jitter = new float[maxLineSegments];

        _device = device;
        _material = materialFactory.LoadMaterial("art/meshes/lightning.mdf",
            material => material.perVertexColor = true);

        _indexBuffer = CreateIndexBuffer(_device, maxLineSegments, debugName);
        _capIndexBuffer = CreateIndexBuffer2(_device, debugName);
        _vertexBuffer =
            _device.CreateEmptyVertexBuffer(Vertex.Size * Vertices.Length, debugName: debugName);
        _bufferBinding = _device.CreateMdfBufferBinding(true).Ref();
        _bufferBinding.Resource.AddBuffer<Vertex>(_vertexBuffer.Resource, 0)
            .AddElement(VertexElementType.Float3, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float3, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
    }

    protected void InitVertices(Vector3 normal)
    {
        for (var i = 0; i < Vertices.Length; i += 2)
        {
            ref var leftVertex = ref Vertices[i];
            ref var rightVertex = ref Vertices[i + 1];

            leftVertex.normal = normal;
            rightVertex.normal = normal;

            leftVertex.uv.X = 0;
            leftVertex.uv.Y = 0.5f;
            rightVertex.uv.X = 1.0f;
            rightVertex.uv.Y = 0.5f;
        }
    }

    private ResourceRef<IndexBuffer> CreateIndexBuffer2(RenderingDevice device, string debugName)
    {
        // Generate a "fan" that consists of triangles connecting the origin point with two of the cap vertices each
        Span<ushort> indices = stackalloc ushort[3 * CapTriangleCount];
        var indexIdx = 0;
        for (var i = 0; i < CapTriangleCount; i++)
        {
            indices[indexIdx] = 0;
            indices[indexIdx + 1] = (ushort)(i + 2);
            indices[indexIdx + 2] = (ushort)(i + 1);
            indexIdx += 3;
        }

        return device.CreateIndexBuffer(indices, debugName: debugName + "Cap");
    }

    private static ResourceRef<IndexBuffer> CreateIndexBuffer(RenderingDevice device, int maxLineSegments, string debugName)
    {
        // Generate indices for 2 triangles per line segment (essentially a quad)
        Span<ushort> indices = stackalloc ushort[6 * maxLineSegments];
        var indexIdx = 0;
        for (var i = 0; i < 2 * maxLineSegments; i += 2)
        {
            indices[indexIdx] = (ushort)i;
            indices[indexIdx + 1] = (ushort)(i + 1);
            indices[indexIdx + 2] = (ushort)(i + 3);
            indices[indexIdx + 3] = (ushort)i;
            indices[indexIdx + 4] = (ushort)(i + 3);
            indices[indexIdx + 5] = (ushort)(i + 2);
            indexIdx += 6;
        }

        return device.CreateIndexBuffer(indices, debugName: debugName);
    }

    protected void CalculateLineJitter(int elapsedMs, int segments, float lengthPerSegment, float noiseY)
    {
        var timePeriod = GetJitterTimePeriod(elapsedMs);
        Jitter[0] = _noise.Noise2D(timePeriod, noiseY) * MaxJitterOffset;

        for (var i = 1; i < segments; i++)
        {
            var y = i / (720f / lengthPerSegment - 1f) * 720.0f / 400f + noiseY;
            var absOffset = _noise.Noise2D(timePeriod, y) * MaxJitterOffset;
            Jitter[i] = absOffset - Jitter[0];
        }

        // The start of the chain-lightning should not be offset, it should be anchored at the caster
        Jitter[0] = 0f;
    }

    protected virtual float GetJitterTimePeriod(int elapsedMs)
    {
        return (elapsedMs % 127) / 200f;
    }

    // Offset the line segment with noise-based jitter
    // The second term is used to bring the arc back closer to its target at the end
    protected virtual float GetOffsetAt(int i, int segments)
    {
        return Jitter[i] - (Jitter[segments - 1] - Jitter[0]) * i / (segments - 1);
    }

    protected void RenderMainArc(WorldCamera camera, Vector3 from, Vector3 to, int segments, Vector3 perpenNormal, float timeFade)
    {
        var extrudeVec = perpenNormal * MainArcWidth / 2;

        for (var i = 0; i < segments; i++)
        {
            ref var leftVertex = ref Vertices[2 * i];
            ref var rightVertex = ref Vertices[2 * i + 1];

            var alpha = (byte)(timeFade * GetMainArcDistanceFade(i, segments) * 255);
            var color = new PackedLinearColorA(alpha, alpha, alpha, alpha);
            leftVertex.diffuse = color;
            rightVertex.diffuse = color;

            // The position along the line from start to end at the current segment
            var pos = Vector3.Lerp(from, to, i / (float)(segments - 1));

            pos += GetOffsetAt(i, segments) * perpenNormal;

            // Build two vertices for the line stop to extrude the line
            leftVertex.pos = pos + extrudeVec;
            rightVertex.pos = pos - extrudeVec;
        }

        RenderLineSegments(camera, segments);
    }

    protected void RenderFork(WorldCamera camera, Vector3 from, Vector3 to, int segments,
        Vector3 perpenNormal, float timeFade, int minLength, int maxLength)
    {
        Debug.Assert(minLength < maxLength);
        if (segments < minLength)
        {
            return;
        }

        var forkSegments = minLength + ThreadSafeRandom.Next() % (maxLength - minLength);

        // "Reuse" a different segment of the arc's jitter to make the offsets for the fork look different
        var jitterOverlayStart = ThreadSafeRandom.Next() % (segments - forkSegments);
        var forkStartSegment = ThreadSafeRandom.Next() % (segments - forkSegments);

        // The offset at the start of the fork should equal the main arc's offset so that the fork seems
        // to "split" from it at that point.
        var forkBaseOffset = GetOffsetAt(forkStartSegment + 1, segments);

        // Forks build backwards, interestingly
        var curForkSegment = forkStartSegment + forkSegments;

        // Forks have a width of 8
        var forkExtrudeVec = perpenNormal * ForkArcWidth / 2;
        for (var j = 0; j < forkSegments; j++)
        {
            ref var leftVertex = ref Vertices[2 * j];
            ref var rightVertex = ref Vertices[2 * j + 1];

            var pos = Vector3.Lerp(from, to, (float)curForkSegment / (segments - 1));

            var offset = forkBaseOffset;
            // Amplify the jitter for the arc (but keep it 0 at the start)
            offset += 2 * (Jitter[jitterOverlayStart + j] - Jitter[jitterOverlayStart + forkSegments - 1]);

            pos += offset * perpenNormal;

            leftVertex.pos = pos + forkExtrudeVec;
            rightVertex.pos = pos - forkExtrudeVec;

            var distanceFade = j / (float)(forkSegments - 1);
            distanceFade *= GetMainArcDistanceFade(curForkSegment, segments);

            var forkAlpha = (byte)(timeFade * distanceFade * 255);
            var forkColor = new PackedLinearColorA(forkAlpha, forkAlpha, forkAlpha, forkAlpha);
            leftVertex.diffuse = forkColor;
            rightVertex.diffuse = forkColor;
            --curForkSegment;
        }

        RenderLineSegments(camera, forkSegments);
    }

    protected virtual float GetMainArcDistanceFade(int i, int segments)
    {
        return 1f;
    }

    protected void RenderLineSegments(WorldCamera camera, int segments)
    {
        _vertexBuffer.Resource.Update<Vertex>(Vertices.AsSpan().Slice(0, 2 * segments));

        _device.SetIndexBuffer(_indexBuffer);
        _material.Resource.Bind(camera, _device, Array.Empty<Light3d>());
        _bufferBinding.Resource.Bind();
        _device.DrawIndexed(PrimitiveType.TriangleList, 2 * segments, (segments - 1) * 6);
    }

    /// <summary>
    /// Renders a "starting cap" on the starting point of the lightning bolt so it doesn't start with a jagged
    /// edge.
    /// </summary>
    protected void RenderStartingCap(WorldCamera camera, Vector3 from, Vector3 normal, Vector3 perpendicularNormal, float timeFade)
    {
        _capVertices[0].pos = from;
        _capVertices[0].uv.X = 0.5f;
        _capVertices[0].uv.Y = 0.5f;
        var alpha = (byte)(timeFade * 255);
        _capVertices[0].diffuse = new PackedLinearColorA(alpha, alpha, alpha, alpha);

        var rotationAxis = Vector3.Normalize(Vector3.Cross(normal, perpendicularNormal));

        perpendicularNormal *= MainArcWidth / 2;

        for (var i = 1; i < _capVertices.Length; i++)
        {
            ref var vertex = ref _capVertices[i];

            var rotation = (i - 1) / (float) (_capVertices.Length - 2) * MathF.PI;
            var q = Quaternion.CreateFromAxisAngle(rotationAxis, rotation);
            var n = Vector3.Transform(perpendicularNormal, q);
            vertex.pos = from + n;

            vertex.diffuse = _capVertices[0].diffuse;
            vertex.uv.X = 1f;
            vertex.uv.Y = 0.5f;
        }

        _vertexBuffer.Resource.Update<Vertex>(_capVertices);
        _device.SetIndexBuffer(_capIndexBuffer);
        _material.Resource.Bind(camera, _device, Array.Empty<Light3d>());
        _bufferBinding.Resource.Bind();
        _device.DrawIndexed(PrimitiveType.TriangleList, CapTriangleCount + 2, CapTriangleCount * 3);
    }

    [StructLayout(LayoutKind.Sequential)]
    protected struct Vertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public PackedLinearColorA diffuse;
        public Vector2 uv;
        public static readonly int Size = Marshal.SizeOf<Vertex>();
    }

    public void Dispose()
    {
        _material.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _capIndexBuffer.Dispose();
        _bufferBinding.Dispose();
    }
}