using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Vfx
{
    /// <summary>
    /// Renders the chain lightning spell effect.
    /// </summary>
    public class ChainLightningRenderer : IDisposable
    {
        private readonly PerlinNoise _noise;

        /// <summary>
        /// Delay in milliseconds before an arc that is the result of chaining to another target is shown.
        /// First arc is shown immediately.
        /// </summary>
        public const int ChainDelay = 256;

        /// <summary>
        /// How long an arc should be visible in milliseconds.
        /// </summary>
        public const int Duration = 7 * ChainDelay;

        // The length of a single line segment
        private const float LineSegmentLength = 2.5f;
        // The maximum number of segments that can be rendered (length will be clamped so this is not exceeded)
        private const int MaxLineSegments = 600;

        private const float MaxJitterOffset = 80;

        private const float MainArcWidth = 24;

        // Minimum length of a fork in line-segments
        private const int MinForkSegments = 16;
        private const float ForkArcWidth = 8;
        private const int ForkCount = 2;

        // Used to store the "jitter" of lightning lines, which is the deviation from the center at a given stop
        [TempleDllLocation(0x10B397C0)]
        private readonly float[] _jitter = new float[MaxLineSegments];

        [StructLayout(LayoutKind.Sequential)]
        internal struct Vertex
        {
            public Vector4 pos;
            public Vector4 normal;
            public PackedLinearColorA diffuse;
            public Vector2 uv;
            public static readonly int Size = Marshal.SizeOf<Vertex>();
        }

        private readonly Vertex[] _vertices = new Vertex[2 * MaxLineSegments];

        private readonly RenderingDevice _device;
        private ResourceRef<IMdfRenderMaterial> _material;
        private ResourceRef<VertexBuffer> _vertexBuffer;
        private ResourceRef<IndexBuffer> _indexBuffer;
        private ResourceRef<BufferBinding> _bufferBinding;

        public ChainLightningRenderer(PerlinNoise noise) : this(Tig.RenderingDevice, Tig.MdfFactory, noise)
        {
        }

        public ChainLightningRenderer(RenderingDevice device, MdfMaterialFactory materialFactory, PerlinNoise noise)
        {
            _noise = noise;

            _device = device;
            _material = materialFactory.LoadMaterial("art/meshes/lightning.mdf", material => material.perVertexColor = true);

            _indexBuffer = CreateIndexBuffer(_device);
            _vertexBuffer =
                _device.CreateEmptyVertexBuffer(Vertex.Size * _vertices.Length, debugName: "ChainLightning");
            _bufferBinding = _device.CreateMdfBufferBinding(true).Ref();
            _bufferBinding.Resource.AddBuffer<Vertex>(_vertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
            InitVertices();
        }

        private void InitVertices()
        {
            for (var i = 0; i < _vertices.Length; i += 2)
            {
                ref var leftVertex = ref _vertices[i];
                ref var rightVertex = ref _vertices[i + 1];

                // The line's "surface" will always be facing up
                leftVertex.normal = Vector4.UnitY;
                rightVertex.normal = Vector4.UnitY;

                leftVertex.uv.X = 0;
                leftVertex.uv.Y = 0.5f;
                rightVertex.uv.X = 1.0f;
                rightVertex.uv.Y = 0.5f;
            }
        }

        private static ResourceRef<IndexBuffer> CreateIndexBuffer(RenderingDevice device)
        {
            // Generate indices for 2 triangles per line segment (essentially a quad)
            Span<ushort> indices = stackalloc ushort[6 * MaxLineSegments];
            var indexIdx = 0;
            for (var i = 0; i < 2 * MaxLineSegments; i += 2)
            {
                indices[indexIdx] = (ushort)i;
                indices[indexIdx + 1] = (ushort)(i + 1);
                indices[indexIdx + 2] = (ushort)(i + 3);
                indices[indexIdx + 3] = (ushort)i;
                indices[indexIdx + 4] = (ushort)(i + 3);
                indices[indexIdx + 5] = (ushort)(i + 2);
                indexIdx += 6;
            }

            return device.CreateIndexBuffer(indices, debugName: "ChainLightning");
        }

        public void Render(WorldCamera camera, int targetIndex, int elapsedMs, Vector3 from, Vector3 to)
        {
            var line = to - from;
            var lineLength = line.Length();

            // Normal perpendicular to the line (points to the "left") on the X,Z plane which is the floor
            var perpenNormal = new Vector3(-line.Z / lineLength, 0, line.X / lineLength);

            // Ramp the color from alpha 1->0 over the duration using the cosine function
            var colorRamp = MathF.Cos((elapsedMs % Duration) / (float) (Duration - 1) * MathF.PI / 2);

            var segments = Math.Min(MaxLineSegments, (int)MathF.Round(lineLength / LineSegmentLength));
            CalculateLineJitter(elapsedMs, segments, targetIndex);

            RenderMainArc(camera, from, to, segments, perpenNormal, colorRamp);
            RenderForks(camera, from, to, segments, perpenNormal, colorRamp);
        }

        private void RenderMainArc(WorldCamera camera, Vector3 from, Vector3 to, int segments, Vector3 perpenNormal, float colorRamp)
        {
            var alpha = (byte)(colorRamp * 255.0);
            var color = new PackedLinearColorA(alpha, alpha, alpha, alpha);
            var extrudeVec = perpenNormal * MainArcWidth / 2;

            for (var i = 0; i < segments; i++)
            {
                var vertexIdx = 2 * i;

                ref var leftVertex = ref _vertices[vertexIdx];
                ref var rightVertex = ref _vertices[vertexIdx + 1];

                leftVertex.diffuse = color;
                rightVertex.diffuse = color;

                // The position along the line from start to end at the current segment
                var pos = Vector3.Lerp(from, to, i / (float)(segments - 1));

                pos += perpenNormal * GetOffsetAt(i, segments);

                // Build two vertices for the line stop to extrude the line
                leftVertex.pos = new Vector4(pos + extrudeVec, 1);
                rightVertex.pos = new Vector4(pos - extrudeVec, 1);
            }

            RenderLineSegments(camera, segments);
        }

        private void RenderForks(WorldCamera camera, Vector3 from, Vector3 to, int segments,
            Vector3 perpenNormal, float alpha)
        {
            if (segments <= MinForkSegments)
            {
                return;
            }

            for (var i = 0; i < ForkCount; i++)
            {
                var forkSegments = MinForkSegments + (ThreadSafeRandom.Next() % (segments - MinForkSegments));
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
                    ref var leftVertex = ref _vertices[2 * j];
                    ref var rightVertex = ref _vertices[2 * j + 1];

                    var pos = Vector3.Lerp(from, to, (float)curForkSegment / segments);

                    var offset = forkBaseOffset;
                    // Amplify the jitter for the arc (but keep it 0 at the start)
                    offset += 2 * (_jitter[jitterOverlayStart + j] - _jitter[jitterOverlayStart + forkSegments - 1]);

                    pos += offset * perpenNormal;

                    leftVertex.pos = new Vector4(pos + forkExtrudeVec, 1);
                    rightVertex.pos = new Vector4(pos - forkExtrudeVec, 1);

                    var forkAlpha = (byte)(alpha * (j / (float)(forkSegments - 1)) * 255);
                    var forkColor = new PackedLinearColorA(forkAlpha, forkAlpha, forkAlpha, forkAlpha);
                    leftVertex.diffuse = forkColor;
                    rightVertex.diffuse = forkColor;
                    --curForkSegment;
                }

                RenderLineSegments(camera, forkSegments);
            }
        }

        // Offset the line segment with noise-based jitter
        // The second term is used to bring the arc back closer to its target at the end
        private float GetOffsetAt(int i, int segments)
        {
            return _jitter[i] - (_jitter[segments - 1] - _jitter[0]) * i / (segments - 1);
        }

        private void RenderLineSegments(WorldCamera camera, int segments)
        {
            _vertexBuffer.Resource.Update<Vertex>(_vertices.AsSpan().Slice(0, 2 * segments));

            _device.SetIndexBuffer(_indexBuffer);
            _material.Resource.Bind(camera, _device, Array.Empty<Light3d>());
            _bufferBinding.Resource.Bind();
            _device.DrawIndexed(PrimitiveType.TriangleList, 2 * segments, (segments - 1) * 6);
        }

        private void CalculateLineJitter(int elapsedMs, int segments, int targetIndex)
        {
            // This causes the noise to be sampled in a different grid-cell per target
            var targetNoiseOffset = 0.5f * targetIndex;
            var timePeriod = (elapsedMs % 127) / 200f;
            _jitter[0] = _noise.Noise2D(timePeriod, targetNoiseOffset) * MaxJitterOffset;

            for (var i = 1; i < segments; i++)
            {
                var y = i / (segments - 1f) * 720.0f / 400f + targetNoiseOffset;
                var absOffset = _noise.Noise2D(timePeriod, y) * MaxJitterOffset;
                _jitter[i] = absOffset - _jitter[0];
            }

            // The start of the chain-lightning should not be offset, it should be anchored at the caster
            _jitter[0] = 0f;
        }

        public void Dispose()
        {
            _material.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _bufferBinding.Dispose();
        }
    }
}