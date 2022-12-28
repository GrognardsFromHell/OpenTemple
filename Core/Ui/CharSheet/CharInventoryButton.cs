using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet;

public class CharInventoryButton : WidgetButton
{
    // 0 is the default inventory
    private readonly int _inventoryIdx;
    private readonly ResourceRef<ITexture> _borderTexture;
    private readonly ResourceRef<ITexture> _arcTopTexture;
    private readonly ResourceRef<ITexture> _arcBottomTexture;
    private readonly ResourceRef<ITexture> _bagIconTexture;

    public CharInventoryButton(CharUiParams uiParams, int inventoryIdx)
    {
        _inventoryIdx = inventoryIdx;

        Rectangle rectangle;
        switch (inventoryIdx)
        {
            case 0:
                rectangle = uiParams.CharUiSelectInventory0Button;
                break;
            case 1:
                rectangle = uiParams.CharUiSelectInventory1Button;
                break;
            case 2:
                rectangle = uiParams.CharUiSelectInventory2Button;
                break;
            case 3:
                rectangle = uiParams.CharUiSelectInventory3Button;
                break;
            case 4:
                rectangle = uiParams.CharUiSelectInventory4Button;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Pos = rectangle.Location;
        PixelSize = rectangle.Size;

        _borderTexture = Tig.Textures.Resolve(uiParams.TexturePaths[CharUiTexture.ButtonContainerSelected], false);
        _arcTopTexture = Tig.Textures.Resolve(uiParams.TexturePaths[CharUiTexture.ButtonArcTopSelected], false);
        _arcBottomTexture =
            Tig.Textures.Resolve(uiParams.TexturePaths[CharUiTexture.ButtonArcBottomSelected], false);
        _bagIconTexture = Tig.Textures.Resolve(uiParams.TexturePaths[CharUiTexture.BagTemp], false);
    }

    [TempleDllLocation(0x10145560)]
    [TempleDllLocation(0x10145760)]
    [TempleDllLocation(0x10145990)]
    [TempleDllLocation(0x10145be0)]
    [TempleDllLocation(0x10145e40)]
    public override void Render(UiRenderContext context)
    {
        using var bagTexture = GetBagTexture();
        if (!bagTexture.IsValid)
        {
            return;
        }

        var contentArea = GetContentArea();

        // This draws the blue selection border around the inventory button
        if (UiSystems.CharSheet.CurrentPage == _inventoryIdx)
        {
            RenderSelection(contentArea);
        }

        // Draw the bag icon
        if (!Disabled)
        {
            Tig.ShapeRenderer2d.DrawRectangle(
                contentArea.X,
                contentArea.Y - 1,
                contentArea.Width,
                contentArea.Height,
                bagTexture.Resource
            );
        }
    }

    private ResourceRef<ITexture> GetBagTexture()
    {
        if (_inventoryIdx == 0)
        {
            return _bagIconTexture;
        }
        else
        {
            var currentCritter = UiSystems.CharSheet.CurrentCritter;
            if (currentCritter == null)
            {
                return default;
            }

            GameObject bagItem;
            switch (_inventoryIdx)
            {
                case 1:
                    bagItem = GameSystems.Item.ItemWornAt(currentCritter, EquipSlot.Bag1);
                    break;
                case 2:
                    bagItem = GameSystems.Item.ItemWornAt(currentCritter, EquipSlot.Bag2);
                    break;
                case 3:
                    bagItem = GameSystems.Item.ItemWornAt(currentCritter, EquipSlot.Bag3);
                    break;
                case 4:
                    bagItem = GameSystems.Item.ItemWornAt(currentCritter, EquipSlot.Bag4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (bagItem == null)
            {
                return default;
            }

            var texturePath = bagItem.GetInventoryIconPath();
            return Tig.Textures.Resolve(texturePath, false);
        }
    }

    private void RenderSelection(RectangleF contentArea)
    {
        Tig.ShapeRenderer2d.DrawRectangle(
            contentArea.X - 4,
            contentArea.Y - 5,
            contentArea.Width + 12,
            contentArea.Height + 8,
            _borderTexture.Resource
        );

        Tig.ShapeRenderer2d.DrawRectangle(
            contentArea.X + 11,
            contentArea.Y - 23,
            21,
            21,
            _arcTopTexture.Resource
        );

        Tig.ShapeRenderer2d.DrawRectangle(
            contentArea.X + 11,
            contentArea.Y + contentArea.Height,
            21,
            21,
            _arcBottomTexture.Resource
        );
    }
}