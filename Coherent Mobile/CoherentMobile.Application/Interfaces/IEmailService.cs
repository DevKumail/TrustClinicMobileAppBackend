namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Email sending service interface
/// </summary>
public interface IEmailService
{
    Task<bool> SendOTPAsync(string email, string otpCode, string recipientName);
    Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string recipientName);
    Task<bool> SendWelcomeEmailAsync(string email, string recipientName);
}
