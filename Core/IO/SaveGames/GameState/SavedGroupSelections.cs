using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedGroupSelections
    {
        // Key is the number on the keyboard (0-9), value is the critters in the respective group
        public Dictionary<int, ObjectId[]> SavedGroups { get; set; }

        [TempleDllLocation(0x1002ad80)]
        public static SavedGroupSelections Load(byte[] buffer)
        {
            // 32 object ids + count for 10 groups
            var expectedSize = (32 * 24 + 4) * 10;
            if (buffer.Length != expectedSize)
            {
                throw new CorruptSaveException(
                    $"Invalid partyconfig.bin size {buffer.Length}. Expected: {expectedSize}.");
            }

            var result = new SavedGroupSelections();

            using var reader = new BinaryReader(new MemoryStream(buffer));

            Span<int> groupLengths = stackalloc int[10];
            reader.Read(MemoryMarshal.Cast<int, byte>(groupLengths));

            for (var groupId = 0; groupId < 10; groupId++)
            {
                var ids = new ObjectId[groupLengths[groupId]];
                for (var i = 0; i < 32; i++)
                {
                    if (i < ids.Length)
                    {
                        ids[i] = reader.ReadObjectId();
                    }
                    else
                    {
                        reader.ReadObjectId();
                    }
                }

                if (ids.Length > 0)
                {
                    result.SavedGroups[groupId] = ids;
                }
            }

            return result;
        }

        [TempleDllLocation(0x1002ac70)]
        public void Save(BinaryWriter writer)
        {
            // 32 object ids + count for 10 groups

            for (var i = 0; i < 10; i++)
            {
                if (SavedGroups.TryGetValue(i, out var savedGroup))
                {
                    writer.WriteInt32(savedGroup.Length);
                }
                else
                {
                    writer.WriteInt32(0);
                }
            }

            for (var i = 0; i < 10; i++)
            {
                if (SavedGroups.TryGetValue(i, out var savedGroup))
                {
                    for (var j = 0; j < 32; j++)
                    {
                        if (j < savedGroup.Length)
                        {
                            writer.WriteObjectId(savedGroup[j]);
                        }
                        else
                        {
                            writer.WriteObjectId(ObjectId.CreateNull());
                        }
                    }
                }
                else
                {
                    for (var j = 0; j < 32; j++)
                    {
                        writer.WriteObjectId(ObjectId.CreateNull());
                    }
                }
            }
        }
    }
}