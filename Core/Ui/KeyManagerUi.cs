using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.MainMenu;
using SDL2;

namespace OpenTemple.Core.Ui
{
    public class KeyManagerUi : IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10BE8CF0)]
        private bool _modalIsOpen;

        [TempleDllLocation(0x10be7018)]
        private readonly Dictionary<int, string> _translations;

        [TempleDllLocation(0x10be701c)]
        private string QuitGameMessage => _translations[0];

        [TempleDllLocation(0x10be8cec)]
        private string QuitGameTitle => _translations[1];

        [TempleDllLocation(0x10be8cf0)]
        private bool uiManagerDoYouWantToQuitActive = false;

        [TempleDllLocation(0x10143bd0)]
        public KeyManagerUi()
        {
            _modalIsOpen = false;
            _translations = Tig.FS.ReadMesFile("mes/ui_manager.mes");
        }

        // The cited DLL locations refer to the old key-handler methods splattered across in-game ui
        [TempleDllLocation(0x10114eb0)]
        [TempleDllLocation(0x101132b0)]
        [TempleDllLocation(0x10114e30)]
        [TempleDllLocation(0x101130b0)]
        [TempleDllLocation(0x10114ef0)]
        public IEnumerable<HotkeyAction> EnumerateHotkeyActions()
        {
            yield return new HotkeyAction(InGameHotKey.SelectChar1, () => SelectPartyMember(0), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar2, () => SelectPartyMember(1), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar3, () => SelectPartyMember(2), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar4, () => SelectPartyMember(3), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar5, () => SelectPartyMember(4), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar6, () => SelectPartyMember(5), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar7, () => SelectPartyMember(6), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar8, () => SelectPartyMember(7), CanTriggerOutOfCombatHotkeys);
            yield return new HotkeyAction(InGameHotKey.SelectChar9, () => SelectPartyMember(8), CanTriggerOutOfCombatHotkeys);

            yield return new HotkeyAction(InGameHotKey.TogglePartySelection1, () => TogglePartyMemberSelection(0), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection2, () => TogglePartyMemberSelection(1), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection3, () => TogglePartyMemberSelection(2), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection4, () => TogglePartyMemberSelection(3), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection5, () => TogglePartyMemberSelection(4), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection6, () => TogglePartyMemberSelection(5), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection7, () => TogglePartyMemberSelection(6), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection8, () => TogglePartyMemberSelection(7), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.TogglePartySelection9, () => TogglePartyMemberSelection(8), CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.AssignGroup1, () => AssignGroup(0), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup2, () => AssignGroup(1), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup3, () => AssignGroup(2), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup4, () => AssignGroup(3), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup5, () => AssignGroup(4), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup6, () => AssignGroup(5), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup7, () => AssignGroup(6), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup8, () => AssignGroup(7), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.AssignGroup9, () => AssignGroup(8), CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.RecallGroup1, () => SelectGroup(0), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup2, () => SelectGroup(1), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup3, () => SelectGroup(2), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup4, () => SelectGroup(3), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup5, () => SelectGroup(4), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup6, () => SelectGroup(5), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup7, () => SelectGroup(6), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup8, () => SelectGroup(7), CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.RecallGroup9, () => SelectGroup(8), CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.SelectAll, SelectAll, CanTriggerOutOfCombatHotkeys);

            yield return new HotkeyAction(InGameHotKey.ToggleConsole, Tig.Console.ToggleVisible);

            yield return new HotkeyAction(InGameHotKey.CenterOnChar, CenterScreenOnParty, CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.ToggleMainMenu, ToggleMainMenu, CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.QuickLoad, QuickLoad, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.QuickSave, QuickSave, CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.Quit, QuitGame, CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.ShowInventory, ToggleInventory, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.ShowLogbook, ToggleLogbook, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.ShowMap, ToggleMap, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.ShowFormation, ToggleFormation, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.Rest, ToggleRest, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.ShowHelp, UiSystems.HelpManager.ClickForHelpToggle, CanTriggerHotkeys);
            yield return new HotkeyAction(InGameHotKey.ShowOptions, ToggleOptions, CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.ToggleCombat, ToggleCombat, CanTriggerHotkeys);

            yield return new HotkeyAction(InGameHotKey.EndTurn, EndTurn, CanTriggerTurnBasedPartyHotkey);

            // Add all user-assignable hotkeys for out-of-combat. In-combat is handled in the turn-based UI
            foreach (var userHotkey in InGameHotKey.UserAssignableHotkeys)
            {
                yield return new HotkeyAction(
                    userHotkey,
                    e => TriggerUserHotkey(userHotkey, e),
                    CanTriggerOutOfCombatHotkeys
                );
            }
        }

        private void TriggerUserHotkey(Hotkey hotkey, KeyboardEvent e)
        {
            if (e.IsCtrlHeld)
            {
                // assign hotkey
                var leader = GameSystems.Party.GetConsciousLeader();
                var viewport = GameViews.Primary;
                if (leader != null && viewport != null)
                {
                    UiSystems.RadialMenu.SpawnAndStartHotkeyAssignment(viewport, leader, hotkey);
                }
            }
            else if (GameSystems.D20.Hotkeys.IsAssigned(hotkey))
            {
                GameSystems.D20.Hotkeys.AddHotkeyActionToSequence(hotkey);
            }
        }

        private bool CanTriggerOutOfCombatHotkeys()
        {
            return CanTriggerHotkeys() && !GameSystems.Combat.IsCombatActive();
        }

        /// <summary>
        /// Condition for hotkeys that affect the current party-member in turn-based combat.
        /// </summary>
        private bool CanTriggerTurnBasedPartyHotkey()
        {
            if (!CanTriggerHotkeys() || !GameSystems.Combat.IsCombatActive())
            {
                return false;
            }

            var currentActor = GameSystems.D20.Initiative.CurrentActor;
            return currentActor != null
                   && GameSystems.Party.IsInParty(currentActor)
                   && !GameSystems.D20.Actions.IsCurrentlyPerforming(currentActor);
        }

        private bool CanTriggerHotkeys()
        {
            return !uiManagerDoYouWantToQuitActive;
        }

        [TempleDllLocation(0x10143190)]
        public void Reset()
        {
            uiManagerDoYouWantToQuitActive = false;
        }

        // End turn for current actor
        private void EndTurn()
        {
            // TODO: This should really not be needed here.
            UiSystems.CharSheet.CurrentPage = 0;
            UiSystems.CharSheet.Hide(UiSystems.CharSheet.State);

            // TODO: These conditions should prevent hotkey processing elsewhere!
            if (!UiSystems.InGameSelect.IsPicking && !UiSystems.RadialMenu.IsOpen)
            {
                var currentActor = GameSystems.D20.Initiative.CurrentActor;
                GameSystems.Combat.AdvanceTurn(currentActor);
                Logger.Info("Advancing turn for {0}", currentActor);
            }
        }

        [TempleDllLocation(0x101435b0)]
        public void ToggleCombat()
        {
            var leader = GameSystems.Party.GetConsciousLeader();
            if (!GameSystems.Combat.IsCombatActive())
            {
                GameSystems.Combat.EnterCombat(leader);
                GameSystems.Combat.StartCombat(leader, true);
                return;
            }

            if (GameSystems.Combat.AllCombatantsFarFromParty())
            {
                GameSystems.Combat.CritterLeaveCombat(leader);
            }
        }

        [TempleDllLocation(0x101436c0)]
        private void HideAll()
        {
            if (!UiSystems.Camping.IsHidden)
            {
                UiSystems.Camping.Hide();
            }

            if (UiSystems.CharSheet.HasCurrentCritter)
            {
                UiSystems.CharSheet.Hide(0);
            }

            if (UiSystems.Dialog.IsVisible)
            {
                UiSystems.Dialog.Hide();
            }

            if (UiSystems.Formation.IsVisible)
            {
                UiSystems.Formation.Hide();
            }

            if (UiSystems.Help.IsVisible)
            {
                UiSystems.Help.Hide();
            }

            if (UiSystems.Logbook.IsVisible)
            {
                UiSystems.Logbook.Hide();
            }

            if (UiSystems.Options.IsVisible)
            {
                UiSystems.Options.Hide();
            }

            if (UiSystems.TownMap.IsVisible)
            {
                UiSystems.TownMap.Hide();
            }

            if (UiSystems.WorldMap.IsVisible)
            {
                UiSystems.WorldMap.Hide();
            }

            GameSystems.D20.Actions.ResetCursor();
        }

        [TempleDllLocation(0x10143b40)]
        private void ToggleOptions()
        {
            if (UiSystems.Options.IsVisible)
            {
                UiSystems.Options.Hide();
                return;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.CharSheet.HasCurrentCritter
                || UiSystems.WorldMap.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.PCCreation.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return;
            }
            else
            {
                HideAll();
                UiSystems.Options.Show(false);
            }
        }

        [TempleDllLocation(0x10143ac0)]
        private void ToggleRest()
        {
            if (UiSystems.Camping.IsVisible)
            {
                UiSystems.Camping.Hide();
                return;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible
                || GameSystems.RandomEncounter.SleepStatus == SleepStatus.Impossible)
            {
                // TODO: UI event was blocked, notify user with sound
                return;
            }
            else
            {
                HideAll();
                UiSystems.Camping.Show();
            }
        }

        [TempleDllLocation(0x10143a40)]
        private void ToggleFormation()
        {
            if (UiSystems.Formation.IsVisible)
            {
                UiSystems.Formation.Hide();
                return;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return;
            }
            else
            {
                HideAll();
                UiSystems.Formation.Show();
            }
        }

        [TempleDllLocation(0x10143940)]
        private void ToggleMap()
        {
            if (UiSystems.TownMap.IsVisible || UiSystems.WorldMap.IsVisible)
            {
                if (UiSystems.TownMap.IsVisible)
                {
                    UiSystems.TownMap.Hide();
                    return;
                }

                if (UiSystems.WorldMap.IsVisible)
                {
                    UiSystems.WorldMap.Hide();
                    return;
                }
            }
            else if (!UiSystems.Dialog.IsVisible
                     && UiSystems.TownMap.IsTownMapAvailable
                     && !UiSystems.PartyPool.IsVisible
                     && !UiSystems.SaveGame.IsVisible
                     && !UiSystems.SaveGame.IsVisible
                     && !UiSystems.PCCreation.IsVisible
                     && !UiSystems.Options.IsVisible
                     && !UiSystems.Dialog.IsVisible
                     && !UiSystems.PartyPool.IsVisible
                     && !UiSystems.SaveGame.IsVisible
                     && !UiSystems.SaveGame.IsVisible
                     && !UiSystems.PCCreation.IsVisible
                     && !UiSystems.Options.IsVisible)
            {
                HideAll();
                UiSystems.TownMap.Show();
                return;
            }

            // TODO: UI event was blocked, notify user with sound
        }

        [TempleDllLocation(0x101438c0)]
        private void ToggleLogbook()
        {
            if (UiSystems.Logbook.IsVisible)
            {
                UiSystems.Logbook.Hide();
                return;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return;
            }
            else
            {
                HideAll();
                UiSystems.Logbook.Show();
            }
        }

        [TempleDllLocation(0x10143820)]
        private void ToggleInventory()
        {
            var leader = GameSystems.Party.GetConsciousLeader();
            if (GameSystems.Party.IsAiFollower(leader))
            {
                return;
            }

            if (UiSystems.CharSheet.HasCurrentCritter)
            {
                UiSystems.CharSheet.CurrentPage = 0;
                UiSystems.CharSheet.Hide(CharInventoryState.Closed);
                return;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return;
            }
            else
            {
                HideAll();
                UiSystems.CharSheet.Show(leader);
            }
        }

        [TempleDllLocation(0x101437d0)]
        private void QuitGame()
        {
            uiManagerDoYouWantToQuitActive = true;
            GameSystems.TimeEvent.PauseGameTime();
            GameSystems.D20.Actions.ResetCursor();
            UiSystems.Popup.ConfirmBox(
                QuitGameMessage,
                QuitGameTitle,
                true,
                QuitGameConfirm);
        }

        [TempleDllLocation(0x10143580)]
        private void QuitGameConfirm(int buttonIndex)
        {
            uiManagerDoYouWantToQuitActive = false;
            GameSystems.TimeEvent.ResumeGameTime();
            if (buttonIndex == 0)
            {
                Globals.GameLib.Reset();
                UiSystems.Reset();
                UiSystems.MainMenu.Show(0);
            }
        }

        [TempleDllLocation(0x10143560)]
        private void QuickSave()
        {
            if (!UiSystems.Dialog.IsVisible)
            {
                Globals.GameLib.QuickSave();
            }
        }

        [TempleDllLocation(0x10143530)]
        private void QuickLoad()
        {
            if (!Globals.GameLib.IsIronmanGame)
            {
                Globals.GameLib.QuickLoad();
                UiSystems.Party.UpdateAndShowMaybe();
                UiSystems.InGame.CenterOnParty();
            }
        }

        [TempleDllLocation(0x101434e0)]
        private void ToggleMainMenu()
        {
            if (UiSystems.MainMenu.IsVisible())
            {
                UiSystems.MainMenu.Hide();
                return;
            }

            if (Globals.GameLib.IsIronmanGame)
            {
                GameSystems.D20.Actions.ResetCursor();
                UiSystems.MainMenu.Show(MainMenuPage.InGameIronman);
                return;
            }

            GameSystems.D20.Actions.ResetCursor();
            UiSystems.MainMenu.Show(MainMenuPage.InGameNormal);
        }

        [TempleDllLocation(0x10143430)]
        private void CenterScreenOnParty()
        {
            UiSystems.InGame.CenterOnParty();
        }

        [TempleDllLocation(0x10143450)]
        [TemplePlusLocation("ui_legacysystems.cpp:1596")]
        private void SelectPartyMember(int index)
        {
            if (index >= GameSystems.Party.PartySize)
            {
                return;
            }

            if (UiSystems.Dialog.IsConversationOngoing) return;

            var critter = GameSystems.Party.GetPartyGroupMemberN(index);
            if (GameSystems.Party.IsAiFollower(critter)) return;

            var uiCharState = UiSystems.CharSheet.State;
            if (uiCharState == CharInventoryState.LevelUp)
            {
                return;
            }

            if (UiSystems.CharSheet.CurrentCritter != null)
            {
                UiSystems.CharSheet.ShowInState(uiCharState, critter);
                return;
            }

            GameSystems.Party.ClearSelection();
            GameSystems.Party.AddToSelection(critter);
        }

        [TempleDllLocation(0x10143310)]
        private void TogglePartyMemberSelection(int index)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                if (index < GameSystems.Party.PartySize)
                {
                    var partyMember = GameSystems.Party.GetPartyGroupMemberN(index);
                    if (!GameSystems.Party.IsAiFollower(partyMember))
                    {
                        if (GameSystems.Party.IsSelected(partyMember))
                        {
                            GameSystems.Party.RemoveFromSelection(partyMember);
                        }
                        else
                        {
                            GameSystems.Party.AddToSelection(partyMember);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10143380)]
        private void AssignGroup(int index)
        {
            GameSystems.Party.SaveSelection(index);
        }

        [TempleDllLocation(0x101433b0)]
        private void SelectGroup(int index)
        {
            GameSystems.Party.LoadSelection(index);
        }

        [TempleDllLocation(0x101433e0)]
        private void SelectAll()
        {
            GameSystems.Party.ClearSelection();
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                GameSystems.Party.AddToSelection(partyMember);
            }
        }
    }
}