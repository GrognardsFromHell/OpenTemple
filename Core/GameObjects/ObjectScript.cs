using System.Runtime.InteropServices;

namespace OpenTemple.Core.GameObjects;

// Stored in obj_f.script_idx array
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ObjectScript
{
    public int unk1;
    public uint counters;
    public int scriptId;

    public bool Equals(ObjectScript other)
    {
        return unk1 == other.unk1 && counters == other.counters && scriptId == other.scriptId;
    }

    public override bool Equals(object obj)
    {
        return obj is ObjectScript other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = unk1;
            hashCode = (hashCode * 397) ^ (int) counters;
            hashCode = (hashCode * 397) ^ scriptId;
            return hashCode;
        }
    }

    public static bool operator ==(ObjectScript left, ObjectScript right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ObjectScript left, ObjectScript right)
    {
        return !left.Equals(right);
    }
}