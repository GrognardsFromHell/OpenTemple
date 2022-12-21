using System;

namespace OpenTemple.Core.GFX;

public struct ResourceRef<T> : IDisposable where T : class, IRefCounted
{
    private T? _resource;
    
    public T Resource
    {
        get
        {
            if (_resource == null)
            {
                throw new InvalidOperationException("This resource has been disposed");
            }

            return _resource;
        }
    }

    public ResourceRef(T resource)
    {
        _resource = resource;
        _resource?.Reference();
    }

    public static implicit operator T(ResourceRef<T> resourceRef) => resourceRef.Resource;
    
    public static implicit operator OptionalResourceRef<T>(ResourceRef<T> resourceRef)
    {
        var result = new OptionalResourceRef<T>(resourceRef.Resource);
        // We need to de-reference once here because the constructor references it again
        resourceRef.Resource.Dereference();
        return result;
    }

    public bool IsValid => _resource != null;

    public ResourceRef<T> CloneRef()
    {
        return new ResourceRef<T>(Resource);
    }

    public void Dispose()
    {
        _resource?.Dereference();
        _resource = null;
    }
}