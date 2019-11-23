using System.IO;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedMapFleeState
    {
        public int MapId { get; set; }
        public LocAndOffsets Location { get; set; }
        public int EnterX { get; set; }
        public int EnterY { get; set; }
        public bool IsFleeing { get; set; }

        public static SavedMapFleeState Load(byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));

            var result = new SavedMapFleeState();
            result.MapId = reader.ReadInt32();
            reader.ReadInt32(); // Padding
            result.Location = reader.ReadLocationAndOffsets();
            result.EnterX = reader.ReadInt32();
            result.EnterY = reader.ReadInt32();
            result.IsFleeing = reader.ReadInt32() != 0;
            return result;
        }
    }
}