using System;
using System.Text.Json;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Ui.Styles
{
    internal static class JsonElementExtensions
    {
        public static bool IsBool(this JsonElement element)
        {
            return element.Type == JsonValueType.True || element.Type == JsonValueType.False;
        }

        public static bool GetBoolProp(this JsonElement element, ReadOnlySpan<char> propertyName, bool defaultVal)
        {
            if (element.TryGetProperty(propertyName, out var propertyValue))
            {
                switch (propertyValue.Type)
                {
                    case JsonValueType.True:
                        return true;
                    case JsonValueType.False:
                        return false;
                    default:
                        throw new InvalidOperationException(
                            $"Property '{new string(propertyName)}' is not boolean."
                        );
                }
            }
            else
            {
                return defaultVal;
            }
        }

        public static int GetInt32Prop(this JsonElement element, ReadOnlySpan<char> propertyName, int defaultVal)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue))
            {
                return defaultVal;
            }

            if (!propertyValue.TryGetInt32(out var intValue))
            {
                throw new InvalidOperationException(
                    $"Property '{new string(propertyName)}' is not an integer."
                );
            }

            return intValue;
        }
    }
}