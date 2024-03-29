using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.MaterialDefinitions;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.GFX.RenderMaterials;

public class MdfRenderMaterial : GpuResource<MdfRenderMaterial>, IMdfRenderMaterial
{
    private int _id;

    private string _name;

    private ResourceRef<Material> _deviceMaterial;

    private MdfMaterial _spec;

    public MdfRenderMaterial(int id, string name, MdfMaterial mdfMaterial, Material material)
    {
        _id = id;
        _name = name;
        _deviceMaterial = material.Ref();
        _spec = mdfMaterial;
    }

    protected override void FreeResource()
    {
        _deviceMaterial.Dispose();
    }

    public int GetId() => _id;

    public string GetName()
    {
        throw new NotImplementedException();
    }

    public ITexture? GetPrimaryTexture()
    {
        var samplers = _deviceMaterial.Resource.Samplers;
        if (samplers.Count == 0)
        {
            return null;
        }

        return samplers[0].Resource.Texture.Resource;
    }

    public MdfMaterial GetSpec() => _spec;

    public void Bind(WorldCamera? camera, RenderingDevice device, IReadOnlyList<Light3d> lights,
        MdfRenderOverrides? overrides = null)
    {
        device.SetMaterial(_deviceMaterial);

        BindShader(camera, device, lights, overrides);
    }

    private void BindShader(WorldCamera? camera,
        RenderingDevice device,
        IReadOnlyList<Light3d> lights,
        MdfRenderOverrides? overrides)
    {
        // Fill out the globals for the shader
        var globals = new MdfGlobalConstants();

        Matrix4x4 viewProj;
        if (overrides is {UiProjection: true})
        {
            viewProj = device.UiProjection;
        }
        else
        {
            viewProj = camera?.GetViewProj() ?? device.UiProjection;
        }

        // Should we use a separate world matrix?
        if (overrides != null && overrides.UseWorldMatrix)
        {
            // Build a world * view * proj matrix
            var worldViewProj = overrides.WorldMatrix * viewProj;
            globals.viewProj = worldViewProj;
        }
        else
        {
            globals.viewProj = viewProj;
        }

        // Set material diffuse color for shader
        Vector4 color;
        // TODO: This is a bug, it should check overrideDiffuse
        if (overrides != null && overrides.OverrideColor != default)
        {
            color = overrides.OverrideColor.ToRGBA();
        }
        else
        {
            color = new PackedLinearColorA(_spec.Diffuse).ToRGBA();
        }

        globals.matDiffuse = color;
        if (overrides != null && overrides.Alpha != 1.0f)
        {
            globals.matDiffuse.W *= overrides.Alpha;
        }

        // Set time for UV animation in minutes as a floating point number
        var timeInSec = (float) (device.GetLastFrameStart() - device.GetDeviceCreated()).Seconds;
        globals.uvAnimTime.X = timeInSec / 60.0f;
        // Clamp to [0, 1]
        if (globals.uvAnimTime.X > 1)
        {
            globals.uvAnimTime.X -= MathF.Floor(globals.uvAnimTime.X);
        }

        // Swirl is more complicated due to cos/sin involvement
        // This means speedU is in "full rotations every 60 seconds" . RPM
        var uvRotations = globals.UvRotations;
        for (var i = 0; i < _spec.Samplers.Count; ++i)
        {
            var sampler = _spec.Samplers[i];
            if (sampler.UvType != MdfUvType.Swirl)
            {
                continue;
            }

            ref var uvRot = ref uvRotations[i];
            uvRot.X = MathF.Cos(sampler.SpeedU * globals.uvAnimTime.X * MathF.PI * 2) * 0.1f;
            uvRot.Y = MathF.Sin(sampler.SpeedV * globals.uvAnimTime.X * MathF.PI * 2) * 0.1f;
        }

        if (!_spec.NotLit)
        {
            var ignoreLighting = overrides != null && overrides.IgnoreLighting;
            BindVertexLighting(ref globals, lights, ignoreLighting);
        }

        device.SetVertexShaderConstants(0, ref globals);
    }

    private void BindVertexLighting(ref MdfGlobalConstants globals,
        IReadOnlyList<Light3d> lights,
        bool ignoreLighting)
    {
        const int maxLights = 8;

        if (lights.Count > maxLights)
        {
            // TODO THIS SUCKS
            var limitedLights = new Light3d[maxLights];
            for (int i = 0; i < limitedLights.Length; i++)
            {
                limitedLights[i] = lights[i];
            }

            lights = limitedLights;
        }

        // To make indexing in the HLSL shader more efficient, we sort the
        // lights here in the following order: directional, point lights, spot lights

        var directionalCount = 0;
        var pointCount = 0;
        var spotCount = 0;

        var lightDir = globals.LightDir;
        var lightDiffuse = globals.LightDiffuse;
        var lightSpecular = globals.LightSpecular;
        var lightAmbient = globals.LightAmbient;
        var lightPos = globals.LightPos;
        var lightRange = globals.LightRange;
        var lightAttenuation = globals.LightAttenuation;
        var lightSpot = globals.LightSpot;

        // In order to not have to use a different shader, we instead
        // opt for using a diffuse light with ambient 1,1,1,1
        if (ignoreLighting)
        {
            directionalCount = 1;
            lightDir[0] = Vector4.Zero;
            lightDiffuse[0] = Vector4.Zero;
            lightSpecular[0] = Vector4.Zero;

            lightAmbient[0] = new Vector4(1, 1, 1, 0);
        }
        else
        {
            // Count the number of lights of each type
            foreach (var light in lights)
            {
                switch (light.Type)
                {
                    case Light3dType.Directional:
                        ++directionalCount;
                        break;
                    case Light3dType.Point:
                        ++pointCount;
                        break;
                    case Light3dType.Spot:
                        ++spotCount;
                        break;
                    default:
                        continue;
                }
            }

            var pointFirstIdx = directionalCount;
            var spotFirstIdx = pointFirstIdx + pointCount;

            directionalCount = 0;
            pointCount = 0;
            spotCount = 0;

            foreach (var light in lights)
            {
                int lightIdx;
                switch (light.Type)
                {
                    case Light3dType.Directional:
                        lightIdx = directionalCount++;
                        break;
                    case Light3dType.Point:
                        lightIdx = pointFirstIdx + pointCount++;
                        break;
                    case Light3dType.Spot:
                        lightIdx = spotFirstIdx + spotCount++;
                        break;
                    default:
                        continue;
                }

                lightPos[lightIdx].X = light.Pos.X;
                lightPos[lightIdx].Y = light.Pos.Y;
                lightPos[lightIdx].Z = light.Pos.Z;

                lightDir[lightIdx].X = light.Dir.X;
                lightDir[lightIdx].Y = light.Dir.Y;
                lightDir[lightIdx].Z = light.Dir.Z;

                lightAmbient[lightIdx].X = light.Ambient.R;
                lightAmbient[lightIdx].Y = light.Ambient.G;
                lightAmbient[lightIdx].Z = light.Ambient.B;
                lightAmbient[lightIdx].W = 0;

                lightDiffuse[lightIdx].X = light.Color.R;
                lightDiffuse[lightIdx].Y = light.Color.G;
                lightDiffuse[lightIdx].Z = light.Color.B;
                lightDiffuse[lightIdx].W = 0;

                lightSpecular[lightIdx].X = light.Color.R;
                lightSpecular[lightIdx].Y = light.Color.G;
                lightSpecular[lightIdx].Z = light.Color.B;
                lightSpecular[lightIdx].W = 0;

                lightRange[lightIdx].X = light.Range;

                lightAttenuation[lightIdx].X = 0;
                lightAttenuation[lightIdx].Y = 0;
                lightAttenuation[lightIdx].Z = 4.0f / (light.Range * light.Range);
                var phiRad = Angles.ToRadians(light.Phi);
                lightSpot[lightIdx].X = MathF.Cos(phiRad * 0.6f * 0.5f);
                lightSpot[lightIdx].Y = MathF.Cos(phiRad * 0.5f);
                lightSpot[lightIdx].Z = 1;
            }
        }

        globals.bSpecular = new RawInt4((_spec.Specular != 0) ? 1 : 0, 0, 0, 0);
        globals.fMaterialPower = new Vector4(_spec.SpecularPower, 0, 0, 0);

        // Set the specular color
        globals.matSpecular = new PackedLinearColorA(_spec.Specular).ToRGBA();

        globals.lightCount.X = directionalCount;
        globals.lightCount.Y = globals.lightCount.X + pointCount;
        globals.lightCount.Z = globals.lightCount.Y + spotCount;
    }
}


/*
    Please note that the restrictive rules on constant buffer packing make it much easier to just
    keep using Vector4's although they are oversized. We might want to optimize the packing here by
    hand later.
*/
[StructLayout(LayoutKind.Sequential)]
internal struct MdfGlobalConstants
{
    public Matrix4x4 viewProj;
    public Vector4 matDiffuse;
    public Vector4 uvAnimTime;
    public Vector4 uvRotation1; // One per texture stage
    public Vector4 uvRotation2;
    public Vector4 uvRotation3;
    public Vector4 uvRotation4;

    public Span<Vector4> UvRotations => MemoryMarshal.CreateSpan(ref uvRotation1, 4);

    // Lighting related
    public Vector4 lightPos1;
    public Vector4 lightPos2;
    public Vector4 lightPos3;
    public Vector4 lightPos4;
    public Vector4 lightPos5;
    public Vector4 lightPos6;
    public Vector4 lightPos7;
    public Vector4 lightPos8;
    public Span<Vector4> LightPos => MemoryMarshal.CreateSpan(ref lightPos1, 8);

    public Vector4 lightDir1;
    public Vector4 lightDir2;
    public Vector4 lightDir3;
    public Vector4 lightDir4;
    public Vector4 lightDir5;
    public Vector4 lightDir6;
    public Vector4 lightDir7;
    public Vector4 lightDir8;
    public Span<Vector4> LightDir => MemoryMarshal.CreateSpan(ref lightDir1, 8);

    public Vector4 lightAmbient1;
    public Vector4 lightAmbient2;
    public Vector4 lightAmbient3;
    public Vector4 lightAmbient4;
    public Vector4 lightAmbient5;
    public Vector4 lightAmbient6;
    public Vector4 lightAmbient7;
    public Vector4 lightAmbient8;
    public Span<Vector4> LightAmbient => MemoryMarshal.CreateSpan(ref lightAmbient1, 8);

    public Vector4 lightDiffuse1;
    public Vector4 lightDiffuse2;
    public Vector4 lightDiffuse3;
    public Vector4 lightDiffuse4;
    public Vector4 lightDiffuse5;
    public Vector4 lightDiffuse6;
    public Vector4 lightDiffuse7;
    public Vector4 lightDiffuse8;
    public Span<Vector4> LightDiffuse => MemoryMarshal.CreateSpan(ref lightDiffuse1, 8);

    public Vector4 lightSpecular1;
    public Vector4 lightSpecular2;
    public Vector4 lightSpecular3;
    public Vector4 lightSpecular4;
    public Vector4 lightSpecular5;
    public Vector4 lightSpecular6;
    public Vector4 lightSpecular7;
    public Vector4 lightSpecular8;
    public Span<Vector4> LightSpecular => MemoryMarshal.CreateSpan(ref lightSpecular1, 8);

    public Vector4 lightRange1;
    public Vector4 lightRange2;
    public Vector4 lightRange3;
    public Vector4 lightRange4;
    public Vector4 lightRange5;
    public Vector4 lightRange6;
    public Vector4 lightRange7;
    public Vector4 lightRange8;
    public Span<Vector4> LightRange => MemoryMarshal.CreateSpan(ref lightRange1, 8);

    public Vector4 lightAttenuation1; //1, D, D^2;
    public Vector4 lightAttenuation2;
    public Vector4 lightAttenuation3;
    public Vector4 lightAttenuation4;
    public Vector4 lightAttenuation5;
    public Vector4 lightAttenuation6;
    public Vector4 lightAttenuation7;
    public Vector4 lightAttenuation8;
    public Span<Vector4> LightAttenuation => MemoryMarshal.CreateSpan(ref lightAttenuation1, 8);

    public Vector4 lightSpot1; //cos(theta/2), cos(phi/2), falloff
    public Vector4 lightSpot2;
    public Vector4 lightSpot3;
    public Vector4 lightSpot4;
    public Vector4 lightSpot5;
    public Vector4 lightSpot6;
    public Vector4 lightSpot7;
    public Vector4 lightSpot8;
    public Span<Vector4> LightSpot => MemoryMarshal.CreateSpan(ref lightSpot1, 8);

    public RawInt4 bSpecular;
    public Vector4 fMaterialPower;
    public Vector4 matSpecular;
    public RawInt4 lightCount; // Directional, point, spot
};

internal enum MdfShaderRegisters
{
    // Projection Matrix (in ToEE's case this is viewProj)
    MDF_REG_VIEWPROJ = 0,
    MDF_REG_MATDIFFUSE = 4,
    MDF_REG_UVANIMTIME = 5,
    MDF_REG_UVROTATION = 6,

    // Lighting related registers
    MDF_REG_LIGHT_POS = 10,
    MDF_REG_LIGHT_DIR = 18,
    MDF_REG_LIGHT_AMBIENT = 26,
    MDF_REG_LIGHT_DIFFUSE = 34,
    MDF_REG_LIGHT_SPECULAR = 42,
    MDF_REG_LIGHT_RANGE = 50,
    MDF_REG_LIGHT_ATTENUATION = 58,
    MDF_REG_LIGHT_SPOT = 66,
    MDF_REG_LIGHT_SPECULARENABLE = 74,
    MDF_REG_LIGHT_SPECULARPOWER = 75,
    MDF_REG_LIGHT_MAT_SPECULAR = 76,
    MDF_REG_LIGHT_COUNT = 77
}