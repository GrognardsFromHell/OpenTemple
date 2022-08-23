using System;
using System.Collections.Generic;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.MainMenu;

namespace OpenTemple.Core.Ui
{
    public class KeyManagerUi : IResetAwareSystem
    {
        [TempleDllLocation(0x10BE8CF0)]
        private bool _modalIsOpen;

        // TODO 1.0: This seems to never be set to anything but 0 in Vanilla
        [field: TempleDllLocation(0x10be8cf4)]
        [TempleDllLocation(0x101431e0)]
        [TempleDllLocation(0x101431d0)]
        public int InputState { get; set; } = 0;

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

        [TempleDllLocation(0x10143190)]
        public void Reset()
        {
            uiManagerDoYouWantToQuitActive = false;
            InputState = 0;
        }

        [TempleDllLocation(0x10143d60)]
        public bool HandleKeyEvent(MessageKeyStateChangeArgs kbMsg)
        {
            return false;

            // var modifier = GetKeyEventFromModifier();
            // var evt = GetKeyEvent(kbMsg, modifier);
            // var action = evt;
            //
            // switch (action)
            // {
            //     case InGameHotKey.TogglePartySelection1:
            //     case InGameHotKey.TogglePartySelection2:
            //     case InGameHotKey.TogglePartySelection3:
            //     case InGameHotKey.TogglePartySelection4:
            //     case InGameHotKey.TogglePartySelection5:
            //     case InGameHotKey.TogglePartySelection6:
            //     case InGameHotKey.TogglePartySelection7:
            //     case InGameHotKey.TogglePartySelection8:
            //         return TogglePartyMemberSelection(kbMsg, modifier, action);
            //     case InGameHotKey.AssignGroup1:
            //     case InGameHotKey.AssignGroup2:
            //     case InGameHotKey.AssignGroup3:
            //     case InGameHotKey.AssignGroup4:
            //     case InGameHotKey.AssignGroup5:
            //     case InGameHotKey.AssignGroup6:
            //     case InGameHotKey.AssignGroup7:
            //     case InGameHotKey.AssignGroup8:
            //         return AssignGroup(kbMsg, modifier, action);
            //     case InGameHotKey.RecallGroup1:
            //     case InGameHotKey.RecallGroup2:
            //     case InGameHotKey.RecallGroup3:
            //     case InGameHotKey.RecallGroup4:
            //     case InGameHotKey.RecallGroup5:
            //     case InGameHotKey.RecallGroup6:
            //     case InGameHotKey.RecallGroup7:
            //     case InGameHotKey.RecallGroup8:
            //         return SelectGroup(kbMsg, modifier, action);
            //     case InGameHotKey.SelectAll:
            //         return SelectAll(kbMsg, modifier, action);
            //     case InGameHotKey.ToggleConsole:
            //         Tig.Console.ToggleVisible();
            //         return true;
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
        private bool TogglePartyMemberSelection(Hotkey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                int index;
                if (action == InGameHotKey.TogglePartySelection1)
                    index = 0;
                else if (action == InGameHotKey.TogglePartySelection2)
                    index = 1;
                else if (action == InGameHotKey.TogglePartySelection3)
                    index = 2;
                else if (action == InGameHotKey.TogglePartySelection4)
                    index = 3;
                else if (action == InGameHotKey.TogglePartySelection5)
                    index = 4;
                else if (action == InGameHotKey.TogglePartySelection6)
                    index = 5;
                else if (action == InGameHotKey.TogglePartySelection7)
                    index = 6;
                else if (action == InGameHotKey.TogglePartySelection8)
                    index = 7;
                else if (action == InGameHotKey.TogglePartySelection9)
                    index = 8;
                else
                    throw new ArgumentOutOfRangeException(nameof(action));
                
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

            return true;
        }

        [TempleDllLocation(0x10143380)]
        private bool AssignGroup(Hotkey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                int index;
                if (action == InGameHotKey.AssignGroup1)
                    index = 0;
                else if (action == InGameHotKey.AssignGroup2)
                    index = 1;
                else if (action == InGameHotKey.AssignGroup3)
                    index = 2;
                else if (action == InGameHotKey.AssignGroup4)
                    index = 3;
                else if (action == InGameHotKey.AssignGroup5)
                    index = 4;
                else if (action == InGameHotKey.AssignGroup6)
                    index = 5;
                else if (action == InGameHotKey.AssignGroup7)
                    index = 6;
                else if (action == InGameHotKey.AssignGroup8)
                    index = 7;
                else if (action == InGameHotKey.AssignGroup9)
                    index = 8;
                else
                    throw new ArgumentOutOfRangeException(nameof(action));
                
                GameSystems.Party.SaveSelection(index);
            }

            return true;
        }

        [TempleDllLocation(0x101433b0)]
        private bool SelectGroup(Hotkey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                int index;
                if (action == InGameHotKey.RecallGroup1)
                    index = 0;
                else if (action == InGameHotKey.RecallGroup2)
                    index = 1;
                else if (action == InGameHotKey.RecallGroup3)
                    index = 2;
                else if (action == InGameHotKey.RecallGroup4)
                    index = 3;
                else if (action == InGameHotKey.RecallGroup5)
                    index = 4;
                else if (action == InGameHotKey.RecallGroup6)
                    index = 5;
                else if (action == InGameHotKey.RecallGroup7)
                    index = 6;
                else if (action == InGameHotKey.RecallGroup8)
                    index = 7;
                else if (action == InGameHotKey.RecallGroup9)
                    index = 8;
                else
                    throw new ArgumentOutOfRangeException(nameof(action));

                GameSystems.Party.LoadSelection(index);
            }

            return true;
        }

        [TempleDllLocation(0x101433e0)]
        private bool SelectAll(Hotkey action)
        {
            if (!uiManagerDoYouWantToQuitActive && !GameSystems.Combat.IsCombatActive())
            {
                GameSystems.Party.ClearSelection();
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    GameSystems.Party.AddToSelection(partyMember);
                }
            }

            return true;
        }
    }
}