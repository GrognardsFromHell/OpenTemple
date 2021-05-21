using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Styles
{
/**
* Serves as a registry for Widget button styles.
*/
    public sealed class WidgetButtonStyles
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private const string DefaultFile = "templeplus/button_styles.json";

        public WidgetButtonStyles()
        {
            if (Tig.FS.FileExists(DefaultFile))
            {
                LoadStylesFile(DefaultFile);
            }
            else
            {
                Logger.Warn("Failed to load widget button styles file '{0}'", DefaultFile);
            }
        }

        public void AddStyle(string id, WidgetButtonStyle textStyle)
        {
            if (!_styles.TryAdd(id, textStyle))
            {
                throw new Exception($"Duplicate button style defined: {id}");
            }
        }

        public WidgetButtonStyle GetStyle(string id)
        {
            if (_styles.TryGetValue(id, out var style))
            {
                return style;
            }

            Logger.Warn("Undefined widget button style: {0}", id);
            return new WidgetButtonStyle();
        }

        public bool HasStyle(string id)
        {
            return _styles.ContainsKey(id);
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
                throw new Exception($"Unable to parse button styles from {path}.", e);
            }

            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("Button style files must start with an array at the root");
            }

            LoadStyles(root);
        }

        public void LoadStyles(JsonElement jsonStyleArray)
        {
            foreach (var style in jsonStyleArray.EnumerateArray())
            {
                string id = style.GetProperty("id").GetString();
                if (id.Length == 0)
                {
                    throw new Exception("Found button style without id!");
                }

                // Process the inherit attribute (what is the base style)
                WidgetButtonStyle buttonStyle;
                if (style.TryGetProperty("inherit", out var inheritNode))
                {
                    var inheritId = inheritNode.GetString();
                    if (!HasStyle(inheritId))
                    {
                        Logger.Warn("Style {0} inherits from unknown style {1}", id, inheritId);
                    }

                    buttonStyle = GetStyle(inheritId).Copy();
                    buttonStyle.inherits = inheritId;
                }
                else
                {
                    buttonStyle = new WidgetButtonStyle();
                }

                buttonStyle.id = id;

                if (style.TryGetProperty("textStyle", out var textStyleNode))
                {
                    buttonStyle.textStyleId = textStyleNode.GetString();
                }

                if (style.TryGetProperty("hoverTextStyle", out var hoverTextStyleNode))
                {
                    buttonStyle.hoverTextStyleId = hoverTextStyleNode.GetString();
                }

                if (style.TryGetProperty("pressedTextStyle", out var pressedTextStyleNode))
                {
                    buttonStyle.pressedTextStyleId = pressedTextStyleNode.GetString();
                }

                if (style.TryGetProperty("disabledTextStyle", out var disabledTextStyleNode))
                {
                    buttonStyle.disabledTextStyleId = disabledTextStyleNode.GetString();
                }

                if (style.TryGetProperty("disabledImage", out var disabledImageNode))
                {
                    buttonStyle.disabledImagePath = disabledImageNode.GetString();
                }

                if (style.TryGetProperty("normalImage", out var normalImageNode))
                {
                    buttonStyle.normalImagePath = normalImageNode.GetString();
                }

                if (style.TryGetProperty("hoverImage", out var hoverImageNode))
                {
                    buttonStyle.hoverImagePath = hoverImageNode.GetString();
                }

                if (style.TryGetProperty("pressedImage", out var pressedImageNode))
                {
                    buttonStyle.pressedImagePath = pressedImageNode.GetString();
                }

                if (style.TryGetProperty("frameImage", out var frameImageNode))
                {
                    buttonStyle.frameImagePath = frameImageNode.GetString();
                }

                if (style.TryGetProperty("activatedImage", out var activatedImageNode))
                {
                    buttonStyle.activatedImagePath = activatedImageNode.GetString();
                }

                if (style.TryGetProperty("defaultSounds", out var defaultSoundsNode))
                {
                    if (defaultSoundsNode.IsBool() && defaultSoundsNode.GetBoolean())
                    {
                        buttonStyle.UseDefaultSounds();
                    }
                }

                if (style.TryGetProperty("soundEnter", out var soundEnterNode))
                {
                    buttonStyle.soundEnter = soundEnterNode.GetInt32();
                }

                if (style.TryGetProperty("soundLeave", out var soundLeaveNode))
                {
                    buttonStyle.soundLeave = soundLeaveNode.GetInt32();
                }

                if (style.TryGetProperty("soundDown", out var soundDownNode))
                {
                    buttonStyle.soundDown = soundDownNode.GetInt32();
                }

                if (style.TryGetProperty("soundClick", out var soundClickNode))
                {
                    buttonStyle.soundClick = soundClickNode.GetInt32();
                }

                if (_styles.ContainsKey(id))
                {
                    throw new Exception($"Duplicate button style: {id}");
                }

                _styles[id] = buttonStyle;
            }
        }

        private readonly Dictionary<string, WidgetButtonStyle> _styles = new Dictionary<string, WidgetButtonStyle>();
    }
}