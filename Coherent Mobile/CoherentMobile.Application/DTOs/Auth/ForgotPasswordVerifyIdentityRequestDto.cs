namespace CoherentMobile.Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO for verifying user identity in forgot password flow (Step 1)
    /// </summary>
    public class ForgotPasswordVerifyIdentityRequestDto
    {
        /// <summary>
        /// Medical Record Number (Required)
        /// </summary>
        public string MRNO { get; set; } = string.Empty;

        /// <summary>
        /// Emirates ID Number (Required if PassportNumber is empty)
        /// When Emirates ID is provided, MobileNumber is required
        /// </summary>
        public string? EmiratesId { get; set; }

        /// <summary>
        /// Passport Number (Required if EmiratesId is empty)
        /// When Passport Number is provided, Email is required
        /// </summary>
        public string? PassportNumber { get; set; }

        /// <summary>
        /// Email Address (Required ONLY when PassportNumber is provided)
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Mobile Number (Required ONLY when EmiratesId is provided)
        /// </summary>
        public string? MobileNumber { get; set; }

        /// <summary>
        /// Date of Birth (Required)
        /// Format: yyyy-MM-dd
        /// </summary>
        public string DateOfBirth { get; set; } = string.Empty;
    }
}
