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

        public bool Equals(ObjHndl other)
        {
            return Handle == other.Handle;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjHndl other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public static bool operator ==(ObjHndl left, ObjHndl right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjHndl left, ObjHndl right)
        {
            return !left.Equals(right);
        }
    }
}