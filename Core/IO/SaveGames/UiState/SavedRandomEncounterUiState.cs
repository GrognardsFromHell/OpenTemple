using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.UiState
{
    public class SavedRandomEncounterUiState
    {
        // TODO: I do not believe this system is actually being used
        [TempleDllLocation(0x10120d40)]
        public static SavedRandomEncounterUiState Read(BinaryReader reader)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                reader.ReadInt64();
            }

            var result = new SavedRandomEncounterUiState();
            Stub.TODO();
            return result;
        }
    }
}