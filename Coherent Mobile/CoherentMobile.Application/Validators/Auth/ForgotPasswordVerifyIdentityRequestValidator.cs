using CoherentMobile.Application.DTOs.Auth;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Auth
{
    public class ForgotPasswordVerifyIdentityRequestValidator : AbstractValidator<ForgotPasswordVerifyIdentityRequestDto>
    {
        public ForgotPasswordVerifyIdentityRequestValidator()
        {
            RuleFor(x => x.MRNO)
                .NotEmpty().WithMessage("MRN is required")
                .MaximumLength(50).WithMessage("MRN cannot exceed 50 characters");

            // Either Emirates ID or Passport Number must be provided
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.EmiratesId) || !string.IsNullOrWhiteSpace(x.PassportNumber))
                .WithMessage("Either Emirates ID or Passport Number is required");

            RuleFor(x => x.EmiratesId)
                .Matches(@"^\d{3}-\d{4}-\d{7}-\d{1}$")
                .When(x => !string.IsNullOrWhiteSpace(x.EmiratesId))
                .WithMessage("Emirates ID must be in format: XXX-YYYY-NNNNNNN-C (e.g., 784-1990-1234567-1)");

            RuleFor(x => x.PassportNumber)
                .MinimumLength(6).WithMessage("Passport number must be at least 6 characters")
                .MaximumLength(20).WithMessage("Passport number cannot exceed 20 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.PassportNumber));

            // Email is required ONLY when Passport Number is provided
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required when using Passport Number")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.PassportNumber));

            // Mobile Number is required ONLY when Emirates ID is provided
            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage("Mobile number is required when using Emirates ID")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid mobile number format")
                .When(x => !string.IsNullOrWhiteSpace(x.EmiratesId));

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Date of birth must be in format: yyyy-MM-dd")
                .Must(BeAValidDate).WithMessage("Invalid date of birth");
        }

        private bool BeAValidDate(string dateString)
        {
            return DateTime.TryParseExact(dateString, "yyyy-MM-dd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out _);
        }
    }
}
