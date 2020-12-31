using System;
using System.Text.Json;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Ui.Styles
{
    public static class JsonElementExtensions
    {
        public static bool IsBool(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False;
        }

        public static bool GetBoolProp(this JsonElement element, ReadOnlySpan<char> propertyName, bool defaultVal)
        {
            if (element.TryGetProperty(propertyName, out var propertyValue))
            {
                switch (propertyValue.ValueKind)
                {
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
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

        public static int GetInt32Prop(this JsonElement element, ReadOnlySpan<char> propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue))
            {
                throw new InvalidOperationException(
                    $"Required property '{new string(propertyName)} is missing."
                );
            }

            if (!propertyValue.TryGetInt32(out var intValue))
            {
                throw new InvalidOperationException(
                    $"Property '{new string(propertyName)}' is not an integer."
                );
            }

            return intValue;
        }

        public static string GetStringProp(this JsonElement element, ReadOnlySpan<char> propertyName, string defaultValue)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue))
            {
                return defaultValue;
            }

            if (propertyValue.ValueKind !=  JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    $"Property '{new string(propertyName)}' is not a string."
                );
            }

            return propertyValue.GetString();
        }

        public static string GetStringProp(this JsonElement element, ReadOnlySpan<char> propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue))
            {
                throw new InvalidOperationException(
                    $"Required property '{new string(propertyName)} is missing."
                );
            }

            if (propertyValue.ValueKind !=  JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    $"Property '{new string(propertyName)}' is not a string."
                );
            }

            return propertyValue.GetString();
        }

    }
}