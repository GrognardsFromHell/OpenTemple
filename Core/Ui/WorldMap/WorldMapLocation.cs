using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Text.Json;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.WorldMap;

public class WorldMapLocation
{
    public string Name { get; }

    public IImmutableSet<int> AreaIds { get; }

    public int TeleportMapId { get; }

    public Point Position { get; }

    public int Radius { get; }

    public IImmutableList<WorldMapImage> Images { get; }

    public int UsePathsOf { get; }

    public IImmutableSet<int> OutgoingPaths { get; }

    public IImmutableSet<int> IncomingPaths { get; }

    public WorldMapLocationState InitialState { get; }

    public WorldMapLocation(string name,
        IImmutableSet<int> areaIds,
        int teleportMapId,
        Point position,
        int radius,
        IImmutableList<WorldMapImage> images,
        int usePathsOf,
        IImmutableSet<int> outgoingPaths,
        IImmutableSet<int> incomingPaths,
        WorldMapLocationState initialState)
    {
        Name = name;
        AreaIds = areaIds;
        TeleportMapId = teleportMapId;
        Position = position;
        Radius = radius;
        Images = images;
        UsePathsOf = usePathsOf;
        OutgoingPaths = outgoingPaths;
        IncomingPaths = incomingPaths;
        InitialState = initialState;
    }

    private const string JsonInitialStateUndiscovered = "undiscovered";
    private const string JsonInitialStateDiscovered = "discovered";
    private const string JsonInitialStateVisited = "visited";

    public static WorldMapLocation LoadFromJson(JsonElement element)
    {
        var name = element.GetStringProp("name");
        var x = element.GetInt32Prop("x");
        var y = element.GetInt32Prop("y");
        var radius = element.GetInt32Prop("radius");
        var teleportMapId = element.GetInt32Prop("teleportMapId");
        var initialStateJson = element.GetStringProp("initialState", JsonInitialStateUndiscovered);
        var initialState = initialStateJson switch
        {
            JsonInitialStateUndiscovered => WorldMapLocationState.Undiscovered,
            JsonInitialStateDiscovered => WorldMapLocationState.Discovered,
            JsonInitialStateVisited => WorldMapLocationState.Visited,
            _ => throw new InvalidOperationException($"Invalid initial state: {initialStateJson}")
        };

        var areaIds = LoadAreaIds(element);

        var images = ImmutableList<WorldMapImage>.Empty;
        if (element.TryGetProperty("images", out var imagesElement))
        {
            try
            {
                images = LoadImagesFromJson(imagesElement);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to parse images: {e}");
            }
        }

        LoadPathConnections(element, out var incomingPaths, out var outgoingPaths, out var usePathsOf);

        return new WorldMapLocation(
            name,
            areaIds,
            teleportMapId,
            new Point(x, y),
            radius,
            images.ToImmutableList(),
            usePathsOf,
            outgoingPaths,
            incomingPaths,
            initialState
        );
    }

    private static void LoadPathConnections(JsonElement element, out IImmutableSet<int> incomingPaths,
        out IImmutableSet<int> outgoingPaths, out int usePathsOf)
    {
        incomingPaths = ImmutableSortedSet<int>.Empty;
        outgoingPaths = ImmutableSortedSet<int>.Empty;
        usePathsOf = element.GetInt32Prop("usePathsOf", -1);
        if (usePathsOf != -1)
        {
            if (element.TryGetProperty("incomingPaths", out _) || element.TryGetProperty("outgoingPaths", out _))
            {
                throw new InvalidOperationException(
                    "If usePathsOf is used, incoming and outgoing Paths are not allowed.");
            }

            return;
        }

        if (element.TryGetProperty("incomingPaths", out var incomingPathsElement))
        {
            incomingPaths = LoadPathIdList(incomingPathsElement);
        }

        if (element.TryGetProperty("outgoingPaths", out var outgoingPathsElement))
        {
            outgoingPaths = LoadPathIdList(outgoingPathsElement);
        }
    }

    private static IImmutableSet<int> LoadPathIdList(JsonElement listElement)
    {
        if (listElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Expected path reference list to be an array.");
        }

        var idList = new List<int>(listElement.GetArrayLength());
        for (var i = 0; i < listElement.GetArrayLength(); i++)
        {
            if (listElement[i].ValueKind != JsonValueKind.Number)
            {
                throw new InvalidOperationException("Expected path id list element to be a number.");
            }

            idList.Add(listElement[i].GetInt32());
        }

        return idList.ToImmutableSortedSet();
    }

    private static ImmutableSortedSet<int> LoadAreaIds(JsonElement element)
    {
        if (element.TryGetProperty("areaIds", out var areasElement))
        {
            if (areasElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Expected areaIds to be an array.");
            }

            var areaIds = new SortedSet<int>();
            for (var i = 0; i < areasElement.GetArrayLength(); i++)
            {
                if (areasElement[i].ValueKind != JsonValueKind.Number)
                {
                    throw new InvalidOperationException("Expected area ids to be numeric.");
                }

                areaIds.Add(areasElement[i].GetInt32());
            }

            return areaIds.ToImmutableSortedSet();
        }
        else
        {
            return ImmutableSortedSet<int>.Empty;
        }
    }

    private static ImmutableList<WorldMapImage> LoadImagesFromJson(in JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Image list is not an array.");
        }

        var result = new List<WorldMapImage>(jsonElement.GetArrayLength());

        for (var i = 0; i < jsonElement.GetArrayLength(); i++)
        {
            var imageJson = jsonElement[i];
            if (imageJson.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Image is not an object.");
            }

            result.Add(new WorldMapImage(
                new Rectangle(
                    imageJson.GetInt32Prop("x"),
                    imageJson.GetInt32Prop("y"),
                    imageJson.GetInt32Prop("width"),
                    imageJson.GetInt32Prop("height")
                ),
                imageJson.GetStringProp("path"),
                imageJson.GetBoolProp("showOnlyWhenVisited", false)
            ));
        }

        return result.ToImmutableList();
    }
}

public class WorldMapImage
{
    public Rectangle Rectangle { get; }

    public string Path { get; }

    public bool ShowOnlyWhenVisited { get; }

    public WorldMapImage(Rectangle rectangle, string path, bool showOnlyWhenVisited)
    {
        Rectangle = rectangle;
        Path = path;
        ShowOnlyWhenVisited = showOnlyWhenVisited;
    }
}