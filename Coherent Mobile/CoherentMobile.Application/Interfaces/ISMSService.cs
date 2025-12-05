namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// SMS sending service interface
/// </summary>
public interface ISMSService
{
    Task<bool> SendOTPAsync(string mobileNumber, string otpCode);
    Task<bool> SendMessageAsync(string mobileNumber, string message);
}
