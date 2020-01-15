using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using OpenTemple.Core.IO.SaveGames.GameState;

namespace OpenTemple.Core.IO.SaveGames.MapState
{
    /// <summary>
    /// Time events saved on a per-map basis in TimeEvent.dat
    /// </summary>
    public class MapTimeEventState
    {
        public List<SavedTimeEvent> Events { get; set; }

        public static MapTimeEventState Load(string path)
        {
            MapTimeEventState savedEvents;
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                savedEvents = Load(reader);
            }

            return savedEvents;
        }

        public static MapTimeEventState Load(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var events = new List<SavedTimeEvent>(count);
            for (var i = 0; i < count; i++)
            {
                events.Add(SavedTimeEvent.Load(reader));
            }

            return new MapTimeEventState {Events = events};
        }

        [SuppressMessage("ReSharper", "RedundantCast")]
        public static void Save(BinaryWriter writer, MapTimeEventState state)
        {
            writer.WriteInt32( state.Events.Count);
            foreach (var timeEvent in state.Events)
            {
                SavedTimeEvent.Save(writer, timeEvent);
            }
        }
    }
}