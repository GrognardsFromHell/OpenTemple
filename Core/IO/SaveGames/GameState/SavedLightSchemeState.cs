using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedLightSchemeState
    {
        public int LightSchemeId { get; set; }

        public int HourOfDay { get; set; }

        [TempleDllLocation(0x1006f470)]
        public static SavedLightSchemeState Read(BinaryReader reader)
        {
            var result = new SavedLightSchemeState();
            result.LightSchemeId = reader.ReadInt32();
            result.HourOfDay = reader.ReadInt32();
            return result;
        }

        [TempleDllLocation(0x1006ef90)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(LightSchemeId);
            writer.WriteInt32(HourOfDay);
        }
    }
}