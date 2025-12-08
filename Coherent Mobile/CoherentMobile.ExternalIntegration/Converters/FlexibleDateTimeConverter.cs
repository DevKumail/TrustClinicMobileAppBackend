// CoherentMobile.ExternalIntegration/Converters/FlexibleDateTimeConverter.cs
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoherentMobile.ExternalIntegration.Converters
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] Formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss",
            "MM/dd/yyyy HH:mm:ss",
            "MM-dd-yyyy HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss",
            "dd/MM/yyyy HH:mm:ss",
            "dd-MM-yyyy HH:mm:ss"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (string.IsNullOrEmpty(dateString))
                    return default;

                if (DateTime.TryParseExact(dateString, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;

                if (DateTime.TryParse(dateString, out result))
                    return result;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                // Handle Unix timestamp (seconds since epoch)
                if (reader.TryGetInt64(out var unixTime))
                    return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            }

            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}