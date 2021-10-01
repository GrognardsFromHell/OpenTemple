using System;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Vfx
{
    /// <summary>
    /// Renders the call lightning spell effect.
    /// </summary>
    public class CallLightningRenderer : LineSegmentRenderer
    {
        // The length of a single line segment
        private const float LineSegmentLength = 2.5f;

        // The height of a call lightning arc
        private const float ArcHeight = 720f;
        private const float ArcWidth = 24f;

        // The number of segments to render
        private static readonly int LineSegments = (int)(ArcHeight / LineSegmentLength);

        public CallLightningRenderer(PerlinNoise noise) : this(Tig.RenderingDevice, Tig.MdfFactory, noise)
        {
        }

        public CallLightningRenderer(RenderingDevice device, MdfMaterialFactory materialFactory, PerlinNoise noise)
            : base(device, materialFactory, noise, LineSegments, "CallLightning")
        {
            // This is the normal that vanilla used. Since lightning is not lit, the normal doesn't really matter
            InitVertices(new Vector3(MathF.Sqrt(0.5f), 0, MathF.Sqrt(0.5f)));
        }

        public void Render(WorldCamera camera, int elapsedMs, Vector3 target)
        {
            // Increasing the line segment length here will essentially increase the frequency,
            // which leads to a better look compared to vanilla. We can't replicate the original 1:1
            // because we actually draw the lightning in screen space
            CalculateLineJitter(elapsedMs, LineSegments, LineSegmentLength * 1.5f, 0);

            // Call lightning ramps alpha from 1->0 in 127 millisecond intervals using cosine to create quick
            // "flashes" of lightning.
            // You can visualize the ramp in Wolfram Alpha:
            // https://www.wolframalpha.com/input/?i=plot+cos%28%28x+mod+127%29+%2F+254+*+PI%29+from+x%3D0+to+768
            var alphaRamp = MathF.Cos(elapsedMs % 127 / 127f * 0.5f * MathF.PI);
            var alpha = (byte)(alphaRamp * 255);
            var color = new PackedLinearColorA(alpha, alpha, alpha, alpha);

            // These are normals for screen-space pixels
            var (right, up) = camera.GetBillboardNormals();

            // Vector used to extrude the line to it's intended screen-space width
            var extrudeVec = ArcWidth / 2 * right;

            for (var i = 0; i < LineSegments; i++)
            {
                ref var leftVertex = ref Vertices[i * 2];
                ref var rightVertex = ref Vertices[i * 2 + 1];

                var y = (float)i / (LineSegments - 1) * ArcHeight;

                var pos = target + y * up - Jitter[i] * right;
                leftVertex.pos = pos - extrudeVec;
                rightVertex.pos = pos + extrudeVec;

                leftVertex.diffuse = color;
                rightVertex.diffuse = color;
            }

            RenderLineSegments(camera, LineSegments);

            RenderStartingCap(camera, target, up, right, alphaRamp);

            for (var i = 0; i < 4; i++)
            {
                // Fake a bit here by drawing from top to bottom (reverse of the main arc)
                // The perpendicular normal should also point to the "left" hence the inversion here
                RenderFork(camera, target + up * ArcHeight, target, LineSegments, -right, alphaRamp, 2, 128);
            }
        }

        protected override float GetJitterTimePeriod(int elapsedMs)
        {
            // This is different from Vanilla, but causes subsequent flashes to not repeat after 127ms
            return elapsedMs / 200.0f;
        }

        protected override float GetOffsetAt(int i, int segments)
        {
            return Jitter[segments - i - 1];
        }
    }
}