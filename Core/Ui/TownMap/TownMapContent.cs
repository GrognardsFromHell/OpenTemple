using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.TownMap;

public class TownMapContent : WidgetButtonBase
{
    [TempleDllLocation(0x10be0d54)]
    private const string CursorClosedHand = "art/interface/cursors/Map_GrabHand_Closed.tga";

    [TempleDllLocation(0x10be0d58)]
    private const string CursorOpenHand = "art/interface/cursors/Map_GrabHand_Open.tga";

    [TempleDllLocation(0x10be0d60)]
    private const string CursorRemoveMarker = "art/interface/cursors/DeleteFlagCursor.tga";

    [TempleDllLocation(0x10be0d68)]
    private const string CursorPlaceMarker = "art/interface/cursors/PlaceFlagCursor.tga";

    [TempleDllLocation(0x10be1780)]
    private const string CursorZoom = "art/interface/cursors/ZoomCursor.tga";

    private const string TexturePredefined = "art/interface/TOWNMAP_UI/Info_Tool_NPC_FLAG.tga";

    private const string TextureCustom = "art/interface/TOWNMAP_UI/Info_Tool_DROPPED_FLAG.tga";

    private const string TextureParty = "art/interface/TOWNMAP_UI/Info_Tool_Player_Marker.tga";

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    /// <summary>
    /// The unscaled marker size in pixels.
    /// </summary>
    private const int MarkerSize = 50;

    /// <summary>
    /// The unsacled size for party position indicators.
    /// </summary>
    private const int PartySize = 32;

    [TempleDllLocation(0x10be1f4c)]
    public float XTranslation { get; set; }

    [TempleDllLocation(0x10be1f50)]
    public float YTranslation { get; set; }

    [TempleDllLocation(0x102f8960)]
    public float Zoom { get; set; }

    [TempleDllLocation(0x10be1f54)]
    [TempleDllLocation(0x10be1f58)]
    [TempleDllLocation(0x10be1f5c)]
    [TempleDllLocation(0x10be1f60)]
    private Rectangle MapBoundingBox { get; set; }

    [TempleDllLocation(0x10be1f44)]
    public float MinZoom { get; set; } = 0.1f;

    private const float MaxZoom = 0.25f;

    private readonly TownMapTileRenderer _tileRenderer;

    private ImmutableList<TownMapMarker> _markers = ImmutableList<TownMapMarker>.Empty;

    private readonly List<WidgetImage> _markerImages = new();

    public ImmutableList<TownMapMarker> Markers
    {
        get => _markers;
        set
        {
            _markers = value;
            UpdateMarkerImages();
        }
    }

    public event Action<TownMapMarker>? OnMarkerCreated;

    public event Action<TownMapMarker>? OnMarkerRemoved;

    private readonly List<locXY> _partyPositions = new();

    private readonly List<WidgetImage> _partyPositionImages = new();

    public List<locXY> PartyPositions
    {
        get => _partyPositions;
        set
        {
            _partyPositions.Clear();
            _partyPositions.AddRange(value);
            ResizeImagePool(_partyPositionImages, _partyPositions.Count);
            foreach (var image in _partyPositionImages)
            {
                image.SetTexture(TextureParty);
            }
        }
    }

    public TownmapFogTile[,] FogOfWar
    {
        set => _tileRenderer.UpdateFogOfWar(value);
    }

    public event Action<string>? OnCursorChanged;

    public event Action<TownMapControlMode>? OnControlModeChange;

    [TempleDllLocation(0x10BE1F41)]
    private bool _grabbingMap;

    [TempleDllLocation(0x102f8964)]
    private float _grabMapX;

    [TempleDllLocation(0x102f8968)]
    private float _grabMapY;

    [TempleDllLocation(0x10be1f40)]
    private bool _zooming;

    [TempleDllLocation(0x102f896c)]
    private float _zoomStartX;

    [TempleDllLocation(0x102f8970)]
    private float _zoomStartY;

    public TownMapContent(Rectangle rect) : base(rect)
    {
        _tileRenderer = new TownMapTileRenderer();
        AddContent(_tileRenderer);
    }

    public void SetData(TownMapData mapData)
    {
        _tileRenderer.LoadData(mapData);
        MapBoundingBox = mapData.WorldRectangle;
    }

    [TempleDllLocation(0x10be1f64)]
    public TownMapControlMode ControlMode
    {
        get => _controlMode;
        set
        {
            if (value != _controlMode)
            {
                _controlMode = value;
                OnControlModeChange?.Invoke(_controlMode);
            }
        }
    }

    // Handle mouse-wheel zooming
    [TempleDllLocation(0x1012c870)]
    protected override void HandleMouseWheel(WheelEvent e)
    {
        var oldZoom = Zoom;

        Zoom += e.DeltaY * 0.1f / 400.0f;
        Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);

        // Try to keep the map centered on the previous center point after zooming
        var zoomDelta = 1.0f / oldZoom - 1.0f / Zoom;
        XTranslation = (float) Width / 2 * zoomDelta + XTranslation;
        YTranslation = (float) Height / 2 * zoomDelta + YTranslation;
    }

    [TempleDllLocation(0x1012c870)]
    protected override void HandleMouseMove(MouseEvent e)
    {
        if (HasMouseCapture)
        {
            if (ControlMode == TownMapControlMode.Pan)
            {
                if (!_grabbingMap)
                {
                    _grabbingMap = true;
                    _grabMapX = e.X;
                    _grabMapY = e.Y;
                    OnCursorChanged?.Invoke(CursorClosedHand);
                }

                // We accumulate mouse movement until we actually see a change in the map translation
                // based on the current zoom-level.
                var deltaX = _grabMapX - e.X;
                var deltaY = _grabMapY - e.Y;
                var translationDeltaX = deltaX / Zoom; 
                var translationDeltaY = deltaY / Zoom;
                if (Math.Abs(translationDeltaX) >= 1 || Math.Abs(translationDeltaY) >= 1)
                {
                    XTranslation += translationDeltaX;
                    YTranslation += translationDeltaY;
                    _grabMapX = e.X;
                    _grabMapY = e.Y;                    
                }
            }
            else if (ControlMode == TownMapControlMode.Zoom)
            {
                if (_zooming)
                {
                    var oldZoom = Zoom;

                    Zoom += (_zoomStartY - e.Y) / 400.0f;
                    Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);

                    // Ensures the point on the map that we started zooming on stays under the mouse
                    var zoomDelta = 1.0f / oldZoom - 1.0f / Zoom;
                    var contentArea = GetContentArea();
                    XTranslation = (_zoomStartX - contentArea.X) * zoomDelta + XTranslation;
                    YTranslation = (_zoomStartY - contentArea.Y) * zoomDelta + YTranslation;
                }
                else
                {
                    _zooming = true;
                    Tig.Mouse.PushCursorLock();
                    _zoomStartX = e.X;
                    _zoomStartY = e.Y;
                }
            }
        }
    }
    
    [TempleDllLocation(0x1012c870)]
    protected override void HandleMouseEnter(MouseEvent e)
    {
        switch (ControlMode)
        {
            case TownMapControlMode.Pan:
                _grabbingMap = false;
                OnCursorChanged?.Invoke(CursorOpenHand);
                break;
            case TownMapControlMode.Zoom:
                OnCursorChanged?.Invoke(CursorZoom);
                break;
            case TownMapControlMode.PlaceMarker:
                OnCursorChanged?.Invoke(CursorPlaceMarker);
                break;
            case TownMapControlMode.RemoveMarker:
                OnCursorChanged?.Invoke(CursorRemoveMarker);
                break;
        }
    }

    [TempleDllLocation(0x1012c870)]
    protected override void HandleMouseDown(MouseEvent e)
    {
        if (e.Button == MouseButton.LEFT && SetMouseCapture())
        {
            Logger.Info("Drag Start");
            _grabbingMap = true;
            _grabMapX = e.X;
            _grabMapY = e.Y;
        }
    }

    [TempleDllLocation(0x1012c870)]
    protected override void HandleMouseUp(MouseEvent e)
    {
        if (!HasMouseCapture)
        {
            if (e.Button == MouseButton.RIGHT && ControlMode != TownMapControlMode.Pan)
            {
                ControlMode = TownMapControlMode.Pan;
                OnCursorChanged?.Invoke(CursorOpenHand);
            }
            else if (e.Button == MouseButton.LEFT)
            {
                switch (ControlMode)
                {
                    case TownMapControlMode.Pan:
                        OnCursorChanged?.Invoke(CursorOpenHand);
                        break;
                    case TownMapControlMode.Zoom:
                        OnCursorChanged?.Invoke(CursorZoom);
                        break;
                    case TownMapControlMode.PlaceMarker:
                        OnCursorChanged?.Invoke(CursorPlaceMarker);
                        break;
                    case TownMapControlMode.RemoveMarker:
                        OnCursorChanged?.Invoke(CursorRemoveMarker);
                        break;
                }

                e.StopPropagation();
            }

            return;
        }

        if (ControlMode == TownMapControlMode.Zoom)
        {
            OnCursorChanged?.Invoke(CursorZoom);
            if (_zooming)
            {
                Tig.Mouse.PopCursorLock();
                _zooming = false;
            }
        }
        else if (ControlMode == TownMapControlMode.PlaceMarker)
        {
            if (!UiSystems.TextEntry.IsVisible)
            {
                OnCursorChanged?.Invoke(CursorPlaceMarker);
                if (Markers.Count >= 20)
                {
                    return;
                }

                // The mouse message x,y coordinates need to be mapped into the widget's coordinates
                var contentArea = GetContentArea();
                var worldLoc = ProjectViewToWorld(new Point(
                    (int) e.X - contentArea.X,
                    (int) e.Y - contentArea.Y
                ));
                var marker = new TownMapMarker(worldLoc)
                {
                    IsUserMarker = true,
                    Text = " "
                };
                Markers = Markers.Add(marker);

                CreateOrEditMarker(marker, true);
            }
        }
        else if (ControlMode is TownMapControlMode.RemoveMarker or TownMapControlMode.Pan)
        {
            var marker = GetHoveredFlagWidgetIdx(e.X, e.Y);
            if (marker is {IsUserMarker: true})
            {
                // Only allows user-defined markers to be removed, essentially
                if (ControlMode == TownMapControlMode.RemoveMarker)
                {
                    Markers = Markers.Remove(marker);
                    OnMarkerRemoved?.Invoke(marker);
                }
                else if (ControlMode == default)
                {
                    // Clicking on a marker without any special control mode will allow it to be renamed
                    CreateOrEditMarker(marker, false);
                }
            }

            if (ControlMode == TownMapControlMode.RemoveMarker)
            {
                OnCursorChanged?.Invoke(CursorRemoveMarker);
            }
            else
            {
                OnCursorChanged?.Invoke(CursorOpenHand);
            }
        }

        _grabbingMap = false;
        Logger.Info("Drag Stop - Released");
        ReleaseMouseCapture();
    }

    [TempleDllLocation(0x1012c870)]
    protected override void HandleLostMouseCapture(MouseEvent e)
    {
        // TODO: Fat note on the mouse capture: It's broken in Vanilla.
        // TODO: Since Exited will still be triggered even if the mouse is captured, it'll actually cancel the panning...
        ControlMode = default;
        OnCursorChanged?.Invoke(null);
                
        if (_grabbingMap)
        {
            _grabbingMap = false;
            _grabMapX = -1;
            _grabMapY = -1;            
            Logger.Info("Drag Stop - Aborted");
        }

        if (_zooming)
        {
            Tig.Mouse.PopCursorLock();
            _zooming = false;
        }
    }

    [TempleDllLocation(0x1012b8e0)]
    [TempleDllLocation(0x1012b860)]
    private void CreateOrEditMarker(TownMapMarker marker, bool creatingMarker)
    {
        // Translate the position of the flag into on-screen coordinates to place the dialog
        var screenPos = ProjectWorldToView(marker.Position);
        var contentRect = GetContentArea();
        var screenX = contentRect.X + screenPos.X;
        var screenY = contentRect.Y + screenPos.Y;

        var crNamePkt = new UiCreateNamePacket
        {
            OkButtonLabel = "#{townmap:150}",
            CancelButtonLabel = "#{townmap:151}",
            InitialValue = marker.Text,
            DialogTitle = "#{townmap:152}",
            DialogX = screenX + 10,
            DialogY = screenY + 10,
            Callback = (enteredName, confirmed) =>
            {
                if (confirmed)
                {
                    Logger.Info("Changing townmap marker text to {0}", enteredName);
                    marker.Text = enteredName;
                    if (creatingMarker)
                    {
                        OnMarkerCreated?.Invoke(marker);
                    }
                }
                else if (creatingMarker)
                {
                    Logger.Info("Removing the text marker on cancel.");
                    Markers = Markers.Remove(marker);
                }
            }
        };
        UiSystems.TextEntry.ShowTextEntry(crNamePkt);
    }

    /// <summary>
    /// Project a position from the game world into the coordinate system of this widget,
    /// respecting panning and zooming.
    /// </summary>
    private Point ProjectWorldToView(locXY location)
    {
        // This projects the tile into the x,y space of the town map
        var townMapPos = TileToWorld(location);

        // This makes it relative to the provided town map tiles
        var x = (townMapPos.X - MapBoundingBox.Left - XTranslation) * Zoom;
        var y = (townMapPos.Y - MapBoundingBox.Top - YTranslation) * Zoom;

        return new Point((int) MathF.Round(x), (int) MathF.Round(y));
    }

    /// <summary>
    /// This is the inverse of ProjectWorldToView.
    /// </summary>
    private locXY ProjectViewToWorld(Point point)
    {
        // Start by transforming point from the viewport into the actual coordinate
        // space of the background map
        var tileMapX = (point.X / Zoom) + XTranslation + MapBoundingBox.Left;
        var tileMapY = (point.Y / Zoom) + YTranslation + MapBoundingBox.Top;

        // Now apply the reverse transform of what's happening in GameCamera.TileToWorld
        locXY position = default;
        var x = tileMapX / 20;
        var y = tileMapY / 14;
        position.locx = (int) MathF.Round((x - y + 1) / -2);
        position.locy = (int) MathF.Round(y - position.locx);
        return position;
    }

    [TempleDllLocation(0x1012ba10)]
    private TownMapMarker? GetHoveredFlagWidgetIdx(float x, float y)
    {
        var contentArea = GetContentArea();
        x -= contentArea.X;
        y -= contentArea.Y;

        for (var i = 0; i < _markerImages.Count; i++)
        {
            var image = _markerImages[i];
            if (x >= image.X
                && x < image.X + image.FixedWidth
                && y >= image.Y
                && y < image.Y + image.FixedHeight)
            {
                return _markers[i];
            }
        }

        return null;
    }

    [TempleDllLocation(0x1012c040)]
    protected override void UpdateLayout()
    {
        var contentRect = GetContentArea();

        if (XTranslation < 0.0f)
        {
            XTranslation = 0.0f;
        }

        var v3 = contentRect.Width / Zoom;
        if (v3 + XTranslation > MapBoundingBox.Width)
        {
            XTranslation = MapBoundingBox.Width - v3;
        }

        if (YTranslation < 0.0f)
        {
            YTranslation = 0;
        }

        var v5 = contentRect.Height / Zoom;
        if (v5 + YTranslation > MapBoundingBox.Height)
        {
            YTranslation = MapBoundingBox.Height - v5;
        }

        var srcRect = new Rectangle(
            (int) (MapBoundingBox.X + XTranslation),
            (int) (MapBoundingBox.Y + YTranslation),
            (int) (contentRect.Width / Zoom),
            (int) (contentRect.Height / Zoom)
        );

        _tileRenderer.SourceRect = srcRect;

        UpdatePartyPositions();
        UpdateMarkerPositions();

        base.UpdateLayout();
    }

    private void ResizeImagePool(List<WidgetImage> images, int count)
    {
        // Shrink the images
        while (images.Count > count)
        {
            RemoveContent(images[^1]);
            images[^1].Dispose();
            images.RemoveAt(images.Count - 1);
        }

        // Grow the images
        while (images.Count < count)
        {
            var image = new WidgetImage();
            AddContent(image);
            images.Add(image);
        }
    }

    private void UpdateMarkerImages()
    {
        ResizeImagePool(_markerImages, _markers.Count);

        // Update the image paths
        for (var i = 0; i < _markerImages.Count; i++)
        {
            var marker = _markers[i];
            var image = _markerImages[i];
            image.SetTexture(marker.IsUserMarker ? TextureCustom : TexturePredefined);
        }
    }

    private void UpdatePartyPositions()
    {
        var dimension = (int) (PartySize * 4 * Zoom);

        // Position widgets accordingly
        for (var i = 0; i < _partyPositions.Count; i++)
        {
            var position = _partyPositions[i];
            var image = _partyPositionImages[i];
            var imagePos = ProjectWorldToView(position);
            image.X = imagePos.X - dimension / 2;
            image.Y = imagePos.Y - dimension / 2;
            image.FixedWidth = dimension;
            image.FixedHeight = dimension;
        }
    }

    private void UpdateMarkerPositions()
    {
        var dimension = (int) (MarkerSize * 4 * Zoom);

        // Position widgets accordingly
        for (var i = 0; i < _markerImages.Count; i++)
        {
            var marker = _markers[i];
            var image = _markerImages[i];
            var markerPos = ProjectWorldToView(marker.Position);
            image.X = markerPos.X - dimension / 2;
            image.Y = markerPos.Y - dimension / 2;
            image.FixedWidth = dimension;
            image.FixedHeight = dimension;
        }
    }

    private readonly WidgetTooltipRenderer _tooltipRenderer = new();
    private TownMapControlMode _controlMode = TownMapControlMode.Pan;

    [TempleDllLocation(0x1012c180)]
    public override void RenderTooltip(int x, int y)
    {
        if (!UiSystems.TextEntry.IsVisible)
        {
            var marker = GetHoveredFlagWidgetIdx(x, y);
            if (marker != null)
            {
                _tooltipRenderer.TooltipText = marker.Text;
                _tooltipRenderer.Render(x, y);
            }
        }
    }

    public void Reset()
    {
        _grabbingMap = false;
        if (HasMouseCapture)
        {
            ReleaseMouseCapture();
        }

        if (_zooming)
        {
            Tig.Mouse.PopCursorLock();
            _zooming = false;
        }
    }

    // replaces 10028EC0
    // Apparently used for townmap projection !
    [TempleDllLocation(0x10028ec0)]
    public static Vector2 TileToWorld(locXY tilePos)
    {
        return new Vector2(
            (tilePos.locy - tilePos.locx - 1) * 20,
            (tilePos.locy + tilePos.locx) * 14
        );
    }
}

public enum TownMapControlMode
{
    Pan = 0,
    Zoom = 1,
    PlaceMarker = 2,
    RemoveMarker = 3
}