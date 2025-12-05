using CoherentMobile.Application.DTOs.Auth;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    // Signup Flow
    Task<VerifyInformationResponseDto> VerifyInformationAsync(VerifyInformationRequestDto request, QRScanResponseDto qrData);
    Task<VerifyOTPResponseDto> VerifyOTPAsync(VerifyOTPRequestDto request);
    Task<CreateProfileResponseDto> CreateProfileAsync(CreateProfileRequestDto request);
    
    // Login Flow
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    
    // Forgot Password Flow (2-Step Process)
    // Step 1: Verify user identity using MRN, Emirates ID/Passport, Email, Mobile, DOB
    Task<ForgotPasswordVerifyIdentityResponseDto> ForgotPasswordVerifyIdentityAsync(ForgotPasswordVerifyIdentityRequestDto request);
    
    // Step 2: Set new password using verification token from Step 1
    Task<ForgotPasswordResetResponseDto> ForgotPasswordSetNewPasswordAsync(ForgotPasswordSetNewPasswordRequestDto request);
}
