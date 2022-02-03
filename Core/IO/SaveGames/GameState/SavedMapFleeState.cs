using System.IO;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedMapFleeState
{
    public int MapId { get; set; }
    public LocAndOffsets Location { get; set; }
    public int EnterX { get; set; }
    public int EnterY { get; set; }
    public bool IsFleeing { get; set; }

    public static SavedMapFleeState Load(byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));

        var result = new SavedMapFleeState();
        result.MapId = reader.ReadInt32();
        reader.ReadInt32(); // Padding
        result.Location = reader.ReadLocationAndOffsets();
        result.EnterX = reader.ReadInt32();
        result.EnterY = reader.ReadInt32();
        result.IsFleeing = reader.ReadInt32() != 0;
        return result;
    }

    public void Save(BinaryWriter writer)
    {
        writer.WriteInt32(MapId);
        writer.WriteInt32(0); // Padding
        writer.WriteLocationAndOffsets(Location);
        writer.WriteInt32(EnterX);
        writer.WriteInt32(EnterY);
        writer.WriteInt32(IsFleeing ? 1 : 0);
    }
}