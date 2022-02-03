using System;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Vfx;

/// <summary>
/// Renders the chain lightning spell effect.
/// </summary>
public class ChainLightningRenderer : LineSegmentRenderer
{

    /// <summary>
    /// Delay in milliseconds before an arc that is the result of chaining to another target is shown.
    /// First arc is shown immediately.
    /// </summary>
    public const int ChainDelay = 256;

    /// <summary>
    /// How long an arc should be visible in milliseconds.
    /// </summary>
    public const int Duration = 7 * ChainDelay;

    // Minimum length of a fork in line-segments
    private const int MinForkSegments = 16;
    private const int ForkCount = 2;

    // The length of a single line segment
    private const float LineSegmentLength = 2.5f;
    // The maximum number of segments that can be rendered (length will be clamped so this is not exceeded)
    private const int MaxLineSegments = 600;

    public ChainLightningRenderer(PerlinNoise noise) : this(Tig.RenderingDevice, Tig.MdfFactory, noise)
    {
    }

    public ChainLightningRenderer(RenderingDevice device, MdfMaterialFactory materialFactory, PerlinNoise noise)
        : base(device, materialFactory, noise, MaxLineSegments, "LightningBolt")
    {
        // The line's "surface" will always be facing up
        InitVertices(Vector3.UnitY);
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

        // This causes the noise to be sampled in a different grid-cell per target
        var targetNoiseOffset = 0.5f * targetIndex;
        CalculateLineJitter(elapsedMs, segments, LineSegmentLength, targetNoiseOffset);

        RenderMainArc(camera, from, to, segments, perpenNormal, colorRamp);

        for (var i = 0; i < ForkCount; i++)
        {
            RenderFork(camera, from, to, segments, perpenNormal, colorRamp, MinForkSegments, segments);
        }
    }

}