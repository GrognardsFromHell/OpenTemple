using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.SaveGames.MapState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.TimeEvents
{
    public class TimeEventSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10AA83B8)]
        private GameTime _currentRealTime;

        [TempleDllLocation(0x10AA83C0)]
        private GameTime _currentGameTime;

        [TempleDllLocation(0x10AA83C8)]
        private GameTime _currentAnimTime;

        [TempleDllLocation(0x10AA83DC)]
        private bool _isAdvancingTime = false;

        [TempleDllLocation(0x10AA83E0)]
        private bool _isSpecialScheduling = false;

        private List<TimeEventListEntry> _eventQueueRealTime = new List<TimeEventListEntry>();

        private List<TimeEventListEntry> _eventQueueGameTime = new List<TimeEventListEntry>();

        private List<TimeEventListEntry> _eventQueueAnimTime = new List<TimeEventListEntry>();

        private List<TimeEventListEntry> _eventQueueRealTimeWhileAdvancing = new List<TimeEventListEntry>();

        private List<TimeEventListEntry> _eventQueueGameTimeWhileAdvancing = new List<TimeEventListEntry>();

        private List<TimeEventListEntry> _eventQueueAnimTimeWhileAdvancing = new List<TimeEventListEntry>();

        private const int SecondsPerDay = 24 * 60 * 60;

        [TempleDllLocation(0x102BDF88)]
        public int StartingYear { get; private set; }

        [TempleDllLocation(0x102BDF8C)]
        public int StartingHourOfDayInMs { get; private set; }

        public int StartingHourOfDay { get; private set; }

        [TempleDllLocation(0x102BDF90)]
        public int StartingDayOfYear { get; private set; }

        [TempleDllLocation(0x1005fde0)]
        public int CurrentDayOfYear => StartingDayOfYear + _currentGameTime.timeInDays;

        // Interesting calendar...
        public int CurrentYear => StartingYear + (StartingDayOfYear + _currentGameTime.timeInDays) / 364;

        [TempleDllLocation(0x1005ff70)]
        public int HourOfDay => _currentGameTime.timeInMs / 3600000 % 24;

        [TempleDllLocation(0x1005ff90)]
        public int MinuteOfHour => _currentGameTime.timeInMs / 60000 % 60;

        [TempleDllLocation(0x1005ffb0)]
        public int SecondOfMinute => _currentGameTime.timeInMs / 1000 % 60;

        [TempleDllLocation(0x1005fc90)]
        [TempleDllLocation(0x100612b0)]
        public TimePoint GameTime => _currentGameTime.ToTimePoint();

        public CampaignCalendar CampaignCalendar => CampaignCalendar.FromElapsedTime(GameTime, StartingYear,
            StartingDayOfYear, StartingHourOfDay);

        [TempleDllLocation(0x1005fc60)]
        public TimePoint AnimTime => _currentAnimTime.ToTimePoint();

        private static GameTime ToGameTime(TimePoint timePoint) => timePoint.ToGameTime();

        [TempleDllLocation(0x100600e0)]
        public bool IsDaytime => HourOfDay >= 6 && HourOfDay < 18;

        [TempleDllLocation(0x10aa73f8)]
        private readonly Dictionary<int, string> _calendarTranslations;

        // @H = hours (0-23)
        // @M = minutes (0-59)
        // @D = day the game
        // @W = day of the week (see lines 1100-1106)
        // @N = name of month / festival
        // @Y = numeric year
        // @@ = the @ symbol
        // English: @H:@M, Day @D (@W), @N, @Y CY
        [TempleDllLocation(0x10aa7408)]
        private readonly string _calendarDateTimeFormat;

        [TempleDllLocation(0x100616a0)]
        public TimeEventSystem()
        {
            _calendarTranslations = Tig.FS.ReadMesFile("mes/calendar.mes");
            _calendarDateTimeFormat = _calendarTranslations[1000];
        }

        [TempleDllLocation(0x10061820)]
        public void Dispose()
        {
            ClearEvents();
        }

        [TempleDllLocation(0x10061310)]
        public void FormatGameTime(TimePoint time, StringBuilder result)
        {
            var dayOfYear = ((int) (time.Seconds / SecondsPerDay) + StartingDayOfYear) % CampaignCalendar.DaysPerYear;
            var currentYear = StartingYear
                              + ((int) (time.Seconds / SecondsPerDay) + StartingDayOfYear) /
                              CampaignCalendar.DaysPerYear;

            for (var i = 0; i < _calendarDateTimeFormat.Length; i++)
            {
                var ch = _calendarDateTimeFormat[i];
                if (ch == '@' && i + 1 < _calendarDateTimeFormat.Length)
                {
                    i++; // Skip the @

                    switch (_calendarDateTimeFormat[i])
                    {
                        case '@':
                            result.Append('@');
                            break;

                        case 'H':
                            // Append hours
                            var hours = (int) (time.Seconds / 3600) % 24;
                            if (hours < 10)
                            {
                                result.Append('0');
                            }

                            result.Append(hours);
                            break;

                        case 'M':
                            var minutes = (int) (time.Seconds / 60) % 60;
                            if (minutes < 10)
                            {
                                result.Append('0');
                            }

                            result.Append(minutes);
                            break;

                        case 'D':
                            // NOTE that this is not actually the day of the year, but rather the day since the game
                            // has started.
                            result.Append((int) (time.Seconds / SecondsPerDay) + 1);
                            break;

                        case 'W':
                            var dayOfWeek = dayOfYear % 7;
                            result.Append(_calendarTranslations[1100 + dayOfWeek]);
                            break;

                        case 'N':
                            var weekOfYear = dayOfYear / 7 % 52;
                            result.Append(_calendarTranslations[1200 + weekOfYear]);
                            break;

                        case 'Y':
                            result.Append(currentYear);
                            break;
                    }
                }
                else
                {
                    result.Append(ch);
                }
            }
        }

        [TempleDllLocation(0x1005fdf0)]
        public int GetDayOfMonth(TimePoint time)
        {
            var gameTime = ToGameTime(time);
            return (StartingDayOfYear + gameTime.timeInDays) % 28 + 1;
        }

        public int GetMonthOfYear(TimePoint time)
        {
            var gameTime = ToGameTime(time);
            return (StartingDayOfYear + gameTime.timeInDays) / 28 % 13 + 1;
        }

        [TempleDllLocation(0x1005ffd0)]
        public string FormatTimeOfDay(TimePoint time)
        {
            var totalSeconds = (int) time.Seconds;
            var hours = (totalSeconds / 3600) % 24;
            var minutes = (totalSeconds / 60) % 60;
            var seconds = totalSeconds % 60;
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        [TempleDllLocation(0x10061840)]
        public void SaveGame(SavedGameState savedGameState)
        {
            Trace.Assert(_eventQueueAnimTimeWhileAdvancing.Count == 0);
            Trace.Assert(_eventQueueGameTimeWhileAdvancing.Count == 0);
            Trace.Assert(_eventQueueRealTimeWhileAdvancing.Count == 0);

            savedGameState.TimeEventState = new SavedTimeEventState
            {
                RealTime = _currentRealTime,
                GameTime = _currentGameTime,
                AnimTime = _currentAnimTime,
                RealTimeEvents = SavedTimeEvents(_eventQueueRealTime),
                GameTimeEvents = SavedTimeEvents(_eventQueueGameTime),
                AnimTimeEvents = SavedTimeEvents(_eventQueueAnimTime)
            };
        }

        private List<SavedTimeEvent> SavedTimeEvents(List<TimeEventListEntry> timeEvents)
        {
            var result = new List<SavedTimeEvent>(timeEvents.Count);
            foreach (var entry in timeEvents)
            {
                ref readonly var system = ref TimeEventTypeRegistry.Get(entry.evt.system);
                if (system.persistent)
                {
                    var savedEvent = new SavedTimeEvent
                    {
                        Type = entry.evt.system,
                        ExpiresAt = entry.evt.time
                    };

                    var savedArgs = new (TimeEventArgType, object)[system.argTypes.Length];
                    for (var i = 0; i < system.argTypes.Length; i++)
                    {
                        savedArgs[i] = SaveArg(entry, system.argTypes[i], i);
                    }

                    savedEvent.Args = savedArgs;

                    result.Add(savedEvent);
                }
            }

            return result;
        }

        private (TimeEventArgType, object) SaveArg(TimeEventListEntry entry, TimeEventArgType argType, int index)
        {
            ref var eventArg = ref entry.evt.GetArg(index);
            object savedValue = argType switch
            {
                // If the argument type is an object, use the originally saved object id / location, even though
                // this might actually be worse than just using the current handle, if the frozen obj ref is not
                // updated before being saved!
                // TODO: It's doubtful that Frozen refs actually guard against missing objects, replace with simple ObjectIds?
                TimeEventArgType.Object when eventArg.handle != null =>
                GameSystems.MapObject.CreateFrozenRef(eventArg.handle),
                TimeEventArgType.Object => entry.objects[index],
                TimeEventArgType.Int => eventArg.int32,
                TimeEventArgType.Float => eventArg.float32,
                TimeEventArgType.PythonObject => throw new NotSupportedException(),
                TimeEventArgType.Location => eventArg.location,
                _ => throw new ArgumentOutOfRangeException(nameof(argType), argType, null)
            };

            return (argType, savedValue);
        }

        [TempleDllLocation(0x10061f90)]
        public void LoadGame(SavedGameState savedGameState)
        {
            var timerState = savedGameState.TimeEventState;

            _currentRealTime = timerState.RealTime;
            _currentGameTime = timerState.GameTime;
            _currentAnimTime = timerState.AnimTime;

            GameSystems.Light.UpdateDaylight();
            GameSystems.Terrain.UpdateDayNight();

            foreach (var timeEvent in timerState.RealTimeEvents)
            {
                LoadTimeEvent(timeEvent);
            }

            foreach (var timeEvent in timerState.GameTimeEvents)
            {
                LoadTimeEvent(timeEvent);
            }

            foreach (var timeEvent in timerState.AnimTimeEvents)
            {
                LoadTimeEvent(timeEvent);
            }
        }

        private void LoadTimeEvent(SavedTimeEvent savedTimeEvent)
        {
            var timeEvent = new TimeEvent(savedTimeEvent.Type);

            for (var i = 0; i < savedTimeEvent.Args.Length; i++)
            {
                var (savedType, savedValue) = savedTimeEvent.Args[i];
                if (!LoadTimeEventArg(ref timeEvent.GetArg(i), savedType, savedValue))
                {
                    // It depends on the event type how we handle this
                    if (savedTimeEvent.Type == TimeEventType.CombatFocusWipe)
                    {
                        // CombatFocusWipe events are not cleared for objects that are being destroyed
                        // so especially during combat, they might be queued for critters that are later
                        // destroyed (i.e. summoned creatures).
                        Logger.Warn("Skipping {0} event because argument {1},{2} could not be restored",
                            savedTimeEvent.Type, savedType, savedValue);
                        return;
                    }
                }
            }

            ScheduleInternal(savedTimeEvent.ExpiresAt, timeEvent, out _);
        }

        private static SavedTimeEvent SaveTimeEvent(TimeEvent timeEvent)
        {
            var saveSpec = TimeEventSaveSpecs.SpecByType[timeEvent.system];
            var args = new (TimeEventArgType, object)[saveSpec.Args.Length];

            for (var i = 0; i < saveSpec.Args.Length; i++)
            {
                var arg = timeEvent.GetArg(i);
                var argType = saveSpec.Args[i];
                args[i] = (argType, SaveTimeEventArg(arg, argType));
            }

            return new SavedTimeEvent
            {
                Type = timeEvent.system,
                ExpiresAt = timeEvent.time,
                Args = args
            };
        }

        private bool LoadTimeEventArg(ref TimeEventArg arg, TimeEventArgType savedType, object savedValue)
        {
            switch (savedType)
            {
                case TimeEventArgType.Int:
                    arg.int32 = (int) savedValue;
                    break;
                case TimeEventArgType.Float:
                    arg.float32 = (float) savedValue;
                    break;
                case TimeEventArgType.Object:
                    if (GameSystems.MapObject.Unfreeze((FrozenObjRef) savedValue, out var handle))
                    {
                        arg.handle = handle;
                    }
                    else
                    {
                        return false;
                    }

                    break;
                case TimeEventArgType.PythonObject:
                    throw new NotImplementedException();
                case TimeEventArgType.Location:
                    arg.location = (LocAndOffsets) savedValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(savedType), savedType, null);
            }

            return true;
        }

        private static object SaveTimeEventArg(TimeEventArg arg, TimeEventArgType savedType)
        {
            switch (savedType)
            {
                case TimeEventArgType.Int:
                    return arg.int32;
                case TimeEventArgType.Float:
                    return arg.float32;
                case TimeEventArgType.Object:
                    return GameSystems.MapObject.CreateFrozenRef(arg.handle);
                case TimeEventArgType.PythonObject:
                    throw new NotImplementedException();
                case TimeEventArgType.Location:
                    return arg.location;
                default:
                    throw new ArgumentOutOfRangeException(nameof(savedType), savedType, null);
            }
        }

        [TempleDllLocation(0x100617a0)]
        public void Reset()
        {
            ClearEvents();

            _currentRealTime = new GameTime(0, 1);

            _currentGameTime = new GameTime(0, StartingHourOfDayInMs);
            if (StartingHourOfDayInMs == 0)
            {
                _currentGameTime.timeInMs = 0;
            }

            _currentAnimTime = _currentGameTime;

            GameSystems.Light.UpdateDaylight();
            GameSystems.Terrain.UpdateDayNight();

            _timePoint = TimePoint.Now;
        }

        // Trigger removal callbacks for all pending events, then clear the queues
        [TempleDllLocation(0x100608c0)]
        private void ClearEvents()
        {
            foreach (var clockType in ClockTypes)
            {
                foreach (var pendingEvent in GetEventQueue(clockType))
                {
                    ref readonly var system = ref TimeEventTypeRegistry.Get(pendingEvent.evt.system);
                    system.removedCallback?.Invoke(pendingEvent.evt);
                }

                foreach (var pendingEvent in GetEventQueueWhileAdvancing(clockType))
                {
                    ref readonly var system = ref TimeEventTypeRegistry.Get(pendingEvent.evt.system);
                    system.removedCallback?.Invoke(pendingEvent.evt);
                }

                GetEventQueue(clockType).Clear();
                GetEventQueueWhileAdvancing(clockType).Clear();
            }
        }

        [TempleDllLocation(0x10aa83d0)]
        private TimePoint _timePoint;

        [TempleDllLocation(0x10AA83D8)]
        private int timeAdvanceBlockerCount;

        private static readonly GameClockType[] ClockTypes =
        {
            GameClockType.RealTime,
            GameClockType.GameTime,
            GameClockType.GameTimeAnims
        };

        [TempleDllLocation(0x100620c0)]
        public void AdvanceTime(TimePoint time)
        {
            var timeDelta = time - _timePoint;
            _timePoint = time;

            // At most advance 250ms in one iteration
            if (timeDelta.TotalMilliseconds > 250)
            {
                timeDelta = TimeSpan.FromMilliseconds(250);
            }

            _isAdvancingTime = true;

            _currentRealTime.Add(timeDelta);
            if (timeAdvanceBlockerCount == 0)
            {
                if (!GameUiBridge.IsDialogOpen() && !GameSystems.Combat.IsCombatActive())
                {
                    _currentGameTime.Add(timeDelta);

                    GameSystems.Light.UpdateDaylight();
                    GameSystems.Terrain.UpdateDayNight();
                }

                _currentAnimTime.Add(timeDelta);
            }
            else if (GameUiBridge.IsDialogOpen())
            {
                _currentAnimTime.Add(timeDelta);
            }

            var iterations = 0;

            foreach (var clockType in ClockTypes)
            {
                var clockTime = GetCurrentTime(clockType);

                var eventQueue = GetEventQueue(clockType);

                while (eventQueue.Count > 0)
                {
                    var evt = eventQueue[0];

                    // Skip if event is still in the future
                    if (evt.evt.time.timeInDays > clockTime.timeInDays
                        || evt.evt.time.timeInDays == clockTime.timeInDays
                        && evt.evt.time.timeInMs > clockTime.timeInMs)
                    {
                        break;
                    }

                    // Remove the first event
                    eventQueue.RemoveAt(0);

                    ref readonly var sysSpec = ref TimeEventTypeRegistry.Get(evt.evt.system);
                    if (evt.IsValid(false))
                    {
                        sysSpec.expiredCallback(evt.evt);
                    }

                    sysSpec.removedCallback?.Invoke(evt.evt);

                    if (++iterations >= 500)
                    {
                        Logger.Warn("Reached max iterations in time event processing");
                        break;
                    }
                }
            }

            _isAdvancingTime = false;

            // Move any events that were queued while processing to the main queue
            foreach (var clockType in ClockTypes)
            {
                var queue = GetEventQueueWhileAdvancing(clockType);
                for (var i = queue.Count - 1; i >= 0; i--)
                {
                    var evt = queue[i];
                    queue.RemoveAt(i);

                    if (evt.ObjHandlesValid())
                    {
                        ScheduleInternal(evt.evt.time, evt.evt, out _);
                    }
                }
            }
        }

        [TempleDllLocation(0x10060c90)]
        [TempleDllLocation(0x10062390)]
        public void AddGameTime(TimeSpan timeToAdvance)
        {
            var rounds = (int) (timeToAdvance.TotalSeconds / 6);
            GameSystems.D20.ObjectRegistry.BeginRoundForAll(rounds);

            if (timeToAdvance.TotalMilliseconds <= 0)
            {
                timeToAdvance = TimeSpan.FromMilliseconds(1);
            }

            AdvancePartyTime(timeToAdvance);

            _currentGameTime.Add(timeToAdvance);

            GameSystems.Light.UpdateDaylight();
            GameSystems.Terrain.UpdateDayNight();

            _currentAnimTime.Add(timeToAdvance);
        }

        [TempleDllLocation(0x10061b10)]
        private void AdvancePartyTime(TimeSpan timeToAdvance)
        {
            // TODO: All of this is not very robust.
            var passedDays = false;
            var fullDays = (int) timeToAdvance.TotalDays;
            var fullMilliseconds = (int) (timeToAdvance.TotalMilliseconds - fullDays * 86400000);
            for (var i = 0; i < fullDays; i++)
            {
                passedDays = true;
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    partyMember.DispatchNewCalendarDay();
                }
            }

            var newDay = _currentGameTime.timeInDays + fullDays;
            var newMilliseconds = _currentGameTime.timeInMs + fullMilliseconds;
            if (newDay == _currentGameTime.timeInDays)
            {
                if (!passedDays && newMilliseconds >= 28800000)
                {
                    foreach (var partyMember in GameSystems.Party.PartyMembers)
                    {
                        partyMember.DispatchNewCalendarDay();
                    }
                }
            }
            else
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    partyMember.DispatchNewCalendarDay();
                }
            }
        }


        [TempleDllLocation(0x10060c00)]
        public bool IsScheduled(TimeEventType systemType)
        {
            var clockType = TimeEventTypeRegistry.Get(systemType).clock;

            return GetEventQueue(clockType).Any(e => e.evt.system == systemType)
                   || GetEventQueueWhileAdvancing(clockType).Any(e => e.evt.system == systemType);
        }

        [TempleDllLocation(0x10062310)]
        public bool Schedule(TimeEvent evt, int delayInMs, out GameTime triggerTimeOut)
        {
            GameTime delay = new GameTime(0, delayInMs);
            return Schedule(evt, delay, null, out triggerTimeOut);
        }

        public bool Schedule(TimeEvent evt, TimeSpan delay, out GameTime triggerTimeOut)
        {
            return Schedule(evt, (int) delay.TotalMilliseconds, out triggerTimeOut);
        }

        [TempleDllLocation(0x10062340)]
        public void ScheduleNow(TimeEvent evt)
        {
            var time = new GameTime(0, 1);

            _isSpecialScheduling = true;
            ScheduleInternal(time, evt, out _);
            _isSpecialScheduling = false;
        }

        public bool ScheduleAbsolute(TimeEvent evt, GameTime? baseTime, int delayInMs, out GameTime triggerTimeOut)
        {
            var delay = new GameTime(0, delayInMs);
            return Schedule(evt, delay, baseTime, out triggerTimeOut);
        }

        [TempleDllLocation(0x10060720)]
        private bool Schedule(TimeEvent evt, GameTime delay, GameTime? baseTime, out GameTime triggerTimeOut)
        {
            Trace.Assert(evt != null);

            GameTime time = default;
            var clockType = TimeEventTypeRegistry.Get(evt.system).clock;

            if (baseTime.HasValue && (baseTime.Value.timeInDays > 0 || baseTime.Value.timeInMs > 0))
            {
                time.timeInDays = baseTime.Value.timeInDays;
                time.timeInMs = baseTime.Value.timeInMs;
            }
            else
            {
                time = GetCurrentTime(clockType);
            }

            time.timeInDays += delay.timeInDays;
            time.timeInMs += delay.timeInMs;
            if (time.timeInMs >= 86400000)
            {
                // 1 or more days in msec
                time.timeInDays += time.timeInMs / 86400000;
                time.timeInMs %= 86400000;
            }

            return ScheduleInternal(time, evt, out triggerTimeOut);
        }

        private GameTime GetCurrentTime(GameClockType clockType)
        {
            switch (clockType)
            {
                case GameClockType.RealTime:
                    return _currentRealTime;
                case GameClockType.GameTime:
                    return _currentGameTime;
                case GameClockType.GameTimeAnims:
                    return _currentAnimTime;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clockType), clockType, null);
            }
        }

        [TempleDllLocation(0x10AA73FC)]
        private List<TimeEventListEntry> GetEventQueue(GameClockType clockType)
        {
            switch (clockType)
            {
                case GameClockType.RealTime:
                    return _eventQueueRealTime;
                case GameClockType.GameTime:
                    return _eventQueueGameTime;
                case GameClockType.GameTimeAnims:
                    return _eventQueueAnimTime;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clockType), clockType, null);
            }
        }

        [TempleDllLocation(0x10AA73E8)]
        private List<TimeEventListEntry> GetEventQueueWhileAdvancing(GameClockType clockType)
        {
            // TODO: Investigate whether this is still needed at all or can be moved into GetEventQueue
            switch (clockType)
            {
                case GameClockType.RealTime:
                    return _eventQueueRealTimeWhileAdvancing;
                case GameClockType.GameTime:
                    return _eventQueueGameTimeWhileAdvancing;
                case GameClockType.GameTimeAnims:
                    return _eventQueueAnimTimeWhileAdvancing;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clockType), clockType, null);
            }
        }

        [TempleDllLocation(0x100605c0)]
        private bool ScheduleInternal(GameTime time, TimeEvent evt, out GameTime triggerTimeOut)
        {
            Trace.Assert(evt != null);
            if (time.timeInMs == 0)
            {
                time.timeInMs = 1;
            }

            ref readonly var sysSpec = ref TimeEventTypeRegistry.Get(evt.system);

            evt.time = time;

            var newEntry = new TimeEventListEntry();
            // build the new entry
            newEntry.evt = evt;
            // store object references
            newEntry.objects = new FrozenObjRef[sysSpec.argTypes.Length];
            for (var i = 0; i < sysSpec.argTypes.Length; i++)
            {
                if (sysSpec.argTypes[i] == TimeEventArgType.Object)
                {
                    newEntry.objects[i] = GameSystems.MapObject.CreateFrozenRef(evt.GetArg(i).handle);
                }
            }

            var evtList = GetEventQueue(sysSpec.clock);
            if (_isAdvancingTime && !_isSpecialScheduling)
            {
                evtList = GetEventQueueWhileAdvancing(sysSpec.clock);
            }

            // insert event to the list (sorting from earliest to latest)
            evtList.Add(newEntry);
            evtList.Sort(); // This can probably be greatly improved

            triggerTimeOut = evt.time;
            return true;
        }

        [TempleDllLocation(0x10060150)]
        [TempleDllLocation(0x10060160)]
        [TempleDllLocation(0x100601c0)]
        public void SetStartingTime(int year, int day, int hourOfDay)
        {
            Trace.Assert(year >= 0);
            Trace.Assert(day >= 0);
            Trace.Assert(hourOfDay >= 0);

            StartingYear = year;
            StartingDayOfYear = day;
            StartingHourOfDay = hourOfDay;
            StartingHourOfDayInMs = 3600000 * hourOfDay;

            // Reset game time
            _currentGameTime.timeInDays = 0;
            _currentGameTime.timeInMs = 3600000 * hourOfDay;
            if (hourOfDay == 0)
            {
                _currentGameTime.timeInMs = 1;
            }

            GameSystems.Light.UpdateDaylight();
            GameSystems.Terrain.UpdateDayNight();

            // Reset animation time
            _currentAnimTime.timeInDays = 0;
            _currentAnimTime.timeInMs = StartingHourOfDayInMs;
            if (_currentAnimTime.timeInMs == 0)
                _currentAnimTime.timeInMs = 1;
        }

        private struct TimeEventListEntry : IComparable<TimeEventListEntry>
        {
            public TimeEvent evt;

            // Keeps track of objs referenced in the time event
            public FrozenObjRef[] objects;

            [TempleDllLocation(0x10060570)]
            public bool ObjHandlesValid()
            {
                ref readonly var sysSpec = ref TimeEventTypeRegistry.Get(evt.system);

                for (var i = 0; i < sysSpec.argTypes.Length; i++)
                {
                    if (sysSpec.argTypes[i] == TimeEventArgType.Object)
                    {
                        var obj = evt.GetArg(i).handle;
                        if (obj != null && !GameSystems.Object.IsValidHandle(obj))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            [TempleDllLocation(0x10060430)]
            public bool IsValid(bool isLoadingMap) // can do object handle recovery
            {
                ref readonly var system = ref TimeEventTypeRegistry.Get(evt.system);

                for (var i = 0; i < system.argTypes.Length; i++)
                {
                    var objectId = objects[i].guid;

                    ref var parVal = ref evt.GetArg(i);
                    var handle = parVal.handle;
                    if (objectId.IsNull)
                    {
                        if (system.argTypes[i] == TimeEventArgType.Object)
                        {
                            parVal.handle = null;

                            if (handle != null)
                            {
                                Logger.Warn("Non-null handle for ObjectIdKind.Null in TimeEventValidate: {0}", handle);
                            }
                        }

                        continue;
                    }

                    if (system.argTypes[i] == TimeEventArgType.Object && handle == null)
                    {
                        Logger.Debug("Null object handle for GUID {}", objectId);
                    }

                    if (handle != null || isLoadingMap)
                    {
                        if (GameSystems.Map.IsClearingMap())
                        {
                            return false;
                        }

                        if (GameSystems.Object.IsValidHandle(handle))
                        {
                            if (handle != null && handle.GetFlags().HasFlag(ObjectFlag.DESTROYED))
                            {
                                handle = null;
                                Logger.Debug("Destroyed object caught in TimeEvent IsValidHandle");
                                return false;
                            }

                            continue;
                        }

                        if (GameSystems.MapObject.Unfreeze(objects[i], out handle))
                        {
                            parVal.handle = handle;
                            if (handle == null || handle.GetFlags().HasFlag(ObjectFlag.DESTROYED))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            parVal.handle = null;
                            Logger.Debug("TimeEvent: Error: Object validate recovery Failed. TE-Type: {0}", evt.system);
                            return false;
                        }
                    }
                }

                return true;
            }

            public int CompareTo(TimeEventListEntry obj)
            {
                if (evt.time.timeInDays != obj.evt.time.timeInDays)
                {
                    return evt.time.timeInDays.CompareTo(obj.evt.time.timeInDays);
                }

                return evt.time.timeInMs.CompareTo(obj.evt.time.timeInMs);
            }
        }

        [TempleDllLocation(0x10060a40)]
        public void Remove(TimeEventType eventType, Predicate<TimeEvent> predicate)
        {
            ref readonly var system = ref TimeEventTypeRegistry.Get(eventType);
            var removedCallback = system.removedCallback;
            var clockType = system.clock;

            bool RemovePredicate(TimeEventListEntry timeEvent)
            {
                if (predicate(timeEvent.evt))
                {
                    removedCallback?.Invoke(timeEvent.evt);
                    return true;
                }

                return false;
            }

            GetEventQueue(clockType).RemoveAll(RemovePredicate);
            GetEventQueueWhileAdvancing(clockType).RemoveAll(RemovePredicate);
        }

        [TempleDllLocation(0x10060970)]
        public void RemoveAll(TimeEventType eventType)
        {
            ref readonly var system = ref TimeEventTypeRegistry.Get(eventType);
            var removedCallback = system.removedCallback;
            var clockType = system.clock;

            // Call the cleanup callback for every event we're about to remove, if the system has one
            if (removedCallback != null)
            {
                var queue = GetEventQueue(clockType);
                for (var index = queue.Count - 1; index >= 0; index--)
                {
                    removedCallback(queue[index].evt);
                    queue.RemoveAt(index);
                }

                var queueWhileAdvancing = GetEventQueueWhileAdvancing(clockType);
                for (var index = queueWhileAdvancing.Count - 1; index >= 0; index--)
                {
                    removedCallback(queueWhileAdvancing[index].evt);
                    queueWhileAdvancing.RemoveAt(index);
                }
            }
        }

        [TempleDllLocation(0x10061A50)]
        public void ClearForMapClose()
        {
            GameSystems.Anim.InterruptAll();
            RemoveAll(TimeEventType.Anim);
            RemoveAll(TimeEventType.AI);
            RemoveAll(TimeEventType.Combat);
            RemoveAll(TimeEventType.TBCombat);
            RemoveAll(TimeEventType.WorldMap);
            RemoveAll(TimeEventType.Teleported);

            // This is essentially the remainder of events that were not saved by SaveForTeleportDestination,
            // and we're saving them to the map we're in the process of leaving
            if (GameSystems.Teleport.IsProcessing)
            {
                var mapId = GameSystems.Map.GetCurrentMapId();
                GameSystems.Anim.SaveToMap(mapId, false);
                SaveForTeleport(mapId, false);
            }
        }

        private const string SaveFilename = "TimeEvent.dat";

        /// <summary>
        /// This is used to take all time events and animations that reference objects being teleported to another
        /// map, and append them to that map's files. When the current map switches over to the destination map,
        /// the animations and events will then be loaded again.
        /// </summary>
        [TempleDllLocation(0x100611d0)]
        public void SaveForTeleportDestination(int mapId)
        {
            RemoveAll(TimeEventType.Anim);
            GameSystems.Anim.SaveToMap(mapId, true);

            SaveForTeleport(mapId, true);
        }

        [TempleDllLocation(0x10061d10)]
        public void ValidateEvents()
        {
            foreach (var clockType in ClockTypes)
            {
                ValidateEvents(GetEventQueue(clockType));
                ValidateEvents(GetEventQueueWhileAdvancing(clockType));
            }
        }

        private void ValidateEvents(List<TimeEventListEntry> events)
        {
            foreach (var timeEvent in events)
            {
                ValidateEvent(timeEvent.evt);
            }
        }

        private void ValidateEvent(TimeEvent timeEvent)
        {
            var saveSpec = TimeEventSaveSpecs.SpecByType[timeEvent.system];
            for (var i = 0; i < saveSpec.Args.Length; i++)
            {
                var argType = saveSpec.Args[i];
                var arg = timeEvent.GetArg(i);
                if (argType == TimeEventArgType.Object)
                {
                    if (arg.handle != null)
                    {

                    }
                }
            }
        }

        [TempleDllLocation(0x10061d10)]
        [TempleDllLocation(0x10061bd0)]
        public void LoadFromMap(int mapId)
        {
            var saveDir = GameSystems.Map.GetSaveDir(mapId);
            var path = Path.Join(saveDir, SaveFilename);

            if (!File.Exists(path))
            {
                Logger.Info("Not loading saved time events for map {0} because none exist.", mapId);
                return;
            }

            var savedEvents = MapTimeEventState.Load(path);

            // NOTE: Events for the *current* map are actually stored in the primary save file and not
            //       in the map directory. This means this file must not stick around or otherwise events are
            //       going to be duplicated when the save is reloaded and this map is still the current map.
            File.Delete(path);

            foreach (var timeEvent in savedEvents.Events)
            {
                LoadTimeEvent(timeEvent);
            }

            Logger.Info("Loaded {0} saved time events for map {1}.", savedEvents.Events.Count, mapId);
        }

        [TempleDllLocation(0x10060db0)]
        private void SaveForTeleport(int mapId, bool forDestinationMap)
        {
            var saveDir = GameSystems.Map.GetSaveDir(mapId);
            Directory.CreateDirectory(saveDir);

            var path = Path.Join(saveDir, SaveFilename);

            // Maintain the time events that are already in that file.
            // this should _not_ be required since usually there can only be a single map transition
            // but to make sure, this is the solution...
            var savedEvents = new List<SavedTimeEvent>();
            if (File.Exists(path))
            {
                using var reader = new BinaryReader(new FileStream(path, FileMode.Open));
                savedEvents.AddRange(MapTimeEventState.Load(reader).Events);
            }

            static bool ShouldSave(TimeEvent timeEvent, bool forDestinationMap)
            {
                var saveSpec = TimeEventSaveSpecs.SpecByType[timeEvent.system];
                for (var i = 0; i < saveSpec.Args.Length; i++)
                {
                    if (saveSpec.Args[i] != TimeEventArgType.Object)
                    {
                        continue;
                    }

                    var obj = timeEvent.GetArg(i).handle;
                    if (obj == null)
                    {
                        continue;
                    }

                    var isTeleporting = GameSystems.Teleport.IsTeleporting(obj);
                    if (isTeleporting != forDestinationMap)
                    {
                        return false;
                    }
                }

                // Time Events that reference no objects should not be moved to the destination map
                return !forDestinationMap;
            }

            static void Process(ref List<TimeEventListEntry> events,
                List<SavedTimeEvent> savedEvents,
                bool forDestinationMap)
            {
                // Collect and serialize all time events that have game object arguments, which are being teleported
                for (var index = events.Count - 1; index >= 0; index--)
                {
                    var timeEvent = events[index];
                    if (ShouldSave(timeEvent.evt, forDestinationMap))
                    {
                        savedEvents.Add(SaveTimeEvent(timeEvent.evt));
                        events.RemoveAt(index);

                        ref readonly var system = ref TimeEventTypeRegistry.Get(timeEvent.evt.system);
                        system.removedCallback?.Invoke(timeEvent.evt);
                    }
                }
            }

            Process(ref _eventQueueRealTime, savedEvents, forDestinationMap);
            Process(ref _eventQueueGameTime, savedEvents, forDestinationMap);
            Process(ref _eventQueueAnimTime, savedEvents, forDestinationMap);

            using var writer = new BinaryWriter(new FileStream(path, FileMode.Create));
            MapTimeEventState.Save(writer, new MapTimeEventState
            {
                Events = savedEvents
            });
        }

        [TempleDllLocation(0x100603f0)]
        public void PauseGameTime()
        {
            if ( timeAdvanceBlockerCount < 30 )
            {
                ++timeAdvanceBlockerCount;
                GameSystems.Anim.fidgetDisabled = true;
            }
        }

        [TempleDllLocation(0x10060410)]
        public void ResumeGameTime()
        {
            if ( timeAdvanceBlockerCount > 0 && --timeAdvanceBlockerCount == 0 )
            {
                GameSystems.Anim.FidgetEnable();
            }
        }

        [TempleDllLocation(0x10060110)]
        public bool IsDaytimeInHours(int hoursElapsed)
        {
            var hourOfDay = (HourOfDay + hoursElapsed) % 24;
            return hourOfDay >= 6 && hourOfDay < 18;
        }
    }
}