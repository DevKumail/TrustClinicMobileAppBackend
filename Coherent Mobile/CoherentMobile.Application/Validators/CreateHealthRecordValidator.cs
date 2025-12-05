using CoherentMobile.Application.DTOs;
using FluentValidation;

namespace CoherentMobile.Application.Validators;

/// <summary>
/// Fluent validation for health record creation
/// </summary>
public class CreateHealthRecordValidator : AbstractValidator<CreateHealthRecordDto>
{
    public CreateHealthRecordValidator()
    {
        RuleFor(x => x.RecordType)
            .NotEmpty().WithMessage("Record type is required")
            .MaximumLength(50).WithMessage("Record type must not exceed 50 characters");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(100).WithMessage("Value must not exceed 100 characters");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required")
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters");

        RuleFor(x => x.RecordedAt)
            .NotEmpty().WithMessage("Recorded date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Recorded date cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
