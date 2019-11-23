using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedRandomEncounterState
    {
        [TempleDllLocation(0x100458c0)]
        public static SavedRandomEncounterState Read(BinaryReader reader)
        {
            var result = new SavedRandomEncounterState();
            Stub.TODO();
            return result;
        }
    }
}