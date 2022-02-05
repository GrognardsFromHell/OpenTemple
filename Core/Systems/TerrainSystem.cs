using System;
using System.Collections.Generic;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using SharpDX;

namespace OpenTemple.Core.Systems;

public class TerrainSystem : IGameSystem, IModuleAwareSystem, IResetAwareSystem
{
    /// <summary>
    /// Both width and height of a single tile in pixels.
    /// </summary>
    private const int TileSize = 256;

    /// <summary>
    /// Max. dimensions of a map in tiles.
    /// </summary>
    private const int MapWidthTiles = 66;

    private const int MapHeightTiles = 71;

    /// <summary>
    /// Max. dimensions of a map in pixels.
    /// </summary>
    private const int MapWidthPixels = MapWidthTiles * TileSize;

    private const int MapHeightPixels = MapHeightTiles * TileSize;

    /// Time to fade between the day/night versions upon transitioning
    private static readonly TimeSpan TransitionTime = TimeSpan.FromSeconds(2);

    /// Offset for the map art ids for nighttime ids
    private const int NightArtIdOffset = 1000;

    private readonly RenderingDevice _device;
    private readonly ShapeRenderer2d _shapeRenderer;

    private int _mapArtId;

    /// Tracking for the day/night transition
    private bool _isNightTime;

    private bool _isTransitioning;
    private TimePoint _transitionStart;
    private float _transitionProgress;

    /// Terrain colors are managed by the daylight system
    /// Previously initialized in ground_init
    [TempleDllLocation(0x11E69574)]
    [TempleDllLocation(0x11E69570)]
    [TempleDllLocation(0x11E69564)]
    private PackedLinearColorA _terrainTint = PackedLinearColorA.White;

    public LinearColor Tint
    {
        get => new(_terrainTint);
        set => _terrainTint = new PackedLinearColorA(value);
    }

    private Dictionary<int, string> _terrainDirs;

    [TempleDllLocation(0x1002c100)]
    public TerrainSystem(RenderingDevice device, ShapeRenderer2d shapeRenderer2d)
    {
        _device = device;
        _shapeRenderer = shapeRenderer2d;
    }

    [TempleDllLocation(0x1002dc40)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x1002DC70)]
    public void Render(WorldCamera camera)
    {
        using var _ = _device.CreatePerfGroup("Terrain");

        // Special dirty case for the 5000 map which has no terrain
        if (GameSystems.Map.GetCurrentMapId() == 5000)
        {
            return;
        }

        // Check the day<.night transition and stop if necessary
        if (_isTransitioning)
        {
            var timeSinceMapEntered = TimePoint.Now - _transitionStart;
            if (timeSinceMapEntered > TransitionTime)
            {
                _isTransitioning = false;
            }

            _transitionProgress =
                (float) (timeSinceMapEntered.TotalMilliseconds / TransitionTime.TotalMilliseconds);
        }

        var viewportWidth = camera.ViewportSize.Width;
        var viewportHeight = camera.ViewportSize.Height;

        // The terrain is centered on the center tile of the map
        // This is 480,480 for all vanilla maps, but theoretically it
        // depends on the map size specified in the map's map.prp file
        var mapCenter = GameSystems.Location.GetLimitsCenter();

        // The center of the map in pixels relative to the current screen viewport
        // GameSystems.Location.GetTranslation(mapCenter.locx, mapCenter.locy,
        //     out var terrainOriginX, out var terrainOriginY);
        var mapCenterScreen = camera.WorldToScreenUi(mapCenter.ToInches3D());

        // Since the origin is still pointing at the map center, shift it left/up by half
        // the terrains overall size. The Map is slightly off-center for *some* unknown
        // reason. This may be a relic from Arkanum
        var terrainOriginX = (int) mapCenterScreen.X - MapWidthPixels / 2 + 20;
        var terrainOriginY = (int) mapCenterScreen.Y - MapHeightPixels / 2;

        var startX = -terrainOriginX / TileSize;
        var startY = -terrainOriginY / TileSize;

        for (var y = startY; y < MapHeightTiles; ++y)
        {
            var destRect = new RectangleF();
            destRect.Y = terrainOriginY + y * TileSize;
            destRect.Height = TileSize;
            for (var x = startX; x < MapWidthTiles; ++x)
            {
                destRect.X = terrainOriginX + x * TileSize;
                destRect.Width = TileSize;

                // Get the correct texture for this tile
                RenderTile(x, y, destRect);

                // The next column would be out of view
                if (destRect.X + TileSize >= viewportWidth)
                {
                    break;
                }
            }

            // The next column would be out of view
            if (destRect.Y + TileSize >= viewportHeight)
            {
                break;
            }
        }
    }

    private void RenderTile(int x, int y, RectangleF destRect)
    {
        var primaryMapArtId = _mapArtId;

        // Handling transition
        if (_isTransitioning)
        {
            // This is flipped while we transition, since we have to draw
            // the old map first
            if (!_isNightTime)
            {
                primaryMapArtId += NightArtIdOffset;
            }
        }
        else
        {
            if (_isNightTime)
            {
                primaryMapArtId += NightArtIdOffset;
            }
        }

        var textureName = GetTileTexture(primaryMapArtId, x, y);
        using var texture = _device.GetTextures().Resolve(textureName, false);

        if (!texture.Resource.IsValid())
        {
            return;
        }

        var color = _terrainTint;

        var destX = destRect.X;
        var destY = destRect.Y;
        var destWidth = destRect.Width;
        var destHeight = destRect.Height;

        _shapeRenderer.DrawRectangle(destX, destY, destWidth, destHeight, texture.Resource, color);

        if (_isTransitioning)
        {
            // Use the real map here
            if (_isNightTime)
            {
                primaryMapArtId = _mapArtId + NightArtIdOffset;
            }
            else
            {
                primaryMapArtId = _mapArtId;
            }

            textureName = GetTileTexture(primaryMapArtId, x, y);
            using var otherTexture = _device.GetTextures().Resolve(textureName, false);

            // Draw the "new" map over the old one with alpha that is based on
            // the time since the transition started
            color.A = (byte) (_transitionProgress * 255.0f);

            _shapeRenderer.DrawRectangle(destX, destY, destWidth, destHeight, otherTexture.Resource, color);
        }
    }

    [TempleDllLocation(0x1002d290)]
    public void UpdateDayNight()
    {
        if (GameSystems.TimeEvent.IsDaytime)
        {
            // Start the night . day transition
            if (_isNightTime)
            {
                _isTransitioning = true;
                _isNightTime = false;
                _transitionStart = TimePoint.Now;
            }
        }
        else
        {
            // Start the day . night transition
            if (!_isNightTime)
            {
                _isTransitioning = true;
                _isNightTime = true;
                _transitionStart = TimePoint.Now;
            }
        }
    }

    public void Load(int groundArtId)
    {
        if (_mapArtId == groundArtId)
        {
            return;
        }

        _mapArtId = groundArtId;
        _isTransitioning = false;
        _isNightTime = GameSystems.TimeEvent.IsDaytime;
    }

    public void LoadModule()
    {
        var groundMapping = Tig.FS.ReadMesFile("art/ground/ground.mes");
        _terrainDirs = new Dictionary<int, string>(groundMapping.Count);
        foreach (var (groundArtId, name) in groundMapping)
        {
            _terrainDirs[groundArtId] = $"art/ground/{name}/";
        }
    }

    public void UnloadModule()
    {
        _terrainDirs.Clear();
    }

    private string GetTileTexture(int mapArtId, int x, int y)
    {
        // Find the directory that the JPEGs are kept in
        if (!_terrainDirs.TryGetValue(mapArtId, out var basePath))
        {
            return null;
        }

        return $"{basePath}{y:x04}{x:x04}.jpg";
    }

    public void Reset()
    {
        _isTransitioning = false;
    }
}