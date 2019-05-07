using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;
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

        [TempleDllLocation(0x102BDF88)]
        public int StartingYear { get; private set; }

        [TempleDllLocation(0x102BDF8C)]
        public int StartingHourOfDayInMs { get; private set; }

        [TempleDllLocation(0x102BDF90)]
        public int StartingDayOfYear { get; private set; }

        public int HourOfDay => _currentGameTime.timeInMs / 3600000 % 24;

        public TimePoint GameTime => new TimePoint(TimePoint.TicksPerMillisecond * _currentGameTime.timeInMs
                                                   + TimePoint.TicksPerSecond * _currentGameTime.timeInDays * 24 * 60 * 60);

        [TempleDllLocation(0x100616a0)]
        public TimeEventSystem()
        {
            // TODO
        }

        [TempleDllLocation(0x10061820)]
        public void Dispose()
        {
            // TODO
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
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x100620c0)]
        public void AdvanceTime(TimePoint time)
        {
            // TODO
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
                    newEntry.objects[i] = GameSystems.Object.CreateFrozenRef(evt.GetArg(i).handle);
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

        private struct TimeEventListEntry
        {
            public TimeEvent evt;

            // Keeps track of objs referenced in the time event
            public FrozenObjRef[] objects;

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

                        if (GameSystems.Object.Unfreeze(objects[i], out handle))
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
                            Logger.Debug("TImeEvent: Error: Object validate recovery Failed. TE-Type: {0}", evt.system);
                            return false;
                        }
                    }
                }

                return true;
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

        [TempleDllLocation(0x10061A50)]
        public void ClearForMapClose()
        {
            // TODO
        }

        [TempleDllLocation(0x10061d10)]
        public void LoadForCurrentMap()
        {
            // TODO
        }
    }
}