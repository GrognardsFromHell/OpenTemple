using System;
using System.Collections.Generic;
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
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Portrait;

public class CharSheetPortraitUi : IDisposable
{
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
    public CharSheetPortraitUi()
    {
        var textures = Tig.FS.ReadMesFile("art/interface/CHAR_UI/CHAR_PORTRAIT_UI/2_char_portrait_ui_textures.mes");
        var rules = Tig.FS.ReadMesFile("art/interface/CHAR_UI/CHAR_PORTRAIT_UI/2_char_portrait_ui.mes");
        var uiParams = new PortraitUiParams(rules, textures);

        Container = new WidgetContainer
        {
            Pos = uiParams.MainWindow.Location,
            PixelSize = uiParams.MainWindow.Size
        };
        Container.Width = Dimension.Pixels(195);

        CreateButtons(uiParams);

        CreatePortraitContainer(uiParams);

        CreateMiniatureContainer(uiParams);

        CreatePaperdollContainer();

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
        _miniatureWidget.Width = Dimension.Percent(100);
        _miniatureWidget.Height = Dimension.Percent(100);
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
        _miniatureButton.Pos = uiParams.MiniatureButton.Location;
        _miniatureButton.SetStyle(new WidgetButtonStyle
        {
            NormalImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelNormal],
            ActivatedImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelSelected],
            PressedImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelPressed],
            HoverImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelHover],
            DisabledImagePath = uiParams.TexturePaths[PortraitUiTexture.Button3dModelDisabled]
        });
        _miniatureButton.AddClickListener(() => Mode = CharSheetPortraitMode.Miniature);
        _miniatureButton.TooltipText = UiSystems.Tooltip.GetString(5000);
        Container.Add(_miniatureButton);

        _portraitButton = new WidgetButton();
        _portraitButton.Pos = uiParams.PortraitButton.Location;
        _portraitButton.SetStyle(new WidgetButtonStyle
        {
            NormalImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitNormal],
            ActivatedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitSelected],
            PressedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitPressed],
            HoverImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitHover],
            DisabledImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPortraitDisabled]
        });
        _portraitButton.AddClickListener(() => Mode = CharSheetPortraitMode.Portrait);
        _portraitButton.TooltipText = UiSystems.Tooltip.GetString(5001);
        Container.Add(_portraitButton);

        _paperdollButton = new WidgetButton();
        _paperdollButton.Pos = uiParams.PaperdollButton.Location;
        _paperdollButton.SetStyle(new WidgetButtonStyle
        {
            NormalImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollNormal],
            ActivatedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollSelected],
            PressedImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollPressed],
            HoverImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollHover],
            DisabledImagePath = uiParams.TexturePaths[PortraitUiTexture.ButtonPaperdollDisabled]
        });
        _paperdollButton.AddClickListener(() => Mode = CharSheetPortraitMode.Paperdoll);
        _paperdollButton.TooltipText = UiSystems.Tooltip.GetString(5002);
        Container.Add(_paperdollButton);
    }

    private void CreatePaperdollContainer()
    {
        var widgetDoc = WidgetDoc.Load("ui/char_paperdoll.json", (type, definition) =>
        {
            if (type == "slot")
            {
                var slotName = definition.GetStringProp("slot");
                if (!EquipSlots.SlotsById.TryGetValue(slotName, out var slot))
                {
                    throw new ArgumentException("Invalid slot id: " + slotName);
                }

                return new PaperdollSlotWidget(slot);
            }

            return null;
        });
        _paperdollContainer = widgetDoc.GetRootContainer();

        // Create an equipslot widget for each slot that has a defined rectangle
        _slotWidgets = new Dictionary<EquipSlot, PaperdollSlotWidget>(EquipSlots.SlotsById.Count);
        foreach (var descendant in _paperdollContainer.EnumerateDescendantsInTreeOrder())
        {
            if (descendant is PaperdollSlotWidget slotWidget)
            {
                if (!_slotWidgets.TryAdd(slotWidget.Slot, slotWidget))
                {
                    throw new InvalidOperationException($"Two widgets in the paperdoll use the same slot: {slotWidget.Slot}");
                }

                new ItemSlotBehavior(slotWidget,
                    () => slotWidget.CurrentItem,
                    () => slotWidget.Critter);
            }
        }

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