using CoherentMobile.Application.DTOs.Auth;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Auth;

public class VerifyInformationRequestValidator : AbstractValidator<VerifyInformationRequestDto>
{
    public VerifyInformationRequestValidator()
    {
        RuleFor(x => x.MRNO)
            .NotEmpty().WithMessage("MRNO is required");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required")
            .Matches(@"^\+971\d{9}$").WithMessage("Mobile number must be in format +971XXXXXXXXX");

        RuleFor(x => x.DeliveryChannel)
            .Must(x => x == "SMS" || x == "Email").WithMessage("Delivery channel must be SMS or Email");

        // Emirates ID validation
        When(x => !string.IsNullOrEmpty(x.EmiratesId), () =>
        {
            RuleFor(x => x.EmiratesId)
                .Matches(@"^\d{3}-\d{4}-\d{7}-\d{1}$")
                .WithMessage("Emirates ID must be in format XXX-XXXX-XXXXXXX-X");
            
            RuleFor(x => x.PassportNumber)
                .Empty().WithMessage("Passport number should not be provided with Emirates ID");
        });

        // Passport validation
        When(x => !string.IsNullOrEmpty(x.PassportNumber), () =>
        {
            RuleFor(x => x.PassportNumber)
                .Length(6, 12).WithMessage("Passport number must be 6-12 characters");
            
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required for passport holders")
                .EmailAddress().WithMessage("Invalid email format");
            
            RuleFor(x => x.EmiratesId)
                .Empty().WithMessage("Emirates ID should not be provided with Passport");
        });

        // Either Emirates ID or Passport must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.EmiratesId) || !string.IsNullOrEmpty(x.PassportNumber))
            .WithMessage("Either Emirates ID or Passport Number must be provided");
    }
}
