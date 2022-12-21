using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.IO.MapMarkers;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Ui.WorldMap;

namespace OpenTemple.Core.Ui.TownMap;

public class TownMapUi : IResetAwareSystem, ISaveGameAwareUi
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10be1f74)]
    private bool _isAvailable;

    [TempleDllLocation(0x10128b60)]
    public bool IsVisible => _mainWindow.IsInTree;

    private readonly WidgetContainer _mainWindow;

    private readonly WidgetButton _worldMapButton;

    private readonly TownMapContent _mapContent;

    private readonly WidgetScrollView _visitedMapsList;

    [TempleDllLocation(0x10BE1F48)]
    private int _currentMapId;

    // Saves the last view settings (position and zoom) for each map
    [TempleDllLocation(0x10be0d70)]
    private readonly Dictionary<int, TownmapPosition> _savedMapViewSettings = new();

    [TempleDllLocation(0x10be1f54)]
    [TempleDllLocation(0x10be1f58)]
    [TempleDllLocation(0x10be1f60)]
    [TempleDllLocation(0x10be1f5c)]
    private Rectangle _townMapWorldRect;

    [TempleDllLocation(0x11e69568)]
    private bool _showFogOfWar;

    [TempleDllLocation(0x10be17b0)]
    private readonly Dictionary<int, List<TownMapMarker>> _userMarkers = new();

    // Stores which predefined markers have been revealed for the user
    private readonly HashSet<PredefinedMarkerId> _revealedMarkers = new();

    private readonly WidgetButton _placeMarkerButton;

    private readonly WidgetButton _removeMarkerButton;

    private readonly WidgetButton _zoomButton;

    public TownMapUi()
    {
        var doc = WidgetDoc.Load("ui/townmap_ui.json");

        _mainWindow = doc.GetRootContainer();
        _mainWindow.AddActionHotkey(UiHotkeys.Cancel, Hide);

        var exit = doc.GetButton("exit");
        exit.AddClickListener(Hide);

        _worldMapButton = doc.GetButton("worldMapButton");
        _worldMapButton.AddClickListener(OpenWorldMap);

        // Since we're on the worldmap, this button switches to the townmap
        var currentMapButton = doc.GetButton("currentMapButton");
        currentMapButton.AddClickListener(SwitchToCurrentMap);

        var contentContainer = doc.GetContainer("mapContent");
        _mapContent = new TownMapContent(new Rectangle(Point.Empty, contentContainer.Size));
        contentContainer.Add(_mapContent);
        _mapContent.OnControlModeChange += UpdateModeButtons;
        _mapContent.OnBeforeRender += UpdateCurrentMapData;
        _mapContent.OnMarkerCreated += MarkerCreated;
        _mapContent.OnMarkerRemoved += MarkerRemoved;

        var centerOnPartyButton = doc.GetButton("centerOnPartyButton");
        centerOnPartyButton.AddClickListener(CenterOnPartyClicked);

        _placeMarkerButton = doc.GetButton("placeMarkerButton");
        _placeMarkerButton.AddClickListener(EnterPlaceMarkerMode);

        _removeMarkerButton = doc.GetButton("removeMarkerButton");
        _removeMarkerButton.AddClickListener(EnterDeleteMarkerMode);

        _zoomButton = doc.GetButton("zoomButton");
        _zoomButton.AddClickListener(EnterZoomMode);

        _visitedMapsList = doc.GetScrollView("visitedMapsList");
    }

    private void UpdateModeButtons(TownMapControlMode mode)
    {
        _placeMarkerButton.SetActive(mode == TownMapControlMode.PlaceMarker);
        _removeMarkerButton.SetActive(mode == TownMapControlMode.RemoveMarker);
        _zoomButton.SetActive(mode == TownMapControlMode.Zoom);
    }

    private void SwitchToCurrentMap()
    {
        var currentMapId = GameSystems.Map.GetCurrentMapId();
        ChangeCurrentMap(currentMapId);
    }

    [TempleDllLocation(0x1012c340)]
    private void OpenWorldMap()
    {
        if (UiSystems.WorldMap.IsWorldMapAccessible)
        {
            Hide();
            UiSystems.WorldMap.Show();
        }
    }

    [TempleDllLocation(0x1012c1d0)]
    private void UpdateWorldMapButton()
    {
        if (UiSystems.WorldMap.IsWorldMapAccessible)
        {
            _worldMapButton.TooltipText = null;
            _worldMapButton.Disabled = false;
        }
        else
        {
            _worldMapButton.Disabled = true;
            _worldMapButton.TooltipText = "#{townmap:200}";
        }
    }

    [TempleDllLocation(0x1012c6a0)]
    public void Show()
    {
        if (GameSystems.Combat.IsCombatActive())
        {
            return;
        }

        if (!UiSystems.RandomEncounter.DontAskToExitMap &&
            UiSystems.WorldMap.NeedToClearEncounterMap)
        {
            UiSystems.RandomEncounter.AskToExitMap();
            return;
        }

        var mapNum = GameSystems.Map.GetCurrentMapId();
        if (GameSystems.Map.IsRandomEncounterMap(mapNum))
        {
            UiSystems.WorldMap.Show(WorldMapMode.LeaveRandomEncounter);
            return;
        }

        if (!IsTownMapAvailable)
        {
            return;
        }

        if (!IsVisible)
        {
            GameSystems.TimeEvent.PauseGameTime();
        }

        UiSystems.HideOpenedWindows(true);
        Globals.UiManager.AddWindow(_mainWindow);
        _mainWindow.BringToFront();
        UpdateWorldMapButton();

        if (_currentMapId != GameSystems.Map.GetCurrentMapId())
        {
            ChangeCurrentMap(GameSystems.Map.GetCurrentMapId());
            UiTownmapCenterOnParty();
        }

        UpdateVisitedMapsList();
    }

    [TempleDllLocation(0x1012bcb0)]
    public void Hide()
    {
        if (!Globals.UiManager.RemoveWindow(_mainWindow))
        {
            return;
        }

        _mapContent.Reset();

        if (UiSystems.TextEntry.IsVisible)
        {
            Logger.Info("Canceling editing a marker because the townmap is closing.");
            UiSystems.TextEntry.Cancel();
        }

        GameSystems.TimeEvent.ResumeGameTime();
    }

    private void UpdateVisitedMapsList()
    {
        _visitedMapsList.Clear(true);

        var currentY = 0;
        foreach (var visitedMap in GameSystems.Map.VisitedMaps)
        {
            var visitedMapButton = new WidgetButton(new Rectangle(
                0, currentY,
                179, 15
            ));
            visitedMapButton.SetStyle("visitedMapButton");
            visitedMapButton.Text = GameSystems.Map.GetMapDescription(visitedMap);
            visitedMapButton.AddClickListener(() =>
            {
                if (_currentMapId != visitedMap)
                {
                    ChangeCurrentMap(visitedMap);
                }
            });
            _visitedMapsList.Add(visitedMapButton);

            currentY += visitedMapButton.Height + 2;
        }
    }

    [TempleDllLocation(0x1012b490)]
    public void ChangeCurrentMap(int mapId)
    {
        _mapContent.PartyPositions = new List<locXY>();

        // Save the current position on the townmap
        if (_currentMapId != 0)
        {
            _savedMapViewSettings[_currentMapId] = new TownmapPosition
            {
                XOffset = _mapContent.XTranslation,
                YOffset = _mapContent.YTranslation,
                Zoom = _mapContent.Zoom
            };
        }

        LoadTownMapData(mapId, out _townMapWorldRect);

        LoadMarkers(mapId);

        _currentMapId = mapId;

        // TODO: Move this logic into the map content display
        float minZoomToFitMap;
        if (_townMapWorldRect.Width >= _townMapWorldRect.Height)
        {
            minZoomToFitMap = _mapContent.Height / (float) _townMapWorldRect.Height;
        }
        else
        {
            minZoomToFitMap = _mapContent.Width / (float) _townMapWorldRect.Width;
        }

        _mapContent.MinZoom = MathF.Max(0.1f, minZoomToFitMap);

        if (!_savedMapViewSettings.ContainsKey(_currentMapId))
        {
            // Center on the map's starting position
            var startingPos = GameSystems.Map.GetStartPos(_currentMapId);
            CenterOn(startingPos);
            _mapContent.Zoom = _mapContent.MinZoom;
        }
        else
        {
            _mapContent.XTranslation = _savedMapViewSettings[_currentMapId].XOffset;
            _mapContent.YTranslation = _savedMapViewSettings[_currentMapId].YOffset;
            _mapContent.Zoom = _savedMapViewSettings[_currentMapId].Zoom;
        }
    }

    // TODO: This was previously in the ground system
    [TempleDllLocation(0x1002c530)]
    private void LoadTownMapData(int mapId, out Rectangle worldRect)
    {
        var townMapData = TownMapData.LoadFromFolder($"townmap/{mapId}");
        _mapContent.SetData(townMapData);
        worldRect = townMapData.WorldRectangle;

        _showFogOfWar = !GameSystems.Map.IsUnfogged(mapId);
        // If the map is not the active map, we'll need to load the map fog information from disk,
        // and we'll only do it once.
        if (_showFogOfWar && mapId != GameSystems.Map.GetCurrentMapId())
        {
            var saveDir = GameSystems.Map.GetSaveDir(mapId);
            _mapContent.FogOfWar = GameSystems.MapFogging.LoadTownMapFogOfWar(saveDir);
        }
        else
        {
            _mapContent.FogOfWar = null;
        }
    }

    private void UpdateCurrentMapData()
    {
        // Take the fog from the live map
        if (GameSystems.Map.GetCurrentMapId() == _currentMapId)
        {
            if (_showFogOfWar)
            {
                _mapContent.FogOfWar = GameSystems.MapFogging.TownMapFogTiles;
            }

            _mapContent.PartyPositions = GameSystems.Party.PartyMembers
                .Select(critter => critter.GetLocation())
                .ToList();
        }
    }

    private void LoadMarkers(int mapId)
    {
        // Load markers
        var markersFile = $"{GameSystems.Map.GetDataDir(mapId)}/map_markers.json";
        var markers = ImmutableList.CreateBuilder<TownMapMarker>();
        if (Tig.FS.FileExists(markersFile))
        {
            var predefinedMarkers = MapMarkersReader.Load(Tig.FS, markersFile);
            foreach (var predefinedMarker in predefinedMarkers.Markers)
            {
                if (!_revealedMarkers.Contains(new PredefinedMarkerId(mapId, predefinedMarker.Id)))
                {
                    continue;
                }

                var marker = new TownMapMarker(new locXY(predefinedMarker.X, predefinedMarker.Y))
                {
                    Text = predefinedMarker.Text
                };
                markers.Add(marker);
            }
        }

        Logger.Info("Loaded {0} predefined map markers from {1}", markers.Count, markersFile);

        // Add user-defined markers on top
        if (_userMarkers.TryGetValue(mapId, out var userMarkers))
        {
            markers.AddRange(userMarkers);
            Logger.Info("Loaded {0} user-defined map-markers", userMarkers.Count);
        }

        _mapContent.Markers = markers.ToImmutable();
    }

    [TempleDllLocation(0x10128420)]
    public bool IsTownMapAvailable
    {
        get
        {
            var result = _isAvailable;
            if (!_isAvailable)
            {
                var curmap = GameSystems.Map.GetCurrentMapId();
                if (GameSystems.Map.IsVignetteMap(curmap))
                {
                    result = _isAvailable;
                }
                else
                {
                    result = true;
                    _isAvailable = true;
                }
            }

            return result;
        }
    }

    [TempleDllLocation(0x1012bb40)]
    public void Reset()
    {
        _currentMapId = -1;
        _revealedMarkers.Clear();
        _savedMapViewSettings.Clear();
        _userMarkers.Clear();
        _mapContent.Reset();
        _isAvailable = false;
    }

    [TempleDllLocation(0x10128650)]
    public void SaveGame(SavedUiState savedState)
    {
        savedState.TownmapState = new SavedTownmapUiState
        {
            IsAvailable = _isAvailable
        };
        Stub.TODO();
    }

    [TempleDllLocation(0x101288f0)]
    public void LoadGame(SavedUiState savedState)
    {
        var townmapState = savedState.TownmapState;

        _isAvailable = townmapState.IsAvailable;

        foreach (var revealedMapMarker in townmapState.RevealedMapMarkers)
        {
            _revealedMarkers.Add(revealedMapMarker);
        }

        foreach (var userMarker in townmapState.UserMapMarkers)
        {
            if (!_userMarkers.ContainsKey(userMarker.MapId))
            {
                _userMarkers[userMarker.MapId] = new List<TownMapMarker>();
            }

            var marker = new TownMapMarker(userMarker.Position)
            {
                IsUserMarker = true,
                Text = userMarker.Text
            };
            _userMarkers[userMarker.MapId].Add(marker);
        }
    }

    [TempleDllLocation(0x1012c660)]
    public void CenterOnPartyClicked()
    {
        if (_currentMapId != GameSystems.Map.GetCurrentMapId())
        {
            ChangeCurrentMap(GameSystems.Map.GetCurrentMapId());
        }

        UiTownmapCenterOnParty();
    }

    [TempleDllLocation(0x10129d40)]
    private void EnterPlaceMarkerMode()
    {
        if (_mapContent.ControlMode != TownMapControlMode.PlaceMarker)
        {
            _mapContent.ControlMode = TownMapControlMode.PlaceMarker;
        }
        else
        {
            _mapContent.ControlMode = TownMapControlMode.Pan;
        }
    }

    [TempleDllLocation(0x10129d80)]
    private void EnterDeleteMarkerMode()
    {
        if (_mapContent.ControlMode != TownMapControlMode.RemoveMarker)
        {
            _mapContent.ControlMode = TownMapControlMode.RemoveMarker;
        }
        else
        {
            _mapContent.ControlMode = TownMapControlMode.Pan;
        }
    }

    [TempleDllLocation(0x10129dc0)]
    private void EnterZoomMode()
    {
        if (_mapContent.ControlMode != TownMapControlMode.Zoom)
        {
            _mapContent.ControlMode = TownMapControlMode.Zoom;
        }
        else
        {
            _mapContent.ControlMode = TownMapControlMode.Pan;
        }
    }

    [TempleDllLocation(0x1012ba70)]
    public void UiTownmapCenterOnParty()
    {
        var leader = GameSystems.Party.GetConsciousLeader();

        locXY centerOnTile;
        if (leader != null)
        {
            centerOnTile = leader.GetLocation();
        }
        else
        {
            centerOnTile = GameSystems.Map.GetStartPos(_currentMapId);
        }

        CenterOn(centerOnTile);
    }

    private void CenterOn(locXY position)
    {
        if (GameViews.Primary == null)
        {
            return;
        }

        var centerOnPos = TownMapContent.TileToWorld(position);

        var zoomx2 = 2 * _mapContent.Zoom;
        _mapContent.XTranslation = centerOnPos.X - _townMapWorldRect.X - _mapContent.Width / zoomx2;
        _mapContent.YTranslation = centerOnPos.Y - _townMapWorldRect.Y - _mapContent.Height / zoomx2;
    }

    private struct TownmapPosition
    {
        public float XOffset { get; set; }
        public float YOffset { get; set; }
        public float Zoom { get; set; }
    }

    [TempleDllLocation(0x10128360)]
    public void RevealMarker(int mapId, int markerId)
    {
        if (!_revealedMarkers.Add(new PredefinedMarkerId(mapId, markerId)))
        {
            Logger.Info("Marker {0} on map {1} was already revealed.", markerId, mapId);
            return;
        }

        Logger.Info("Revealing marker {0} on map {1}.", markerId, mapId);
        UiSystems.UtilityBar.PulseMapButton();
    }

    private void MarkerCreated(TownMapMarker marker)
    {
        if (!_userMarkers.TryGetValue(_currentMapId, out var userMarkers))
        {
            userMarkers = new List<TownMapMarker>();
            _userMarkers[_currentMapId] = userMarkers;
        }

        userMarkers.Add(marker);
    }

    private void MarkerRemoved(TownMapMarker marker)
    {
        if (_userMarkers.TryGetValue(_currentMapId, out var userMarkers))
        {
            userMarkers.Remove(marker);
        }
    }
}

public class TownMapMarker
{
    public locXY Position { get; }

    public bool IsUserMarker { get; set; }

    public string Text { get; set; }

    public TownMapMarker(locXY position)
    {
        Position = position;
    }
}