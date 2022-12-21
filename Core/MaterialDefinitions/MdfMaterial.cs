using System.Collections.Generic;

namespace OpenTemple.Core.MaterialDefinitions;

public class MdfMaterial {
    public MdfMaterial(MdfType type) {
        Type = type;
        for (var i = 0; i < 4; i++)
        {
            Samplers.Add(new MdfGeneralMaterialSampler());
        }
    }

    public readonly MdfType Type;
    public MdfBlendType BlendType = MdfBlendType.Alpha;
    public float SpecularPower = 50.0f;
    public uint Specular = 0;
    public uint Diffuse = 0xFFFFFFFF;
    public string? Glossmap; // Filename of glossmap texture
    public bool FaceCulling = true;
    public bool LinearFiltering = false;
    public bool RecalculateNormals = false;
    public bool EnableZWrite = true;
    public bool DisableZ = false;
    public bool EnableColorWrite = true;
    public bool NotLit = false;
    public bool Clamp = false;
    public bool Outline = false; // Ignored during rendering
    public bool Wireframe = false; // Ignored during rendering
    public bool PerVertexColor = false; // per-vertex color info (only used for lightning, more or less)
    public List<MdfGeneralMaterialSampler> Samplers = new();
}
