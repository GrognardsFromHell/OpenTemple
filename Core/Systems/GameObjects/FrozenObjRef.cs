using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.GameObjects;

public readonly struct FrozenObjRef
{
    public static readonly FrozenObjRef Null = default;

    public readonly ObjectId guid;
    public readonly locXY location;
    public readonly int mapNumber;
    public readonly int padding;

    public FrozenObjRef(ObjectId guid, locXY location, int mapNumber) : this()
    {
        this.guid = guid;
        this.location = location;
        this.mapNumber = mapNumber;
        padding = 0;
    }

    public override string ToString()
    {
        return $"{guid} ({location}, {mapNumber})";
    }
}