using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Ui.CharSheet;

public class CharUiParams
{
    public CharUiParams(Dictionary<int, string> settings, Dictionary<int, string> textures)
    {
        void LoadRectangle(out Rectangle rect, int baseId, bool makeRelative = false)
        {
            rect = new Rectangle(
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
        }

        LoadRectangle(out CharUiMainExitButton, 30, true);
        LoadRectangle(out CharUiSelectInventory0Button, 60, true);
        LoadRectangle(out CharUiSelectInventory1Button, 80, true);
        LoadRectangle(out CharUiSelectInventory2Button, 100, true);
        LoadRectangle(out CharUiSelectInventory3Button, 120, true);
        LoadRectangle(out CharUiSelectInventory4Button, 140, true);
        LoadRectangle(out CharUiSelectSkillsButton, 160, true);
        LoadRectangle(out CharUiSelectFeatsButton, 180, true);
        LoadRectangle(out CharUiSelectSpellsButton, 200, true);
        CharUiModeXNormal = int.Parse(settings[380]);
        CharUiModeXLooting = int.Parse(settings[381]);
        CharUiModeXLevelUp = int.Parse(settings[382]);

        foreach (var texture in (CharUiTexture[]) Enum.GetValues(typeof(CharUiTexture)))
        {
            TexturePaths[texture] = "art/interface/char_ui/" + textures[(int) texture];
        }
    }

    [TempleDllLocation(0x10BE9428)]
    public Rectangle CharUiMainExitButton;

    [TempleDllLocation(0x10BE9458)]
    public Rectangle CharUiSelectInventory0Button;

    [TempleDllLocation(0x10BE9468)]
    public Rectangle CharUiSelectInventory1Button;

    [TempleDllLocation(0x10BE9478)]
    public Rectangle CharUiSelectInventory2Button;

    [TempleDllLocation(0x10BE9488)]
    public Rectangle CharUiSelectInventory3Button;

    [TempleDllLocation(0x10BE9498)]
    public Rectangle CharUiSelectInventory4Button;

    [TempleDllLocation(0x10BE94A8)]
    public Rectangle CharUiSelectSkillsButton;

    [TempleDllLocation(0x10BE94B8)]
    public Rectangle CharUiSelectFeatsButton;

    [TempleDllLocation(0x10BE94C8)]
    public Rectangle CharUiSelectSpellsButton;

    [TempleDllLocation(0x10BE9568)]
    public int CharUiModeXNormal;

    [TempleDllLocation(0x10BE956C)]
    public int CharUiModeXLooting;

    [TempleDllLocation(0x10BE9570)]
    public int CharUiModeXLevelUp;

    [TempleDllLocation(0x10BE8D3C)]
    public Dictionary<CharUiTexture, string> TexturePaths { get; set; } = new();
}

public enum CharUiTexture
{
    MainExitButtonDisabled = 30,
    MainExitButtonHoverOff = 31,
    MainExitButtonHoverOn = 32,
    MainExitButtonHoverPressed = 33,

    ButtonSkillsSelected = 100,
    ButtonSkillsUnselected = 101,
    ButtonSkillsHover = 102,
    ButtonSkillsClick = 103,

    ButtonFeatsSelected = 120,
    ButtonFeatsUnselected = 121,
    ButtonFeatsHover = 122,
    ButtonFeatsClick = 123,

    ButtonSpellsSelected = 140,
    ButtonSpellsUnselected = 141,
    ButtonSpellsHover = 142,
    ButtonSpellsClick = 143,

    BagTemp = 250,

    ButtonArcBottomSelected = 260,
    ButtonArcTopSelected = 261,

    ButtonContainerSelected = 270
}