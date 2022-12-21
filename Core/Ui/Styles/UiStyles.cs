
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

    private readonly Dictionary<string, List<(StylingState, IStyleDefinition)>> _pseudoClassRules = new();

    public StyleResolver StyleResolver { get; private set; }

    public UiStyles()
    {
        Reload();
    }

    private void AddStyle(string id, IStyleDefinition style)
    {
        // Consider pseudo-class selectors
        var pseudoClasses = GetPseudoClasses(ref id);
        if (pseudoClasses != default)
        {
            if (_pseudoClassRules.TryGetValue(id, out var rules))
            {
                rules.Add((pseudoClasses, style));
            }
            else
            {
                _pseudoClassRules[id] = new List<(StylingState, IStyleDefinition)>
                {
                    (pseudoClasses, style)
                };
            }

            return;
        }

        if (!_styles.TryAdd(id, style))
        {
            throw new Exception($"Duplicate style defined: {id}");
        }
    }

    private static StylingState GetPseudoClasses(ref string id)
    {
        var idx = id.IndexOf(':', StringComparison.Ordinal);
        if (idx == -1)
        {
            return default;
        }

        StylingState result = default;
        foreach (var selector in id[(idx + 1)..].Split(':'))
        {
            switch (selector)
            {
                case "hover":
                    result |= StylingState.Hover;
                    break;
                case "pressed":
                    result |= StylingState.Pressed;
                    break;
                case "disabled":
                    result |= StylingState.Disabled;
                    break;
                default:
                    throw new StyleParsingException($"Style id ${id} has invalid selector ${selector}");
            }
        }

        id = id[..idx];
        return result;
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

    public IEnumerable<IStyleDefinition> GetPseudoClassRules(string id, StylingState pseudoClasses)
    {
        if (_pseudoClassRules.TryGetValue(id, out var rules))
        {
            return rules.Where(r => (pseudoClasses & r.Item1) == r.Item1).Select(r => r.Item2);
        }

        return Enumerable.Empty<IStyleDefinition>();
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
            throw new Exception("Text style files must start with an object at the root which has a 'styles' property.");
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

    public void Clear()
    {
        _styles.Clear();
        _pseudoClassRules.Clear();
        Reload();
    }

    private void Reload()
    {
        LoadStylesFile(DefaultFile);

        if (!_styles.TryGetValue("default", out var defaultStyle))
        {
            throw new Exception("Missing required UI style 'default'");
        }

        StyleResolver = new StyleResolver(defaultStyle);
    }
}