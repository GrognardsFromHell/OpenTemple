using JetBrains.Annotations;
using Microsoft.VisualBasic.CompilerServices;

namespace SpicyTemple.Core.GFX
{

    public interface IRefCounted
    {
        void Reference();

        void Dereference();

    }

    public static class RefCountedExtensions
    {

        public static ResourceRef<T> Ref<T>(this T obj) where T : class, IRefCounted
        {
            return new ResourceRef<T>(obj);
        }

    }

    public abstract class GpuResource<T> : IRefCounted where T : GpuResource<T>
    {
        private int _refs = 0;

        protected abstract void FreeResource();

        public void Reference()
        {
            ++_refs;
        }

        public void Dereference()
        {
            if (--_refs <= 0)
            {
                FreeResource();
            }
        }

    }
}