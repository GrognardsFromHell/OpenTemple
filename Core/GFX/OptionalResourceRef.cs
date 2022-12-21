using System;

namespace OpenTemple.Core.GFX;

/// <summary>
/// Same as <see cref="OpenTemple.Core.GFX.ResourceRef{T}"/>, but expressing that the referenced resource may be null.
/// </summary>
public struct OptionalResourceRef<T> : IDisposable where T : class, IRefCounted
{
    public T? Resource { get; private set; }

    public OptionalResourceRef(T? resource)
    {
        Resource = resource;
        Resource?.Reference();
    }

    public static implicit operator T?(OptionalResourceRef<T> resourceRef) => resourceRef.Resource;
    
    public OptionalResourceRef<T> CloneRef()
    {
        return new OptionalResourceRef<T>(Resource);
    }

    public void Dispose()
    {
        Resource?.Dereference();
        Resource = null;
    }
}
