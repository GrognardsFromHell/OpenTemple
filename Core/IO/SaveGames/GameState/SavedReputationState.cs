using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedReputationState
{

    private const int ItemSize = 16;

    public List<SavedReputation> SavedReputations { get; set; } = new List<SavedReputation>();

    [TempleDllLocation(0x100542f0)]
    public static SavedReputationState Read(BinaryReader reader)
    {
        var reputations = reader.ReadIndexTable<SavedReputation>(ItemSize, ReadReputation);

        var result = new SavedReputationState();
        result.SavedReputations = new List<SavedReputation>(reputations.Values);
        return result;
    }

    [TempleDllLocation(0x100542d0)]
    public void Write(BinaryWriter writer)
    {
        var indexTable = SavedReputations.ToDictionary(
            r => r.Id,
            r => r
        );
        writer.WriteIndexTable(indexTable, ItemSize, WriteReputation);
    }

    private static void ReadReputation(BinaryReader reader, out SavedReputation reputation)
    {
        var isEarned = reader.ReadInt32() != 0;
        var repId = reader.ReadInt32();
        var timeEarned = reader.ReadGameTime();

        reputation = new SavedReputation(isEarned, repId, timeEarned.ToTimePoint());
    }

    private static void WriteReputation(BinaryWriter writer, SavedReputation reputation)
    {
        writer.WriteInt32(reputation.IsEarned ? 1 : 0);
        writer.WriteInt32(reputation.Id);
        writer.WriteGameTime(reputation.TimeEarned);
    }
}

public readonly struct SavedReputation
{
    public readonly bool IsEarned;
    public readonly int Id;
    public readonly TimePoint TimeEarned;

    public SavedReputation(bool isEarned, int id, TimePoint timeEarned)
    {
        IsEarned = isEarned;
        Id = id;
        TimeEarned = timeEarned;
    }
}