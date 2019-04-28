namespace SpicyTemple.Core.Systems.GameObjects
{
    public readonly struct ObjHndl
    {
        public static readonly ObjHndl Null = new ObjHndl();

        public readonly ulong Handle;

        public ObjHndl(ulong handle)
        {
            Handle = handle;
        }

        public static implicit operator bool(in ObjHndl handle) => handle.Handle != 0;

    }
}