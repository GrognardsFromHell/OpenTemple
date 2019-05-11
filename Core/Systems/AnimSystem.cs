using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.Systems
{

    public enum AnimGoalPriority
    {
        FIVE
    }

    public class AnimSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10054e10)]
        public bool IsProcessing
        {
            get
            {
                return false; // TODO
            }
        }

        [TempleDllLocation(0x10015d70)]
        public void PushFidget(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        public void PushDisableFidget()
        {
            Stub.TODO();
        }

        public void PopDisableFidget()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100146c0)]
        public void StartFidgetTimer()
        {
            if (!GameSystems.TimeEvent.IsScheduled(TimeEventType.FidgetAnim))
            {
                var partySize = GameSystems.Party.PartySize;
                if (partySize <= 0)
                {
                    partySize = 1;
                }

                var delay = TimeSpan.FromSeconds(10.0 / partySize);

                var timeEvent = new TimeEvent(TimeEventType.FidgetAnim);

                GameSystems.TimeEvent.Schedule(timeEvent, delay, out _);
            }
        }

        public bool ProcessAnimEvent(TimeEvent evt)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000c760)]
        public void ClearForObject(GameObjectBody obj)
        {
            // TODO
        }

        [TempleDllLocation(0x1000c890)]
        public void InterruptAll()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000c7e0)]
        public void Interrupt(GameObjectBody obj, AnimGoalPriority priority, bool all)
        {
            Stub.TODO();
        }

        public void ClearGoalDestinations()
        {
            // TODO
        }

        [TempleDllLocation(0x1001a1d0)]
        public void PushIdleOrLoop(GameObjectBody obj)
        {
            Logger.Info("PushIdleOrLoop for {0}", obj);
            // TODO
        }

        [TempleDllLocation(0x1000c750)]
        public void SetAllGoalsClearedCallback(Action callback)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1000c950)]
        public bool InterruptAllForTbCombat()
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1001aaa0)]
        public void NotifySpeedRecalc(GameObjectBody obj)
        {
            Stub.TODO();
        }

    }
}