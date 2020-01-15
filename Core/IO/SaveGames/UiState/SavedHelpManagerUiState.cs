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

        [TempleDllLocation(0x10124880)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(ClickForHelpActive ? 1 : 0);
            writer.WriteInt32(TutorialActive ? 1 : 0);
        }
    }
}