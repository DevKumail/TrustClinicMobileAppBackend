using CoherentMobile.Application.DTOs.Auth;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Auth;

public class VerifyOTPRequestValidator : AbstractValidator<VerifyOTPRequestDto>
{
    public VerifyOTPRequestValidator()
    {
        RuleFor(x => x.MRNO)
            .NotEmpty().WithMessage("MRNO is required");

        RuleFor(x => x.OTPCode)
            .NotEmpty().WithMessage("OTP code is required")
            .Length(6).WithMessage("OTP code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits");
    }
}
