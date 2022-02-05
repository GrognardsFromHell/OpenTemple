using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.IO.SaveGames.GameState;

/// <summary>
/// This is the saved animation state for the _current_ map, which is distinct from the state saved
/// for maps when transitioning areas.
/// </summary>
public class SavedAnimState
{
    private const int MaxSlots = 512;

    public int NextUniqueId { get; set; }

    public int ActiveGoalCount { get; set; }

    // This is enabled if -animcatchup command line param was ever used
    public bool UseAbsoluteTime { get; set; }

    public int NextUniqueActionId { get; set; }

    public Dictionary<int, SavedAnimSlot> Slots { get; set; } = new();

    [TempleDllLocation(0x1001d250)]
    public static SavedAnimState Read(BinaryReader reader)
    {
        var result = new SavedAnimState();

        result.NextUniqueId = reader.ReadInt32();
        result.ActiveGoalCount = reader.ReadInt32();
        result.UseAbsoluteTime = reader.ReadInt32() != 0;
        result.NextUniqueActionId = reader.ReadInt32();
        // Skip 15 32-bit integers with unknown purpose
        reader.BaseStream.Seek(15 * 4, SeekOrigin.Current);

        var maxSlotCount = reader.ReadInt32();

        for (var i = 0; i < maxSlotCount;)
        {
            // ToEE will save a "skip-list" of sorts
            var chunkSize = reader.ReadInt32();

            if (chunkSize < 0)
            {
                // negative chunk size indicates a block of unused/free slots
                i += -chunkSize;
            }
            else if (chunkSize > 0)
            {
                for (var j = 0; j < chunkSize; j++)
                {
                    var slot = SavedAnimSlot.Load(reader);
                    if (slot.Id.slotIndex != i)
                    {
                        throw new CorruptSaveException($"Read slot index {slot.Id.slotIndex} from save " +
                                                       $"in position of slot {i}.");
                    }

                    result.Slots[i++] = slot;
                }
            }
            else
            {
                throw new CorruptSaveException("A zero-length chunk of anim slots is not allowed.");
            }
        }

        return result;
    }

    [TempleDllLocation(0x1001cab0)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(NextUniqueId);
        writer.WriteInt32(ActiveGoalCount);
        writer.WriteInt32(UseAbsoluteTime ? 1 : 0);
        writer.WriteInt32(NextUniqueActionId);

        // Write 15 32-bit integers with unknown purpose
        for (var i = 0; i < 15; i++)
        {
            writer.WriteInt32(0);
        }

        // ToEE uses a run-length encoding to batch together inactive slots
        // It's beyond me why they dont just write active ones, since they all include their IDs anyway...
        writer.WriteInt32(MaxSlots);
        for (var i = 0; i < MaxSlots;)
        {
            if (!Slots.ContainsKey(i))
            {
                WriteInactiveSpan(writer, ref i, MaxSlots);
            }
            else
            {
                WriteActiveSpan(writer, ref i, MaxSlots);
            }
        }
    }

    private void WriteInactiveSpan(BinaryWriter writer, ref int i, int maxSlots)
    {
        // Count the number of inactive slots and skips them
        var runLength = 0;
        while (i < maxSlots && !Slots.ContainsKey(i))
        {
            i++;
            runLength++;
        }

        writer.WriteInt32(-runLength);
    }

    private void WriteActiveSpan(BinaryWriter writer, ref int i, in int maxSlots)
    {
        // Count the number of active slots and skips them
        var runLength = 0;
        while (i + runLength < maxSlots && Slots.ContainsKey(i + runLength))
        {
            runLength++;
        }

        writer.WriteInt32(runLength);
        while (i < maxSlots && Slots.TryGetValue(i, out var slot))
        {
            if (slot.Id.slotIndex != i)
            {
                throw new CorruptSaveException($"Anim slot at index {i} has a mismatched id: {slot.Id}");
            }

            SavedAnimSlot.Save(writer, slot);
            i++;
        }
    }
}

public class SavedAnimSlot
{
    public AnimSlotId Id { get; set; }

    public int CurrentState { get; set; }

    public AnimSlotFlag Flags { get; set; }

    public int Field14 { get; set; }

    public FrozenObjRef AnimatedObject { get; set; }

    public List<SavedAnimGoal> Goals { get; set; } = new();

    public SavedAnimPath AnimPath { get; set; }

    public TimeSpan PathPauseTime { get; set; }

    public GameTime NextTriggerTime { get; set; }

    public TimePoint GameTimeSth { get; set; }

    public int CurrentPing { get; set; }

    [TempleDllLocation(0x1001ada0)]
    public static SavedAnimSlot Load(BinaryReader reader)
    {
        var result = new SavedAnimSlot();
        result.Id = new AnimSlotId
        {
            slotIndex = reader.ReadInt32(),
            uniqueId = reader.ReadInt32(),
            field_8 = reader.ReadInt32()
        };

        result.Flags = (AnimSlotFlag) reader.ReadInt32();
        result.CurrentState = reader.ReadInt32();
        result.Field14 = reader.ReadInt32();
        result.AnimatedObject = reader.ReadFrozenObjRef();
        var goalCount = reader.ReadInt32() + 1;
        result.Goals.Capacity = goalCount;
        for (var i = 0; i < goalCount; i++)
        {
            result.Goals.Add(SavedAnimGoal.Load(reader));
        }

        result.AnimPath = SavedAnimPath.Load(reader);

        var pauseTime = reader.ReadGameTime();
        result.PathPauseTime = new TimeSpan(pauseTime.timeInDays, 0, 0, 0, pauseTime.timeInMs);
        result.NextTriggerTime = reader.ReadGameTime();
        result.GameTimeSth = reader.ReadGameTime().ToTimePoint();
        result.CurrentPing = reader.ReadInt32();

        return result;
    }

    [SuppressMessage("ReSharper", "RedundantCast")]
    public static void Save(BinaryWriter writer, SavedAnimSlot slot)
    {
        writer.WriteInt32(slot.Id.slotIndex);
        writer.WriteInt32(slot.Id.uniqueId);
        writer.WriteInt32(slot.Id.field_8);

        writer.WriteInt32((int) slot.Flags);
        writer.WriteInt32(slot.CurrentState);
        writer.WriteInt32(slot.Field14);
        writer.WriteFrozenObjRef(slot.AnimatedObject);
        writer.WriteInt32((slot.Goals.Count - 1));
        foreach (var savedGoal in slot.Goals)
        {
            SavedAnimGoal.Save(writer, savedGoal);
        }

        SavedAnimPath.Save(writer, slot.AnimPath);
        writer.WriteGameTime(slot.PathPauseTime);
        writer.WriteGameTime(slot.NextTriggerTime);
        writer.WriteGameTime(slot.GameTimeSth);
        writer.WriteInt32(slot.CurrentPing);
    }
}

public class SavedAnimPath
{
    public AnimPathFlag Flags { get; set; }

    public sbyte[] Deltas { get; set; }

    public int Range { get; set; }

    public CompassDirection FieldD0 { get; set; }

    public int FieldD4 { get; set; }

    public int DeltaIdxMax { get; set; }

    public int FieldDC { get; set; }

    public int MaxPathLength { get; set; }

    public int FieldE4 { get; set; }

    public locXY ObjectLoc { get; set; }

    public locXY TargetLoc { get; set; }

    public static SavedAnimPath Load(BinaryReader reader)
    {
        var result = new SavedAnimPath();
        result.Flags = (AnimPathFlag) reader.ReadInt32();
        result.Deltas = new sbyte[200];
        reader.Read(MemoryMarshal.Cast<sbyte, byte>(result.Deltas));
        result.Range = reader.ReadInt32();
        result.FieldD0 = (CompassDirection) reader.ReadInt32();
        result.FieldD4 = reader.ReadInt32();
        result.DeltaIdxMax = reader.ReadInt32();
        result.FieldDC = reader.ReadInt32();
        result.MaxPathLength = reader.ReadInt32();
        result.FieldE4 = reader.ReadInt32();
        result.ObjectLoc = reader.ReadTileLocation();
        result.TargetLoc = reader.ReadTileLocation();
        return result;
    }

    [SuppressMessage("ReSharper", "RedundantCast")]
    public static void Save(BinaryWriter writer, SavedAnimPath path)
    {
        writer.WriteInt32((int) path.Flags);
        writer.Write(MemoryMarshal.Cast<sbyte, byte>(path.Deltas));
        writer.WriteInt32(path.Range);
        writer.WriteInt32((int) path.FieldD0);
        writer.WriteInt32(path.FieldD4);
        writer.WriteInt32(path.DeltaIdxMax);
        writer.WriteInt32(path.FieldDC);
        writer.WriteInt32(path.MaxPathLength);
        writer.WriteInt32(path.FieldE4);
        writer.WriteTileLocation(path.ObjectLoc);
        writer.WriteTileLocation(path.TargetLoc);
    }
}

public class SavedAnimGoal
{
    public AnimGoalType Type { get; set; }

    public FrozenObjRef Self { get; set; }
    public FrozenObjRef Target { get; set; }
    public FrozenObjRef Block { get; set; }
    public FrozenObjRef Scratch { get; set; }
    public FrozenObjRef Parent { get; set; }
    public locXY TargetTile { get; set; }
    public locXY Range { get; set; }
    public int AnimId { get; set; }
    public int AnimIdPrevious { get; set; }
    public int AnimData { get; set; }
    public int SpellData { get; set; }
    public int SkillData { get; set; }
    public int FlagsData { get; set; }
    public int ScratchVal1 { get; set; }
    public int ScratchVal2 { get; set; }
    public int ScratchVal3 { get; set; }
    public int ScratchVal4 { get; set; }
    public int ScratchVal5 { get; set; }
    public int ScratchVal6 { get; set; }
    public int SoundHandle { get; set; }
    public int SoundStreamId { get; set; }
    public int SoundStreamId2 { get; set; }

    [TempleDllLocation(0x10016e90)]
    public static SavedAnimGoal Load(BinaryReader reader)
    {
        var result = new SavedAnimGoal();
        var goalTypeId = reader.ReadInt32();
        // TODO: This should use a hard coded translation table to protect against changes in the enum ordinals
        result.Type = (AnimGoalType) goalTypeId;

        // Load goal parameters
        result.Self = reader.ReadFrozenObjRef();
        result.Target = reader.ReadFrozenObjRef();
        result.Block = reader.ReadFrozenObjRef();
        result.Scratch = reader.ReadFrozenObjRef();
        result.Parent = reader.ReadFrozenObjRef();
        result.TargetTile = reader.ReadTileLocation();
        result.Range = reader.ReadTileLocation();
        result.AnimId = reader.ReadInt32();
        result.AnimIdPrevious = reader.ReadInt32();
        result.AnimData = reader.ReadInt32();
        result.SpellData = reader.ReadInt32();
        result.SkillData = reader.ReadInt32();
        result.FlagsData = reader.ReadInt32();
        result.ScratchVal1 = reader.ReadInt32();
        result.ScratchVal2 = reader.ReadInt32();
        result.ScratchVal3 = reader.ReadInt32();
        result.ScratchVal4 = reader.ReadInt32();
        result.ScratchVal5 = reader.ReadInt32();
        result.ScratchVal6 = reader.ReadInt32();
        result.SoundHandle = reader.ReadInt32();
        // I believe one will always be written as -1, while the other is transient data
        result.SoundStreamId = reader.ReadInt32();
        result.SoundStreamId2 = reader.ReadInt32();

        return result;
    }

    [SuppressMessage("ReSharper", "RedundantCast")]
    public static void Save(BinaryWriter writer, SavedAnimGoal goal)
    {
        // TODO: This should use a hard coded translation table to protect against changes in the enum ordinals
        writer.WriteInt32((int) goal.Type);
        writer.WriteFrozenObjRef(goal.Self);
        writer.WriteFrozenObjRef(goal.Target);
        writer.WriteFrozenObjRef(goal.Block);
        writer.WriteFrozenObjRef(goal.Scratch);
        writer.WriteFrozenObjRef(goal.Parent);
        writer.WriteTileLocation(goal.TargetTile);
        writer.WriteTileLocation(goal.Range);
        writer.WriteInt32(goal.AnimId);
        writer.WriteInt32(goal.AnimIdPrevious);
        writer.WriteInt32(goal.AnimData);
        writer.WriteInt32(goal.SpellData);
        writer.WriteInt32(goal.SkillData);
        writer.WriteInt32(goal.FlagsData);
        writer.WriteInt32(goal.ScratchVal1);
        writer.WriteInt32(goal.ScratchVal2);
        writer.WriteInt32(goal.ScratchVal3);
        writer.WriteInt32(goal.ScratchVal4);
        writer.WriteInt32(goal.ScratchVal5);
        writer.WriteInt32(goal.ScratchVal6);
        writer.WriteInt32(goal.SoundHandle);
        // I believe one will always be written as -1, while the other is transient data
        writer.WriteInt32(goal.SoundStreamId);
        writer.WriteInt32(goal.SoundStreamId2);
    }
}