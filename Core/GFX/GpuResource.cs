namespace SpicyTemple.Core.GFX
{
    public abstract class GpuResource
    {
        private int _refs = 0;

        protected RenderingDevice Device { get; private set; }

        internal GpuResource(RenderingDevice device)
        {
            Device = device;
        }

        protected abstract void FreeResource();

        internal void Reference()
        {
            ++_refs;
        }

        internal void Dereference()
        {
            if (--_refs <= 0)
            {
                FreeResource();
                Device = null;
            }
        }
    }
}