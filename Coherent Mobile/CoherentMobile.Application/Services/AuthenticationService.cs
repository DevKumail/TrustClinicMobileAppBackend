using CoherentMobile.Application.DTOs.Auth;
using CoherentMobile.Application.Exceptions;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Application.Services.Helpers;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.ExternalIntegration.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IPatientRepository _patientRepo;
    private readonly IOTPVerificationRepository _otpRepo;
    private readonly IAuthAuditLogRepository _auditRepo;
    private readonly IPasswordResetTokenRepository _resetTokenRepo;
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    private readonly JwtTokenGenerator _jwtGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPatientHealthApiClient _patientHealthApiClient;

    // Temporary storage for signup data (in production, use Redis or similar)
    private static readonly Dictionary<string, SignupCacheData> _signupDataCache = new();

    private class SignupCacheData
    {
        public QRScanResponseDto QRData { get; set; } = null!;
        public VerifyInformationRequestDto VerifyRequest { get; set; } = null!;
        public DateTime CachedAt { get; set; }
    }

    public AuthenticationService(
        IPatientRepository patientRepo,
        IOTPVerificationRepository otpRepo,
        IAuthAuditLogRepository auditRepo,
        IPasswordResetTokenRepository resetTokenRepo,
        ISMSService smsService,
        IEmailService emailService,
        JwtTokenGenerator jwtGenerator,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger,
        IPatientHealthApiClient patientHealthApiClient)
    {
        _patientRepo = patientRepo;
        _otpRepo = otpRepo;
        _auditRepo = auditRepo;
        _resetTokenRepo = resetTokenRepo;
        _smsService = smsService;
        _emailService = emailService;
        _jwtGenerator = jwtGenerator;
        _configuration = configuration;
        _logger = logger;
        _patientHealthApiClient = patientHealthApiClient;
    }

    public async Task<VerifyInformationResponseDto> VerifyInformationAsync(
        VerifyInformationRequestDto request, 
        QRScanResponseDto qrData)
    {
        try
        {
            _logger.LogInformation("Starting information verification for MRNO: {MRNO}", request.MRNO);

            // Validate MRNO matches QR data
            if (request.MRNO != qrData.MRNO)
            {
                await LogAuditAsync(null, request.MRNO, "VerifyInformation", "Failed", "MRNO mismatch");
                return new VerifyInformationResponseDto
                {
                    Success = false,
                    Message = "Invalid MRNO provided"
                };
            }

            // Check if user already exists
            var existingPatient = await _patientRepo.GetByMRNOAsync(request.MRNO);
            if (existingPatient != null)
            {
                await LogAuditAsync(existingPatient.Id, request.MRNO, "VerifyInformation", "Failed", "Patient already registered");
                return new VerifyInformationResponseDto
                {
                    Success = false,
                    Message = "Patient already registered. Please login."
                };
            }

            // Validate Emirates ID or Passport
            if (!string.IsNullOrEmpty(request.EmiratesId))
            {
                if (await _patientRepo.EmiratesIdExistsAsync(request.EmiratesId))
                {
                    return new VerifyInformationResponseDto
                    {
                        Success = false,
                        Message = "Emirates ID already registered"
                    };
                }
            }
            else if (!string.IsNullOrEmpty(request.PassportNumber))
            {
                if (await _patientRepo.PassportNumberExistsAsync(request.PassportNumber))
                {
                    return new VerifyInformationResponseDto
                    {
                        Success = false,
                        Message = "Passport number already registered"
                    };
                }
            }

            // Generate OTP
            var otpCode = OTPGenerator.Generate(6);
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            // Save OTP to database
            var otpEntity = new OTPVerification
            {
                OTPCode = otpCode,
                OTPType = "Signup",
                MRNO = request.MRNO,
                DeliveryChannel = request.DeliveryChannel,
                RecipientContact = request.DeliveryChannel == "SMS" ? request.MobileNumber : request.Email ?? string.Empty,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            await _otpRepo.AddAsync(otpEntity);

            // Send OTP
            bool sent = false;
            if (request.DeliveryChannel == "SMS")
            {
                sent = await _smsService.SendOTPAsync(request.MobileNumber, otpCode);
            }
            else if (request.DeliveryChannel == "Email" && !string.IsNullOrEmpty(request.Email))
            {
                sent = await _emailService.SendOTPAsync(request.Email, otpCode, qrData.FullName);
            }

            if (!sent)
            {
                _logger.LogWarning("Failed to send OTP to {Contact}", otpEntity.RecipientContact);
                return new VerifyInformationResponseDto
                {
                    Success = false,
                    Message = "Failed to send OTP. Please try again."
                };
            }

            // Store signup data temporarily for profile creation
            _signupDataCache[request.MRNO] = new SignupCacheData
            {
                QRData = qrData,
                VerifyRequest = request,
                CachedAt = DateTime.UtcNow
            };

            // Log audit
            await LogAuditAsync(null, request.MRNO, "OTPSent", "Success", $"OTP sent via {request.DeliveryChannel}");

            _logger.LogInformation("OTP sent successfully to {Contact}", MaskContact(otpEntity.RecipientContact));

            return new VerifyInformationResponseDto
            {
                Success = true,
                Message = "OTP sent successfully",
                DeliveryChannel = request.DeliveryChannel,
                ExpiresIn = 300, // 5 minutes
                RecipientContact = MaskContact(otpEntity.RecipientContact)
            };
        }
        catch (DatabaseException)
        {
            // Database exception from repository - just rethrow
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in VerifyInformationAsync for MRNO: {MRNO}", request.MRNO);
            await LogAuditAsync(null, request.MRNO, "VerifyInformation", "Error", "Unexpected error");
            throw new AuthenticationException("An unexpected error occurred while verifying information", ex, "VERIFY_INFO_ERROR");
        }
    }

    public async Task<VerifyOTPResponseDto> VerifyOTPAsync(VerifyOTPRequestDto request)
    {
        try
        {
            _logger.LogInformation("Verifying OTP for MRNO: {MRNO}", request.MRNO);

            // Verify OTP using stored procedure
            var isValid = await _otpRepo.VerifyOTPAsync(request.OTPCode, request.MRNO, null);

            if (!isValid)
            {
                await LogAuditAsync(null, request.MRNO, "OTPVerified", "Failed", "Invalid OTP");
                return new VerifyOTPResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired OTP"
                };
            }

            // Log audit
            await LogAuditAsync(null, request.MRNO, "OTPVerified", "Success", "OTP verified successfully");

            _logger.LogInformation("OTP verified successfully for MRNO: {MRNO}", request.MRNO);

            return new VerifyOTPResponseDto
            {
                Success = true,
                Message = "OTP verified successfully",
                TempToken = GenerateTempToken(request.MRNO) // For additional security
            };
        }
        catch (DatabaseException)
        {
            // Database exception from repository - just rethrow
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in VerifyOTPAsync for MRNO: {MRNO}", request.MRNO);
            await LogAuditAsync(null, request.MRNO, "OTPVerified", "Error", "Unexpected error");
            throw new OTPException("An unexpected error occurred while verifying OTP", ex, "VERIFY_OTP_ERROR");
        }
    }

    public async Task<CreateProfileResponseDto> CreateProfileAsync(CreateProfileRequestDto request)
    {
        try
        {
            _logger.LogInformation("Creating profile for MRNO: {MRNO}", request.MRNO);

            // Get signup data from cache
            if (!_signupDataCache.TryGetValue(request.MRNO, out var signupData))
            {
                return new CreateProfileResponseDto
                {
                    Success = false,
                    Message = "Session expired. Please start signup process again."
                };
            }

            var qrData = signupData.QRData;
            var verifyRequest = signupData.VerifyRequest;

            // Check if OTP was verified
            var latestOTP = await _otpRepo.GetLatestByMRNOAsync(request.MRNO, "Signup");
            if (latestOTP == null || !latestOTP.IsVerified)
            {
                return new CreateProfileResponseDto
                {
                    Success = false,
                    Message = "OTP not verified. Please verify OTP first."
                };
            }

            // Hash password
            var (hash, salt) = PasswordHasher.HashPassword(request.Password);

            // Create patient entity with all required fields
            var patient = new Patient
            {
                MRNO = qrData.MRNO,
                FullName = qrData.FullName,
                DateOfBirth = qrData.DateOfBirth,
                EmiratesIdType = qrData.EmiratesIdType,
                EmiratesId = verifyRequest.EmiratesId,
                PassportNumber = verifyRequest.PassportNumber,
                MobileNumber = verifyRequest.MobileNumber,
                Email = verifyRequest.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsProfileComplete = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Add patient to database
            var patientId = await _patientRepo.AddAsync(patient);
            patient.Id = patientId;

            // Remove from cache
            _signupDataCache.Remove(request.MRNO);

            // Log audit
            await LogAuditAsync(patient.Id, request.MRNO, "SignupComplete", "Success", "Profile created successfully");

            // Mark patient as mobile user in third-party system
            try
            {
                var mobileUserResponse = await _patientHealthApiClient.MarkPatientAsMobileUserAsync(patient.MRNO);
                _logger.LogInformation("Patient marked as mobile user: MRNO={MRNO}, IsMobileUser={IsMobileUser}", 
                    patient.MRNO, mobileUserResponse.IsMobileUser);
            }
            catch (Exception ex)
            {
                // Log error but don't fail registration - this is a non-critical operation
                _logger.LogWarning(ex, "Failed to mark patient as mobile user in third-party system for MRNO: {MRNO}", patient.MRNO);
            }

            // Send welcome email
            if (!string.IsNullOrEmpty(patient.Email))
            {
                await _emailService.SendWelcomeEmailAsync(patient.Email, patient.FullName);
            }

            _logger.LogInformation("Profile created successfully for MRNO: {MRNO}, PatientId: {PatientId}", request.MRNO, patientId);

            return new CreateProfileResponseDto
            {
                Success = true,
                Message = "Profile created successfully",
                PatientId = patientId
            };
        }
        catch (DatabaseException)
        {
            // Database exception from repository - just rethrow
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateProfileAsync for MRNO: {MRNO}", request.MRNO);
            await LogAuditAsync(null, request.MRNO, "CreateProfile", "Error", "Unexpected error");
            throw new AuthenticationException("An unexpected error occurred while creating profile", ex, "CREATE_PROFILE_ERROR");
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", MaskContact(request.Email));

            // Get patient by Email
            var patient = await _patientRepo.GetByEmailAsync(request.Email);

            if (patient == null)
            {
                await LogAuditAsync(null, null, "Login", "Failed", $"User not found: {MaskContact(request.Email)}");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            // Check if account is locked
            if (patient.IsLocked)
            {
                await LogAuditAsync(patient.Id, patient.MRNO, "Login", "Failed", "Account locked");
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Account is locked due to multiple failed login attempts. Please contact support."
                };
            }

            // Verify password
            if (!PasswordHasher.VerifyPassword(request.Password, patient.PasswordHash, patient.PasswordSalt))
            {
                await _patientRepo.UpdateFailedLoginAttemptsAsync(patient.Id);
                await LogAuditAsync(patient.Id, patient.MRNO, "Login", "Failed", "Invalid password");
                
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            // Reset failed login attempts
            await _patientRepo.ResetFailedLoginAttemptsAsync(patient.Id);

            // Generate JWT tokens
            var accessToken = _jwtGenerator.GenerateAccessToken(
                patient.Id,
                
                patient.MRNO,
                patient.FullName,
                patient.Email ?? string.Empty
            );
            var refreshToken = _jwtGenerator.GenerateRefreshToken();

            // Log audit
            await LogAuditAsync(patient.Id, patient.MRNO, "Login", "Success", "Login successful");

            _logger.LogInformation("Login successful for PatientId: {PatientId}", patient.Id);

            return new LoginResponseDto
            {
                Success = true,
                Message = "Login successful",
                PatientId = patient.Id,
                MRNO = patient.MRNO,
                FullName = patient.FullName,
                Gender = patient.Gender,
                MobileNumber = patient.MobileNumber,
                Email = patient.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpiryHours"]) * 3600
            };
        }
        catch (DatabaseException)
        {
            // Database exception from repository - just rethrow
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in LoginAsync");
            throw new AuthenticationException("An unexpected error occurred during login", ex, "LOGIN_ERROR");
        }
    }

    /// <summary>
    /// Step 1: Verify user identity for forgot password flow
    /// Validates MRN, Emirates ID/Passport, Email, Mobile, and DOB
    /// Returns verification token if successful
    /// </summary>
    public async Task<ForgotPasswordVerifyIdentityResponseDto> ForgotPasswordVerifyIdentityAsync(ForgotPasswordVerifyIdentityRequestDto request)
    {
        try
        {
            _logger.LogInformation("Forgot password identity verification for MRNO: {MRNO}", request.MRNO);

            // Get patient by MRNO
            var patient = await _patientRepo.GetByMRNOAsync(request.MRNO);
            
            if (patient == null)
            {
                await LogAuditAsync(null, request.MRNO, "ForgotPasswordVerifyIdentity", "Failed", "Patient not found");
                return new ForgotPasswordVerifyIdentityResponseDto
                {
                    IsVerified = false,
                    Message = "Invalid information provided. Please check your details and try again."
                };
            }

            // Verify patient information
            bool isValid = true;
            
            // Check Emirates ID or Passport
            if (!string.IsNullOrWhiteSpace(request.EmiratesId))
            {
                isValid = patient.EmiratesId == request.EmiratesId;
            }
            else if (!string.IsNullOrWhiteSpace(request.PassportNumber))
            {
                isValid = patient.PassportNumber == request.PassportNumber;
            }
            else
            {
                isValid = false;
            }
            
            // Verify other details based on ID type
            if (isValid)
            {
                // Parse DateOfBirth string
                if (DateTime.TryParseExact(request.DateOfBirth, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, 
                    out DateTime dob))
                {
                    // Check DOB
                    isValid = patient.DateOfBirth.Date == dob.Date;
                    
                    // If Emirates ID is provided, verify Mobile Number
                    if (isValid && !string.IsNullOrWhiteSpace(request.EmiratesId))
                    {
                        isValid = patient.MobileNumber == request.MobileNumber;
                    }
                    
                    // If Passport Number is provided, verify Email
                    if (isValid && !string.IsNullOrWhiteSpace(request.PassportNumber))
                    {
                        isValid = patient.Email == request.Email;
                    }
                }
                else
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                await LogAuditAsync(patient.Id, request.MRNO, "ForgotPasswordVerifyIdentity", "Failed", "Information mismatch");
                return new ForgotPasswordVerifyIdentityResponseDto
                {
                    IsVerified = false,
                    Message = "Invalid information provided. Please check your details and try again."
                };
            }

            // Generate verification token (JWT-like token that expires in 10 minutes)
            var verificationToken = GenerateVerificationToken(patient.Id, patient.MRNO);

            // Log successful verification
            await LogAuditAsync(patient.Id, request.MRNO, "ForgotPasswordVerifyIdentity", "Success", "Identity verified successfully");

            _logger.LogInformation("Identity verified successfully for MRNO: {MRNO}", request.MRNO);

            return new ForgotPasswordVerifyIdentityResponseDto
            {
                IsVerified = true,
                VerificationToken = verificationToken,
                Message = "Identity verified successfully. You can now set your new password."
            };
        }
        catch (DatabaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ForgotPasswordVerifyIdentityAsync for MRNO: {MRNO}", request.MRNO);
            await LogAuditAsync(null, request.MRNO, "ForgotPasswordVerifyIdentity", "Error", "Unexpected error");
            throw new AuthenticationException("An unexpected error occurred while verifying identity", ex, "FORGOT_PASSWORD_VERIFY_IDENTITY_ERROR");
        }
    }

    /// <summary>
    /// Step 2: Set new password using verification token from Step 1
    /// </summary>
    public async Task<ForgotPasswordResetResponseDto> ForgotPasswordSetNewPasswordAsync(ForgotPasswordSetNewPasswordRequestDto request)
    {
        try
        {
            _logger.LogInformation("Setting new password for forgot password flow");

            // Decode and validate verification token
            var (isValid, patientId, mrno) = ValidateVerificationToken(request.VerificationToken);
            
            if (!isValid || patientId == null || string.IsNullOrEmpty(mrno))
            {
                await LogAuditAsync(null, null, "ForgotPasswordSetNewPassword", "Failed", "Invalid or expired verification token");
                return new ForgotPasswordResetResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired verification token. Please start the process again."
                };
            }

            // Get patient by ID
            var patient = await _patientRepo.GetByIdAsync(patientId.Value);
            
            if (patient == null)
            {
                await LogAuditAsync(null, mrno, "ForgotPasswordSetNewPassword", "Failed", "Patient not found");
                return new ForgotPasswordResetResponseDto
                {
                    Success = false,
                    Message = "Invalid request. Please start the process again."
                };
            }

            // Hash new password
            var (hash, salt) = PasswordHasher.HashPassword(request.Password);

            // Update patient password
            patient.PasswordHash = hash;
            patient.PasswordSalt = salt;
            patient.UpdatedAt = DateTime.UtcNow;

            await _patientRepo.UpdateAsync(patient);

            // Log audit
            await LogAuditAsync(patient.Id, patient.MRNO, "ForgotPasswordSetNewPassword", "Success", "Password updated successfully");

            _logger.LogInformation("Password updated successfully for PatientId: {PatientId}", patient.Id);

            return new ForgotPasswordResetResponseDto
            {
                Success = true,
                Message = "Password updated successfully. Please login with your new password."
            };
        }
        catch (DatabaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ForgotPasswordSetNewPasswordAsync");
            await LogAuditAsync(null, null, "ForgotPasswordSetNewPassword", "Error", "Unexpected error");
            throw new AuthenticationException("An unexpected error occurred while setting new password", ex, "FORGOT_PASSWORD_SET_NEW_PASSWORD_ERROR");
        }
    }

    // Helper methods
    private async Task LogAuditAsync(int? patientId, string? mrno, string action, string status, string details)
    {
        var log = new AuthAuditLog
        {
            PatientId = patientId,
            MRNO = mrno,
            Action = action,
            Status = status,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _auditRepo.AddAsync(log);
    }

    private string MaskContact(string contact)
    {
        if (string.IsNullOrEmpty(contact)) return string.Empty;

        if (contact.Contains("@"))
        {
            // Email masking
            var parts = contact.Split('@');
            if (parts[0].Length > 2)
            {
                return $"{parts[0].Substring(0, 2)}***@{parts[1]}";
            }
        }
        else if (contact.StartsWith("+"))
        {
            // Phone masking
            return $"{contact.Substring(0, 7)}***{contact.Substring(contact.Length - 2)}";
        }

        return contact;
    }

    private string GenerateTempToken(string mrno)
    {
        // Simple temp token for additional security (in production, use proper token generation)
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{mrno}:{DateTime.UtcNow.Ticks}"));
    }

    private string GenerateVerificationToken(int patientId, string mrno)
    {
        // Generate a secure verification token that expires in 10 minutes
        // Format: Base64(patientId:mrno:expiryTimestamp:signature)
        var expiry = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        var payload = $"{patientId}:{mrno}:{expiry}";
        
        // Add a simple signature (in production, use HMAC or similar)
        var signature = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(payload + "_SECRET_KEY")
            )
        ).Substring(0, 16);
        
        var token = $"{payload}:{signature}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
    }

    private (bool isValid, int? patientId, string? mrno) ValidateVerificationToken(string token)
    {
        try
        {
            // Decode the token
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            
            if (parts.Length != 4)
            {
                return (false, null, null);
            }

            var patientId = int.Parse(parts[0]);
            var mrno = parts[1];
            var expiry = long.Parse(parts[2]);
            var signature = parts[3];

            // Verify signature
            var payload = $"{patientId}:{mrno}:{expiry}";
            var expectedSignature = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(payload + "_SECRET_KEY")
                )
            ).Substring(0, 16);

            if (signature != expectedSignature)
            {
                return (false, null, null);
            }

            // Check expiry
            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(expiry);
            if (DateTimeOffset.UtcNow > expiryTime)
            {
                return (false, null, null);
            }

            return (true, patientId, mrno);
        }
        catch
        {
            return (false, null, null);
        }
    }
}
