namespace CoherentMobile.Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO for setting new password in forgot password flow (Step 2)
    /// </summary>
    public class ForgotPasswordSetNewPasswordRequestDto
    {
        /// <summary>
        /// Verification token from Step 1
        /// </summary>
        public string VerificationToken { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// Must meet ADHICS security standards:
        /// - At least 8 characters
        /// - One uppercase letter (A-Z)
        /// - One lowercase letter (a-z)
        /// - One number (0-9)
        /// - One special character (!@#$%^&*)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirm new password (must match Password)
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
