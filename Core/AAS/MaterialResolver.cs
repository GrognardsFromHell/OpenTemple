using System;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.AAS
{
    internal class MaterialResolver : IMaterialResolver
    {
        public MaterialResolver(Func<string, object> resolver)
        {
            resolver_ = resolver;
        }

        public AasMaterial Acquire(ReadOnlySpan<char> materialName, ReadOnlySpan<char> context)
        {
            // Handle material replacement slots
            if (materialName == "HEAD")
            {
                return new AasMaterial(MaterialPlaceholderSlot.HEAD, null);
            }
            else if (materialName == "GLOVES")
            {
                return new AasMaterial(MaterialPlaceholderSlot.GLOVES, null);
            }
            else if (materialName == "CHEST")
            {
                return new AasMaterial(MaterialPlaceholderSlot.CHEST, null);
            }
            else if (materialName == "BOOTS")
            {
                return new AasMaterial(MaterialPlaceholderSlot.BOOTS, null);
            }

            return new AasMaterial(null, resolver_(new string(materialName)));
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
            return material.Slot.Value;
        }

        private readonly Func<string, object> resolver_;
    }
}