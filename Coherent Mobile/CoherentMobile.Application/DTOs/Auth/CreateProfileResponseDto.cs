namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Response after profile creation
/// </summary>
public class CreateProfileResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PatientId { get; set; }
}
