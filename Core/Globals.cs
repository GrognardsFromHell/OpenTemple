using OpenTemple.Core.Config;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core;

/// <summary>
/// remove after porting
/// </summary>
public static class Globals
{
    public static GameLib GameLib { get; set; } = new();

    public static GameLoop GameLoop { get; set; }

    public static GameFolders GameFolders { get; set; }

    public static GameConfigManager ConfigManager { get; set; }

    public static GameConfig Config => ConfigManager.Config;

    public static UiManager UiManager { get; set; }

    public static UiStyles UiStyles { get; set; }

    public static WidgetButtonStyles WidgetButtonStyles { get; set; }

    public static UiAssets UiAssets { get; set; }
}

// TODO: Expand this into an actual manager