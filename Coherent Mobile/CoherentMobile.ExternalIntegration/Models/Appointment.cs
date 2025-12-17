using System;
using System.Text.Json.Serialization;

namespace CoherentMobile.ExternalIntegration.Models
{
    public class Appointment
    {
        [JsonPropertyName("appId")]
        public int AppId { get; set; }

        [JsonPropertyName("mrNo")]
        public string? MrNo { get; set; }

        [JsonPropertyName("doctorId")]
        public int DoctorId { get; set; }

        [JsonPropertyName("doctorName")]
        public string? DoctorName { get; set; }

        [JsonPropertyName("doctorLicenseNo")]
        public string? DoctorLicenseNo { get; set; }

        [JsonPropertyName("speciality")]
        public string? Speciality { get; set; }

        [JsonPropertyName("siteId")]
        public int SiteId { get; set; }

        [JsonPropertyName("siteName")]
        public string? SiteName { get; set; }

        [JsonPropertyName("appointmentDate")]
        public DateTime? AppointmentDate { get; set; }

        [JsonPropertyName("appointmentDateTime")]
        public DateTime? AppointmentDateTime { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime? CreatedDate { get; set; }

        [JsonPropertyName("createdBy")]
        public string? CreatedBy { get; set; }
    }
}
