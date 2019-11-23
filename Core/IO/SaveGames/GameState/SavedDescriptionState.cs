using System.Collections.Generic;
using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedDescriptionState
    {
        public List<string> CustomNames { get; set; } = new List<string>();

        [TempleDllLocation(0x100868b0)]
        public static SavedDescriptionState Read(BinaryReader reader)
        {
            var customNameCount = reader.ReadInt32();

            var result = new SavedDescriptionState();
            result.CustomNames.Capacity = customNameCount;
            for (var i = 0; i < customNameCount; i++)
            {
                result.CustomNames.Add(reader.ReadPrefixedString());
            }

            return result;
        }
    }
}