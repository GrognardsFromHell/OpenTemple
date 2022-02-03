using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.CharSheet.Stats;

public class StatsUiParams
{
    public StatsUiParams(
        Rectangle mainWindowRectangle,
        Dictionary<int, string> settings,
        Dictionary<int, string> textures,
        Dictionary<int, string> translations)
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
                rect.X -= MainWindow.X;
                rect.Y -= MainWindow.Y;
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

        LoadRectangle(out MainWindow, 0);
        LoadRectangle(out PlatinumButton, 5, true);
        LoadRectangle(out GoldButton, 10, true);
        LoadRectangle(out SilverButton, 14, true);
        LoadRectangle(out CopperButton, 20, true);

        LoadRectangle(out XpLabel, 400, true);
        LoadRectangle(out XpValue, 740, true);
        LoadRectangle(out LevelLabel, 420, true);
        LoadRectangle(out LevelValue, 760, true);
        LoadRectangle(out StrLabel, 440, true);
        LoadRectangle(out StrValue, 780, true);
        LoadRectangle(out StrBonusValue, 980, true);
        LoadRectangle(out DexLabel, 460, true);
        LoadRectangle(out DexValue, 820, true);
        LoadRectangle(out DexBonusValue, 1000, true);
        LoadRectangle(out ConLabel, 480, true);
        LoadRectangle(out ConValue, 840, true);
        LoadRectangle(out ConBonusValue, 1020, true);
        LoadRectangle(out IntLabel, 500, true);
        LoadRectangle(out IntValue, 860, true);
        LoadRectangle(out IntBonusValue, 1040, true);
        LoadRectangle(out WisLabel, 520, true);
        LoadRectangle(out WisValue, 880, true);
        LoadRectangle(out WisBonusValue, 1060, true);
        LoadRectangle(out ChaLabel, 540, true);
        LoadRectangle(out ChaValue, 900, true);
        LoadRectangle(out ChaBonusValue, 1080, true);

        LoadRectangle(out HpLabel, 560, true);
        LoadRectangle(out HpValue, 940, true);
        LoadRectangle(out AcLabel, 580, true);
        LoadRectangle(out AcValue, 960, true);

        LoadRectangle(out FortLabel, 600, true);
        LoadRectangle(out FortValue, 1100, true);
        LoadRectangle(out RefLabel, 620, true);
        LoadRectangle(out RefValue, 1120, true);
        LoadRectangle(out WillLabel, 640, true);
        LoadRectangle(out WillValue, 1140, true);

        LoadRectangle(out HeightLabel, 649, true);
        LoadRectangle(out HeightValue, 1148, true);
        LoadRectangle(out WeightLabel, 653, true);
        LoadRectangle(out WeightValue, 1152, true);

        LoadRectangle(out InitLabel, 660, true);
        LoadRectangle(out InitValue, 1160, true);
        LoadRectangle(out PrimaryAtkLabel, 680, true);
        PrimaryAtkLabelText = translations[10];
        LoadRectangle(out PrimaryAtkValue, 1180, true);
        LoadRectangle(out SecondaryAtkLabel, 700, true);
        SecondaryAtkLabelText = translations[20];
        LoadRectangle(out SecondaryAtkValue, 1200, true);
        LoadRectangle(out SpeedLabel, 720, true);
        LoadRectangle(out SpeedValue, 1220, true);

        var normalFont = settings[260];
        var normalFontSize = int.Parse(settings[261]);
        var boldFont = settings[270];
        var boldFontSize = int.Parse(settings[271]);
        var moneyFont = settings[280];
        var moneyFontSize = int.Parse(settings[281]);

        // LoadColor(out FontNormalColor, 300);
        // LoadColor(out FontDarkColor, 319);

        // TooltipUiStyle = int.Parse(settings[200]);

        MainWindow.X -= mainWindowRectangle.X;
        MainWindow.Y -= mainWindowRectangle.Y;

        foreach (var texture in (StatsUiTexture[]) Enum.GetValues(typeof(StatsUiTexture)))
        {
            TexturePaths[texture] = "art/interface/char_ui/char_stats_ui/" + textures[(int) texture];
        }
    }

    public Rectangle MainWindow;
    public Rectangle PlatinumButton;
    public Rectangle GoldButton;
    public Rectangle SilverButton;
    public Rectangle CopperButton;

    // XP & Level
    public Rectangle XpLabel;
    public Rectangle XpValue;
    public Rectangle LevelLabel;
    public Rectangle LevelValue;

    // Attributes
    public Rectangle StrLabel;
    public Rectangle StrValue;
    public Rectangle StrBonusValue;
    public Rectangle DexLabel;
    public Rectangle DexValue;
    public Rectangle DexBonusValue;
    public Rectangle ConLabel;
    public Rectangle ConValue;
    public Rectangle ConBonusValue;
    public Rectangle IntLabel;
    public Rectangle IntValue;
    public Rectangle IntBonusValue;
    public Rectangle WisLabel;
    public Rectangle WisValue;
    public Rectangle WisBonusValue;
    public Rectangle ChaLabel;
    public Rectangle ChaValue;
    public Rectangle ChaBonusValue;

    // Defenses
    public Rectangle HpLabel;
    public Rectangle HpValue;
    public Rectangle AcLabel;
    public Rectangle AcValue;

    // Saves
    public Rectangle FortLabel;
    public Rectangle FortValue;
    public Rectangle RefLabel;
    public Rectangle RefValue;
    public Rectangle WillLabel;
    public Rectangle WillValue;

    public Rectangle HeightLabel;
    public Rectangle HeightValue;
    public Rectangle WeightLabel;
    public Rectangle WeightValue;

    // Combat
    public Rectangle InitLabel;
    public Rectangle InitValue;
    public Rectangle PrimaryAtkLabel;
    public string PrimaryAtkLabelText;
    public Rectangle PrimaryAtkValue;
    public Rectangle SecondaryAtkLabel;
    public string SecondaryAtkLabelText;
    public Rectangle SecondaryAtkValue;
    public Rectangle SpeedLabel;
    public Rectangle SpeedValue;

    public Dictionary<StatsUiTexture, string> TexturePaths { get; set; } = new Dictionary<StatsUiTexture, string>();
}

public enum StatsUiTexture
{
    MainWindow = 10,

    ButtonAgeClick = 20,
    ButtonAgeHover = 21,

    ButtonAgeOutputSelected = 25,
    ButtonAgeOutputUnselected = 26,
    ButtonAgeOutputHover = 27,
    ButtonAgeOutputClick = 28,

    ButtonEXPClick = 30,
    ButtonEXPHover = 31,

    ButtonEXPOutputSelected = 35,
    ButtonEXPOutputUnselected = 36,
    ButtonEXPOutputHover = 37,
    ButtonEXPOutputClick = 38,

    ButtonFORTClick = 40,
    ButtonFORTHover = 41,

    ButtonFORTOutputSelected = 45,
    ButtonFORTOutputUnselected = 46,
    ButtonFORTOutputHover = 47,
    ButtonFORTOutputClick = 48,

    ButtonHPClick = 50,
    ButtonHPHover = 51,

    ButtonHPOutputSelected = 55,
    ButtonHPOutputUnselected = 56,
    ButtonHPOutputHover = 57,
    ButtonHPOutputClick = 58,

    ButtonInitiativeClick = 60,
    ButtonInitiativeHover = 61,

    ButtonInitiativeOutputSelected = 65,
    ButtonInitiativeOutputUnselected = 66,
    ButtonInitiativeOutputHover = 67,
    ButtonInitiativeOutputClick = 68,

    ButtonLevelClick = 70,
    ButtonLevelHover = 71,

    ButtonLevelOutputSelected = 75,
    ButtonLevelOutputUnselected = 76,
    ButtonLevelOutputHover = 77,
    ButtonLevelOutputClick = 78,

    ButtonSpeedClick = 80,
    ButtonSpeedHover = 81,

    ButtonSpeedOutputSelected = 85,
    ButtonSpeedOutputUnselected = 86,
    ButtonSpeedOutputHover = 87,
    ButtonSpeedOutputClick = 88,

    ButtonSTRClick = 90,
    ButtonSTRHover = 91,

    ButtonSTROutputSelected = 95,
    ButtonSTROutputUnselected = 96,
    ButtonSTROutputHover = 97,

    ButtonHTClick = 100,
    ButtonHTHover = 101,

    ButtonHTOutputSelected = 105,
    ButtonHTOutputUnselected = 106,
    ButtonHTOutputHover = 107,
    ButtonHTOutputClick = 108,
}