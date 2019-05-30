using System;
using SpicyTemple.Core.GameObject;
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

        [TempleDllLocation(0x10159530)]
        public CharSheetInventoryUi()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10156e60)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10155040)]
        public void Show()
        {
            IsVisible = true;
            Stub.TODO();
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
            Stub.TODO();
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