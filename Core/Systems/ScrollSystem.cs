using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class ScrollSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private const bool IsEditor = false;

        /// <summary>
        /// Used to animate the scrolling of the main menu background map.
        /// </summary>
        [TempleDllLocation(0x10307350)]
        private TimePoint _scrollMainMenuRefPoint;

        [TempleDllLocation(0x10307380)]
        private float _mainMenuScrollState = 0;

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

        [TempleDllLocation(0x10005E70)]
        public ScrollSystem()
        {
            Globals.Config.AddVanillaSetting("scroll_speed", "3", ReReadScrollConfig);
            Globals.Config.AddVanillaSetting("scroll_butter", "300", ReReadScrollConfig);
            Globals.Config.AddVanillaSetting("scroll_butter_type", "1", ReReadScrollConfig);

            ReReadScrollConfig();

            _screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;

            CalculateScrollSpeed();
            GameSystems.Location.OnMapCentered += CenterViewDirectly;

            _mapLimits = LoadMapLimits();

            _currentLimits = MapLimits.Default;
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
            _scrollSpeed = Globals.Config.GetVanillaInt("scroll_speed");
            ScrollButter = 2 * Globals.Config.GetVanillaInt("scroll_butter");
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
            if (_scrollSpeed < 0)
            {
                Globals.Config.SetVanillaInt("scroll_speed", 0);
            }

            if (_scrollSpeed > 4)
            {
                Globals.Config.SetVanillaInt("scroll_speed", 4);
            }

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
        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10005700)]
        public void Reset()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10006000)]
        public void AdvanceTime(TimePoint time)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10006480)]
        public void SetScrollDirection(int scrollDir)
        {
        }

        [TempleDllLocation(0x102AC238)]
        public int ScrollButter { get; private set; }

        [TempleDllLocation(0x100056e0)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10307318)]
        private locXY _someLocation;

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