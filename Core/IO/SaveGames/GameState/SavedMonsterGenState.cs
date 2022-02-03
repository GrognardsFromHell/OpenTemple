using System;
using System.Collections.Generic;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState;

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

    public void Write(BinaryWriter writer)
    {
        Span<byte> packedState = stackalloc byte[256];

        foreach (var spawner in Spawners)
        {
            if (spawner.Id >= packedState.Length)
            {
                throw new CorruptSaveException($"Cannot save spawner state for id {spawner.Id}");
            }

            var state = (byte) (spawner.CurrentlySpawned & 0x1F);
            if (spawner.IsDisabled)
            {
                state |= 0x80;
            }
            packedState[spawner.Id] = state;
        }

        writer.Write(packedState);
    }
}

public class SavedSpawnerState
{
    public int Id { get; set; }

    public bool IsDisabled { get; set; }

    public int CurrentlySpawned { get; set; }
}