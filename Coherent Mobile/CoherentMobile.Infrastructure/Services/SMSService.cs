using CoherentMobile.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Infrastructure.Services;

/// <summary>
/// SMS service implementation (stub - integrate with actual SMS provider)
/// </summary>
public class SMSService : ISMSService
{
    private readonly ILogger<SMSService> _logger;

    public SMSService(ILogger<SMSService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendOTPAsync(string mobileNumber, string otpCode)
    {
        try
        {
            // TODO: Integrate with actual SMS provider (Twilio, AWS SNS, etc.)
            _logger.LogInformation("SMS OTP sent to {Mobile}: {OTP}", mobileNumber, otpCode);
            
            // Simulate sending
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS OTP to {Mobile}", mobileNumber);
            return false;
        }
    }

    public async Task<bool> SendMessageAsync(string mobileNumber, string message)
    {
        try
        {
            // TODO: Integrate with actual SMS provider
            _logger.LogInformation("SMS message sent to {Mobile}: {Message}", mobileNumber, message);
            
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {Mobile}", mobileNumber);
            return false;
        }
    }
}
