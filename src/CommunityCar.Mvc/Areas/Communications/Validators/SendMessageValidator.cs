using CommunityCar.Web.Areas.Communications.ViewModels;
using FluentValidation;

namespace CommunityCar.Web.Areas.Communications.Validators;

public class SendMessageValidator : AbstractValidator<SendMessageViewModel>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty()
            .WithMessage("Receiver is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Message content is required")
            .MaximumLength(2000)
            .WithMessage("Message cannot exceed 2000 characters")
            .MinimumLength(1)
            .WithMessage("Message must contain at least 1 character");
    }
}
