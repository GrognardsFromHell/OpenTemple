using System;
using System.Collections.Generic;
using System.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedTimeEventState
    {
        public GameTime RealTime { get; set; }

        public GameTime GameTime { get; set; }

        public GameTime AnimTime { get; set; }

        public List<SavedTimeEvent> RealTimeEvents { get; set; }

        public List<SavedTimeEvent> GameTimeEvents { get; set; }

        public List<SavedTimeEvent> AnimTimeEvents { get; set; }

        [TempleDllLocation(0x10061f90)]
        public static SavedTimeEventState Read(BinaryReader reader)
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

            if (!TimeEventTypes.TryGetValue(typeCode, out var saveSpec))
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

        private static readonly Dictionary<int, TimeEventTypeSaveSpec> TimeEventTypes =
            new Dictionary<int, TimeEventTypeSaveSpec>
            {
                {
                    0, new TimeEventTypeSaveSpec(
                        0,
                        TimeEventType.Debug,
                        false,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int
                    )
                },
                {
                    1, new TimeEventTypeSaveSpec(
                        1,
                        TimeEventType.Anim,
                        true,
                        TimeEventArgType.Int
                    )
                },
                {
                    2, new TimeEventTypeSaveSpec(
                        2,
                        TimeEventType.BkgAnim,
                        false,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int
                    )
                },
                {
                    3, new TimeEventTypeSaveSpec(
                        3,
                        TimeEventType.FidgetAnim,
                        false
                    )
                },
                {
                    4, new TimeEventTypeSaveSpec(
                        4,
                        TimeEventType.Script,
                        true,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int,
                        TimeEventArgType.Object,
                        TimeEventArgType.Object
                    )
                },
                {
                    5, new TimeEventTypeSaveSpec(
                        5,
                        TimeEventType.PythonScript,
                        true,
                        TimeEventArgType.PythonObject,
                        TimeEventArgType.PythonObject
                    )
                },
                {
                    6, new TimeEventTypeSaveSpec(
                        6,
                        TimeEventType.Poison,
                        true,
                        TimeEventArgType.Int,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    7, new TimeEventTypeSaveSpec(
                        7,
                        TimeEventType.NormalHealing,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    8, new TimeEventTypeSaveSpec(
                        8,
                        TimeEventType.SubdualHealing,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    9, new TimeEventTypeSaveSpec(
                        9,
                        TimeEventType.Aging,
                        true
                    )
                },
                {
                    10, new TimeEventTypeSaveSpec(
                        10,
                        TimeEventType.AI,
                        false,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    11, new TimeEventTypeSaveSpec(
                        11,
                        TimeEventType.AIDelay,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    12, new TimeEventTypeSaveSpec(
                        12,
                        TimeEventType.Combat,
                        true
                    )
                },
                {
                    13, new TimeEventTypeSaveSpec(
                        13,
                        TimeEventType.TBCombat,
                        true
                    )
                },
                {
                    14, new TimeEventTypeSaveSpec(
                        14,
                        TimeEventType.AmbientLighting,
                        true
                    )
                },
                {
                    15, new TimeEventTypeSaveSpec(
                        15,
                        TimeEventType.WorldMap,
                        true
                    )
                },
                {
                    16, new TimeEventTypeSaveSpec(
                        16,
                        TimeEventType.Sleeping,
                        false,
                        TimeEventArgType.Int
                    )
                },
                {
                    17, new TimeEventTypeSaveSpec(
                        17,
                        TimeEventType.Clock,
                        true
                    )
                },
                {
                    18, new TimeEventTypeSaveSpec(
                        18,
                        TimeEventType.NPCWaitHere,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    19, new TimeEventTypeSaveSpec(
                        19,
                        TimeEventType.MainMenu,
                        false,
                        TimeEventArgType.Int
                    )
                },
                {
                    20, new TimeEventTypeSaveSpec(
                        20,
                        TimeEventType.Light,
                        false,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int
                    )
                },
                {
                    21, new TimeEventTypeSaveSpec(
                        21,
                        TimeEventType.Lock,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    22, new TimeEventTypeSaveSpec(
                        22,
                        TimeEventType.NPCRespawn,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    23, new TimeEventTypeSaveSpec(
                        23,
                        TimeEventType.DecayDeadBodies,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    24, new TimeEventTypeSaveSpec(
                        24,
                        TimeEventType.ItemDecay,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    25, new TimeEventTypeSaveSpec(
                        25,
                        TimeEventType.CombatFocusWipe,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    26, new TimeEventTypeSaveSpec(
                        26,
                        TimeEventType.Fade,
                        true,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int,
                        TimeEventArgType.Float,
                        TimeEventArgType.Int
                    )
                },
                {
                    27, new TimeEventTypeSaveSpec(
                        27,
                        TimeEventType.GFadeControl,
                        true
                    )
                },
                {
                    28, new TimeEventTypeSaveSpec(
                        28,
                        TimeEventType.Teleported,
                        false,
                        TimeEventArgType.Object
                    )
                },
                {
                    29, new TimeEventTypeSaveSpec(
                        29,
                        TimeEventType.SceneryRespawn,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    30, new TimeEventTypeSaveSpec(
                        30,
                        TimeEventType.RandomEncounters,
                        true
                    )
                },
                {
                    31, new TimeEventTypeSaveSpec(
                        31,
                        TimeEventType.ObjFade,
                        true,
                        TimeEventArgType.Int,
                        TimeEventArgType.Object
                    )
                },
                {
                    32, new TimeEventTypeSaveSpec(
                        32,
                        TimeEventType.ActionQueue,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    33, new TimeEventTypeSaveSpec(
                        33,
                        TimeEventType.Search,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    34, new TimeEventTypeSaveSpec(
                        34,
                        TimeEventType.IntgameTurnbased,
                        false,
                        TimeEventArgType.Int,
                        TimeEventArgType.Int
                    )
                },
                {
                    35, new TimeEventTypeSaveSpec(
                        35,
                        TimeEventType.PythonDialog,
                        true,
                        TimeEventArgType.Object,
                        TimeEventArgType.Object,
                        TimeEventArgType.Int
                    )
                },
                {
                    36, new TimeEventTypeSaveSpec(
                        36,
                        TimeEventType.EncumberedComplain,
                        true,
                        TimeEventArgType.Object
                    )
                },
                {
                    37, new TimeEventTypeSaveSpec(
                        37,
                        TimeEventType.PythonRealtime,
                        true,
                        TimeEventArgType.PythonObject,
                        TimeEventArgType.PythonObject
                    )
                }
            };
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
}