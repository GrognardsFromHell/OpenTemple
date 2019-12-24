using System.IO;

namespace OpenTemple.Core.IO.SaveGames.UiState
{
    public class SavedCampingUiState
    {
        public int CampingType { get; set; }

        public int DaysToRest { get; set; }

        public int HoursToRest { get; set; }

        public bool DontAskToPassTime { get; set; }

        [TempleDllLocation(0x1012e3b0)]
        public static SavedCampingUiState Read(BinaryReader reader)
        {
            var result = new SavedCampingUiState();
            result.CampingType = reader.ReadInt32();
            result.DaysToRest = reader.ReadInt32();
            result.HoursToRest = reader.ReadInt32();
            result.DontAskToPassTime = reader.ReadInt32() == 0;
            return result;
        }
    }
}