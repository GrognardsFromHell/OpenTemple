using System;
using System.Collections;
using System.Collections.Generic;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Scenes;
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

        public static IStage Stage { get; set; }

        public static GameLib GameLib { get; set; } = new GameLib();

        public static GameLoop GameLoop { get; set; }

        public static GameFolders GameFolders { get; set; }

        public static GameConfigManager ConfigManager { get; set; }

        public static GameConfig Config => ConfigManager.Config;

        public static UiManager UiManager { get; set; }

        public static WidgetTextStyles WidgetTextStyles { get; set; }

        public static WidgetButtonStyles WidgetButtonStyles { get; set; }

        public static UiAssets UiAssets { get; set; }
    }

    // TODO: Expand this into an actual manager
    public class GameViews
    {
        public delegate void PrimaryChangeEvent(IGameViewport previous, IGameViewport current);

        public static event PrimaryChangeEvent OnPrimaryChange;

        public static IGameViewport Primary { get; private set; }

        private static readonly ISet<IGameViewport> VisibleGameViews = new HashSet<IGameViewport>();

        public static IEnumerable<IGameViewport> AllVisible => VisibleGameViews;

        public static void Add(IGameViewport gameView)
        {
            var oldPrimary = Primary;
            Primary = gameView;
            OnPrimaryChange?.Invoke(oldPrimary, Primary);

            VisibleGameViews.Add(gameView);
        }

        public static void Remove(IGameViewport gameView)
        {
            if (Primary == gameView)
            {
                Primary = null;
                OnPrimaryChange?.Invoke(gameView, null);
            }

            VisibleGameViews.Remove(gameView);
        }
    }
}
