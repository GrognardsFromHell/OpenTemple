using System;
using System.Collections.Generic;
using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedMonsterGenState
    {
        public List<SavedSpawnerState> Spawners { get; set; } = new List<SavedSpawnerState>();

        [TempleDllLocation(0x100501a0)]
        public static SavedMonsterGenState Read(BinaryReader reader)
        {
            Span<byte> packedState = stackalloc byte[256];
            reader.Read(packedState);

            var result = new SavedMonsterGenState();
            for (var i = 0; i < packedState.Length; i++)
            {
                var disabled = (packedState[i] & 0x80) != 0;
                var currentlySpawned = packedState[i] & 0x1F;
                if (!disabled && currentlySpawned == 0)
                {
                    // Spawner is in default state
                    continue;
                }

                result.Spawners.Add(new SavedSpawnerState
                {
                    Id = i,
                    IsDisabled = disabled,
                    CurrentlySpawned = currentlySpawned
                });
            }

            return result;
        }
    }

    public class SavedSpawnerState
    {
        public int Id { get; set; }

        public bool IsDisabled { get; set; }

        public int CurrentlySpawned { get; set; }
    }
}