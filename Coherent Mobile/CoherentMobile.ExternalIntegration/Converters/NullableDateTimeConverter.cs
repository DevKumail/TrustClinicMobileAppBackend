using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoherentMobile.ExternalIntegration.Converters
{
    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly FlexibleDateTimeConverter _innerConverter = new();

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            return _innerConverter.Read(ref reader, typeof(DateTime), options);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                _innerConverter.Write(writer, value.Value, options);
            else
                writer.WriteNullValue();
        }
    }
}
