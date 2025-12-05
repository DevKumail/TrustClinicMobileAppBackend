using CoherentMobile.Application.DTOs;
using FluentValidation;

namespace CoherentMobile.Application.Validators;

/// <summary>
/// Fluent validation for user registration
/// </summary>
public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain uppercase, lowercase, number and special character");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required")
            .Must(g => new[] { "Male", "Female", "Other" }.Contains(g))
            .WithMessage("Gender must be Male, Female, or Other");

        RuleFor(x => x.DateOfBirth)
            .Must(BeAValidAge).WithMessage("User must be at least 13 years old")
            .When(x => x.DateOfBirth.HasValue);
    }

    private bool BeAValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;
        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;
        return age >= 13;
    }
}
