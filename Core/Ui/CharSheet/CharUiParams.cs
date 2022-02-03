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
                rect.X -= CharUiMainWindow.X;
                rect.Y -= CharUiMainWindow.Y;
            }
        }

        void LoadColor(out PackedLinearColorA color, int baseId)
        {
            color = new PackedLinearColorA(
                byte.Parse(settings[baseId]),
                byte.Parse(settings[baseId + 1]),
                byte.Parse(settings[baseId + 2]),
                byte.Parse(settings[baseId + 3])
            );
        }

        void LoadPoint(out Point point, int baseId, bool makeRelative = false)
        {
            point = new Point(
                int.Parse(settings[baseId]),
                int.Parse(settings[baseId + 1])
            );
            if (makeRelative)
            {
                point.X -= CharUiMainWindow.X;
                point.Y -= CharUiMainWindow.Y;
            }
        }

        void LoadSize(out Size point, int baseId)
        {
            point = new Size(
                int.Parse(settings[baseId]),
                int.Parse(settings[baseId + 1])
            );
        }

        LoadRectangle(out CharUiMainWindow, 0);
        LoadColor(out BorderOutside, 10);
        LoadColor(out BorderInside, 14);
        FontName = settings[18];
        FontSize = int.Parse(settings[19]);
        FontSmallName = settings[20];
        FontSmallSize = int.Parse(settings[21]);
        LoadPoint(out NameClassesLevel, 22, true);
        LoadPoint(out AlignmentGenderRaceWorship, 24, true);
        FontBig = settings[26];
        FontBigSize = int.Parse(settings[27]);
        LoadRectangle(out CharUiMainExitButton, 30, true);
        LoadRectangle(out CharUiMainTextField, 40, true);
        LoadRectangle(out CharUiSelectInventory0Button, 60, true);
        LoadRectangle(out CharUiSelectInventory1Button, 80, true);
        LoadRectangle(out CharUiSelectInventory2Button, 100, true);
        LoadRectangle(out CharUiSelectInventory3Button, 120, true);
        LoadRectangle(out CharUiSelectInventory4Button, 140, true);
        LoadRectangle(out CharUiSelectSkillsButton, 160, true);
        LoadRectangle(out CharUiSelectFeatsButton, 180, true);
        LoadRectangle(out CharUiSelectSpellsButton, 200, true);
        LoadRectangle(out CharUiSelectAbilitiesButton, 210, true);
        LoadRectangle(out CharUiMainEditorClassButton, 220, true);
        LoadRectangle(out CharUiMainEditorStatsButton, 224, true);
        LoadRectangle(out CharUiMainEditorFeatsButton, 228, true);
        LoadRectangle(out CharUiMainEditorSkillsButton, 232, true);
        LoadRectangle(out CharUiMainEditorSpellsButton, 236, true);
        LoadRectangle(out CharUiMainEditorFinishButton, 240, true);
        LoadColor(out FontNormalColor, 260);
        LoadColor(out FontDarkColor, 280);
        LoadColor(out FontHighlightColor, 300);
        LoadSize(out TabLeft, 320);
        LoadSize(out TabFill, 340);
        LoadRectangle(out InnerWindowBorder, 360);
        CharUiModeXNormal = int.Parse(settings[380]);
        CharUiModeXLooting = int.Parse(settings[381]);
        CharUiModeXLevelUp = int.Parse(settings[382]);
        LoadRectangle(out CharUiMainNavEditorWindow, 400, true);
        CharUiMainNavFontOffsetX = int.Parse(settings[420]);
        LoadRectangle(out CharUiMainNameButton, 500, true);
        LoadRectangle(out CharUiMainClassLevelButton, 520, true);
        LoadRectangle(out CharUiMainAlignmentGenderRaceButton, 540, true);
        LoadRectangle(out CharUiMainWorshipButton, 560, true);
        TooltipUiStyle = int.Parse(settings[200]);

        foreach (var texture in (CharUiTexture[]) Enum.GetValues(typeof(CharUiTexture)))
        {
            TexturePaths[texture] = "art/interface/char_ui/" + textures[(int) texture];
        }
    }

    public Rectangle CharUiMainWindow;

    [TempleDllLocation(0x10BE93C8)]
    public PackedLinearColorA BorderOutside;

    [TempleDllLocation(0x10BE93D8)]
    public PackedLinearColorA BorderInside;

    [TempleDllLocation(0x10BE9394)]
    public string FontName;

    [TempleDllLocation(0x10BE9390)]
    public int FontSize;

    [TempleDllLocation(0x10BE939c)]
    public string FontSmallName;

    [TempleDllLocation(0x10BE9398)]
    public int FontSmallSize;

    [TempleDllLocation(0x10BE93E8)]
    public Point NameClassesLevel;

    [TempleDllLocation(0x10BE93F0)]
    public Point AlignmentGenderRaceWorship;

    [TempleDllLocation(0x10BE93A4)]
    public string FontBig;

    [TempleDllLocation(0x10BE93A0)]
    public int FontBigSize;

    [TempleDllLocation(0x10BE9428)]
    public Rectangle CharUiMainExitButton;

    [TempleDllLocation(0x10BE9448)]
    public Rectangle CharUiMainTextField;

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

    [TempleDllLocation(0x10BE94D8)]
    public Rectangle CharUiSelectAbilitiesButton;

    [TempleDllLocation(0x10BE94E8)]
    public Rectangle CharUiMainEditorClassButton;

    [TempleDllLocation(0x10BE94F8)]
    public Rectangle CharUiMainEditorStatsButton;

    [TempleDllLocation(0x10BE9508)]
    public Rectangle CharUiMainEditorFeatsButton;

    [TempleDllLocation(0x10BE9518)]
    public Rectangle CharUiMainEditorSkillsButton;

    [TempleDllLocation(0x10BE9528)]
    public Rectangle CharUiMainEditorSpellsButton;

    [TempleDllLocation(0x10BE9538)]
    public Rectangle CharUiMainEditorFinishButton;

    [TempleDllLocation(0x10BE9360)]
    public PackedLinearColorA FontNormalColor;

    [TempleDllLocation(0x10BE9370)]
    public PackedLinearColorA FontDarkColor;

    [TempleDllLocation(0x10BE9380)]
    public PackedLinearColorA FontHighlightColor;

    [TempleDllLocation(0x10BE9548)]
    public Size TabLeft;

    [TempleDllLocation(0x10BE9550)]
    public Size TabFill;

    [TempleDllLocation(0x10BE9558)]
    public Rectangle InnerWindowBorder;

    [TempleDllLocation(0x10BE9568)]
    public int CharUiModeXNormal;

    [TempleDllLocation(0x10BE956C)]
    public int CharUiModeXLooting;

    [TempleDllLocation(0x10BE9570)]
    public int CharUiModeXLevelUp;

    [TempleDllLocation(0x10BE93B8)]
    public Rectangle CharUiMainNavEditorWindow;

    [TempleDllLocation(0x10BE9574)]
    public int CharUiMainNavFontOffsetX;

    [TempleDllLocation(0x10BE93F8)]
    public Rectangle CharUiMainNameButton;

    [TempleDllLocation(0x10BE9408)]
    public Rectangle CharUiMainClassLevelButton;

    [TempleDllLocation(0x10BE9418)]
    public Rectangle CharUiMainAlignmentGenderRaceButton;

    [TempleDllLocation(0x10BE9438)]
    public Rectangle CharUiMainWorshipButton;

    [TempleDllLocation(0x10BE9578)]
    public int TooltipUiStyle;

    [TempleDllLocation(0x10BE8D3C)]
    public Dictionary<CharUiTexture, string> TexturePaths { get; set; } = new Dictionary<CharUiTexture, string>();
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

    ButtonAbilitiesSelected = 160,
    ButtonAbilitiesUnselected = 161,
    ButtonAbilitiesHover = 162,
    ButtonAbilitiesClick = 163,

    TabLeftInactive = 170,

    TabLeftActive = 190,

    TabFillInactive = 210,

    TabFillActive = 230,

    BagTemp = 250,

    ButtonArcBottomSelected = 260,
    ButtonArcTopSelected = 261,

    ButtonContainerSelected = 270
}