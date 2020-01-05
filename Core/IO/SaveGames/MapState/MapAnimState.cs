using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.IO.SaveGames.GameState;

namespace OpenTemple.Core.IO.SaveGames.MapState
{
    /// <summary>
    /// This is the saved state for object animations when a map transition happens.
    /// When this state is loaded, it is deleted because it is usually loaded as the state of the current map,
    /// which is then saved to a different file later.
    /// </summary>
    public class MapAnimState
    {
        public List<SavedAnimSlot> Slots { get; set; } = new List<SavedAnimSlot>();

        public static MapAnimState Load(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            using var reader = new BinaryReader(stream);
            var result = Load(reader);

            if (stream.Position < stream.Length)
            {
                throw new CorruptSaveException($"Anim file {path} has trailing data at position {stream.Position}");
            }

            return result;
        }

        [TempleDllLocation(0x1001b5f0)]
        private static MapAnimState Load(BinaryReader reader)
        {
            var result = new MapAnimState();

            var slotCount = reader.ReadInt32();
            result.Slots.Capacity = slotCount;

            for (var i = 0; i < slotCount; i++)
            {
                result.Slots.Add(SavedAnimSlot.Load(reader));
            }

            return result;
        }

        public static void Save(string path, MapAnimState savedState)
        {
            using var writer = new BinaryWriter(new FileStream(path, FileMode.Create));
            writer.Write(savedState.Slots.Count);

            foreach (var savedSlot in savedState.Slots)
            {
                SavedAnimSlot.Save(writer, savedSlot);
            }
        }
    }
}