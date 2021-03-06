using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Styles
{
    /// <summary>
    /// Serves as a registry for Widget text styles.
    /// </summary>
    public sealed class WidgetTextStyles
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

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

            _defaultStyle = new TextStyle
            {
                fontFace = "arial-10",
                pointSize = 10
            };
        }

        public void AddStyle(string id, TextStyle textStyle)
        {
            if (!_textStyles.TryAdd(id, textStyle))
            {
                throw new Exception($"Duplicate text style defined: {id}");
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
                    Logger.Warn("Skipping text style that is not an object.");
                    continue;
                }

                var idNode = item.GetProperty("id");
                if (idNode.ValueKind != JsonValueKind.String)
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

                style.id = id;

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
                    style.foreground = foregroundNode.GetBrush();
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
                    style.dropShadowBrush = dropShadowBrushNode.GetBrush();
                }

                style.legacyLeading = item.GetInt32Prop("legacyLeading", style.legacyLeading);
                style.legacyKerning = item.GetInt32Prop("legacyKerning", style.legacyKerning);
                style.legacyTracking = item.GetInt32Prop("legacyTracking", style.legacyTracking);
                if (item.TryGetProperty("legacyExtraColors", out var extraColorsNode))
                {
                    var extraColors = new List<Brush>();
                    foreach (var extraColorNode in extraColorsNode.EnumerateArray())
                    {
                        extraColors.Add(extraColorNode.GetBrush());
                    }
                    style.legacyExtraColors = extraColors.ToArray();
                }

                AddStyle(id, style);
            }
        }

        private TextStyle _defaultStyle = new TextStyle();
        private readonly Dictionary<string, TextStyle> _textStyles = new Dictionary<string, TextStyle>();
    };
}