using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.MaterialDefinitions;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.GFX.RenderMaterials;

public enum Light3dType {
    Point = 1,
    Spot = 2,
    Directional = 3
};

public class Light3d {
    public Light3dType Type;
    public LinearColor Ambient;
    public LinearColor Color;
    public Vector4 Pos;
    public Vector4 Dir;
    public float Range;
    public float Phi;
};

public class MdfRenderOverrides {
    public bool IgnoreLighting = false;
    public bool OverrideDiffuse = false;
    public PackedLinearColorA OverrideColor = default;
    public float Alpha = 1;
    public bool UseWorldMatrix = false;
    public Matrix4x4 WorldMatrix;
    public bool UiProjection = false;
};

public interface IMdfRenderMaterial : IRefCounted
{

    int GetId();

    string GetName();

    ITexture? GetPrimaryTexture();

    MdfMaterial GetSpec();

    void Bind(WorldCamera? camera, RenderingDevice g, IReadOnlyList<Light3d> lights, MdfRenderOverrides? overrides = null);

    void Bind(IGameViewport? viewport, RenderingDevice g, IReadOnlyList<Light3d> lights,
        MdfRenderOverrides? overrides = null)
    {
        Bind(viewport?.Camera, g, lights, overrides);
    }

}