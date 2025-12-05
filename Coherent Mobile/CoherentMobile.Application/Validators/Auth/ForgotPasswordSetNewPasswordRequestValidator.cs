using CoherentMobile.Application.DTOs.Auth;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Auth
{
    public class ForgotPasswordSetNewPasswordRequestValidator : AbstractValidator<ForgotPasswordSetNewPasswordRequestDto>
    {
        public ForgotPasswordSetNewPasswordRequestValidator()
        {
            RuleFor(x => x.VerificationToken)
                .NotEmpty().WithMessage("Verification token is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z)")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter (a-z)")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number (0-9)")
                .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*)");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
