using System.Collections.Generic;
using System.IO;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.IO.SaveGames.GameState
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