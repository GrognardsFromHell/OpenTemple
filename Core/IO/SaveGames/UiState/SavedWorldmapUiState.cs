using System.Collections.Generic;
using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.UiState
{
    public class SavedWorldmapUiState
    {
        public List<SavedWorldmapLocation> Locations { get; set; } = new List<SavedWorldmapLocation>();

        public int RandomEncounterX { get; set; }

        public int RandomEncounterY { get; set; }

        public int RandomEncounterStatus { get; set; }

        public bool NeedToCleanEncounterMap { get; set; }

        public bool DontAskToExitEncounterMap { get; set; }

        [TempleDllLocation(0x1015e0f0)]
        public static SavedWorldmapUiState Read(BinaryReader reader)
        {
            var result = new SavedWorldmapUiState();

            var visitedCount = reader.ReadInt32();
            result.Locations.Capacity = visitedCount;

            for (var i = 0; i < 20; i++)
            {
                var visitedMap = SavedWorldmapLocation.Read(reader);

                if (i < visitedCount)
                {
                    result.Locations.Add(visitedMap);
                }
            }

            result.RandomEncounterX = reader.ReadInt32();
            result.RandomEncounterY = reader.ReadInt32();
            result.RandomEncounterStatus = reader.ReadInt32();
            result.NeedToCleanEncounterMap = reader.ReadInt32() != 0;
            result.DontAskToExitEncounterMap = reader.ReadInt32() != 0;

            return result;
        }
    }

    public readonly struct SavedWorldmapLocation
    {
        // Location index of the worldmap location
        public readonly int Index;

        public readonly bool Discovered;

        public readonly bool Visited;

        public static SavedWorldmapLocation Read(BinaryReader reader)
        {
            var packed = reader.ReadInt32();

            return new SavedWorldmapLocation(
                packed >> 8,
                (packed & 1) != 0,
                (packed & 2) != 0
            );
        }

        public SavedWorldmapLocation(int index, bool discovered, bool visited)
        {
            Index = index;
            Discovered = discovered;
            Visited = visited;
        }
    }
}