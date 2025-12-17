using System;
using System.Text.Json.Serialization;

namespace CoherentMobile.ExternalIntegration.Models
{
    public sealed class VitalSign
    {
        [JsonPropertyName("mrno")]
        public string? Mrno { get; set; }

        [JsonPropertyName("weight")]
        public decimal? Weight { get; set; }

        [JsonPropertyName("height")]
        public decimal? Height { get; set; }

        [JsonPropertyName("bmi")]
        public decimal? Bmi { get; set; }

        [JsonPropertyName("temperature")]
        public decimal? Temperature { get; set; }

        [JsonPropertyName("bpSystolic")]
        public int? BpSystolic { get; set; }

        [JsonPropertyName("bpDiastolic")]
        public int? BpDiastolic { get; set; }

        [JsonPropertyName("bloodPressure")]
        public string? BloodPressure { get; set; }

        [JsonPropertyName("heartRate")]
        public int? HeartRate { get; set; }

        [JsonPropertyName("pulseRate")]
        public int? PulseRate { get; set; }

        [JsonPropertyName("respirationRate")]
        public int? RespirationRate { get; set; }

        [JsonPropertyName("spO2")]
        public int? SpO2 { get; set; }

        [JsonPropertyName("recordedDate")]
        public DateTime? RecordedDate { get; set; }
    }
}
