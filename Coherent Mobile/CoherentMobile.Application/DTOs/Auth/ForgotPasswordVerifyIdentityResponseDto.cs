namespace CoherentMobile.Application.DTOs.Auth
{
    /// <summary>
    /// Response DTO for identity verification in forgot password flow
    /// </summary>
    public class ForgotPasswordVerifyIdentityResponseDto
    {
        /// <summary>
        /// Whether identity verification was successful
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Verification token to be used in the next step (password reset)
        /// This token proves that the user has been verified
        /// </summary>
        public string? VerificationToken { get; set; }

        /// <summary>
        /// Message to display to the user
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
