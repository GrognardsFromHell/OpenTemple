using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenTemple.Core.IO.MapMarkers;

public class MapMarkersReader
{
    public static PredefinedMapMarkers Load(IFileSystem fs, string path)
    {
        using var memory = fs.ReadFile(path);
        var markers = JsonSerializer.Deserialize<PredefinedMapMarkers>(memory.Memory.Span);
        if (markers == null)
        {
            throw new InvalidDataException($"Content of file {path} was a literal 'null' value. Expected: map markers");
        }
        return markers;
    }
}

public class PredefinedMapMarkers
{
    [JsonPropertyName("markers")]
    public List<PredefinedMapMarker> Markers { get; set; }
}

public class PredefinedMapMarker
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("initiallyVisible")]
    public bool InitiallyVisible { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}