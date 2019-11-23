using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedAnimState
    {
        public int NextUniqueId { get; set; }

        public int ActiveGoalCount { get; set; }

        // This is enabled if -animcatchup command line param was ever used
        public bool UseAbsoluteTime { get; set; }

        public int NextUniqueActionId { get; set; }

        public Dictionary<int, SavedAnimSlot> Slots { get; set; } = new Dictionary<int, SavedAnimSlot>();

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
    }

    public class SavedAnimSlot
    {
        public AnimSlotId Id { get; set; }

        public int CurrentState { get; set; }

        public int Field14 { get; set; }

        public FrozenObjRef AnimatedObject { get; set; }

        public List<SavedAnimGoal> Goals { get; set; } = new List<SavedAnimGoal>();

        public SavedAnimPath AnimPath { get; set; }

        public GameTime PathPauseTime { get; set; }

        public GameTime NextTriggerTime { get; set; }

        public GameTime GameTimeSth { get; set; }

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

            result.Field14 = reader.ReadInt32();
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

            result.PathPauseTime = reader.ReadGameTime();
            result.NextTriggerTime = reader.ReadGameTime();
            result.GameTimeSth = reader.ReadGameTime();
            result.CurrentPing = reader.ReadInt32();

            return result;
        }
    }

    public class SavedAnimPath
    {
        public int Flags { get; set; }

        public sbyte[] Deltas { get; set; }

        public int Range { get; set; }

        public int FieldD0 { get; set; }

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
            result.Flags = reader.ReadInt32();
            result.Deltas = new sbyte[200];
            reader.Read(MemoryMarshal.Cast<sbyte, byte>(result.Deltas));
            result.Range = reader.ReadInt32();
            result.FieldD0 = reader.ReadInt32();
            result.FieldD4 = reader.ReadInt32();
            result.DeltaIdxMax = reader.ReadInt32();
            result.FieldDC = reader.ReadInt32();
            result.MaxPathLength = reader.ReadInt32();
            result.FieldE4 = reader.ReadInt32();
            result.ObjectLoc = reader.ReadTileLocation();
            result.TargetLoc = reader.ReadTileLocation();
            return result;
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
    }
}