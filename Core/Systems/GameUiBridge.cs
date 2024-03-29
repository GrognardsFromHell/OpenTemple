using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Ui.WorldMap;

namespace OpenTemple.Core.Systems;

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
    public static void OnObjectDestroyed(GameObject obj)
    {
        UiSystems.Dialog.CancelDialog(obj);
    }

    [TempleDllLocation(0x1009a790)]
    public static void OnMapChangeBegin(int destMapId)
    {
        UiSystems.WorldMap.OnTravelingToMap(destMapId);
    }

    [TempleDllLocation(0x1009ab90)]
    public static void ShowTutorialTopic(TutorialTopic topicId)
    {
        UiSystems.HelpManager.ShowTutorialTopic(topicId);
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
        UiSystems.Combat.Initiative.Update();
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
    public static GameObject GetUiFocus()
    {
        return UiSystems.InGameSelect.Focus;
    }

    [TempleDllLocation(0x1009a7b0)]
    [TempleDllLocation(0x10B3D6D4)]
    public static void OnKeyReceived(int keyId, TimePoint timePoint)
    {
        UiSystems.Logbook.Keys.KeyAcquired(keyId, timePoint);
    }

    [TempleDllLocation(0x1009A730)]
    [TempleDllLocation(0x10B3D6BC)]
    public static void UpdateInitiativeUi()
    {
        UiSystems.Combat.Initiative.UpdateIfNeeded();
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

    [TempleDllLocation(0x1009a870)]
    [TempleDllLocation(0x10B3D6F4)]
    public static void ShowHelp(HelpRequest request, bool hideUis)
    {
        UiSystems.Help.Show(request, hideUis);
    }

    [TempleDllLocation(0x1009ac00)]
    [TempleDllLocation(0x10112fc0)]
    public static ActionCursor? GetCursor()
    {
        return UiSystems.InGame.GetCursor();
    }

    [TempleDllLocation(0x1009a7e0)]
    public static void MarkKeyUsed(int keyId, TimePoint timeUsed)
    {
        UiSystems.Logbook.Keys.MarkKeyUsed(keyId, timeUsed);
    }

    [TempleDllLocation(0x1009a810)]
    [TempleDllLocation(0x10b3d6d8)]
    public static bool IsKeyAcquired(int keyId)
    {
        return UiSystems.Logbook.Keys.IsKeyAcquired(keyId);
    }

    [TempleDllLocation(0x1009a7a0)]
    public static void ShowWorldMap(WorldMapMode mode)
    {
        UiSystems.WorldMap.Show(mode);
    }

    [TempleDllLocation(0x1009a430)]
    public static void OpenContainer(GameObject actor, GameObject container)
    {
        UiSystems.TB.OpenContainer(actor, container);
    }

    [TempleDllLocation(0x1009a460)]
    public static void InitiateDialog(GameObject source, GameObject target)
    {
        UiSystems.TB.InitiateDialog(source, target);
    }

    [TempleDllLocation(0x10007a90)]
    public static void InitiateDialog(GameObject pc, GameObject npc, int scriptId, int i, int line)
    {
        UiSystems.Dialog.InitiateDialog(pc, npc, scriptId, i, line);
    }

    [TempleDllLocation(0x1009A5D0)]
    public static void CancelDialog(GameObject critter)
    {
        UiSystems.Dialog.CancelDialog(critter);
    }

    [TempleDllLocation(0x1009ab00)]
    public static bool IsDialogOpen()
    {
        return UiSystems.Dialog.IsVisible;
    }

    [TempleDllLocation(0x10B3D6B4)]
    [TempleDllLocation(0x1009a710)]
    public static void RenderTurnBasedUI(IGameViewport viewport)
    {
        UiSystems.TB.Render(viewport);
    }

    [TempleDllLocation(0x1009ac20)]
    [TempleDllLocation(0x10B3D7B0)]
    public static void SetLogbookQuotes(bool enable)
    {
        UiSystems.Logbook.SetCanShowQuotes(enable);
    }

    [TempleDllLocation(0x1009A910)]
    public static void RecordKill(GameObject killer, GameObject killed)
    {
        UiSystems.Logbook.RecordKill(killer, killed);
    }

    public static void ShowTextBubble(GameObject performer, string text)
    {
        UiSystems.Dialog.ShowTextBubble(performer, performer, text, -1);
    }

    [TempleDllLocation(0x1009a9f0)]
    public static void IncreaseCritHits(GameObject attacker)
    {
        UiSystems.Logbook.RecordCriticalHit(attacker);
    }

    [TempleDllLocation(0x1009a6d0)]
    public static bool ShowPicker(PickerArgs picker, object? callbackArgs = null)
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

    [TempleDllLocation(0x1009A940)]
    [TempleDllLocation(0x10B3D720)]
    public static void LogbookExperience(int xpTotalFromCombat)
    {
        UiSystems.Logbook.RecordCombatExperience(xpTotalFromCombat);
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
    public static SleepStatus GetSleepStatus()
    {
        return GameSystems.RandomEncounter.SleepStatus;
    }

    [TempleDllLocation(0x1009a890)]
    [TempleDllLocation(0x10b3d700)]
    public static void CreateItem(GameObject creator, int actionData1)
    {
        UiSystems.ItemCreation.CreateItem(creator, actionData1);
    }

    [TempleDllLocation(0x1009a830)]
    [TempleDllLocation(0x10b3d6e4)]
    public static void OpenInventory(GameObject critter)
    {
        UiSystems.CharSheet.Show(critter);
    }

    [TempleDllLocation(0x1009a780)]
    [TempleDllLocation(0x10b3d6c4)]
    public static bool IsWorldmapMakingTrip()
    {
        return UiSystems.WorldMap.IsMakingTrip;
    }

    [TempleDllLocation(0x1009aa90)]
    [TempleDllLocation(0x10b3d758)]
    public static void ApplySkillMastery(GameObject critter)
    {
        UiSystems.SkillMastery.SkillMasteryCallback(critter);
    }

    [TempleDllLocation(0x1009ab40)]
    [TempleDllLocation(0x10b3d788)]
    public static bool IsRadialMenuOpen()
    {
        return UiSystems.RadialMenu.IsOpen;
    }

    [TempleDllLocation(0x1009ac10)]
    [TempleDllLocation(0x10b3d7ac)]
    public static bool GetIntgameWidgetEnteredForRender()
    {
        return UiSystems.TurnBased.uiIntgameWidgetEnteredForRender;
    }

    public static GameObject GetIntgameTargetFromRaycast()
    {
        return UiSystems.TurnBased.intgameTargetFromRaycast;
    }

    [TempleDllLocation(0x1009ac10)]
    [TempleDllLocation(0x10b3d7ac)]
    public static bool GetIntgameWidgetEnteredForGameplay()
    {
        return UiSystems.TurnBased.WidgetEnteredForGameplay;
    }

    [TempleDllLocation(0x1009a9d0)]
    [TempleDllLocation(0x10b3d738)]
    public static void LogbookCombatMiss(GameObject performer)
    {
        UiSystems.Logbook.RecordCombatMiss(performer);
    }

    [TempleDllLocation(0x1009a9b0)]
    [TempleDllLocation(0x10b3d734)]
    public static void LogbookCombatHit(GameObject performer)
    {
        UiSystems.Logbook.RecordCombatHit(performer);
    }

    [TempleDllLocation(0x1009aa10)]
    [TempleDllLocation(0x10b3d740)]
    public static void LogbookCombatDamage(bool weaponDamage, int damageAmount,
        GameObject attacker, GameObject victim)
    {
        UiSystems.Logbook.RecordCombatDamage(weaponDamage, damageAmount, attacker, victim);
    }

    [TempleDllLocation(0x1009a8e0)]
    [TempleDllLocation(0x10B3D70C)]
    public static void ActivateTrack(GameObject tracker)
    {
        UiSystems.Track.Show(tracker);
    }

    [TempleDllLocation(0x1009a750)]
    [TempleDllLocation(0x10B3D714)]
    public static void Confirm(string body, string title, bool yesNoButtons, Action<int> callback)
    {
        UiSystems.Popup.ConfirmBox(body, title, yesNoButtons, callback);
    }

    [TempleDllLocation(0x1009a760)]
    [TempleDllLocation(0x10B3D718)]
    public static void ShowTextEntry(string message, Action<string, bool> callback)
    {
        UiSystems.Popup.RequestTextEntry(message, "", callback);
    }

    [TempleDllLocation(0x1009aba0)]
    [TempleDllLocation(0x10b3d794)]
    public static void ShowAlert(HelpRequest helpRequest, Action<int> callback, string buttonText)
    {
        UiSystems.Alert.Show(helpRequest, callback, buttonText);
    }

    [TempleDllLocation(0x1009a850)]
    [TempleDllLocation(0x10b3d6f0)]
    public static void AddRumor(int rumorId)
    {
        UiSystems.Logbook.Rumors.Add(rumorId);
    }

    [TempleDllLocation(0x1009a770)]
    [TempleDllLocation(0x10b3d6c8)]
    public static void AreaDiscovered(int area)
    {
        UiSystems.WorldMap.AreaDiscovered(area);
    }

    [TempleDllLocation(0x1009ab10)]
    [TempleDllLocation(0x10B3D770)]
    public static void RecordSkillUse(GameObject critter, SkillId skill)
    {
        UiSystems.Logbook.RecordSkillUse(critter, skill);
    }

    [TempleDllLocation(0x1009a990)]
    [TempleDllLocation(0x10b3d730)]
    public static void RecordTrapSetOff(GameObject critter)
    {
        UiSystems.Logbook.RecordTrapSetOff(critter);
    }

    [TempleDllLocation(0x1009a970)]
    [TempleDllLocation(0x10b3d72c)]
    public static void RecordTrapDisarmed(GameObject critter)
    {
        UiSystems.Logbook.RecordTrapDisarmed(critter);
    }

    [TempleDllLocation(0x1009a4e0)]
    [TempleDllLocation(0x10b3d5fc)]
    public static void TotalPartyKill()
    {
        UiSystems.Anim.BkgAnimTimeEventSchedule(0, 5000);
    }

    [TempleDllLocation(0x1009a4f0)]
    [TempleDllLocation(0x10B3D600)]
    [TempleDllLocation(0x1014E160)]
    public static void EndGame()
    {
        UiSystems.Anim.BkgAnimTimeEventSchedule(1, 50);
    }

    [TempleDllLocation(0x1009aaf0)]
    [TempleDllLocation(0x10b3d768)]
    public static void PulseLogbookButton()
    {
        UiSystems.UtilityBar.PulseLogbookButton();
    }

    [TempleDllLocation(0x1009a6c0)]
    [TempleDllLocation(0x10b3d6a8)]
    public static void RevealTownMapMarker(int mapId, int markerId)
    {
        UiSystems.TownMap.RevealMarker(mapId, markerId);
    }

    [TempleDllLocation(0x1009a900)]
    [TempleDllLocation(0x10b3d710)]
    public static void WorldMapTravelByDialog(int areaId)
    {
        UiSystems.WorldMap.TravelToArea(areaId);
    }

    [TempleDllLocation(0x1009ab60)]
    [TempleDllLocation(0x10b3d78c)]
    public static bool IsRestDisabled()
    {
        // Previously checked the "input state" which was never set to anything but 0
        return false;
    }
}