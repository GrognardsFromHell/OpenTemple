using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.GameObjects;

public enum ObjectIdKind
{
    Null = 0,
    Prototype = 1,
    Permanent = 2,
    Positional = 3,
    Handle = 0xFFFE,
    Blocked = 0xFFFF
}

[StructLayout(LayoutKind.Sequential)]
public struct PositionalId
{
    public int X;
    public int Y;
    public int TempId;
    public int MapId;

    public PositionalId(int mapId, int x, int y, int tempId)
    {
        X = x;
        Y = y;
        TempId = tempId;
        MapId = mapId;
    }

    public bool Equals(PositionalId other)
    {
        return X == other.X && Y == other.Y && TempId == other.TempId && MapId == other.MapId;
    }

    public override bool Equals(object obj)
    {
        return obj is PositionalId other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = X;
            hashCode = (hashCode * 397) ^ Y;
            hashCode = (hashCode * 397) ^ TempId;
            hashCode = (hashCode * 397) ^ MapId;
            return hashCode;
        }
    }

    public static bool operator ==(PositionalId left, PositionalId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PositionalId left, PositionalId right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{X:X08}_{Y:X08}_{TempId:X08}_{MapId:X08}";
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ObjectId : IComparable<ObjectId>
{
    public static readonly int Size = Marshal.SizeOf<ObjectId>();

    static ObjectId()
    {
        Trace.Assert(Size == 24);
    }

    public ObjectIdKind Type => subtype;

    [FieldOffset(0)] public ObjectIdKind subtype;

    [FieldOffset(8)] public Guid guid;

    [FieldOffset(8)] public int protoId;

    [FieldOffset(8)]
    private PositionalId _positionalId;

    public bool IsNull => subtype == ObjectIdKind.Null;

    public bool IsPermanent => subtype == ObjectIdKind.Permanent;

    public bool IsPrototype => subtype == ObjectIdKind.Prototype;

    public bool IsPositional => subtype == ObjectIdKind.Positional;
    public bool IsBlocked => subtype == ObjectIdKind.Blocked;

    // Can this object id be persisted and later restored to a handle?
    public bool IsPersistable() => IsNull || IsPermanent || IsPrototype || IsPositional;

    public ushort PrototypeId
    {
        get
        {
            Trace.Assert(IsPrototype);
            return (ushort) protoId;
        }
    }

    public PositionalId PositionalId
    {
        get
        {
            Trace.Assert(IsPositional);
            return _positionalId;
        }
    }

    public Guid PermanentId
    {
        get
        {
            Trace.Assert(IsPermanent);
            return guid;
        }
    }

    public static implicit operator bool(in ObjectId id) => !id.IsNull;

    public bool Equals(ObjectId other)
    {
        if (subtype != other.subtype) {
            return false;
        }

        switch (subtype) {
            case ObjectIdKind.Null:
                return true;
            case ObjectIdKind.Prototype:
                if (protoId == other.protoId) {
                    return true;
                } else {
                    return false;
                }
            case ObjectIdKind.Permanent:
                if (guid == other.guid) {
                    return true;
                } else {
                    return false;
                }
            case ObjectIdKind.Positional:
                if (_positionalId.X == other._positionalId.X
                    && _positionalId.Y == other._positionalId.Y
                    && _positionalId.TempId == other._positionalId.TempId
                    && _positionalId.MapId == other._positionalId.MapId) {
                    return true;
                } else {
                    return false;
                }
            default:
                return false;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is ObjectId other && Equals(other);
    }

    public override int GetHashCode()
    {
        switch (subtype) {
            case ObjectIdKind.Prototype:
                return protoId.GetHashCode();
            case ObjectIdKind.Permanent:
                return guid.GetHashCode();
            case ObjectIdKind.Positional:
                return _positionalId.GetHashCode();
            default:
                return 0;
        }
    }

    public static bool operator ==(ObjectId left, ObjectId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ObjectId left, ObjectId right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        switch (subtype)
        {
            case ObjectIdKind.Null:
                return "NULL";
            case ObjectIdKind.Prototype:
                return $"A_{protoId:X08}";
            case ObjectIdKind.Permanent:
                return $"G_{guid.ToString().ToUpperInvariant()}";
            case ObjectIdKind.Positional:
                return $"P_{_positionalId}";
            case ObjectIdKind.Blocked:
                return "Blocked";
            default:
                return "UNKNOWN";
        }
    }

    // Randomly generates a GUID and returns an object id that contains it
    public static ObjectId CreatePermanent() => CreatePermanent(Guid.NewGuid());

    public static ObjectId CreatePermanent(in Guid guid)
    {
        return new ObjectId {subtype = ObjectIdKind.Permanent, guid = guid};
    }

    // Creates a positional object id
    public static ObjectId CreatePositional(int mapId, int tileX, int tileY, int tempId)
    {
        return CreatePositional(new PositionalId(mapId, tileX, tileY, tempId));
    }

    public static ObjectId CreatePositional(in PositionalId positionalId)
    {
        return new ObjectId
        {
            subtype = ObjectIdKind.Positional,
            _positionalId = positionalId
        };
    }

    // Creates a prototype object id
    public static ObjectId CreatePrototype(ushort prototypeId)
    {
        return new ObjectId
        {
            subtype = ObjectIdKind.Prototype,
            protoId = prototypeId
        };
    }

    // Creates a null object id
    public static ObjectId CreateNull()
    {
        return new ObjectId
        {
            subtype = ObjectIdKind.Null
        };
    }

    public static ObjectId CreateBlocked()
    {
        return new ObjectId
        {
            subtype = ObjectIdKind.Blocked
        };
    }

    public int CompareTo(ObjectId other)
    {
        var subtypeComparison = subtype.CompareTo(other.subtype);
        if (subtypeComparison != 0)
        {
            return subtypeComparison;
        }

        var guidComparison = guid.CompareTo(other.guid);
        if (guidComparison != 0)
        {
            return guidComparison;
        }

        return protoId.CompareTo(other.protoId);
    }

    public static bool operator <(ObjectId left, ObjectId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(ObjectId left, ObjectId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(ObjectId left, ObjectId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(ObjectId left, ObjectId right)
    {
        return left.CompareTo(right) >= 0;
    }
}