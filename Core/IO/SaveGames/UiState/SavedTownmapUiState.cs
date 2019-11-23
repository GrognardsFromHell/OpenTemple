using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SpicyTemple.Core.IO.SaveGames.UiState
{
    public class SavedTownmapUiState
    {
        public bool IsAvailable { get; set; }

        public Dictionary<int, SavedTownmapMapState> Maps { get; set; } = new Dictionary<int, SavedTownmapMapState>();

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

                var mapData = new SavedTownmapMapState();
                mapData.MapId = 4999 + mapIndex;
                var flagCount = flagCountPerMap[mapIndex];
                mapData.Markers.Capacity = flagCount;
                for (var j = 0; j < flagCount; j++)
                {
                    mapData.Markers.Add(SavedTownmapMarker.Read(reader));
                }

                result.Maps[mapData.MapId] = mapData;
            }

            // This data was reset when the townmap was opened for the first time after starting a game
            // Keeping it around is not worth it. it's just the last centered location + zoom level.
            Span<Vector3> mapStartingPos = stackalloc Vector3[200];
            reader.Read(MemoryMarshal.Cast<Vector3, byte>(mapStartingPos));

            return result;
        }
    }


    public class SavedTownmapMapState
    {
        public int MapId { get; set; }

        public List<SavedTownmapMarker> Markers { get; set; } = new List<SavedTownmapMarker>();
    }

    public class SavedTownmapMarker
    {
        public int CurrentIndex { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public SavedTownmapMarkerStatus Status { get; set; }

        public string Text { get; set; }

        public static SavedTownmapMarker Read(BinaryReader reader)
        {
            var result = new SavedTownmapMarker();
            result.CurrentIndex = reader.ReadInt32();
            result.X = reader.ReadInt32();
            result.Y = reader.ReadInt32();
            result.Status = (SavedTownmapMarkerStatus) reader.ReadInt32();
            reader.ReadInt32(); // Skip stale text pointer
            result.Text = reader.ReadFixedString(260);
            return result;
        }
    }

    public enum SavedTownmapMarkerStatus
    {
        Predefined = 0,
        Revealed = 1,
        Unknown = 2,
        CustomMarker = 3
    }
}