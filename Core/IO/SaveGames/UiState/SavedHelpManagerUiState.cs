using System.IO;

namespace OpenTemple.Core.IO.SaveGames.UiState
{
    public class SavedHelpManagerUiState
    {
        public bool ClickForHelpActive { get; set; }

        public bool TutorialActive { get; set; }

        [TempleDllLocation(0x101248b0)]
        public static SavedHelpManagerUiState Read(BinaryReader reader)
        {
            var result = new SavedHelpManagerUiState();
            result.ClickForHelpActive = reader.ReadInt32() != 0;
            result.TutorialActive = reader.ReadInt32() != 0;
            return result;
        }
    }
}