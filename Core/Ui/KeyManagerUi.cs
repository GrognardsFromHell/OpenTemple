using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.MainMenu;

namespace OpenTemple.Core.Ui
{
    public class KeyManagerUi : IResetAwareSystem
    {
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

            var hotkeys = ImmutableList.CreateBuilder<HotkeyAction>();
        }

        public IEnumerable<HotkeyAction> EnumerateHotkeyActions()
        {
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
        }

        private bool CanTriggerOutOfCombatHotkeys()
        {
            return CanTriggerHotkeys() && !GameSystems.Combat.IsCombatActive();
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

        [TempleDllLocation(0x10143d60)]
        public void HandleKeyDown(KeyboardEvent e)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10143d60)]
        public void HandleKeyUp(KeyboardEvent e)
        {
            Stub.TODO();

            // var modifier = GetKeyEventFromModifier();
            // var evt = GetKeyEvent(kbMsg, modifier);
            // var action = evt;
            //
            // switch (action)
            // {


            //     case InGameHotKey.CenterOnChar:
            //         return CenterScreenOnParty(kbMsg, modifier, action);
            //     case InGameHotKey.SelectChar1:
            //     case InGameHotKey.SelectChar2:
            //     case InGameHotKey.SelectChar3:
            //     case InGameHotKey.SelectChar4:
            //     case InGameHotKey.SelectChar5:
            //     case InGameHotKey.SelectChar6:
            //     case InGameHotKey.SelectChar7:
            //     case InGameHotKey.SelectChar8:
            //         return SelectPartyMember(kbMsg, modifier, action);
            //     case InGameHotKey.ToggleMainMenu:
            //         return ToggleMainMenu();
            //     case InGameHotKey.QuickLoad:
            //         return QuickLoad();
            //     case InGameHotKey.QuickSave:
            //         return QuickSave();
            //     case InGameHotKey.Quit:
            //         return QuitGame();
            //     case InGameHotKey.ShowInventory:
            //         return ToggleInventory();
            //     case InGameHotKey.ShowLogbook:
            //         return ToggleLogbook();
            //     case InGameHotKey.ShowMap:
            //         return ToggleMap();
            //     case InGameHotKey.ShowFormation:
            //         return ToggleFormation();
            //     case InGameHotKey.Rest:
            //         return ToggleRest();
            //     case InGameHotKey.ShowHelp:
            //         if (!uiManagerDoYouWantToQuitActive)
            //         {
            //             UiSystems.HelpManager.ClickForHelpToggle();
            //         }
            //
            //         return true;
            //     // case InGameHotKey.0:
            //     // case InGameHotKey.1:
            //     case InGameHotKey.Screenshot:
            //     case InGameHotKey.ObjectHighlight:
            //         return true;
            //     case InGameHotKey.ShowOptions:
            //         return ToggleOptions();
            //     case InGameHotKey.ToggleCombat:
            //         return ToggleCombat();
            //     case InGameHotKey.EndTurn:
            //     case InGameHotKey.EndTurnNonParty:
            //         return true;
            //     default:
            //         return false;
            // }            
        }

        [TempleDllLocation(0x101435b0)]
        public bool ToggleCombat()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            var leader = GameSystems.Party.GetConsciousLeader();
            if (!GameSystems.Combat.IsCombatActive())
            {
                GameSystems.Combat.EnterCombat(leader);
                GameSystems.Combat.StartCombat(leader, true);
                return true;
            }

            if (GameSystems.Combat.AllCombatantsFarFromParty())
            {
                GameSystems.Combat.CritterLeaveCombat(leader);
                return true;
            }

            return false;
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
        private bool ToggleOptions()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            if (UiSystems.Options.IsVisible)
            {
                UiSystems.Options.Hide();
                return true;
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
                return false;
            }
            else
            {
                HideAll();
                UiSystems.Options.Show(false);
                return true;
            }
        }

        [TempleDllLocation(0x10143ac0)]
        private bool ToggleRest()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            if (UiSystems.Camping.IsVisible)
            {
                UiSystems.Camping.Hide();
                return true;
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
                return false;
            }
            else
            {
                HideAll();
                UiSystems.Camping.Show();
                return true;
            }
        }

        [TempleDllLocation(0x10143a40)]
        private bool ToggleFormation()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            if (UiSystems.Formation.IsVisible)
            {
                UiSystems.Formation.Hide();
                return true;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return false;
            }
            else
            {
                HideAll();
                UiSystems.Formation.Show();
                return true;
            }
        }

        [TempleDllLocation(0x10143940)]
        private bool ToggleMap()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            if (UiSystems.TownMap.IsVisible || UiSystems.WorldMap.IsVisible)
            {
                if (UiSystems.TownMap.IsVisible)
                {
                    UiSystems.TownMap.Hide();
                    return true;
                }

                if (UiSystems.WorldMap.IsVisible)
                {
                    UiSystems.WorldMap.Hide();
                    return true;
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
                return true;
            }

            // TODO: UI event was blocked, notify user with sound
            return false;
        }

        [TempleDllLocation(0x101438c0)]
        private bool ToggleLogbook()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            if (UiSystems.Logbook.IsVisible)
            {
                UiSystems.Logbook.Hide();
                return true;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return false;
            }
            else
            {
                HideAll();
                UiSystems.Logbook.Show();
                return true;
            }
        }

        [TempleDllLocation(0x10143820)]
        private bool ToggleInventory()
        {
            if (uiManagerDoYouWantToQuitActive)
            {
                return true;
            }

            var leader = GameSystems.Party.GetConsciousLeader();
            if (GameSystems.Party.IsAiFollower(leader))
            {
                return true;
            }

            if (UiSystems.CharSheet.HasCurrentCritter)
            {
                UiSystems.CharSheet.CurrentPage = 0;
                UiSystems.CharSheet.Hide(CharInventoryState.Closed);
                return true;
            }

            if (UiSystems.Dialog.IsVisible
                || UiSystems.PartyPool.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.SaveGame.IsVisible
                || UiSystems.PCCreation.IsVisible
                || UiSystems.Options.IsVisible)
            {
                // TODO: UI event was blocked, notify user with sound
                return false;
            }
            else
            {
                HideAll();
                UiSystems.CharSheet.Show(leader);
                return true;
            }
        }

        [TempleDllLocation(0x101437d0)]
        private bool QuitGame()
        {
            if (!uiManagerDoYouWantToQuitActive)
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

            return true;
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
        private bool QuickSave()
        {
            if (!uiManagerDoYouWantToQuitActive && !UiSystems.Dialog.IsVisible)
            {
                Globals.GameLib.QuickSave();
            }

            return true;
        }

        [TempleDllLocation(0x10143530)]
        private bool QuickLoad()
        {
            if (!uiManagerDoYouWantToQuitActive && !Globals.GameLib.IsIronmanGame)
            {
                Globals.GameLib.QuickLoad();
                UiSystems.Party.UpdateAndShowMaybe();
                UiSystems.InGame.CenterOnParty();
            }

            return true;
        }

        [TempleDllLocation(0x101434e0)]
        private bool ToggleMainMenu()
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                if (UiSystems.MainMenu.IsVisible())
                {
                    UiSystems.MainMenu.Hide();
                    return true;
                }

                if (Globals.GameLib.IsIronmanGame)
                {
                    GameSystems.D20.Actions.ResetCursor();
                    UiSystems.MainMenu.Show(MainMenuPage.InGameIronman);
                    return true;
                }

                GameSystems.D20.Actions.ResetCursor();
                UiSystems.MainMenu.Show(MainMenuPage.InGameNormal);
            }

            return true;
        }

        [TempleDllLocation(0x10143430)]
        private bool CenterScreenOnParty(Hotkey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                UiSystems.InGame.CenterOnParty();
            }

            return true;
        }

        [TempleDllLocation(0x10143450)]
        [TemplePlusLocation("ui_legacysystems.cpp:1596")]
        private bool SelectPartyMember(Hotkey action)
        {
            if (uiManagerDoYouWantToQuitActive)
                return true;

            int index;
            if (action == InGameHotKey.SelectChar1)
                index = 0;
            else if (action == InGameHotKey.SelectChar2)
                index = 1;
            else if (action == InGameHotKey.SelectChar3)
                index = 2;
            else if (action == InGameHotKey.SelectChar4)
                index = 3;
            else if (action == InGameHotKey.SelectChar5)
                index = 4;
            else if (action == InGameHotKey.SelectChar6)
                index = 5;
            else if (action == InGameHotKey.SelectChar7)
                index = 6;
            else if (action == InGameHotKey.SelectChar8)
                index = 7;
            else if (action == InGameHotKey.SelectChar9)
                index = 8;
            else
                throw new ArgumentOutOfRangeException(nameof(action));

            if (index >= GameSystems.Party.PartySize || GameSystems.Combat.IsCombatActive())
            {
                return true;
            }

            if (UiSystems.Dialog.IsConversationOngoing)
                return true;

            var critter = GameSystems.Party.GetPartyGroupMemberN(index);
            if (GameSystems.Party.IsAiFollower(critter))
                return true;

            var uiCharState = UiSystems.CharSheet.State;
            if (uiCharState == CharInventoryState.LevelUp)
            {
                return true;
            }


            if (UiSystems.CharSheet.CurrentCritter != null)
            {
                UiSystems.CharSheet.ShowInState(uiCharState, critter);
                return true;
            }

            GameSystems.Party.ClearSelection();
            GameSystems.Party.AddToSelection(critter);
            return true;
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