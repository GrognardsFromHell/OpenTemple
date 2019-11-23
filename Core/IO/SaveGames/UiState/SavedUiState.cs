using System;
using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.UiState
{
    public class SavedUiState
    {
        public SavedRandomEncounterUiState RandomEncounterState { get; set; }
        public SavedDialogUiState DialogState { get; set; }
        public SavedLogbookUiState LogbookState { get; set; }
        public SavedTownmapUiState TownmapState { get; set; }
        public SavedWorldmapUiState WorldmapState { get; set; }
        public SavedPartyPoolUiState PartyPoolState { get; set; }
        public SavedCampingUiState CampingState { get; set; }
        public SavedHelpManagerUiState HelpManagerState { get; set; }

        public static SavedUiState Load(byte[] buffer)
        {
            using var reader = new BinaryReader(new MemoryStream(buffer));

            var result = new SavedUiState();

            var version = reader.ReadInt32();
            if (version != 0)
            {
                throw new CorruptSaveException($"Expected saved UI state version 0, but got: {version}");
            }

            SkipSentinel(reader); // Intgame UI
            SkipSentinel(reader); // Anim UI
            result.RandomEncounterState = LoadState(reader, SavedRandomEncounterUiState.Read);
            result.DialogState = LoadState(reader, SavedDialogUiState.Read);
            result.LogbookState = LoadState(reader, SavedLogbookUiState.Read);
            result.TownmapState = LoadState(reader, SavedTownmapUiState.Read);
            result.WorldmapState = LoadState(reader, SavedWorldmapUiState.Read);
            result.PartyPoolState = LoadState(reader, SavedPartyPoolUiState.Read);
            SkipSentinel(reader); // Party
            SkipSentinel(reader); // Formation
            result.CampingState = LoadState(reader, SavedCampingUiState.Read);
            SkipSentinel(reader); // Help Inventory UI
            SkipSentinel(reader); // UI Manager
            result.HelpManagerState = LoadState(reader, SavedHelpManagerUiState.Read);

            return result;
        }

        private static T LoadState<T>(BinaryReader reader, Func<BinaryReader, T> stateReader)
        {
            var posBeforeState = reader.BaseStream.Position;
            var state = stateReader(reader);
            var posAfterState = reader.BaseStream.Position;

            var sentinel = reader.ReadUInt32();
            if (sentinel != 0xBEEFCAFEu)
            {
                throw new CorruptSaveException($"Read sentinel 0x{sentinel:X} at pos {posAfterState} for " +
                                               $"state {typeof(T)} (which started at {posBeforeState})");
            }

            return state;
        }

        private static void SkipSentinel(BinaryReader reader)
        {
            var posAfterState = reader.BaseStream.Position;

            var sentinel = reader.ReadUInt32();
            if (sentinel != 0xBEEFCAFEu)
            {
                throw new CorruptSaveException($"Read sentinel 0x{sentinel:X} at pos {posAfterState}");
            }
        }
    }
}