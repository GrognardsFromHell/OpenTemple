using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenTemple.Core.Ui.DOM
{
    /// <summary>
    /// Defines known system events.
    /// </summary>
    public enum SystemEventType
    {
        // https://www.w3.org/TR/uievents/#events-focus-types
        #region Focus Events
        Blur,
        Focus,
        FocusIn,
        FocusOut,
        #endregion

        // https://www.w3.org/TR/uievents/#events-mouse-types
        #region Mouse Events
        AuxClick,
        Click,
        DblClick,
        MouseDown,
        MouseEnter,
        MouseLeave,
        MouseMove,
        MouseOut,
        MouseOver,
        MouseUp,
        #endregion

        // https://www.w3.org/TR/uievents/#events-wheel-types
        #region Wheel Events
        Wheel,
        #endregion

        // https://www.w3.org/TR/uievents/#events-wheel-types
        #region Keyboard Events
        KeyDown,
        KeyUp,
        #endregion

        /// <summary>
        /// Used for custom events.
        /// </summary>
        Custom,
    }

    public static class SystemEventTypes
    {
        private static readonly string[] ToNameMap;
        private static readonly Dictionary<string, SystemEventType> ToTypeMap;

        static SystemEventTypes()
        {
            var values = (SystemEventType[]) Enum.GetValues(typeof(SystemEventType));
            ToNameMap = new string[values.Length];
            ToTypeMap = new Dictionary<string, SystemEventType>(values.Length);
            foreach (var systemEventType in values)
            {
                var name = Enum.GetName(typeof(SystemEventType), systemEventType)!.ToLowerInvariant();
                ToNameMap[(int) systemEventType] = name;
                ToTypeMap.Add(name, systemEventType);
            }
        }

        public static string GetName(SystemEventType type)
        {
            Debug.Assert(type != SystemEventType.Custom);
            return ToNameMap[(int) type];
        }

        public static SystemEventType FromName(string type) =>
            ToTypeMap.GetValueOrDefault(type, SystemEventType.Custom);
    }

}