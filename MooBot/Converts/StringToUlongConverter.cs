using System.Text.Json;
using System.Text.Json.Serialization;

namespace MooBot.Converts
{
    public class StringToULongConverter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (ulong.TryParse(reader.GetString(), out ulong result))
                {
                    return result;
                }
                throw new JsonException("Invalid ulong value");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetUInt64();
            }
            throw new JsonException("Unexpected token type");
        }

        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
