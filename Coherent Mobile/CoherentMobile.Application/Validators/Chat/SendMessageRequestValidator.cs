using CoherentMobile.Application.DTOs.Chat;
using FluentValidation;

namespace CoherentMobile.Application.Validators.Chat
{
    public class SendMessageRequestValidator : AbstractValidator<SendMessageRequestDto>
    {
        public SendMessageRequestValidator()
        {
            RuleFor(x => x.ConversationId)
                .GreaterThan(0).WithMessage("Conversation ID must be greater than 0");

            RuleFor(x => x.MessageType)
                .NotEmpty().WithMessage("Message type is required")
                .Must(BeValidMessageType).WithMessage("Invalid message type. Allowed: Text, Image, File, Voice, Video");

            // Content required for text messages
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Message content is required")
                .MaximumLength(5000).WithMessage("Message content cannot exceed 5000 characters")
                .When(x => x.MessageType == "Text");

            // File info required for non-text messages
            RuleFor(x => x.FileUrl)
                .NotEmpty().WithMessage("File URL is required")
                .When(x => x.MessageType != "Text");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .When(x => x.MessageType != "Text");

            RuleFor(x => x.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than 0")
                .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("File size cannot exceed 10MB")
                .When(x => x.MessageType != "Text");
        }

        private bool BeValidMessageType(string messageType)
        {
            var validTypes = new[] { "Text", "Image", "File", "Voice", "Video" };
            return validTypes.Contains(messageType);
        }
    }
}
