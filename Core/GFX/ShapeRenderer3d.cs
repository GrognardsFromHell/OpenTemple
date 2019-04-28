using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GFX.RenderMaterials;

namespace SpicyTemple.Core.GFX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShapeVertex3d
    {
        public Vector4 pos;
        public Vector4 normal;
        public Vector2 uv;
    };

    public class ShapeRenderer3d
    {

        public void DrawQuad(ReadOnlySpan<ShapeVertex3d> corners,
            PackedLinearColorA color,
            ITexture texture)
        {
            throw new NotImplementedException(); // TODO
        }

        public void DrawQuad(ReadOnlySpan<ShapeVertex3d> corners,
        IMdfRenderMaterial material,
        PackedLinearColorA color){
            throw new NotImplementedException(); // TODO
        }

        public void DrawDisc(Vector3 center, 
        float rotation, 
        float radius,
            IMdfRenderMaterial material){
            throw new NotImplementedException(); // TODO
        }

        public void DrawLine(Vector3 from,
        Vector3 to,
        PackedLinearColorA color){
            throw new NotImplementedException(); // TODO
        }

        public void DrawLineWithoutDepth(Vector3 from,
        Vector3 to,
        PackedLinearColorA color){
            throw new NotImplementedException(); // TODO
        }

        public void DrawCylinder(Vector3 pos, float radius, float height){
            throw new NotImplementedException(); // TODO
        }

        /*
            occludedOnly means that the circle will only draw
            in already occluded areas (based on depth buffer)
        */
        public void DrawFilledCircle(Vector3 center,
        float radius,
            PackedLinearColorA borderColor,
        PackedLinearColorA fillColor,
        bool occludedOnly = false){
            throw new NotImplementedException(); // TODO
        }
        
    }
}