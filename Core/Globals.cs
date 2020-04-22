using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core
{
    /// <summary>
    /// remove after porting
    /// </summary>
    public static class Globals
    {

        public static GameLib GameLib { get; set; } = new GameLib();

        public static GameFolders GameFolders { get; set; }

        public static GameConfigManager ConfigManager { get; set; }

        public static GameConfig Config => ConfigManager.Config;

        public static UiManager UiManager { get; set; }

        public static WidgetTextStyles WidgetTextStyles { get; set; }

        public static WidgetButtonStyles WidgetButtonStyles { get; set; }

        public static UiAssets UiAssets { get; set; }

    }

}
