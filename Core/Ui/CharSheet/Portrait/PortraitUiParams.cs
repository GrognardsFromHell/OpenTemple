using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.CharSheet.Portrait;

public class PortraitUiParams
{
    public readonly Dictionary<PortraitUiTexture, string> TexturePaths;

    /// <summary>
    /// Encompasses all the other parts of the char portrait ui.
    /// </summary>
    public Rectangle MainWindow { get; set; }

    public Rectangle MiniatureWindow { get; }
    public Rectangle PortraitWindow { get; }
    public Rectangle PaperdollWindow { get; }
    public Rectangle MiniatureButton { get; }
    public Rectangle PortraitButton { get; }
    public Rectangle PaperdollButton { get; }

    public Dictionary<EquipSlot, Rectangle> Slots { get; }

    public Rectangle TabNavWindow { get; }

    public Rectangle[] TabButtons { get; }

    public PackedLinearColorA InnerBorderColorHover { get; }

    public PackedLinearColorA InnerBorderColorPressed { get; }

    public PackedLinearColorA NormalFontColor { get; }

    public PackedLinearColorA DarkFontColor { get; }

    public PackedLinearColorA PaperdollBorderColor { get; }

    public Size Portrait { get; }

    public PortraitUiParams(
        Dictionary<int, string> settings,
        Dictionary<int, string> texturePaths)
    {
        TexturePaths = new Dictionary<PortraitUiTexture, string>();
        foreach (var textureId in (PortraitUiTexture[]) Enum.GetValues(typeof(PortraitUiTexture)))
        {
            TexturePaths[textureId] = "art/interface/char_ui/char_portrait_ui/" + texturePaths[(int) textureId];
        }

        Rectangle LoadRectangle(int baseId, bool makeRelative = true)
        {
            var rect = new Rectangle(
                int.Parse(settings[baseId]),
                int.Parse(settings[baseId + 1]),
                int.Parse(settings[baseId + 2]),
                int.Parse(settings[baseId + 3])
            );
            if (makeRelative)
            {
                rect.X -= 68;
                rect.Y -= 47;
            }

            return rect;
        }

        PackedLinearColorA LoadColor(int baseId)
        {
            return new PackedLinearColorA(
                byte.Parse(settings[baseId]),
                byte.Parse(settings[baseId + 1]),
                byte.Parse(settings[baseId + 2]),
                byte.Parse(settings[baseId + 3])
            );
        }

        MiniatureWindow = LoadRectangle(0);
        PortraitWindow = LoadRectangle(10);
        PaperdollWindow = LoadRectangle(20);
        var buttonWindow = LoadRectangle(30);
        MiniatureButton = LoadRectangle(60);
        PortraitButton = LoadRectangle(80);
        PaperdollButton = LoadRectangle(100);

        Slots = new Dictionary<EquipSlot, Rectangle>();
        Slots[EquipSlot.Necklace] = LoadRectangle(120);
        Slots[EquipSlot.WeaponPrimary] = LoadRectangle(140);
        Slots[EquipSlot.RingPrimary] = LoadRectangle(160);
        Slots[EquipSlot.Helmet] = LoadRectangle(180);
        Slots[EquipSlot.Armor] = LoadRectangle(200);
        Slots[EquipSlot.Boots] = LoadRectangle(220);
        Slots[EquipSlot.Gloves] = LoadRectangle(240);
        Slots[EquipSlot.WeaponSecondary] = LoadRectangle(260);
        Slots[EquipSlot.RingSecondary] = LoadRectangle(280);
        Slots[EquipSlot.Cloak] = LoadRectangle(300);
        Slots[EquipSlot.Ammo] = LoadRectangle(320);
        Slots[EquipSlot.Shield] = LoadRectangle(340);
        Slots[EquipSlot.Robes] = LoadRectangle(350);
        Slots[EquipSlot.Bracers] = LoadRectangle(354);
        Slots[EquipSlot.BardicItem] = LoadRectangle(360);
        Slots[EquipSlot.Lockpicks] = LoadRectangle(380);
        // The slots are all incorrectly sized and positioned akwardly...
        Slots = Slots.ToDictionary(
            kp => kp.Key,
            kp => new Rectangle(
                kp.Value.X - MiniatureWindow.X,
                kp.Value.Y - MiniatureWindow.Y - 1,
                kp.Value.Width + 2,
                kp.Value.Height + 2
            )
        );

        TabNavWindow = LoadRectangle(390, false);
        TabButtons = new Rectangle[5];
        for (int i = 0; i < 5; i++)
        {
            TabButtons[i] = LoadRectangle(400 + 20 * i, false);
            TabButtons[i].X -= TabNavWindow.X;
            TabButtons[i].Y -= TabNavWindow.Y;
        }

        TabNavWindow = LoadRectangle(390); // Reload, but relative

        InnerBorderColorHover = LoadColor(490);
        InnerBorderColorPressed = LoadColor(494);
        // var fontName = settings[500];
        // var fontSize = int.Parse(settings[501]);

        NormalFontColor = LoadColor(520);
        DarkFontColor = LoadColor(540);
        PaperdollBorderColor = LoadColor(560);

        Portrait = new Size(int.Parse(settings[600]), int.Parse(settings[601]));

        // Merge the individual rectangles into one large one
        // Vanilla didn't use clipping for UI rendering, but we do, so we need
        // one encompassing window
        var parentRect = Rectangle.Union(MiniatureWindow, PaperdollWindow);
        parentRect = Rectangle.Union(parentRect, PortraitWindow);
        parentRect = Rectangle.Union(parentRect, TabNavWindow);
        parentRect = Rectangle.Union(parentRect, buttonWindow);
        MainWindow = parentRect;

        Rectangle MakeRelativeToParent(Rectangle rect)
        {
            return new Rectangle(rect.X - parentRect.X, rect.Y - parentRect.Y, rect.Width, rect.Height);
        }

        // Then make them all relative to that new window
        MiniatureWindow = MakeRelativeToParent(MiniatureWindow);
        PaperdollWindow = MakeRelativeToParent(PaperdollWindow);
        PortraitWindow = MakeRelativeToParent(PortraitWindow);
        TabNavWindow = MakeRelativeToParent(TabNavWindow);

        // Buttons are also shifted -1,-1 during rendering
        var miniatureButton = MakeRelativeToParent(MiniatureButton);
        miniatureButton.Offset(-1, -1);
        MiniatureButton = miniatureButton;
        var portraitButton = MakeRelativeToParent(PortraitButton);
        portraitButton.Offset(-1, -1);
        PortraitButton = portraitButton;
        var paperdollButton = MakeRelativeToParent(PaperdollButton);
        paperdollButton.Offset(-1, -1);
        PaperdollButton = paperdollButton;
    }
}