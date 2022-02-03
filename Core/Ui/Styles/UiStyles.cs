#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Styles;

/// <summary>
/// Serves as a registry for Widget text styles.
/// </summary>
public sealed class UiStyles
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    /// <summary>
    /// An empty style definition that is used when an undefined style id is queried.
    /// </summary>
    private static readonly IStyleDefinition FallbackStyle = new StyleDefinition();

    private const string DefaultFile = "ui/styles.json";

    private readonly Dictionary<string, IStyleDefinition> _styles = new();

    public StyleResolver StyleResolver { get; }

    public UiStyles()
    {
        LoadStylesFile(DefaultFile);

        if (!_styles.TryGetValue("default", out var defaultStyle))
        {
            throw new Exception("Missing required UI style 'default'");
        }

        StyleResolver = new StyleResolver(defaultStyle);
    }

    public void AddStyle(string id, IStyleDefinition style)
    {
        if (!_styles.TryAdd(id, style))
        {
            throw new Exception($"Duplicate style defined: {id}");
        }
    }

    public ComputedStyles GetComputed(params string[] ids)
    {
        return StyleResolver.Resolve(ids.Select(Get).ToImmutableArray());
    }

    public IStyleDefinition Get(string id)
    {
        if (_styles.TryGetValue(id, out var style))
        {
            return style;
        }

        Logger.Warn("Missing style: {0}", id);
        return _styles[id] = FallbackStyle;
    }

    public void LoadStylesFile(string path)
    {
        var jsonContent = Tig.FS.ReadBinaryFile(path);

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(jsonContent);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to parse text styles from {path}.", e);
        }

        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new Exception("Text style files must start with an object at the root");
        }

        if (!root.TryGetProperty("styles", out var stylesEl))
        {
            throw new Exception("Text style files must start with an object at the root wich has a 'styles' property.");
        }

        LoadStyles(stylesEl);
    }

    public void LoadStyles(JsonElement jsonStyleArray)
    {
        foreach (var item in jsonStyleArray.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                Logger.Warn("Skipping style that is not an object.");
                continue;
            }

            var idNode = item.GetProperty("id");
            var id = idNode.GetString();
            if (id == null)
            {
                Logger.Warn("Skipping style that is missing 'id' attribute.");
                continue;
            }

            var style = new StyleDefinition();

            // Process the inherit attribute (what is the base style)
            if (item.TryGetProperty("inherit", out var inheritNode))
            {
                var inheritId = inheritNode.GetString();
                if (inheritId == null || !_styles.ContainsKey(inheritId))
                {
                    Logger.Warn("Style {0} inherits from unknown style {1}", id, inheritId);
                }
                else
                {
                    Get(inheritId).MergeInto(style);
                }
            }

            // Deserialize additional shorthand properties not handled by the auto-generated code
            DeserializeShorthandProperties(item, style);

            StyleJsonDeserializer.DeserializeProperties(item, style);

            AddStyle(id, style);
        }
    }

    private void DeserializeShorthandProperties(JsonElement item, StyleDefinition style)
    {
        JsonElement propertyNode;
        if (item.TryGetProperty("padding", out propertyNode))
        {
            if (propertyNode.ValueKind != JsonValueKind.Number)
            {
                throw new StyleParsingException("Expected padding to be a number");
            }

            var padding = propertyNode.GetSingle();
            style.PaddingTop = padding;
            style.PaddingRight = padding;
            style.PaddingBottom = padding;
            style.PaddingLeft = padding;
        }

        if (item.TryGetProperty("margin", out propertyNode))
        {
            if (propertyNode.ValueKind != JsonValueKind.Number)
            {
                throw new StyleParsingException("Expected margin to be a number");
            }

            var margin = propertyNode.GetSingle();
            style.MarginTop = margin;
            style.MarginRight = margin;
            style.MarginBottom = margin;
            style.MarginLeft = margin;
        }
    }
};