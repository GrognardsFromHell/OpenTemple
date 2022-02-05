using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.TimeEvents;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedTimeEventState
{
    public GameTime RealTime { get; set; }

    public GameTime GameTime { get; set; }

    public GameTime AnimTime { get; set; }

    public List<SavedTimeEvent> RealTimeEvents { get; set; }

    public List<SavedTimeEvent> GameTimeEvents { get; set; }

    public List<SavedTimeEvent> AnimTimeEvents { get; set; }

    [TempleDllLocation(0x10061f90)]
    public static SavedTimeEventState Load(BinaryReader reader)
    {
        var result = new SavedTimeEventState();
        result.RealTime = reader.ReadGameTime();
        result.GameTime = reader.ReadGameTime();
        result.AnimTime = reader.ReadGameTime();

        // Read time events for all clock types separately
        result.RealTimeEvents = ReadTimeEvents(reader);
        result.GameTimeEvents = ReadTimeEvents(reader);
        result.AnimTimeEvents = ReadTimeEvents(reader);

        return result;
    }

    [TempleDllLocation(0x10061840)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteGameTime(RealTime);
        writer.WriteGameTime(GameTime);
        writer.WriteGameTime(AnimTime);

        // Save time events for all clock types separately
        WriteTimeEvents(writer, RealTimeEvents);
        WriteTimeEvents(writer, GameTimeEvents);
        WriteTimeEvents(writer, AnimTimeEvents);
    }

    private static List<SavedTimeEvent> ReadTimeEvents(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var result = new List<SavedTimeEvent>(count);
        for (var i = 0; i < count; i++)
        {
            result.Add(SavedTimeEvent.Load(reader));
        }

        return result;
    }

    private static void WriteTimeEvents(BinaryWriter writer, List<SavedTimeEvent> events)
    {
        writer.WriteInt32(events.Count);
        foreach (var timeEvent in events)
        {
            SavedTimeEvent.Save(writer, timeEvent);
        }
    }

}

public class SavedTimeEvent
{
    public GameTime ExpiresAt { get; set; }

    public TimeEventType Type { get; set; }

    /// <summary>
    /// Each argument of the time event as a pair of it's type and the value.
    /// </summary>
    public (TimeEventArgType, object)[] Args { get; set; }

    [TempleDllLocation(0x100602f0)]
    public static SavedTimeEvent Load(BinaryReader reader)
    {
        var result = new SavedTimeEvent();
        result.ExpiresAt = reader.ReadGameTime();
        var typeCode = reader.ReadInt32();

        if (!TimeEventSaveSpecs.SpecByCode.TryGetValue(typeCode, out var saveSpec))
        {
            throw new CorruptSaveException($"Found time event of type {typeCode}, which is unknown.");
        }

        result.Type = saveSpec.System;
        result.Args = new (TimeEventArgType, object)[saveSpec.Args.Length];
        for (var i = 0; i < result.Args.Length; i++)
        {
            var argType = saveSpec.Args[i];
            result.Args[i] = (argType, LoadArg(reader, argType));
        }

        return result;
    }

    [TempleDllLocation(0x10060230)]
    public static void Save(BinaryWriter writer, SavedTimeEvent timeEvent)
    {
        if (!TimeEventSaveSpecs.SpecByType.TryGetValue(timeEvent.Type, out var saveSpec))
        {
            throw new CorruptSaveException($"Found time event of type {timeEvent.Type}, which is unknown.");
        }

        writer.WriteGameTime(timeEvent.ExpiresAt);
        writer.Write(saveSpec.Code);
        if (timeEvent.Args.Length != saveSpec.Args.Length)
        {
            throw new CorruptSaveException($"Time Event for system {timeEvent.Type} has {timeEvent.Args.Length}, " +
                                           $"but it should have {saveSpec.Args.Length}");
        }

        for (var i = 0; i < timeEvent.Args.Length; i++)
        {
            var (argType, argValue) = timeEvent.Args[i];
            if (argType != saveSpec.Args[i])
            {
                throw new CorruptSaveException(
                    $"Time Event for system {timeEvent.Type} has argument #{i} of type {argType}, " +
                    $"but it should have {saveSpec.Args[i]}");
            }

            SaveArg(writer, argType, argValue);
        }
    }

    private static object LoadArg(BinaryReader reader, TimeEventArgType type)
    {
        switch (type)
        {
            case TimeEventArgType.Int:
                return reader.ReadInt32();
            case TimeEventArgType.Float:
                return reader.ReadSingle();
            case TimeEventArgType.Object:
                return reader.ReadFrozenObjRef();
//                case TimeEventArgType.PythonObject:
//                    break; TODO
            case TimeEventArgType.Location:
                // TODO: Note that vanilla only saves locx,locy, but we now allow for precise locs
                return new LocAndOffsets(reader.ReadTileLocation());
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static void SaveArg(BinaryWriter writer, TimeEventArgType type, object value)
    {
        switch (type)
        {
            case TimeEventArgType.Int:
                writer.WriteInt32((int) value);
                break;
            case TimeEventArgType.Float:
                writer.Write((float) value);
                break;
            case TimeEventArgType.Object:
                writer.WriteFrozenObjRef((FrozenObjRef) value);
                break;
//                case TimeEventArgType.PythonObject:
//                    break; TODO
            case TimeEventArgType.Location:
                // TODO: Note that vanilla only saves locx,locy, but we now allow for precise locs
                writer.WriteTileLocation(((LocAndOffsets) value).location);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}

/// <summary>
/// Descriptors that are used to decide how to serialize time events of specific types.
/// </summary>
public static class TimeEventSaveSpecs
{
    private static readonly TimeEventTypeSaveSpec[] Specs =
    {
        new(
            0,
            TimeEventType.Debug,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),
        new(
            1,
            TimeEventType.Anim,
            true,
            TimeEventArgType.Int
        ),
        new(
            2,
            TimeEventType.BkgAnim,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),
        new(
            3,
            TimeEventType.FidgetAnim,
            false
        ),
        new(
            4,
            TimeEventType.Script,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Int,
            TimeEventArgType.Object,
            TimeEventArgType.Object
        ),
        new(
            5,
            TimeEventType.PythonScript,
            true,
            TimeEventArgType.PythonObject,
            TimeEventArgType.PythonObject
        ),
        new(
            6,
            TimeEventType.Poison,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            7,
            TimeEventType.NormalHealing,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            8,
            TimeEventType.SubdualHealing,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            9,
            TimeEventType.Aging,
            true
        ),
        new(
            10,
            TimeEventType.AI,
            false,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            11,
            TimeEventType.AIDelay,
            true,
            TimeEventArgType.Object
        ),
        new(
            12,
            TimeEventType.Combat,
            true
        ),
        new(
            13,
            TimeEventType.TBCombat,
            true
        ),
        new(
            14,
            TimeEventType.AmbientLighting,
            true
        ),
        new(
            15,
            TimeEventType.WorldMap,
            true
        ),
        new(
            16,
            TimeEventType.Sleeping,
            false,
            TimeEventArgType.Int
        ),
        new(
            17,
            TimeEventType.Clock,
            true
        ),
        new(
            18,
            TimeEventType.NPCWaitHere,
            true,
            TimeEventArgType.Object
        ),
        new(
            19,
            TimeEventType.MainMenu,
            false,
            TimeEventArgType.Int
        ),
        new(
            20,
            TimeEventType.Light,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),
        new(
            21,
            TimeEventType.Lock,
            true,
            TimeEventArgType.Object
        ),
        new(
            22,
            TimeEventType.NPCRespawn,
            true,
            TimeEventArgType.Object
        ),
        new(
            23,
            TimeEventType.DecayDeadBodies,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            24,
            TimeEventType.ItemDecay,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            25,
            TimeEventType.CombatFocusWipe,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            26,
            TimeEventType.Fade,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Int,
            TimeEventArgType.Float,
            TimeEventArgType.Int
        ),
        new(
            27,
            TimeEventType.GFadeControl,
            true
        ),
        new(
            28,
            TimeEventType.Teleported,
            false,
            TimeEventArgType.Object
        ),
        new(
            29,
            TimeEventType.SceneryRespawn,
            true,
            TimeEventArgType.Object
        ),
        new(
            30,
            TimeEventType.RandomEncounters,
            true
        ),
        new(
            31,
            TimeEventType.ObjFade,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Object
        ),
        new(
            32,
            TimeEventType.ActionQueue,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            33,
            TimeEventType.Search,
            true,
            TimeEventArgType.Object
        ),
        new(
            34,
            TimeEventType.IntgameTurnbased,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),
        new(
            35,
            TimeEventType.PythonDialog,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),
        new(
            36,
            TimeEventType.EncumberedComplain,
            true,
            TimeEventArgType.Object
        ),
        new(
            37,
            TimeEventType.PythonRealtime,
            true,
            TimeEventArgType.PythonObject,
            TimeEventArgType.PythonObject
        )
    };

    public static readonly Dictionary<int, TimeEventTypeSaveSpec> SpecByCode = Specs
        .ToDictionary(
            spec => spec.Code,
            spec => spec
        );

    // Build a reverse index
    public static readonly Dictionary<TimeEventType, TimeEventTypeSaveSpec> SpecByType = Specs
        .ToDictionary(
            spec => spec.System,
            spec => spec
        );
}

public class TimeEventTypeSaveSpec
{
    public int Code { get; }

    public bool IncludeInSaveGame { get; set; }

    public TimeEventType System { get; }

    public TimeEventArgType[] Args { get; }

    public TimeEventTypeSaveSpec(int code, TimeEventType type, bool includeInSave, params TimeEventArgType[] args)
    {
        Code = code;
        System = type;
        IncludeInSaveGame = includeInSave;
        Args = args;
    }
}