using System;
using System.Collections.Generic;
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