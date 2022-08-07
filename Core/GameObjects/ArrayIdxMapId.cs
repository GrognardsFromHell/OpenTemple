namespace OpenTemple.Core.GameObjects;

/// <summary>
/// Used as an opaque identifier to a pooled ArrayIndexBitmaps entry.
/// </summary>
public readonly struct ArrayIdxMapId
{
    public readonly int Id;

    public bool IsValid => Id != -1;

    public static readonly ArrayIdxMapId Null = new(-1);

    public ArrayIdxMapId(int id)
    {
        Id = id;
    }

    public bool Equals(ArrayIdxMapId other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is ArrayIdxMapId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(ArrayIdxMapId left, ArrayIdxMapId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArrayIdxMapId left, ArrayIdxMapId right)
    {
        return !left.Equals(right);
    }
}