using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Inventory;

public class InventorySlotWidget : WidgetContainer, IItemDropTarget
{
    private static readonly PackedLinearColorA SlotBackground = PackedLinearColorA.Black;

    private static readonly PackedLinearColorA SlotNormalOutline = new(100, 100, 100, 255);

    private static readonly PackedLinearColorA SlotRedOutline = new(255, 0, 0, 255);

    private static readonly PackedLinearColorA SlotCantWearOutline = new(216, 16, b: 0x10, 255);

    private readonly PackedLinearColorA _slotHoverColor;

    private readonly PackedLinearColorA _slotPressedColor;

    private readonly WidgetText _quantityLabel;

    private readonly WidgetRectangle _background;

    public GameObject? Inventory { get; set; }

    public int InventoryIndex { get; }

    public GameObject? CurrentItem
    {
        get
        {
            if (Inventory != null)
            {
                return GameSystems.Item.GetItemAtInvIdx(Inventory, InventoryIndex);
            }

            return null;
        }
    }

    public InventorySlotWidget(Size size, int inventoryIdx) : base(size)
    {
        InventoryIndex = inventoryIdx;

        _slotHoverColor = new PackedLinearColorA(13, 107, 227, 255);
        _slotPressedColor = new PackedLinearColorA(255, 16, 16, 255);

        _quantityLabel = CreateQuantityLabel();

        _background = new WidgetRectangle();
        _background.Brush = new Brush(SlotBackground);
        _background.Pen = SlotNormalOutline;
        AddContent(_background);
    }

    public static WidgetText CreateQuantityLabel()
    {
        return new WidgetText("", "inventory-slot-quantity");
    }

    public override void Render()
    {
        _background.Pen = GetOutlineColor();

        base.Render();

        var itemInSlot = CurrentItem;
        if (itemInSlot != null)
        {
            RenderItemInSlot(itemInSlot, PackedLinearColorA.White);
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

        arg.customTexture = texture.Resource;

        arg.srcRect = new Rectangle(Point.Empty, texture.Resource.GetSize());
        var destRect = GetContentArea();
        destRect.Inflate(-3, -3);
        arg.destRect = destRect;
        arg.vertexColors = new[] {itemTint, itemTint, itemTint, itemTint};
        Tig.ShapeRenderer2d.DrawRectangle(ref arg);

        RenderItemQuantity(item);
    }

    private PackedLinearColorA GetOutlineColor()
    {
        var currentItem = CurrentItem;
        if (currentItem == null)
        {
            return SlotNormalOutline;
        }

        var critter = UiSystems.CharSheet.CurrentCritter;
        var color = SlotNormalOutline;

        if (ContainsPress)
        {
            color = _slotPressedColor;
        }
        else if (ContainsMouse)
        {
            color = _slotHoverColor;

            // If the weapon cannot be wielded, mark it as red
            if (currentItem.type == ObjectType.weapon)
            {
                if (GameSystems.Item.GetWieldType(critter, currentItem) == 3)
                {
                    color = SlotCantWearOutline;
                }
            }
        }

        if (currentItem.type == ObjectType.weapon)
        {
            if (!GameSystems.Feat.IsProficientWithWeapon(critter, currentItem))
            {
                color = SlotRedOutline;
            }
        }
        else if (currentItem.type == ObjectType.armor)
        {
            var wearFlags = currentItem.GetItemWearFlags();
            if (wearFlags.HasFlag(ItemWearFlag.ARMOR))
            {
                if (!currentItem.GetArmorFlags().HasFlag(ArmorFlag.TYPE_NONE))
                {
                    if (!GameSystems.Feat.IsProficientWithArmor(critter, currentItem))
                    {
                        color = SlotRedOutline;
                    }
                }
            }
        }

        return color;
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

    protected override void HandleMouseEnter(MouseEvent e)
    {
        var itemInSlot = CurrentItem;
        if (itemInSlot != null)
        {
            UiSystems.CharSheet.Help.ShowItemDescription(itemInSlot, Inventory);
        }
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

        e.TextContent = ItemTooltipBuilder.BuildItemTooltip(UiSystems.CharSheet.CurrentCritter, itemInSlot);
    }
}