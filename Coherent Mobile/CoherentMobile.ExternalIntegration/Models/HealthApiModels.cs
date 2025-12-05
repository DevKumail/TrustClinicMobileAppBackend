namespace CoherentMobile.ExternalIntegration.Models;

/// <summary>
/// Response model from external health data API
/// </summary>
public class ExternalHealthDataResponse
{
    public string Status { get; set; } = string.Empty;
    public HealthData? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Health data from external API
/// </summary>
public class HealthData
{
    public string DataType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Request model for external notification API
/// </summary>
public class NotificationRequest
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
}

/// <summary>
/// Response from notification API
/// </summary>
public class NotificationResponse
{
    public bool Success { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
