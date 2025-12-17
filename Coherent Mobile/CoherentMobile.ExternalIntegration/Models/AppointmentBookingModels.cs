using System.Text.Json.Serialization;

namespace CoherentMobile.ExternalIntegration.Models;

public sealed class BookAppointmentRequest
{
    [JsonPropertyName("doctorID")]
    public string DoctorID { get; set; } = string.Empty;

    [JsonPropertyName("facilityID")]
    public string FacilityID { get; set; } = string.Empty;

    [JsonPropertyName("serviceID")]
    public string ServiceID { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("mrNo")]
    public string MrNo { get; set; } = string.Empty;
}

public sealed class BookAppointmentResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("appointmentId")]
    public int AppointmentId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class CancelAppointmentRequest
{
    [JsonPropertyName("appBookingId")]
    public int AppBookingId { get; set; }
}

public sealed class CancelAppointmentResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("appBookingId")]
    public int AppBookingId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class ChangeBookedAppointmentRequest
{
    [JsonPropertyName("appId")]
    public int AppId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("doctorID")]
    public string DoctorID { get; set; } = string.Empty;

    [JsonPropertyName("facilityID")]
    public string FacilityID { get; set; } = string.Empty;

    [JsonPropertyName("serviceID")]
    public string ServiceID { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("mrNo")]
    public string MrNo { get; set; } = string.Empty;
}

public sealed class ChangeBookedAppointmentResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("appointmentId")]
    public int AppointmentId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
