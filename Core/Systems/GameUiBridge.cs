using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Ui.InGameSelect;

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

        [TempleDllLocation(0x1009ab80)]
        [TempleDllLocation(0x1009ab70)]
        public static void EnableTutorial()
        {
            if (!UiSystems.HelpManager.IsTutorialActive)
            {
                UiSystems.HelpManager.ToggleTutorial();
            }
        }

        [TempleDllLocation(0x1009A540)]
        [TempleDllLocation(0x1014e170)]
        public static void OnAfterMapLoad()
        {
            // TODO
            UiSystems.TB.OnAfterMapLoad();
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

        [TempleDllLocation(0x10AA83F4)]
        public static void CombatSthCallback()
        {
            UiSystems.Combat.SthCallback();
        }

        [TempleDllLocation(0x10AA83FC)]
        public static void RefreshInitiativePortraits()
        {
            UiSystems.Combat.Update();
        }

        [TempleDllLocation(0x1009A6A0)]
        public static void SaveUiFocus()
        {
            UiSystems.InGameSelect.SaveFocus();
        }

        [TempleDllLocation(0x1009a6b0)]
        [TempleDllLocation(0x10138de0)]
        public static void RestoreUiFocus()
        {
            UiSystems.InGameSelect.RestoreFocus();
        }

        [TempleDllLocation(0x1009ABE0)]
        public static GameObjectBody GetUiFocus()
        {
            return UiSystems.InGameSelect.Focus;
        }

        [TempleDllLocation(0x1009a7b0)]
        [TempleDllLocation(0x10198230)]
        public static void OnKeyReceived(int keyId, TimePoint timePoint)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1009A730)]
        public static void UpdateCombatUi()
        {
            UiSystems.Combat.Update();
        }

        [TempleDllLocation(0x1009AA40)]
        public static bool IsPartyPoolVisible()
        {
            return UiSystems.PartyPool.IsVisible;
        }

        [TempleDllLocation(0x1009a740)]
        public static void UpdatePartyUi()
        {
            UiSystems.Party.Update();
        }

        [TempleDllLocation(0x10B3D6F4)]
        public static void ShowHelp(HelpRequest a, int b)
        {
            UiSystems.Help.Show(a, b);
        }

        [TempleDllLocation(0x1009ac00)]
        [TempleDllLocation(0x10112fc0)]
        public static CursorType? GetCursor()
        {
            return UiSystems.InGame.GetCursor();
        }

        [TempleDllLocation(0x1009a7e0)]
        public static void MarkKeyUsed(int keyId, TimePoint timeUsed)
        {
            UiSystems.Logbook.MarkKeyUsed(keyId, timeUsed);
        }

        [TempleDllLocation(0x1009a810)]
        [TempleDllLocation(0x10b3d6d8)]
        public static bool IsKeyAcquired(int keyId)
        {
            return UiSystems.Logbook.IsKeyAcquired(keyId);
        }

        [TempleDllLocation(0x1009a7a0)]
        public static void ShowWorldMap(int mode)
        {
            UiSystems.WorldMap.Show(mode);
        }

        [TempleDllLocation(0x1009a430)]
        public static void OpenContainer(GameObjectBody actor, GameObjectBody container)
        {
            UiSystems.TB.OpenContainer(actor, container);
        }

        [TempleDllLocation(0x1009a460)]
        public static void InitiateDialog(GameObjectBody source, GameObjectBody target)
        {
            UiSystems.TB.InitiateDialog(source, target);
        }

        [TempleDllLocation(0x1009A5D0)]
        public static void CancelDialog(GameObjectBody critter)
        {
            UiSystems.Dialog.CancelDialog(critter);
        }

        [TempleDllLocation(0x1009ab00)]
        public static bool IsDialogOpen()
        {
            return UiSystems.Dialog.IsActive;
        }

        [TempleDllLocation(0x10B3D6B4)]
        [TempleDllLocation(0x1009a710)]
        public static void RenderTurnBasedUI()
        {
            UiSystems.TB.Render();
        }

        [TempleDllLocation(0x1009A910)]
        public static void RecordKill(GameObjectBody killer, GameObjectBody killed)
        {
            UiSystems.Logbook.RecordKill(killer, killed);
        }

        public static void ShowTextBubble(GameObjectBody performer, string text)
        {
            UiSystems.Dialog.ShowTextBubble(performer, performer, text, -1);
        }

        [TempleDllLocation(0x1009a9f0)]
        public static void IncreaseCritHits(GameObjectBody attacker)
        {
            UiSystems.Logbook.RecordCriticalHit(attacker);
        }

        [TempleDllLocation(0x1009a6d0)]
        public static bool ShowPicker(PickerArgs picker, object callbackArgs = null)
        {
            return UiSystems.InGameSelect.ShowPicker(picker, callbackArgs);
        }

        [TempleDllLocation(0x1009a6f0)]
        public static void FreeCurrentPicker()
        {
            UiSystems.InGameSelect.FreeCurrentPicker();
        }

        [TempleDllLocation(0x1009A950)]
        [TempleDllLocation(0x10B3D724)]
        public static void LogbookStartCombat()
        {
            UiSystems.Logbook.RecordCombatStart();
        }

        [TempleDllLocation(0x1009A960)]
        [TempleDllLocation(0x10B3D728)]
        public static void LogbookNextTurn()
        {
            UiSystems.Logbook.RecordNewTurn();
        }

        [TempleDllLocation(0x1009ab50)]
        [TempleDllLocation(0x10B3D784)]
        public static void ResetSelectionInput()
        {
            UiSystems.InGame.ResetInput();
        }

        [TempleDllLocation(0x1009abf0)]
        [TempleDllLocation(0x10b3d7a4)]
        public static bool IsPickerTargetInvalid()
        {
            return UiSystems.InGameSelect.IsCurrentPickerTargetInvalid;
        }

        [TempleDllLocation(0x1009abd0)]
        [TempleDllLocation(0x10b3d79c)]
        public static int GetSleepStatus()
        {
            return GameSystems.RandomEncounter.SleepStatus;
        }

        [TempleDllLocation(0x1009a890)]
        [TempleDllLocation(0x10b3d700)]
        public static void CreateItem(GameObjectBody creator, int actionData1)
        {
            UiSystems.ItemCreation.CreateItem(creator, actionData1);
        }

        [TempleDllLocation(0x1009a830)]
        [TempleDllLocation(0x10b3d6e4)]
        public static void OpenInventory(GameObjectBody critter)
        {
            UiSystems.CharSheet.Show(critter);
        }
    }
}