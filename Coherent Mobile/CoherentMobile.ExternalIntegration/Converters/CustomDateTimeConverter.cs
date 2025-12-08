using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoherentMobile.ExternalIntegration.Converters
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _format;

        public CustomDateTimeConverter() : this("yyyy-MM-dd HH:mm:ss") { }

        public CustomDateTimeConverter(string format)
        {
            _format = format;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
                return default;

            if (DateTime.TryParseExact(dateString, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
        }
    }
}
