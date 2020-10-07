using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using SharpDX;

namespace OpenTemple.Core.Systems
{
    public class ScrollSystem : IGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private const bool IsEditor = false;

        // How many milliseconds until the scroll velocity fully decays
        private const float FullDecelerationTime = 600;
        private const float FullDecelerationTimeHalf = FullDecelerationTime / 2;
        private const float FullDecelerationTimeSquared = FullDecelerationTime * FullDecelerationTime;

        /// <summary>
        /// Used to animate the scrolling of the main menu background map.
        /// </summary>
        [TempleDllLocation(0x10307350)]
        private TimePoint _scrollMainMenuRefPoint;

        [TempleDllLocation(0x10307380)]
        private float _mainMenuScrollState;

        [TempleDllLocation(0x10307360)]
        private int _scrollSpeed;

        [TempleDllLocation(0x10307304)]
        private Size _screenSize;

        [TempleDllLocation(0x10307370)]
        private int _mapScrollX;

        [TempleDllLocation(0x10307338)]
        private int _mapScrollY;

        [TempleDllLocation(0x103072E8)]
        private int _mapScrollXSpeed;

        [TempleDllLocation(0x1030733C)]
        private int _mapScrollYSpeed;

        [TempleDllLocation(0x10307310)]
        private locXY _screenCenterTile;

        [TempleDllLocation(0x10307340)]
        [TempleDllLocation(0x10307368)]
        [TempleDllLocation(0x103072F0)]
        [TempleDllLocation(0x10307358)]
        private MapLimits _currentLimits;

        // Map limits loaded from MapLimits.mes, keyed by map id
        private Dictionary<int, MapLimits> _mapLimits;

        private readonly ScrollingController _scrollingController = new ScrollingController();

        [TempleDllLocation(0x10005E70)]
        public ScrollSystem()
        {
            ReReadScrollConfig();
            Globals.ConfigManager.OnConfigChanged += ReReadScrollConfig;

            _screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;

            CalculateScrollSpeed();
            GameSystems.Location.OnMapCentered += CenterViewDirectly;

            _mapLimits = LoadMapLimits();

            _currentLimits = MapLimits.Default;

            _resizeListener = Tig.RenderingDevice.AddResizeListener((x, y) => ResizeViewport());
        }

        private static Dictionary<int, MapLimits> LoadMapLimits()
        {
            var mapLimitsMes = Tig.FS.ReadMesFile("rules/MapLimits.mes");
            var limits = new Dictionary<int, MapLimits>();

            foreach (var (mapId, line) in mapLimitsMes)
            {
                var tokens = line.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                limits[mapId] = new MapLimits
                {
                    Left = int.Parse(tokens[0]),
                    Top = int.Parse(tokens[1]),
                    Right = int.Parse(tokens[2]),
                    Bottom = int.Parse(tokens[3])
                };
            }

            return limits;
        }

        [TempleDllLocation(0x10005e30)]
        private void ReReadScrollConfig()
        {
            _scrollSpeed = Math.Clamp(Globals.Config.ScrollSpeed, 0, 4);
        }

        [TempleDllLocation(0x10005C60)]
        private void CenterViewDirectly(int tileX, int tileY)
        {
            if (!IsEditor)
            {
                _mapScrollX = 0;
                _mapScrollY = 0;
                _screenCenterTile = new locXY(tileX, tileY);
                GameSystems.SoundGame.SetViewCenterTile(_screenCenterTile);
            }
        }

        [TempleDllLocation(0x10005ca0)]
        private void CalculateScrollSpeed()
        {
            if (IsEditor)
            {
                switch (_scrollSpeed)
                {
                    case 0:
                        _mapScrollXSpeed = _screenSize.Width / 2;
                        _mapScrollYSpeed = _screenSize.Width / 4;
                        break;
                    case 1:
                        _mapScrollXSpeed = _screenSize.Width;
                        _mapScrollYSpeed = _screenSize.Width / 2;
                        break;
                    case 2:
                        _mapScrollYSpeed = _screenSize.Width;
                        _mapScrollXSpeed = 2 * _screenSize.Width;
                        break;
                    case 3:
                        _mapScrollXSpeed = 4 * _screenSize.Width;
                        _mapScrollYSpeed = 2 * _screenSize.Width;
                        break;
                    case 4:
                        _mapScrollYSpeed = 4 * _screenSize.Width;
                        _mapScrollXSpeed = 8 * _screenSize.Width;
                        break;
                    default:
                        return;
                }
            }
            else
            {
                switch (_scrollSpeed)
                {
                    case 0:
                        _mapScrollXSpeed = 8;
                        _mapScrollYSpeed = 4;
                        break;
                    case 1:
                        _mapScrollXSpeed = 14;
                        _mapScrollYSpeed = 7;
                        break;
                    case 2:
                        _mapScrollXSpeed = 28;
                        _mapScrollYSpeed = 14;
                        break;
                    case 3:
                        _mapScrollXSpeed = 56;
                        _mapScrollYSpeed = 28;
                        break;
                    case 4:
                        _mapScrollXSpeed = 112;
                        _mapScrollYSpeed = 56;
                        break;
                    default:
                        return;
                }
            }
        }

        [TempleDllLocation(0x10005870)]
        private void ResizeViewport()
        {
            _screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
            CalculateScrollSpeed();
        }

        [TempleDllLocation(0x10005700)]
        public void Reset()
        {
            _mapScrollY = 0;
            _mapScrollX = 0;
            _timeLastScroll = default;
        }

        [TempleDllLocation(0x10307388)]
        private TimePoint _screenShakeStart;

        [TempleDllLocation(0x11E72694)]
        private float _screenShakeDuration;

        [TempleDllLocation(0x11E72690)]
        private float _screenShakeAmount;

        [TempleDllLocation(0x11E72688)]
        private int _screenShakeLastXOffset;

        [TempleDllLocation(0x11E7268C)]
        private int _screenShakeLastYOffset;

        [TempleDllLocation(0x10307378)]
        private TimePoint _timeLastScroll;

        [TempleDllLocation(0x1030737C)]
        private TimePoint _timeLastScrollDirectionChange;

        [TempleDllLocation(0x10006000)]
        public void AdvanceTime(TimePoint time)
        {
            if (GameSystems.Map.GetCurrentMapId() == 5000 && !IsEditor)
            {
                ProcessMainMenuScrolling();
                return;
            }

            ProcessScreenShake(time);

            ProcessScrollButter(time);

            _scrollingController.Update();
        }

        private void ProcessMainMenuScrolling()
        {
            var elapsedSeconds = (TimePoint.Now - _scrollMainMenuRefPoint).TotalSeconds;
            if (elapsedSeconds < 1.0f)
            {
                _mainMenuScrollState += (float) elapsedSeconds;
            }

            var amountToScroll = _mainMenuScrollState;
            if (amountToScroll < 0.0f)
            {
                amountToScroll = 0.0f;
                _mainMenuScrollState = amountToScroll;
            }
            else
            {
                while (amountToScroll > 100.0f)
                {
                    amountToScroll -= 100.0f;
                }
            }

            var screenHeight = (int) Tig.RenderingDevice.GetCamera().GetScreenHeight();
            var targetTranslationX = 1400 - (int) (amountToScroll * 53.599998);
            var targetTranslationY = screenHeight / 2 - 13726;

            GameSystems.Location.AddTranslation(
                targetTranslationX - GameSystems.Location.LocationTranslationX,
                targetTranslationY - GameSystems.Location.LocationTranslationY
            );
            _scrollMainMenuRefPoint = TimePoint.Now;
        }

        [TempleDllLocation(0x10005840)]
        public void ShakeScreen(float amount, float duration)
        {
            _screenShakeStart = TimePoint.Now;
            _screenShakeAmount = amount;
            _screenShakeDuration = duration;
        }

        private void ProcessScreenShake(TimePoint time)
        {
            var screenShakeElapsed = (float) (time - _screenShakeStart).TotalMilliseconds;
            if (screenShakeElapsed < _screenShakeDuration)
            {
                var shakeRemaining = (1.0 - screenShakeElapsed / _screenShakeDuration);

                var xTime = time.Milliseconds / 50.0f;
                // TODO RANDOMIZE v7 = sub_10089EE0(v7, 4.0, 4.0, 3);
                var xOffset = (int) (xTime * _screenShakeAmount * shakeRemaining);

                var yTime = (time.Milliseconds + 100) / 50.0f;
                // TODO RANDOMIZE v10 = sub_10089EE0(v10, 4.0, 4.0, 3);
                var yOffset = (int) (yTime * _screenShakeAmount * shakeRemaining);
                ScrollBy(xOffset - _screenShakeLastXOffset, yOffset - _screenShakeLastYOffset);
                _screenShakeLastXOffset = xOffset;
                _screenShakeLastYOffset = yOffset;
            }
            else if (_screenShakeLastXOffset != 0 || _screenShakeLastYOffset != 0)
            {
                ScrollBy(-_screenShakeLastXOffset, -_screenShakeLastYOffset);
                _screenShakeLastYOffset = 0;
                _screenShakeLastXOffset = 0;
            }
        }

        private void ProcessScrollButter(TimePoint time)
        {
            if (_mapScrollX != 0 || _mapScrollY != 0)
            {
                var elapsedTime = (float) (time - _timeLastScroll).TotalSeconds;
                _timeLastScroll = time;

                if (elapsedTime > 0.1f)
                {
                    elapsedTime = 0.1f;
                }

                var deltaX = (int) (_mapScrollX * elapsedTime);
                var deltaY = (int) (_mapScrollY * elapsedTime);
                ScrollBy(deltaX, deltaY);
                _mapScrollX -= deltaX;
                _mapScrollY -= deltaY;

                var timeSinceManualScroll = (float) (time - _timeLastScrollDirectionChange).TotalMilliseconds;
                if (timeSinceManualScroll > FullDecelerationTime)
                {
                    timeSinceManualScroll = FullDecelerationTime;
                }

                var decayFactor = timeSinceManualScroll * timeSinceManualScroll / FullDecelerationTimeSquared;
                if (timeSinceManualScroll < FullDecelerationTimeHalf)
                {
                    decayFactor = 1.0f - decayFactor;
                }

                _mapScrollX = (int) (_mapScrollX * decayFactor);
                _mapScrollY = (int) (_mapScrollY * decayFactor);
            }
        }

        [TempleDllLocation(0x100058f0)]
        private void ScrollBy(int x, int y)
        {
            var translationX = GameSystems.Location.LocationTranslationX;
            var translationY = GameSystems.Location.LocationTranslationY;

            var screenWidth = (int) Tig.RenderingDevice.GetCamera().GetScreenWidth();
            var screenHeight = (int) Tig.RenderingDevice.GetCamera().GetScreenHeight();

            // Perform clamping to the scroll-limit on the x/y values
            if (!IsEditor)
            {
                if (x + translationX >= _currentLimits.Right + screenWidth)
                {
                    if (x + translationX > _currentLimits.Left)
                    {
                        x = _currentLimits.Left - translationX;
                    }
                }
                else
                {
                    x = _currentLimits.Right + screenWidth - translationX;
                }

                if (y + translationY < _currentLimits.Bottom + screenHeight)
                {
                    y = _currentLimits.Bottom + screenHeight - translationY;
                }
                else if (y + translationY > _currentLimits.Top)
                {
                    y = _currentLimits.Top - translationY;
                }
            }

            GameSystems.Location.AddTranslation(x, y);

            x = GameSystems.Location.LocationTranslationX;
            y = GameSystems.Location.LocationTranslationY;

            if (x != translationX || y != translationY)
            {
                if (!IsEditor)
                {
                    GameSystems.Location.ScreenToLoc(screenWidth / 2, screenHeight / 2,
                        out var screenCenter);
                    if (screenCenter != _screenCenterTile)
                    {
                        GameSystems.SoundGame.SetViewCenterTile(screenCenter);
                        _screenCenterTile = screenCenter;
                    }
                }
            }
        }

        private const float MaxDistanceForSmoothScrolling = 2400.0f;

        private const float MaxDistanceForSmoothScrollingSquared =
            MaxDistanceForSmoothScrolling * MaxDistanceForSmoothScrolling;

        public void CenterOnSmooth(GameObjectBody obj)
        {
            var location = obj.GetLocation();
            CenterOnSmooth(location.locx, location.locy);
        }

        public void CenterOnScreenSmooth(int screenX, int screenY)
        {
            var worldPos = Tig.RenderingDevice.GetCamera().ScreenToTile(screenX, screenY);
            CenterOnSmooth(worldPos.location.locx, worldPos.location.locy);
        }

        [TempleDllLocation(0x10005bc0)]
        public void CenterOnSmooth(int tileX, int tileY)
        {
            GameSystems.Location.GetTranslationDelta(tileX, tileY, out var xa, out var ya);

            var scrollDistanceSquared = (xa * xa + ya * ya);
            if (scrollDistanceSquared <= MaxDistanceForSmoothScrollingSquared)
            {
                _mapScrollX = xa;
                _mapScrollY = ya;
            }
            else
            {
                GameSystems.Location.CenterOn(tileX, tileY);
            }
        }

        [TempleDllLocation(0x10006480)]
        public void SetScrollDirection(ScrollDirection scrollDir)
        {
            var deltaX = 0;
            var deltaY = 0;
            switch (scrollDir)
            {
                case ScrollDirection.UP:
                    deltaY = _mapScrollYSpeed;
                    break;
                case ScrollDirection.UP_RIGHT:
                    deltaX = -4 - _mapScrollXSpeed;
                    deltaY = _mapScrollYSpeed + 2;
                    break;
                case ScrollDirection.RIGHT:
                    deltaX = -_mapScrollXSpeed;
                    break;
                case ScrollDirection.DOWN_RIGHT:
                    deltaX = -4 - _mapScrollXSpeed;
                    deltaY = -2 - _mapScrollYSpeed;
                    break;
                case ScrollDirection.DOWN:
                    deltaY = -_mapScrollYSpeed;
                    break;
                case ScrollDirection.DOWN_LEFT:
                    deltaX = _mapScrollXSpeed + 4;
                    deltaY = -2 - _mapScrollYSpeed;
                    break;
                case ScrollDirection.LEFT:
                    deltaX = _mapScrollXSpeed;
                    break;
                case ScrollDirection.UP_LEFT:
                    deltaX = _mapScrollXSpeed + 4;
                    deltaY = _mapScrollYSpeed + 2;
                    break;
            }

            _timeLastScrollDirectionChange = TimePoint.Now;
            if (Globals.Config.ScrollAcceleration)
            {
                _mapScrollX += deltaX;
                _mapScrollY += deltaY;
            }
            else
            {
                ScrollBy(deltaX, deltaY);
            }
        }

        [TempleDllLocation(0x100056e0)]
        public void Dispose()
        {
            Tig.RenderingDevice.RemoveResizeListener(_resizeListener);
        }

        [TempleDllLocation(0x10307318)]
        private locXY _someLocation;

        private int _resizeListener;

        [TempleDllLocation(0x10005b40)]
        public void SetLocation(locXY loc)
        {
            // TODO: This may be unused
            _someLocation = loc;
        }

        [TempleDllLocation(0x10005720)]
        public void SetMapId(int mapId)
        {
            if (mapId == 5000)
            {
                _scrollMainMenuRefPoint = TimePoint.Now;
                _mainMenuScrollState = 0;
            }

            _currentLimits = _mapLimits.GetValueOrDefault(mapId, MapLimits.Default);

            // This is a TemplePlus extension:
            if (mapId != 5000)
            {
                var deltaW = _currentLimits.Left - _currentLimits.Right;
                var deltaH = _currentLimits.Top - _currentLimits.Bottom;
                if (deltaW < _screenSize.Width + 100)
                {
                    _currentLimits.Left += (_screenSize.Width - deltaW) / 2 + 50;
                    _currentLimits.Right -= (_screenSize.Width - deltaW) / 2 + 50;
                }

                if (deltaH < _screenSize.Height + 100)
                {
                    _currentLimits.Top += (_screenSize.Height - deltaH) / 2 + 50;
                    _currentLimits.Bottom -= (_screenSize.Height - deltaH) / 2 + 50;
                }
            }
        }
    }

    public enum ScrollDirection
    {
        UP = 0,
        UP_RIGHT = 1,
        RIGHT = 2,
        DOWN_RIGHT = 3,
        DOWN = 4,
        DOWN_LEFT = 5,
        LEFT = 6,
        UP_LEFT = 7
    }

    public struct MapLimits
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public static MapLimits Default => new MapLimits
        {
            Left = 9000,
            Top = 0,
            Right = -9000,
            Bottom = -18000
        };
    }
}