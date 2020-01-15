using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.IO.SaveGames.UiState
{
    public class SavedPartyPoolUiState
    {
        public HashSet<ObjectId> AlreadyBeenInParty { get; set; } = new HashSet<ObjectId>();

        [TempleDllLocation(0x10165da0)]
        public static SavedPartyPoolUiState Read(BinaryReader reader)
        {
            var result = new SavedPartyPoolUiState();

            var count = reader.ReadInt32();
            result.AlreadyBeenInParty.EnsureCapacity(count);
            for (var i = 0; i < count; ++i)
            {
                result.AlreadyBeenInParty.Add(reader.ReadObjectId());
            }

            return result;
        }

        [TempleDllLocation(0x10165d10)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(AlreadyBeenInParty.Count);
            foreach (var objectId in AlreadyBeenInParty)
            {
                writer.WriteObjectId(objectId);
            }
        }
    }
}