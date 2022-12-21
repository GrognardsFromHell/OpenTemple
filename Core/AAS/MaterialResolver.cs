using System;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.AAS;

internal class MaterialResolver : IMaterialResolver
{
    private readonly Func<string, object> _resolver;

    public MaterialResolver(Func<string, object> resolver)
    {
        _resolver = resolver;
    }

    public AasMaterial Acquire(ReadOnlySpan<char> materialName, ReadOnlySpan<char> context)
    {
        // Handle material replacement slots
        if (materialName.Equals("HEAD", StringComparison.Ordinal))
        {
            return new AasMaterial(MaterialPlaceholderSlot.HEAD, null);
        }
        else if (materialName.Equals("GLOVES", StringComparison.Ordinal))
        {
            return new AasMaterial(MaterialPlaceholderSlot.GLOVES, null);
        }
        else if (materialName.Equals("CHEST", StringComparison.Ordinal))
        {
            return new AasMaterial(MaterialPlaceholderSlot.CHEST, null);
        }
        else if (materialName.Equals("BOOTS", StringComparison.Ordinal))
        {
            return new AasMaterial(MaterialPlaceholderSlot.BOOTS, null);
        }

        return new AasMaterial(null, _resolver(new string(materialName)));
    }

    public void Release(AasMaterial material, ReadOnlySpan<char> context)
    {
        if (material.Material is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public bool IsMaterialPlaceholder(AasMaterial material)
    {
        return material.Slot.HasValue;
    }

    public MaterialPlaceholderSlot GetMaterialPlaceholderSlot(AasMaterial material)
    {
        if (!material.Slot.HasValue)
        {
            throw new InvalidOperationException("Cannot get the placeholder slot from a material that is not a placeholder.");
        }

        return material.Slot.Value;
    }
}