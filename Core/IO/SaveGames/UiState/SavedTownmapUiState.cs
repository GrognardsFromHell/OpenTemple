using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui.TownMap;

namespace OpenTemple.Core.IO.SaveGames.UiState
{
    public class SavedTownmapUiState
    {
        public bool IsAvailable { get; set; }

        public ISet<PredefinedMarkerId> RevealedMapMarkers { get; set; } = new HashSet<PredefinedMarkerId>();

        public List<SavedUserMapMarker> UserMapMarkers { get; set; } = new List<SavedUserMapMarker>();

        [TempleDllLocation(0x101288f0)]
        public static SavedTownmapUiState Read(BinaryReader reader)
        {
            var result = new SavedTownmapUiState();
            result.IsAvailable = reader.ReadInt32() != 0;

            var mapsWithFlagData = reader.ReadInt32();

            Span<byte> flagCountPerMap = stackalloc byte[200];
            reader.Read(flagCountPerMap);

            for (var i = 0; i < mapsWithFlagData; i++)
            {
                var mapIndex = reader.ReadInt32();
                if (mapIndex < 0 || mapIndex >= 200)
                {
                    throw new CorruptSaveException("Townmap map index out of range: " + mapIndex);
                }

                var mapId = 4999 + mapIndex;
                var flagCount = flagCountPerMap[mapIndex];
                for (var j = 0; j < flagCount; j++)
                {
                    // "Current index", which is seemingly pointless if the flags indicate whether the flag is
                    // revealed or not.
                    reader.ReadInt32();
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var flags = reader.ReadInt32();
                    reader.ReadInt32(); // Skip stale text pointer
                    var text = reader.ReadFixedString(260);

                    // The x and y positions are actually already projected onto screen coordinates
                    // and need to be translated back into world coordinates
                    var position = UnprojectTownMapPosition(x, y);

                    // Flags indicate:
                    // 1: Revealed
                    // 2: User Marker
                    // Since unrevealed user markers are pointless, 3 is usually used for user markers

                    // Ignore unrevealed map markers
                    if ((flags & 1) == 0)
                    {
                        continue;
                    }

                    if ((flags & 2) == 0)
                    {
                        result.RevealedMapMarkers.Add(new PredefinedMarkerId(mapId, j));
                    }
                    else
                    {
                        result.UserMapMarkers.Add(new SavedUserMapMarker(
                            mapId,
                            position,
                            text
                        ));
                    }
                }
            }

            // This data was reset when the townmap was opened for the first time after starting a game
            // Keeping it around is not worth it. it's just the last centered location + zoom level.
            Span<Vector3> mapStartingPos = stackalloc Vector3[200];
            reader.Read(MemoryMarshal.Cast<Vector3, byte>(mapStartingPos));

            return result;
        }

        private static bool IsSupportedMapId(int mapId)
        {
            var index = mapId - 4999;
            return index >= 0 && index < 200;
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(IsAvailable ? 1 : 0);

            // Count how many maps overall have flags on them
            var mapsWithFlags = new SortedSet<int>();
            foreach (var vanillaMarker in VanillaMarkers)
            {
                mapsWithFlags.Add(vanillaMarker.MapId);
            }
            foreach (var mapMarkers in UserMapMarkers)
            {
                if (IsSupportedMapId(mapMarkers.MapId))
                {
                    mapsWithFlags.Add(mapMarkers.MapId);
                }
            }

            writer.WriteInt32(mapsWithFlags.Count);

            // This is the most retarded format for saving this.
            Span<byte> flagCountPerMap = stackalloc byte[200];
            foreach (var mapId in mapsWithFlags)
            {
                flagCountPerMap[mapId - 4999] = CountFlagsForMap(mapId);
            }
            writer.Write(flagCountPerMap);

            foreach (var mapId in mapsWithFlags)
            {
                var mapIndex = mapId - 4999;
                writer.WriteInt32(mapIndex);

                var writtenFlags = 0;

                foreach (var marker in VanillaMarkers)
                {
                    if (marker.MapId != mapId)
                    {
                        continue;
                    }

                    var markerId = writtenFlags++;
                    writer.WriteInt32(markerId);
                    var projectedPos = ProjectTownMapPosition(marker.X, marker.Y);
                    writer.WriteInt32(projectedPos.X);
                    writer.WriteInt32(projectedPos.Y);

                    var flags = 0;
                    if (RevealedMapMarkers.Contains(new PredefinedMarkerId(mapId, markerId)))
                    {
                        flags |= 1;
                    }

                    writer.WriteInt32(flags);
                    writer.WriteInt32(0); // Skip stale text pointer
                    writer.WriteFixedString(260, marker.Text);
                }

                foreach (var marker in UserMapMarkers)
                {
                    if (marker.MapId != mapId)
                    {
                        continue;
                    }
                    
                    var markerId = writtenFlags++;
                    writer.WriteInt32(markerId);
                    var projectedPos = ProjectTownMapPosition(marker.Position.locx, marker.Position.locy);
                    writer.WriteInt32(projectedPos.X);
                    writer.WriteInt32(projectedPos.Y);
                    writer.WriteInt32(3); // User marker AND revealed
                    writer.WriteInt32(0); // Skip stale text pointer
                    writer.WriteFixedString(260, marker.Text);
                }

                if (writtenFlags != flagCountPerMap[mapIndex])
                {
                    throw new CorruptSaveException("Failed to write correct number of markers");
                }
            }

            // This data was reset when the townmap was opened for the first time after starting a game
            // Keeping it around is not worth it. it's just the last centered location + zoom level.
            Span<Vector3> mapStartingPos = stackalloc Vector3[200];
            writer.Write(MemoryMarshal.Cast<Vector3, byte>(mapStartingPos));
        }

        private byte CountFlagsForMap(int mapId)
        {
            byte count = 0;
            foreach (var mapMarker in UserMapMarkers)
            {
                if (mapMarker.MapId == mapId)
                {
                    count++;
                }
            }
            foreach (var vanillaMarker in VanillaMarkers)
            {
                if (vanillaMarker.MapId == mapId)
                {
                    count++;
                }
            }
            return count;
        }

        // This reverses the projection found in Camera.TileToWorld
        private static locXY UnprojectTownMapPosition(int x, int y)
        {
            locXY position = default;
            x /= 20;
            y /= 14;
            position.locx = (x - y + 1) / -2;
            position.locy = y - position.locx;
            return position;
        }

        private static Point ProjectTownMapPosition(int x, int y)
        {
            return new Point(
                (y - x - 1) * 20,
                (y + x) * 14
            );
        }

        private readonly struct SerializedMapMarker
        {
            public readonly int MapId;
            public readonly int Index;
            public readonly int X;
            public readonly int Y;
            public readonly string Text;

            public SerializedMapMarker(int mapId, int index, int x, int y, string text)
            {
                MapId = mapId;
                Index = index;
                X = x;
                Y = y;
                Text = text;
            }
        }

        /// <summary>
        /// Since ToEE actually copies the marker into the save, while we only store the ID of the revealed marker,
        /// we have to replicate Vanilla's marker data here.
        /// </summary>
        private static readonly SerializedMapMarker[] VanillaMarkers =
        {
            new SerializedMapMarker(5050, 0, 416, 394, "This flag marks the spot of something uninteresting."),
            new SerializedMapMarker(5078, 0, 628, 470, "Alrrem's presumed location of Falrinth"),
            new SerializedMapMarker(5078, 1, 551, 489, "Hedrack's area of the disturbance"),
            new SerializedMapMarker(5078, 2, 580, 599, "Room of fungi"),
            new SerializedMapMarker(5078, 3, 520, 618, "Stairs to Greater Temple"),
            new SerializedMapMarker(5066, 0, 419, 373, "Wonnilon's hideout"),
            new SerializedMapMarker(5066, 1, 552, 392, "Spiral staircase to level 2"),
            new SerializedMapMarker(5066, 2, 564, 489, "Banquet hall"),
            new SerializedMapMarker(5066, 3, 553, 588, "Stairs up to Temple"),
            new SerializedMapMarker(5066, 4, 414, 588, "Stairs up to Temple"),
            new SerializedMapMarker(5067, 0, 416, 505, "Location of Alrrem's underpriests"),
            new SerializedMapMarker(5067, 1, 539, 573, "Location of the Water Temple"),
            new SerializedMapMarker(5067, 2, 578, 583, "Stairs to level 3"),
            new SerializedMapMarker(5067, 3, 433, 569, "Location of the Fire Temple"),
            new SerializedMapMarker(5067, 4, 487, 569, "Location of the Air Temple"),
            new SerializedMapMarker(5080, 0, 581, 426, "Deggum says something fun is here."),
            new SerializedMapMarker(5080, 1, 570, 612, "Kella said hill giants are here"),
            new SerializedMapMarker(5080, 2, 580, 580, "Kella indicated this room is full of ettins"),
            new SerializedMapMarker(5080, 3, 478, 446, "Kella marked this as the main temple"),
            new SerializedMapMarker(5080, 4, 592, 529, "Kella said that Commander Hedrack slept here"),
            new SerializedMapMarker(5080, 5, 387, 524, "Kella said a powerful wizard named Senshock was here"),
            new SerializedMapMarker(5080, 6, 421, 534, "Kella said troop commanders Barkinar and Deggum were here"),
            new SerializedMapMarker(5080, 7, 450, 554, "Kella indicated trolls in this room"),
            new SerializedMapMarker(5080, 8, 508, 554, "Kella indicated trolls in this room"),
            new SerializedMapMarker(5080, 9, 388, 576, "Kella indicated ogres in this room"),
            new SerializedMapMarker(5080, 10, 424, 592, "Kella said this was a commons room for temple troops"),
            new SerializedMapMarker(5005, 0, 475, 537, "Gnolls tell me the master is here."),
            new SerializedMapMarker(5011, 0, 496, 493, "Terjon is here."),
        };

    }

    public class SavedUserMapMarker
    {
        public int MapId { get; }

        public locXY Position { get; }

        public string Text { get; }

        public SavedUserMapMarker(int mapId, locXY position, string text)
        {
            MapId = mapId;
            Position = position;
            Text = text;
        }
    }
}