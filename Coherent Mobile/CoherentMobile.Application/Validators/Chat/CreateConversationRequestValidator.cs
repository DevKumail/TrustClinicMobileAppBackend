using CoherentMobile.Application.DTOs.Chat;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Chat
{
    public class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequestDto>
    {
        public CreateConversationRequestValidator()
        {
            RuleFor(x => x.OtherUserId)
                .GreaterThan(0).WithMessage("User ID must be greater than 0");

            RuleFor(x => x.OtherUserType)
                .NotEmpty().WithMessage("User type is required")
                .Must(BeValidUserType).WithMessage("Invalid user type. Allowed: Patient, Doctor, Staff");

            RuleFor(x => x.InitialMessage)
                .MaximumLength(5000).WithMessage("Initial message cannot exceed 5000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.InitialMessage));
        }

        private bool BeValidUserType(string userType)
        {
            var validTypes = new[] { "Patient", "Doctor", "Staff" };
            return validTypes.Contains(userType);
        }
    }
}
