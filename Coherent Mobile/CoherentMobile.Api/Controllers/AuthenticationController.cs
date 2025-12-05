using CoherentMobile.Application.DTOs.Auth;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

/// <summary>
/// Authentication controller for signup, login, and password management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authService,
        ILogger<AuthenticationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Step 2: Verify user information and send OTP
    /// Called after QR scan (QR data comes from web portal)
    /// QR data can be passed as query parameters or will use dummy data for testing
    /// </summary>
    [HttpPost("verify-information")]
    [ProducesResponseType(typeof(VerifyInformationResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyInformation(
        [FromBody] VerifyInformationRequestDto request,
        [FromQuery] string? qrMRNO = null,
        [FromQuery] string? qrFullName = null,
        [FromQuery] string? qrDateOfBirth = null,
        [FromQuery] string? qrEmiratesIdType = null)
    {
        try
        {
            // Build QR data from query parameters or use dummy data
            QRScanResponseDto qrData;
            
            if (!string.IsNullOrEmpty(qrMRNO) && !string.IsNullOrEmpty(qrFullName))
            {
                // QR data provided via query parameters
                qrData = new QRScanResponseDto
                {
                    MRNO = qrMRNO,
                    FullName = qrFullName,
                    DateOfBirth = !string.IsNullOrEmpty(qrDateOfBirth) 
                        ? DateTime.Parse(qrDateOfBirth) 
                        : DateTime.Parse("1990-01-01"),
                    EmiratesIdType = qrEmiratesIdType ?? "Emirates"
                };
            }
            else
            {
                // For testing purposes, create dummy QR data
                qrData = new QRScanResponseDto
                {
                    MRNO = request.MRNO,
                    FullName = "Test User", // In production, this comes from QR
                    DateOfBirth = DateTime.Parse("1990-01-01"),
                    EmiratesIdType = "Emirates" // Will be auto-detected by service
                };
            }

            var result = await _authService.VerifyInformationAsync(request, qrData);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VerifyInformation");
            return StatusCode(500, new { Success = false, Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Step 3: Verify OTP code
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(VerifyOTPResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequestDto request)
    {
        try
        {
            var result = await _authService.VerifyOTPAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VerifyOTP");
            return StatusCode(500, new { Success = false, Message = "An error occurred while verifying OTP" });
        }
    }

    /// <summary>
    /// Step 4: Create profile and set password
    /// </summary>
    [HttpPost("create-profile")]
    [ProducesResponseType(typeof(CreateProfileResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequestDto request)
    {
        try
        {
            var result = await _authService.CreateProfileAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(CreateProfile), new { id = result.PatientId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateProfile");
            return StatusCode(500, new { Success = false, Message = "An error occurred while creating profile" });
        }
    }

    /// <summary>
    /// Login with Emirates ID/Passport and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Login");
            return StatusCode(500, new { Success = false, Message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Forgot Password Step 1: Verify user identity
    /// Validates MRN, Emirates ID/Passport, Email, Mobile Number, and Date of Birth
    /// Returns a verification token if successful
    /// </summary>
    [HttpPost("forgot-password/verify-identity")]
    [ProducesResponseType(typeof(ForgotPasswordVerifyIdentityResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ForgotPasswordVerifyIdentity([FromBody] ForgotPasswordVerifyIdentityRequestDto request)
    {
        try
        {
            var result = await _authService.ForgotPasswordVerifyIdentityAsync(request);
            
            if (!result.IsVerified)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ForgotPasswordVerifyIdentity");
            return StatusCode(500, new { IsVerified = false, Message = "An error occurred while verifying your identity" });
        }
    }

    /// <summary>
    /// Forgot Password Step 2: Set new password
    /// Uses verification token from Step 1 to authenticate the request
    /// Password must meet ADHICS security standards
    /// </summary>
    [HttpPost("forgot-password/set-new-password")]
    [ProducesResponseType(typeof(ForgotPasswordResetResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ForgotPasswordSetNewPassword([FromBody] ForgotPasswordSetNewPasswordRequestDto request)
    {
        try
        {
            var result = await _authService.ForgotPasswordSetNewPasswordAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ForgotPasswordSetNewPassword");
            return StatusCode(500, new { Success = false, Message = "An error occurred while setting your new password" });
        }
    }
}
