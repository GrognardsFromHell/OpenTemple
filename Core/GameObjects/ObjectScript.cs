using System.Runtime.InteropServices;

namespace OpenTemple.Core.GameObjects;

// Stored in obj_f.script_idx array
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ObjectScript
{
    public int unk1;
    public uint counters;
    public int scriptId;

    public byte Counter1
    {
        get => (byte) (counters & 0xFF);
        set => counters = (uint) ((counters & ~0xFF) | value);
    }
    public byte Counter2
    {
        get => (byte) ((counters >> 8) & 0xFF);
        set => counters = (uint) ((counters & ~0xFF00) | ((uint) value << 8));
    }

    public byte Counter3
    {
        get => (byte) ((counters >> 16) & 0xFF);
        set => counters = (uint) ((counters & ~0xFF0000) | ((uint) value << 16));
    }
    
    public byte Counter4
    {
        get => (byte) ((counters >> 24) & 0xFF);
        set => counters = (counters & ~0xFF000000) | ((uint) value << 24);
    }

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