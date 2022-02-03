using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet.Inventory;
using OpenTemple.Core.Ui.CharSheet.Looting;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Portrait
{
    public class CharSheetPortraitUi : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

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
                _miniatureContainer.Visible = value == CharSheetPortraitMode.Miniature;
                _portraitButton.SetActive(value == CharSheetPortraitMode.Portrait);
                _portraitContainer.Visible = value == CharSheetPortraitMode.Portrait;
                _paperdollButton.SetActive(value == CharSheetPortraitMode.Paperdoll);
                _paperdollContainer.Visible = value == CharSheetPortraitMode.Paperdoll;
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
            Container.Width = 195;

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
            _portraitImage.X = (uiParams.PortraitWindow.Width - uiParams.Portrait.Width) / 2;
            _portraitImage.Y = (uiParams.PortraitWindow.Height - uiParams.Portrait.Height) / 2;
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
            backgroundImage.FixedWidth = backgroundImage.GetPreferredSize().Width;
            backgroundImage.FixedHeight = backgroundImage.GetPreferredSize().Height;
            container.AddContent(backgroundImage);
        }

        private void UpdatePortraitImage(GameObject critter)
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
                NormalImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelNormal],
                ActivatedImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelSelected],
                PressedImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelPressed],
                HoverImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelHover],
                DisabledImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelDisabled]
            });
            _miniatureButton.SetClickHandler(() => Mode = CharSheetPortraitMode.Miniature);
            _miniatureButton.TooltipText = UiSystems.Tooltip.GetString(5000);
            Container.Add(_miniatureButton);

            _portraitButton = new WidgetButton();
            _portraitButton.SetPos(uiParams.PortraitButton.Location);
            _portraitButton.SetStyle(new WidgetButtonStyle
            {
                NormalImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitNormal],
                ActivatedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitSelected],
                PressedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitPressed],
                HoverImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitHover],
                DisabledImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitDisabled]
            });
            _portraitButton.SetClickHandler(() => Mode = CharSheetPortraitMode.Portrait);
            _portraitButton.TooltipText = UiSystems.Tooltip.GetString(5001);
            Container.Add(_portraitButton);

            _paperdollButton = new WidgetButton();
            _paperdollButton.SetPos(uiParams.PaperdollButton.Location);
            _paperdollButton.SetStyle(new WidgetButtonStyle
            {
                NormalImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollNormal],
                ActivatedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollSelected],
                PressedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollPressed],
                HoverImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollHover],
                DisabledImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollDisabled]
            });
            _paperdollButton.SetClickHandler(() => Mode = CharSheetPortraitMode.Paperdoll);
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
            _paperdollContainer = widgetDoc.GetRootContainer();

            // Create an equipslot widget for each slot that has a defined rectangle
            _slotWidgets = SlotWidgetIds.ToDictionary(
                kp => kp.Key,
                kp =>
                {
                    var parent = widgetDoc.GetContainer(kp.Value);
                    var slotWidget = new PaperdollSlotWidget(uiParams, parent.GetSize(), kp.Key);
                    new ItemSlotBehavior(slotWidget,
                        () => slotWidget.CurrentItem,
                        () => slotWidget.Critter);
                    parent.Add(slotWidget);
                    return slotWidget;
                }
            );

            Container.Add(_paperdollContainer);
        }

        private void UpdateMiniature(GameObject critter)
        {
            _miniatureWidget.Object = critter;
        }

        [TempleDllLocation(0x101a3150)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101a3180)]
        public void Show(GameObject critter)
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