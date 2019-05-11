using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;

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
        public static bool IsTutorialActive() => UiSystems.HelpManager.IsTutorialActive;

        [TempleDllLocation(0x1009A540)]
        [TempleDllLocation(0x1014e170)]
        public static void OnAfterMapLoad()
        {
            // TODO
        }

        [TempleDllLocation(0x1009a3b0)]
        [TempleDllLocation(0x1014de90)]
        public static void OnObjectDestroyed(GameObjectBody obj)
        {
            // TODO
        }

        public static void OnAfterRest(int hoursToPass)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1009a790)]
        public static void OnMapChangeBegin(int destMapId)
        {
            // TODO Worldmap UI call @ 0x101596a0
        }

        [TempleDllLocation(0x1009ab90)]
        public static void ShowTutorialTopic(int topicId)
        {
            UiSystems.HelpManager.ShowTopic(topicId);
        }

        [TempleDllLocation(0x10AA83F8)]
        public static void OnExitCombat()
        {
            UiSystems.Combat.Reset();
        }

        [TempleDllLocation(0x1009A6A0)]
        public static void OnPartyTeleport()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1009a6b0)]
        [TempleDllLocation(0x10138de0)]
        public static void OnAfterTeleport()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1009a7b0)]
        [TempleDllLocation(0x10198230)]
        public static void OnKeyReceived(int keyId, TimePoint timePoint)
        {
            Stub.TODO();
        }
    }
}