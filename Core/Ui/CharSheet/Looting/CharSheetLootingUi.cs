using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Looting
{
    internal class LootingSlot
    {
        public int Flags;
        public int InvIndex = -1;
        public LootingSlotWidget Widget;

        public void Reset()
        {
            InvIndex = -1;
            Flags = 0;
        }
    }

    public class CharSheetLootingUi : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10BE6EE8)]
        private bool _visible;

        [TempleDllLocation(0x10BE6EB8)]
        private int dword_10BE6EB8;

        [TempleDllLocation(0x10be6ef0)]
        private bool _identifyIsActive;

        [TempleDllLocation(0x1013de30)]
        [TempleDllLocation(0x10BE6EC0)]
        public GameObjectBody LootingContainer { get; private set; }

        [TempleDllLocation(0x10BE6EC8)]
        [TempleDllLocation(0x1013de20)]
        public GameObjectBody Vendor { get; private set; }

        [TempleDllLocation(0x10be6e9c)] [TempleDllLocation(0x10be6eb4)]
        private LootingSlot[] _lootingSlots = new LootingSlot[12];

        private static readonly string[] LootingContainerIcons =
        {
            "art/interface/char_ui/char_looting_ui/Looting_ChestPortrait.tga",
            "art/interface/char_ui/char_looting_ui/Anvil.tga",
            "art/interface/char_ui/char_looting_ui/HommletWell.tga",
            "art/interface/char_ui/char_looting_ui/RainbowRock.tga"
        };

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x10be6ea0)]
        private readonly WidgetContainer _mainWindow;

        [TempleDllLocation(0x10be6ea4)]
        private WidgetContainer _lootingPortrait;

        private WidgetScrollBar _scrollBar;

        [TempleDllLocation(0x101412a0)]
        public CharSheetLootingUi()
        {
            _translations = Tig.FS.ReadMesFile("mes/6_char_looting_ui_text.mes");

            _mainWindow = new WidgetContainer(new Rectangle(4, 73, 138, 478));
            _mainWindow.SetVisible(false);
            _mainWindow.AddContent(new WidgetImage("art/interface/char_ui/char_looting_ui/looting_background.img"));
            // _mainWindow.OnHandleMessage += 0x1013ea00;

            for (var i = 0; i < _lootingSlots.Length; i++)
            {
                var lootingSlot = new LootingSlot();

                var slotX = i % 2;
                var slotY = i / 2;

                var slotsOrigin = new Point(12, 128);
                var slotPos = slotsOrigin;
                slotPos.Offset(
                    slotX * (LootingSlotWidget.Size.Width + LootingSlotWidget.MarginRight),
                    slotY * (LootingSlotWidget.Size.Height + LootingSlotWidget.MarginBottom)
                );

                var slotWidget = new LootingSlotWidget(slotPos);
                _mainWindow.Add(slotWidget);
                lootingSlot.Widget = slotWidget;

                _lootingSlots[i] = lootingSlot;
            }

            var char_looting_ui_take_all_button1 = new WidgetButton(new Rectangle(14, 101, 111, 20));
            char_looting_ui_take_all_button1.SetStyle(new WidgetButtonStyle());
            // char_looting_ui_take_all_button1.OnHandleMessage += 0x1013eaf0;
            // char_looting_ui_take_all_button1.OnBeforeRender += 0x1013e380;
            // char_looting_ui_take_all_button1.OnRenderTooltip += 0x1013e870;
            _mainWindow.Add(char_looting_ui_take_all_button1);

            _lootingPortrait = new WidgetContainer(new Rectangle(45, 105, 53, 47));
            // char_looting_ui_portrait_window1.OnBeforeRender += 0x1013de80;
            // char_looting_ui_portrait_window1.OnRenderTooltip += 0x1013e6c0;
            _mainWindow.Add(_lootingPortrait);

            _scrollBar = new WidgetScrollBar(new Rectangle(117, 131, 13, 324));
            // 1.OnHandleMessage += 0x101fa410;
            // 1.OnBeforeRender += 0x101fa1b0;
            /*_scrollBar.yMax = 100;
            _scrollBar.scrollQuantum = 1;
            _scrollBar.field8C = 6;*/
            _mainWindow.Add(_scrollBar);
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

        private void ResetSlots()
        {
            foreach (var lootingSlot in _lootingSlots)
            {
                lootingSlot.Reset();
            }
        }

        [TempleDllLocation(0x1013f6c0)]
        [TemplePlusLocation("ui_char.cpp:1413")]
        public void Show(GameObjectBody target)
        {
            GameObjectBody v6;

            var v1 = 0;
            IsIdentifying = false;

            if (target != null)
            {
                LootingContainer = target;
            }
            else if (LootingContainer == null)
            {
                Logger.Info("Cannot show looting without either a current object or a target object.");
                return;
            }

            if (UiSystems.CharSheet.State == CharInventoryState.Bartering && LootingContainer.IsNPC())
            {
                var substituteInventory = LootingContainer.GetObject(obj_f.npc_substitute_inventory);
                if (substituteInventory != null)
                {
                    Vendor = LootingContainer;
                    LootingContainer = substituteInventory;

                    // This is actually dubious, it auto-identifies everything in a vendor's inventory,
                    // which makes "gambling" like vendors that sell unidentified stuff impossible.
                    foreach (var item in substituteInventory.EnumerateChildren())
                    {
                        item.SetItemFlag(ItemFlag.IDENTIFIED, true);
                    }
                }
            }

            _visible = true;
            _mainWindow.SetVisible(true);
            _mainWindow.BringToFront();
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
        public CharInventoryState GetLootingState()
        {
            if (!_visible)
            {
                return CharInventoryState.Closed;
            }

            return UiSystems.CharSheet.State;
        }

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