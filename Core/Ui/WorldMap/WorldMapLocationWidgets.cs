using System;
using System.Collections.Generic;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.WorldMap;

internal class WorldMapLocationWidgets
{
    public WorldMapLocation Location { get; }

    public LocationRingWidget Ring { get; }

    public WidgetButton ListButton { get; }

    private readonly List<(WorldMapImage, WidgetButtonBase)> _images;

    public event Action<WorldMapLocation> OnClick;

    private WorldMapLocationState _state = WorldMapLocationState.Undiscovered;

    [TempleDllLocation(0x11ea4900)]
    public int gameareasSthg;

    public int gameareasSthg2;

    public WorldMapLocationState State
    {
        get => _state;
        set
        {
            _state = value;
            UpdateVisibility();
        }
    }

    public WorldMapLocationWidgets(WorldMapLocation location, WidgetContainer parent)
    {
        Location = location;

        _images = new List<(WorldMapImage, WidgetButtonBase)>(location.Images.Count);
        foreach (var image in location.Images)
        {
            var rect = image.Rectangle;
            var button = new WidgetButtonBase(rect);
            button.AddContent(new WidgetImage(image.Path));
            button.SetMouseMsgHandler(_ => false);

            parent.Add(button);
            _images.Add((image, button));
        }

        Ring = new LocationRingWidget(location.Position, location.Radius)
        {
            TooltipText = location.Name
        };
        Ring.SetClickHandler(() => OnClick?.Invoke(Location));
        parent.Add(Ring);

        ListButton = new WidgetButton();
        ListButton.SetStyle("mapLocationListButton"); // Defined in worldmap_ui.json
        ListButton.Text = location.Name;
        ListButton.SetClickHandler(() => OnClick?.Invoke(Location));
        ListButton.OnMouseEnter += _ => Ring.ForceVisible = true;
        ListButton.OnMouseExit += _ => Ring.ForceVisible = false;

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (_state >= WorldMapLocationState.Discovered)
        {
            gameareasSthg = 2;
        }

        if (_state >= WorldMapLocationState.Visited)
        {
            gameareasSthg2 = 2;
        }

        foreach (var (image, button) in _images)
        {
            button.Visible = _state switch
            {
                WorldMapLocationState.Undiscovered => false,
                WorldMapLocationState.Discovered => !image.ShowOnlyWhenVisited,
                WorldMapLocationState.Visited => true,
                _ => button.Visible
            };
        }

        Ring.Visible = _state >= WorldMapLocationState.Discovered;
    }

    public void Reset()
    {
        Ring.Reset();
    }
}