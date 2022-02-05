using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenTemple.Core.Config;

public class JsonTimeSpanConverter : JsonConverterFactory
{
    private readonly Converter _converter = new();

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(TimeSpan);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(TimeSpan))
        {
            return _converter;
        }

        throw new ArgumentOutOfRangeException();
    }

    private class Converter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.FromMilliseconds(reader.GetDouble());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.TotalMilliseconds);
        }
    }
}