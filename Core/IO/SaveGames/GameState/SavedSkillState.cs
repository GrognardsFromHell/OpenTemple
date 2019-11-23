using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedSkillState
    {
        [TempleDllLocation(0x118CD578)]
        public int Unused { get; set; }

        [TempleDllLocation(0x1007d0e0)]
        public static SavedSkillState Read(BinaryReader reader)
        {
            var result = new SavedSkillState();
            result.Unused = reader.ReadInt32();
            return result;
        }
    }
}