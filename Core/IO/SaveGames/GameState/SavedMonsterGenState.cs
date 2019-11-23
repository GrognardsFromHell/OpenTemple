using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedMonsterGenState
    {
        public byte[] State { get; set; }

        [TempleDllLocation(0x100501a0)]
        public static SavedMonsterGenState Read(BinaryReader reader)
        {
            var result = new SavedMonsterGenState();
            result.State = reader.ReadBytes(256);
            return result;
        }
    }
}