using System;

namespace OpenTemple.Core.GFX;

public struct ResourceRef<T> : IDisposable where T : class, IRefCounted
{
    public T Resource;

    public ResourceRef(T resource)
    {
        Resource = resource;
        Resource?.Reference();
    }

    public static implicit operator T(ResourceRef<T> resourceRef) => resourceRef.Resource;
    public bool IsValid => Resource != null;

    public ResourceRef<T> CloneRef()
    {
        return new ResourceRef<T>(Resource);
    }

    public void Dispose()
    {
        Resource?.Dereference();
        Resource = null;
    }
}