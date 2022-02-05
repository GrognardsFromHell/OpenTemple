using System.Collections.Generic;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedSecretDoorState
{
    public List<int> SeenSceneryNames { get; set; } = new();

    [TempleDllLocation(0x10046400)]
    public static SavedSecretDoorState Read(BinaryReader reader)
    {
        var knownCount = reader.ReadInt32();

        var result = new SavedSecretDoorState();
        result.SeenSceneryNames.Capacity = knownCount;
        for (var i = 0; i < knownCount; i++)
        {
            result.SeenSceneryNames.Add(reader.ReadInt32());
        }

        return result;
    }

    [TempleDllLocation(0x100463b0)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(SeenSceneryNames.Count);
        foreach (var nameId in SeenSceneryNames)
        {
            writer.WriteInt32(nameId);
        }
    }
}