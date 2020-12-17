namespace OpenTemple.Core.Ui.DOM
{
    /// <summary>
    /// Exposes the state of modifier keys on the keyboard.
    /// </summary>
    public abstract class KeyboardModifierEvent : UiEvent
    {
        protected KeyboardModifierEvent(SystemEventType systemType, string type, EventModifierInit eventInit) : base(systemType, type, eventInit)
        {
            if (eventInit != null)
            {
                CtrlKey = eventInit.CtrlKey;
                AltKey = eventInit.AltKey;
                ShiftKey = eventInit.ShiftKey;
                MetaKey = eventInit.MetaKey;
            }
        }

        public bool CtrlKey { get; }
        public bool ShiftKey { get; }
        public bool AltKey { get; }
        public bool MetaKey { get; }

        /// <summary>
        /// Checks that the modifier state has only the given modifiers pressed, and no more.
        /// </summary>
        public bool HasOnlyModifiers(KeyboardModifier modifiers) => ActiveModifiers == modifiers;

        /// <summary>
        /// Checks that the modifier state has at least the given modifiers pressed, but also
        /// allows for additional modifiers to be present.
        /// </summary>
        public bool HasModifiers(KeyboardModifier modifiers) => (ActiveModifiers & modifiers) == modifiers;

        /// <summary>
        /// Returns true if no modifiers are active.
        /// </summary>
        public bool HasNoModifiers => ActiveModifiers == 0;

        /// <summary>
        /// Returns the currently pressed modifiers.
        /// </summary>
        public KeyboardModifier ActiveModifiers
        {
            get
            {
                KeyboardModifier result = 0;
                if (CtrlKey)
                {
                    result |= KeyboardModifier.Control;
                }

                if (ShiftKey)
                {
                    result |= KeyboardModifier.Shift;
                }

                if (AltKey)
                {
                    result |= KeyboardModifier.Alt;
                }

                if (MetaKey)
                {
                    result |= KeyboardModifier.Meta;
                }

                return result;
            }
        }

        public override void CopyTo(EventInit init)
        {
            base.CopyTo(init);

            if (init is EventModifierInit modInit)
            {
                modInit.CtrlKey = CtrlKey;
                modInit.ShiftKey = ShiftKey;
                modInit.AltKey = AltKey;
                modInit.MetaKey = MetaKey;
            }
        }
    }
}