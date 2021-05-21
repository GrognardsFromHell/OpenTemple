using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.MaterialDefinitions;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.GFX.RenderMaterials
{
    public enum Light3dType {
        Point = 1,
        Spot = 2,
        Directional = 3
    };

    public class Light3d {
        public Light3dType type;
        public LinearColor ambient;
        public LinearColor color;
        public Vector4 pos;
        public Vector4 dir;
        public float range;
        public float phi;
    };

    public class MdfRenderOverrides {
        public bool ignoreLighting = false;
        public bool overrideDiffuse = false;
        public PackedLinearColorA overrideColor = default;
        public float alpha = 1;
        public bool useWorldMatrix = false;
        public Matrix4x4 worldMatrix;
        public bool uiProjection = false;
    };

    public interface IMdfRenderMaterial : IRefCounted
    {

        int GetId();

        string GetName();

        ITexture GetPrimaryTexture();

        MdfMaterial GetSpec();

        void Bind(IGameViewport viewport, RenderingDevice g, IList<Light3d> lights, MdfRenderOverrides overrides = null);

    }
}