using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Ui.CharSheet.Stats
{
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

            void LoadPoint(out Point point, int baseId, bool makeRelative = false)
            {
                point = new Point(
                    int.Parse(settings[baseId]),
                    int.Parse(settings[baseId + 1])
                );
                if (makeRelative)
                {
                    point.X -= MainWindow.X;
                    point.Y -= MainWindow.Y;
                }
            }

            void LoadSize(out Size point, int baseId)
            {
                point = new Size(
                    int.Parse(settings[baseId]),
                    int.Parse(settings[baseId + 1])
                );
            }

            LoadRectangle(out MainWindow, 0);
            LoadRectangle(out PlatinumButton, 5, true);
            LoadRectangle(out GoldButton, 10, true);
            LoadRectangle(out SilverButton, 14, true);
            LoadRectangle(out CopperButton, 20, true);

            NormalFont = settings[260];
            NormalFontSize = int.Parse(settings[261]);
            BigFont = settings[270];
            BigFontSize = int.Parse(settings[271]);
            MoneyFont = settings[280];
            MoneyFontSize = int.Parse(settings[281]);

            LoadColor(out FontNormalColor, 300);
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

        public string NormalFont;
        public int NormalFontSize;
        public string BigFont;
        public int BigFontSize;
        public string MoneyFont;
        public int MoneyFontSize;
        public PackedLinearColorA FontNormalColor;
        public PackedLinearColorA FontDarkColor;

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
}