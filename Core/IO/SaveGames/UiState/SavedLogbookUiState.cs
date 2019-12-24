using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.IO.SaveGames.UiState
{
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
    }

    public class SavedLogbookEgoUiState
    {
        public int ActiveTab { get; set; }

        public SavedLogbookEgoCombatUiState Combat { get; set; }

        public SavedLogbookEgoDamageUiState Damage { get; set; }

        public SavedLogbookEgoMiscUiState Misc { get; set; }

        public static SavedLogbookEgoUiState Read(BinaryReader reader)
        {
            var result = new SavedLogbookEgoUiState();
            result.ActiveTab = reader.ReadInt32();
            result.Combat = SavedLogbookEgoCombatUiState.Read(reader);
            result.Damage = SavedLogbookEgoDamageUiState.Read(reader);
            result.Misc = SavedLogbookEgoMiscUiState.Read(reader);
            return result;
        }
    }

    public class SavedLogbookEgoCombatUiState
    {
        public List<LogbookCombatEntry> A { get; set; }
        public List<LogbookCombatEntry> B { get; set; }
        public List<LogbookCombatEntry> C { get; set; }
        public List<LogbookCombatEntry> D { get; set; }
        public List<LogbookCombatEntry> E { get; set; }
        public List<LogbookCombatEntry> F { get; set; }
        public List<LogbookCombatEntry> G { get; set; }

        public List<SavedLogbookEgoKill> Kills { get; set; }
        public List<SavedLogbookEgoKill> MostExperienceEncounterKilled { get; set; }
        public int MostExperienceEncounterMapId { get; set; }
        public int MostExperienceEncounterEnemies { get; set; }
        public int MostExperienceEncounterExperience { get; set; }

        public List<SavedLogbookEgoKill> MostExperienceEncounterKilledTemp { get; set; }

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
    }

    public class SavedLogbookEgoDamageUiState
    {
        public List<LogbookCombatEntry> A { get; set; }
        public List<LogbookCombatEntry> B { get; set; }
        public List<LogbookCombatEntry> C { get; set; }
        public List<LogbookCombatEntry> D { get; set; }

        public static SavedLogbookEgoDamageUiState Read(BinaryReader reader)
        {
            var result = new SavedLogbookEgoDamageUiState();
            result.A = LogbookCombatEntry.ReadEntries(reader);
            result.B = LogbookCombatEntry.ReadEntries(reader);
            result.C = LogbookCombatEntry.ReadEntries(reader);
            result.D = LogbookCombatEntry.ReadEntries(reader);
            return result;
        }
    }

    public class SavedLogbookEgoMiscUiState
    {
        public List<LogbookCombatEntry> A { get; set; }
        public List<LogbookCombatEntry> B { get; set; }
        public List<LogbookCombatEntry> C { get; set; }
        public List<LogbookCombatEntry> D { get; set; }
        public List<LogbookCombatEntry> E { get; set; }
        public List<LogbookCombatEntry> F { get; set; }

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

        public static List<LogbookCombatEntry> ReadEntries(BinaryReader reader)
        {
            var result = new List<LogbookCombatEntry>(5);
            for (var i = 0; i < 5; i++)
            {
                result.Add(Read(reader));
            }

            return result;
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
    }

    public class SavedLogbookRumorsUiState
    {
        public int CurrentPage { get; set; }

        public List<SavedRumorState> Rumors { get; set; }

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
    }

    public readonly struct SavedRumorState
    {
        public readonly int Id;

        public readonly TimePoint FirstHeard;

        [TempleDllLocation(0x10190410)]
        public static SavedRumorState Read(BinaryReader reader)
        {
            var id = reader.ReadInt32();
            reader.ReadInt32(); // I think this is padding
            var firstHeard = reader.ReadGameTime().ToTimePoint();
            return new SavedRumorState(id, firstHeard);
        }

        public SavedRumorState(int id, TimePoint firstHeard)
        {
            Id = id;
            FirstHeard = firstHeard;
        }
    }
}