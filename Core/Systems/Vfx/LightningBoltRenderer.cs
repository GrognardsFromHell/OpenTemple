using System;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Vfx;

/// <summary>
/// Renders the lightning bolt spell effect.
/// </summary>
public class LightningBoltRenderer : LineSegmentRenderer
{
    // The length of a single line segment
    private const float LineSegmentLength = 2.5f;

    // The maximum number of segments that can be rendered, Vanilla used a target that was always 1440 units away.
    // Hence this is always the number of segments.
    private const int MaxLineSegments = 576;

    public LightningBoltRenderer(PerlinNoise noise) : this(Tig.RenderingDevice, Tig.MdfFactory, noise)
    {
    }

    public LightningBoltRenderer(RenderingDevice device, MdfMaterialFactory materialFactory, PerlinNoise noise)
        : base(device, materialFactory, noise, MaxLineSegments, "LightningBolt")
    {
        InitVertices(Vector3.UnitY);
    }

    public void Render(WorldCamera camera, int elapsedMs, Vector3 from, Vector3 normal)
    {
        if (MathF.Abs(normal.Y) > 0.001f)
        {
            throw new ArgumentException("Assumed the lightning bolt to be on the X,Z plane, but got normal: " +
                                        normal);
        }

        var segments = MaxLineSegments;

        CalculateLineJitter(elapsedMs, segments, LineSegmentLength, 0);

        // Normal perpendicular to the line segment's direction (counter-clockwise, left)
        var perpenNormal = new Vector3(-normal.Z, 0, normal.X);

        // Ramp alpha 1->0 over the duration using the cosine function
        var timeFade = MathF.Cos((elapsedMs % ChainLightningRenderer.Duration) /
            (float)(ChainLightningRenderer.Duration - 1) * MathF.PI / 2);

        var to = from + normal * segments * LineSegmentLength;

        RenderMainArc(camera, from, to, segments, perpenNormal, timeFade);

        var forks = segments / 64;
        for (var i = 0; i < forks; i++)
        {
            RenderFork(camera, from, to, segments, perpenNormal, timeFade, 16, 196);
        }

        RenderStartingCap(camera, from, normal, perpenNormal, timeFade);
    }

    protected override float GetMainArcDistanceFade(int i, int segments)
    {
        return MathF.Cos((float)i / (segments - 1) * MathF.PI / 2);
    }
}