using System;

namespace SpicyTemple.Core.GFX
{
    public struct ResourceRef<T> : IDisposable where T : GpuResource
    {
        public T Resource;

        public ResourceRef(T resource)
        {
            Resource = resource;
            Resource.Reference();
        }

        public ResourceRef<T> CloneRef()
        {
            return new ResourceRef<T>(Resource);
        }

        public void Dispose()
        {
            Resource.Dereference();
            Resource = null;
        }
    }
}