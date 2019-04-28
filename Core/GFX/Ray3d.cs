using System.Numerics;

namespace SpicyTemple.Core.GFX
{

    public struct Ray3d
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray3d(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }
    }

}