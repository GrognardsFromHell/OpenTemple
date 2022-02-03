using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedScriptState
{
    public int[] GlobalVars { get; set; } = new int[2000];

    public uint[] GlobalFlags { get; set; } = new uint[100];

    public int StoryState { get; set; }

    public List<int> EncounterQueue { get; set; } = new List<int>();

    [TempleDllLocation(0x10006670)]
    public static SavedScriptState Read(BinaryReader reader)
    {
        var result = new SavedScriptState();

        var varsView = MemoryMarshal.Cast<int, byte>(result.GlobalVars);
        reader.Read(varsView);

        var flagsView = MemoryMarshal.Cast<uint, byte>(result.GlobalFlags);
        reader.Read(flagsView);

        result.StoryState = reader.ReadInt32();

        // Load encounter queue
        var encounterIdCount = reader.ReadInt32();
        result.EncounterQueue.Capacity = encounterIdCount;
        for (var i = 0; i < encounterIdCount; i++)
        {
            result.EncounterQueue.Add(reader.ReadInt32());
        }

        return result;
    }

    [TempleDllLocation(0x100066e0)]
    public void Write(BinaryWriter writer)
    {
        var varsView = MemoryMarshal.Cast<int, byte>(GlobalVars);
        writer.Write(varsView);

        var flagsView = MemoryMarshal.Cast<uint, byte>(GlobalFlags);
        writer.Write(flagsView);

        writer.WriteInt32(StoryState);

        // Load encounter queue
        writer.WriteInt32(EncounterQueue.Count);
        foreach (var encounterId in EncounterQueue)
        {
            writer.WriteInt32(encounterId);
        }
    }
}