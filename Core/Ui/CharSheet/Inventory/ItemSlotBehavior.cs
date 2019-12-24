using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.CharSheet.Portrait;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet.Inventory
{

    public enum ItemSlotMode
    {
        /// <summary>
        /// A slot belonging to one's own inventory.
        /// </summary>
        Inventory,
        /// <summary>
        /// A slot belonging to a container or corpse that is being looted.
        /// </summary>
        Looting,
        /// <summary>
        /// A slot belonging to a vendor's inventory who is being bartered with.
        /// </summary>
        Bartering
    }

    public class ItemSlotBehavior
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly WidgetBase _slotWidget;

        private readonly Func<GameObjectBody> _currentItemSupplier;

        private readonly Func<GameObjectBody> _actingCritterSupplier;

        private GameObjectBody CurrentItem => _currentItemSupplier();

        private GameObjectBody ActingCritter => _actingCritterSupplier();

        public bool AllowShowInfo { get; set; }

        public ItemSlotMode Mode { get; set; } = ItemSlotMode.Inventory;

        public ItemSlotBehavior(WidgetBase slotWidget,
            Func<GameObjectBody> currentItemSupplier,
            Func<GameObjectBody> actingCritterSupplier)
        {
            _slotWidget = slotWidget;
            _currentItemSupplier = currentItemSupplier;
            _actingCritterSupplier = actingCritterSupplier;

            slotWidget.SetMouseMsgHandler(HandleMouseMessage);
            slotWidget.SetWidgetMsgHandler(HandleWidgetMessage);
        }

        private bool HandleWidgetMessage(MessageWidgetArgs msg)
        {
            // Determine where inside the widget the user clicked.
            var contentArea = _slotWidget.GetContentArea();
            var relPos = new Point(msg.x - contentArea.X, msg.y - contentArea.Y);

            var currentItem = CurrentItem;

            switch (msg.widgetEventType)
            {
                case TigMsgWidgetEvent.Exited:
                    UiSystems.CharSheet.Help.ClearHelpText();
                    break;
                case TigMsgWidgetEvent.Clicked:
                {
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
                    var iconSize = _slotWidget.GetSize();
                    iconSize.Height -= 4;
                    iconSize.Width -= 4;
                    Tig.Mouse.SetDraggedIcon(texturePath, new Point(-relPos.X + 4, -relPos.Y + 4), iconSize);
                    return false;
                }

                case TigMsgWidgetEvent.MouseReleased:
                    if (UiSystems.CharSheet.Looting.IsIdentifying)
                    {
                        if (currentItem == null || GameSystems.Item.IsIdentified(currentItem) ||
                            GameSystems.Party.GetPartyMoney() < 10000)
                        {
                            return false;
                        }

                        GameSystems.Party.RemovePartyMoney(0, 100, 0, 0);
                        currentItem.SetItemFlag(ItemFlag.IDENTIFIED, true);
                    }
                    else
                    {
                        // Shift+Click will show extra info about an item
                        if (AllowShowInfo
                            && (Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LSHIFT)
                                || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RSHIFT))
                            && GameSystems.MapObject.HasLongDescription(currentItem))
                        {
                            UiSystems.CharSheet.ShowItemDetailsPopup(currentItem, ActingCritter);
                        }

                        UiSystems.CharSheet.Inventory.DraggedObject = null;
                        Tig.Mouse.ClearDraggedIcon();
                    }

                    return false;
                case TigMsgWidgetEvent.MouseReleasedAtDifferentButton:
                    var droppedOn = Globals.UiManager.GetAdvancedWidgetAt(msg.x, msg.y);
                    if (droppedOn != null)
                    {
                        var mouseMsg = new Message(new MessageMouseArgs(msg.x, msg.y, 0, MouseEventFlag.LeftReleased));
                        _slotWidget.HandleMessage(mouseMsg);
                    }

                    UiSystems.CharSheet.Inventory.DraggedObject = null;
                    Tig.Mouse.ClearDraggedIcon();
                    break;
            }

            return true;
        }

        private bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if (msg.flags.HasFlag(MouseEventFlag.LeftReleased))
            {
                if (UiSystems.CharSheet.State == CharInventoryState.CastingSpell)
                {
                    UiSystems.CharSheet.CallItemPickCallback(CurrentItem);
                    return true;
                }

                var droppedOn = Globals.UiManager.GetAdvancedWidgetAt(msg.X, msg.Y);
                var item = UiSystems.CharSheet.Inventory.DraggedObject;
                if (item == null || droppedOn == _slotWidget)
                {
                    return true;
                }

                if (droppedOn is PaperdollSlotWidget targetSlotWidget)
                {
                    Insert(ActingCritter, CurrentItem, targetSlotWidget);
                }
                else if (droppedOn == UiSystems.CharSheet.Inventory.UseItemWidget)
                {
                    UseItem(ActingCritter, CurrentItem);
                }
                else if (droppedOn == UiSystems.CharSheet.Inventory.DropItemWidget)
                {
                    DropItem(ActingCritter, CurrentItem);
                }
                else if (UiSystems.CharSheet.State == CharInventoryState.Looting &&
                         UiSystems.CharSheet.Looting.TryGetInventoryIdxForWidget(droppedOn, out var lootingInvIdx))
                {
                    InsertIntoLootContainer(ActingCritter, CurrentItem, lootingInvIdx);
                }
                else if (UiSystems.CharSheet.State == CharInventoryState.Bartering &&
                         UiSystems.CharSheet.Looting.TryGetInventoryIdxForWidget(droppedOn, out var barterInvIdx))
                {
                    InsertIntoBarterContainer(ActingCritter, CurrentItem, barterInvIdx);
                }
                else if (UiSystems.Party.TryGetPartyMemberByWidget(droppedOn, out var partyMember))
                {
                    InsertIntoPartyPortrait(ActingCritter, CurrentItem, partyMember);
                }
                else if (UiSystems.CharSheet.Inventory.TryGetInventoryIdxForWidget(droppedOn, out var invIdx))
                {
                    InsertIntoInventorySlot(ActingCritter, CurrentItem, invIdx);
                }

                UiSystems.CharSheet.Inventory.DraggedObject = null;
                Tig.Mouse.ClearDraggedIcon();
            }
            else if (msg.flags.HasFlag(MouseEventFlag.RightReleased))
            {
                var critter = ActingCritter;
                var item = CurrentItem;
                if (item != null)
                {
                    // If the item is in another container, then we'll simply try to take it
                    if (GameSystems.Item.GetParent(item) != critter)
                    {

                    }
                    // If the item is currently equipped, unequip it,
                    else if (GameSystems.Item.IsEquipped(item))
                    {
                        UnequipItem(item, critter);
                    }
                    // otherwise equip it
                    else
                    {
                        EquipItem(item, critter);
                    }
                }

                return true;
            }

            return true;
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

        private void EquipItem(GameObjectBody item, GameObjectBody critter)
        {
            if (!AttemptEquipmentChangeInCombat(critter, item))
            {
                return;
            }

            if (IsSplittable(item, out var quantity))
            {
                var iconPath = item.GetInventoryIconPath();
                UiSystems.CharSheet.SplitItem(item, critter, 0, quantity,
                    iconPath, 2, -1, 0, ItemInsertFlag.Allow_Swap | ItemInsertFlag.Use_Wield_Slots);
            }

            var err = GameSystems.Item.ItemTransferWithFlags(item, critter, -1,
                ItemInsertFlag.Allow_Swap | ItemInsertFlag.Use_Wield_Slots, null);
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

            if (GameSystems.Script.ExecuteObjectScript(critter, item, 0, ObjScriptEvent.Use) == 0)
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
            var target = UiSystems.CharSheet.Looting.LootingContainer;
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
            var buyer = UiSystems.CharSheet.Looting.LootingContainer;
            if (!buyer.IsCritter())
            {
                buyer = UiSystems.CharSheet.Looting.Vendor;
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
                && tbStatus.hourglassState < HourglassState.MOVE &&
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
                            ActionCostType.Move);
                    tbStatus.tbsFlags |= TurnBasedStatusFlags.ChangedWornItem;
                }
            }

            return true;
        }
    }
}