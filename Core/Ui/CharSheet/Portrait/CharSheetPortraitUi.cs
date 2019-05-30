using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.CharSheet.Inventory;
using SpicyTemple.Core.Ui.CharSheet.Looting;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Portrait
{
    public class CharSheetPortraitUi : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public WidgetContainer Container { get; }

        [TempleDllLocation(0x10C4D4A0)]
        private CharSheetPortraitMode _lastMode;

        private WidgetButton _miniatureButton;
        private WidgetButton _portraitButton;
        private WidgetButton _paperdollButton;

        private WidgetContainer _portraitContainer;

        private WidgetImage _portraitImage;

        private WidgetContainer _miniatureContainer;

        private MiniatureWidget _miniatureWidget;

        private WidgetContainer _paperdollContainer;

        private Dictionary<EquipSlot, PaperdollSlotWidget> _slotWidgets;

        private CharSheetPortraitMode Mode
        {
            get => _lastMode;
            set
            {
                _miniatureButton.SetActive(value == CharSheetPortraitMode.Miniature);
                _miniatureContainer.SetVisible(value == CharSheetPortraitMode.Miniature);
                _portraitButton.SetActive(value == CharSheetPortraitMode.Portrait);
                _portraitContainer.SetVisible(value == CharSheetPortraitMode.Portrait);
                _paperdollButton.SetActive(value == CharSheetPortraitMode.Paperdoll);
                _paperdollContainer.SetVisible(value == CharSheetPortraitMode.Paperdoll);
                _lastMode = value;
            }
        }

        [TempleDllLocation(0x101a5b40)]
        public CharSheetPortraitUi(Rectangle mainWindowRectangle)
        {
            var textures = Tig.FS.ReadMesFile("art/interface/CHAR_UI/CHAR_PORTRAIT_UI/2_char_portrait_ui_textures.mes");
            var rules = Tig.FS.ReadMesFile("art/interface/CHAR_UI/CHAR_PORTRAIT_UI/2_char_portrait_ui.mes");
            var uiParams = new PortraitUiParams(mainWindowRectangle, rules, textures);

            Container = new WidgetContainer(uiParams.MainWindow);

            CreateButtons(uiParams);

            CreatePortraitContainer(uiParams);

            CreateMiniatureContainer(uiParams);

            CreatePaperdollContainer(uiParams);

            Container.Add(new WidgetContainer(uiParams.TabNavWindow));

            Mode = CharSheetPortraitMode.Paperdoll;
        }

        private void CreatePortraitContainer(PortraitUiParams uiParams)
        {
            _portraitContainer = new WidgetContainer(uiParams.PortraitWindow);
            AddPortraitFrame(uiParams, _portraitContainer);

            _portraitImage = new WidgetImage();
            _portraitImage.FixedSize = uiParams.Portrait;
            _portraitImage.SetX((uiParams.PortraitWindow.Width - uiParams.Portrait.Width) / 2);
            _portraitImage.SetY((uiParams.PortraitWindow.Height - uiParams.Portrait.Height) / 2);
            _portraitContainer.AddContent(_portraitImage);

            Container.Add(_portraitContainer);
        }

        private void CreateMiniatureContainer(PortraitUiParams uiParams)
        {
            _miniatureContainer = new WidgetContainer(uiParams.MiniatureWindow);
            AddPortraitFrame(uiParams, _miniatureContainer);

            _miniatureWidget = new MiniatureWidget();
            _miniatureWidget.SetSizeToParent(true);
            _miniatureContainer.Add(_miniatureWidget);

            Container.Add(_miniatureContainer);
        }

        private void AddPortraitFrame(PortraitUiParams uiParams, WidgetContainer container)
        {
            var backgroundImage = new WidgetImage(uiParams.TexturePaths[PortraitUiTexture.PortraitBorder]);
            backgroundImage.SetFixedWidth(backgroundImage.GetPreferredSize().Width);
            backgroundImage.SetFixedHeight(backgroundImage.GetPreferredSize().Height);
            container.AddContent(backgroundImage);
        }

        private void UpdatePortraitImage(GameObjectBody critter)
        {
            var portraitId = critter.GetInt32(obj_f.critter_portrait);
            string portraitPath;
            if (critter.IsPC())
            {
                portraitPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Big);
            }
            else
            {
                portraitPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
                // TODO Fixed X offset. Cool. 42;
                // Fixed X offset. Cool. 50;
            }

            _portraitImage.SetTexture(portraitPath);
        }

        private void CreateButtons(PortraitUiParams uiParams)
        {
            _miniatureButton = new WidgetButton();
            _miniatureButton.SetPos(uiParams.MiniatureButton.Location);
            _miniatureButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelNormal],
                activatedImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelSelected],
                pressedImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelPressed],
                hoverImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelHover],
                disabledImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelDisabled]
            });
            _miniatureButton.SetClickHandler(() => Mode = CharSheetPortraitMode.Miniature);
            _miniatureButton.TooltipStyle = UiSystems.Tooltip.GetStyle(uiParams.TooltipStyle);
            _miniatureButton.TooltipText = UiSystems.Tooltip.GetString(5000);
            Container.Add(_miniatureButton);

            _portraitButton = new WidgetButton();
            _portraitButton.SetPos(uiParams.PortraitButton.Location);
            _portraitButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitNormal],
                activatedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitSelected],
                pressedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitPressed],
                hoverImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitHover],
                disabledImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitDisabled]
            });
            _portraitButton.SetClickHandler(() => Mode = CharSheetPortraitMode.Portrait);
            _portraitButton.TooltipStyle = UiSystems.Tooltip.GetStyle(uiParams.TooltipStyle);
            _portraitButton.TooltipText = UiSystems.Tooltip.GetString(5001);
            Container.Add(_portraitButton);

            _paperdollButton = new WidgetButton();
            _paperdollButton.SetPos(uiParams.PaperdollButton.Location);
            _paperdollButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollNormal],
                activatedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollSelected],
                pressedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollPressed],
                hoverImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollHover],
                disabledImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollDisabled]
            });
            _paperdollButton.SetClickHandler(() => Mode = CharSheetPortraitMode.Paperdoll);
            _paperdollButton.TooltipStyle = UiSystems.Tooltip.GetStyle(uiParams.TooltipStyle);
            _paperdollButton.TooltipText = UiSystems.Tooltip.GetString(5002);
            Container.Add(_paperdollButton);
        }

        /// <summary>
        /// IDs of the containers we'll put the slot widgets in. Found in char_paperdoll.json
        /// </summary>
        private static readonly Dictionary<EquipSlot, string> SlotWidgetIds = new Dictionary<EquipSlot, string>
        {
            {EquipSlot.Gloves, "slot_gloves"},
            {EquipSlot.WeaponPrimary, "slot_weapon_primary"},
            {EquipSlot.Ammo, "slot_ammo"},
            {EquipSlot.RingPrimary, "slot_ring_primary"},
            {EquipSlot.Helmet, "slot_helmet"},
            {EquipSlot.Armor, "slot_armor"},
            {EquipSlot.Boots, "slot_boots"},
            {EquipSlot.Necklace, "slot_necklace"},
            {EquipSlot.Cloak, "slot_cloak"},
            {EquipSlot.WeaponSecondary, "slot_weapon_secondary"},
            {EquipSlot.Shield, "slot_shield"},
            {EquipSlot.RingSecondary, "slot_ring_secondary"},
            {EquipSlot.Robes, "slot_robes"},
            {EquipSlot.Bracers, "slot_bracers"},
            {EquipSlot.BardicItem, "slot_bardic_item"},
            {EquipSlot.Lockpicks, "slot_lockpicks"}
        };

        private void CreatePaperdollContainer(PortraitUiParams uiParams)
        {
            var widgetDoc = WidgetDoc.Load("ui/char_paperdoll.json");
            _paperdollContainer = widgetDoc.TakeRootContainer();

            // Create an equipslot widget for each slot that has a defined rectangle
            _slotWidgets = SlotWidgetIds.ToDictionary(
                kp => kp.Key,
                kp =>
                {
                    var parent = widgetDoc.GetWindow(kp.Value);
                    var slotWidget = new PaperdollSlotWidget(uiParams, parent.GetSize(), kp.Key);
                    MakeDraggable(slotWidget);
                    parent.Add(slotWidget);
                    return slotWidget;
                }
            );

            Container.Add(_paperdollContainer);
        }

        private static bool IsSplittable(GameObjectBody item, out int quantity)
        {
            return item.TryGetQuantity(out quantity) && quantity > 1 && item.type != ObjectType.money;
        }

        private void Insert(GameObjectBody critter, GameObjectBody item, PaperdollSlotWidget targetSlotWidget)
        {
            if (!AttemptEquipmentChangeInCombat(critter, item))
            {
                return;
            }

            var errorCode = GameSystems.Item.ItemTransferWithFlags(
                item,
                critter,
                targetSlotWidget.InventoryIndex,
                ItemInsertFlag.Allow_Swap | ItemInsertFlag.Use_Wield_Slots,
                null);
            if (errorCode != ItemErrorCode.OK)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(errorCode);
            }
        }

        private void UnequipItem(GameObjectBody item, GameObjectBody critter)
        {
            if (!AttemptEquipmentChangeInCombat(critter, item))
            {
                return;
            }

            var err = GameSystems.Item.ItemTransferWithFlags(
                item, critter, -1, ItemInsertFlag.Unk4, null);
            if (err != ItemErrorCode.OK)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(err);
            }
        }

        private static void UseItem(GameObjectBody critter, GameObjectBody item)
        {
            Logger.Info("Use item via dragging.");
            var soundId = GameSystems.SoundMap.CombatFindWeaponSound(item, critter, null, 2);
            GameSystems.SoundGame.PositionalSound(soundId, 1, critter);

            if (GameSystems.Script.ExecuteObjectScript(critter, item, 0, 0, ObjScriptEvent.Use, 0) == 0)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.ItemCannotBeUsed);
                return;
            }

            if (item.type == ObjectType.written)
            {
                UiSystems.Written.Show(item);
                return;
            }

            if (item.type == ObjectType.food || item.type == ObjectType.generic || item.type == ObjectType.scroll)
            {
                if (GameSystems.D20.Actions.TurnBasedStatusInit(critter))
                {
                    GameSystems.D20.Actions.CurSeqReset(critter);
                    GameSystems.D20.Actions.GlobD20ActnInit();
                    if (GameSystems.D20.Actions.DoUseItemAction(critter, null, item))
                    {
                        GameSystems.D20.Actions.sequencePerform();
                        UiSystems.CharSheet.Hide();
                        return;
                    }
                }

                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.FailedToUseItem);
            }
        }

        private static void DropItem(GameObjectBody critter, GameObjectBody item)
        {
            Logger.Info("Dropping item via drag&drop");

            var soundId = GameSystems.SoundMap.CombatFindWeaponSound(item, critter, null, 1);
            GameSystems.SoundGame.PositionalSound(soundId, 1, critter);

            if (IsSplittable(item, out var quantity))
            {
                if (!UiSystems.CharSheet.SplitItem(item, null, 0, quantity, item.GetInventoryIconPath(),
                    1, -1, 0, 0))
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
                }
            }
            else
            {
                if (!GameSystems.Item.ItemDrop(item))
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
                }
            }
        }

        private void InsertIntoLootContainer(GameObjectBody critter, GameObjectBody item, int msg)
        {
            var target = UiSystems.CharSheet.Looting.Target;
            ItemInsertFlag insertFlags;
            if (target.IsCritter())
            {
                insertFlags = ItemInsertFlag.Unk4;
            }
            else if (target.type == ObjectType.container)
            {
                insertFlags = ItemInsertFlag.Unk4 | ItemInsertFlag.Use_Max_Idx_200;
            }
            else
            {
                return;
            }

            if (GameSystems.Item.GetItemAtInvIdx(target, msg) != null)
            {
                msg = -1;
            }

            if (IsSplittable(item, out var quantity) && critter != target)
            {
                var iconTexture = item.GetInventoryIconPath();
                if (!UiSystems.CharSheet.SplitItem(item, target, 0, quantity,
                    iconTexture, 2, msg, 0, insertFlags))
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
                }
            }
            else
            {
                var err = GameSystems.Item.ItemTransferWithFlags(
                    item,
                    target,
                    msg,
                    insertFlags,
                    null);

                if (err != ItemErrorCode.OK)
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(err);
                }
            }
        }

        private void InsertIntoBarterContainer(GameObjectBody critter, GameObjectBody item, int msg)
        {
            var buyer = UiSystems.CharSheet.Looting.Target;
            if (!buyer.IsCritter())
            {
                buyer = UiSystems.CharSheet.Looting.TargetCritter;
            }

            var appraiser = GameSystems.Party.GetMemberWithHighestSkill(0);
            var appraisedWorth = GameSystems.Item.GetAppraisedWorth(item, appraiser, buyer);

            if (buyer == null)
            {
                return;
            }

            if (appraisedWorth <= 0)
            {
                GameSystems.Dialog.TryGetWontSellVoiceLine(buyer, critter, out _, out var soundId);
                UiSystems.Dialog.PlayVoiceLine(buyer, critter, soundId);
                return;
            }

            if (IsSplittable(item, out var quantity))
            {
                var icon = item.GetInventoryIconPath();
                if (!UiSystems.CharSheet.SplitItem(item, buyer, 0, quantity, icon, 4, msg,
                    appraisedWorth, ItemInsertFlag.Unk4 | ItemInsertFlag.Use_Max_Idx_200))
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
                }
            }
            else
            {
                var err = GameSystems.Item.ItemTransferWithFlags(item,
                    buyer, msg, ItemInsertFlag.Unk4 | ItemInsertFlag.Use_Max_Idx_200, null);
                if (err != ItemErrorCode.OK)
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(err);
                    return;
                }

                GameSystems.Item.SplitMoney(appraisedWorth,
                    true, out var platinum,
                    true, out var gold,
                    true, out var silver,
                    out var copper);
                GameSystems.Party.AddPartyMoney(platinum, gold, silver, copper);
            }
        }

        private void InsertIntoPartyPortrait(GameObjectBody critter, GameObjectBody item, GameObjectBody partyMember)
        {
            if (partyMember == critter
                || !UiSystems.CharSheet.Inventory.IsCloseEnoughToTransferItem(critter, partyMember)
                || GameSystems.Combat.IsCombatActive())
            {
                return;
            }

            if (IsSplittable(item, out var quantity))
            {
                var icon = item.GetInventoryIconPath();
                if (!UiSystems.CharSheet.SplitItem(item, partyMember, 0, quantity, icon, 2, -1,
                    0, ItemInsertFlag.Unk4))
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode
                        .Item_Cannot_Be_Dropped);
                }

                return;
            }

            var err = GameSystems.Item.ItemTransferWithFlags(item,
                partyMember, -1, ItemInsertFlag.Unk4, null);
            if (err != ItemErrorCode.OK)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(err);
            }
        }

        private void InsertIntoInventorySlot(GameObjectBody critter, GameObjectBody item, int invIdx)
        {
            if (!AttemptEquipmentChangeInCombat(critter, item))
            {
                return;
            }

            var itemInsertFlag = ItemInsertFlag.Unk4;
            if (GameSystems.Combat.IsCombatActive())
            {
                itemInsertFlag |= ItemInsertFlag.Allow_Swap;
            }

            var err = GameSystems.Item.ItemTransferWithFlags(item, critter, invIdx, itemInsertFlag, null);
            if (err != ItemErrorCode.OK)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(err);
            }
        }

        private void MakeDraggable(PaperdollSlotWidget slotWidget)
        {
            slotWidget.SetMouseMsgHandler(msg =>
            {
                if (msg.flags.HasFlag(MouseEventFlag.LeftReleased))
                {
                    if (UiSystems.CharSheet.State == CharInventoryState.CastingSpell)
                    {
                        UiSystems.CharSheet.CallItemPickCallback(slotWidget.CurrentItem);
                        return true;
                    }

                    var a3 = Globals.UiManager.GetAdvancedWidgetAt(msg.X, msg.Y);
                    var item = UiSystems.CharSheet.Inventory.DraggedObject;
                    if (item != null && a3 != slotWidget)
                    {
                        if (a3 is PaperdollSlotWidget targetSlotWidget)
                        {
                            Insert(slotWidget.Critter, slotWidget.CurrentItem, targetSlotWidget);
                        }
                        else if (a3 == UiSystems.CharSheet.Inventory.UseItemWidget)
                        {
                            UseItem(slotWidget.Critter, slotWidget.CurrentItem);
                        }
                        else if (a3 == UiSystems.CharSheet.Inventory.DropItemWidget)
                        {
                            DropItem(slotWidget.Critter, slotWidget.CurrentItem);
                        }
                        else if (UiSystems.CharSheet.State == CharInventoryState.Looting
                                 && UiSystems.CharSheet.Looting.TryGetInventoryIdxForWidget(a3, out var lootingInvIdx))
                        {
                            InsertIntoLootContainer(slotWidget.Critter, slotWidget.CurrentItem, lootingInvIdx);
                        }
                        else if (UiSystems.CharSheet.State == CharInventoryState.Bartering
                                 && UiSystems.CharSheet.Looting.TryGetInventoryIdxForWidget(a3, out var barterInvIdx))
                        {
                            InsertIntoBarterContainer(slotWidget.Critter, slotWidget.CurrentItem, barterInvIdx);
                        }
                        else if (UiSystems.Party.TryGetPartyMemberByWidget(a3, out var partyMember))
                        {
                            InsertIntoPartyPortrait(slotWidget.Critter, slotWidget.CurrentItem, partyMember);
                        }
                        else if (UiSystems.CharSheet.Inventory.TryGetInventoryIdxForWidget(a3, out var invIdx))
                        {
                            InsertIntoInventorySlot(slotWidget.Critter, slotWidget.CurrentItem, invIdx);
                        }
                    }

                    UiSystems.CharSheet.Inventory.DraggedObject = null;
                    Tig.Mouse.ClearDraggedIcon();
                }
                else if (msg.flags.HasFlag(MouseEventFlag.RightReleased))
                {
                    var critter = slotWidget.Critter;
                    var item = slotWidget.CurrentItem;
                    if (item != null)
                    {
                        UnequipItem(item, critter);
                    }

                    return true;
                }

                return true;
            });

            slotWidget.SetWidgetMsgHandler(msg =>
            {
                // Determine where inside the widget the user clicked.
                var contentArea = slotWidget.GetContentArea();
                var relPos = new Point(msg.x - contentArea.X, msg.y - contentArea.Y);

                switch (msg.widgetEventType)
                {
                    case TigMsgWidgetEvent.Exited:
                        UiSystems.HelpInventory.SetString("");
                        break;
                    case TigMsgWidgetEvent.Clicked:
                    {
                        var currentItem = slotWidget.CurrentItem;
                        if (currentItem == null)
                        {
                            return false;
                        }

                        if (UiSystems.CharSheet.Looting.IsIdentifying)
                        {
                            return false;
                        }

                        if (UiSystems.CharSheet.State == CharInventoryState.CastingSpell)
                        {
                            return false;
                        }

                        UiSystems.CharSheet.Inventory.DraggedObject = currentItem;
                        var texturePath = currentItem.GetInventoryIconPath();
                        var iconSize = slotWidget.GetSize();
                        iconSize.Height -= 4;
                        iconSize.Width -= 4;
                        Tig.Mouse.SetDraggedIcon(
                            texturePath,
                            new Point(-relPos.X + 4, -relPos.Y + 4),
                            iconSize
                        );
                        return false;
                    }

                    case TigMsgWidgetEvent.MouseReleased:
                        if (!UiSystems.CharSheet.Looting.IsIdentifying)
                        {
                            UiSystems.CharSheet.Inventory.DraggedObject = null;
                            Tig.Mouse.ClearDraggedIcon();
                        }
                        else
                        {
                            var currentItem = slotWidget.CurrentItem;
                            if (currentItem == null
                                || GameSystems.Item.IsIdentified(currentItem)
                                || GameSystems.Party.GetPartyMoney() < 10000)
                            {
                                return false;
                            }

                            GameSystems.Party.RemovePartyMoney(0, 100, 0, 0);
                            currentItem.SetItemFlag(ItemFlag.IDENTIFIED, true);
                        }

                        return false;
                    case TigMsgWidgetEvent.MouseReleasedAtDifferentButton:
                        var droppedOn = Globals.UiManager.GetAdvancedWidgetAt(msg.x, msg.y);
                        if (droppedOn != null)
                        {
                            var mouseMsg = new Message(new MessageMouseArgs(
                                msg.x,
                                msg.y,
                                0,
                                MouseEventFlag.LeftReleased
                            ));
                            slotWidget.HandleMessage(mouseMsg);
                        }

                        UiSystems.CharSheet.Inventory.DraggedObject = null;
                        Tig.Mouse.ClearDraggedIcon();
                        break;
                }

                return true;
            });
        }

        private bool AttemptEquipmentChangeInCombat(GameObjectBody critter, GameObjectBody item)
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                return true;
            }

            if (!UiSystems.CharSheet.Inventory.EquippingIsAction(item))
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Cannot_Transfer);
                return false;
            }

            var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
            if (!GameSystems.Feat.HasFeat(critter, FeatId.QUICK_DRAW)
                && tbStatus.hourglassState < 1 &&
                !tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.ChangedWornItem))
            {
                Logger.Info("Cannot change equipment, not enough time left!");
                return false;
            }

            if (!GameSystems.Feat.HasFeat(critter, FeatId.QUICK_DRAW))
            {
                if (!tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.ChangedWornItem))
                {
                    tbStatus.hourglassState =
                        GameSystems.D20.Actions.GetHourglassTransition(
                            tbStatus.hourglassState,
                            1);
                    tbStatus.tbsFlags |= TurnBasedStatusFlags.ChangedWornItem;
                }
            }

            return true;
        }

        private void UpdateMiniature(GameObjectBody critter)
        {
            _miniatureWidget.Object = critter;
        }

        [TempleDllLocation(0x101a3150)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101a3180)]
        public void Show(GameObjectBody critter)
        {
            UpdatePortraitImage(critter);
            UpdateMiniature(critter);
            foreach (var widget in _slotWidgets.Values)
            {
                widget.Critter = critter;
            }
        }

        [TempleDllLocation(0x101a3260)]
        public void Hide()
        {
            UpdateMiniature(null);
        }

        public void Reset()
        {
        }
    }
}