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

        [TempleDllLocation(0x1012e330)]
        public void Write(BinaryWriter writer)
        {
             writer.WriteInt32(CampingType);
             writer.WriteInt32(DaysToRest);
             writer.WriteInt32(HoursToRest);
             // Note that this is flipped in meaning vs ToEE
             writer.WriteInt32(DontAskToPassTime ? 0 : 1);
        }
    }
}