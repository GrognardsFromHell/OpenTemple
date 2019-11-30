using System;
using System.Collections.Generic;
using SpicyTemple.Core.IO.SaveGames.UiState;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;

namespace SpicyTemple.Core.Ui
{
    public class HelpManagerUi : IResetAwareSystem, ISaveGameAwareUi
    {
        private readonly Dictionary<TutorialTopic, string> _tutorialTopicNames = new Dictionary<TutorialTopic, string>
        {
            {TutorialTopic.RestCamp, "TAG_TUT_REST_CAMP"},
            {TutorialTopic.Portraits, "TAG_TUT_PORTRAITS"},
            {TutorialTopic.OpenDoor, "TAG_TUT_OPEN_DOOR"},
            {TutorialTopic.OpenChest, "TAG_TUT_OPEN_CHEST"},
            {TutorialTopic.Inventory, "TAG_TUT_INVENTORY"},
            {TutorialTopic.Dialogue, "TAG_TUT_DIALOGUE"},
            {TutorialTopic.Movement, "TAG_TUT_MOVEMENT"},
            {TutorialTopic.CombatActionBar, "TAG_TUT_COMBAT_ACTION_BAR"},
            {TutorialTopic.SelectCharacter, "TAG_TUT_SELECT_CHARACTER"},
            {TutorialTopic.CastSpells, "TAG_TUT_CAST_SPELLS"},
            {TutorialTopic.CombatInitiativeBar, "TAG_TUT_COMBAT_INITIATIVE_BAR"},
            {TutorialTopic.CombatAttacking, "TAG_TUT_COMBAT_ATTACKING"},
            {TutorialTopic.Keys, "TAG_TUT_KEYS"},
            {TutorialTopic.TradeItems, "TAG_TUT_TRADE_ITEMS"},
            {TutorialTopic.UsePotions, "TAG_TUT_USE_POTIONS"},
            {TutorialTopic.MemorizeSpells, "TAG_TUT_MEMORIZE_SPELLS"},
            {TutorialTopic.PassageIcon, "TAG_TUT_PASSAGE_ICON"},
            {TutorialTopic.Looting, "TAG_TUT_LOOTING"},
            {TutorialTopic.LootingSword, "TAG_TUT_LOOTING_SWORD"},
            {TutorialTopic.MultipleCharacters, "TAG_TUT_MULTIPLE_CHARACTERS"},
            {TutorialTopic.Picklock, "TAG_TUT_PICKLOCK"},
            {TutorialTopic.Room1Overview, "TAG_TUT_ROOM1_OVERVIEW"},
            {TutorialTopic.Room2Overview, "TAG_TUT_ROOM2_OVERVIEW"},
            {TutorialTopic.Room3Overview, "TAG_TUT_ROOM3_OVERVIEW"},
            {TutorialTopic.LockedDoorReminder, "TAG_TUT_LOCKED_DOOR_REMINDER"},
            {TutorialTopic.Room4Overview, "TAG_TUT_ROOM4_OVERVIEW"},
            {TutorialTopic.Room5Overview, "TAG_TUT_ROOM5_OVERVIEW"},
            {TutorialTopic.Room6Overview, "TAG_TUT_ROOM6_OVERVIEW"},
            {TutorialTopic.Room7Overview, "TAG_TUT_ROOM7_OVERVIEW"},
            {TutorialTopic.CastSpellsMagicMissile, "TAG_TUT_CAST_SPELLS_MAGIC_MISSILE"},
            {TutorialTopic.Room8Overview, "TAG_TUT_ROOM8_OVERVIEW"},
            {TutorialTopic.LootPreference, "TAG_TUT_LOOT_PREFERENCE"},
            {TutorialTopic.LootReminder, "TAG_TUT_LOOT_REMINDER"},
            {TutorialTopic.Room9Overview, "TAG_TUT_ROOM9_OVERVIEW"},
            {TutorialTopic.WandUse, "TAG_TUT_WAND_USE"},
            {TutorialTopic.WandFire, "TAG_TUT_WAND_FIRE"},
            {TutorialTopic.LootPreferenceArielDead, "TAG_TUT_LOOT_PREFERENCE_ARIEL_DEAD"},
            {TutorialTopic.RestCampArielDead, "TAG_TUT_REST_CAMP_ARIEL_DEAD"},
            {TutorialTopic.ArielKill, "TAG_TUT_ARIEL_KILL"},
            {TutorialTopic.End, "TAG_TUT_END"},
        };

        [TempleDllLocation(0x10124a10)]
        [TempleDllLocation(0x10BDE3DC)]
        public bool IsTutorialActive { get; private set; }

        [TempleDllLocation(0x10BDE3D8)]
        [TempleDllLocation(0x10124a00)]
        public bool IsSelectingHelpTarget { get; private set; }

        [TempleDllLocation(0x10124840)]
        public HelpManagerUi()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10124870)]
        public void Reset()
        {
            IsSelectingHelpTarget = false;
            IsTutorialActive = false;
        }

        [TempleDllLocation(0x10124880)]
        public void SaveGame(SavedUiState savedState)
        {
            savedState.HelpManagerState = new SavedHelpManagerUiState
            {
                TutorialActive = IsTutorialActive,
                ClickForHelpActive = IsSelectingHelpTarget
            };
        }

        [TempleDllLocation(0x101248b0)]
        public void LoadGame(SavedUiState savedState)
        {
            var helpManagerState = savedState.HelpManagerState;
            IsTutorialActive = helpManagerState.TutorialActive;
            IsSelectingHelpTarget = helpManagerState.ClickForHelpActive;
        }

        [TempleDllLocation(0x101249e0)]
        public void ToggleTutorial()
        {
            IsTutorialActive = !IsTutorialActive;
        }

        [TempleDllLocation(0x10124be0)]
        public void ShowTutorialTopic(TutorialTopic topic)
        {
            var topicName = _tutorialTopicNames[topic];
            var okTxt = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.ok);

            switch (topic)
            {
                case TutorialTopic.Room1Overview:
                    GameSystems.Help.ShowAlert(topicName, btn => ShowTutorialTopic(TutorialTopic.SelectCharacter),
                        okTxt);
                    break;
                case TutorialTopic.Room2Overview:
                    GameSystems.Help.ShowAlert(topicName, btn => ShowTutorialTopic(TutorialTopic.OpenChest), okTxt);
                    break;
                case TutorialTopic.CombatInitiativeBar:
                    GameSystems.Help.ShowAlert(topicName, btn => ShowTutorialTopic(TutorialTopic.CombatActionBar),
                        okTxt);
                    break;
                case TutorialTopic.Room7Overview:
                    GameSystems.Help.ShowAlert(topicName, btn => ShowTutorialTopic(TutorialTopic.CastSpells), okTxt);
                    break;
                default:
                    GameSystems.Help.ShowAlert(topicName, null, okTxt);
                    break;
            }
        }

        [TempleDllLocation(0x101249d0)]
        public CursorType? GetCursor()
        {
            if (IsSelectingHelpTarget)
            {
                return CursorType.IdentifyCursor2;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x10124a40)]
        public void ShowPredefinedTopic(int id)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101249b0)]
        public void ClickForHelpToggle()
        {
            IsSelectingHelpTarget = !IsSelectingHelpTarget;
        }
    }

    public enum TutorialTopic
    {
        RestCamp = 1,
        Portraits = 2,
        OpenDoor = 3,
        OpenChest = 4,
        Inventory = 5,
        Dialogue = 6,
        Movement = 7,
        CombatActionBar = 8,
        SelectCharacter = 9,
        CastSpells = 10,
        CombatInitiativeBar = 11,
        CombatAttacking = 12,
        Keys = 13,
        TradeItems = 14,
        UsePotions = 15,
        MemorizeSpells = 16,
        PassageIcon = 17,
        Looting = 18,
        LootingSword = 19,
        MultipleCharacters = 20,
        Picklock = 21,
        Room1Overview = 22,
        Room2Overview = 23,
        Room3Overview = 24,
        LockedDoorReminder = 25,
        Room4Overview = 26,
        Room5Overview = 27,
        Room6Overview = 28,
        Room7Overview = 29,
        CastSpellsMagicMissile = 30,
        Room8Overview = 31,
        LootPreference = 32,
        LootReminder = 33,
        Room9Overview = 34,
        WandUse = 35,
        WandFire = 36,
        LootPreferenceArielDead = 37,
        RestCampArielDead = 38,
        ArielKill = 39,
        End = 40,
    }
}