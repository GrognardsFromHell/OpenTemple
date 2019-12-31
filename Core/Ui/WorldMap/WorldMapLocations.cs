using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.WorldMap
{
    public class WorldMapLocations
    {
        public IImmutableList<WorldMapLocation> Locations { get; }

        public WorldMapLocations(IImmutableList<WorldMapLocation> locations)
        {
            Locations = locations;
        }

        public static WorldMapLocations LoadFromJson(string path)
        {
            using var data = Tig.FS.ReadFile(path);

            var jsonDoc = JsonDocument.Parse(data.Memory);

            return LoadFromJson(jsonDoc.RootElement);
        }

        private static WorldMapLocations LoadFromJson(in JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("Expected World Map Locations root element to be an object.");
            }

            var locations = ImmutableList<WorldMapLocation>.Empty;
            if (element.TryGetProperty("locations", out var locationsElement))
            {
                if (locationsElement.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException("Expected locations property to be an array.");
                }

                var locationsList = new List<WorldMapLocation>(locationsElement.GetArrayLength());
                for (var i = 0; i < locationsElement.GetArrayLength(); i++)
                {
                    locationsList.Add(WorldMapLocation.LoadFromJson(locationsElement[i]));
                }

                locations = locationsList.ToImmutableList();
            }

            return new WorldMapLocations(locations);
        }
    }
}