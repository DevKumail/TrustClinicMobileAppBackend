using CoherentMobile.Application.DTOs.Auth;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Auth;

public class ForgotPasswordVerifyRequestValidator : AbstractValidator<ForgotPasswordVerifyRequestDto>
{
    public ForgotPasswordVerifyRequestValidator()
    {
        RuleFor(x => x.MRNO)
            .NotEmpty().WithMessage("MRNO is required");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.EmiratesId) || !string.IsNullOrEmpty(x.PassportNumber))
            .WithMessage("Either Emirates ID or Passport Number is required");

        When(x => !string.IsNullOrEmpty(x.EmiratesId), () =>
        {
            RuleFor(x => x.EmiratesId)
                .Matches(@"^\d{3}-\d{4}-\d{7}-\d{1}$")
                .WithMessage("Emirates ID must be in format: XXX-YYYY-NNNNNNN-C (e.g., 784-1990-1234567-1)");
        });

        When(x => !string.IsNullOrEmpty(x.PassportNumber), () =>
        {
            RuleFor(x => x.PassportNumber)
                .MinimumLength(6).WithMessage("Passport number must be at least 6 characters")
                .MaximumLength(20).WithMessage("Passport number must not exceed 20 characters");
        });

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required")
            .Matches(@"^\+971(50|52|54|55|56|58)\d{7}$")
            .WithMessage("Mobile number must be in UAE format: +971XXXXXXXXX");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past");

        RuleFor(x => x.DeliveryChannel)
            .NotEmpty().WithMessage("Delivery channel is required")
            .Must(x => x == "SMS" || x == "Email")
            .WithMessage("Delivery channel must be either 'SMS' or 'Email'");
    }
}
