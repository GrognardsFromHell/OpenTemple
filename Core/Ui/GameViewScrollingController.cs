using System;
using System.Drawing;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public class GameViewScrollingController
    {
        private readonly WidgetContainer _widget;
        private readonly IGameViewport _viewport;
        private TimePoint _lastScrolling;
        private static readonly TimeSpan ScrollButterDelay = TimeSpan.FromMilliseconds(16);
        private bool _grabMoving;
        private Point _grabMoveRef;
        private bool _mouseHasMoved;

        public GameViewScrollingController(WidgetContainer widget, IGameViewport viewport)
        {
            _widget = widget;
            _viewport = viewport;
        }

        public bool MiddleMouseDown(Point pos)
        {
            _grabMoving = true;
            _grabMoveRef = pos;
            return true;
        }

        public bool MiddleMouseUp()
        {
            if (_grabMoving)
            {
                _grabMoving = false;
                _grabMoveRef = Point.Empty;
                return true;
            }

            return false;
        }

        public bool MouseMoved(Point pos)
        {
            if (!_grabMoving)
            {
                return false;
            }

            _mouseHasMoved = true;

            var dx = pos.X - _grabMoveRef.X;
            var dy = pos.Y - _grabMoveRef.Y;
            dx = (int) (dx / _viewport.Zoom);
            dy = (int) (dy / _viewport.Zoom);

            GameSystems.Scroll.ScrollBy(_viewport, dx, dy);

            _grabMoveRef = pos;
            return true;
        }

        [TempleDllLocation(0x10001010)]
        public void UpdateTime(TimePoint time, Point mousePos)
        {
            // When we're grab-moving, do not do border-scrolling
            if (_grabMoving)
            {
                return;
            }

            // Wait until the mouse has moved at least once to avoid the issue of the mouse at 0,0
            // causing the screen to move directly on startup.
            if (!_mouseHasMoved)
            {
                return;
            }

            if (!IsMouseScrolling)
            {
                return;
            }

            var config = Globals.Config.Window;
            if (config.Windowed && Tig.Mouse.IsMouseOutsideWindow)
            {
                return;
            }

            if (_lastScrolling.Time != 0 && time - _lastScrolling < ScrollButterDelay)
            {
                if (!Globals.Config.ScrollAcceleration)
                {
                    return;
                }
            }

            _lastScrolling = time;

            int scrollMarginV = 2;
            int scrollMarginH = 3;
            if (config.Windowed)
            {
                scrollMarginV = 7;
                scrollMarginH = 7;
            }

            // TODO This should be the size of the game view
            var size = _widget.GetSize();
            var renderWidth = size.Width;
            var renderHeight = size.Height;

            ScrollDirection? scrollDir = null;
            if (mousePos.X <= scrollMarginH) // scroll left
            {
                if (mousePos.Y <= scrollMarginV) // scroll upper left
                    scrollDir = ScrollDirection.UP_LEFT;
                else if (mousePos.Y >= renderHeight - scrollMarginV) // scroll bottom left
                    scrollDir = ScrollDirection.DOWN_LEFT;
                else
                    scrollDir = ScrollDirection.LEFT;
            }
            else if (mousePos.X >= renderWidth - scrollMarginH) // scroll right
            {
                if (mousePos.Y <= scrollMarginV) // scroll top right
                    scrollDir = ScrollDirection.UP_RIGHT;
                else if (mousePos.Y >= renderHeight - scrollMarginV) // scroll bottom right
                    scrollDir = ScrollDirection.DOWN_RIGHT;
                else
                    scrollDir = ScrollDirection.RIGHT;
            }
            else // scroll vertical only
            {
                if (mousePos.Y <= scrollMarginV) // scroll up
                    scrollDir = ScrollDirection.UP;
                else if (mousePos.Y >= renderHeight - scrollMarginV) // scroll down
                    scrollDir = ScrollDirection.DOWN;
            }

            if (scrollDir.HasValue)
            {
                GameSystems.Scroll.SetScrollDirection(scrollDir.Value);
            }
        }

        public bool IsMouseScrolling { get; set; } = true;
    }
}