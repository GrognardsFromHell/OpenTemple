using System.Collections.Generic;

namespace OpenTemple.Core.MaterialDefinitions
{
    public class MdfMaterial {
        public MdfMaterial(MdfType type) {
            this.type = type;
            for (int i = 0; i < 4; i++)
            {
                samplers.Add(new MdfGeneralMaterialSampler());
            }
        }

        public readonly MdfType type;
        public MdfBlendType blendType = MdfBlendType.Alpha;
        public float specularPower = 50.0f;
        public uint specular = 0;
        public uint diffuse = 0xFFFFFFFF;
        public string glossmap; // Filename of glossmap texture
        public bool faceCulling = true;
        public bool linearFiltering = false;
        public bool recalculateNormals = false;
        public bool enableZWrite = true;
        public bool disableZ = false;
        public bool enableColorWrite = true;
        public bool notLit = false;
        public bool clamp = false;
        public bool outline = false; // Ignored during rendering
        public bool wireframe = false; // Ignored during rendering
        public bool perVertexColor = false; // per-vertex color info (only used for lightning, more or less)
        public List<MdfGeneralMaterialSampler> samplers = new List<MdfGeneralMaterialSampler>();
    };
}