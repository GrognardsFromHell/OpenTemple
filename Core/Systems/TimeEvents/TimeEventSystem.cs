using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.TimeEvents
{
    public class TimeEventSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

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
        public TimePoint GameTime => ToTimePoint(_currentGameTime);

        public CampaignCalendar CampaignCalendar => CampaignCalendar.FromElapsedTime(GameTime, StartingYear,
            StartingDayOfYear, StartingHourOfDay);

        [TempleDllLocation(0x1005fc60)]
        public TimePoint AnimTime => new TimePoint(TimePoint.TicksPerMillisecond * _currentAnimTime.timeInMs
                                                   + TimePoint.TicksPerSecond * _currentAnimTime.timeInDays * SecondsPerDay);

        private static TimePoint ToTimePoint(GameTime gameTime)
        {
            return new TimePoint(TimePoint.TicksPerMillisecond * gameTime.timeInMs
                                 + TimePoint.TicksPerSecond * gameTime.timeInDays * SecondsPerDay);
        }

        private static GameTime ToGameTime(TimePoint gameTime)
        {
            var ms = (long) gameTime.Milliseconds;
            var msecs = ms % (SecondsPerDay * 1000);
            var days = ms / (SecondsPerDay * 1000);
            return new GameTime((int) days, (int) msecs);
        }

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
                              + ((int) (time.Seconds / SecondsPerDay) + StartingDayOfYear) / CampaignCalendar.DaysPerYear;

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
                            if ( hours < 10 )
                            {
                                result.Append('0');
                            }

                            result.Append(hours);
                            break;

                        case 'M':
                            var minutes = (int)(time.Seconds / 60) % 60;
                            if (minutes < 10)
                            {
                                result.Append('0');
                            }

                            result.Append(minutes);
                            break;

                        case 'D':
                            // NOTE that this is not actually the day of the year, but rather the day since the game
                            // has started.
                            result.Append((int)(time.Seconds / SecondsPerDay) + 1);
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
        public bool SaveGame()
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10061f90)]
        public bool LoadGame()
        {
            throw new System.NotImplementedException();
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
                    system.expiredCallback?.Invoke(pendingEvent.evt);
                }

                foreach (var pendingEvent in GetEventQueueWhileAdvancing(clockType))
                {
                    ref readonly var system = ref TimeEventTypeRegistry.Get(pendingEvent.evt.system);
                    system.expiredCallback?.Invoke(pendingEvent.evt);
                }

                GetEventQueue(clockType).Clear();
                GetEventQueueWhileAdvancing(clockType).Clear();
            }
        }

        [TempleDllLocation(0x10aa83d0)]
        private TimePoint _timePoint;

        [TempleDllLocation(0x10AA83D8)]
        public int timeAdvanceBlockerCount { get; set; }


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
            Stub.TODO();
        }

        [TempleDllLocation(0x10060c00)]
        public bool IsScheduled(TimeEventType systemType)
        {
            var clockType = TimeEventTypeRegistry.Get(systemType).clock;

            return GetEventQueue(clockType).Any(e => e.evt.system == systemType)
                   || GetEventQueueWhileAdvancing(clockType).Any(e => e.evt.system == systemType);
        }

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
            for (var i = 0; i < 4; i++)
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

                for (var i = 0; i < 4; i++)
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

                for (var i = 0; i < 4; i++)
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
            Stub.TODO();
        }

        [TempleDllLocation(0x10061d10)]
        public void LoadForCurrentMap()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100611d0)]
        public void SaveForMap(int mapId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100603f0)]
        public void PushDisableFidget()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10060410)]
        public void PopDisableFidget()
        {
            Stub.TODO();
        }
    }
}