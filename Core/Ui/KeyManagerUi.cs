using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.IO;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.MainMenu;

namespace OpenTemple.Core.Ui
{
    public class KeyManagerUi : IDisposable, IResetAwareSystem
    {
        [TempleDllLocation(0x10BE8CF0)]
        private bool _modalIsOpen;

        // TODO 1.0: This seems to never be set to anything but 0 in Vanilla
        [field:TempleDllLocation(0x10be8cf4)]
        [TempleDllLocation(0x101431e0)]
        [TempleDllLocation(0x101431d0)]
        public int InputState { get; set; } = 0;

        [TempleDllLocation(0x10be7018)]
        private readonly Dictionary<int, string> _translations;

        [TempleDllLocation(0x10be701c)]
        private string QuitGameMessage => _translations[0];

        [TempleDllLocation(0x10be8cec)]
        private string QuitGameTitle => _translations[1];

        [TempleDllLocation(0x10be7020)]
        private readonly HotKeySpec[] keyConfigEntries = new HotKeySpec[61 + 2];

        [TempleDllLocation(0x10be7900)]
        private readonly Dictionary<DIK, List<InGameHotKey>> keyconfigs = new Dictionary<DIK, List<InGameHotKey>>();

        [TempleDllLocation(0x10be8cf0)]
        private bool uiManagerDoYouWantToQuitActive = false;

        [TempleDllLocation(0x10143bd0)]
        public KeyManagerUi()
        {
            _modalIsOpen = false;
            _translations = Tig.FS.ReadMesFile("mes/ui_manager.mes");

            InitKeyConfigEntries();
        }

        private void InitKeyConfigEntries()
        {
            for (var i = 0; i < keyConfigEntries.Length; i++)
            {
                keyConfigEntries[i].eventId = InGameHotKey.None;
            }

            keyConfigEntries[61].eventId = InGameHotKey.None;
            keyConfigEntries[62].eventId = InGameHotKey.None;

            foreach (var defaultHotKey in DefaultHotKeys.Entries)
            {
                UiManagerRefreshKeyConfig(defaultHotKey);
            }
        }

        [TempleDllLocation(0x10143270)]
        private void UiManagerRefreshKeyConfig(HotKeySpec kcEvt)
        {
            void AddKeyMapping(DIK keyCode, InGameHotKey hotKey)
            {
                if (keyCode != 0)
                {
                    if (!keyconfigs.TryGetValue(keyCode, out var mappedActions))
                    {
                        mappedActions = new List<InGameHotKey>();
                        keyconfigs[keyCode] = mappedActions;
                    }

                    mappedActions.Add(hotKey);
                }
            }

            if (kcEvt.primaryKeyCode != 0)
            {
                AddKeyMapping(kcEvt.primaryKeyCode, kcEvt.eventId);
            }

            if (kcEvt.secondaryKeyCode != 0)
            {
                AddKeyMapping(kcEvt.secondaryKeyCode, kcEvt.eventId);
            }

            keyConfigEntries[(int) kcEvt.eventId] = kcEvt;
        }

        [TempleDllLocation(0x101431a0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10143190)]
        public void Reset()
        {
            uiManagerDoYouWantToQuitActive = false;
            InputState = 0;
        }

        [TempleDllLocation(0x10143750)]
        private HotKeyModifier GetKeyEventFromModifier()
        {
            var result = HotKeyModifier.None;
            if (Tig.Keyboard.IsPressed(DIK.DIK_LCONTROL) || Tig.Keyboard.IsPressed(DIK.DIK_RCONTROL))
            {
                result |= HotKeyModifier.Ctrl;
            }

            if (Tig.Keyboard.IsPressed(DIK.DIK_LSHIFT) || Tig.Keyboard.IsPressed(DIK.DIK_RSHIFT))
            {
                result |= HotKeyModifier.Shift;
            }

            if (Tig.Keyboard.IsPressed(DIK.DIK_LMENU) || Tig.Keyboard.IsPressed(DIK.DIK_RMENU))
            {
                result |= HotKeyModifier.Alt;
            }

            return result;
        }

        [TempleDllLocation(0x101431f0)]
        private InGameHotKey GetKeyEvent(MessageKeyStateChangeArgs keyArgs, HotKeyModifier modifierKey)
        {
            if (!keyconfigs.TryGetValue(keyArgs.key, out var mappedEvents))
            {
                return InGameHotKey.None;
            }

            foreach (var mappedEvent in mappedEvents)
            {
                var eventConfig = keyConfigEntries[(int) mappedEvent];
                if (eventConfig.primaryKeyCode == keyArgs.key && eventConfig.primaryOnDown == keyArgs.down
                    || eventConfig.secondaryKeyCode == keyArgs.key && eventConfig.secondaryOnDown == keyArgs.down)
                {
                    if (eventConfig.modifier == modifierKey)
                    {
                        return mappedEvent;
                    }
                }
            }

            return InGameHotKey.None;
        }

        private bool DontHandle(InGameHotKey evtName)
        {
            if (evtName <= InGameHotKey.ObjectHighlight)
            {
                return InputState == 1;
            }

            if (evtName == InGameHotKey.ShowHelp)
            {
                return false;
            }

            if (evtName == InGameHotKey.Rest)
            {
                return (InputState == 1 || InputState == 2);
            }

            if (evtName <= InGameHotKey.EndTurnNonParty)
            {
                return InputState == 1;
            }

            return false;
        }

        [TempleDllLocation(0x10143d60)]
        public bool HandleKeyEvent(MessageKeyStateChangeArgs kbMsg)
        {
            var modifier = GetKeyEventFromModifier();
            var evt = GetKeyEvent(kbMsg, modifier);
            var action = evt;

            if (DontHandle(action))
            {
                return false;
            }

            switch (action)
            {
                case InGameHotKey.TogglePartySelection1:
                case InGameHotKey.TogglePartySelection2:
                case InGameHotKey.TogglePartySelection3:
                case InGameHotKey.TogglePartySelection4:
                case InGameHotKey.TogglePartySelection5:
                case InGameHotKey.TogglePartySelection6:
                case InGameHotKey.TogglePartySelection7:
                case InGameHotKey.TogglePartySelection8:
                    return TogglePartyMemberSelection(kbMsg, modifier, action);
                case InGameHotKey.AssignGroup1:
                case InGameHotKey.AssignGroup2:
                case InGameHotKey.AssignGroup3:
                case InGameHotKey.AssignGroup4:
                case InGameHotKey.AssignGroup5:
                case InGameHotKey.AssignGroup6:
                case InGameHotKey.AssignGroup7:
                case InGameHotKey.AssignGroup8:
                    return AssignGroup(kbMsg, modifier, action);
                case InGameHotKey.RecallGroup1:
                case InGameHotKey.RecallGroup2:
                case InGameHotKey.RecallGroup3:
                case InGameHotKey.RecallGroup4:
                case InGameHotKey.RecallGroup5:
                case InGameHotKey.RecallGroup6:
                case InGameHotKey.RecallGroup7:
                case InGameHotKey.RecallGroup8:
                    return SelectGroup(kbMsg, modifier, action);
                case InGameHotKey.SelectAll:
                    return SelectAll(kbMsg, modifier, action);
                case InGameHotKey.ToggleConsole:
                    Tig.Console.ToggleVisible();
                    return true;
                case InGameHotKey.CenterOnChar:
                    return CenterScreenOnParty(kbMsg, modifier, action);
                case InGameHotKey.SelectChar1:
                case InGameHotKey.SelectChar2:
                case InGameHotKey.SelectChar3:
                case InGameHotKey.SelectChar4:
                case InGameHotKey.SelectChar5:
                case InGameHotKey.SelectChar6:
                case InGameHotKey.SelectChar7:
                case InGameHotKey.SelectChar8:
                    return SelectPartyMember(kbMsg, modifier, action);
                case InGameHotKey.ToggleMainMenu:
                    return ToggleMainMenu();
                case InGameHotKey.QuickLoad:
                    return QuickLoad();
                case InGameHotKey.QuickSave:
                    return QuickSave();
                case InGameHotKey.Quit:
                    return QuitGame();
                case InGameHotKey.ShowInventory:
                    return ToggleInventory();
                case InGameHotKey.ShowLogbook:
                    return ToggleLogbook();
                case InGameHotKey.ShowMap:
                    return ToggleMap();
                case InGameHotKey.ShowFormation:
                    return ToggleFormation();
                case InGameHotKey.Rest:
                    return ToggleRest();
                case InGameHotKey.ShowHelp:
                    if (!uiManagerDoYouWantToQuitActive)
                    {
                        UiSystems.HelpManager.ClickForHelpToggle();
                    }

                    return true;
                // case InGameHotKey.0:
                // case InGameHotKey.1:
                case InGameHotKey.Screenshot:
                case InGameHotKey.ObjectHighlight:
                    return true;
                case InGameHotKey.ShowOptions:
                    return ToggleOptions();
                case InGameHotKey.ToggleCombat:
                    return ToggleCombat();
                case InGameHotKey.EndTurn:
                case InGameHotKey.EndTurnNonParty:
                    return true;
                default:
                    return false;
            }
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
            if ( !UiSystems.Camping.IsHidden )
            {
                UiSystems.Camping.Hide();
            }
            if ( UiSystems.CharSheet.HasCurrentCritter )
            {
                UiSystems.CharSheet.Hide(0);
            }
            if ( UiSystems.Dialog.IsVisible )
            {
                UiSystems.Dialog.Hide();
            }
            if ( UiSystems.Formation.IsVisible )
            {
                UiSystems.Formation.Hide();
            }
            if ( UiSystems.Help.IsVisible )
            {
                UiSystems.Help.Hide();
            }
            if ( UiSystems.Logbook.IsVisible )
            {
                UiSystems.Logbook.Hide();
            }
            if ( UiSystems.Options.IsVisible )
            {
                UiSystems.Options.Hide();
            }
            if ( UiSystems.TownMap.IsVisible )
            {
                UiSystems.TownMap.Hide();
            }
            if ( UiSystems.WorldMap.IsVisible )
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
        private bool CenterScreenOnParty(MessageKeyStateChangeArgs kbMsg, HotKeyModifier modifier, InGameHotKey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                UiSystems.InGame.CenterOnParty();
            }

            return true;
        }

        [TempleDllLocation(0x10143450)]
        [TemplePlusLocation("ui_legacysystems.cpp:1596")]
        private bool SelectPartyMember(MessageKeyStateChangeArgs kbMsg, HotKeyModifier modifier, InGameHotKey action)
        {
            if (uiManagerDoYouWantToQuitActive)
                return true;

            var charNumber = action - InGameHotKey.SelectChar1;
            if (charNumber >= GameSystems.Party.PartySize || GameSystems.Combat.IsCombatActive())
            {
                return true;
            }

            if (UiSystems.Dialog.IsConversationOngoing)
                return true;

            var dude = GameSystems.Party.GetPartyGroupMemberN(charNumber);
            if (GameSystems.Party.IsAiFollower(dude))
                return true;

            var uiCharState = UiSystems.CharSheet.State;
            if (uiCharState == CharInventoryState.LevelUp)
            {
                return true;
            }


            if (UiSystems.CharSheet.CurrentCritter != null)
            {
                UiSystems.CharSheet.ShowInState(uiCharState, dude);
                return true;
            }

            GameSystems.Party.ClearSelection();
            GameSystems.Party.AddToSelection(dude);
            return true;
        }

        [TempleDllLocation(0x10143310)]
        private bool TogglePartyMemberSelection(MessageKeyStateChangeArgs keyArgs, HotKeyModifier modifiers,
            InGameHotKey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                var partyIndex = action - InGameHotKey.TogglePartySelection1;
                if (partyIndex < GameSystems.Party.PartySize)
                {
                    var partyMember = GameSystems.Party.GetPartyGroupMemberN(partyIndex);
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
        private bool AssignGroup(MessageKeyStateChangeArgs keyArgs, HotKeyModifier modifiers, InGameHotKey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                var groupIndex = action - InGameHotKey.AssignGroup1;
                GameSystems.Party.SaveSelection(groupIndex);
            }

            return true;
        }

        [TempleDllLocation(0x101433b0)]
        private bool SelectGroup(MessageKeyStateChangeArgs keyArgs, HotKeyModifier modifiers, InGameHotKey action)
        {
            if (!uiManagerDoYouWantToQuitActive)
            {
                var groupIndex = action - InGameHotKey.AssignGroup1;
                GameSystems.Party.LoadSelection(groupIndex);
            }

            return true;
        }

        [TempleDllLocation(0x101433e0)]
        private bool SelectAll(MessageKeyStateChangeArgs keyArgs, HotKeyModifier modifiers, InGameHotKey action)
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

    [Flags]
    internal enum HotKeyModifier
    {
        None,
        Ctrl, // DIK_LCONTROL
        Shift, // DIK_LSHIFT
        Alt // DIK_LMENU
    };

    internal struct HotKeySpec
    {
        public bool unkBool; // TODO Unused?
        public bool unkBool2; // TODO Unused?
        public HotKeyModifier modifier;
        public InGameHotKey eventId;
        public DIK primaryKeyCode;
        public bool primaryOnDown;
        public DIK secondaryKeyCode;
        public bool secondaryOnDown;

        public HotKeySpec(bool unkBool, bool unkBool2, HotKeyModifier modifier, InGameHotKey eventId,
            DIK primaryKeyCode, bool primaryOnDown)
        {
            this.unkBool = unkBool;
            this.unkBool2 = unkBool2;
            this.modifier = modifier;
            this.eventId = eventId;
            this.primaryKeyCode = primaryKeyCode;
            this.primaryOnDown = primaryOnDown;
            this.secondaryKeyCode = 0;
            this.secondaryOnDown = false;
        }

        public HotKeySpec(bool unkBool, bool unkBool2, HotKeyModifier modifier, InGameHotKey eventId,
            DIK primaryKeyCode, bool primaryOnDown, DIK secondaryKeyCode, bool secondaryOnDown)
        {
            this.unkBool = unkBool;
            this.unkBool2 = unkBool2;
            this.modifier = modifier;
            this.eventId = eventId;
            this.primaryKeyCode = primaryKeyCode;
            this.primaryOnDown = primaryOnDown;
            this.secondaryKeyCode = secondaryKeyCode;
            this.secondaryOnDown = secondaryOnDown;
        }
    }

    internal static class DefaultHotKeys
    {
        [TempleDllLocation(0x102f9c88)]
        internal static readonly IImmutableList<HotKeySpec> Entries = new List<HotKeySpec>
        {
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection1, DIK.DIK_1, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection2, DIK.DIK_2, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection3, DIK.DIK_3, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection4, DIK.DIK_4, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection5, DIK.DIK_5, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection6, DIK.DIK_6, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection7, DIK.DIK_7, false),
            new HotKeySpec(true, false, HotKeyModifier.Shift, InGameHotKey.TogglePartySelection8, DIK.DIK_8, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup1, DIK.DIK_F1, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup2, DIK.DIK_F2, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup3, DIK.DIK_F3, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup4, DIK.DIK_F4, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup5, DIK.DIK_F5, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup6, DIK.DIK_F6, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup7, DIK.DIK_F7, false),
            new HotKeySpec(true, false, HotKeyModifier.Ctrl, InGameHotKey.AssignGroup8, DIK.DIK_F8, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup1, DIK.DIK_F1, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup2, DIK.DIK_F2, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup3, DIK.DIK_F3, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup4, DIK.DIK_F4, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup5, DIK.DIK_F5, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup6, DIK.DIK_F6, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup7, DIK.DIK_F7, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.RecallGroup8, DIK.DIK_F8, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.SelectAll, DIK.DIK_GRAVE, false),
            new HotKeySpec(false, false, HotKeyModifier.Shift, InGameHotKey.ToggleConsole, DIK.DIK_GRAVE, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.CenterOnChar, DIK.DIK_HOME, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar1, DIK.DIK_1, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar2, DIK.DIK_2, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar3, DIK.DIK_3, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar4, DIK.DIK_4, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar5, DIK.DIK_5, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar6, DIK.DIK_6, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar7, DIK.DIK_7, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.SelectChar8, DIK.DIK_8, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ToggleMainMenu, DIK.DIK_ESCAPE, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.QuickLoad, DIK.DIK_F9, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.QuickSave, DIK.DIK_F12, false, DIK.DIK_F11,
                false),
            new HotKeySpec(false, false, HotKeyModifier.Alt, InGameHotKey.Quit, DIK.DIK_Q, false),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.Screenshot, DIK.DIK_SYSRQ,
                false), // (print screen)
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollUp, DIK.DIK_UP, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollDown, DIK.DIK_DOWN, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollLeft, DIK.DIK_LEFT, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollRight, DIK.DIK_RIGHT, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollUpArrow, DIK.DIK_UP, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollDownArrow, DIK.DIK_DOWN, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollLeftArrow, DIK.DIK_LEFT, true),
            new HotKeySpec(true, false, HotKeyModifier.None, InGameHotKey.ScrollRightArrow, DIK.DIK_RIGHT, true),
            new HotKeySpec(true, false, HotKeyModifier.Alt, InGameHotKey.ObjectHighlight, DIK.DIK_TAB, true),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ShowInventory, DIK.DIK_I, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ShowLogbook, DIK.DIK_L, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ShowMap, DIK.DIK_M, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ShowFormation, DIK.DIK_F, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.Rest, DIK.DIK_R, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ShowHelp, DIK.DIK_H, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ShowOptions, DIK.DIK_O, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.ToggleCombat, DIK.DIK_C, false),
            new HotKeySpec(false, false, HotKeyModifier.None, InGameHotKey.EndTurn, DIK.DIK_SPACE, false,
                DIK.DIK_RETURN, false),
            new HotKeySpec(true, true, HotKeyModifier.Shift, InGameHotKey.EndTurnNonParty, DIK.DIK_RETURN, false),
        }.ToImmutableList();
    }

    internal enum InGameHotKey
    {
        /*
            Toggles the corresponding party member selection
        */
        TogglePartySelection1 = 2,
        TogglePartySelection2 = 3,
        TogglePartySelection3 = 4,
        TogglePartySelection4 = 5,
        TogglePartySelection5 = 6,
        TogglePartySelection6 = 7,
        TogglePartySelection7 = 8,
        TogglePartySelection8 = 9,
        AssignGroup1 = 10,
        AssignGroup2 = 11,
        AssignGroup3 = 12,
        AssignGroup4 = 13,
        AssignGroup5 = 14,
        AssignGroup6 = 15,
        AssignGroup7 = 16,
        AssignGroup8 = 17,
        RecallGroup1 = 18,
        RecallGroup2 = 19,
        RecallGroup3 = 20,
        RecallGroup4 = 21,
        RecallGroup5 = 22,
        RecallGroup6 = 23,
        RecallGroup7 = 24,
        RecallGroup8 = 25,
        SelectAll = 26,
        ToggleConsole = 27,
        CenterOnChar = 28,
        SelectChar1 = 29,
        SelectChar2 = 30,
        SelectChar3 = 31,
        SelectChar4 = 32,
        SelectChar5 = 33,
        SelectChar6 = 34,
        SelectChar7 = 35,
        SelectChar8 = 36,
        ToggleMainMenu = 37,
        QuickLoad = 38,
        QuickSave = 39,
        Quit = 40,
        Screenshot = 41, // Handled elsewhere, but swallowed
        ScrollUp = 42, // Handled elsewhere
        ScrollDown = 43, // Handled elsewhere
        ScrollLeft = 44, // Handled elsewhere
        ScrollRight = 45, // Handled elsewhere
        ScrollUpArrow = 46, // Handled elsewhere
        ScrollDownArrow = 47, // Handled elsewhere
        ScrollLeftArrow = 48, // Handled elsewhere
        ScrollRightArrow = 49, // Handled elsewhere
        ObjectHighlight = 50, // Handled elsewhere (but swallowed)
        ShowInventory = 51,
        ShowLogbook = 52,
        ShowMap = 53,
        ShowFormation = 54,
        Rest = 55,
        ShowHelp = 56,
        ShowOptions = 57,
        ToggleCombat = 58,
        EndTurn = 59,
        EndTurnNonParty = 60,
        None = 62
    }
}