using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenTemple.Core.Config
{
    public class SeenMovieJsonConverter : JsonConverter<(int, int)>
    {
        public override (int, int) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            reader.Read();

            var movieId = reader.GetInt32();
            var soundtrackId = reader.GetInt32();

            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException();
            }

            reader.Read();

            return (movieId, soundtrackId);
        }

        public override void Write(Utf8JsonWriter writer, (int, int) value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Item1);
            writer.WriteNumberValue(value.Item2);
            writer.WriteEndArray();
        }
    }
}