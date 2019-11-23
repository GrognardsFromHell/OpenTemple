using System.Collections.Generic;
using System.IO;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedReputationState
    {
        public List<SavedReputation> SavedReputations { get; set; } = new List<SavedReputation>();

        [TempleDllLocation(0x100542f0)]
        public static SavedReputationState Read(BinaryReader reader)
        {
            var reputations = reader.ReadIndexTable<SavedReputation>(16, ReadReputation);

            var result = new SavedReputationState();
            result.SavedReputations = new List<SavedReputation>(reputations.Values);
            return result;
        }

        private static void ReadReputation(BinaryReader reader, out SavedReputation reputation)
        {
            var isEarned = reader.ReadInt32() != 0;
            var repId = reader.ReadInt32();
            var timeEarned = reader.ReadGameTime();

            reputation = new SavedReputation(isEarned, repId, timeEarned.ToTimePoint());
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
}