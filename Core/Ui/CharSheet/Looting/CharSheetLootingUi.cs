using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Looting
{
    public class CharSheetLootingUi : IDisposable
    {
        [TempleDllLocation(0x10BE6EE8)]
        private bool _visible;

        [TempleDllLocation(0x10BE6EB8)]
        private int dword_10BE6EB8;

        [TempleDllLocation(0x101412a0)]
        public CharSheetLootingUi()
        {
        }

        [TempleDllLocation(0x1013dd50)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013de40)]
        [TempleDllLocation(0x10BE6EB8)]
        public bool IsIdentifying { get; set; }

        [TempleDllLocation(0x1013dd20)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013f6c0)]
        public void Show(GameObjectBody target)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013f880)]
        public void Hide()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013f9c0)]
        public bool TryGetInventoryIdxForWidget(WidgetBase widget, out int inventoryIndex)
        {
            Stub.TODO();
            inventoryIndex = -1;
            return false;
        }

        [TempleDllLocation(0x1013de00)]
        public int GetLootingState()
        {
            if (!_visible)
            {
                return 0;
            }

            return (int) UiSystems.CharSheet.State;
        }

        [TempleDllLocation(0x1013de30)]
        [TempleDllLocation(0x10BE6EC0)]
        public GameObjectBody Target { get; private set; }

        [TempleDllLocation(0x10BE6EC8)]
        [TempleDllLocation(0x1013de20)]
        public GameObjectBody TargetCritter { get; private set; }

        [TempleDllLocation(0x1013ddf0)]
        public CursorType? GetCursor()
        {
            if (dword_10BE6EB8 != 0)
            {
                return CursorType.IdentifyCursor;
            }
            else
            {
                return null;
            }
        }
    }
}