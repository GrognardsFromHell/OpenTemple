using System;

namespace SpicyTemple.Core.Systems.TimeEvents
{
    /// <summary>
    /// Contains the specification of how a type of time event is to be handled.
    /// </summary>
    internal readonly struct TimeEventTypeSpec
    {
        /// Which clock is used for events of this type
        public readonly GameClockType clock;

        /// Called when an event of this type expires
        public readonly Func<TimeEvent, bool> expiredCallback;

        /// Called whenever an event is freed (even if not expired)
        public readonly Func<TimeEvent, bool> removedCallback;

        /// Events of this type are saved to the savegame
        public readonly bool persistent;

        /// The types of the arguments stored in the time event
        public readonly TimeEventArgType[] argTypes;

        public TimeEventTypeSpec(GameClockType clock,
            Func<TimeEvent, bool> expiredCallback,
            Func<TimeEvent, bool> removedCallback,
            bool persistent,
            TimeEventArgType arg1 = TimeEventArgType.None,
            TimeEventArgType arg2 = TimeEventArgType.None,
            TimeEventArgType arg3 = TimeEventArgType.None,
            TimeEventArgType arg4 = TimeEventArgType.None
        )
        {
            this.clock = clock;
            this.expiredCallback = expiredCallback;
            this.removedCallback = removedCallback;
            this.persistent = persistent;

            argTypes = new[] {arg1, arg2, arg3, arg4};
        }
    };
}