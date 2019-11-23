using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedCombatState
    {
        public bool InCombat { get; set; }

        [TempleDllLocation(0x10062470)]
        public static SavedCombatState Read(BinaryReader reader)
        {
            var result = new SavedCombatState();
            result.InCombat = reader.ReadInt32() != 0;
            return result;
        }
    }
}