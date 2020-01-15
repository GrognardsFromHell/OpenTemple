using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedObjFadeState
    {
        public int NextObjectFadeId { get; set; }

        public Dictionary<int, SavedFadeSettings> ActiveFades { get; set; }

        [TempleDllLocation(0x1004c220)]
        public static SavedObjFadeState Read(BinaryReader reader)
        {
            var result = new SavedObjFadeState();
            result.ActiveFades = reader.ReadIndexTable<SavedFadeSettings>(0x18, ReadFade);
            result.NextObjectFadeId = reader.ReadInt32();
            return result;
        }

        [TempleDllLocation(0x1004c1c0)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteIndexTable(ActiveFades, 0x18, WriteFade);
            writer.WriteInt32(NextObjectFadeId);
        }

        private static void ReadFade(BinaryReader reader, out SavedFadeSettings item)
        {
            item = new SavedFadeSettings();
            item.Id = reader.ReadInt32();
            item.InitialOpacity = reader.ReadInt32();
            item.GoalOpacity = reader.ReadInt32();
            item.MillisPerTick = reader.ReadInt32();
            item.OpacityPerTick = reader.ReadInt32();
            item.FadeOutResult = (FadeOutResult) reader.ReadInt32();
        }

        private static void WriteFade(BinaryWriter writer, SavedFadeSettings item)
        {
            writer.WriteInt32(item.Id);
            writer.WriteInt32(item.InitialOpacity);
            writer.WriteInt32(item.GoalOpacity);
            writer.WriteInt32(item.MillisPerTick);
            writer.WriteInt32(item.OpacityPerTick);
            writer.WriteInt32((int) item.FadeOutResult);
        }

    }

    public class SavedFadeSettings
    {
        public int Id { get; set; }

        public int InitialOpacity { get; set; }

        public int GoalOpacity { get; set; }

        public int MillisPerTick { get; set; }

        public int OpacityPerTick { get; set; }

        public FadeOutResult FadeOutResult { get; set; }
    }
}