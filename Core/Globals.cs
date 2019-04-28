using SpicyTemple.Core.Config;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Ui.Assets;
using SpicyTemple.Core.Ui.Styles;

namespace SpicyTemple.Core
{
    /// <summary>
    /// remove after porting
    /// </summary>
    public static class Globals
    {

        public static GameFolders GameFolders { get; set; }

        public static GameConfig Config { get; set; }

        public static GameConfigManager ConfigManager { get; set; }

        public static UiManager UiManager { get; set; }

        public static WidgetTextStyles WidgetTextStyles { get; set; }

        public static WidgetButtonStyles WidgetButtonStyles { get; set; }

        public static UiAssets UiAssets { get; set; }

    }

}
