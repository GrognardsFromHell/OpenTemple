using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.Styles
{
    /// <summary>
    /// Serves as a registry for Widget text styles.
    /// </summary>
    public sealed class WidgetTextStyles
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const string DefaultFile = "templeplus/text_styles.json";

        public WidgetTextStyles()
        {
            if (Tig.FS.FileExists(DefaultFile))
            {
                LoadStylesFile(DefaultFile);
            }
            else
            {
                Logger.Warn("Failed to load widget text styles file '{0}'", DefaultFile);
            }
        }

        public void AddStyle(string id, TextStyle textStyle)
        {
            if (!_textStyles.TryAdd(id, textStyle))
            {
                throw new Exception($"Duplicate text style defined: {id}");
            }

            if (_textStyles.Count == 1)
            {
                // The very first style is used as the default style
                _defaultStyle = textStyle;
            }
        }

        public TextStyle GetDefaultStyle()
        {
            return _defaultStyle;
        }

        public void SetDefaultStyle(TextStyle textStyle)
        {
            Trace.Assert(textStyle != null);
            _defaultStyle = textStyle;
        }

        public TextStyle GetTextStyle(string id)
        {
            if (_textStyles.TryGetValue(id, out var style))
            {
                return style;
            }

            return _defaultStyle;
        }

        public bool HasStyle(string id)
        {
            return _textStyles.ContainsKey(id);
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
            if (root.Type != JsonValueType.Array)
            {
                throw new Exception("Text style files must start with an array at the root");
            }

            LoadStyles(root);
        }

        public void LoadStyles(JsonElement jsonStyleArray)
        {
            foreach (var item in jsonStyleArray.EnumerateArray())
            {
                if (item.Type != JsonValueType.Object)
                {
                    Logger.Warn("Skipping text style that is not an object.");
                    continue;
                }

                var idNode = item.GetProperty("id");
                if (idNode.Type != JsonValueType.String)
                {
                    Logger.Warn("Skipping text style that is missing 'id' attribute.");
                    continue;
                }

                var id = idNode.GetString();

                TextStyle style;

                // Process the inherit attribute (what is the base style)
                if (item.TryGetProperty("inherit", out var inheritNode))
                {
                    var inheritId = inheritNode.GetString();
                    if (!HasStyle(inheritId))
                    {
                        Logger.Warn("Style {0} inherits from unknown style {1}", id, inheritId);
                    }

                    style = GetTextStyle(inheritId).Copy();
                }
                else
                {
                    style = _defaultStyle.Copy();
                }

                // Every other attribute from here on out is optional
                if (item.TryGetProperty("fontFamily", out var fontFamilyNode))
                {
                    style.fontFace = fontFamilyNode.GetString();
                }

                if (item.TryGetProperty("pointSize", out var pointSizeNode))
                {
                    style.pointSize = pointSizeNode.GetSingle();
                }

                if (item.TryGetProperty("bold", out var boldNode))
                {
                    style.bold = boldNode.GetBoolean();
                }

                if (item.TryGetProperty("italic", out var italicNode))
                {
                    style.italic = italicNode.GetBoolean();
                }

                if (item.TryGetProperty("align", out var alignNode))
                {
                    var align = alignNode.GetString();
                    if (align == "left")
                    {
                        style.align = TextAlign.Left;
                    }
                    else if (align == "center")
                    {
                        style.align = TextAlign.Center;
                    }
                    else if (align == "right")
                    {
                        style.align = TextAlign.Right;
                    }
                    else if (align == "justified")
                    {
                        style.align = TextAlign.Justified;
                    }
                    else
                    {
                        Logger.Warn("Invalid text alignment: '{0}'", align);
                    }
                }

                if (item.TryGetProperty("paragraphAlign", out var paragraphAlign))
                {
                    var align = paragraphAlign.GetString();
                    switch (align)
                    {
                        case "near":
                            style.paragraphAlign = ParagraphAlign.Near;
                            break;
                        case "far":
                            style.paragraphAlign = ParagraphAlign.Far;
                            break;
                        case "center":
                            style.paragraphAlign = ParagraphAlign.Center;
                            break;
                        default:
                            Logger.Warn("Invalid paragraph alignment: '{0}'", align);
                            break;
                    }
                }

                if (item.TryGetProperty("foreground", out var foregroundNode))
                {
                    style.foreground = ParseBrush(foregroundNode);
                }

                if (item.TryGetProperty("uniformLineHeight", out var uniformLineHeightNode))
                {
                    style.uniformLineHeight = uniformLineHeightNode.GetBoolean();
                }

                if (item.TryGetProperty("lineHeight", out var lineHeightNode))
                {
                    style.lineHeight = lineHeightNode.GetSingle();
                }

                if (item.TryGetProperty("baseLine", out var baseLineNode))
                {
                    style.baseLine = baseLineNode.GetSingle();
                }

                if (item.TryGetProperty("dropShadow", out var dropShadowNode))
                {
                    style.dropShadow = dropShadowNode.GetBoolean();
                }

                if (item.TryGetProperty("dropShadowBrush", out var dropShadowBrushNode))
                {
                    style.dropShadowBrush = ParseBrush(dropShadowBrushNode);
                }

                AddStyle(id, style);
            }
        }

        private static PackedLinearColorA ParseColor(string def)
        {
            PackedLinearColorA color = PackedLinearColorA.White;

            if (def.Length != 7 && def.Length != 9)
            {
                Logger.Warn("Color definition '{0}' has to be #RRGGBB or #RRGGBBAA.", def);
                return color;
            }

            if (!def.StartsWith("#"))
            {
                Logger.Warn("Color definition '{0}' has to start with # sign.", def);
                return color;
            }

            color.R = ParseHexColor(def.Substring(1, 2));
            color.G = ParseHexColor(def.Substring(3, 2));
            color.B = ParseHexColor(def.Substring(5, 2));
            if (def.Length >= 9)
            {
                color.A = ParseHexColor(def.Substring(7, 2));
            }

            return color;
        }

        private static byte ParseHexColor(ReadOnlySpan<char> stringPart)
        {
            return (byte) int.Parse(stringPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private static Brush ParseBrush(JsonElement jsonVal)
        {
            if (jsonVal.Type == JsonValueType.Array)
            {
                if (jsonVal.GetArrayLength() != 2)
                {
                    throw new Exception($"Brush specification {jsonVal} has to have 2 elements for a gradient!");
                }

                Brush gradient;
                gradient.gradient = true;
                gradient.primaryColor = ParseColor(jsonVal[0].GetString());
                gradient.secondaryColor = ParseColor(jsonVal[1].GetString());
                return gradient;
            }

            Brush brush;
            brush.gradient = false;
            brush.primaryColor = ParseColor(jsonVal.GetString());
            brush.secondaryColor = brush.primaryColor;
            return brush;
        }

        private TextStyle _defaultStyle = new TextStyle();
        private readonly Dictionary<string, TextStyle> _textStyles = new Dictionary<string, TextStyle>();
    };
}