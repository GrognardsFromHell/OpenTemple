using System.Collections.Generic;
using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedSecretDoorState
    {
        public List<int> SeenSceneryNames { get; set; } = new List<int>();

        [TempleDllLocation(0x10046400)]
        public static SavedSecretDoorState Read(BinaryReader reader)
        {
            var knownCount = reader.ReadInt32();

            var result = new SavedSecretDoorState();
            result.SeenSceneryNames.Capacity = knownCount;
            for (var i = 0; i < knownCount; i++)
            {
                result.SeenSceneryNames.Add(reader.ReadInt32());
            }

            return result;
        }
    }
}