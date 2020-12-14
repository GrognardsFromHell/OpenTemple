namespace OpenTemple.Core.Ui.DOM
{
    public class WheelEvent : MouseEvent
    {
        public WheelEvent(SystemEventType type, WheelEventInit eventInit = default) : this(type, null, eventInit)
        {
        }

        public WheelEvent(string type, WheelEventInit eventInit = default) : this(SystemEventType.Custom, type,
            eventInit)
        {
        }

        private WheelEvent(SystemEventType systemType, string type, WheelEventInit eventInit = null) : base(systemType,
            type, eventInit)
        {
            if (eventInit != null)
            {
                DeltaX = eventInit.DeltaX;
                DeltaY = eventInit.DeltaY;
                DeltaMode = eventInit.DeltaMode;
                WheelTicksX = eventInit.WheelTicksX;
                WheelTicksY = eventInit.WheelTicksY;
            }
        }

        public double DeltaX { get; }
        public double DeltaY { get; }
        public DeltaModeCode DeltaMode { get; }

        // Non-Standard Extensions
        public double WheelTicksX { get; set; }
        public double WheelTicksY { get; set; }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is WheelEventInit wheelEventInit)
            {
                wheelEventInit.DeltaX = DeltaX;
                wheelEventInit.DeltaY = DeltaY;
                wheelEventInit.WheelTicksX = WheelTicksX;
                wheelEventInit.WheelTicksY = WheelTicksY;
                wheelEventInit.DeltaMode = DeltaMode;
            }
        }

        public override EventImpl Copy()
        {
            var init = new WheelEventInit();
            CopyTo(init);
            return new WheelEvent(SystemType, Type, init);
        }
    }

    public enum DeltaModeCode
    {
        LINE = 1,
        PAGE = 2
    }

    public class WheelEventInit : MouseEventInit
    {
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public double WheelTicksX { get; set; }
        public double WheelTicksY { get; set; }
        public DeltaModeCode DeltaMode { get; set; }
    }
}