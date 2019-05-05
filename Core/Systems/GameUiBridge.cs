using System;

namespace SpicyTemple.Core.Systems
{
    public static class GameUiBridge
    {
        [TempleDllLocation(0x1009AC40)]
        public static void ShowTip(string title,
            string bodyText,
            string confirmBtnText,
            string cancelBtnText,
            string checboxText,
            out bool statePtr,
            Action<bool> callback)
        {
            // TODO
            statePtr = false;
        }

        [TempleDllLocation(0x1009ab80)]
        public static bool IsTutorialActive()
        {
            // TODO
            return false;
        }

        [TempleDllLocation(0x1009A540)]
        [TempleDllLocation(0x1014e170)]
        public static void OnAfterMapLoad()
        {
            // TODO
        }
    }
}