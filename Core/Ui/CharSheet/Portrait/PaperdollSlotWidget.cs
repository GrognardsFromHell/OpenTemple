using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet.Inventory;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.CharSheet.Portrait;

public class PaperdollSlotWidget : WidgetContainer, IItemDropTarget
{
    private readonly EquipSlot _slot;

    private readonly PackedLinearColorA _slotHoverColor;

    private readonly PackedLinearColorA _slotPressedColor;

    private readonly WidgetText _quantityLabel;

    private readonly List<ResourceRef<ITexture>> _weaponSlotHighlights;

    public GameObject? CurrentItem
    {
        get
        {
            if (Critter != null)
            {
                return GameSystems.Item.ItemWornAt(Critter, _slot);
            }

            return null;
        }
    }

    public GameObject? Critter { get; set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _weaponSlotHighlights.DisposeAndClear();
    }

    public PaperdollSlotWidget(PortraitUiParams uiParams, Size size, EquipSlot slot) : base(size)
    {
        _slot = slot;

        _slotHoverColor = uiParams.InnerBorderColorHover;
        _slotPressedColor = uiParams.InnerBorderColorPressed;

        _weaponSlotHighlights = new List<ResourceRef<ITexture>>();

        void LoadWeaponSlotHighlight(PortraitUiTexture texture)
        {
            var path = uiParams.TexturePaths[texture];
            _weaponSlotHighlights.Add(Tig.Textures.Resolve(path, false));
        }

        LoadWeaponSlotHighlight(PortraitUiTexture.WeaponHighlightBlue);
        LoadWeaponSlotHighlight(PortraitUiTexture.WeaponHighlightGreen);
        LoadWeaponSlotHighlight(PortraitUiTexture.WeaponHighlightPurple);
        LoadWeaponSlotHighlight(PortraitUiTexture.WeaponHighlightRed);
        LoadWeaponSlotHighlight(PortraitUiTexture.WeaponHighlightYellow);

        _quantityLabel = CreateQuantityLabel();

        // We use a custom renderer, so hit-testing content won't work
        HitTesting = HitTestingMode.Area;
    }

    private static WidgetText CreateQuantityLabel()
    {
        return new WidgetText("", "inventory-slot-quantity");
    }

    public override void Render()
    {
        var itemInSlot = CurrentItem;
        if (itemInSlot != null)
        {
            RenderItemInSlot(itemInSlot, PackedLinearColorA.White);
        }
        else if (_slot == EquipSlot.WeaponSecondary)
        {
            RenderImplicitOffHandItem();
        }

        // Render the weapon-set color
        if (Critter != null && Critter.IsPC() && GameSystems.Item.IsSlotPartOfWeaponSet(_slot))
        {
            RenderWeaponSetColor();
        }
    }

    private void RenderImplicitOffHandItem()
    {
        var gray = new PackedLinearColorA(128, 128, 128, 255);

        var shield = GameSystems.Item.ItemWornAt(Critter, EquipSlot.Shield);
        if (shield != null)
        {
            if (shield.type == ObjectType.armor && shield.GetArmorFlags().IsShield())
            {
                // A buckler does not block the off-hand, while a normal shield does
                if (!shield.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                {
                    RenderItemInSlot(shield, gray);
                    return;
                }
            }
        }

        var primaryWeapon = GameSystems.Item.ItemWornAt(Critter, EquipSlot.WeaponPrimary);
        if (primaryWeapon != null)
        {
            var wieldType = GameSystems.Item.GetWieldType(Critter, primaryWeapon);
            if (wieldType == 2)
            {
                RenderItemInSlot(primaryWeapon, gray);
            }
        }
    }

    private void RenderItemInSlot(GameObject item, PackedLinearColorA itemTint)
    {
        // Don't render the item if it is currently being dragged elsewhere
        if (item == UiSystems.CharSheet.Inventory.DraggedObject)
        {
            return;
        }

        var texturePath = item.GetInventoryIconPath();
        using var texture = Tig.Textures.Resolve(texturePath, false);

        var arg = new Render2dArgs();
        arg.flags = Render2dFlag.VERTEXCOLORS | Render2dFlag.DISABLEBLENDING | Render2dFlag.BUFFERTEXTURE;
        // WeaponPrimary (flips the icon)
        if (_slot == EquipSlot.WeaponPrimary)
        {
            arg.flags |= Render2dFlag.FLIPH;
        }

        arg.customTexture = texture.Resource;

        arg.srcRect = new Rectangle(Point.Empty, texture.Resource.GetSize());
        var destRect = GetContentArea();
        destRect.Inflate(-3, -3);
        arg.destRect = destRect;
        arg.vertexColors = new[] {itemTint, itemTint, itemTint, itemTint};
        Tig.ShapeRenderer2d.DrawRectangle(ref arg);

        RenderItemQuantity(item);

        // Render the mouse hover/mouse down rectangle
        if (!GameSystems.Item.IsSlotPartOfWeaponSet(_slot))
        {
            var area = GetContentArea();
            area.Inflate(-1, -1);

            if (ContainsPress)
            {
                Tig.ShapeRenderer2d.DrawRectangleOutline(
                    area,
                    _slotPressedColor
                );
            }
            else if (ContainsMouse)
            {
                Tig.ShapeRenderer2d.DrawRectangleOutline(
                    area,
                    _slotHoverColor
                );
            }
        }
    }

    private void RenderItemQuantity(GameObject item)
    {
        // Renders the stack size on top of the slot
        if (item.TryGetQuantity(out var quantity))
        {
            var quantityText = quantity.ToString(CultureInfo.InvariantCulture);
            _quantityLabel.Text = quantityText;

            var contentArea = GetContentArea();

            // Position the label in the lower right corner
            var textSize = _quantityLabel.GetPreferredSize();
            _quantityLabel.SetBounds(new Rectangle(
                contentArea.Right - 2 - textSize.Width,
                contentArea.Bottom - 2 - textSize.Height,
                textSize.Width,
                textSize.Height
            ));
            _quantityLabel.Render();
        }
    }

    private void RenderWeaponSetColor()
    {
        var srcRect = new Rectangle(0, 0, 44, 44);
        var contentArea = GetContentArea();

        var destRect = new Rectangle(
            contentArea.X,
            contentArea.Y,
            44,
            44
        );

        var arg = new Render2dArgs();
        var idx = GameSystems.Item.GetWeaponSlotsIndex(Critter);
        Trace.Assert(idx >= 0 && idx < _weaponSlotHighlights.Count);
        arg.customTexture = _weaponSlotHighlights[idx].Resource;
        arg.flags = Render2dFlag.BUFFERTEXTURE;

        switch (_slot)
        {
            case EquipSlot.WeaponSecondary:
                arg.flags |= Render2dFlag.FLIPH;
                break;
            case EquipSlot.Ammo:
                arg.flags |= Render2dFlag.FLIPV;
                break;
            case EquipSlot.Shield:
                arg.flags |= Render2dFlag.FLIPH | Render2dFlag.FLIPV;
                break;
        }

        arg.srcRect = srcRect;
        arg.destRect = destRect;
        Tig.ShapeRenderer2d.DrawRectangle(ref arg);
    }

    protected override void HandleTooltip(TooltipEvent e)
    {
        if (Pressed)
        {
            return;
        }

        var itemInSlot = CurrentItem;
        if (itemInSlot == null || UiSystems.CharSheet.Inventory.DraggedObject == itemInSlot)
        {
            return;
        }

        UiSystems.CharSheet.Help.ShowItemDescription(itemInSlot, Critter);

        var tooltip = ItemTooltipBuilder.BuildItemTooltip(UiSystems.CharSheet.CurrentCritter, itemInSlot);

        if (tooltip != null)
        {
            e.TextContent = tooltip;
        }
    }

    public int InventoryIndex => GameSystems.Item.InvIdxForSlot(_slot);
}