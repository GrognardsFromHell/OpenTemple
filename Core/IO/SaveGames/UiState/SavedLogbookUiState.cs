using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.IO.SaveGames.UiState;

public class SavedLogbookUiState
{
    public int ActiveTab { get; set; }

    public SavedLogbookEgoUiState Ego { get; set; }

    public SavedLogbookKeysUiState Keys { get; set; }

    public SavedLogbookQuestsUiState Quests { get; set; }

    public SavedLogbookRumorsUiState Rumors { get; set; }

    [TempleDllLocation(0x10125e40)]
    public static SavedLogbookUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookUiState();
        result.Ego = SavedLogbookEgoUiState.Read(reader);
        result.Keys = SavedLogbookKeysUiState.Read(reader);
        result.Quests = SavedLogbookQuestsUiState.Read(reader);
        result.Rumors = SavedLogbookRumorsUiState.Read(reader);

        result.ActiveTab = reader.ReadInt32();
        return result;
    }

    [TempleDllLocation(0x10125de0)]
    public void Write(BinaryWriter writer)
    {
        Ego.Write(writer);
        Keys.Write(writer);
        Quests.Write(writer);
        Rumors.Write(writer);

        writer.WriteInt32(ActiveTab);
    }
}

public class SavedLogbookEgoUiState
{
    public int ActiveTab { get; set; }

    public SavedLogbookEgoCombatUiState Combat { get; set; } = new SavedLogbookEgoCombatUiState();

    public SavedLogbookEgoDamageUiState Damage { get; set; } = new SavedLogbookEgoDamageUiState();

    public SavedLogbookEgoMiscUiState Misc { get; set; } = new SavedLogbookEgoMiscUiState();

    public static SavedLogbookEgoUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookEgoUiState();
        result.ActiveTab = reader.ReadInt32();
        result.Combat = SavedLogbookEgoCombatUiState.Read(reader);
        result.Damage = SavedLogbookEgoDamageUiState.Read(reader);
        result.Misc = SavedLogbookEgoMiscUiState.Read(reader);
        return result;
    }

    [TempleDllLocation(0x10199120)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(ActiveTab);
        Combat.Write(writer);
        Damage.Write(writer);
        Misc.Write(writer);
    }
}

public class SavedLogbookEgoCombatUiState
{
    public List<LogbookCombatEntry> A { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> B { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> C { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> D { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> E { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> F { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> G { get; set; } = new List<LogbookCombatEntry>();

    public List<SavedLogbookEgoKill> Kills { get; set; } = new List<SavedLogbookEgoKill>();
    public List<SavedLogbookEgoKill> MostExperienceEncounterKilled { get; set; } = new List<SavedLogbookEgoKill>();
    public int MostExperienceEncounterMapId { get; set; }
    public int MostExperienceEncounterEnemies { get; set; }
    public int MostExperienceEncounterExperience { get; set; }

    public List<SavedLogbookEgoKill> MostExperienceEncounterKilledTemp { get; set; } = new List<SavedLogbookEgoKill>();

    [TempleDllLocation(0x101d0650)]
    public static SavedLogbookEgoCombatUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookEgoCombatUiState();
        result.A = LogbookCombatEntry.ReadEntries(reader);
        result.B = LogbookCombatEntry.ReadEntries(reader);
        result.C = LogbookCombatEntry.ReadEntries(reader);
        result.D = LogbookCombatEntry.ReadEntries(reader);
        result.E = LogbookCombatEntry.ReadEntries(reader);
        result.F = LogbookCombatEntry.ReadEntries(reader);
        result.G = LogbookCombatEntry.ReadEntries(reader);

        var count = reader.ReadInt32();
        if (count > 400)
        {
            throw new CorruptSaveException($"Different critters killed exceed 400: " + count);
        }

        result.Kills = new List<SavedLogbookEgoKill>(count);
        for (var i = 0; i < count; i++)
        {
            result.Kills.Add(SavedLogbookEgoKill.Read(reader));
        }

        // Skip the sorted index lists because we'd rather just resort on the fly
        reader.BaseStream.Seek(400 * 4, SeekOrigin.Current); // Sorted by HD
        reader.BaseStream.Seek(400 * 4, SeekOrigin.Current); // Sorted by CR
        reader.BaseStream.Seek(400 * 4, SeekOrigin.Current); // Sorted by Name

        count = reader.ReadInt32();
        if (count > 20)
        {
            throw new CorruptSaveException($"Critters in encounter-with-most-experience exceed 20: " + count);
        }

        result.MostExperienceEncounterKilled = new List<SavedLogbookEgoKill>(count);
        for (var i = 0; i < count; i++)
        {
            result.MostExperienceEncounterKilled.Add(SavedLogbookEgoKill.Read(reader));
        }

        // Skip the sorted index lists because we'd rather just resort on the fly
        reader.BaseStream.Seek(20 * 4, SeekOrigin.Current); // Sorted by HD
        reader.BaseStream.Seek(20 * 4, SeekOrigin.Current); // Sorted by CR
        reader.BaseStream.Seek(20 * 4, SeekOrigin.Current); // Sorted by Name

        result.MostExperienceEncounterMapId = reader.ReadInt32();
        result.MostExperienceEncounterEnemies = reader.ReadInt32();
        result.MostExperienceEncounterExperience = reader.ReadInt32();

        // I believe this might be valid only for the currently active combat session
        count = reader.ReadInt32();
        if (count > 20)
        {
            throw new CorruptSaveException($"Different critters killed in current combat encounter exceed 20: " +
                                           count);
        }

        result.MostExperienceEncounterKilledTemp = new List<SavedLogbookEgoKill>(count);
        for (var i = 0; i < count; i++)
        {
            result.MostExperienceEncounterKilledTemp.Add(SavedLogbookEgoKill.Read(reader));
        }

        // Skip the sorted index lists because we'd rather just resort on the fly
        reader.BaseStream.Seek(20 * 4, SeekOrigin.Current); // Sorted by HD
        reader.BaseStream.Seek(20 * 4, SeekOrigin.Current); // Sorted by CR
        reader.BaseStream.Seek(20 * 4, SeekOrigin.Current); // Sorted by Name

        return result;
    }

    private static void WriteSortedIndices(BinaryWriter writer, IReadOnlyCollection<int> indices, int fixedLength)
    {
        foreach (var index in indices)
        {
            writer.WriteInt32(index);
        }

        // ToEE uses fixed length entry lists
        for (var i = indices.Count; i < fixedLength; i++)
        {
            writer.WriteInt32(0);
        }
    }

    [TempleDllLocation(0x101d02d0)]
    public void Write(BinaryWriter writer)
    {
        LogbookCombatEntry.WriteEntries(writer, A);
        LogbookCombatEntry.WriteEntries(writer, B);
        LogbookCombatEntry.WriteEntries(writer, C);
        LogbookCombatEntry.WriteEntries(writer, D);
        LogbookCombatEntry.WriteEntries(writer, E);
        LogbookCombatEntry.WriteEntries(writer, F);
        LogbookCombatEntry.WriteEntries(writer, G);

        WriteKillsList(writer, Kills, 400);
        WriteKillsList(writer, MostExperienceEncounterKilled, 20);

        writer.WriteInt32(MostExperienceEncounterMapId);
        writer.WriteInt32(MostExperienceEncounterEnemies);
        writer.WriteInt32(MostExperienceEncounterExperience);

        // I believe this might be valid only for the currently active combat session
        WriteKillsList(writer, MostExperienceEncounterKilledTemp, 20);

    }

    private static void WriteKillsList(BinaryWriter writer, IList<SavedLogbookEgoKill> kills, int fixedLength)
    {
        writer.WriteInt32(kills.Count);
        if (kills.Count > fixedLength)
        {
            throw new CorruptSaveException($"Different critters killed exceed {fixedLength}: {kills.Count}");
        }

        foreach (var kill in kills)
        {
            kill.Write(writer);
        }

        // Skip the sorted index lists because we'd rather just resort on the fly
        var sortedKills = Enumerable.Range(0, kills.Count).ToList();

        // Sorted by HD
        sortedKills.Sort((x, y) => kills[x].HitDice.CompareTo(kills[y].HitDice));
        WriteSortedIndices(writer, sortedKills, fixedLength);

        // Sorted by CR
        sortedKills.Sort((x, y) => kills[x].ChallengeRating.CompareTo(kills[y].ChallengeRating));
        WriteSortedIndices(writer, sortedKills, fixedLength);

        // Sorted by Name
        sortedKills.Sort((x, y) => string.Compare(kills[x].Name, kills[y].Name, StringComparison.Ordinal));
        WriteSortedIndices(writer, sortedKills, fixedLength);
    }

}

public class SavedLogbookEgoKill
{
    public string Name { get; set; }

    public int ChallengeRating { get; set; }

    public int HitDice { get; set; }

    public int ArmorClass { get; set; }

    public int Count { get; set; }

    public GameTime FirstKilled { get; set; }

    public GameTime LastKilled { get; set; }

    public static SavedLogbookEgoKill Read(BinaryReader reader)
    {
        var result = new SavedLogbookEgoKill();
        result.Name = reader.ReadFixedString(260);
        reader.ReadInt32(); // Padding
        result.ChallengeRating = reader.ReadInt32();
        result.HitDice = reader.ReadInt32();
        result.ArmorClass = reader.ReadInt32();
        result.Count = reader.ReadInt32();
        result.FirstKilled = reader.ReadGameTime();
        result.LastKilled = reader.ReadGameTime();
        return result;
    }

    public void Write(BinaryWriter writer)
    {
        writer.WriteFixedString(260, Name);
        writer.WriteInt32(0); // Padding
        writer.WriteInt32(ChallengeRating);
        writer.WriteInt32(HitDice);
        writer.WriteInt32(ArmorClass);
        writer.WriteInt32(Count);
        writer.WriteGameTime(FirstKilled);
        writer.WriteGameTime(LastKilled);
    }
}

public class SavedLogbookEgoDamageUiState
{
    public List<LogbookCombatEntry> A { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> B { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> C { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> D { get; set; } = new List<LogbookCombatEntry>();

    public static SavedLogbookEgoDamageUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookEgoDamageUiState();
        result.A = LogbookCombatEntry.ReadEntries(reader);
        result.B = LogbookCombatEntry.ReadEntries(reader);
        result.C = LogbookCombatEntry.ReadEntries(reader);
        result.D = LogbookCombatEntry.ReadEntries(reader);
        return result;
    }

    [TempleDllLocation(0x101ce800)]
    public void Write(BinaryWriter writer)
    {
        LogbookCombatEntry.WriteEntries(writer, A);
        LogbookCombatEntry.WriteEntries(writer, B);
        LogbookCombatEntry.WriteEntries(writer, C);
        LogbookCombatEntry.WriteEntries(writer, D);
    }
}

public class SavedLogbookEgoMiscUiState
{
    public List<LogbookCombatEntry> A { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> B { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> C { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> D { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> E { get; set; } = new List<LogbookCombatEntry>();
    public List<LogbookCombatEntry> F { get; set; } = new List<LogbookCombatEntry>();

    [TempleDllLocation(0x101ccfc0)]
    public static SavedLogbookEgoMiscUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookEgoMiscUiState();
        result.A = LogbookCombatEntry.ReadEntries(reader);
        result.B = LogbookCombatEntry.ReadEntries(reader);
        result.C = LogbookCombatEntry.ReadEntries(reader);
        result.D = LogbookCombatEntry.ReadEntries(reader);
        result.E = LogbookCombatEntry.ReadEntries(reader);
        result.F = LogbookCombatEntry.ReadEntries(reader);
        return result;
    }

    [TempleDllLocation(0x101ccf70)]
    public void Write(BinaryWriter writer)
    {
        LogbookCombatEntry.WriteEntries(writer, A);
        LogbookCombatEntry.WriteEntries(writer, B);
        LogbookCombatEntry.WriteEntries(writer, C);
        LogbookCombatEntry.WriteEntries(writer, D);
        LogbookCombatEntry.WriteEntries(writer, E);
        LogbookCombatEntry.WriteEntries(writer, F);
    }
}

public struct LogbookCombatEntry
{
    public ObjectId Id { get; set; }

    public int Count { get; set; }

    public int ProtoId { get; set; }

    public static LogbookCombatEntry Read(BinaryReader reader)
    {
        var result = new LogbookCombatEntry();
        result.Id = reader.ReadObjectId();
        result.Count = reader.ReadInt32();
        result.ProtoId = reader.ReadInt32();
        return result;
    }

    public void Write(BinaryWriter writer)
    {
        writer.WriteObjectId(Id);
        writer.WriteInt32(Count);
        writer.WriteInt32(ProtoId);
    }

    public static List<LogbookCombatEntry> ReadEntries(BinaryReader reader)
    {
        var result = new List<LogbookCombatEntry>(5);
        for (var i = 0; i < 5; i++)
        {
            var entry = Read(reader);
            if (entry.Count > 0)
            {
                result.Add(entry);
            }
        }

        return result;
    }

    public static void WriteEntries(BinaryWriter writer, List<LogbookCombatEntry> entries)
    {
        Trace.Assert(entries.Count <= 5);
        for (var i = 0; i < 5; i++)
        {
            if (i < entries.Count)
            {
                entries[i].Write(writer);
            }
            else
            {
                writer.WriteObjectId(ObjectId.CreateNull());
                writer.WriteInt32(0);
                writer.WriteInt32(0);
            }
        }
    }
}

public class SavedLogbookKeysUiState
{
    public Dictionary<int, SavedKeyState> Keys { get; } = new Dictionary<int, SavedKeyState>();

    // Notify the player if new keys are acquired
    public bool EnableKeyNotifications { get; set; }

    [TempleDllLocation(0x10195360)]
    public static SavedLogbookKeysUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookKeysUiState();
        result.Keys.EnsureCapacity(100);

        for (var i = 0; i < 100; i++)
        {
            var acquired = reader.ReadGameTime().ToTimePoint();
            var used = reader.ReadGameTime().ToTimePoint();
            if (acquired != default || used != default)
            {
                result.Keys[i] = new SavedKeyState(acquired, used);
            }
        }

        result.EnableKeyNotifications = reader.ReadInt32() != 0;

        return result;
    }

    [TempleDllLocation(0x101952c0)]
    public void Write(BinaryWriter writer)
    {
        GameTime unusedAndUnaquired = default;
        for (var i = 0; i < 100; i++)
        {
            if (Keys.TryGetValue(i, out var keyState))
            {
                writer.WriteGameTime(keyState.Acquired);
                writer.WriteGameTime(keyState.Used);
            }
            else
            {
                writer.WriteGameTime(unusedAndUnaquired);
                writer.WriteGameTime(unusedAndUnaquired);
            }
        }

        writer.WriteInt32(EnableKeyNotifications ? 1 : 0);
    }
}

public readonly struct SavedKeyState
{
    public readonly TimePoint Acquired;
    public readonly TimePoint Used;

    public SavedKeyState(TimePoint acquired, TimePoint used)
    {
        Acquired = acquired;
        Used = used;
    }
}

public class SavedLogbookQuestsUiState
{
    public int ActiveTab { get; set; }

    [TempleDllLocation(0x101784a0)]
    public static SavedLogbookQuestsUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookQuestsUiState();
        result.ActiveTab = reader.ReadInt32();
        return result;
    }

    [TempleDllLocation(0x10178460)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(ActiveTab);
    }
}

public class SavedLogbookRumorsUiState
{
    public int CurrentPage { get; set; }

    public List<SavedRumorState> Rumors { get; set; } = new List<SavedRumorState>();

    [TempleDllLocation(0x10190410)]
    public static SavedLogbookRumorsUiState Read(BinaryReader reader)
    {
        var result = new SavedLogbookRumorsUiState();

        var count = reader.ReadInt32();
        result.CurrentPage = reader.ReadInt32();
        result.Rumors = new List<SavedRumorState>(count);
        for (var i = 0; i < count; i++)
        {
            result.Rumors.Add(SavedRumorState.Read(reader));
        }

        return result;
    }

    [TempleDllLocation(0x10190380)]
    public void Write(BinaryWriter writer)
    {

        writer.WriteInt32(Rumors.Count);
        writer.WriteInt32(CurrentPage);
        foreach (var rumor in Rumors)
        {
            rumor.Write(writer);
        }
    }
}

public readonly struct SavedRumorState
{
    public readonly int Id;

    public readonly TimePoint FirstHeard;

    public SavedRumorState(int id, TimePoint firstHeard)
    {
        Id = id;
        FirstHeard = firstHeard;
    }

    [TempleDllLocation(0x10190410)]
    public static SavedRumorState Read(BinaryReader reader)
    {
        var id = reader.ReadInt32();
        reader.ReadInt32(); // I think this is padding
        var firstHeard = reader.ReadGameTime().ToTimePoint();
        return new SavedRumorState(id, firstHeard);
    }

    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(Id);
        writer.WriteInt32(0); // I think this is padding
        writer.WriteGameTime(FirstHeard);
    }
}