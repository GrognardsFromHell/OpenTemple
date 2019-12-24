using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.CharSheet.Abilities;
using SpicyTemple.Core.Ui.CharSheet.Feats;
using SpicyTemple.Core.Ui.CharSheet.HelpInventory;
using SpicyTemple.Core.Ui.CharSheet.Inventory;
using SpicyTemple.Core.Ui.CharSheet.LevelUp;
using SpicyTemple.Core.Ui.CharSheet.Looting;
using SpicyTemple.Core.Ui.CharSheet.Portrait;
using SpicyTemple.Core.Ui.CharSheet.Skills;
using SpicyTemple.Core.Ui.CharSheet.Spells;
using SpicyTemple.Core.Ui.CharSheet.Stats;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet
{
    public class CharSheetUi : IDisposable, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10BE9308)]
        private CharUiMainWidget _mainWidget;

        [TempleDllLocation(0x10BE9344)]
        private WidgetButton char_ui_main_select_spells_button;

        [TempleDllLocation(0x10BE9340)]
        private WidgetButton char_ui_main_select_feats_button;

        [TempleDllLocation(0x10BE933C)]
        private WidgetButton char_ui_main_select_skills_button;

        [TempleDllLocation(0x10BE9328)]
        private WidgetButton char_ui_main_select_inventory_0_button;

        [TempleDllLocation(0x10BE932C)]
        private WidgetButton char_ui_main_select_inventory_1_button;

        [TempleDllLocation(0x10BE9330)]
        private WidgetButton char_ui_main_select_inventory_2_button;

        [TempleDllLocation(0x10BE9334)]
        private WidgetButton char_ui_main_select_inventory_3_button;

        [TempleDllLocation(0x10BE9338)]
        private WidgetButton char_ui_main_select_inventory_4_button;

        [TempleDllLocation(0x10BE9324)]
        private WidgetButton char_ui_main_worship_button;

        [TempleDllLocation(0x10BE9320)]
        private WidgetButton char_ui_main_alignment_gender_race_button;

        [TempleDllLocation(0x10BE931C)]
        private WidgetButton char_ui_main_class_level_button;

        [TempleDllLocation(0x10BE9318)]
        private WidgetButton char_ui_main_exit_button;

        [TempleDllLocation(0x10BE9314)]
        private CharUiNameLabel char_ui_main_name_button;

        [TempleDllLocation(0x10BE9310)]
        private WidgetContainer char_ui_main_nav_editor_window;

        [TempleDllLocation(0x10BE930C)]
        private WidgetContainer char_ui_main_exit_window;

        private CharUiParams _uiParams;

        [TempleDllLocation(0x10BE9968)]
        private Func<GameObjectBody, bool> _itemPickedCallback;

        [TempleDllLocation(0x10BE996C)]
        private Action _closeCallback; // TODO: Unused

        public CharSheetInventoryUi Inventory { get; }

        public CharSheetSkillsUi Skills { get; }

        public CharSheetFeatsUi Feats { get; }

        public CharSheetAbilitiesUi Abilities { get; }

        public CharSheetSpellsUi Spells { get; }

        public CharSheetLootingUi Looting { get; }

        public CharSheetStatsUi Stats { get; }

        public CharSheetPortraitUi Portrait { get; }

        public CharSheetLevelUpUi LevelUp { get; }

        public CharSheetHelpUi Help { get; }

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x1014b900)]
        public CharSheetUi()
        {
            _uiParams = new CharUiParams(
                Tig.FS.ReadMesFile("art/interface/char_ui/0_char_ui.mes"),
                Tig.FS.ReadMesFile("art/interface/char_ui/0_char_ui_textures.mes")
            );
            _translations = Tig.FS.ReadMesFile("mes/0_char_ui_text.mes");

            _mainWidget = new CharUiMainWidget(_uiParams);

            char_ui_main_nav_editor_window = new WidgetContainer(_uiParams.CharUiMainNavEditorWindow);
            _mainWidget.Add(char_ui_main_nav_editor_window);

            char_ui_main_name_button = new CharUiNameLabel(_uiParams);
            _mainWidget.Add(char_ui_main_name_button);

            CreateExitButton();

            _mainWidget.Add(new CharUiClassLevel(_uiParams));
            _mainWidget.Add(new CharUiAlignGenderRace(_uiParams));
            _mainWidget.Add(new CharUiWorship(_uiParams));

            for (int i = 0; i < 5; i++)
            {
                var button = new CharInventoryButton(_uiParams, i);
                if (i == 0)
                {
                    button.SetClickHandler(() => SelectInventoryTab(0));
                    // TODO: Click handlers for the other bags were never implemented apparently
                }

                _mainWidget.Add(button);
            }

            var skillsButton = new CharUiTopButton(_uiParams, 5);
            skillsButton.SetClickHandler(SelectSkillsTab);
            _mainWidget.Add(skillsButton);

            var featsButton = new CharUiTopButton(_uiParams, 6);
            featsButton.SetClickHandler(SelectFeatsTab);
            _mainWidget.Add(featsButton);

            var spellsButton = new CharUiTopButton(_uiParams, 7);
            spellsButton.SetClickHandler(SelectSpellsTab);
            _mainWidget.Add(spellsButton);

            Skills = new CharSheetSkillsUi();
            _mainWidget.Add(Skills.Container);
            Inventory = new CharSheetInventoryUi();
            _mainWidget.Add(Inventory.Widget);
            Feats = new CharSheetFeatsUi();
            _mainWidget.Add(Feats.Container);
            Abilities = new CharSheetAbilitiesUi();
            Spells = new CharSheetSpellsUi();
            _mainWidget.Add(Spells.Container);
            Looting = new CharSheetLootingUi();
            Stats = new CharSheetStatsUi(_uiParams.CharUiMainWindow);
            _mainWidget.Add(Stats.Container);
            Portrait = new CharSheetPortraitUi(_uiParams.CharUiMainWindow);
            _mainWidget.Add(Portrait.Container);
            LevelUp = new CharSheetLevelUpUi();
            Help = new CharSheetHelpUi();
            _mainWidget.Add(Help.Container);
        }

        [TempleDllLocation(0x10146fd0)]
        private void SelectInventoryTab(int inventoryIdx)
        {
            if (State == CharInventoryState.LevelUp)
            {
                Logger.Warn("You cannot switch to the inventory tab while leveling up using this button.");
            }
            else if (CurrentPage != inventoryIdx)
            {
                Logger.Debug($"Switching to inventory {inventoryIdx} of character sheet.");
                CurrentPage = inventoryIdx;
                Inventory.Hide();
                Inventory.Show(CurrentCritter);
                Skills.Hide();
                Feats.Hide();
                Spells.Hide();
                Abilities.Hide();
                Inventory.BagIndex = inventoryIdx;
            }
            else
            {
                Inventory.BagIndex = inventoryIdx;
            }
        }

        [TempleDllLocation(0x101470e0)]
        private void SelectSkillsTab()
        {
            if (State == CharInventoryState.LevelUp)
            {
                Logger.Warn("You cannot switch to the skills tab while leveling up using this button.");
            }
            else if (CurrentPage != 5)
            {
                Logger.Debug("Switching to skills tab of character sheet.");
                CurrentPage = 5;
                Skills.Show();
                Inventory.Hide();
                Feats.Hide();
                Spells.Hide();
                Abilities.Hide();
            }
        }

        [TempleDllLocation(0x101470e0)]
        private void SelectFeatsTab()
        {
            if (State == CharInventoryState.LevelUp)
            {
                Logger.Warn("You cannot switch to the feats tab while leveling up using this button.");
            }
            else if (CurrentPage != 6)
            {
                Logger.Debug("Switching to feats tab of character sheet.");
                CurrentPage = 6;
                Feats.Show(CurrentCritter);
                Inventory.Hide();
                Skills.Hide();
                Spells.Hide();
                Abilities.Hide();
            }
        }

        [TempleDllLocation(0x10147160)]
        private void SelectSpellsTab()
        {
            if (State == CharInventoryState.LevelUp)
            {
                Logger.Warn("You cannot switch to the spells tab while leveling up using this button.");
            }
            else if (State == CharInventoryState.PartyPool)
            {
                Logger.Warn("You cannot switch to the spells tab while in the party pool using this button.");
            }
            else if (CurrentPage != 7)
            {
                Logger.Debug("Switching to spells tab of character sheet.");
                CurrentPage = 7;
                Spells.Show(CurrentCritter);
                Inventory.Hide();
                Skills.Hide();
                Feats.Hide();
                Abilities.Hide();
            }
        }

        private void CreateExitButton()
        {
            var exitButton = new WidgetButton(_uiParams.CharUiMainExitButton);
            exitButton.SetAutoSizeHeight(false);
            exitButton.SetAutoSizeWidth(false);
            exitButton.SetStyle(new WidgetButtonStyle
            {
                disabledImagePath = _uiParams.TexturePaths[CharUiTexture.MainExitButtonDisabled],
                hoverImagePath = _uiParams.TexturePaths[CharUiTexture.MainExitButtonHoverOn],
                normalImagePath = _uiParams.TexturePaths[CharUiTexture.MainExitButtonHoverOff],
                pressedImagePath = _uiParams.TexturePaths[CharUiTexture.MainExitButtonHoverPressed]
            });
            exitButton.SetClickHandler(ExitClicked);
            _mainWidget.Add(exitButton);
        }

        [TempleDllLocation(0x10148fb0)]
        public void CallItemPickCallback(GameObjectBody item)
        {
            if (_itemPickedCallback == null || _itemPickedCallback(item))
            {
                _itemPickedCallback = null;
                Hide(CharInventoryState.Closed);
            }
        }

        private void ExitClicked()
        {
            if (_state != CharInventoryState.CastingSpell)
            {
                CurrentPage = 0;
                Hide(CharInventoryState.Closed);
            }
            else
            {
                CallItemPickCallback(null);
            }

            if (_closeCallback != null)
            {
                _closeCallback();
                _closeCallback = null;
            }

            if (GameSystems.Map.GetCurrentMapId() == 5116)
            {
                if (GameSystems.Script.GetGlobalFlag(2))
                {
                    if (!UiSystems.HelpManager.IsTutorialActive)
                        UiSystems.HelpManager.ToggleTutorial();
                    UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.Keys);
                    GameSystems.Script.SetGlobalFlag(2, false);
                }

                return;
            }

            if (GameSystems.Map.GetCurrentMapId() == 5117 && GameSystems.Script.GetGlobalFlag(1))
            {
                if (!UiSystems.HelpManager.IsTutorialActive)
                {
                    UiSystems.HelpManager.ToggleTutorial();
                }

                if (GameSystems.Script.GetGlobalFlag(11))
                {
                    UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.RestCampArielDead);
                }
                else
                {
                    UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.RestCamp);
                }

                GameSystems.Script.SetGlobalFlag(1, false);
            }
        }

        public void Hide()
        {
            Hide(CharInventoryState.Closed);
        }

        [TempleDllLocation(0x101499e0)]
        public void Show(GameObjectBody obj)
        {
            if (CurrentCritter != null)
            {
                Hide(_state);
            }

            GameSystems.TimeEvent.PushDisableFidget();

            if (obj != CurrentCritter)
            {
                ResetPages();
            }

            CurrentCritter = obj;
            Inventory.Container = obj;
            _mainWidget.Visible = true;
            _mainWidget.BringToFront();
            Stats.Show();
            Portrait.Show(obj);

            if (_state == CharInventoryState.Unknown6)
            {
                CurrentPage = 7;
                Spells.Show(CurrentCritter);
            }
            else if (_state != CharInventoryState.LevelUp)
            {
                switch (CurrentPage)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        Inventory.Show(obj);
                        break;
                    case 5:
                        Skills.Show();
                        break;
                    case 6:
                        Feats.Show(CurrentCritter);
                        break;
                    case 7:
                        Spells.Show(CurrentCritter);
                        break;
                    case 8:
                        Abilities.Show();
                        break;
                    default:
                        Logger.Warn("Showing default character sheet page (inventory).");
                        Inventory.Show(obj);
                        break;
                }
            }

            switch (_state)
            {
                case CharInventoryState.Looting:
                case CharInventoryState.Bartering:
                case CharInventoryState.Unknown6:
                    Looting.Show(null);
                    break;
                case CharInventoryState.LevelUp:
                    CurrentPage = 9;
                    LevelUp.Show();
                    break;
            }

            CenterOnScreen();

            Help.Show();

            HandleLootingTutorialTopics();
        }

        public void CenterOnScreen()
        {
            // We need to center both the looting and inventory together
            if (State == CharInventoryState.Bartering || State == CharInventoryState.Looting || State == CharInventoryState.Unknown6)
            {
                var screenSize = Globals.UiManager.ScreenSize;

                // Vertical centering is easy enough
                var y = (screenSize.Height - _mainWidget.Height) / 2;
                _mainWidget.Y = y;
                Looting.Container.Y = y;

                var totalWidth = Looting.Container.Width + _mainWidget.Width;
                var x = (screenSize.Width - totalWidth) / 2;
                Looting.Container.X = x;
                _mainWidget.X = x + Looting.Container.Width;
            }
            else
            {
                _mainWidget.CenterOnScreen();
            }
        }

        private void HandleLootingTutorialTopics()
        {
            // Handle initiating looting of "Tutorial Chest A"
            if (UiSystems.HelpManager.IsTutorialActive)
            {
                if (_state == CharInventoryState.Looting)
                {
                    if (Looting.GetLootingState() != 0)
                    {
                        if (Looting.LootingContainer?.ProtoId == 1048)
                        {
                            if (GameSystems.Script.GetGlobalFlag(5))
                            {
                                UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.LootingSword);
                                GameSystems.Script.SetGlobalFlag(5, false);
                            }
                        }
                    }
                }
            }

            // Handle looting of wand of fireball
            if (GameSystems.Map.GetCurrentMapId() == 5118 && !GameSystems.Script.GetGlobalFlag(8))
            {
                var lootingTarget = Looting.LootingContainer;
                if (lootingTarget != null && lootingTarget.IsNPC())
                {
                    var hasWandOfFireball = lootingTarget.EnumerateChildren().Any(item => item.ProtoId == 12581);
                    if (hasWandOfFireball)
                    {
                        if (!UiSystems.HelpManager.IsTutorialActive)
                        {
                            UiSystems.HelpManager.ToggleTutorial();
                        }

                        if (GameSystems.Script.GetGlobalFlag(11))
                        {
                            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.LootPreferenceArielDead);
                        }
                        else
                        {
                            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.LootPreference);
                        }

                        // TODO: Might this be a mistake and it should be flag 11?
                        GameSystems.Script.SetGlobalFlag(8, true);
                    }
                }
            }
        }

        [TempleDllLocation(0x10144030)]
        public bool HasCurrentCritter => CurrentCritter != null;

        [TempleDllLocation(0x10144050)]
        [TempleDllLocation(0x10BE9940)]
        public GameObjectBody CurrentCritter { get; private set; }

        [TempleDllLocation(0x10143fe0)]
        [TempleDllLocation(0x10BE9948)]
        public int CurrentPage { get; set; }

        [TempleDllLocation(0x10148e20)]
        public void Hide(CharInventoryState newState)
        {
            if (CurrentCritter != null)
            {
                GameSystems.TimeEvent.PopDisableFidget();
            }

            if (UiSystems.Popup.IsAnyOpen())
            {
                UiSystems.Popup.CloseAll();
            }

            if (CurrentPage >= 1 && CurrentPage <= 4)
            {
                CurrentPage = 0;
            }

            Inventory.BagIndex = 0;

            if (GameSystems.Combat.IsCombatActive())
            {
                // Clear the flag that allows us to change more than one item while in the inventory menu,
                // without losing even more of our turn.
                var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
                tbStatus.tbsFlags &= ~TurnBasedStatusFlags.ChangedWornItem;
            }

            if (Inventory.DraggedObject != null)
            {
                Inventory.DraggedObject = null;
                Tig.Mouse.ClearDraggedIcon();
            }

            _mainWidget.Hide();
            Stats.Hide();
            Portrait.Hide();
            Inventory.Hide();
            Skills.Hide();
            Feats.Hide();
            Spells.Hide();
            Abilities.Hide();
            LevelUp.Hide();

            switch (_state)
            {
                case CharInventoryState.Closed:
                    break;
                case CharInventoryState.Looting:
                case CharInventoryState.Bartering:
                case CharInventoryState.Unknown6:
                    if (newState != _state)
                    {
                        Looting.Hide();
                    }

                    break;
                case CharInventoryState.LevelUp:
                    LevelUp.Hide();
                    break;
                case CharInventoryState.CastingSpell:
                    _itemPickedCallback?.Invoke(null);
                    _itemPickedCallback = null;
                    break;
                case CharInventoryState.PartyPool:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            State = newState;
            CurrentCritter = null;
            Help.Hide();
        }

        public CharInventoryState State
        {
            [TempleDllLocation(0x101441b0)]
            get => _state;
            [TempleDllLocation(0x101441c0)]
            set
            {
                _state = value;
                UpdateUiFromState();
            }
        }

        private void UpdateUiFromState()
        {
            var screenWidthFactor = Globals.UiManager.ScreenSize.Width / 800.0f;

            int x = UiSystems.CharSheet._mainWidget.X;

            int xOffset = 0;
            switch (_state)
            {
                case CharInventoryState.Closed:
                    xOffset = (int) (screenWidthFactor * _uiParams.CharUiModeXNormal) - x;
                    break;
                case CharInventoryState.Looting:
                case CharInventoryState.Bartering:
                case CharInventoryState.Unknown6:
                    xOffset = (int) (screenWidthFactor * _uiParams.CharUiModeXLooting) - x;
                    break;
                case CharInventoryState.LevelUp:
                    xOffset = (int) (screenWidthFactor * _uiParams.CharUiModeXLevelUp) - x;
                    break;
            }

            Stub.TODO();
            // TODO We only need to move the parent, really...
            // TODO But we also need to account for the larger width when looting/bartering/leveling...
            // char_ui_move(v1);
            // ui_char_stats_move(v1);
            // ui_char_portrait_move(v1);
            // char_ui_inventory_move(v1);
            // char_ui_skills_move(v1);
            // char_ui_feats_move(v1);
            // char_ui_spells_move(v1);
            // char_ui_abilities_move(v1);
        }

        [TempleDllLocation(0x10BE994C)]
        private CharInventoryState _state;

        [TempleDllLocation(0x10149dd0)]
        public void ShowInState(CharInventoryState state, GameObjectBody obj)
        {
            Hide(state);
            Show(obj);
        }

        public void Dispose()
        {
            Abilities?.Dispose();
            Feats?.Dispose();
            Inventory?.Dispose();
            Looting?.Dispose();
            Skills?.Dispose();
            Spells?.Dispose();
            Stats?.Dispose();
            Portrait?.Dispose();
            LevelUp?.Dispose();
        }

        [TempleDllLocation(0x10143f80)]
        public void Reset()
        {
            CurrentPage = 0;
            Inventory.BagIndex = 0;

            Help.Reset();

            ResetPages();
        }

        private void ResetPages()
        {
            Abilities.Reset();
            Feats.Reset();
            Inventory.Reset();
            Looting.Reset();
            Skills.Reset();
            Spells.Reset();
            Stats.Reset();
            Portrait.Reset();
            LevelUp.Reset();
        }

        [TempleDllLocation(0x101443b0)]
        public void ItemTransferErrorPopup(ItemErrorCode errorCode)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10BE8D48)] [TempleDllLocation(0x10BE8D50)]
        private SliderParams? _itemTransfer;

        [TempleDllLocation(0x10149e80)]
        public bool SplitItem(GameObjectBody item, GameObjectBody parent, int minAmt, int itemQty,
            string texturePath, int transferType, int itemInsertLocation, int sum, ItemInsertFlag flags)
        {
            if (!_itemTransfer.HasValue)
            {
                if (transferType != 1)
                {
                    var err = GameSystems.Item.ItemInsertGetLocation(item, parent, ref itemInsertLocation,
                        null, flags);
                    if (err != ItemErrorCode.OK)
                    {
                        ItemTransferErrorPopup(err);
                        return _itemTransfer.HasValue;
                    }
                }

                var sliderParams = new SliderParams();
                sliderParams.transferType = 0;
                sliderParams.obj = null;
                sliderParams.parent = null;
                sliderParams.invIdx = -1;
                sliderParams.sum = -1;
                sliderParams.header = _translations[13];
                sliderParams.icon = texturePath;
                sliderParams.MinAmount = minAmt;
                sliderParams.obj = item;
                sliderParams.transferType = transferType;
                sliderParams.Amount = itemQty;
                sliderParams.callback = TransferItemCallback;
                sliderParams.parent = parent;
                sliderParams.invIdx = itemInsertLocation;
                sliderParams.sum = sum;
                if (transferType == 3)
                {
                    sliderParams.textDrawCallback = UiItemSplitMoneyDrawText;
                    sliderParams.Amount = (int) (GameSystems.Party.GetPartyMoney() / (double) sum);
                    if (sliderParams.Amount > itemQty)
                        sliderParams.Amount = itemQty;
                }
                else if (transferType == 4)
                {
                    sliderParams.textDrawCallback = UiItemSplitMoneyDrawText;
                }

                _itemTransfer = sliderParams;
                UiSystems.Slider.Show(ref sliderParams);
            }

            return _itemTransfer.HasValue;
        }

        [TempleDllLocation(0x10144460)]
        private void UiItemSplitMoneyDrawText(int widgetId)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101493D0)]
        private void TransferItemCallback(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10144400)]
        public void ShowItemDetailsPopup(GameObjectBody item, GameObjectBody observer)
        {
            if (!UiSystems.Popup.IsAnyOpen() && GameSystems.MapObject.HasLongDescription(item))
            {
                var title = GameSystems.MapObject.GetDisplayName(item, observer);
                var body = GameSystems.MapObject.GetLongDescription(item, observer);
                UiSystems.Popup.ConfirmBox(body, title, false, null, 0);
            }
        }

        /// <summary>
        /// Shows the inventory of the given critter for the purposes of selecting an item.
        /// The given callback will be called with the item. When the callback returns true,
        /// the inventory is closed, otherwise it is kept open.
        /// </summary>
        [TempleDllLocation(0x10149e20)]
        public void ShowItemPicker(GameObjectBody critter, Func<GameObjectBody, bool> callback)
        {
            _itemPickedCallback = callback;
            State = CharInventoryState.CastingSpell;
            Show(critter);
        }
    }
}