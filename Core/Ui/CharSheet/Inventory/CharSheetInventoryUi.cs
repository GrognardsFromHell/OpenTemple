using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Inventory
{
    public class CharSheetInventoryUi : IDisposable
    {
        [TempleDllLocation(0x101552C0)]
        [TempleDllLocation(0x10BEECC8)]
        public int BagIndex { get; set; }

        [TempleDllLocation(0x101552a0)]
        [TempleDllLocation(0x10155290)]
        [TempleDllLocation(0x10BEECB8)]
        public GameObjectBody Container { get; set; }

        public WidgetBase Widget { get; set; }

        [TempleDllLocation(0x10BEECC0)]
        private GameObjectBody _draggedObject;

        [TempleDllLocation(0x10155160)]
        [TempleDllLocation(0x10155170)]
        public GameObjectBody DraggedObject
        {
            get => _draggedObject;
            set
            {
                _draggedObject = value;
                Globals.UiManager.IsDragging = value != null;
            }
        }

        private static readonly Size SlotSize = new Size(65, 65);

        private List<InventorySlotWidget> _slots = new List<InventorySlotWidget>();

        [TempleDllLocation(0x10159530)]
        public CharSheetInventoryUi()
        {
            Stub.TODO();

            var widgetDoc = WidgetDoc.Load("ui/char_inventory.json");
            Widget = widgetDoc.TakeRootContainer();

            UseItemWidget = widgetDoc.GetWidget("useItemButton");
            DropItemWidget = widgetDoc.GetWidget("dropItemButton");

            var slotContainer = widgetDoc.GetWindow("slotsContainer");
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    var inventoryIdx = row * 6 + col;
                    var slot = new InventorySlotWidget(SlotSize, inventoryIdx);
                    var x = 1 + col * (slot.GetWidth() + 2);
                    var y = 1 + row * (slot.GetHeight() + 2);
                    slot.SetPos(new Point(x, y));
                    slotContainer.Add(slot);
                    new ItemSlotBehavior(slot,
                        () => slot.CurrentItem,
                        () => UiSystems.CharSheet.CurrentCritter);
                    _slots.Add(slot);
                }
            }
        }

        [TempleDllLocation(0x10156e60)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10155040)]
        public void Show(GameObjectBody critter)
        {
            IsVisible = true;
            Stub.TODO();

            foreach (var slotWidget in _slots)
            {
                slotWidget.Inventory = critter;
            }
        }

        [TempleDllLocation(0x10156f00)]
        public void Hide()
        {
            IsVisible = false;
            Stub.TODO();
        }

        [TempleDllLocation(0x10156e50)]
        public void Reset()
        {
            BagIndex = 0;

            if (DraggedObject != null)
            {
                DraggedObject = null;
                Tig.Mouse.ClearDraggedIcon();
            }
        }

        [TempleDllLocation(0x10155170)]
        public void SetContainer(GameObjectBody container)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10BEECD0)]
        public bool IsVisible { get; private set; }

        [TempleDllLocation(0x101551a0)]
        public WidgetBase UseItemWidget { get; } // TODO

        [TempleDllLocation(0x101551b0)]
        [TempleDllLocation(0x102FB278)]
        public WidgetBase DropItemWidget { get; } // TODO

        [TempleDllLocation(0x10157030)]
        [TempleDllLocation(0x102FB27C)]
        public bool TryGetInventoryIdxForWidget(WidgetBase widget, out int invIdx)
        {
            if (widget is InventorySlotWidget inventorySlotWidget)
            {
                invIdx = inventorySlotWidget.InventoryIndex;
                return true;
            }

            invIdx = -1;
            return false;
        }

        [TempleDllLocation(0x10156df0)]
        public bool EquippingIsAction(GameObjectBody obj)
        {
            if (obj.type == ObjectType.armor)
            {
                if (!obj.GetArmorFlags().IsShield())
                {
                    if (!obj.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x10155260)]
        public bool IsCloseEnoughToTransferItem(GameObjectBody from, GameObjectBody to)
        {
            return from.DistanceToObjInFeet(to) < 40;
        }
    }
}