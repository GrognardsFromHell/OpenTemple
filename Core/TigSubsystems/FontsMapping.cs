using System;
using System.Collections.Generic;
using System.Text.Json;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.TigSubsystems
{
    public class FontsMapping
    {
        private readonly Dictionary<string, TextStyle> _mappings = new Dictionary<string, TextStyle>();

        private static readonly ILogger Logger = new ConsoleLogger();

        private const string DefaultFile = "fonts/mapping.json";

        public FontsMapping()
        {
            if (!Tig.FS.FileExists(DefaultFile))
            {
                Logger.Warn("Failed to load font mapping file {0}", DefaultFile);
                return;
            }

            var mappingData = Tig.FS.ReadBinaryFile("fonts/mapping.json");

            var json = JsonDocument.Parse(mappingData);
            var root = json.RootElement;

            if (root.Type != JsonValueType.Array)
            {
                throw new Exception("Expected an array on the top-level of fonts/mapping.json");
            }

            foreach (var record in root.EnumerateArray())
            {
                var id = record.GetProperty("id").GetString();

                var style = new TextStyle();
                style.fontFace = record.GetProperty("fontFace").GetString();
                style.pointSize = record.GetProperty("size").GetSingle();
                style.bold = record.GetProperty("bold").GetBoolean();
                style.italic = record.GetProperty("italic").GetBoolean();
                if (record.TryGetProperty("uniformLineHeight", out var uniformLineHeight))
                {
                    style.uniformLineHeight = true;
                    style.lineHeight = uniformLineHeight.GetProperty("lineHeight").GetSingle();
                    style.baseLine = uniformLineHeight.GetProperty("baseline").GetSingle();
                }

                _mappings[id] = style;
            }
        }

        public bool TryGetMapping(string fontId, out TextStyle textStyle) =>
            _mappings.TryGetValue(fontId, out textStyle);
    }
}