using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Ui.CharSheet.Inventory
{
    public class InventorySlotWidget : WidgetContainer, IItemDropTarget
    {
        private static readonly PackedLinearColorA SlotBackground = PackedLinearColorA.Black;

        private static readonly PackedLinearColorA SlotNormalOutline = new PackedLinearColorA(100, 100, 100, 255);

        private static readonly PackedLinearColorA SlotRedOutline = new PackedLinearColorA(255, 0, 0, 255);

        private static readonly PackedLinearColorA SlotCantWearOutline = new PackedLinearColorA(216, 16, b: 0x10, 255);

        private readonly PackedLinearColorA _slotHoverColor;

        private readonly PackedLinearColorA _slotPressedColor;

        private readonly WidgetLegacyText _quantityLabel;

        private readonly WidgetTooltipRenderer _tooltipRenderer = new WidgetTooltipRenderer();

        private readonly WidgetRectangle _background;

        public GameObjectBody Inventory { get; set; }

        public int InventoryIndex { get; }

        public GameObjectBody CurrentItem
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

            _tooltipRenderer.TooltipStyle = UiSystems.Tooltip.GetStyle(0);

            _background = new WidgetRectangle();
            _background.Brush = new Brush(SlotBackground);
            _background.Pen = SlotNormalOutline;
            AddContent(_background);
        }

        public static WidgetLegacyText CreateQuantityLabel()
        {
            var bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, a: 153));
            var shadowColor = new ColorRect(PackedLinearColorA.Black);
            var textColor = new ColorRect(PackedLinearColorA.White);

            var quantityTextStyle = new TigTextStyle
            {
                bgColor = bgColor,
                textColor = textColor,
                flags = TigTextStyleFlag.TTSF_BORDER | TigTextStyleFlag.TTSF_BACKGROUND |
                        TigTextStyleFlag.TTSF_DROP_SHADOW,
                shadowColor = shadowColor,
                kerning = 2,
                tracking = 2
            };

            return new WidgetLegacyText("", PredefinedFont.ARIAL_10, quantityTextStyle);
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

        private void RenderItemInSlot(GameObjectBody item, PackedLinearColorA itemTint)
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

            if (MouseState == LgcyWindowMouseState.Hovered)
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
            else if (MouseState == LgcyWindowMouseState.Pressed)
            {
                color = _slotPressedColor;
            }

            if (currentItem.type == ObjectType.weapon)
            {
                var weaponType = currentItem.GetWeaponType();
                if (GameSystems.Feat.IsProficientWithWeaponType(critter, weaponType))
                {
                    color = SlotRedOutline;
                }
            }
            else if (currentItem.type == ObjectType.armor)
            {
                var wearFlags = currentItem.GetItemWearFlags();
                if (wearFlags.HasFlag(ItemWearFlag.ARMOR))
                {
                    if ( !currentItem.GetArmorFlags().HasFlag(ArmorFlag.TYPE_NONE) )
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

        private void RenderItemQuantity(GameObjectBody item)
        {
            // Renders the stack size on top of the slot
            if (item.TryGetQuantity(out var quantity))
            {
                var quantityText = quantity.ToString(CultureInfo.InvariantCulture);
                _quantityLabel.Text = quantityText;

                var contentArea = GetContentArea();

                // Position the label in the lower right corner
                var textSize = _quantityLabel.GetPreferredSize();
                _quantityLabel.SetContentArea(new Rectangle(
                    contentArea.Right - 2 - textSize.Width,
                    contentArea.Bottom - 2 - textSize.Height,
                    textSize.Width,
                    textSize.Height
                ));
                _quantityLabel.Render();
            }
        }

        public override void RenderTooltip(int x, int y)
        {
            if (MouseState == LgcyWindowMouseState.Pressed)
            {
                return;
            }

            var itemInSlot = CurrentItem;
            if (itemInSlot == null || UiSystems.CharSheet.Inventory.DraggedObject == itemInSlot)
            {
                return;
            }

            UiSystems.CharSheet.Help.ShowItemDescription(itemInSlot, Inventory);

            var tooltip = ItemTooltipBuilder.BuildItemTooltip(UiSystems.CharSheet.CurrentCritter, itemInSlot);
            if (tooltip != null)
            {
                _tooltipRenderer.TooltipText = tooltip;
                _tooltipRenderer.Render(x, y);
            }
        }
    }
}