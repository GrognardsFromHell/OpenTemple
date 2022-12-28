using System;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Ui;
using Rectangle = System.Drawing.Rectangle;

namespace OpenTemple.Core.Systems;

public class LocationSystem : IGameSystem
{
    public const bool IsEditor = false;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private Size ScreenSize => Size.Truncate(GameViews.Primary?.Camera.ViewportSize ?? Size.Empty);

    [TempleDllLocation(0x1002a9a0)]
    public LocationSystem()
    {
    }

    public void Dispose()
    {
    }

    [TempleDllLocation(0x10029990, true)]
    [TempleDllLocation(0x10808D00)]
    public int LocationTranslationX { get; set; }

    [TempleDllLocation(0x10029990, true)]
    [TempleDllLocation(0x10808D48)]
    public int LocationTranslationY { get; set; }

    [TempleDllLocation(0x10808D38)]
    public int LocationLimitX { get; set; } = int.MaxValue;

    [TempleDllLocation(0x10808D20)]
    public int LocationLimitY { get; set; } = int.MaxValue;

    [TempleDllLocation(0x10808D28)]
    public long LocationLimitYScroll { get; set; }

    public delegate void MapCenterCallback(int centerTileX, int centerTileY);

    [TempleDllLocation(0x10808D5C)]
    [TempleDllLocation(0x100299C0)]
    public event MapCenterCallback OnMapCentered;

    /// <summary>
    /// Given a rectangle in screen coordinates, calculates the rectangle in
    /// tile-space that is visible.
    /// </summary>
    [TempleDllLocation(0x1002A6B0)]
    public bool GetVisibleTileRect(IGameViewport viewport, in RectangleF screenRect, out TileRect tiles)
    {
        // TODO: This way of figuring out the visible tiles has to go,
        // TODO since it does not use the camera transforms, but rather
        // TODO hardcoded assumptions about projection.

        var rect = screenRect;
        tiles.y1 = viewport.ScreenToTile(rect.X, rect.Y).location.locy;
        tiles.x1 = viewport.ScreenToTile(rect.X + rect.Width, rect.Y).location.locx;
        tiles.x2 = viewport.ScreenToTile(rect.X, rect.Y + rect.Height).location.locx;
        tiles.y2 = viewport.ScreenToTile(rect.X + rect.Width, rect.Y + rect.Height).location.locy;
        if (tiles.x1 > tiles.x2 || tiles.y1 > tiles.y2)
        {
            return false;
        }

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
        deltaX = originTransX + ScreenSize.Width / 2 - tileTransX - prevX;
        deltaY = originTransY + ScreenSize.Height / 2 - tileTransY - prevY;
        LocationTranslationX = prevX;
        LocationTranslationY = prevY;
    }

    [TempleDllLocation(0x1002A580)]
    public void CenterOn(int tileX, int tileY)
    {
        if (GameViews.Primary == null)
        {
            return;
        }

        GetTranslationDelta(tileX, tileY, out var xa, out var ya);
        AddTranslation(xa, ya);
        OnMapCentered?.Invoke(tileX, tileY);
    }

    [TempleDllLocation(0x1002a3e0)]
    public void AddTranslation(int x, int y)
    {
        var screenSize = ScreenSize;

        if (!IsEditor)
        {
            if (x + LocationTranslationX <= screenSize.Width / 2)
            {
                if (y + LocationTranslationY + LocationLimitYScroll > screenSize.Height / 2)
                {
                    if (!ScreenToLoc(screenSize.Width - x, -y, out _))
                        return;
                    if (!ScreenToLoc(screenSize.Width - x, screenSize.Height - y, out _))
                        return;
                }
            }
            else if (y + LocationTranslationY + LocationLimitYScroll > screenSize.Height / 2)
            {
                if (!ScreenToLoc(-x, -y, out _))
                    return;
                if (!ScreenToLoc(-x, screenSize.Height - y, out _))
                    return;
            }
        }

        LocationTranslationX += x;
        LocationTranslationY += y;
        UpdateProjectionMatrix();
    }

    [TempleDllLocation(0x1002a310)]
    private void UpdateProjectionMatrix()
    {
        if (LocationLimitX >= 0)
        {
            if (GameSystems.Map.GetCurrentMapId() != 5000 || IsEditor)
            {
                Update3dProjMatrix(1.0f);
            }
            else
            {
                Update3dProjMatrix(ScreenSize.Height / 600.0f);
            }
        }
    }

    private void Update3dProjMatrix(float scale)
    {
        var camera = GameViews.Primary.Camera;
        camera.SetTranslation(LocationTranslationX, LocationTranslationY);
        // camera.Scale = scale;
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

    // This is a TemplePlus extension
    public LocAndOffsets TrimToLength(LocAndOffsets srcLoc, LocAndOffsets tgtLoc, float lengthInches)
    {
        var src = srcLoc.ToInches2D();
        var tgt = tgtLoc.ToInches2D();
        var normDir = Vector2.Normalize(tgt - src);

        var trimmedTgt = src + normDir * lengthInches;

        return LocAndOffsets.FromInches(trimmedTgt.X, trimmedTgt.Y);
    }

    // TODO: This was originally part of a different file (target.c??)
    [TempleDllLocation(0x100b99a0)]
    public bool TargetRandomTile(GameObject obj, int distance, out locXY locOut)
    {
        locOut = default;

        var sourceLocation = obj.GetLocation();
        var targetTile = sourceLocation;

        var relPosCode = (CompassDirection) GameSystems.Random.GetInt(0, 8);

        for (var i = 0; i < distance; i++)
        {
            targetTile = targetTile.Offset(relPosCode);
        }

        var pkt = new AnimPathData();
        pkt.destLoc = targetTile;
        pkt.handle = obj;
        pkt.srcLoc = sourceLocation;
        pkt.size = 200;
        pkt.deltas = new sbyte[pkt.size];
        pkt.flags = AnimPathDataFlags.UNK10;
        pkt.size = GameSystems.PathX.AnimPathSearch(ref pkt);

        if (pkt.size == 0 && pkt.distTiles <= 0 || pkt.size >= distance + 2)
        {
            locOut = sourceLocation;
            return true;
        }

        if (pkt.distTiles == 0)
        {
            pkt.distTiles = pkt.size;
        }

        targetTile = sourceLocation;
        for (var i = 0; i < pkt.distTiles; i++)
        {
            if (obj.IsCritter())
            {
                using var objListResult = ObjList.ListTile(targetTile, ObjectListFilter.OLC_CRITTERS);
                if (objListResult.Count == 0)
                {
                    break;
                }
            }

            targetTile = targetTile.Offset((CompassDirection) pkt.deltas[i]);
        }

        locOut = targetTile;
        return true;
    }
}

public static class LocationExtensions
{
    [TempleDllLocation(0x100236e0)]
    public static float DistanceToObjInFeet(this GameObject fromObj, GameObject toObj,
        bool clampToZero = true)
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
        if (clampToZero && result < 0)
        {
            result = 0;
        }

        return result;
    }

    [TempleDllLocation(0x10023800)] /* This version actually does not clamp to zero */
    public static float DistanceToLocInFeet(this GameObject fromObj, LocAndOffsets toLoc,
        bool clampToZero = true)
    {
        var fromLoc = fromObj.GetLocationFull();
        var fromRadius = fromObj.GetRadius();
        var result = (fromLoc.DistanceTo(toLoc) - fromRadius) / locXY.INCH_PER_FEET;
        if (clampToZero && result < 0)
        {
            result = 0;
        }

        return result;
    }

    [TempleDllLocation(0x1001f870)]
    public static CompassDirection GetCompassDirection(this GameObject self, GameObject other)
    {
        return self.GetLocation().GetCompassDirection(other.GetLocation());
    }
}