using System;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Ui.Styles
{
    public static class JsonColorLoader
    {
        private static readonly Regex RgbaRegex =
            new Regex(@"rgba\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,(\d{1,3})\s*,\s*(\d{1,3})\s*\)");

        private static readonly Regex RgbRegex = new Regex(@"rgb\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*\)");

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static PackedLinearColorA GetColor(this JsonElement element) => ParseColor(element.GetString());

        private static PackedLinearColorA ParseColor(string colorSpec)
        {
            var m = RgbaRegex.Match(colorSpec);
            if (m.Success)
            {
                return new PackedLinearColorA(
                    byte.Parse(m.Groups[1].Value),
                    byte.Parse(m.Groups[2].Value),
                    byte.Parse(m.Groups[3].Value),
                    byte.Parse(m.Groups[4].Value)
                );
            }

            m = RgbRegex.Match(colorSpec);
            if (m.Success)
            {
                return new PackedLinearColorA(
                    byte.Parse(m.Groups[1].Value),
                    byte.Parse(m.Groups[2].Value),
                    byte.Parse(m.Groups[3].Value),
                    255
                );
            }

            var color = PackedLinearColorA.White;

            if (colorSpec.Length != 7 && colorSpec.Length != 9)
            {
                Logger.Warn("Color definition '{0}' has to be #RRGGBB or #RRGGBBAA.", colorSpec);
                return color;
            }

            if (!colorSpec.StartsWith("#"))
            {
                Logger.Warn("Color definition '{0}' has to start with # sign.", colorSpec);
                return color;
            }

            color.R = ParseHexColor(colorSpec.Substring(1, 2));
            color.G = ParseHexColor(colorSpec.Substring(3, 2));
            color.B = ParseHexColor(colorSpec.Substring(5, 2));
            if (colorSpec.Length >= 9)
            {
                color.A = ParseHexColor(colorSpec.Substring(7, 2));
            }

            return color;
        }

        private static byte ParseHexColor(ReadOnlySpan<char> stringPart)
        {
            return (byte) int.Parse(stringPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        public static Brush GetBrush(this JsonElement jsonVal)
        {
            if (jsonVal.ValueKind == JsonValueKind.Array)
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
    }
}