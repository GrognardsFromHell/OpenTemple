using System.Collections.Generic;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core
{
    public class GameViews
    {
        public delegate void PrimaryChangeEvent(IGameViewport previous, IGameViewport current);

        public static event PrimaryChangeEvent OnPrimaryChange;

        public static IGameViewport Primary { get; private set; }

        private static readonly ISet<IGameViewport> VisibleGameViews = new HashSet<IGameViewport>();

        public static IEnumerable<IGameViewport> AllVisible => VisibleGameViews;

        private static int _drawEnableCount = 1;

        public static bool IsDrawingEnabled => _drawEnableCount >= 1;

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

        [TempleDllLocation(0x100027E0)]
        public static void EnableDrawing()
        {
            _drawEnableCount++;
        }

        [TempleDllLocation(0x100027C0)]
        public static void DisableDrawing()
        {
            _drawEnableCount--;
        }

        [TempleDllLocation(0x100027D0)]
        public static void DisableDrawingForce()
        {
            _drawEnableCount = 0;
        }

    }
}