using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Serialization;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.Ui.Cursors;

/// <summary>
/// Manages mouse cursor images and their hotspots.
/// </summary>
public class CursorRegistry
{
    private const string CursorFolder = "art/interface/cursors/";
    private const string CursorSuffix = ".cursor.json";
    private const string CursorSearch = CursorFolder + "*" + CursorSuffix;

    private readonly Dictionary<string, CursorDefinition> _cursors = new();

    public CursorRegistry(IFileSystem fs)
    {
        // Enumerate all cursors
        foreach (var path in fs.Search(CursorSearch))
        {
            var cursorId = path[CursorFolder.Length..^CursorSuffix.Length];
            // TODO: Validate cursor id!

            var definition = fs.ReadJsonFile(path, CursorDefinitionContext.Default.CursorDefinition);
            if (definition.TexturePath == null)
            {
                throw new InvalidDataException($"Cursor definition {path} is missing texturePath");
            }

            _cursors[cursorId] = definition;
        }
    }

    public bool TryGetValue(string id, [MaybeNullWhen(false)] out CursorDefinition definition)
    {
        return _cursors.TryGetValue(id, out definition);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CursorDefinition))]
internal partial class CursorDefinitionContext : JsonSerializerContext
{
}

public class CursorDefinition
{
    [JsonPropertyName("hotspotX")]
    public int HotspotX { get; set; }
    
    [JsonPropertyName("hotspotY")]
    public int HotspotY { get; set; }

    [JsonPropertyName("texturePath")]
    public string TexturePath { get; set; }
}
