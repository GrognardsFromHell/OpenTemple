using System;

namespace OpenTemple.Core.Systems.TimeEvents
{
    public class TimeEvent
    {
        public GameTime time;
        public TimeEventType system;
        public TimeEventArg arg1;
        public TimeEventArg arg2;
        public TimeEventArg arg3;
        public TimeEventArg arg4;

        public TimeEvent(TimeEventType system)
        {
            this.system = system;
        }

        public ref TimeEventArg GetArg(int index)
        {
            switch (index)
            {
                case 0:
                    return ref arg1;
                case 1:
                    return ref arg2;
                case 2:
                    return ref arg3;
                case 3:
                    return ref arg4;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}