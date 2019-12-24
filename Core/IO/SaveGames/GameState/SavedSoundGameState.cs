using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedSoundGameState
    {

        public (int, int) CurrentSchemeIds { get; set; }

        /// <summary>
        /// This is triggered by the /OVER flag for sound schemes and uses the two stashed sound scheme ids below.
        /// </summary>
        public bool IsOneShotScheme { get; set; }

        // Supressed by the one shot scheme
        public (int, int) SchemesSuppressedByOneShot { get; set; }

        public bool IsCombatMusicPlaying { get; set; }

        public (int, int) SchemesSuppressedByCombatMusic { get; set; }

        [TempleDllLocation(0x1003cb70)]
        public static SavedSoundGameState Read(BinaryReader reader)
        {
            var result = new SavedSoundGameState();

            // Currently active scheme ids
            result.CurrentSchemeIds = (reader.ReadInt32(), reader.ReadInt32());

            result.IsOneShotScheme = reader.ReadInt32() != 0;
            result.SchemesSuppressedByOneShot = (reader.ReadInt32(), reader.ReadInt32());

            result.IsCombatMusicPlaying = reader.ReadInt32() != 0;
            result.SchemesSuppressedByCombatMusic = (reader.ReadInt32(), reader.ReadInt32());

            return result;
        }
    }
}