using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.IO.WorldMapPaths;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Teleport;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.WorldMap;

public class WorldMapUi : IResetAwareSystem, ISaveGameAwareUi
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private const string TrailDotTexture = "art/interface/WORLDMAP_UI/worldmap_trail_dot.tga";

    /// <summary>
    /// This is how many pixels of path are between each trail dot.
    /// </summary>
    private const int PixelsPerTrailDot = 12;

    // Was previously configurable via worldmap_ui_locations.mes (key 3002)
    [TempleDllLocation(0x10bef5c4)]
    private const int YouAreHereDefaultRadius = 25;

    [TempleDllLocation(0x10bef7dc)]
    [TempleDllLocation(0x10159b10)]
    public bool IsVisible => _mainWindow.IsInTree;

    [TempleDllLocation(0x11ea2406)]
    private readonly WorldMapPath[] _paths;

    private readonly WidgetContainer _mainWindow;

    [TempleDllLocation(0x10beee68)]
    private int _originalVolume;

    [TempleDllLocation(0x10bef808)]
    private WorldMapMode _mode;

    [TempleDllLocation(0x10beee6c)]
    private int _soundStream1;

    [TempleDllLocation(0x10bef780)]
    private int _soundStream2;

    [TempleDllLocation(0x102fb3e8)]
    private WorldMapLocation? _travelByDialogDestination = null;

    [TempleDllLocation(0x10bef81c)]
    private bool _destinationReached;

    [TempleDllLocation(0x10bef818)]
    private bool _randomEncounterTriggered;

    [TempleDllLocation(0x102fb3d4)]
    [TempleDllLocation(0x102fb3d8)]
    private PointF _randomEncounterPoint;

    [TempleDllLocation(0x10beee60)]
    [TempleDllLocation(0x10bef788)]
    private PointF _lastTrailPos;

    [TempleDllLocation(0x10bef814)]
    private RandomEncounter? _randomEncounterDetails;

    [TempleDllLocation(0x10159790)]
    [TempleDllLocation(0x10bef800)]
    public bool NeedToClearEncounterMap { get; private set; }

    [TempleDllLocation(0x102fb3d0)]
    private int _randomEncounterStatus = 1;

    [TempleDllLocation(0x102fb3cc)]
    private int _teleportMapId;

    private WidgetButton _currentMapButton;

    private readonly List<WorldMapLocationWidgets> _locationWidgets;

    private YouAreHereWidget _youAreHereWidget;

    [TempleDllLocation(0x102fb3c8)]
    private int _currenTrailDotIndex;

    [TempleDllLocation(0x10bef7f4)]
    private int dword_10BEF7F4;

    private WidgetContainer _locationList;

    private readonly WidgetContainer _mapContent;

    private readonly WidgetContainer _trailDotsContainer;

    private readonly List<WidgetImage> _trailDots = new();

    [TempleDllLocation(0x10beece8)]
    private readonly List<TimePoint> _trailDotTimes = new();

    [TempleDllLocation(0x10bef804)]
    public int dword_10BEF804 { get; set; }

    private readonly DecodedImage _terrainMap;

    private static readonly IImmutableDictionary<PackedLinearColorA, MapTerrain> TerrainColors =
        new Dictionary<PackedLinearColorA, MapTerrain>
        {
            {new PackedLinearColorA(0, 255, 0, 255), MapTerrain.Scrub},
            {new PackedLinearColorA(0, 127, 0, 255), MapTerrain.Forest},
            {new PackedLinearColorA(255, 255, 0, 255), MapTerrain.Scrub | MapTerrain.RoadFlag},
            {new PackedLinearColorA(0, 0, 127, 255), MapTerrain.Riverside},
            {new PackedLinearColorA(0, 0, 255, 255), MapTerrain.Riverside | MapTerrain.RoadFlag},
            {new PackedLinearColorA(255, 0, 0, 255), MapTerrain.Swamp},
            {new PackedLinearColorA(127, 0, 0, 255), MapTerrain.Swamp | MapTerrain.RoadFlag},
        }.ToImmutableDictionary();

    public WorldMapUi()
    {
        using var pathReader = Tig.FS.OpenBinaryReader("art/interface/WORLDMAP_UI/worldmap_ui_paths.bin");
        _paths = WorldMapPathReader.Read(pathReader);

        var locations = WorldMapLocations.LoadFromJson("ui/worldmap_locations.json");

        var doc = WidgetDoc.Load("ui/worldmap_ui.json");
        _mainWindow = doc.GetRootContainer();
        // Allow closing the window while no trip is taking place
        _mainWindow.AddActionHotkey(UiHotkeys.Cancel, Hide, () => !IsMakingTrip);
        _mainWindow.AddInterval(UpdateTime);

        // This is the container that is sized exactly as the actual map in the window
        _mapContent = doc.GetContainer("mapContent");

        _locationList = doc.GetContainer("locationList");

        // Create the required widgets for each of the locations
        _locationWidgets = new List<WorldMapLocationWidgets>();
        foreach (var location in locations.Locations)
        {
            var widgets = new WorldMapLocationWidgets(location, _mapContent);
            widgets.OnClick += LocationClicked;
            _locationWidgets.Add(widgets);
        }

        var exit = doc.GetButton("exit");
        exit.AddClickListener(() =>
        {
            if (!IsMakingTrip)
            {
                Hide();
            }
        });

        // Since we're already on the worldmap, this button does nothing
        var worldMapButton = doc.GetButton("worldMapButton");
        worldMapButton.SetActive(true);

        // Since we're on the worldmap, this button switches to the townmap
        _currentMapButton = doc.GetButton("currentMapButton");
        _currentMapButton.AddClickListener(SwitchToTownMap);

        var centerOnParty = doc.GetButton("centerOnPartyButton");
        centerOnParty.AddClickListener(() =>
        {
            // TODO: This is ... weird. The renderer will swap in a greyed out texture, but the message
            // handler will still react ?!
            if (!IsMakingTrip)
            {
                Hide();
                Show();
            }

            UiSystems.TownMap.CenterOnPartyClicked();
        });

        // This is placed on top of the other map widgets to show the trail dots above the location icons
        _trailDotsContainer = new WidgetContainer();
        _trailDotsContainer.Anchors.FillParent();
        _mapContent.Add(_trailDotsContainer);

        _youAreHereWidget = new YouAreHereWidget();
        _mapContent.Add(_youAreHereWidget);

        _terrainMap = ImageIO.DecodeImage(Tig.FS, "ui/worldmap_terrain.png");
    }

    [TempleDllLocation(0x1015f320)]
    private void LocationClicked(WorldMapLocation location)
    {
        if (IsMakingTrip)
        {
            return;
        }

        var currentArea = GameSystems.Area.GetCurrentArea();
        if (TryGetWidgetsForArea(currentArea, out var widgets))
        {
            MakeTrip(widgets, location);
        }
    }

    private void SwitchToTownMap()
    {
        if (_mode != WorldMapMode.LeaveRandomEncounter && IsMakingTrip)
        {
            Hide();
            UiSystems.TownMap.Show();
        }
    }

    [TempleDllLocation(0x10bef824)]
    private TimePoint uiRndEncTrailDotTime = default;

    [TempleDllLocation(0x1015a9b0)]
    private void UpdateTime()
    {
        if (_destinationReached)
        {
            TeleportToDestination();
            _destinationReached = false;
            return;
        }

        if (_randomEncounterTriggered)
        {
            CreateRandomEncounter(_lastTrailPos, _randomEncounterDetails);
            _randomEncounterTriggered = false;
        }

        if (_currenTrailDotIndex >= _trailDots.Count)
        {
            _currenTrailDotIndex = _trailDots.Count;
        }

        // Fade out previously faded-in trail dots after 2 seconds to 25% opacity
        for (var i = 0; i < _currenTrailDotIndex; i++)
        {
            if (_trailDotTimes[i] == default)
            {
                _trailDotTimes[i] = TimePoint.Now;
                Tig.Sound.SetStreamSourceFromSoundId(_soundStream2, 3002);
            }

            var fadeOutElapsed = TimePoint.Now - _trailDotTimes[i];
            var alpha = 1.0f - (float) fadeOutElapsed.TotalSeconds / 2;
            if (alpha <= 0.3f)
            {
                alpha = 0.25f;
            }

            _trailDots[i].Visible = true;
            _trailDots[i].Color = PackedLinearColorA.OfFloats(1, 1, 1, alpha);
        }

        if (!IsMakingTrip)
        {
            return;
        }

        if (uiRndEncTrailDotTime == default)
        {
            if (dword_10BEF804 == 0)
            {
                if (_randomEncounterStatus == 1
                    && _currenTrailDotIndex > 3
                    && _currenTrailDotIndex + 3 < uiWorldmapTrailDotIdx + 1
                    && !UiSystems.WorldMapRandomEncounter.IsActive)
                {
                    var currentTrailDot = _trailDots[_currenTrailDotIndex];
                    _lastTrailPos.X = currentTrailDot.X + currentTrailDot.FixedWidth / 2;
                    _lastTrailPos.Y = currentTrailDot.Y + currentTrailDot.FixedHeight / 2;

                    var terrain = GetTerrain(_lastTrailPos);

                    var reInfo = new RandomEncounterQuery(
                        terrain,
                        RandomEncounterType.Traveling
                    );

                    if (GameSystems.RandomEncounter.Query(reInfo, out _randomEncounterDetails))
                    {
                        Logger.Info("encounter FOUND - ID: {0}, MAP: {1}, FLAGS: {2}",
                            _randomEncounterDetails.Id,
                            _randomEncounterDetails.Map,
                            _randomEncounterDetails.Flags
                        );

                        // Search for the party member with the highest survival skill to roll against the encounter
                        GameObject partyMemberWithHighestSkill = null;
                        var highestSurvivalSkill = -1;
                        foreach (var partyMember in GameSystems.Party.PartyMembers)
                        {
                            foreach (var enemySpec in _randomEncounterDetails.Enemies)
                            {
                                var enemy = GameSystems.Proto.GetProtoById(enemySpec.ProtoId);
                                var survivalSkill = GameSystems.Skill.GetEffectiveBonus(partyMember,
                                    SkillId.wilderness_lore, enemy, SkillCheckFlags.UnderDuress);
                                if (survivalSkill > highestSurvivalSkill)
                                {
                                    highestSurvivalSkill = survivalSkill;
                                    partyMemberWithHighestSkill = partyMember;
                                }
                            }
                        }

                        // Now for every player that is not the player with the highest survival skill, perform
                        // an assist-check to provide the other player with a +2 bonus. This is somewhat weird,
                        // since assisting would require knowledge of what to assist with.
                        // The original check was also VERY lenient since it allowed one attempt to beat DC 10 for every
                        // enemy in the encounter, which seems... odd?
                        var bonus = 0;
                        foreach (var partyMember in GameSystems.Party.PartyMembers)
                        {
                            if (partyMember != partyMemberWithHighestSkill)
                            {
                                for (var j = 0; j < _randomEncounterDetails.Enemies.Count; j++)
                                {
                                    if (GameSystems.Skill.TrySupportingSkillCheck(SkillId.wilderness_lore,
                                            partyMember,
                                            SkillCheckFlags.UnderDuress))
                                    {
                                        bonus += 2;
                                        break;
                                    }
                                }
                            }
                        }

                        // This decides whether the players get a chance to avoid or not
                        if (highestSurvivalSkill + bonus < _randomEncounterDetails.DC)
                        {
                            Logger.Info("random encounter not detected. lore({0}) + bonus({1}) vs. dc({2})",
                                highestSurvivalSkill, bonus, _randomEncounterDetails.DC);
                            _randomEncounterStatus = 2;
                            uiWorldMapInitialTrailDotIdx = _currenTrailDotIndex;
                            dword_10BEF804 = 0;
                            _randomEncounterTriggered = true;
                        }
                        else
                        {
                            dword_10BEF804 = 0;
                            UiSystems.WorldMapRandomEncounter.Show();
                        }
                    }
                }
            }

            if (dword_10BEF804 == 2)
            {
                Logger.Info("random encounter detected and accepted.");
                _randomEncounterStatus = 2;
                uiWorldMapInitialTrailDotIdx = _currenTrailDotIndex;
                dword_10BEF804 = 0;
                _randomEncounterTriggered = true;
            }
            else if (dword_10BEF804 == 1)
            {
                _randomEncounterStatus = 2;
                dword_10BEF804 = 0;
                _randomEncounterDetails = null;
                uiRndEncTrailDotTime = TimePoint.Now;
            }
            else if (!UiSystems.WorldMapRandomEncounter.IsActive)
            {
                dword_10BEF804 = 0;
                uiRndEncTrailDotTime = TimePoint.Now;
                _randomEncounterDetails = null;
            }
        }

        if (uiRndEncTrailDotTime != default)
        {
            var progress = (float) (TimePoint.Now - uiRndEncTrailDotTime).TotalMilliseconds / 100;
            if (progress >= 1.0)
            {
                _trailDots[_currenTrailDotIndex].Color = PackedLinearColorA.White;
                _trailDots[_currenTrailDotIndex].Visible = true;

                ++_currenTrailDotIndex;
                uiRndEncTrailDotTime = default;

                // So every "dot" traveled means 15 ingame minutes pass?
                var time = TimeSpan.FromMilliseconds(900000);
                GameSystems.TimeEvent.AddGameTime(time);

                if (_currenTrailDotIndex >= uiWorldmapTrailDotIdx)
                {
                    IsMakingTrip = false;
                    _randomEncounterStatus = 1;
                    _destinationReached = true;
                }
            }
            else
            {
                _trailDots[_currenTrailDotIndex].Color = PackedLinearColorA.OfFloats(1.0f, 1.0f, 1.0f, progress);
                _trailDots[_currenTrailDotIndex].Visible = true;
            }
        }
    }

    private MapTerrain GetTerrain(PointF pos)
    {
        // Clamp the position to the map container
        var mapWidth = (int) _mapContent.PaddingArea.Width;
        var mapHeight = (int) _mapContent.PaddingArea.Height;
        var x = Math.Clamp((int) MathF.Round(pos.X), 0, mapWidth - 1);
        var y = Math.Clamp((int) MathF.Round(pos.Y), 0, mapHeight - 1);

        var terrainMapX = x * (_terrainMap.info.width - 1) / (mapWidth - 1);
        var terrainMapY = y * (_terrainMap.info.height - 1) / (mapHeight - 1);
        var pixelColor = _terrainMap.ReadPackedPixel(terrainMapX, terrainMapY);

        if (!TerrainColors.TryGetValue(pixelColor, out var terrain))
        {
            Logger.Warn("Unknown world-map terrain color: R={0},G={1},B={2}", pixelColor.R, pixelColor.G,
                pixelColor.B);
            return MapTerrain.Forest;
        }

        return terrain;
    }

    private void PlaceYouAreHereWidget()
    {
        var currentArea = GameSystems.Area.GetCurrentArea();

        if (TryGetWidgetsForArea(currentArea, out var location))
        {
            // Use the current location's radius as the radius for the location ring
            _youAreHereWidget.Width = Dimension.Pixels(2 * location.Location.Radius);
            _youAreHereWidget.Height = Dimension.Pixels(2 * location.Location.Radius);
            _youAreHereWidget.X = location.Location.Position.X - location.Location.Radius;
            _youAreHereWidget.Y = location.Location.Position.Y - location.Location.Radius;
        }
        else
        {
            // Try placing it where the last random encounter occurred
            _youAreHereWidget.X = _randomEncounterPoint.X
                                  - YouAreHereDefaultRadius;
            _youAreHereWidget.Y = _randomEncounterPoint.Y
                                  - YouAreHereDefaultRadius;
            _youAreHereWidget.Width = Dimension.Pixels(2 * YouAreHereDefaultRadius);
            _youAreHereWidget.Height = Dimension.Pixels(2 * YouAreHereDefaultRadius);
        }
    }

    private bool TryGetWidgetsForArea(int areaId, [MaybeNullWhen(false)] out WorldMapLocationWidgets location)
    {
        foreach (var widgets in _locationWidgets)
        {
            if (widgets.Location.AreaIds.Contains(areaId))
            {
                location = widgets;
                return true;
            }
        }

        location = null;
        return false;
    }

    private IEnumerable<WorldMapLocationWidgets> EnumerateWidgetsForArea(int areaId)
    {
        return _locationWidgets.Where(widgets => widgets.Location.AreaIds.Contains(areaId));
    }

    [TempleDllLocation(0x1015f140)]
    public void Show(WorldMapMode mode = WorldMapMode.Travel)
    {
        UiSystems.HideOpenedWindows(true);
        if (_mainWindow.IsInTree)
        {
            GameViews.DisableDrawing();
            UiSystems.Party.Hide();
            GameSystems.TimeEvent.PauseGameTime();
            UiSystems.UtilityBar.Hide();

            // Interesting, it sets the effect volume to 0
            _originalVolume = Tig.Sound.EffectVolume;
            Tig.Sound.EffectVolume = 0;
        }

        _mode = mode;
        Globals.UiManager.AddWindow(_mainWindow);

        // TODO: Honestly, this should just use whether a townmap is available or not
        if (mode == WorldMapMode.LeaveRandomEncounter)
        {
            _currentMapButton.Disabled = true;
            _currentMapButton.TooltipText = "#{townmap:13}";
        }
        else
        {
            _currentMapButton.Disabled = false;
            _currentMapButton.TooltipText = null;
        }

        PlaceYouAreHereWidget();

        foreach (var widgets in _locationWidgets)
        {
            widgets.Reset();
        }

        // TODO: UNSAFE. Doesn't check if it's already allocated
        Tig.Sound.tig_sound_alloc_stream(out _soundStream1, tig_sound_type.TIG_ST_EFFECTS);
        Tig.Sound.tig_sound_alloc_stream(out _soundStream2, tig_sound_type.TIG_ST_EFFECTS);

        _currenTrailDotIndex = 1;

        for (var i = 0; i < _trailDots.Count; i++)
        {
            _trailDots[i].Visible = false;
            _trailDotTimes[i] = default;
        }

        // When we're traveling from dialog, it means we have to immediately initiate travel
        if (_mode == WorldMapMode.TravelFromDialog)
        {
            Debug.Assert(_travelByDialogDestination != null);
            var curArea = GameSystems.Area.GetCurrentArea();
            if (TryGetWidgetsForArea(curArea, out var widgets))
            {
                MakeTrip(widgets, _travelByDialogDestination);
            }
            else
            {
                Logger.Info("Failed to determine current location on world map for area {0}. Teleporting instead",
                    curArea);
                _teleportMapId = _travelByDialogDestination.TeleportMapId;
                TeleportToDestination();
            }
        }

        UpdateLocationVisibility();
    }

    [TempleDllLocation(0x102fb3e0)]
    private int uiWorldMapToId;

    [TempleDllLocation(0x102FB3E4)]
    private int uiWorldMapFromId;

    [TempleDllLocation(0x102fb3dc)]
    private int uiWorldMapInitialTrailDotIdx;

    [TempleDllLocation(0x10bef7f8)]
    private int uiWorldmapTrailDotIdx;

    [TempleDllLocation(0x1015ea20)]
    private void MakeTrip(WorldMapLocationWidgets from, WorldMapLocation to)
    {
        var fromId = _locationWidgets.IndexOf(from);
        var toId = _locationWidgets.FindIndex(widget => widget.Location == to);

        _teleportMapId = to.TeleportMapId;
        if (fromId != -1)
        {
            uiWorldMapFromId = fromId;
            if (from.Location.UsePathsOf != -1)
            {
                uiWorldMapFromId = from.Location.UsePathsOf;
            }

            uiWorldMapToId = toId;
            if (to.UsePathsOf != -1)
            {
                uiWorldMapToId = to.UsePathsOf;
            }
        }

        if (_locationWidgets[fromId].Location.UsePathsOf != -1)
        {
            fromId = _locationWidgets[fromId].Location.UsePathsOf;
        }

        if (_locationWidgets[toId].Location.UsePathsOf != -1)
        {
            toId = _locationWidgets[toId].Location.UsePathsOf;
        }

        var toPos = _locationWidgets[toId].Location.Position;
        var toX = toPos.X;
        var toY = toPos.Y;

        if (_randomEncounterStatus == 2 && toId != uiWorldMapToId && toId != uiWorldMapFromId)
        {
            var fromX = _randomEncounterPoint.X;
            var fromY = _randomEncounterPoint.Y;
            Logger.Info("travelling on the map from {0}, {1} to {2}, {3}", fromX, fromY, toX, toY);
            IsMakingTrip = true;

            // This lays out the trail dots up until the random encounter in a straight line from the
            // previous starting point. Apparently ToEE did not remember the actual trail, so they
            // used this approximation.
            var dX = fromX - toX;
            var dY = fromY - toY;
            var dotCount = (int) MathF.Sqrt(dY * dY + dX * dX) / PixelsPerTrailDot + 1;
            uiWorldmapTrailDotIdx = dotCount;
            EnsureTrailDots(dotCount + 1);
            var curX = 0f;
            var curY = 0f;
            for (var i = 0; i < uiWorldmapTrailDotIdx; i++)
            {
                SetTrailDot(i, fromX - curX / dotCount, fromY - curY / dotCount);
                _trailDots[i].Visible = true;

                curX += dX;
                curY += dY;
            }

            _randomEncounterStatus = 0;
            uiWorldMapFromId = -1;
            uiWorldMapToId = -1;
            uiWorldMapInitialTrailDotIdx = -1;
            return;
        }

        var v50 = false;
        var v45 = false;
        if (_mode == WorldMapMode.Teleport)
        {
            TeleportToDestination();
            return;
        }
        else if (_mode == WorldMapMode.TravelFromDialog)
        {
            // TODO: Why does this only apply for id=0 and id=11+ ???
            if (fromId < 1 && fromId > 10)
            {
                TeleportToDestination();
                return;
            }
        }
        else if (_randomEncounterStatus == 2)
        {
            if (toId == uiWorldMapFromId)
            {
                v50 = true;
                fromId = uiWorldMapToId;
                toId = uiWorldMapFromId;
            }
            else if (toId == uiWorldMapToId)
            {
                v45 = true;
                fromId = uiWorldMapFromId;
                toId = uiWorldMapToId;
            }
        }

        if (fromId == toId)
        {
            Logger.Info("Already at destination, no need to travel.");
            TeleportToDestination();
            return;
        }

        if (!TryFindPath(_locationWidgets[fromId].Location,
                _locationWidgets[toId].Location,
                out var pathId,
                out var reverse))
        {
            Logger.Info("Failed to find path from {0} to {1}.", fromId, toId);
            TeleportToDestination();
            return;
        }

        IsMakingTrip = true;
        LayoutPath(pathId, reverse);

        if (v45)
        {
            var dotDelta = uiWorldmapTrailDotIdx - uiWorldMapInitialTrailDotIdx;
            uiWorldmapTrailDotIdx = dotDelta;

            for (var i = 0; i < dotDelta + 1; i++)
            {
                var src = _trailDots[uiWorldMapInitialTrailDotIdx + i];
                var dest = _trailDots[i];
                dest.X = src.X;
                dest.Y = src.Y;
            }
        }
        else if (v50)
        {
            var dotDelta = uiWorldmapTrailDotIdx - uiWorldMapInitialTrailDotIdx;
            uiWorldmapTrailDotIdx -= uiWorldmapTrailDotIdx - uiWorldMapInitialTrailDotIdx;

            for (var i = 0; i < uiWorldmapTrailDotIdx + 1; i++)
            {
                var src = _trailDots[dotDelta + i];
                var dest = _trailDots[i];
                dest.X = src.X;
                dest.Y = src.Y;
            }
        }
    }

    private void SetTrailDot(int index, float x, float y)
    {
        var trailDot = _trailDots[index];
        trailDot.X = x - trailDot.FixedWidth / 2;
        trailDot.Y = y - trailDot.FixedHeight / 2;
    }

    private void EnsureTrailDots(int count)
    {
        while (_trailDots.Count < count)
        {
            var trailDot = new WidgetImage(TrailDotTexture);
            trailDot.FixedWidth = trailDot.GetPreferredSize().Width;
            trailDot.FixedHeight = trailDot.GetPreferredSize().Height;
            trailDot.Visible = false;
            _trailDotsContainer.AddContent(trailDot);
            _trailDots.Add(trailDot);
        }

        while (_trailDotTimes.Count < count)
        {
            _trailDotTimes.Add(default);
        }
    }

    private void LayoutPath(int pathId, bool reverse)
    {
        ref var path = ref _paths[pathId];

        uiWorldmapTrailDotIdx = path.Directions.Length / PixelsPerTrailDot;
        EnsureTrailDots(uiWorldmapTrailDotIdx + 1);

        var deltaX = 0;
        var deltaY = 0;
        Logger.Info(" World map travel path: {0}, reverse: {1}", pathId, reverse);
        if (reverse)
        {
            SetTrailDot(uiWorldmapTrailDotIdx, path.Start.X + deltaX, path.Start.Y + deltaY);

            var trailDotIdx = uiWorldmapTrailDotIdx - 1;
            for (var i = 0; i < path.Directions.Length; i++)
            {
                ApplyPathStep(in path, i, ref deltaX, ref deltaY);

                if ((i + 1) % PixelsPerTrailDot == 0)
                {
                    SetTrailDot(trailDotIdx, path.Start.X + deltaX, path.Start.Y + deltaY);
                    --trailDotIdx;
                }
            }
        }
        else
        {
            SetTrailDot(0, path.Start.X + deltaX, path.Start.Y + deltaY);

            var trailDotIdx = 1;
            for (var i = 0; i < path.Directions.Length; i++)
            {
                ApplyPathStep(in path, i, ref deltaX, ref deltaY);

                if ((i + 1) % PixelsPerTrailDot == 0)
                {
                    SetTrailDot(trailDotIdx, path.Start.X + deltaX, path.Start.Y + deltaY);
                    ++trailDotIdx;
                }
            }
        }
    }

    private void ApplyPathStep(in WorldMapPath path, int index, ref int x, ref int y)
    {
        switch (path.Directions[index])
        {
            // Left
            case 7:
                --x;
                break;
            // Up/Left
            case 9:
                --x;
                --y;
                break;
            // Up
            case 5:
                --y;
                break;
            // Right/Up
            case 10:
                ++x;
                --y;
                break;
            // Right
            case 8:
                ++x;
                break;
            // Left/down
            case 11:
                --x;
                ++y;
                break;
            // Right/Down
            case 12:
                ++x;
                ++y;
                break;
            // Down
            case 6:
                ++y;
                break;
            default:
                break;
        }
    }

    private bool TryFindPath(WorldMapLocation from, WorldMapLocation to, out int pathId, out bool reversed)
    {
        if (from.UsePathsOf != -1)
        {
            from = _locationWidgets[from.UsePathsOf].Location;
        }

        if (to.UsePathsOf != -1)
        {
            to = _locationWidgets[to.UsePathsOf].Location;
        }

        // Find a path that goes straight from the origin to the destination
        foreach (var outgoingPathId in from.OutgoingPaths)
        {
            if (to.IncomingPaths.Contains(outgoingPathId))
            {
                pathId = outgoingPathId;
                reversed = false;
                return true;
            }
        }

        // A path that goes from the destination to the source is also valid, but needs to be reversed
        foreach (var incomingPathId in from.IncomingPaths)
        {
            if (to.OutgoingPaths.Contains(incomingPathId))
            {
                pathId = incomingPathId;
                reversed = true;
                return true;
            }
        }

        pathId = -1;
        reversed = false;
        return false;
    }

    [TempleDllLocation(0x1015e210)]
    public void Hide()
    {
        if (Globals.UiManager.RemoveWindow(_mainWindow))
        {
            Tig.Sound.EffectVolume = _originalVolume;

            if (!Tig.Sound.IsStreamPlaying(_soundStream1))
            {
                // TODO: Isn't this the wrong condition ?! Shouldn't it fade out _IF_ its playing?
                Tig.Sound.FadeOutStream(_soundStream1, 0);
            }

            IsMakingTrip = false;
            Tig.Sound.FreeStream(_soundStream1);
            Tig.Sound.FreeStream(_soundStream2);
            _travelByDialogDestination = null;
            GameViews.EnableDrawing();
            UiSystems.Party.Show();
            UiSystems.UtilityBar.Show();
            GameSystems.TimeEvent.ResumeGameTime();
            UpdateLocationVisibility();
        }
    }

    [TempleDllLocation(0x1015dfd0)]
    public void UpdateLocationVisibility()
    {
        _locationList.Clear();

        var currentY = 3; // Previously from worldmap_ui_locations #076
        const int spacing = 2; // Previously from worldmap_ui_locations #079
        foreach (var widgets in _locationWidgets)
        {
            if (widgets.State >= WorldMapLocationState.Discovered)
            {
                _locationList.Add(widgets.ListButton);
                widgets.ListButton.X = 10;
                widgets.ListButton.Y = currentY;
                widgets.ListButton.Width = Dimension.Percent(100);
                widgets.ListButton.Height = Dimension.Pixels(15); // Previously from worldmap_ui_locations #078
                currentY += 15 + spacing;
            }
        }
    }

    [TempleDllLocation(0x101595d0)]
    [TempleDllLocation(0x10bef7fc)]
    public bool IsMakingTrip { get; private set; }

    [TempleDllLocation(0x101595e0)]
    public void AreaDiscovered(int areaId)
    {
        var buttonPulsed = false;
        foreach (var widgets in EnumerateWidgetsForArea(areaId))
        {
            if (widgets.gameareasSthg != 2 && widgets.State == WorldMapLocationState.Undiscovered)
            {
                ++dword_10BEF7F4;
                widgets.State = WorldMapLocationState.Discovered;
                widgets.gameareasSthg = 1;
                if (!buttonPulsed)
                {
                    UiSystems.UtilityBar.PulseMapButton();
                    buttonPulsed = true;
                }
            }
        }
    }

    [TempleDllLocation(0x101596a0)]
    public void OnTravelingToMap(int destMapId)
    {
        //  Records the "visited" flag for worldmap locations
        foreach (var widgets in _locationWidgets)
        {
            if (widgets.gameareasSthg2 != 2 && widgets.Location.TeleportMapId == destMapId)
            {
                foreach (var areaId in widgets.Location.AreaIds)
                {
                    AreaDiscovered(areaId);
                }

                widgets.gameareasSthg2 = 1;
                widgets.State = WorldMapLocationState.Visited;
            }
        }
    }

    [TempleDllLocation(0x101597b0)]
    public void Reset()
    {
        foreach (var widgets in _locationWidgets)
        {
            widgets.gameareasSthg = 0;
            widgets.gameareasSthg2 = 0;
            widgets.State = widgets.Location.InitialState;
        }

        NeedToClearEncounterMap = false;
        // They forgot about resetting this in vanilla.
        _randomEncounterStatus = 1;
    }

    [TempleDllLocation(0x101598b0)]
    public void SaveGame(SavedUiState savedState)
    {
        var locations = _locationWidgets
            .Where(widget => widget.State != WorldMapLocationState.Undiscovered)
            .Select((location, index) => new SavedWorldmapLocation(index,
                location.State >= WorldMapLocationState.Discovered,
                location.State >= WorldMapLocationState.Visited))
            .ToList();

        savedState.WorldmapState = new SavedWorldmapUiState
        {
            DontAskToExitEncounterMap = UiSystems.RandomEncounter.DontAskToExitMap,
            NeedToCleanEncounterMap = NeedToClearEncounterMap,
            RandomEncounterPoint = Point.Round(_randomEncounterPoint),
            RandomEncounterStatus = _randomEncounterStatus,
            Locations = locations
        };
    }

    [TempleDllLocation(0x1015e0f0)]
    public void LoadGame(SavedUiState savedState)
    {
        var worldMapState = savedState.WorldmapState;

        _randomEncounterStatus = worldMapState.RandomEncounterStatus;
        _randomEncounterPoint = worldMapState.RandomEncounterPoint;
        NeedToClearEncounterMap = worldMapState.NeedToCleanEncounterMap;

        UiSystems.RandomEncounter.DontAskToExitMap = worldMapState.DontAskToExitEncounterMap;

        foreach (var location in worldMapState.Locations)
        {
            if (location.Visited)
            {
                _locationWidgets[location.Index].State = WorldMapLocationState.Visited;
            }
            else if (location.Discovered)
            {
                _locationWidgets[location.Index].State = WorldMapLocationState.Discovered;
            }
        }
    }

    [TempleDllLocation(0x1015e940)]
    private void CreateRandomEncounter(PointF screenPos, RandomEncounter re)
    {
        _randomEncounterPoint = screenPos;
        NeedToClearEncounterMap = true;
        _randomEncounterStatus = 2;
        Logger.Info("Worldmap travel caused random encounter {0} (Map {1})", re.Id, re.Map);
        Logger.Info(" screen_x = {0}, screen_y = {1}", screenPos.X, screenPos.Y);
        Hide();
        GameSystems.RandomEncounter.CreateEncounter(re);
    }

    [TempleDllLocation(0x1015e840)]
    private void TeleportToDestination()
    {
        _randomEncounterDetails = null;
        NeedToClearEncounterMap = false;

        locXY startPos;
        if (_teleportMapId == 5001)
        {
            startPos = new locXY(623, 421);
            _teleportMapId = 5001;
        }
        else if (_teleportMapId == 15001)
        {
            startPos = new locXY(512, 221);
            _teleportMapId = 5001;
        }
        else if (_teleportMapId == 25001)
        {
            startPos = new locXY(433, 626);
            _teleportMapId = 5001;
        }
        else
        {
            startPos = GameSystems.Map.GetStartPos(_teleportMapId);
        }

        var teleportArgs = FadeAndTeleportArgs.Default;
        teleportArgs.flags = FadeAndTeleportFlags.CenterOnPartyLeader;
        teleportArgs.somehandle = GameSystems.Party.GetLeader();
        teleportArgs.destLoc = startPos;

        if (_teleportMapId != GameSystems.Map.GetCurrentMapId())
        {
            var enterMovie = GameSystems.Map.GetEnterMovie(_teleportMapId, false);
            if (enterMovie != 0)
            {
                teleportArgs.flags |= FadeAndTeleportFlags.play_movie;
                teleportArgs.movieId = enterMovie;
            }

            teleportArgs.destMap = _teleportMapId;
        }
        else
        {
            teleportArgs.destMap = -1;
        }

        Hide();
        GameSystems.Teleport.FadeAndTeleport(in teleportArgs);
    }

    // TODO ToEE sure does love it's hardcoded tables
    [TempleDllLocation(0x1012b960)]
    public bool IsWorldMapAccessible
    {
        get
        {
            var currentMapId = GameSystems.Map.GetCurrentMapId();

            // TODO: 5070-5078 are random encounter maps and this should be marked differently
            if (currentMapId >= 5070 || currentMapId <= 5078)
            {
                return true;
            }

            switch (currentMapId)
            {
                case 5001:
                case 5002:
                case 5051:
                case 5062:
                case 5068:
                case 5069:
                case 5091:
                case 5093:
                case 5094:
                case 5095:
                case 5108:
                case 5112:
                case 5113:
                case 5121:
                case 5132:
                    return true;
                default:
                    return false;
            }
        }
    }

    [TempleDllLocation(0x10160450)]
    public void TravelToArea(int areaId)
    {
        if (!TryGetWidgetsForArea(areaId, out var widgets))
        {
            Logger.Warn($"Failed to travel to area {areaId} because no locations exist for it.");
            return;
        }

        _travelByDialogDestination = widgets.Location;
        Show(WorldMapMode.TravelFromDialog);
    }
}
