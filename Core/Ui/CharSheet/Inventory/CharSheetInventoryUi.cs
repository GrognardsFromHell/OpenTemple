using System;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Ui.CharSheet.Inventory
{
    public class CharSheetInventoryUi : IDisposable
    {
        [TempleDllLocation(0x101552C0)]
        [TempleDllLocation(0x10BEECC8)]
        public int BagIndex { get; set; }

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
        }

        [TempleDllLocation(0x10155170)]
        public void SetContainer(GameObjectBody container)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10BEECD0)]
        public bool IsVisible { get; private set; }
    }
}