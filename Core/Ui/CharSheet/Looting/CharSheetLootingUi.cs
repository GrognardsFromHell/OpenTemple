using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Styles;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Looting
{
    public class CharSheetLootingUi : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10BE6EE8)]
        internal bool IsVisible => _mainWindow.Visible;

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
        private readonly LootingSlotWidget[] _lootingSlots = new LootingSlotWidget[12];

        private const string DefaultLootingContainerIcon =
            "art/interface/char_ui/char_looting_ui/Looting_ChestPortrait.tga";

        // Maps from the proto id of the container being looted to the path of the portrait
        private static readonly Dictionary<int, string> LootingContainerIcons = new Dictionary<int, string>
        {
            {1030, "art/interface/char_ui/char_looting_ui/Anvil.tga"},
            {1052, "art/interface/char_ui/char_looting_ui/Anvil.tga"},
            {1028, "art/interface/char_ui/char_looting_ui/HommletWell.tga"},
            {1031, "art/interface/char_ui/char_looting_ui/RainbowRock.tga"},
            // Fix for Vanilla: This texture was never loaded, icon wouldn't show up
            {1045, "art/interface/char_ui/char_looting_ui/LarethDresser.tga"},
            // Fix for Vanilla: This texture was never loaded, icon dresser wouldn't show up
            {1047, "art/interface/char_ui/char_looting_ui/EarthAltar.tga"},
        };

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x10be6ea0)]
        private readonly WidgetContainer _mainWindow;

        internal WidgetContainer Container => _mainWindow;

        private WidgetScrollBar _scrollBar;

        private WidgetButton _takeAllButton;

        private WidgetLegacyText _title;

        private WidgetLegacyText _containerName;

        private WidgetImage _containerIcon;

        private WidgetButtonBase _containerIconButton;

        private TigTextStyle _titleStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_TRUNCATE | TigTextStyleFlag.TTSF_DROP_SHADOW | TigTextStyleFlag.TTSF_CENTER,
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            kerning = 1,
            tracking = 5
        };

        [TempleDllLocation(0x101412a0)]
        public CharSheetLootingUi()
        {
            var doc = WidgetDoc.Load("ui/char_looting.json");
            var root = doc.TakeRootContainer();
            root.SetVisible(false);

            _translations = Tig.FS.ReadMesFile("mes/6_char_looting_ui_text.mes");

            _mainWindow = new WidgetContainer(new Rectangle(7, 77, 137, 464));
            _mainWindow.SetVisible(false);
            _mainWindow.ZIndex = 100050;
            _mainWindow.Name = "char_looting_ui_main_window";
            _mainWindow.AddContent(new WidgetImage("art/interface/char_ui/char_looting_ui/looting_background.img"));
            // _mainWindow.OnHandleMessage += 0x1013ea00;
            _mainWindow.OnBeforeRender += UpdateSlots;

            // Window title
            _title = new WidgetLegacyText("", PredefinedFont.ARIAL_12, _titleStyle);
            _title.SetY(9);
            _title.SetFixedWidth(_mainWindow.Width);
            _mainWindow.AddContent(_title);

            // Container / Vendor name
            _containerName = new WidgetLegacyText("", PredefinedFont.ARIAL_12, _titleStyle);
            _containerName.SetY(80);
            _containerName.SetFixedWidth(_mainWindow.Width);
            _mainWindow.AddContent(_containerName);

            for (var i = 0; i < _lootingSlots.Length; i++)
            {
                var slotX = i % 2;
                var slotY = i / 2;

                var slotsOrigin = new Point(12, 128);
                var slotPos = slotsOrigin;
                slotPos.Offset(
                    slotX * (LootingSlotWidget.Size.Width + LootingSlotWidget.MarginRight),
                    slotY * (LootingSlotWidget.Size.Height + LootingSlotWidget.MarginBottom)
                );

                var slotWidget = new LootingSlotWidget(i, slotPos);
                slotWidget.InventorySlot = i;
                _mainWindow.Add(slotWidget);
                _lootingSlots[i] = slotWidget;
            }

            _takeAllButton = new WidgetButton(new Rectangle(9, 97, 120, 30));
            _takeAllButton.Name = "char_looting_ui_take_all_button";
            _takeAllButton.SetStyle("charLootingIdentify");
            _takeAllButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            _takeAllButton.SetClickHandler(OnClickTakeAllButton);
            _mainWindow.Add(_takeAllButton);

            _containerIconButton = new WidgetButtonBase(new Rectangle(41, 32, 53, 47));

            // Icon for the container being looted or vendor being bartered with
            _containerIcon = new WidgetImage(null);
            _containerIconButton.AddContent(_containerIcon);
            _containerIconButton.TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            _mainWindow.Add(_containerIconButton);

            _scrollBar = new WidgetScrollBar(new Rectangle(117, 131, 13, 324));
            _scrollBar.SetMax(100); // TODO This is actually shit because there are slots visible
            _scrollBar.SetValueChangeHandler(_ =>
            {
                ResetSlots();
                UpdateSlots();
            });
            /*_scrollBar.yMax = 100;
            _scrollBar.scrollQuantum = 1;
            _scrollBar.field8C = 6;*/
            _mainWindow.Add(_scrollBar);
        }

        /// <summary>
        /// This button handles both looting all items and starting the identify process, depending
        /// on whether a container is being looted or a vendor is being bartered with.
        /// </summary>
        [TempleDllLocation(0x1013eaf0)]
        private void OnClickTakeAllButton()
        {
            // The character that is interacting with the container / vendor
            var looter = UiSystems.CharSheet.CurrentCritter;
            if (Vendor != null)
            {
                // Toggle the identification mode
                IsIdentifying = !IsIdentifying;
                return;
            }

            // Make a copy of the items in the container since we're going to modify the content
            var items = new List<GameObjectBody>(EnumerateLootableItems());

            foreach (var item in items)
            {
                var error = GameSystems.Item.ItemTransferWithFlags(item, looter, -1, ItemInsertFlag.Unk4, null);
                // Cancel early if we ran out of room
                if (error == ItemErrorCode.No_Room_For_Item)
                {
                    Logger.Info("Ran out of room while {0} attempted to take all from {1}", looter, LootingContainer);
                    UiSystems.CharSheet.ItemTransferErrorPopup(error);
                    break;
                }

                if (error != ItemErrorCode.OK)
                {
                    UiSystems.CharSheet.ItemTransferErrorPopup(error);
                }
            }

            UpdateSlots();
        }

        private IEnumerable<GameObjectBody> EnumerateLootableItems()
        {
            return LootingContainer.EnumerateChildren()
                .Where(item => (item.GetItemFlags() & ItemFlag.NO_LOOT) == 0);
        }

        private void UpdateLabels()
        {
            if (Vendor != null)
            {
                _title.Text = _translations[1502];
                _takeAllButton.SetText(_translations[1501]);
                _takeAllButton.TooltipText = UiSystems.Tooltip.GetString(6030);
            }
            else
            {
                _title.Text = _translations[1503];
                _takeAllButton.SetText(_translations[1500]);
                _takeAllButton.TooltipText = null;
            }

            string displayName;
            if (Vendor != null)
            {
                displayName = GameSystems.MapObject.GetDisplayNameForParty(Vendor);
            }
            else
            {
                displayName = GameSystems.MapObject.GetDisplayNameForParty(LootingContainer);
            }

            _containerName.Text = displayName;
            _containerIconButton.TooltipText = displayName;
            _containerIcon.SetTexture(GetContainerIconPath());
        }

        [TempleDllLocation(0x1013de80)]
        private string GetContainerIconPath()
        {
            // For vendors, we just use the vendor's portrait
            var critter = Vendor;
            if (critter == null && LootingContainer.IsCritter())
            {
                critter = LootingContainer;
            }

            if (critter != null)
            {
                var portraitId = critter.GetInt32(obj_f.critter_portrait);
                return GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
            }

            return LootingContainerIcons.GetValueOrDefault(LootingContainer.ProtoId, DefaultLootingContainerIcon);
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
            IsIdentifying = false;
            Vendor = null;
            LootingContainer = null;
            ResetSlots();
            _scrollBar.SetValue(0);
        }

        private void ResetSlots()
        {
            foreach (var lootingSlot in _lootingSlots)
            {
                lootingSlot.Reset();
            }
        }

        private void UpdateSlots()
        {
            foreach (var lootingSlot in _lootingSlots)
            {
                UpdateSlot(lootingSlot);
            }
        }

        /// <summary>
        /// This was previously the rendering function, but we now use it to update
        /// what the slot displays.
        /// </summary>
        [TempleDllLocation(0x1013faf0)]
        private void UpdateSlot(LootingSlotWidget slot)
        {
            if (slot.InventorySlot == -1)
            {
                slot.InventorySlot = 2 * _scrollBar.GetValue() + slot.Index;
            }

            if (UiSystems.CharSheet.State == CharInventoryState.Bartering && slot.EquipmentSlot)
            {
                slot.SetItem(null);
                return;
            }

            var item = GameSystems.Item.GetItemAtInvIdx(LootingContainer, slot.InventorySlot);
            if (item != null)
            {
                slot.SetItem(item);
                return;
            }

            // Reset the slot
            if (slot.EquipmentSlot)
            {
                slot.EquipmentSlot = false;
                slot.InventorySlot = 0;
            }

            // When slots are empty and we're looting, they're auto-assigned to be equipment-slots so it is
            // easier to loot the character's equipment.
            // TODO: In my opinion, there should be something that "packs" the inventory of dead critters to get rid of
            // empty slots
            if (UiSystems.CharSheet.State == CharInventoryState.Looting)
            {
                foreach (var equipSlot in EquipSlots.Slots)
                {
                    var equipInvIdx = GameSystems.Item.InvIdxForSlot(equipSlot);

                    // If the inventory index is shown by a different looting slot already, skip it
                    var found = false;
                    foreach (var otherSlot in _lootingSlots)
                    {
                        if (otherSlot.EquipmentSlot && otherSlot.InventorySlot == equipInvIdx)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        continue;
                    }

                    item = GameSystems.Item.ItemWornAt(LootingContainer, equipSlot);
                    if (item != null)
                    {
                        slot.EquipmentSlot = true;
                        slot.InventorySlot = equipInvIdx;
                        slot.SetItem(item);
                        return;
                    }
                }
            }

            slot.Reset();
        }

        [TempleDllLocation(0x1013f6c0)]
        [TemplePlusLocation("ui_char.cpp:1413")]
        public void Show(GameObjectBody target)
        {
            Reset();
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

            UpdateLabels();
            _mainWindow.SetVisible(true);
            _mainWindow.BringToFront();
        }

        [TempleDllLocation(0x1013f880)]
        public void Hide()
        {
            if (LootingContainer != null)
            {
                if (UiSystems.CharSheet.State == CharInventoryState.Bartering)
                {
                    GameSystems.Item.ScheduleContainerRestock(LootingContainer);
                }

                if (Vendor == null && LootingContainer.type == ObjectType.container)
                {
                    GameSystems.Anim.PushAnimate(LootingContainer, NormalAnimType.Close);
                }
            }

            Reset();
            _mainWindow.SetVisible(false);
        }

        [TempleDllLocation(0x1013f9c0)]
        public bool TryGetInventoryIdxForWidget(WidgetBase widget, out int inventoryIndex)
        {
            var offset = 2 * _scrollBar.GetValue();
            foreach (var lootingSlot in _lootingSlots)
            {
                if (lootingSlot == widget)
                {
                    // This returns the theoretical index, not the actually displayed index, which is somewhat fishy
                    inventoryIndex = lootingSlot.Index + offset;
                    return true;
                }
            }

            inventoryIndex = -1;
            return false;
        }

        [TempleDllLocation(0x1013de00)]
        public CharInventoryState GetLootingState()
        {
            if (!IsVisible)
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