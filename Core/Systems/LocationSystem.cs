using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class LocationSystem : IGameSystem, IBufferResettingSystem
    {
        public const bool IsEditor = false;

        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x1002a9a0)]
        public LocationSystem()
        {
            _screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;

            // TODO
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10029990)]
        [TempleDllLocation(0x10808D00)]
        public int LocationTranslationX { get; set; }

        [TempleDllLocation(0x10029990)]
        [TempleDllLocation(0x10808D48)]
        public int LocationTranslationY { get; set; }

        [TempleDllLocation(0x10808D38)]
        public int LocationLimitX { get; set; } = int.MaxValue;

        [TempleDllLocation(0x10808D20)]
        public int LocationLimitY { get; set; } = int.MaxValue;

        [TempleDllLocation(0x10808D28)]
        public long LocationLimitYScroll { get; set; }

        private Size _screenSize;

        public delegate void MapCenterCallback(int centerTileX, int centerTileY);

        [TempleDllLocation(0x10808D5C)]
        [TempleDllLocation(0x100299C0)]
        public event MapCenterCallback OnMapCentered;

        [TempleDllLocation(0x1002A1E0)]
        private void ScreenToTile(int screenX, int screenY, out int tileX, out int tileY)
        {
            var a = (screenX - LocationTranslationX) / 2;
            var b = (int) (((screenY - LocationTranslationY) / 2)  / 0.7f);
            tileX = (b - a) / 20;
            tileY = (b + a) / 20;
        }

        /// <summary>
        /// Given a rectangle in screen coordinates, calculates the rectangle in
        /// tile-space that is visible.
        /// </summary>
        [TempleDllLocation(0x1002A6B0)]
        public bool GetVisibleTileRect(in Rectangle screenRect, out TileRect tiles)
        {
            // TODO: This way of figuring out the visible tiles has to go,
            // TODO since it does not use the camera transforms, but rather
            // TODO hardcoded assumptions about projection.

            var rect = screenRect;
            ScreenToTile(rect.X, rect.Y, out _, out tiles.y1);
            ScreenToTile(rect.X + rect.Width, rect.Y, out tiles.x1, out _);
            ScreenToTile(rect.X, rect.Y + rect.Height, out tiles.x2, out _);
            ScreenToTile(rect.X + rect.Width, rect.Y + rect.Height, out _, out tiles.y2);
            if (tiles.x1 > tiles.x2 || tiles.y1 > tiles.y2)
                return false;

            // NOTE: A lot of this function dealt with the location limits, which were set
            //       to uint.MaxValue, meaning they never applied.

            return tiles.x1 < tiles.x2 && tiles.y1 < tiles.y2;
        }

        [TempleDllLocation(0x10028e10)]
        public void GetTranslation(int tileX, int tileY, out int translationX, out int translationY)
        {
            translationX = LocationTranslationX + (tileY - tileX - 1) * 20;
            translationY = LocationTranslationY + (tileX + tileY) * 14;
        }

        [TempleDllLocation(0x10029810)]
        public void GetTranslationDelta(int x, int y, out int deltaX, out int deltaY)
        {
            var prevX = LocationTranslationX;
            var prevY = LocationTranslationY;
            LocationTranslationX = 0;
            LocationTranslationY = 0;
            GetTranslation(0, 0, out var originTransX, out var originTransY);
            GetTranslation(x, y, out var tileTransX, out var tileTransY);
            deltaX = originTransX + _screenSize.Width / 2 - tileTransX - prevX;
            deltaY = originTransY + _screenSize.Height / 2 - tileTransY - prevY;
            LocationTranslationX = prevX;
            LocationTranslationY = prevY;
        }

        [TempleDllLocation(0x1002A580)]
        public void CenterOn(int tileX, int tileY)
        {
            GetTranslationDelta(tileX, tileY, out var xa, out var ya);
            AddTranslation(xa, ya);
            OnMapCentered?.Invoke(tileX, tileY);
        }

        [TempleDllLocation(0x1002a3e0)]
        public void AddTranslation(int x, int y)
        {
            if (!IsEditor)
            {
                if (x + LocationTranslationX <= _screenSize.Width / 2)
                {
                    if (y + LocationTranslationY + LocationLimitYScroll > _screenSize.Height / 2)
                    {
                        if (!ScreenToLoc(_screenSize.Width - x, -y, out _))
                            return;
                        if (!ScreenToLoc(_screenSize.Width - x, _screenSize.Height - y, out _))
                            return;
                    }
                }
                else if (y + LocationTranslationY + LocationLimitYScroll > _screenSize.Height / 2)
                {
                    if (!ScreenToLoc(-x, -y, out _))
                        return;
                    if (!ScreenToLoc(-x, _screenSize.Height - y, out _))
                        return;
                }
            }

            LocationTranslationX += x;
            LocationTranslationY += y;
            UpdateProjectionMatrix();
        }

        private struct CameraParams
        {
            public float xOffset;
            public float yOffset;
            public float scale;
        }

        [TempleDllLocation(0x1002a310)]
        private void UpdateProjectionMatrix()
        {
            if (LocationLimitX >= 0)
            {
                var cameraParams = new CameraParams();

                GetTranslation(0, 0, out var translationX, out var translationY);
                cameraParams.xOffset = translationX + 20.0f;
                cameraParams.yOffset = translationY;
                if (GameSystems.Map.GetCurrentMapId() != 5000 || IsEditor)
                {
                    cameraParams.scale = 1.0f;
                    Update3dProjMatrix(cameraParams);
                }
                else
                {
                    cameraParams.scale = _screenSize.Height / 600.0f;
                    Update3dProjMatrix(cameraParams);
                }
            }
        }

        private void Update3dProjMatrix(CameraParams cameraParams)
        {
            var camera = Tig.RenderingDevice.GetCamera();

            camera.SetTranslation(LocationTranslationX, LocationTranslationY);
            camera.SetScale(cameraParams.scale);
        }

        [TempleDllLocation(0x100290c0)]
        public bool ScreenToLoc(int x, int y, out locXY locOut)
        {
            var deltaXHalf = (x - LocationTranslationX) / 2;
            var deltaYHalf = (int) (((y - LocationTranslationY) / 2) / 0.7f);
            if (deltaXHalf <= deltaYHalf)
            {
                var tileX = (deltaYHalf - deltaXHalf) / 20;
                if ((deltaYHalf - deltaXHalf) < 0)
                    tileX = --tileX;
                if (tileX >= 0 && tileX < LocationLimitX)
                {
                    if (deltaXHalf + deltaYHalf >= 0)
                    {
                        var tileY = (deltaYHalf + deltaXHalf) / 20;
                        if (deltaYHalf + deltaXHalf < 0)
                            --tileY;
                        if (tileY >= 0 && tileY < LocationLimitY)
                        {
                            locOut = new locXY(tileX, tileY);
                            return true;
                        }
                    }
                }
            }

            locOut = default;
            return false;
        }

        [TempleDllLocation(0x1002a8f0)]
        public bool SetLimits(ulong limitX, ulong limitY)
        {
            Logger.Debug("location_set_limits( {0}, {1} )", limitX, limitY);
            if (limitX > 0x100000000 || limitY > 0x100000000)
            {
                return false;
            }
            else
            {
                LocationTranslationX = 0;
                LocationTranslationY = 0;
                LocationLimitX = (int) Math.Min(int.MaxValue, limitX);
                LocationLimitY = (int) Math.Min(int.MaxValue, limitY);
                LocationLimitYScroll = LocationLimitY * 14;
                return true;
            }
        }

        [TempleDllLocation(0x1002a170)]
        public locXY GetLimitsCenter()
        {
            var limitX = Math.Min(LocationLimitX, 640000);
            var limitY = Math.Min(LocationLimitY, 640000);
            return new locXY(limitX / 2, limitY / 2);
        }

        public void ResetBuffers()
        {
            _screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
        }
    }

    public static class LocationExtensions
    {
        [TempleDllLocation(0x100236e0)]
        public static float DistanceToObjInFeet(this GameObjectBody fromObj, GameObjectBody toObj)
        {
            if (fromObj == null || toObj == null)
            {
                return 0.0f;
            }

            if (fromObj.IsItem())
            {
                var parent = GameSystems.Item.GetParent(fromObj);
                if (parent == toObj)
                    return 0.0f;
            }

            if (toObj.IsItem())
            {
                var parent = GameSystems.Item.GetParent(toObj);
                if (parent == fromObj)
                    return 0.0f;
            }

            var fromLoc = fromObj.GetLocationFull();
            var toLoc = toObj.GetLocationFull();
            var fromRadius = fromObj.GetRadius();
            var toRadius = toObj.GetRadius();
            var result = (fromLoc.DistanceTo(toLoc) - (fromRadius + toRadius)) / locXY.INCH_PER_FEET;
            return result;
        }

        [TempleDllLocation(0x10023800)]
        public static float DistanceToLocInFeet(this GameObjectBody fromObj, LocAndOffsets toLoc)
        {
            var fromLoc = fromObj.GetLocationFull();
            var fromRadius = fromObj.GetRadius();
            var result = (fromLoc.DistanceTo(toLoc) - fromRadius) / locXY.INCH_PER_FEET;
            return result;
        }

        [TempleDllLocation(0x1001f870)]
        public static CompassDirection GetCompassDirection(this GameObjectBody self, GameObjectBody other)
        {
            return self.GetLocation().GetCompassDirection(other.GetLocation());
        }

    }
}