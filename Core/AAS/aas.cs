using System;
using System.Collections.Generic;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.AAS;

public struct AasAnimParams
{
    public uint flags;
    public uint unknown;
    public int locX;
    public int locY;
    public float offsetX;
    public float offsetY;
    public float offsetZ;
    public float rotation;
    public float scale;
    public float rotationRoll;
    public float rotationPitch;
    public float rotationYaw;
    public AasHandle parentAnim;
    public string attachedBoneName;
}

public class AnimEvents
{
    /**
         * Indicates the animation has ended.  This was indicated by the bit '2'.
         */
    public bool end;

    /**
         * Indicates that the frame on which an action should connect (weapon swings, etc.)
         * has occurred. This was indicated by the bit '1'.
         */
    public bool action;
}

public struct AasMaterial
{
    private object? _material;

    public MaterialPlaceholderSlot? Slot { get; }

    public object? Material
    {
        get => _material;
        set
        {
            if (_material is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _material = value;
        }
    }

    public AasMaterial(MaterialPlaceholderSlot? slot, object? material)
    {
        Slot = slot;
        _material = material;
    }

    public bool Equals(AasMaterial other)
    {
        return Equals(_material, other._material) && Slot == other.Slot;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is AasMaterial other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((_material != null ? _material.GetHashCode() : 0) * 397) ^ Slot.GetHashCode();
        }
    }

    public static bool operator ==(AasMaterial left, AasMaterial right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AasMaterial left, AasMaterial right)
    {
        return !left.Equals(right);
    }
}

public interface IMaterialResolver
{
    AasMaterial Acquire(ReadOnlySpan<char> materialName, ReadOnlySpan<char> context);
    void Release(AasMaterial material, ReadOnlySpan<char> context);

    bool IsMaterialPlaceholder(AasMaterial material);
    MaterialPlaceholderSlot GetMaterialPlaceholderSlot(AasMaterial material);
}

internal class ActiveModel : IDisposable
{
    public readonly AasHandle handle;
    public EncodedAnimId animId = new(WeaponAnim.None);
    public float floatconst = 6.3940001f;
    public TimePoint timeLoaded = TimePoint.Now;
    public readonly AnimatedModel model;
    public readonly Mesh mesh;
    public readonly Skeleton skeleton;
    public readonly List<Mesh> AdditionalMeshes = new();

    public ActiveModel(AasHandle handle, Mesh mesh, Skeleton skeleton)
    {
        this.handle = handle;
        this.mesh = mesh;
        this.skeleton = skeleton;
        this.model = new AnimatedModel(skeleton);
    }

    public void Dispose()
    {
    }
}